using System;
using System.IO;
using Newtonsoft.Json;

namespace LogstashTimer
{
    public class TimeRecorder
    {
        static TimeRecorder()
        {
            JsonConvert.DefaultSettings =
                () => new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    };
        }

        public static void RecordStart(string label)
        {
            try
            {
                if (!Directory.Exists(FileStorage))
                {
                    Logger.DisplayMessage("Creating: {0}", FileStorage);
                    Directory.CreateDirectory(FileStorage);
                }

                var filename = BuildFilename(label);
                CreateOrSetCreationTime(filename);
            }
            catch (Exception ex)
            {
                //Do nothing... this won't work for this person, sorry
                Logger.Error(ex);
            }
        }

        internal static void CreateOrSetCreationTime(string filename)
        {
            //Really Create does not fail if it exists, but figure setting the creation time is quicker?
            if (!File.Exists(filename))
            {
                using (File.Create(filename))
                {
                }
            }
            else
            {
                File.SetCreationTime(filename, DateTime.Now);
            }
        }

        private static string FileStorage
        {
            get { return Path.Combine(Path.GetTempPath(), "BuildTimeTracking"); }
        }

        public static string BuildFilename(string label, string append = ".timer")
        {
            var filename = Path.Combine(FileStorage, label + append);
            return filename;
        }

        private static DateTime? FindStart(string label, bool deleteRecord = true)
        {
            try
            {
                var filename = BuildFilename(label);

                if (!File.Exists(filename))
                    return null;

                var time = File.GetCreationTime(filename);

                if (deleteRecord)
                    File.Delete(filename);

                return time;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        public static Lazy<ILogSender> _logSender = new Lazy<ILogSender>(
            () => new LogSender(Settings.LogstashHostname, Settings.LogstashPort));

        public static void PublishRecord(string label)
        {
            var startTime = FindStart(label);

            var builder = new TimerRecordBuilder(new SourceControlInfo(), new BuildCounter());

            var record = builder
                .WithProjectName(label)
                .WithStartTime(startTime)
                .WithFinishTime(DateTime.Now)
                .WithMachineName()
                .WithUserName()
                .WithTrunkPath()
                .WithSourceControlInfo()
                .WithLocalBuildNumber()
                .Build();

            var recordJson = JsonConvert.SerializeObject(record);
            _logSender.Value.SendString(recordJson);
        }

        public static void PublishTotalBuildRecord(DateTime startTime, DateTime endTime, string buildNumber)
        {
            var builder = new TimerRecordBuilder(new SourceControlInfo(), new BuildCounter());

            var record = builder
                .WithProjectName("Total_Build_Time")
                .WithStartTime(startTime)
                .WithFinishTime(endTime)
                .WithMachineName()
                .WithUserName()
                .WithTrunkPath()
                .WithLocalBuildNumber(buildNumber)
                .Build();

            //TODO validate elsewhere
            if ((Settings.TotalBuildTimeValidMin != null
                    && record.ElapsedTime < Settings.TotalBuildTimeValidMin)
                || (Settings.TotalBuildTimeValidMax != null
                    && record.ElapsedTime > Settings.TotalBuildTimeValidMax))
                return;

            var recordJson = JsonConvert.SerializeObject(record);
            _logSender.Value.SendString(recordJson);
        }
    }
}