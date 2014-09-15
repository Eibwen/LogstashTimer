using System;
using LogstashTimer.Extensions;

namespace LogstashTimer
{
    public static class Settings
    {
        private static Properties.Settings SettingsClass
        {
            get { return Properties.Settings.Default; }
        }


        /// <summary>
        /// This is to prevent bugs/weird cases, set to 0 to get all build time reports
        /// </summary>
        public static TimeSpan? TotalBuildTimeValidMin
        {
            get { return SettingsClass.TotalBuildTimeValidMin.GetTimespan(); }
        }

        /// <summary>
        /// This is to prevent bugs/weird cases, set to 0 to get all build time reports
        /// </summary>
        public static TimeSpan? TotalBuildTimeValidMax
        {
            get { return SettingsClass.TotalBuildTimeValidMax.GetTimespan(); }
        }

        /// <summary>
        /// This is the amount of time to wait after the last build occurs to consider the build done
        ///   If you have 5 projects, and one takes 30 seconds to build, put this higher than 30 seconds
        /// </summary>
        public static TimeSpan TotalBuildTimeTimeout
        {
            get { return SettingsClass.TotalBuildTimeTimeout.GetTimespan() ?? TimeSpan.FromSeconds(60); }
        }


        public static string LogstashHostname
        {
            get { return SettingsClass.LogstashHostname.IsNullOrDefault("logging-dev"); }
        }

        public static int LogstashPort
        {
            get { return SettingsClass.LogstashPort.IsNullOrDefault(9995); }
        }
    }
}