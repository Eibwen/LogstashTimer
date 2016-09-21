using System;
using System.Collections.Generic;

namespace LogstashTimer
{
    internal class Program
    {
        //call like: LogstashTimer.exe /start "uship.enums"
        //call like: LogstashTimer.exe /finish "uship.enums"
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            if (args.Length >= 1)
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
                    case "/totalWatcher":
                        BuildingFinishedWatcher.CheckForWatcher();
                        break;
                }
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Logger.CollectErrorInformation(unhandledExceptionEventArgs.ExceptionObject as Exception, "Unhandled Exception!");
        }


        private static void LogEvent(EventType @event, IList<string> args)
        {
            if (args.Count < 2)
                return;

            var label = args[1];

            switch (@event)
            {
                case EventType.Start:
                    BuildingFinishedWatcher.CheckForWatcher();
                    //For build log to be accurate, Watcher needs to be running
                    //  Yay adding more possibilities for race conditions...
                    ////BuildLog.AddStartEntry(label);
                    TimeRecorder.RecordStart(label);
                    //Make sure it doesn't submit the previous record:
                    TotalBuildTimer.UpdateBuildEnd();
                    break;
                case EventType.Finish:
                    TimeRecorder.PublishRecord(label);
                    TotalBuildTimer.UpdateBuildEnd();
                    break;
            }
        }
    }
}
