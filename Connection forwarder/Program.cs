using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Connection_forwarder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        public static IPAddress GetLocalIP()
        {
            IPHostEntry host;
            IPAddress localIP = IPAddress.Parse("127.0.0.1");
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip;
                }
            }
            return localIP;
        }
        /// <summary>
        //// Int64: ClientID
        /// IP
        /// </summary>
        public static Dictionary<IPEndPoint, Object[]> ConnThreads = new Dictionary<IPEndPoint, Object[]>();
    }
}
