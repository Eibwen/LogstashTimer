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
        private static string PreviousBuildLengthFile
        {
            get { return TimeRecorder.BuildFilename("buildlength", ".counter"); }
        }

        public static void SubmitBuildLength(string buildNumber)
        {
            try
            {
                if (!File.Exists(PreviousBuildLengthFile))
                    return;
                var buildStartTime = File.GetCreationTime(PreviousBuildLengthFile);
                var buildEndTime = File.GetLastAccessTime(PreviousBuildLengthFile);

                //TODO if its 4 minutes, consider it invalid or something???
                TimeRecorder.PublishPreviousBuildRecord(buildStartTime, buildEndTime, buildNumber);
            }
            catch (Exception)
            {
                //More not doing anything!
            }
        }

        public static void UpdateBuildStart()
        {
            TimeRecorder.CreateOrSetCreationTime(PreviousBuildLengthFile);
        }
        public static void UpdateBuildEnd()
        {
            File.SetLastAccessTime(PreviousBuildLengthFile, DateTime.Now);
        }
    }
}