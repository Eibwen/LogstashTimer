using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;

namespace LogstashTimer
{
    internal class Program
    {
        //call like: LogstashTimer.exe /start "uship.enums"
        //call like: LogstashTimer.exe /finish "uship.enums"
        private static void Main(string[] args)
        {
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
                        CheckForWatcher();
                        break;
                }
            }
        }

        static Mutex _mutex;

        private static void CheckForWatcher()
        {
            bool result;
            _mutex = new Mutex(true, "uShipTotalBuildTimeWatcher", out result);

            if (!result)
            {
                //Do not run
                Logger.Info("Already waiting, exiting...");
                _mutex.Dispose();
                return;
            }

            Process process = null;

            try
            {
                var runningLocation = Process.GetCurrentProcess().MainModule.FileName;
                if (!runningLocation.StartsWith(Path.GetTempPath()))
                {
                    var destFolder = TimeRecorder.BuildFilename("", "");
                    try
                    {
                        foreach (var dllFile in Directory.EnumerateFiles(Path.GetDirectoryName(runningLocation))
                                                 .Where(x => x.EndsWith("exe")
                                                             || x.EndsWith("dll")
                                                             || x.EndsWith("config")))
                        {
                            File.Copy(dllFile, Path.Combine(destFolder, Path.GetFileName(dllFile)), true);
                        }
                    }
                    catch (Exception)
                    {
                        //Failed copying... assume its either running or some other bad error
                        return;
                    }

                    //run that exe
                    process = new Process();
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(destFolder, Path.GetFileName(runningLocation)),
                        Arguments = "/totalWatcher",
                        //RedirectStandardOutput = true,
                        //UseShellExecute = false,
                        //CreateNoWindow = true
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                }
                else
                {
                    StartWatching();
                }
            }
            finally
            {
                //Being explicit about releasing...
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }

            if (process != null)
            {
                Logger.Info("Starting exe from temp");
                process.Start();
            }
        }

        private static void StartWatching()
        {
            //Error possiblities
            var nullCount = 0;
            var absoluteQuittingTime = DateTime.Now.AddMinutes(10);
            var finishedTimeout = TimeSpan.FromSeconds(45);

            while (true)
            {
                Logger.Info("Checking for non-updated last build time...");
                var lastEnd = TotalBuildTimer.GetLastBuildEnd();
                if (lastEnd.HasValue)
                {
                    if (lastEnd.Value < (DateTime.Now.Subtract(finishedTimeout)))
                    {
                        //Consider build end
                        var counter = new BuildCounter();
                        var buildNumber = counter.GetIncrementingBuildVersion(false, false);

                        Logger.DisplayMessage("Submitting!:" + lastEnd.Value);

                        TotalBuildTimer.SubmitBuildLength(buildNumber);
                        return;
                    }
                }
                else
                {
                    ++nullCount;
                }
                if (DateTime.Now > absoluteQuittingTime
                    || nullCount >= 5)
                {
                    //Consider this some error
                    return;
                }

                Thread.Sleep(TimeSpan.FromSeconds(15));
            }
        }

        private static void LogEvent(EventType @event, IList<string> args)
        {
            if (args.Count < 2)
                return;

            var label = args[1];

            switch (@event)
            {
                case EventType.Start:
                    TimeRecorder.RecordStart(label);
                    //Make sure it doesn't submit the previous record:
                    TotalBuildTimer.UpdateBuildEnd();
                    CheckForWatcher();
                    break;
                case EventType.Finish:
                    TimeRecorder.PublishRecord(label);
                    break;
            }
        }
    }
}
