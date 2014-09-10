using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LogstashTimer
{
    internal class Program
    {
        //call like: LogstashTimer.exe /start "uship.enums"
        //call like: LogstashTimer.exe /finish "uship.enums"
        private static void Main(string[] args)
        {
            if (args.Length >= 2)
            {
                var logType = args[0];

                switch (logType)
                {
                    case "/start":
                        LogEvent(EventType.Start, args);
                        break;
                    case "/finish":
                    case "/end":
                        LogEvent(EventType.Finish, args);
                        break;
                }
            }
        }

        private static void LogEvent(EventType @event, IList<string> args)
        {
            var label = args[1];

            switch (@event)
            {
                case EventType.Start:
                    RecordStart(label);
                    break;
                case EventType.Finish:
                    PublishRecord(label);
                    break;
            }
        }

        private static void RecordStart(string label)
        {
            try
            {
                if (!Directory.Exists(FileStorage))
                {
                    Console.WriteLine("Creating: {0}", FileStorage);
                    Directory.CreateDirectory(FileStorage);
                }

                var filename = Path.Combine(FileStorage, label);
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

        private static DateTime? FindStart(string label, bool deleteRecord = true)
        {
            try
            {
                var filename = Path.Combine(FileStorage, label);

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

        private static void PublishRecord(string label)
        {
            var startTime = FindStart(label);

            var builder = new TimerRecordBuilder(new SourceControlInfo());

            var record = builder
                .WithProjectName(label)
                .WithStartTime(startTime)
                .WithFinishTime(DateTime.Now)
                .WithMachineName()
                .WithUserName()
                .WithTrunkPath()
                .WithSourceControlInfo()
                .Build();

            var recordJson = JsonConvert.SerializeObject(record);
            _logSender.Value.SendString(recordJson);
        }
    }
}
