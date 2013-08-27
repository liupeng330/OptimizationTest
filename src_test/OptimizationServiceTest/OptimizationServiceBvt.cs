using System;
using System.Workflow.Activities.Rules;
using AdSage.Concert.SEMObjects;
using AdSage.Concert.Test.Framework;
using AFP.Hosting.Optimization.Application;
using AFP.Hosting.Optimization.Application.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptimizationService.Test.Framework;
using OptimizationService.Test.Framework.RuleManagementServiceReference;

namespace OptimizationService.Test
{
    [TestClass]
    public class OptimizationServiceBvt : OptimizationServiceTestBase
    {
        [TestMethod]
        public void TestCreateAutoRuleSet()
        {
            OptimizationServiceTestHelper.InitializeSEMObjectDB();
            Guid userId = RandomData.NextGuid();
            long advertiserId = 10040;
            long campaignObjectId = 6004249897041;
            long accountId = 46848300;

            //Create AutomationRuleSet object
            AutomationRuleSet automationRuleSet = new AutomationRuleSet();
            automationRuleSet.JobID = 0;
            automationRuleSet.OptimizationLevel = (int)SEMObjectDetailType.FacebookAdGroup;
            automationRuleSet.RuleSetID = 0;
            automationRuleSet.UserID = userId.ToString();
            automationRuleSet.AdvertiserID = advertiserId;
            automationRuleSet.RotationId = 0;
            automationRuleSet.Today = new DateTime();
            automationRuleSet.AutomationType = (int)SEMObjectDetailType.FacebookCampaign;
            automationRuleSet.ObjectIDs = new long[] { campaignObjectId };
            automationRuleSet.AccountIDs = new long[] { accountId };

            //Creating AutomationRuleSet.Rules
            RuleComponent ruleComponent1 = new RuleComponent();
            ruleComponent1.IsValidated = false;
            ruleComponent1.Name = "0";
            ruleComponent1.Active = true;
            ruleComponent1.Priority = 0;
            ruleComponent1.ReevaluationBehavior = RuleReevaluationBehavior.Always;

            #region RuleComponent.Condition
            //Creating RuleComponent.Condition
            SimpleExpression simpleExpression = new SimpleExpression();
            simpleExpression.Left = "Clicks";
            simpleExpression.Operator = Operator.Less;
            simpleExpression.Right = (Int64)1;

            PerformanceCondition performanceCondition = new AvgPerformanceCondition();
            performanceCondition.Description = string.Empty;
            performanceCondition.Name = "0";
            performanceCondition.OPTType = (int)SEMObjectDetailType.FacebookAdGroup;
            performanceCondition.PerformanceDays = 1;
            performanceCondition.SimpleExpr = simpleExpression;

            AndConditionComponent andConditionComponent = new AndConditionComponent();
            andConditionComponent.Conditions.Add(performanceCondition);

            OrConditionComponent orConditionComponent = new OrConditionComponent();
            orConditionComponent.Conditions.Add(andConditionComponent);

            ruleComponent1.Condition = new AutomationConditionSet
            {
                Description = string.Empty,
                Name = string.Empty,
                CondSet = orConditionComponent,
            };
            #endregion

            #region RuleComponent.ThenActions
            //Creating RuleComponent.ThenActions
            SimpleExpression simpleExpressionForAction1 = new SimpleExpression();
            simpleExpressionForAction1.Left = "Status";
            simpleExpressionForAction1.Operator = Operator.Equal;
            simpleExpressionForAction1.Right = (Int32)1;

            ActionWithProtect action1 = new ActionWithProtect();
            action1.Name = "0";
            action1.OPTType = (int)SEMObjectDetailType.FacebookAdGroup;

            //If IsNeedConfirm is true, then rule engine will not call hosting service, will insert data into cache db.
            //If IsNeedConfirm is false, then rule enginel will call hosting service, and will insert result data into summary db and cache db.
            action1.IsNeedConfirm = false;
            action1.SimpleExpr = simpleExpressionForAction1;

            SimpleExpression simpleExpressionForAction2 = new SimpleExpression();
            simpleExpressionForAction2.Left = "MaxBid";
            simpleExpressionForAction2.Operator = Operator.PlusEqual;
            simpleExpressionForAction2.Right = (Int32)1;

            ActionWithProtect action2 = new ActionWithProtect();
            action2.Name = "1";
            action2.OPTType = (int)SEMObjectDetailType.FacebookAdGroup;
            action2.IsNeedConfirm = false;
            action2.SimpleExpr = simpleExpressionForAction2;

            ruleComponent1.ThenActions.Add(action1);
            ruleComponent1.ThenActions.Add(action2);
            automationRuleSet.Rules.Add(ruleComponent1); 
            #endregion

            //Creating RuleSetDefinition 
            RuleSetDefinition ruleSetDefinition = new RuleSetDefinition();
            ruleSetDefinition.Description = RandomData.NextEnglishWordLowercase(10);
            ruleSetDefinition.LastRun = new DateTime();
            ruleSetDefinition.ModifiedTime = new DateTime();
            ruleSetDefinition.Name = RandomData.NextEnglishWordLowercase(10);
            ruleSetDefinition.RulesetID = 0;
            ruleSetDefinition.RuleSetStatus = 0;
            ruleSetDefinition.RuleType = 0;//RuleSetTypes.AutoOptimization = 0 
            ruleSetDefinition.ValidFrom = DateTime.Now;
            ruleSetDefinition.Content = RuleEngineUtils.SerializeAutomationRuleSet<AutomationRuleSet>(automationRuleSet);

            #region RuleSetParameter
            //Setting RuleSetParameter

            //Setting Schedule Parameter
            RuleSetParameter scheduleParameter = new RuleSetParameter();
            scheduleParameter.Name = "Schedule";
            scheduleParameter.Value = new RuleSetDefinitionSchedule
            {
                IsRunOncek__BackingField=false,
                RunPeriodTypek__BackingField = RuleSetDefinitionRunPeriodType.Day,
                TimePeriodk__BackingField= new RuleSetDefinitionRunTimePeriod
                {
                    StartTimek__BackingField= DateTime.Now + new TimeSpan(0, 1, 0),
                },
                WeekPeriodk__BackingField= new DayOfWeek[] { DateTime.Now.DayOfWeek },
            };

            //Setting Enable Parameter
            RuleSetParameter enableParameter = new RuleSetParameter();
            enableParameter.Name = "Enable";
            enableParameter.Value = 1;

            //Setting ObjectCount Parameter
            RuleSetParameter objectsCountParameter = new RuleSetParameter();
            objectsCountParameter.Name = "ObjectsCount";
            objectsCountParameter.Value = 1;

            //Setting ObjectDetailType Parameter
            RuleSetParameter objectDetailTypeParameter = new RuleSetParameter();
            objectDetailTypeParameter.Name = "ObjectDetailType";
            objectDetailTypeParameter.Value = (int)SEMObjectDetailType.FacebookCampaign;

            //Setting AccountId Parameter
            RuleSetParameter accountIdParameter = new RuleSetParameter();
            accountIdParameter.Name = "AccountIDs";
            accountIdParameter.Value = new long[] { accountId };

            //Setting IsRuleOptimization Parameter
            RuleSetParameter isRuleOptimizationParameter = new RuleSetParameter();
            isRuleOptimizationParameter.Name = "IsRuleOptimization";
            isRuleOptimizationParameter.Value = true;

            //Setting CronExpression Parameter
            RuleSetParameter cronExpressionParameter = new RuleSetParameter();
            cronExpressionParameter.Name = "CronExpression";
            cronExpressionParameter.Value = OptimizationServiceTestHelper.CreateCronExperssion((RuleSetDefinitionSchedule)scheduleParameter.Value);

            //Setting TimeZone Parameter
            RuleSetParameter timeZoneParameter = new RuleSetParameter();
            timeZoneParameter.Name = "TimeZone";
            timeZoneParameter.Value = OptimizationServiceTestHelper.UTCTimeOffset().ToString();

            ruleSetDefinition.Parameters = new RuleSetParameter[] { scheduleParameter, enableParameter, objectsCountParameter, objectDetailTypeParameter, accountIdParameter, isRuleOptimizationParameter, cronExpressionParameter, timeZoneParameter }; 
            #endregion

            WCFHelper.Using<RuleManagementServiceClient>(new RuleManagementServiceClient(), client =>
                {
                    client.CreateAutoRuleSet(userId.ToString(), advertiserId, ruleSetDefinition);
                });
        }
    }
}
