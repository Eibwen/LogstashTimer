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
                    TimeRecorder.RecordStart(label);
                    break;
                case EventType.Finish:
                    TimeRecorder.PublishRecord(label);
                    break;
            }
        }
    }
}
