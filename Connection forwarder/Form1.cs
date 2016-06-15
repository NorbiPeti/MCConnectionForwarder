using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Connection_forwarder
{
    public partial class Form1 : Form
    {
        public static int PortNumber;
        public static int PortNumber2;
        //public static bool PEVersion;
        //public static List<Thread> Threads = new List<Thread>();

        public Form1()
        {
            InitializeComponent();
            //Threads.Add(Thread.CurrentThread);
        }

        private void startbtn_Click(object sender, EventArgs e)
        {
            var s = new Socket(SocketType.Stream, ProtocolType.Tcp);
            s.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Int32.Parse(portBox.Text)));
            s.Listen(2);
            s = s.Accept();
            var a = new NetworkStream(s);
            byte[] buffer = new byte[512];
            a.Read(buffer, 0, 512);
            MessageBox.Show("Contents:\n" + buffer.ToString());
            s.Close();
            MessageBox.Show(Encoding.ASCII.GetString(buffer));
        }

        private void startforwardbtn_Click(object sender, EventArgs e)
        {
            PortNumber = Int32.Parse(portBox.Text);
            PortNumber2 = Int32.Parse(portBox2.Text);
            //PEVersion = radioButton2.Checked;
            new Thread(new ThreadStart(ListenThread.Run)).Start();
            startforwardbtn.Enabled = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
