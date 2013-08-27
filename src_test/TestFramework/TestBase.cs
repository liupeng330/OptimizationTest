using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Reflection;

namespace AdSage.Concert.Test.Framework
{
    [TestClass]
    public class TestBase
    {
        public TestContext TestContext { get; set; }
        private Dictionary<Type, object> typedCache = new Dictionary<Type, object>();
        private Dictionary<string, Action> cleanupMethods = new Dictionary<string, Action>();
        private string tempMessage;

        //1
        protected TestBase()
        {
            int randomSeed;
            string randomSeedString = ConfigurationManager.AppSettings.Get("RandomSeed");
            if (string.IsNullOrEmpty(randomSeedString))
            {
                randomSeed = (new Random()).Next();
            }
            else
            {
                randomSeed = int.Parse(randomSeedString);
            }
            tempMessage = "Random seed forced to " + randomSeed;
            Random random = new Random(randomSeed);
            typedCache.Add(typeof(Random), random);
        }

        //3
        public virtual void OnTestInitialize()
        {
            if (!string.IsNullOrEmpty(tempMessage))
            {
                TestContext.WriteLine(tempMessage);
            }
        }

        //2
        [TestInitialize]
        public void TestInitialize()
        {
            try
            {
                OnTestInitialize();
            }
            catch
            {
                TestCleanup();
                throw;
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            int cleanupFailures = 0;
            StringBuilder cleanupMessages = new StringBuilder();


            foreach (var cleanup in cleanupMethods)
            {
                DoTestCleanup(cleanup.Key, cleanup.Value, ref cleanupFailures, ref cleanupMessages);
            }

            foreach (var type in typedCache)
            {
                DoTestCleanup(type.Key, type.Value, ref cleanupFailures, ref cleanupMessages);
            }

            TestContext.WriteLine(cleanupMessages.ToString());

            if (cleanupFailures != 0)
            {
                Assert.Fail("CLEANUP FAILED" + Environment.NewLine);
            }
        }

        private void DoTestCleanup(string name, Action action, ref int cleanupFailures, ref StringBuilder cleanupMessages)
        {
            try
            {
                cleanupMessages.Append("Cleanup item \"");
                cleanupMessages.Append(name);
                cleanupMessages.Append("\"");
                cleanupMessages.Append(Environment.NewLine);

                if (action != null)
                {
                    action();
                }
            }
            catch (Exception e)
            {
                cleanupFailures++;
                cleanupMessages.AppendLine("Cleanup failed \"");
                cleanupMessages.AppendLine(e.ToString() + "\"");
                cleanupMessages.AppendLine(Environment.NewLine);
            }
        }

        private void DoTestCleanup(Type type, object value, ref int cleanupFailures, ref StringBuilder cleanupMessages)
        {
            try
            {
                cleanupMessages.Append("Cleanup item \"");
                cleanupMessages.Append(type.ToString());
                cleanupMessages.Append("\"");
                cleanupMessages.Append(Environment.NewLine);

                if (value is TestHelper)
                {
                    ((TestHelper)value).OnTestCleanup();
                    return;
                }
            }
            catch (Exception e)
            {
                cleanupFailures++;
                cleanupMessages.AppendLine("Cleanup failed \"");
                cleanupMessages.AppendLine(e.ToString() + "\"");
                cleanupMessages.AppendLine(Environment.NewLine);
            }
        }

        public void AddTestCleanup(string name, Action cleanup)
        {
            Action getAction;
            if (!this.cleanupMethods.TryGetValue(name, out getAction))
            {
                this.cleanupMethods.Add(name, cleanup);
            }
        }

        public T Get<T>() where T : new()
        {
            object value;

            if (!typedCache.TryGetValue(typeof(T), out value))
            {
                ConstructorInfo constructor = typeof(T).GetConstructor(new Type[] { });
                Assert.IsNotNull(constructor, "Unable to find a constructor for " + typeof(T).Name + ".  Must have a public default constructor.");
                value = constructor.Invoke(new object[] { });
                if (value is TestHelper)
                {
                    ((TestHelper)value).OnHelperCreation(this);
                }
                typedCache.Add(typeof(T), value);
            }

            return (T)value;
        }
    }
}
