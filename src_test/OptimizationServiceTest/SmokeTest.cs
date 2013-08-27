using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptimizationService.Test.Framework.RuleManagementServiceReference;
using AdSage.Concert.Test.Framework;

namespace OptimizationService.Test
{
    [TestClass]
    public class SmokeTest
    {
        [TestMethod]
        [Priority(0)]
        [Owner("LiuPeng")]
        [Description("Verify rule management service deployment")]
        public void RuleManagementSmokeTest()
        {
            WCFHelper.Using<RuleManagementServiceClient>(new RuleManagementServiceClient(), client =>
            {
                client.GetRuleSets(0);
            });
        }
    }
}
