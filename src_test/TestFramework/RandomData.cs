using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdSage.Concert.Test.Framework
{
    /// <summary>
    /// RandomData is a test helper for creating random data.  All random
    /// data creation must be done through this class; if a random number
    /// is needed in the test, use the Random instance here, rather than
    /// your own instance of Random (except in threaded tests).
    /// </summary>
    public class RandomData : TestHelper
    {
        /// <summary>
        /// Valid Ascii space characters
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly char[] AsciiSpaceChars = " \t\r\n".ToCharArray();

        /// <summary>
        /// Valid Ascii word character (alphanumeric, underscore or dash)
        /// </summary>
        public const string AsciiWordChar = @"[0-9A-Za-z_\-]";

        /// <summary>
        /// Valid Unicode word character (ascii alphanumeric, underscore, dash,
        /// Latin-1 (128-255), Latin extended-a (256-383), Cyrillic (1024-1153,1162-1279),
        /// Arabic (1569-1610,1632-1641,1646-1647,1649-1747)
        /// Japanese-Hiragana (12353-12438)
        /// Japanese-Katakana (12449-12539)
        /// Chinese (19968-20068) .... there's a lot more, but we don't want too many here
        /// TODO: there's a language that drove OrdinalIgnoreCase, include it
        /// TODO: CJK compatibility?
        /// </summary>
        public const string UnicodeWordChar = "[" +
            @"0-9A-Za-z_\-" + // basic Ascii
            @"\138\142\154\158\159\192-\255\256-\383" + // Latin
            //            @"\1024-\1159\1162-\1279" + // Cyrillic
            //            @"\1569-\1610,\1632-\1641\1646-\1647,\1649-\1747" + // Arabic
            //            @"\12353-\12438\12449-\12549" + // Japanese, Hiragana and Katakana
            //            @"\19968-\20068" + // Some Chinese ideograms
            @"]";

        /// <summary>
        /// Underlying Random that is used for random number generation.
        /// </summary>
        public Random Random { get; private set; }

        /// <summary>
        /// Basic constructor; use OnHelperCreation() to bind to test and
        /// make this object usable.
        /// </summary>
        public RandomData()
        {
        }

        public override void OnHelperCreation(TestBase test)
        {
            base.OnHelperCreation(test);
            Random = Test.Get<Random>();
        }

        /// <summary>
        /// Create a random DateTime obj within the [from, to) range specified
        /// </summary>
        /// <param name="from">The minimum of the returned DateTime range</param>
        /// <param name="to">The maximum of the returned DateTime range</param>
        /// <returns></returns>
        public DateTime NextDateTime(DateTime from, DateTime to)
        {
            TimeSpan range = new TimeSpan(to.Ticks - from.Ticks);
            return from + new TimeSpan((long)(range.Ticks * Random.NextDouble()));
        }

        /// <summary>
        /// Create a random DateTime obj within the [current - timeToLookBack, current) range specified
        /// </summary>
        /// <param name="timeToLookBack">maximim time to look back</param>
        /// <returns>the random time generated</returns>
        public DateTime NextDateTime(TimeSpan timeToLookBack)
        {
            return NextDateTime(DateTime.Now - timeToLookBack,
                                DateTime.Now);
        }

        /// <summary>
        /// Returns a random string without spaces containing only
        /// English alphabetic letters (upper and lower case), English
        /// numerals, dash and underscore.  The string will be of at least one
        /// character, and no more than maxLength characters.
        /// </summary>
        /// <param name="maxLength">The maximum length of the returned string</param>
        /// <returns>Random string</returns>
        public string NextAsciiWord(int maxLength)
        {
            return NextAsciiWord(1, maxLength);
        }

        /// <summary>
        /// Returns a random string without spaces containing only
        /// English alphabetic letters (upper and lower case), English
        /// numerals, dash and underscore.  The string will be of at least minLength
        /// characters, and no more than maxLength characters.
        /// </summary>
        /// <param name="minLength">The minimum length of the returned string</param>
        /// <param name="maxLength">The maximum length of the returned string</param>
        /// <returns>Random string</returns>
        public string NextAsciiWord(int minLength, int maxLength)
        {
            return RegexGen.NextString(Random, AsciiWordChar + "{" + minLength + "," + maxLength + "}");
        }

        /// <summary>
        /// Returns a string containings words and space (and other) characters.
        /// </summary>
        public string NextAsciiWords(int maxLength)
        {
            return NextAsciiWords(1, maxLength);
        }

        /// <summary>
        /// Returns a string containings words and space (and other) characters.
        /// </summary>
        public string NextAsciiWords(int minLength, int maxLength)
        {
            int length = NextInt32(minLength, maxLength);
            StringBuilder response = new StringBuilder();

            while (response.Length < length)
            {
                int maxWordLength = length - response.Length;
                string word = NextAsciiWord(maxWordLength);
                Assert.IsTrue(word.Length <= maxWordLength, "maxWordLength: " + maxWordLength + ", wordLength: " + word.Length);
                response.Append(word);
                response.Append(NextChoice(AsciiSpaceChars));
            }

            // Remove the trailing space, replacing it with a letter if necessary
            response.Remove(response.Length - 1, 1);
            if (response.Length == length - 1)
            {
                response.Append(NextAsciiWord(1, 1));
            }

            Assert.AreEqual(length, response.Length, "Built string length mismatch");
            return response.ToString();
        }

        /// <summary>
        /// Returns a random string without spaces containing only
        /// valid alphabetic letters (upper and lower case) and numerals in various
        /// scripts, dash and underscore.  The string will be of at least one
        /// character, and no more than maxLength characters.
        /// </summary>
        /// <param name="maxLength">The maximum length of the returned string</param>
        /// <returns>Random string</returns>
        public string NextUnicodeWord(int maxLength)
        {
            return NextUnicodeWord(1, maxLength);
        }

        /// <summary>
        /// Returns a random string without spaces containing only
        /// valid alphabetic letters (upper and lower case) and numerals in various
        /// scripts, dash and underscore.  The string will be of at least minLength
        /// characters, and no more than maxLength characters.
        /// </summary>
        /// <param name="minLength">The minimum length of the returned string</param>
        /// <param name="maxLength">The maximum length of the returned string</param>
        /// <returns>Random string</returns>
        public string NextUnicodeWord(int minLength, int maxLength)
        {
            return RegexGen.NextString(Random, UnicodeWordChar + "{" + minLength + "," + maxLength + "}");
        }

        /// <summary>
        /// Returns a random English word with lower case.
        /// The string will be of at least one character,
        ///  and no more than maxLength characters.
        /// </summary>
        /// <param name="maxLength">The maximum length of the returned string</param>
        /// <returns>Random string</returns>
        public string NextEnglishWordLowercase(int maxLength)
        {
            return NextEnglishWordLowercase(1, maxLength);
        }

        /// <summary>
        /// Returns a random English word with lower case.
        ///  The string will be of at least minLength
        /// characters, and no more than maxLength characters.
        /// </summary>
        /// <param name="minLength">The minimum length of the returned string</param>
        /// <param name="maxLength">The maximum length of the returned string</param>
        /// <returns>Random string</returns>
        public string NextEnglishWordLowercase(int minLength, int maxLength)
        {
            return RegexGen.NextString(Random, @"[a-z]{" + minLength + "," + maxLength + "}");
        }

        /// <summary>
        /// Creates a random digit string with length.
        /// </summary>
        /// <returns>Digitial string</returns>
        public string NextDigits(int length)
        {
            string len = length.ToString(CultureInfo.InvariantCulture);
            return RegexGen.NextString(Random, @"\d{" + len + "}");
        }

        /// <summary>
        /// Creates a random North American phone number.  Note that this
        /// seems to be somewhat buggy.
        /// </summary>
        /// <returns>North American phone number</returns>
        public string NextNorthAmericaPhoneNumber()
        {
            return RegexGen.NextString(Random, @"((\(\d{3}\)?)|(\d{3}-))\d{3}-\d{4}");
        }

        /// <summary>
        /// Creates a random 16-bit unsigned integer
        /// </summary>
        /// <returns>Random 16-bit unsigned integer</returns>
        public UInt16 NextUInt16()
        {
            return NextUInt16(UInt16.MinValue, UInt16.MaxValue);
        }

        /// <summary>
        /// Creates a random 16-bit unsigned integer within the specified range,
        /// up to but NOT INCLUDING maxValue. If Min and Max Values are out of range 
        /// UInt16.MinValue and UInt16.Max value are used.
        /// </summary>
        /// <param name="minValue">Minimum value for the returned integer</param>
        /// <param name="maxValue">Maximum value for the returned integer</param>
        /// <returns>Random 16-bit unsigned integer within the specified range</returns>
        public UInt16 NextUInt16(int minValue, int maxValue)
        {
            if (minValue < 0)
            {
                minValue = UInt16.MinValue;
            }
            if (maxValue > UInt16.MaxValue)
            {
                maxValue = UInt16.MaxValue;
            }

            return Convert.ToUInt16(Random.Next(minValue, maxValue));
        }

        /// <summary>
        /// Creates a random 64-bit unsigned integer.
        /// </summary>
        /// <returns>Random 64-bit unsigned integer</returns>
        public ulong NextUInt64()
        {
            ulong high = ((ulong)NextInt32()) << 32;
            ulong low = (ulong)NextInt32();
            return high | low;
        }

        /// <summary>
        /// Creates a random 32-bit unsigned integer.
        /// </summary>
        /// <returns>Random 32-bit unsigned integer</returns>
        public uint NextUInt32()
        {
            return (uint)NextInt32();
        }

        /// <summary>
        /// Creates a random 32-bit unsigned integer within the specified range,
        /// up to but NOT INCLUDING maxValue.
        /// </summary>
        /// <param name="minValue">Minimum value for the returned unsigned integer</param>
        /// <param name="maxValue">Maximum value +1 for the returned unsigned integer</param>
        /// <returns>Random 32-bit unsigned integer within the specified range</returns>
        public uint NextUInt32(uint minValue, uint maxValue)
        {
            return (uint)NextInt64((long)minValue, (long)maxValue);
        }


        /// <summary>
        /// Creates a random byte.
        /// </summary>
        /// <returns>Random 16-bit integer</returns>
        public byte NextByte()
        {
            return NextByte(Byte.MinValue, Byte.MaxValue);
        }

        /// <summary>
        /// Creates a random byte within the specified range,
        /// up to but NOT INCLUDING maxValue.
        /// </summary>
        /// <param name="minValue">Minimum value for the returned byte</param>
        /// <param name="maxValue">Maximum value +1 for the returned byte</param>
        /// <returns>Random byte within the specified range</returns>
        public byte NextByte(short minValue, short maxValue)
        {
            return (byte)Random.Next(minValue, maxValue);
        }


        /// <summary>
        /// Creates a random 16-bit integer.
        /// </summary>
        /// <returns>Random 16-bit integer</returns>
        public short NextInt16()
        {
            return NextInt16(Int16.MinValue, Int16.MaxValue);
        }

        /// <summary>
        /// Creates a random 16-bit integer within the specified range,
        /// up to but NOT INCLUDING maxValue.
        /// </summary>
        /// <param name="minValue">Minimum value for the returned integer</param>
        /// <param name="maxValue">Maximum value +1 for the returned integer</param>
        /// <returns>Random 16-bit integer within the specified range</returns>
        public short NextInt16(short minValue, short maxValue)
        {
            return (short)Random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Creates a random 32-bit integer.
        /// </summary>
        /// <returns>Random 32-bit integer</returns>
        public int NextInt32()
        {
            return NextInt32(Int32.MinValue, Int32.MaxValue);
        }

        /// <summary>
        /// Creates a random 64-bit integer within the specified range,
        /// up to but NOT INCLUDING maxValue.
        /// </summary>
        /// <param name="minValue">Minimum value for the returned integer</param>
        /// <param name="maxValue">Maximum value +1 for the returned integer</param>
        /// <returns>Random 64-bit integer within the specified range</returns>
        public long NextInt64(long minValue, long maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue", "maxValue must be greater than or equal to minValue");
            }
            long range = maxValue - minValue;
            return ((long)(Random.NextDouble() * range)) + minValue;
        }

        /// <summary>
        /// Creates a random double within the specified range,
        /// up to but NOT INCLUDING maxValue.
        /// </summary>
        /// <param name="minValue">Minimum value for the returned double</param>
        /// <param name="maxValue">Maximum value +epsilon for the returned double</param>
        /// <returns>Random double within the specified range</returns>
        public double NextDouble(double minValue, double maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException("maxValue", "maxValue must be greater than or equal to minValue");
            }
            double range = maxValue - minValue;
            return Random.NextDouble() * range + minValue;
        }

        /// <summary>
        /// Creates a random 64-bit integer.
        /// </summary>
        /// <returns>Random 64-bit integer</returns>
        public long NextInt64()
        {
            long high = ((long)NextInt32()) << 32;
            uint low = NextUInt32();
            return high | low;
        }

        /// <summary>
        /// Creates a random 32-bit integer within the specified range,
        /// up to but NOT INCLUDING maxValue.
        /// </summary>
        /// <param name="minValue">Minimum value for the returned integer</param>
        /// <param name="maxValue">Maximum value +1 for the returned integer</param>
        /// <returns>Random 32-bit integer within the specified range</returns>
        public int NextInt32(int minValue, int maxValue)
        {
            return Random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Creates a random Boolean.
        /// </summary>
        /// <returns>Random boolean</returns>
        public bool NextBoolean()
        {
            return Random.Next(0, 2) == 1;
        }

        /// <summary>
        /// Creates a random Boolean with the specified probability (0.0 to 1.0) of being true
        /// </summary>
        /// <returns>Random boolean</returns>
        public bool NextBoolean(double probability)
        {
            return Random.NextDouble() <= probability;
        }

        /// <summary>
        /// Returns a random item from the passed-in set of choices.
        /// </summary>
        /// <typeparam name="T">The type of the choice</typeparam>
        /// <param name="choices">The choices</param>
        /// <returns>A random item from the choices</returns>
        public T NextChoice<T>(params T[] choices)
        {
            if (choices == null)
            {
                throw new ArgumentNullException("choices");
            }
            if (choices.Length == 0)
            {
                throw new ArgumentOutOfRangeException("choices", "At least one choice must be specified");
            }
            return choices[Random.Next(choices.Length)];
        }

        /// <summary>
        /// Returns a random number of entries from the choices, without
        /// re-using the same entries.
        /// </summary>
        /// <typeparam name="T">The type of the choices</typeparam>
        /// <param name="choices">Choices to pick from</param>
        /// <returns>Random set of choices</returns>
        public T[] NextChoices<T>(T[] choices)
        {
            return NextChoices(choices, NextInt32(1, choices.Length + 1));
        }

        /// <summary>
        /// Returns a number of entries from the choices, without
        /// re-using the same entries.
        /// </summary>
        /// <typeparam name="T">The type of the choices</typeparam>
        /// <param name="choices">Choices to pick from</param>
        /// <param name="numberOfChoices">Number of choices to pick</param>
        /// <returns>Random set of choices</returns>
        public T[] NextChoices<T>(T[] choices, int numberOfChoices)
        {
            if (numberOfChoices > choices.Length)
            {
                throw new ArgumentOutOfRangeException("numberOfChoices", "Cannot select more random items from the array than there are in it");
            }
            T[] selected = new T[numberOfChoices];
            T[] choicesCopy = new T[choices.Length];
            choices.CopyTo(choicesCopy, 0);
            for (int i = 0; i < numberOfChoices; ++i)
            {
                //Choose a random index
                int randIndex = NextInt32(i, choicesCopy.Length);
                //Swap between the item in (randIndex) location and the item in (i) location
                selected[i] = choicesCopy[randIndex];
                choicesCopy[randIndex] = choicesCopy[i];
            }
            return selected;
        }

        /// <summary>
        /// Returns a permutation of the original array
        /// re-using the same entries.
        /// </summary>
        /// <typeparam name="T">The type of the array elements</typeparam>
        /// <param name="originalList">The original array</param>
        /// <returns>Permutation of the original array</returns>
        public T[] NextPermutation<T>(T[] originalList)
        {
            return NextChoices(originalList, originalList.Length);
        }

        /// <summary>
        /// Returns a random guid.  This does not use Guid.NextGuid(),
        /// so do not count on it being world-unique, but it is
        /// predictable via seeding.
        /// </summary>
        /// <returns>Random guid</returns>
        public Guid NextGuid()
        {
            byte[] bytes = new byte[16];
            Random.NextBytes(bytes);
            return new Guid(bytes);
        }

        /// <summary>
        /// Returns a random hex string, representing the specified number of bytes.
        /// </summary>
        /// <param name="byteCount">Number of bytes to convert to hex</param>
        /// <returns>Hex string containing byteCount*2 characters</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "byte")]
        public string NextHexString(int byteCount)
        {
            byte[] bytes = new byte[byteCount];
            Random.NextBytes(bytes);
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        /// <summary>
        /// This function used to generate random string with the specified length.
        /// It can be a combination of alphabets - both cases, integers and special characters
        /// The string will be of at least one character, and no more than 
        /// maxLength characters.
        /// </summary>
        /// <param name="maxLength">Length of the string to be generated.</param>
        /// <returns>String with the length provided, if length is Less than or equal 0 then the string returned is empty.</returns>
        public string NextAsciiStringWithSpecialChars(int maxLength)
        {
            if (maxLength <= 0)
            {
                return string.Empty;
            }

            return NextAsciiStringWithSpecialChars(1, maxLength);
        }

        /// <summary>
        /// This function used to generate random string with the specified length which is not
        /// less than minLength and not greater than maxLength.
        /// It can be a combination of alphabets - both cases, integers and special characters
        /// The string will be of at minLength characters character, and no more than 
        /// maxLength characters.
        /// </summary>
        /// <param name="minLength">Minimum Length of the string to be generated.</param>
        /// /// <param name="maxLength">Maximum Length of the string to be generated.</param>
        /// <returns>String with the length provided, if length is Less than or equal 0 then the string returned is empty.</returns>
        public string NextAsciiStringWithSpecialChars(int minLength, int maxLength)
        {
            return RegexGen.NextString(Random, @"[a-zA-Z0-9{!@#$%^&\*()~_+=\-/?.>,<';:}\t]{" + minLength + "," + maxLength + "}");
        }

        /// <summary>
        /// Returns an ascii fully-qualified domain name
        /// </summary>
        /// <param name="maxLength">Maximum length for the FQDN; it will be up to this length; must be at least 4</param>
        /// <returns>FQDN</returns>
        public string NextAsciiFullyQualifiedDomainName(int maxLength)
        {
            return NextFullyQualifiedDomainName(maxLength, NextAsciiWord);
        }

#if false
        // I thought these were allowed now, but apparently not.  Code left in case .Net v4 allows them

        /// <summary>
        /// Returns an unicode fully-qualified domain name
        /// </summary>
        /// <param name="maxLength">Maximum length for the FQDN; it will be up to this length; must be at least 4</param>
        /// <returns>FQDN</returns>
        public string NextUnicodeFullyQualifiedDomainName(int maxLength)
        {
            // Thought: what happens to the rule "last part >= 2 chars" if there are Hebrew/Arabic chars mixed in?
            return NextFullyQualifiedDomainName(maxLength, NextUnicodeWord);
        }
#endif

        /// <summary>
        /// Core code used by the Ascii and Unicode variants. Do not call directly.
        /// </summary>
        /// <param name="maxLength">Maximum length for the FQDN; it will be up to this length; must be at least 4</param>
        /// <param name="randomizer">Returns a random string within the given length parameters</param>
        /// <returns>FQDN</returns>
        string NextFullyQualifiedDomainName(int maxLength, Func<int, int, string> randomizer)
        {
            if (maxLength < 4)
            {
                throw new ArgumentOutOfRangeException("maxLength", "A FQDN must be at least four characters long");
            }

            int actualLength = Random.Next(4, maxLength + 1);
            // Allow up to five parts, except for very short names
            int numberOfParts = (actualLength < 10) ? 2 : NextInt32(2, 6);

            // Each part has at least 1 character, except the last which has at least 2
            int[] partLengths = new int[numberOfParts];

            int i;
            for (i = 0; i < numberOfParts - 1; i++)
            {
                partLengths[i] = 1;
            }
            partLengths[numberOfParts - 1] = 2;

            // Allocate the remaining characters, allowing n+1 already, and n-1 periods
            for (i = actualLength - (2 * numberOfParts); i > 0; i--)
            {
                partLengths[Random.Next(numberOfParts)]++;
            }

            StringBuilder fqdn = new StringBuilder(actualLength);

            for (i = 0; i < numberOfParts; i++)
            {
                if (i > 0)
                {
                    fqdn.Append('.');
                }
                string part = randomizer(partLengths[i], partLengths[i]);
                if (part.Length > 63)
                {
                    actualLength = actualLength - (part.Length - 63);
                    part = part.Substring(0, 63);
                }
                if (part[0] == '-')
                {
                    part = 'a' + part.Substring(1);
                }
                else if (part[0] == '_')
                {
                    part = 'b' + part.Substring(1);
                }
                fqdn.Append(part);
            }

            Assert.AreEqual(actualLength, fqdn.Length, "Internal error in NextFqdn, generated string was " + fqdn.Length + " characters, maxLength was " + maxLength + ".  Generated string: " + fqdn);
            Assert.AreNotEqual(UriHostNameType.Unknown, Uri.CheckHostName(fqdn.ToString().Replace('_', '-')), "Host name is in an invalid format, name: \"" + fqdn.ToString().Replace('_', '-') + "\"");
            return fqdn.ToString().Replace('_', '-');
        }

        /// <summary>
        /// Creates a random email address no longer than the given length.
        /// </summary>
        /// <param name="maxLength">The maximum length the email address is
        /// allowed to be; must be at least 5</param>
        /// <returns>Random email address</returns>
        public string NextAsciiEmail(int maxLength)
        {
            if (maxLength < 5)
            {
                throw new ArgumentOutOfRangeException("maxLength", "Email addresses must be at least five characters long");
            }

            string fqdn = NextAsciiFullyQualifiedDomainName(maxLength - 2);
            string email = NextAsciiWord(maxLength - 1 - fqdn.Length) + "@" + fqdn;
            return email;
        }

        /// <summary>
        /// Creates a random email address no longer than the given length.
        /// </summary>
        /// <param name="maxLength">The maximum length the email address is
        /// allowed to be; must be at least 5</param>
        /// <returns>Random email address</returns>
        public string NextUnicodeEmail(int maxLength)
        {
            if (maxLength < 5)
            {
                throw new ArgumentOutOfRangeException("maxLength", "Email addresses must be at least five characters long");
            }

            string fqdn = NextAsciiFullyQualifiedDomainName(maxLength - 2);
            string email = NextUnicodeWord(maxLength - 1 - fqdn.Length) + "@" + fqdn;
            return email;
        }

        /// <summary>
        /// Returns a random ascii url (http or https only).
        /// </summary>
        /// <param name="maxLength">maximum length for the url; must be at least 12 characters</param>
        /// <returns>Random url</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public string NextAsciiUrl(int maxLength)
        {
            return "http://www.adsage.com";
            //if (maxLength < 12)
            //{
            //    throw new ArgumentOutOfRangeException("maxLength", "Urls must be at least 12 characters");
            //}

            //string fqdn = NextAsciiFullyQualifiedDomainName(maxLength - 9);
            //string baseUrl = RegexGen.NextString(Random, "https?://") + fqdn;
            //string fullUrl;

            //int remainingLength = maxLength - baseUrl.Length;
            //int numberOfParts = NextInt32(0, remainingLength / 2);
            //if (numberOfParts == 0)
            //{
            //    fullUrl = baseUrl + (NextBoolean() ? "/" : "");
            //}
            //else
            //{
            //    string[] parts = new string[numberOfParts];
            //    int maxPartLength = (remainingLength / numberOfParts) - 1;
            //    for (int i = 0; i < parts.Length; i++)
            //    {
            //        parts[i] = NextAsciiWord(1, maxPartLength);
            //    }
            //    fullUrl = baseUrl + "/" + String.Join("/", parts);
            //}

            //Assert.IsTrue(fullUrl.Length <= maxLength, "Internal error in NextUrl, generated string was " + fullUrl.Length + " characters, maxLength was " + maxLength + ".  Generated string: " + fullUrl);
            //return fullUrl;
        }

        /// <summary>
        /// Returns a random unicode url (http or https only).
        /// </summary>
        /// <param name="maxLength">maximum length for the url; must be at least 12 characters</param>
        /// <returns>Random url</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public string NextUnicodeUrl(int maxLength)
        {
            if (maxLength < 12)
            {
                throw new ArgumentOutOfRangeException("maxLength", "Urls must be at least 12 characters");
            }

            string fqdn = NextAsciiFullyQualifiedDomainName(maxLength - 9);
            string baseUrl = RegexGen.NextString(Random, "https?://") + fqdn;
            string fullUrl;

            int remainingLength = maxLength - baseUrl.Length;
            int numberOfParts = NextInt32(0, remainingLength / 2);
            if (numberOfParts == 0)
            {
                fullUrl = baseUrl + (NextBoolean() ? "/" : "");
            }
            else
            {
                string[] parts = new string[numberOfParts];
                int maxPartLength = (remainingLength / numberOfParts) - 1;
                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i] = NextUnicodeWord(1, maxPartLength);
                }
                fullUrl = baseUrl + "/" + String.Join("/", parts);
            }

            Assert.IsTrue(fullUrl.Length <= maxLength, "Internal error in NextUrl, generated string was " + fullUrl.Length + " characters, maxLength was " + maxLength + ".  Generated string: " + fullUrl);
            return fullUrl;
        }

        /// <summary>
        /// Returns a random IPV4 address
        /// </summary>
        /// <returns>Random IPV4 address</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Pv")]
        public string NextIPv4Address()
        {
            string ip = NextInt16(1, 256).ToString(CultureInfo.InvariantCulture) + "." + NextInt16(0, 256).ToString(CultureInfo.InvariantCulture) + "." + NextInt16(0, 256).ToString(CultureInfo.InvariantCulture) + "." + NextInt16(0, 256).ToString(CultureInfo.InvariantCulture);
            return ip;
        }
    }
}

#if false // useful tests
            CultureInfo turkish = CultureInfo.GetCultureInfo("tr-tr");
            string str1 = "fooi";
            string str2 = "foOI";
            Trace.TraceInformation("i compare not-ignore case invariant: " + String.Compare(str1, str2, false, CultureInfo.InvariantCulture));
            Trace.TraceInformation("i compare     ignore case invariant: " + String.Compare(str1, str2, true, CultureInfo.InvariantCulture));
            Trace.TraceInformation("i compare not-ignore case turkish  : " + String.Compare(str1, str2, false, turkish));
            Trace.TraceInformation("i compare     ignore case turkish  : " + String.Compare(str1, str2, true, turkish));
            str1 = "foo";
            str2 = "foO";
            Trace.TraceInformation("  compare not-ignore case invariant: " + String.Compare(str1, str2, false, CultureInfo.InvariantCulture));
            Trace.TraceInformation("  compare     ignore case invariant: " + String.Compare(str1, str2, true, CultureInfo.InvariantCulture));
            Trace.TraceInformation("  compare not-ignore case turkish  : " + String.Compare(str1, str2, false, turkish));
            Trace.TraceInformation("  compare     ignore case turkish  : " + String.Compare(str1, str2, true, turkish));
            str1 = Get<RandomData>().NextAsciiWord(100,200);
            str2 = str1.ToUpper(CultureInfo.CurrentUICulture);
            Trace.TraceInformation("random ascii: " + str1);
            Trace.TraceInformation("as upper: " + str2);
            Trace.TraceInformation("R  compare not-ignore case invariant: " + String.Compare(str1, str2, false, CultureInfo.InvariantCulture));
            Trace.TraceInformation("R  compare     ignore case invariant: " + String.Compare(str1, str2, true, CultureInfo.InvariantCulture));
            Trace.TraceInformation("R  compare not-ignore case turkish  : " + String.Compare(str1, str2, false, turkish));
            Trace.TraceInformation("R  compare     ignore case turkish  : " + String.Compare(str1, str2, true, turkish));
            str1 = Get<RandomData>().NextUnicodeWord(100,200);
            str2 = str1.ToUpper(CultureInfo.CurrentUICulture);
            Trace.TraceInformation("random unicode: " + str1);
            Trace.TraceInformation("as upper: " + str2);
            Trace.TraceInformation("R  compare not-ignore case invariant: " + String.Compare(str1, str2, false, CultureInfo.InvariantCulture));
            Trace.TraceInformation("R  compare     ignore case invariant: " + String.Compare(str1, str2, true, CultureInfo.InvariantCulture));
            Trace.TraceInformation("R  compare not-ignore case turkish  : " + String.Compare(str1, str2, false, turkish));
            Trace.TraceInformation("R  compare     ignore case turkish  : " + String.Compare(str1, str2, true, turkish));
#endif
