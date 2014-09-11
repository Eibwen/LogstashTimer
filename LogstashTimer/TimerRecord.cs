using System;

namespace LogstashTimer
{
    public class TimerRecord
    {
        internal TimerRecord()
        {
        }

        public string MachineName { get; set; }
        public string ProjectName { get; set; }
        public string UserName { get; set; }
        public string TrunkPath { get; set; }

        ///Have this be Author, hash, number
        public string CurrentTrunkLocalTip { get; set; }
        public string CurrentTrunkPublicTip { get; set; }
        public string CurrentTrunkPublicTipRev { get; set; }

        public string LocalBuildNumber { get; set; }

        public DateTime DateRecorded { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public double? ElapsedSeconds { get { return ElapsedTime.HasValue ? ElapsedTime.Value.TotalSeconds : (double?)null; } }
    }
    //public class TimerRecord
    //{
    //    public string MachineName { get; set; }
    //    public string ProjectName { get; set; }
    //    public string UserName { get; set; }
    //    public string TrunkPath { get; set; }
    //    public string DateRecorded { get; set; }

    //    public string StartTime { get; set; }
    //    public string FinishTime { get; set; }
    //    public string ElapsedTime { get; set; }
    //}
}