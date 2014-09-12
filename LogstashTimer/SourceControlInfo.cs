using System;
using System.Diagnostics;

namespace LogstashTimer
{
    public interface ISourceControlInfo
    {
        string GetCurrentPublicTip();
        string GetCurrentTip();
    }

    public class SourceControlInfo : ISourceControlInfo
    {
        public string GetCurrentPublicTip()
        {
            //return RunHgCommand(@"id --num -i -r ""last(public())""");
            return RunHgCommand(@"log -r ""last(public())"" --template=""{rev} - {node|short} - {author|person}""");
        }

        public string GetCurrentTip()
        {
            //return RunHgCommand(@"id --num -i -r tip");
            return RunHgCommand(@"log -r tip --template=""{rev} - {node|short} - {author|person}""");
        }

        private string RunHgCommand(string arguments)
        {
            try
            {
                return GetProcessOutput("hg", arguments);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        private static string GetProcessOutput(string processFilepath, string arguments)
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo
                {
                    FileName = processFilepath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            process.Start();
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
        }
    }
}