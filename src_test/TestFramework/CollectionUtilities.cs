using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdSage.Concert.Test.Framework
{
    public static class CollectionUtilities
    {
        public static bool AreEqual<TExpected>(this IEnumerable<TExpected> expected, IEnumerable<TExpected> actual, IEqualityComparer<TExpected> comparer, string errorLabel, out string notEqualMsg)
        {
            List<TExpected> expectedMissing = expected.Except(actual, comparer).ToList();
            List<TExpected> actualMissing = actual.Except(expected, comparer).ToList();

            int expectedCount = expected.Count();
            int actualCount = actual.Count();

            if (expectedMissing.Count == 0 && actualMissing.Count == 0 && expectedCount == actualCount)
            {
                notEqualMsg = string.Empty;
                return true;
            }

            StringBuilder error = new StringBuilder(errorLabel);
            error.Append(Environment.NewLine);
            if (expectedCount == actualCount)
            {
                error.Append("Both collections have " + expectedCount + " items." + Environment.NewLine);
            }
            else
            {
                error.Append("The expected collection has " + expectedCount + " items, while the actual collection has " + actualCount + " items." + Environment.NewLine);
            }
            if (expectedMissing.Count == 0)
            {
                error.Append("All expected items were present.");
                error.Append(Environment.NewLine);
            }
            else
            {
                error.Append(expectedMissing.Count + " expected items were missing:");
                error.Append(Environment.NewLine);
                foreach (TExpected t in expectedMissing)
                {
                    error.Append("\t");
                    error.Append(t);
                    error.Append(Environment.NewLine);
                }
            }
            if (actualMissing.Count == 0)
            {
                error.Append("All actual collection items were expected.");
                error.Append(Environment.NewLine);
            }
            else
            {
                error.Append(actualMissing.Count + " actual collection items were unexpected:" + Environment.NewLine);
                foreach (TExpected t in actualMissing)
                {
                    error.Append("\t");
                    error.Append(t);
                    error.Append(Environment.NewLine);
                }
            }
            notEqualMsg = error.Append(Environment.NewLine).ToString();
            return false;
        }

        public static void AreEqual<TExpected>(IEnumerable<TExpected> expected, IEnumerable<TExpected> actual, string message)
        {
            string errorMsg;
            bool result = expected.AreEqual<TExpected>(actual, EqualityComparer<TExpected>.Default, message, out errorMsg);
            if(!result)
            {
                Assert.Fail(errorMsg);
            }
        }
    }
}
