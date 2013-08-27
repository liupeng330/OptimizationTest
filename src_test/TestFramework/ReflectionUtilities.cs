using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace AdSage.Concert.Test.Framework
{
    /// <summary>
    /// Various utilities based on reflection that help with class
    /// implementations.
    /// </summary>
    public static class ReflectionUtilities
    {
        /// <summary>
        /// If set to true, the EqualsByProperties method will emit debug
        /// information explaining why objects are not equal.
        /// </summary>
        public static bool DebugMatches { get; set; }

        /// <summary>
        /// Check if the two objects are the same types and, if so,
        /// are each of their public properties the same value.
        /// </summary>
        /// <param name="left">Left operand</param>
        /// <param name="right">Right operand</param>
        /// <returns>True if the objects are of the same type, false otherwise</returns>
        public static bool EqualsByProperties(this object left, object right)
        {
            if (left.GetType() != right.GetType())
            {
                return false;
            }

            PropertyInfo[] properties = left.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            if (DebugMatches)
            {
                Trace.TraceInformation("Comparing object of type " + left.GetType() + " to object of type " + right.GetType());
                Trace.TraceInformation("    " + left.ToString());
                Trace.TraceInformation("    " + right.ToString());
            }

            bool equals = true;
            foreach (PropertyInfo property in properties)
            {
                object leftValue = property.GetValue(left, null);
                object rightValue = property.GetValue(right, null);
                bool propertyEquals = (leftValue != null) ? leftValue.Equals(rightValue) : ((rightValue == null) ? true : rightValue.Equals(leftValue));
                equals &= propertyEquals;

                if (!propertyEquals && DebugMatches)
                {
                    Trace.TraceInformation("    Property \"" + property.Name + "\" doesn't match");
                    Trace.TraceInformation("        " + (leftValue == null ? "<NULL>" : leftValue.ToString()));
                    Trace.TraceInformation("        " + (rightValue == null ? "<NULL>" : rightValue.ToString()));
                }
            }

            if (equals && DebugMatches)
            {
                Trace.TraceInformation("    ITEMS ARE EQUAL");
            }

            return equals;
        }

        /// <summary>
        /// Calculates a hash code on the object based upon it's public
        /// properties.  Two different instances of the same class, with
        /// identical data, will return an identical hash code.
        /// If DebugMatches is turned on, all objects will return the
        /// same hashcode, allowing for various classes that compare
        /// hashcode before comparing objects to hit the debug code in
        /// the equality method.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetHashCodeFromProperties(this object value)
        {
            if (DebugMatches)
            {
                return 0;
            }

            int hashCode = 0;
            PropertyInfo[] properties = value.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                object propertyValue = property.GetValue(value, null);
                if (propertyValue != null)
                {
                    hashCode += propertyValue.GetHashCode();
                }
            }
            return hashCode;
        }

        /// <summary>
        /// Creates a readable dump of all public properties of the given
        /// object.  This is very useful for ToString(), for example.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DumpProperties(this object value)
        {
            StringBuilder output = new StringBuilder();
            PropertyInfo[] properties = value.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                object propertyValue = property.GetValue(value, null);
                output.Append(property.Name);
                output.Append(": ");
                output.Append(propertyValue == null ? "<NULL>" : propertyValue.ToString());
                output.Append(", ");
            }
            // remove the trailing ", "
            if (output.Length >= 2)
            {
                output.Remove(output.Length - 2, 2);
            }
            return output.ToString();
        }
    }
}
