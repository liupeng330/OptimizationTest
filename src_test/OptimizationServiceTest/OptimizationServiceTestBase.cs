using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdSage.Concert.Test.Framework;
using OptimizationService.Test.Framework;

namespace OptimizationService.Test
{
    public class OptimizationServiceTestBase : TestBase
    {
        protected OptimizationServiceTestHelper OptimizationServiceTestHelper;
        protected RandomData RandomData;

        public override void OnTestInitialize()
        {
            base.OnTestInitialize();
            OptimizationServiceTestHelper = Get<OptimizationServiceTestHelper>();
            RandomData = Get<RandomData>();
        }
    }
}
