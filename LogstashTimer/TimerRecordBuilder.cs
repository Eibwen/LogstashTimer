using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LogstashTimer
{
    public interface ITimerRecordBuilder
    {
        TimerRecordBuilder WithMachineName();
        TimerRecordBuilder WithProjectName(string label);
        TimerRecordBuilder WithUserName();
        TimerRecordBuilder WithTrunkPath();
        TimerRecordBuilder WithStartTime(DateTime? start);
        TimerRecordBuilder WithFinishTime(DateTime? finish);
        TimerRecordBuilder WithSourceControlInfo();
        TimerRecordBuilder WithLocalBuildNumber();
        TimerRecord Build();
    }

    public class TimerRecordBuilder : ITimerRecordBuilder
    {
        private readonly ISourceControlInfo _sourceControlInfo;
        private readonly BuildCounter _buildCounter;
        private readonly TimerRecord _record = new TimerRecord();

        public TimerRecordBuilder(ISourceControlInfo sourceControlInfo, BuildCounter buildCounter)
        {
            _sourceControlInfo = sourceControlInfo;
            _buildCounter = buildCounter;
        }

        public TimerRecordBuilder WithMachineName()
        {
            _record.MachineName = Environment.MachineName;
            return this;
        }

        public TimerRecordBuilder WithProjectName(string label)
        {
            _record.ProjectName = label;
            return this;
        }

        public TimerRecordBuilder WithUserName()
        {
            //            _localVHostName = "local";
            //            var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
            //            if (windowsIdentity != null)
            //            {
            //                var userName = windowsIdentity.Name;
            //                _localVHostName = "local_" + userName.Substring(userName.IndexOf("\\", StringComparison.Ordinal) + 1);
            //            }
            //            return _localVHostName;

            _record.UserName = Environment.UserName;
            return this;
        }

        public TimerRecordBuilder WithTrunkPath()
        {
            try
            {
                //Assuming this assembly is 2 levels deep
                var trunkPath = Path.GetDirectoryName(Path.GetDirectoryName(AssemblyDirectory));
                _record.TrunkPath = Path.GetFileName(trunkPath);
            }
            catch (Exception)
            {
            }
            return this;
        }

        private static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public TimerRecordBuilder WithStartTime(DateTime? start)
        {
            _record.StartTime = start;
            return WithElapsedTime();
        }

        public TimerRecordBuilder WithFinishTime(DateTime? finish)
        {
            _record.FinishTime = finish;
            return WithElapsedTime();
        }

        private TimerRecordBuilder WithElapsedTime()
        {
            if (_record.StartTime.HasValue
                && _record.FinishTime.HasValue)
            {
                _record.ElapsedTime = _record.FinishTime - _record.StartTime;
            }
            return this;
        }

        public TimerRecordBuilder WithSourceControlInfo()
        {
            _record.CurrentTrunkLocalTip = _sourceControlInfo.GetCurrentTip();

            var publicTip = _sourceControlInfo.GetCurrentPublicTip();
            _record.CurrentTrunkPublicTip = publicTip;

            var tipParts = publicTip.Split('-');
            if (tipParts.Length > 0)
                _record.CurrentTrunkPublicTipRev = tipParts.First().Trim();

            return this;
        }

        public TimerRecordBuilder WithLocalBuildNumber()
        {
            _record.LocalBuildNumber = _buildCounter.GetIncrementingBuildVersion();
            return this;
        }

        public TimerRecord Build()
        {
            _record.DateRecorded = DateTime.Now;
            return _record;
        }
    }
}