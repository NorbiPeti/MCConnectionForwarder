using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Cyotek.Data.Nbt;
using System.Threading;

namespace Connection_forwarder
{
    class ClientConnectionThread
    {
        public void Run(object sckt)
        {
            Thread.CurrentThread.Name = "ConnectionThread";
            Form1.Threads.Add(Thread.CurrentThread);
            //var connsocket = ((Socket[])sckt)[0];
            object connsocket;
            if (!Form1.PEVersion)
                connsocket = ((object[])sckt)[0] as Socket;
            else
                connsocket = ((UdpClient)((object[])sckt)[0]);
            //var serversock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //serversock.Connect("localhost", Form1.PortNumber);
            //while (connsocket.Available > 0)
            var serversock = ((Socket)((object[])sckt)[1]);
            //new NetworkStream(connsocket).CopyToAsync(new NetworkStream(serversock));
            //new NetworkStream(serversock).CopyTo(new NetworkStream(connsocket));
            if (!Form1.PEVersion)
            {
                try
                {
                    new NetworkStream(connsocket as Socket).CopyTo(new NetworkStream(serversock));
                }
                catch { }
                (connsocket as Socket).Close();
                serversock.Close();
            }
            else
            {
                
            }
        }
    }
    class ServerConnectionThread
    {
        public void Run(object sckt)
        {
            Thread.CurrentThread.Name = "ConnectionThread";
            Form1.Threads.Add(Thread.CurrentThread);
            object connsocket;
            if (!Form1.PEVersion)
                connsocket = ((object[])sckt)[0] as Socket;
            else
                connsocket = ((UdpClient)((object[])sckt)[0]);
            //var serversock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //serversock.Connect("localhost", Form1.PortNumber);
            //while (connsocket.Available > 0)
            var serversock = ((Socket)((object[])sckt)[1]);
            //new NetworkStream(connsocket).CopyToAsync(new NetworkStream(serversock));
            try
            {
                if (!Form1.PEVersion)
                    new NetworkStream(serversock).CopyTo(new NetworkStream(connsocket as Socket));
                
                /*var nsc = new NetworkStream(connsocket);
                var nss = new NetworkStream(serversock);
                var tr = new BinaryTagReader(nss, NbtOptions.None);
                var tw = new BinaryTagWriter(nsc, NbtOptions.None);
                byte[] ba;
                while (!tr.ReadString().Contains("Blocks"))
                {
                    nss.CopyTo(nsc);
                }
                ba = tr.ReadByteArray();
                Console.WriteLine("Byte array: " + ba);
                for (int i = 0; i < ba.Length; i++)
                    tw.Write(ba[i]);*/
            }
            catch
            {

            }
            (connsocket as Socket).Close();
            serversock.Close();
        }
    }
}
