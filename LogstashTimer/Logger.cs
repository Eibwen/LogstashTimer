using System;

namespace LogstashTimer
{
    public class Logger
    {
        public static void DisplayMessage(string format, params object[] args)
        {
            Console.WriteLine("LogstashTimer: " + format, args);
        }
        public static void Info(string msg)
        {
            DisplayMessage(msg);
        }
        public static void Error(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex);
        }
        public static void CollectErrorInformation(Exception ex, string message)
        {
            TimeRecorder.PublishException(ex, message);
        }
    }
}