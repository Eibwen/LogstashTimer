using System;
using System.IO;
using System.Threading;

namespace LogstashTimer
{
    public class BuildCounter
    {
        private const int LongestBuildLengthMinutes = 5;
        private string BuildCounterFile
        {
            get { return TimeRecorder.BuildFilename("buildcounter", ".counter"); }
        }

        private void IncrementFile()
        {
            try
            {
                var version = 10000;
                using (var fs = File.Open(BuildCounterFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                using (var sw = new StreamWriter(fs))
                {
                    var sr = new StreamReader(fs);
                    var firstLine = sr.ReadLine();

                    fs.Position = 0;

                    var nextVersion = version;

                    if (!string.IsNullOrWhiteSpace(firstLine))
                    {
                        if (int.TryParse(firstLine, out version))
                        {
                            nextVersion = version + 1;
                        }
                    }
                    sw.WriteLine(nextVersion);
                }

                ////Total build timer, relying on the check for this to work correctly
                //TotalBuildTimer.SubmitBuildLength(version.ToString());
                TotalBuildTimer.UpdateBuildStart();
            }
            catch (Exception ex)
            {
                //Do nothing
                Console.WriteLine(ex.ToString());
            }
        }

        public string GetIncrementingBuildVersion(bool retry = true, bool allowIncrementing = true)
        {
            try
            {
                if (allowIncrementing)
                {
                    //var fileDate = File.GetLastWriteTime(BuildCounterFile);
                    var fileDate = File.Exists(BuildCounterFile)
                                       ? File.GetLastWriteTime(BuildCounterFile)
                                       : DateTime.MinValue;
                    if (fileDate < DateTime.Now.AddMinutes(-LongestBuildLengthMinutes))
                    {
                        //If older than 5 minutes, increment the number
                        //  Assuming the total build will never take longer than this number...
                        IncrementFile();
                    }
                }

                var lines = File.Exists(BuildCounterFile)
                                ? File.ReadAllLines(BuildCounterFile)
                                : new[] {""};
                return lines[0];
            }
            catch (Exception)
            {
                //retry once
                if (retry)
                {
                    Thread.Sleep(new Random().Next(5, 30));
                    return GetIncrementingBuildVersion(false);
                }
                return "error";
            }
        }
    }
}