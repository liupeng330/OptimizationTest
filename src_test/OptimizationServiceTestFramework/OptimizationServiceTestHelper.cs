using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using AdSage.Concert.Test.Framework;
using AdSage.Concert.Shared.Cron;
using OptimizationService.Test.Framework.RuleManagementServiceReference;

namespace OptimizationService.Test.Framework
{
    public class OptimizationServiceTestHelper : TestHelper
    {
        public static void InitializeSEMObjectDB()
        {
            string scriptPath = ConfigurationManager.AppSettings["SEMObjectDBInitialzeScript"];
            RunCommandUtilities.ExecuteCommandSync("sqlcmd -U sa -P 123 -S . -i " + scriptPath);
        }

        public static string CreateCronExperssion(RuleSetDefinitionSchedule schedule)
        {
            string experssion = string.Empty;
            DailyCron cron = new DailyCron();
            DateTime startTime = schedule.TimePeriodk__BackingField.StartTimek__BackingField.Value;
            DateTime endTime = schedule.TimePeriodk__BackingField.EndTimek__BackingField== null ? DateTime.MinValue : schedule.TimePeriodk__BackingField.EndTimek__BackingField.Value;
            cron.TimeRange.StartTime = new Time(startTime.Hour, startTime.Minute, startTime.Second);
            if (endTime != DateTime.MinValue)
            {
                cron.TimeRange.EndTime = new Time(endTime.Hour, endTime.Minute, endTime.Second);
            }
            else
            {
                cron.TimeRange.EndTime = Time.Empty;
            }
            cron.TimeRange.Intervals = schedule.TimePeriodk__BackingField.Intervalk__BackingField== null ? 1 : schedule.TimePeriodk__BackingField.Intervalk__BackingField.Value;

            if (schedule.WeekPeriodk__BackingField!= null)
            {
                foreach (DayOfWeek day in schedule.WeekPeriodk__BackingField)
                {
                    cron.DayOfWeeks.Add((CronDayOfWeek)day);
                }
            }
            return cron.ToString();
        }

        public static TimeSpan UTCTimeOffset()
        {
            DateTimeOffset dateTimeOffset = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            return ((DateTimeOffset)dateTimeOffset.LocalDateTime).Offset;
        }

    }
}
