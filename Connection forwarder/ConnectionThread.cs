using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Cyotek.Data.Nbt;
using System.Threading;
using System.Net;

namespace Connection_forwarder
{
    class ClientConnectionThread
    {
        public Int64 ClientID;
        public bool DataChanged = false;
        public byte[] DataBuffer;
        public void Run(object obj)
        {
            Thread.CurrentThread.Name = "ClientConnectionThread";
            //Form1.Threads.Add(Thread.CurrentThread);
            //IPEndPoint RemoteIP = (IPEndPoint)remip;
            IPEndPoint RemoteIP = (IPEndPoint)((object[])obj)[0];
            UdpClient connection = (UdpClient)((object[])obj)[1];
            ClientID = (Int64)((object[])obj)[2];
            //Program.ConnThreads.Add(ClientID, new object[] { Thread.CurrentThread, this }); //Remove it over time
            Program.ConnThreads.Add(RemoteIP, new object[] { Thread.CurrentThread, this }); //Remove it over time
            //UdpClient listenclient;
            //byte[] buffer;
            //bool brvar = false;
            //UdpClient sendsocket;
            var lasttick = Environment.TickCount;
            while (Environment.TickCount - lasttick < 60 * 1000)
            {
                if (DataChanged)
                { //Now DataBuffer contains the received packet
                    Console.WriteLine("Data changed...");
                    Console.WriteLine("PacketID: 0x{0:X}", DataBuffer[0]);
                    lasttick = Environment.TickCount;
                    DataChanged = false; //Finished working
                }
                /*do
                {
                    //new PEPackets(PEPackets.TOCLIENT_LoginStatusPacket, new object[] { 0 }, sendsocket, RemoteIP);

                    //connsocket[1] = new Socket(SocketType.Stream, ProtocolType.Tcp); //We still need to connect to the server
                    //((Socket)connsocket[1]).Connect("localhost", Form1.PortNumber);

                    //After login loop to receive all messages
                } while (false);*/
            }
            Program.ConnThreads.Remove(RemoteIP);
        }
        /*private bool done = false;
        void saea_Completed(object sender, SocketAsyncEventArgs e)
        {
            done = true;
        }*/
        public void SendData(object data)
        {
            
        }
    }
    class ServerConnectionThread
    {
        public void Run(object sckt)
        {
            Thread.CurrentThread.Name = "ServerConnectionThread";
            //Form1.Threads.Add(Thread.CurrentThread);
            /*object connsocket;
            connsocket = ((UdpClient)((object[])sckt)[0]);
            var serversock = ((Socket)((object[])sckt)[1]);
            (connsocket as Socket).Close();
            serversock.Close();*/
        }
    }
}
