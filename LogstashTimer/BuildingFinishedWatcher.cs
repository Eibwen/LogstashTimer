using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace LogstashTimer
{
    public static class BuildingFinishedWatcher
    {
        static Mutex _mutex;

        public static void CheckForWatcher()
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
            //TODO For this to work, CheckForWatcher() needs to be the very first thing called
            TotalBuildTimer.UpdateBuildStart();

            //Error possiblities
            var nullCount = 0;
            var absoluteQuittingTime = DateTime.Now.AddMinutes(10);
            var finishedTimeout = Settings.TotalBuildTimeTimeout;

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
    }
}