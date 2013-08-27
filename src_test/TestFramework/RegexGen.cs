// based on http://codebox/regexgen

//RegexGen.cs -- Includes all the methods for regular expression string generator
//Author - Cagri Aslan (caslan), David Henry (v-dahenr) 03/2007
//The code is based on the algorithm in randstrgen tool

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AdSage.Concert.Test.Framework
{
    abstract class RENode   //Base class for regex elements
    {
        internal RENode Parent; //the parent node that will call Generate on this node

        //Generates a string matching the regex element or not matching if this is RECompiler.invalidNode
        //returns - A string which generated based on the regular expression of this node
        internal abstract string Generate(Random random);

        //Required for GenerateInvalid.
        //Marks along the RegEx tree to ensure that 'child' is part of the generated string
        //child - The child node that must be part of the generated string
        internal virtual void ReservePath(RENode child)
        {
            if (Parent != null)
            {
                Parent.ReservePath(this);
            }
        }

        //Assert in parsing
        //b - Value that must be true for assert to pass
        //message - Message to throw if the assert fails
        static internal void AssertParse(bool b, string message)
        {
            if (!b)
                throw new ArgumentException("Regex parse error: " + message);
        }
    }

    //Represents a text portion of a regex (i.e. 'abc' and 'cde' in regular expression abc[A-Z]|cde{2,3}
    class RETextNode : RENode
    {
        private StringBuilder mNodeText;

        internal RETextNode(string str)
        {
            if ((RECompiler.IsInvalidSection) && (!String.IsNullOrEmpty(str)))
            {
                RECompiler.InvalidableNodes.Add(this);
            }
            mNodeText = new StringBuilder(str);
        }

        internal override string Generate(Random random)
        {
            if (this == RECompiler.InvalidNode)
            {
                //select a character
                int pos = random.Next(mNodeText.Length);

                //generate any other character using a negative SetNode
                RESetNode others = new RESetNode(false);
                others.AddChars(mNodeText[pos].ToString());

                //replace the character
                char backup = mNodeText[pos];
                mNodeText[pos] = others.Generate(random)[0];
                string result = mNodeText.ToString();

                //if this node is repeated it needs to be cleaned up for the next call
                mNodeText[pos] = backup;

                return result;
            }
            else
            {
                return mNodeText.ToString();
            }
        }
    }

    //Represents repetition token ( i.e. 'cde{2,3}' in abc[A-Z]|cde{2,3} )
    class RERepeatNode : RENode
    {
        private int mMinRepeat;
        private int mMaxRepeat;
        private bool mSameValue; //Repeat same character?
        private RENode mRefNode; //The node to repeat
        internal static int extraRepetitions = 10; //The additional number of times to repeat (approximates infinity)
        private RENode mReservedPath; //The child node that must be chosen.
        //If this is not null then the node must repeat at least once

        internal RERepeatNode(RENode refNode, int minRepeat, int maxRepeat, bool sameValue)
        {
            //if this does not cover zero to infinity, then this node can be invalidated
            if (RECompiler.IsInvalidSection && (minRepeat > 0 || maxRepeat != -1))
            {
                RECompiler.InvalidableNodes.Add(this);
            }
            mMinRepeat = minRepeat;
            mMaxRepeat = maxRepeat;
            mSameValue = sameValue;
            mRefNode = refNode;
            mRefNode.Parent = this;
        }

        internal override void ReservePath(RENode child)
        {
            //this child (mRefNode) must be called when generating the string (cannot repeat zero times)
            mReservedPath = child;
            base.ReservePath(child);
        }
        internal override string Generate(Random random)
        {
            int numRepeat;
            StringBuilder buffer = new StringBuilder();
            if (this == RECompiler.InvalidNode)
            {
                //randomly choose to repeat more or less than the given range
                int repeatMore = random.Next(2);
                if ((mMaxRepeat != -1 && 1 == repeatMore) || mMinRepeat == 0)
                {
                    //repeat more than the given range
                    checked
                    {
                        numRepeat = random.Next(mMaxRepeat + 1, mMaxRepeat + 11);
                    }
                }
                else
                {
                    //repeat less than the given range
                    numRepeat = random.Next(0, mMinRepeat);
                }
            }
            else
            {
                //repeat for some number inside the given range
                checked
                {
                    int maxRepeat = (mMaxRepeat == -1) ? mMinRepeat + extraRepetitions : mMaxRepeat;

                    //don't repeat zero times if the repeated node is on the invalidating path
                    int minRepeat = (mMinRepeat == 0 && mRefNode == mReservedPath) ? 1 : mMinRepeat;

                    numRepeat = (minRepeat < maxRepeat) ? random.Next(minRepeat, maxRepeat + 1) : minRepeat;
                }
            }
            string childStr;

            if (mRefNode is RETextNode) //If the referenced node is text node, only repeat the last character
            {
                childStr = mRefNode.Generate(random);
                buffer.Append(childStr.Substring(0, childStr.Length - 1));
                childStr = childStr[childStr.Length - 1].ToString(); //Get last character
                mSameValue = true;
            }
            else
            {
                childStr = mRefNode.Generate(random);
            }

            for (int i = 0; i < numRepeat; i++)
                buffer.Append(mSameValue ? childStr : mRefNode.Generate(random));

            return buffer.ToString();
        }
    }

    //Represents a group of tokens one of which will be generated 
    //For example abc[A-Z]|cde{2,3} is a reornode with 2 children
    class REOrNode : RENode
    {
        internal List<RENode> Children = new List<RENode>();
        private RENode mReservedPath; //The child node that this Or Node must choose
        //Chosen node is random if this is null

        internal override void ReservePath(RENode child)
        {
            //this child (in Children) must be called when generating the string
            mReservedPath = child;
            base.ReservePath(child);
        }
        internal override string Generate(Random random)
        {
            if (mReservedPath != null)
            {
                //call the reserved path
                return mReservedPath.Generate(random);
            }
            else
            {
                //call a random path
                return Children[random.Next(Children.Count)].Generate(random);
            }
        }
    }

    //Represents a group of tokens each of which will be generated 
    //For example abc[A-Z] is represented by a reandnode which contains two children nodes
    class REAndNode : RENode
    {
        internal List<RENode> Children = new List<RENode>();

        internal override string Generate(Random random)
        {
            //call every node and append the strings
            StringBuilder buffer = new StringBuilder();

            foreach (RENode node in Children)
            {
                buffer.Append(node.Generate(random));
            }

            return buffer.ToString();
        }
    }

    //Represents a set of characters inside [ ] 
    //For example [a-z]
    class RESetNode : RENode
    {
        private int mMapSize = 128;
        private byte[] mMap = new byte[128]; //Indicates which characters are present in the set
        private bool mPositiveSet;           //If false, the characters added by the user are excluded
        private int mNumChoices;             //Reflects number of possible characters that can be chosen in the set

        internal RESetNode(bool positiveSet)
        {
            if (RECompiler.IsInvalidSection)
            {
                RECompiler.InvalidableNodes.Add(this);
            }

            mPositiveSet = positiveSet;
            mNumChoices = mPositiveSet ? 0 : mMapSize;  //In a negative set all characters can be chosen
        }

        //Expands the set range to cover unicode characters
        private void ExpandToUnicodeRange()
        {
            byte[] mNewMap = new byte[char.MaxValue + 1];
            Array.Copy(mMap, 0, mNewMap, 0, 128);

            if (!mPositiveSet)
                mNumChoices += char.MaxValue + 1 - 128;

            mMapSize = char.MaxValue + 1;
            mMap = mNewMap;
        }

        internal void AddChars(string chars)
        {
            //mark the added characters and update the number of available choices
            foreach (char c in chars.ToCharArray())
            {
                if (c > mMapSize - 1)
                    ExpandToUnicodeRange();

                if (mMap[c] == 0)
                {
                    mMap[c] = 1;
                    mNumChoices += mPositiveSet ? 1 : -1;
                }
            }

            //check if this set still has invalid characters available
            if ((mPositiveSet && mNumChoices == mMapSize) || (!mPositiveSet && mNumChoices == 0))
            {
                //can never be invalid
                RECompiler.InvalidableNodes.Remove(this);
            }
        }

        //Add the chars in alphabet from start to end to the set
        internal void AddRange(char start, char end)
        {
            RENode.AssertParse((start < end) && end <= char.MaxValue, "Invalid range specified in char set");

            if (end > mMapSize)
                ExpandToUnicodeRange();

            //mark the added characters and update the number of available choices
            for (long c = start; c <= end; c++)
            {
                if (mMap[c] == 0)
                {
                    mMap[c] = 1;
                    mNumChoices += mPositiveSet ? 1 : -1;
                }
            }

            //check if this set still has invalid characters available
            if ((mPositiveSet && mNumChoices == mMapSize) || (!mPositiveSet && mNumChoices == 0))
            {
                //can never be invalid
                RECompiler.InvalidableNodes.Remove(this);
            }
        }

        internal override string Generate(Random random)
        {
            if (this == RECompiler.InvalidNode)
            {
                RENode.AssertParse(mNumChoices > 0, "No valid range specified in char set");

                //select from the elements that are not available (elements that are invalid)
                int randIndex = random.Next(mMapSize - mNumChoices);

                int i = -1;
                while (randIndex >= 0)  //seek to the available element 
                {
                    i++;
                    //invert positive and negative sets
                    if ((mPositiveSet && mMap[i] == 0) || (!mPositiveSet && mMap[i] == 1))
                    {
                        randIndex--;
                    }
                }

                return Convert.ToChar(i).ToString();
            }
            else
            {
                RENode.AssertParse(mNumChoices > 0, "No valid range specified in char set");
                //select from the elements that are available
                int randIndex = random.Next(mNumChoices);

                int i = -1;
                while (randIndex >= 0)  //seek to the available element 
                {
                    i++;
                    if ((mPositiveSet && mMap[i] == 1) || (!mPositiveSet && mMap[i] == 0))
                    {
                        randIndex--;
                    }

                }

                return Convert.ToChar(i).ToString();
            }
        }
    }

    //This node represents a subexpression i.e. anything in parentheses
    //For example (abc) is a subexpression with one node in it
    class RESubExprNode : RENode
    {
        RENode mRefNode;
        internal string Name; //Identifies subexpression by name, used for named backreferences

        internal RESubExprNode(RENode subExpr)
        {
            mRefNode = subExpr;
            mRefNode.Parent = this;
        }

        internal override string Generate(Random random)
        {
            return mRefNode.Generate(random);
        }
    }

    //Transforms given regex into set of connected nodes
    class RECompiler
    {
        //InvalidableNodes is used to determine which nodes can generate non-matching strings
        //Since RESetNode fills character spaces after creation, it may have to remove itself from this list later
        //TODO: Could be implemented as an int maxID if the number of invalidable nodes does not decrease
        //Where all invalidable nodes would have an ID from 1 to maxID
        internal static bool IsInvalidSection;
        internal static List<RENode> InvalidableNodes = new List<RENode>(); //list of nodes that can be invalid
        internal static RENode InvalidNode; //node selected to generate invalid text
        StringBuilder mRegex;   //Regex that is being processed
        List<RENode> mBackRefs = new List<RENode>();       //Holds indexed backreferences
        List<RENode> mNamedBackRefs = new List<RENode>();  //Holds named backreferences
        int mIndex = -1;            //Index of current char being processed
        char mCurrent = '0';        //Current char being processed
        bool mParseDone;            //Parse complete?

        internal void AssertParse(bool b, string message) //Assert in parsing
        {
            if (!b)
                throw new ArgumentException("Regex parse error at index " + mIndex + ": " + message);
        }

        //This function must be used instead of trying to get backref from dictionary
        //Named references should be indexed according to regex spec in .net. This function handles that.
        RENode GetBackRef(int index)
        {
            try
            {
                //Backreference indexes are 1 based
                return (index <= mBackRefs.Count) ? mBackRefs[index - 1]
                                                    : mNamedBackRefs[index - mBackRefs.Count - 1];
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        //Retrieve a backreference based on its name
        RENode GetBackRef(string name)
        {
            foreach (RESubExprNode node in mNamedBackRefs)
            {
                if (node.Name.Equals(name))
                {
                    return node;
                }
            }

            return null;
        }

        //Move onto next char for processing
        private void NextChar()
        {
            if (mIndex < mRegex.Length - 1)
            {
                mCurrent = mRegex[++mIndex];
            }
            else
            {
                mParseDone = true;
            }
        }

        //Parse the character preceded by an escape character
        internal char EscapeValue()
        {
            int value = 0;

            if (Char.ToLower(mCurrent, CultureInfo.InvariantCulture) == 'x')   //Hexadecimal
            {
                NextChar();

                AssertParse(Uri.IsHexDigit(mCurrent), "Invalid escape character.");

                while (Uri.IsHexDigit(mCurrent) && (!mParseDone))
                {
                    value *= 16;
                    value += Char.IsDigit(mCurrent) ? mCurrent - '0' : Char.ToLower(mCurrent, CultureInfo.InvariantCulture) - 'a' + 10;
                    NextChar();
                }
            }
            else if (mCurrent == '0')    //Octal
            {
                NextChar();

                AssertParse(mCurrent >= '0' && mCurrent <= '7', "Invalid escape character.");

                while (mCurrent >= '0' && mCurrent <= '7' && (!mParseDone))
                {
                    value *= 8;
                    value += mCurrent - '0';
                    NextChar();
                }
            }
            else if (Char.IsDigit(mCurrent))    //Decimal
            {
                while (Char.IsDigit(mCurrent) && (!mParseDone))
                {
                    value *= 10;
                    value += mCurrent - '0';
                    NextChar();
                }
            }
            else
            {
                AssertParse(false, "Invalid escape character.");
            }

            return (char)value;
        }

        //Parse the set character preceded by an escape character
        private char EscapeSetChar()
        {
            char c = '0';

            if (Char.ToLower(mCurrent, CultureInfo.InvariantCulture) == 'x' || Char.IsDigit(mCurrent))
            {
                return EscapeValue();
            }

            switch (mCurrent)
            {
                case '^': c = '^'; break;
                case '*': c = '*'; break;
                case '\\': c = '\\'; break;
                case 'r': c = '\r'; break;
                case 'a': c = '\a'; break;
                case 'b': c = '\b'; break;
                case 'e': c = '\x1B'; break; //ESC key
                case 'n': c = '\n'; break;
                case 't': c = '\t'; break;
                case 'f': c = '\f'; break;
                case 'v': c = '\v'; break;
                case '-': c = '-'; break;
                case '[': c = '['; break;
                case ']': c = ']'; break;
                default:
                    AssertParse(false, "Invalid escape inside of set.");
                    break;
            }

            NextChar();

            return c;
        }

        //Compiles set char, also handles characters specified with escape chars
        private char CompileSetChar()
        {
            char val = mCurrent;
            NextChar();
            AssertParse(val != '-', "Invalid character inside set.");
            return (val == '\\') ? EscapeSetChar() : val;
        }

        //Compile the regex given in pattern parameter
        internal RENode Compile(string pattern)
        {
            mRegex = new StringBuilder(pattern);
            mParseDone = false;
            NextChar();
            return CompileExpr();
        }

        //Compile the expression i.e. main body or expr in paranthesis
        internal RENode CompileExpr()
        {
            RENode branch = CompileBranch();

            if (mCurrent != '|')
            {
                return branch;
            }

            REOrNode expr = new REOrNode();
            expr.Children.Add(branch);
            branch.Parent = expr;

            while (mCurrent == '|')
            {
                NextChar();
                RENode nextBranch = CompileBranch();
                expr.Children.Add(nextBranch);
                nextBranch.Parent = expr;
            }

            return expr;
        }

        //Compile node starting with |
        internal RENode CompileBranch()
        {
            RENode piece = CompilePiece();

            if (mParseDone || mCurrent == '|' || mCurrent == ')')
            {
                return piece;
            }

            REAndNode andNode = new REAndNode();
            andNode.Children.Add(piece);
            piece.Parent = andNode;

            while (!(mParseDone || mCurrent == '|' || mCurrent == ')'))
            {
                RENode nextPiece = CompilePiece();
                andNode.Children.Add(nextPiece);
                nextPiece.Parent = andNode;
            }

            return andNode;
        }

        //Compile token followed by *+?{}
        internal RENode CompilePiece()
        {
            RENode node = null;

            //store the old invalidating state for restoring after this node
            bool oldInvalidState = RECompiler.IsInvalidSection;
            //check if we want to invalidate the 'atom' node and subnodes
            if (mCurrent == '\\' && mRegex[mIndex + 1] == 'i') //entering invalidating nodes section
            {
                NextChar();
                NextChar();
                //invalidate the following node and subnodes
                RECompiler.IsInvalidSection = true;
            }

            RENode atom = CompileAtom();

            //revert the invalidating state
            RECompiler.IsInvalidSection = oldInvalidState;

            //check special case of invalidating a repeating node
            //have to confirm with "*+?{" to verify that it's not another type of node (that parses elsewhere)
            if (mCurrent == '\\' && mRegex[mIndex + 1] == 'i' && "*+?{".Contains(mRegex[mIndex + 2].ToString()))
            {
                NextChar();
                NextChar();
                //invalidate the repeating node
                RECompiler.IsInvalidSection = true;
            }

            const int MAXREPEAT = -1; //value representing infinity

            switch (mCurrent)
            {
                case '*': //zero or more repetition
                    node = new RERepeatNode(atom, 0, MAXREPEAT, false);
                    NextChar();
                    break;
                case '+': //one or more repetition
                    node = new RERepeatNode(atom, 1, MAXREPEAT, false);
                    NextChar();
                    break;
                case '?': //zero or one repetition
                    node = new RERepeatNode(atom, 0, 1, false);
                    NextChar();
                    break;
                case '{': //Min and max repetition limits defined
                    int nMin = 0;
                    int nMax = 0;
                    bool sameChar = false;
                    NextChar();

                    if (mCurrent == '=')
                    {
                        sameChar = true;
                        NextChar();
                    }

                    int closeIndex = mRegex.ToString().IndexOf('}', mIndex);
                    AssertParse(closeIndex != -1, "Expected '}'");

                    string[] repeatTokens = mRegex.ToString().Substring(mIndex, closeIndex - mIndex).
                                            Split(new char[] { ',' });

                    if (repeatTokens.Length == 1)
                    {
                        nMin = nMax = int.Parse(repeatTokens[0], CultureInfo.InvariantCulture);
                    }
                    else if (repeatTokens.Length == 2)
                    {
                        nMin = int.Parse(repeatTokens[0], CultureInfo.InvariantCulture);
                        //check for {n,} case
                        if (repeatTokens[1].Length > 0)
                        {
                            nMax = int.Parse(repeatTokens[1], CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            nMax = MAXREPEAT; //only lower bound specified
                        }
                    }
                    else
                    {
                        AssertParse(false, "Repeat values cannot be parsed");
                    }

                    AssertParse(nMin <= nMax || repeatTokens[1].Length == 0, "Max repeat is less than min repeat");
                    mIndex = closeIndex;
                    NextChar();
                    node = new RERepeatNode(atom, nMin, nMax, sameChar);
                    break;
                default:
                    node = atom;
                    break;
            }

            //revert invalidation after generating the repeating node
            RECompiler.IsInvalidSection = oldInvalidState;

            return node;
        }

        //Compile token 
        internal RENode CompileAtom()
        {
            RENode atom = null;
            RESetNode set = null;
            int start = 0;
            int end = 0;

            AssertParse(!mParseDone, "Reached end of string. No element found.");
            AssertParse(!("|)?+*{}".Contains(mCurrent.ToString())), "No element found.");

            switch (mCurrent)
            {
                case '.': //Any single char
                    atom = set = new RESetNode(true);
                    set.AddRange(Convert.ToChar(0), Convert.ToChar(127));
                    NextChar();
                    break;
                case '[': //Positive or negative set
                    NextChar();
                    atom = CompileSet();
                    break;
                case '(': //Sub expression
                    int refIndex = 0; //-2 -> don't capture, -1 -> named capture, 0-> indexed capture
                    NextChar();

                    //By default, subexpressions must be captured for future reference, 
                    if (mCurrent == '?')
                    {
                        NextChar();
                        if (mCurrent == ':') //If sub expression begins with ?: it means don't store reference
                        {
                            NextChar();
                            refIndex = -2;
                        }
                        else //Named backreference, extract backreference name
                        {
                            ExtractBackrefName(ref start, ref end);
                            refIndex = -1;
                        }
                    } //else use indexed backreference

                    atom = new RESubExprNode(CompileExpr());
                    AssertParse(mCurrent == ')', "Expected ')'");
                    NextChar();

                    if (refIndex == -1) //Named backreference
                    {
                        (atom as RESubExprNode).Name = mRegex.ToString().Substring(start, end - start + 1);
                        mNamedBackRefs.Add(atom);
                    }
                    else if (refIndex == 0) //Indexed backreference
                    {
                        mBackRefs.Add(atom);
                    }

                    break;
                case '^':
                case '$':
                    atom = new RETextNode(String.Empty);
                    NextChar();
                    break;
                case '\\':
                    NextChar();

                    if (Char.ToLower(mCurrent, CultureInfo.InvariantCulture) == 'x' || Char.ToLower(mCurrent, CultureInfo.InvariantCulture) == 'u' || mCurrent == '0')
                    {
                        atom = new RETextNode(EscapeValue().ToString());
                    }
                    else if (Char.IsDigit(mCurrent))
                    {
                        atom = GetBackRef((int)EscapeValue());
                        AssertParse(atom != null, "Couldn't find back reference");
                        atom = new RESubExprNode(atom);
                    }
                    else if (mCurrent == 'k') //referencing a backreference by name
                    {
                        NextChar();
                        ExtractBackrefName(ref start, ref end);
                        atom = GetBackRef(mRegex.ToString().Substring(start, end - start + 1));
                        AssertParse(atom != null, "Couldn't find back reference");
                        atom = new RESubExprNode(atom); //Create a copy of the referenced node
                    }
                    else
                    {
                        atom = CompileSimpleMacro(mCurrent);
                        NextChar();
                    }
                    break;
                default:
                    int closeIndex = mRegex.ToString().IndexOfAny("-*+?(){}\\[]^$.|".ToCharArray(), mIndex + 1);

                    if (closeIndex == -1)
                    {
                        mParseDone = true;
                        closeIndex = mRegex.Length - 1;
                        atom = new RETextNode(mRegex.ToString().Substring(mIndex, closeIndex - mIndex + 1));
                    }
                    else
                    {
                        atom = new RETextNode(mRegex.ToString().Substring(mIndex, closeIndex - mIndex));
                    }

                    mIndex = closeIndex;
                    mCurrent = mRegex[mIndex];
                    break;
            }

            return atom;
        }

        //Parse backreference name in form of <name> or 'name'
        internal void ExtractBackrefName(ref int start, ref int end)
        {
            char tChar = mCurrent;
            AssertParse(tChar == '\'' || tChar == '<', "Backref must begin with ' or <.");

            //Set the expected end character, if start char is < then expect >, otherwise expect '
            if (tChar == '<')
            {
                tChar = '>';
            }

            NextChar();

            AssertParse((Char.ToLower(mCurrent, CultureInfo.InvariantCulture) >= 'a' && Char.ToLower(mCurrent, CultureInfo.InvariantCulture) <= 'z') || mCurrent == '_',
                                "Invalid characters in backreference name.");
            start = mIndex;
            NextChar();

            while (mCurrent == '_' || Char.IsLetterOrDigit(mCurrent))
            {
                NextChar();
            }

            AssertParse(mCurrent == tChar, "Name end not found.");
            end = mIndex;
            NextChar();
        }

        //Compile a character set (i.e expressions like [abc], [A-Z])
        internal RENode CompileSet()
        {
            RENode atom = null;
            char cStart, cEnd;
            RESetNode set;

            if (mCurrent == ':')
            {
                NextChar();
                int closeIndex = mRegex.ToString().IndexOf(":]", StringComparison.Ordinal);
                atom = CompileMacro(mIndex, closeIndex - mIndex);
                mIndex = closeIndex;
                NextChar();
                NextChar();
                return atom;
            }

            if (mCurrent == '^')
            {
                atom = set = new RESetNode(false);
                NextChar();
            }
            else
            {
                atom = set = new RESetNode(true);
            }

            if (mCurrent == '-' || mCurrent == ']') //if - or ] are specified as the first char, escape is not required
            {
                set.AddChars(mCurrent.ToString());
                NextChar();
            }

            while ((!mParseDone) && (mCurrent != ']'))
            {
                cStart = CompileSetChar();

                if (mCurrent == '-')
                {
                    NextChar();
                    AssertParse(!mParseDone && mCurrent != ']', "End of range is not specified.");
                    cEnd = CompileSetChar();
                    set.AddRange(cStart, cEnd);
                }
                else
                {
                    set.AddChars(cStart.ToString());
                }
            }

            AssertParse(mCurrent == ']', "Expected ']'.");
            NextChar();
            return atom;
        }

        //Compile \d \D \s \S etc.
        internal RENode CompileSimpleMacro(char c)
        {
            RENode node = null;
            RESetNode set = null;

            if (@"[]{}()*-+.?\|".Contains(c.ToString()))
            {
                return new RETextNode(c.ToString());
            }

            switch (c)
            {
                case 'd': // [0-9]
                    node = set = new RESetNode(true);
                    set.AddRange('0', '9');
                    break;
                case 'D': // [^0-9]
                    node = set = new RESetNode(false);
                    set.AddRange('0', '9');
                    break;
                case 's':
                    node = set = new RESetNode(true);
                    set.AddChars(" \r\n\f\v\t");
                    break;
                case 'S':
                    node = set = new RESetNode(false);
                    set.AddChars(" \r\n\f\v\t");
                    break;
                case 'w': // [a-zA-Z0-9_]
                    node = set = new RESetNode(true);
                    set.AddRange('a', 'z');
                    set.AddRange('A', 'Z');
                    set.AddRange('0', '9');
                    set.AddChars("_");
                    break;
                case 'W': // [^a-zA-Z0-9_]
                    node = set = new RESetNode(false);
                    set.AddRange('a', 'z');
                    set.AddRange('A', 'Z');
                    set.AddRange('0', '9');
                    set.AddChars("_");
                    break;
                case 'f':
                    node = new RETextNode("\f");
                    break;
                case 'n':
                    node = new RETextNode("\n");
                    break;
                case 'r':
                    node = new RETextNode("\r");
                    break;
                case 't':
                    node = new RETextNode("\t");
                    break;
                case 'v':
                    node = new RETextNode("\v");
                    break;
                case 'A':
                case 'Z':
                case 'z':
                    node = new RETextNode(String.Empty);
                    break;
                default:
                    AssertParse(false, "Invalid escape.");
                    break;
            }

            return node;
        }

        //Compile [:alpha:] [:punct:] etc
        internal RENode CompileMacro(int index, int len)
        {
            AssertParse(len >= 0, "Cannot parse macro.");
            string substr = mRegex.ToString().Substring(index, len);
            string expanded = null;

            switch (substr)
            {
                case "alnum": expanded = "[a-zA-Z0-9]"; break;
                case "alpha": expanded = "[a-zA-Z]"; break;
                case "upper": expanded = "[A-Z]"; break;
                case "lower": expanded = "[a-z]"; break;
                case "digit": expanded = "[0-9]"; break;
                case "xdigit": expanded = "[A-F0-9a-f]"; break;
                case "space": expanded = "[ \t]"; break;
                case "print": expanded = "[\\x20-\\x7F]"; break;
                case "punct": expanded = "[,;.!'\"]"; break;
                case "graph": expanded = "[\\x80-\\xFF]"; break;
                case "cntrl": expanded = "[]"; break;
                case "blank": expanded = "[ \t\r\n\f]"; break;
                case "guid": expanded = "[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}"; break;
                default: AssertParse(false, "Cannot parse macro."); break;
            }

            RECompiler subcompiler = new RECompiler();
            return subcompiler.Compile(expanded);
        }
    }

    /// <summary>
    /// Class to generate random data that matches a given regular expression.
    /// </summary>
    public static class RegexGen
    {
        /// <summary>
        /// Generates a string based on the given regular expression
        /// if any nodes are prepended with \i, then one of these nodes will be chosen
        /// at random to be invalidated
        /// </summary>
        /// <param name="random">Random object to use for generation</param>
        /// <param name="regex">Regular expression used to generate the string</param>
        /// <returns>generated string</returns>
        public static string NextString(Random random, string regex)
        {
            //reset the static variables
            RECompiler.IsInvalidSection = false;
            RECompiler.InvalidNode = null;
            RECompiler.InvalidableNodes.Clear();

            //construct the RegEx tree
            RECompiler compiler = new RECompiler();
            RENode node = compiler.Compile(regex);

            //search for a signal to invalidate a node
            if (regex.IndexOf("\\i", StringComparison.Ordinal) != -1)
            {
                //something should have been invalidated
                //select a node to invalidate
                if (RECompiler.InvalidableNodes.Count == 0)
                {
                    throw new ArgumentException("Asked to generate invalid: Impossible to invalidate");
                }
                RECompiler.InvalidNode = RECompiler.InvalidableNodes[random.Next(RECompiler.InvalidableNodes.Count)];

                //Mark REOrNodes and RERepeatNodes to ensure that the invalid node will be part of the string
                RECompiler.InvalidNode.ReservePath(null);
            }

            //generate and return the string
            string result = node.Generate(random);

            if (RECompiler.InvalidNode != null)
            {
                //confirm that the generated string is invalid (e.g. [a-z]|[^a-z] will always fail)
                Regex compare = new Regex("^" + regex.Replace("\\i", "") + "$");
                if (compare.IsMatch(result))
                {
                    throw new ArgumentException(regex + ": Did not generate invalid string: " + result);
                }
            }

            return result;
        }
    }
}
