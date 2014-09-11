using System;
using System.IO;
using Newtonsoft.Json;

namespace LogstashTimer
{
    public class TimeRecorder
    {
        public static void RecordStart(string label)
        {
            try
            {
                if (!Directory.Exists(FileStorage))
                {
                    Console.WriteLine("Creating: {0}", FileStorage);
                    Directory.CreateDirectory(FileStorage);
                }

                var filename = BuildFilename(label);
                //Really Create does not fail if it exists, but figure setting the creation time is quicker?
                if (!File.Exists(filename))
                {
                    using (File.Create(filename))
                    { }
                }
                else
                {
                    File.SetCreationTime(filename, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                //Do nothing... this won't work for this person, sorry
                Console.WriteLine(ex.ToString());
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
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static Lazy<ILogSender> _logSender = new Lazy<ILogSender>(() => new LogSender());

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
    }
}