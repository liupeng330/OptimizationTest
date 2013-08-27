using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdSage.Concert.Test.Framework
{
    public class TestHelper
    {
        public TestBase Test { get; private set; }

        public virtual void OnHelperCreation(TestBase test)
        {
            this.Test = test;
        }

        public virtual void OnTestCleanup() { }
    }
}
