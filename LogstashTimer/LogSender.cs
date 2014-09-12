using System;
using System.Net.Sockets;
using System.Text;

namespace LogstashTimer
{
    public interface ILogSender
    {
        void SendString(string message);
    }

    public class LogSender : ILogSender
    {
        private readonly string _hostname;
        private readonly int _port;

        public LogSender()
        {
            _hostname = "logging-dev";
            _port = 9995;
        }

        public LogSender(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        public void SendString(string message)
        {
            var udpClient = new UdpClient();
            try
            {
                udpClient.Connect(_hostname, _port);

                var messageBytes = Encoding.UTF8.GetBytes(message);

                udpClient.Send(messageBytes, messageBytes.Length);
            }
            catch (Exception)
            {
                // uh... we don't have a backup for our backup yet.
                Logger.Info("Something really bad is happening when udp fails");
            }
            finally
            {
                udpClient.Close();
            }
        }
    }
}