using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connection_forwarder
{
    class ListenThread
    {
        public static void Run()
        {
            Thread.CurrentThread.Name = "ListenThread";
            Form1.Threads.Add(Thread.CurrentThread);
            Socket listensocket;
            //UdpClient listenclient = new UdpClient(Form1.PortNumber2);
            UdpClient listenclient;
            if (!Form1.PEVersion)
            {
                //var listensocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                listensocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                listensocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Form1.PortNumber2));
            }
            else
            {
                listensocket = new Socket(SocketType.Dgram, ProtocolType.Udp); //Just for it to not give "no object" error
                //listensocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Form1.PortNumber2));
                //listensocket.Bind(new IPEndPoint(Program.GetLocalIP(), Form1.PortNumber2));
            }
            while (true)
            {
                if (!Form1.PEVersion)
                {
                    listensocket.Listen(10);
                    //var connsocket = listensocket.Accept();
                    Socket[] connsocket = new Socket[2];
                    connsocket[0] = listensocket.Accept();
                    connsocket[1] = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    connsocket[1].Connect("localhost", Form1.PortNumber);
                    new Thread(new ParameterizedThreadStart(new ClientConnectionThread().Run)).Start(connsocket);
                    new Thread(new ParameterizedThreadStart(new ServerConnectionThread().Run)).Start(connsocket);
                }
                else
                {
                    /*
                     * PE connection:
                     * When player joins start a thread and open connection to server
                     * When player leaves or times out disconnect from the server with matching data sent
                     * 
                     * So basically handle the PE join here to know when to start a new thread
                     */

                    //byte[] buffer = new byte[1500];
                    byte[] buffer;
                    var RemoteIP=new IPEndPoint(IPAddress.Any, Form1.PortNumber2);
                    listenclient = new UdpClient(Form1.PortNumber2);
                    buffer = listenclient.Receive(ref RemoteIP);
                    listenclient.Close();
                    //RemoteIP.Port = 19132;
                    //Console.WriteLine(String.Format("Hex: %x", buffer[0]));
                    //Console.WriteLine("Hex: {0:X}", buffer[0]);
                    //bool stop = false;
                    do
                    {
                        bool brvar = false;
                        if (buffer[0] != PEPackets.TOSERVER_ID_OPEN_CONNECTION_REQUEST_1)
                            break;
                        for (int i = 0; i < PEPackets.MAGIC.Length; i++)
                        {
                            if (buffer[i + 1] != PEPackets.MAGIC[i])
                            {
                                Console.WriteLine("There is no magic for this client...");
                                brvar = true;
                                break;
                            }
                        }
                        if (brvar)
                            break;
                        Console.WriteLine("Magic accepted.");
                        //var sendsocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                        var sendsocket = new UdpClient(Form1.PortNumber2);
                        //sendsocket.Connect(RemoteIP);
                        sendsocket.EnableBroadcast = true;
                        //sendsocket.Connect(IPAddress.Broadcast, RemoteIP.Port);
                        sendsocket.Connect(RemoteIP);
                        if (buffer[PEPackets.MAGIC.Length + 1] != PEPackets.ProtocolVer)
                        {
                            new PEPackets(PEPackets.TOCLIENT_ID_INCOMPATIBLE_PROTOCOL_VERSION, new object[] { PEPackets.ProtocolVer, PEPackets.MAGIC, 0x00000000372cdc9e }, sendsocket, RemoteIP); //ID_INCOMPATIBLE_PROTOCOL_VERSION (0x1A)
                            break;
                        }
                        new PEPackets(PEPackets.TOCLIENT_ID_OPEN_CONNECTION_REPLY_1, new object[] { PEPackets.MAGIC, 0x00000000372cdc9e, 0, 1447 }, sendsocket, RemoteIP);
                        sendsocket.Close();
                        Console.WriteLine("Open connection 1 succeed.");

                        listenclient = new UdpClient(Form1.PortNumber2);
                        buffer = listenclient.Receive(ref RemoteIP);
                        listenclient.Close();
                        //RemoteIP.Port = 19132;
                        if (buffer[0] != PEPackets.TOSERVER_ID_OPEN_CONNECTION_REQUEST_2)
                            break;
                        brvar = false;
                        for (int i = 0; i < PEPackets.MAGIC.Length; i++)
                        {
                            if (buffer[i + 1] != PEPackets.MAGIC[i])
                            {
                                Console.WriteLine("There is no magic for this client...");
                                brvar = true;
                                break;
                            }
                        }
                        if (brvar)
                            break;
                        byte[] bufferPart = new byte[5];
                        Array.Copy(buffer, PEPackets.MAGIC.Length + 1, bufferPart, 0, 5);
                        //if (bufferPart != BitConverter.GetBytes(0x043f57fefd))
                        for (int i = 0; i < bufferPart.Length; i++)
                        {
                            if (bufferPart[i] != 0x00)
                            {
                                brvar = true;
                                break;
                            }
                        }
                        if (brvar)
                            break;
                        Array.Copy(buffer, PEPackets.MAGIC.Length + 6, bufferPart = new byte[2], 0, 2);
                        //if (bufferPart != BitConverter.GetBytes((short)19132))
                        //var bytes = BitConverter.GetBytes((short)19132);
                        /*for (int i = 0; i < 2; i++)
                        {
                            if (bufferPart[i] != bytes[i])
                            {
                                brvar = true;
                                break;
                            }
                        }
                        if (brvar)
                            break;*/
                        short ServerPortFromClient = BitConverter.ToInt16(bufferPart, 0);
                        Console.WriteLine("ServerPortFromClient: " + ServerPortFromClient);
                        Array.Copy(buffer, PEPackets.MAGIC.Length + 8, bufferPart, 0, 2);
                        //if (bufferPart != BitConverter.GetBytes((short)1464))
                        short MTUSize = BitConverter.ToInt16(bufferPart, 0);
                        Console.WriteLine("MTUSize: " + MTUSize);
                        Array.Copy(buffer, PEPackets.MAGIC.Length + 10, bufferPart = new byte[8], 0, 8);
                        //long ClientID = Convert.ToInt64(bufferPart);
                        long ClientID = BitConverter.ToInt64(bufferPart, 0);
                        Console.WriteLine("ClientID: " + ClientID);
                        sendsocket = new UdpClient(Form1.PortNumber2);
                        //sendsocket.EnableBroadcast = true;
                        sendsocket.Connect(RemoteIP);
                        new PEPackets(PEPackets.TOCLIENT_ID_OPEN_CONNECTION_REPLY_2, new object[] { PEPackets.MAGIC, 0x00000000372cdc9e, Form1.PortNumber2, 1464, 0 }, sendsocket, RemoteIP);
                        sendsocket.Close();

                        listenclient = new UdpClient(Form1.PortNumber2);
                        buffer = listenclient.Receive(ref RemoteIP);
                        Console.WriteLine("Received: 0x{0:X}", buffer[0]);
                        Console.WriteLine("Second byte: 0x{0:X}", buffer[1]);
                        listenclient.Close();

                        sendsocket = new UdpClient(Form1.PortNumber2);
                        //sendsocket.EnableBroadcast = true;
                        sendsocket.Connect(RemoteIP);
                        new PEPackets(PEPackets.TOCLIENT_LoginStatusPacket, new object[] { 0 }, sendsocket, RemoteIP);
                        sendsocket.Close();
                        //-----------------------------
                        /*object[] connsocket = new object[2];
                        //if(buffer[0]==PEPackets)
                        connsocket[0] = listenclient;
                        connsocket[1] = new Socket(SocketType.Stream, ProtocolType.Tcp); //We still need to connect to the server
                        ((Socket)connsocket[1]).Connect("localhost", Form1.PortNumber);
                        new Thread(new ParameterizedThreadStart(new ClientConnectionThread().Run)).Start(connsocket);
                        new Thread(new ParameterizedThreadStart(new ServerConnectionThread().Run)).Start(connsocket);*/
                    } while (false);
                }
                //for (int i = 0; i < 2; i++)
                //new Thread(new ParameterizedThreadStart(new ClientConnectionThread().Run)).Start(connsocket);
                //var serversock = new Socket(SocketType.Stream, ProtocolType.Tcp);
                //serversock.Connect("localhost", Form1.PortNumber);
                //new Thread(new ParameterizedThreadStart(new ServerConnectionThread().Run)).Start(serversock);
                //new Thread(new ParameterizedThreadStart(new ServerConnectionThread().Run)).Start(connsocket);
            }
        }
    }
}
