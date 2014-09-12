using System;
using System.IO;

namespace LogstashTimer
{
    //Static cause i'm getting lazy...
    public class TotalBuildTimer
    {
        /// <summary>
        /// This will have the same creation time of BuildCounterFile
        /// But the LastAccessTime will be updated on each counter build
        /// </summary>
        private static string TotalBuildLengthFile
        {
            get { return TimeRecorder.BuildFilename("buildlength", ".counter"); }
        }

        public static void SubmitBuildLength(string buildNumber)
        {
            try
            {
                if (!File.Exists(TotalBuildLengthFile))
                    return;
                var buildStartTime = File.GetCreationTime(TotalBuildLengthFile);
                var buildEndTime = File.GetLastAccessTime(TotalBuildLengthFile);

                //TODO if its 4 minutes, consider it invalid or something???
                TimeRecorder.PublishTotalBuildRecord(buildStartTime, buildEndTime, buildNumber);
            }
            catch (Exception ex)
            {
                //More not doing anything!
                Console.WriteLine(ex.ToString());
            }
        }

        public static void UpdateBuildStart()
        {
            TimeRecorder.CreateOrSetCreationTime(TotalBuildLengthFile);
        }

        public static void UpdateBuildEnd()
        {
            if (File.Exists(TotalBuildLengthFile))
                File.SetLastAccessTime(TotalBuildLengthFile, DateTime.Now);
        }

        public static DateTime? GetLastBuildEnd()
        {
            if (File.Exists(TotalBuildLengthFile))
                return File.GetLastAccessTime(TotalBuildLengthFile);
            return null;
        }
    }
}