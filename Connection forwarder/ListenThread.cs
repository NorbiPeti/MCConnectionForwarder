using Nito.Async;
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
            //Form1.Threads.Add(Thread.CurrentThread);
            UdpClient connection;
            var RemoteIP = new IPEndPoint(IPAddress.Any, Form1.PortNumber2);
            connection = new UdpClient(Form1.PortNumber2);
            while (true)
            {
                /*
                 * PE connection:
                 * When player joins start a thread and open connection to server
                 * When player leaves or times out disconnect from the server with matching data sent
                 * 
                 * So basically handle the PE join here to know when to start a new thread
                 */

                do
                {
                    //connection.Connect(new IPEndPoint(IPAddress.Any, Form1.PortNumber2));
                    byte[] buffer;
                    buffer = connection.Receive(ref RemoteIP); //Receive the packet ID to determine IP
                    /*
                     * Or receive all data and send it to the matching ConnectionThread determined by Client ID
                     * 
                     * Actually the Client ID is not used later on...
                     * So determine IP every time and send the packet to matching thread
                     */
                    //connection.Connect(RemoteIP);
                    //connection.Send(buffer, buffer.Length);
                    //connection.Close();
                    //if (Program.ConnThreads.ContainsKey(RemoteIP))
                    //if (Program.ConnThreads.ContainsKey(PEPackets.GetClientID(buffer)))
                        //break; //Let the ConnectionThread handle the request - By detecting packet type...

                    bool brvar = false;
                    //Int64 cid;
                    Console.WriteLine("Received packet: 0x{0:X}", buffer[0]);
                    //if (buffer[0] != PEPackets.TOSERVER_ID_OPEN_CONNECTION_REQUEST_1 && Program.ConnThreads.ContainsKey(cid = PEPackets.GetClientID(buffer)))
                    if (buffer[0] != PEPackets.TOSERVER_ID_OPEN_CONNECTION_REQUEST_1 && Program.ConnThreads.ContainsKey(RemoteIP))
                    {
                        //Handle packets and send data to matching thread
                        //(new Task(((ClientConnectionThread)Program.ConnThreads[cid][1]).SendData, buffer)).Start(new LimitedConcurrencyLevelTaskScheduler(1));
                        //new ListenThread().MemberwiseClone();
                        while (((ClientConnectionThread)Program.ConnThreads[RemoteIP][1]).DataChanged)
                            ; //Wait until thread finishes
                        ((ClientConnectionThread)Program.ConnThreads[RemoteIP][1]).DataBuffer = buffer;
                        ((ClientConnectionThread)Program.ConnThreads[RemoteIP][1]).DataChanged = true;
                        break;
                    }
                    else if (buffer[0] != PEPackets.TOSERVER_ID_OPEN_CONNECTION_REQUEST_1)
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
                    //var sendsocket = new UdpClient(Form1.PortNumber2);
                    //sendsocket.EnableBroadcast = true;
                    connection.Connect(RemoteIP); //Make it only communicate with the currently joining player
                    if (buffer[PEPackets.MAGIC.Length + 1] != PEPackets.ProtocolVer)
                    {
                        new PEPackets(PEPackets.TOCLIENT_ID_INCOMPATIBLE_PROTOCOL_VERSION, new object[] { PEPackets.ProtocolVer, PEPackets.MAGIC, 0x00000000372cdc9e }, connection, RemoteIP); //ID_INCOMPATIBLE_PROTOCOL_VERSION (0x1A)
                        break;
                    }
                    new PEPackets(PEPackets.TOCLIENT_ID_OPEN_CONNECTION_REPLY_1, new object[] { PEPackets.MAGIC, 0x00000000372cdc9e, 0, 1447 }, connection, RemoteIP);
                    //sendsocket.Close();
                    Console.WriteLine("Open connection 1 succeed.");

                    /*
                     * We must handle the open connection 2 here in order to obtain ClientID
                     */

                    buffer = connection.Receive(ref RemoteIP);
                    Console.WriteLine("Received packet: " + buffer[0]);
                    if (buffer[0] != PEPackets.TOSERVER_ID_OPEN_CONNECTION_REQUEST_2)
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
                    byte[] bufferPart = new byte[5];
                    Console.WriteLine("Extracting data...");
                    Array.Copy(buffer, PEPackets.MAGIC.Length + 1, bufferPart, 0, 5);
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
                    short ServerPortFromClient = BitConverter.ToInt16(bufferPart, 0);
                    Console.WriteLine("ServerPortFromClient: " + ServerPortFromClient);
                    Array.Copy(buffer, PEPackets.MAGIC.Length + 8, bufferPart, 0, 2);
                    short MTUSize = BitConverter.ToInt16(bufferPart, 0);
                    Console.WriteLine("MTUSize: " + MTUSize);
                    Array.Copy(buffer, PEPackets.MAGIC.Length + 10, bufferPart = new byte[8], 0, 8);
                    long ClientID = BitConverter.ToInt64(bufferPart, 0);
                    Console.WriteLine("ClientID: " + ClientID);
                    //sendsocket = new UdpClient(Form1.PortNumber2);
                    //sendsocket.Connect(RemoteIP);
                    new PEPackets(PEPackets.TOCLIENT_ID_OPEN_CONNECTION_REPLY_2, new object[] { PEPackets.MAGIC, 0x00000000372cdc9e, Form1.PortNumber2, 1464, 0 }, connection, RemoteIP);
                    //sendsocket.Close();

                    //listenclient = new UdpClient(Form1.PortNumber2);
                    //buffer = listenclient.Receive(ref RemoteIP);
                    buffer = connection.Receive(ref RemoteIP);
                    //Console.WriteLine("Received: 0x{0:X}", buffer[0]);
                    //Console.WriteLine("Second byte: 0x{0:X}", buffer[1]);
                    //listenclient.Close();
                    while (buffer[0] != 0x84)
                    { //Send NACK back
                        Console.WriteLine("Received packet: 0x{0:X}", buffer[0]);
                        Console.WriteLine("Incorrect packet. Resending...");
                        var tmpreta = PEPackets.DecodeDataPacket(buffer);
                        if (tmpreta == null)
                            break;
                        byte[] Counta = PEPackets.IntTo3Byte((int)tmpreta[1]);
                        //new PEPackets(0xA0, new object[] { (short)1, true, 0x84 }, connection, RemoteIP);
                        new PEPackets(0xA0, new object[] { (short)1, true, Counta }, connection, RemoteIP);
                        buffer = connection.Receive(ref RemoteIP);
                    }
                    /*
                     * Custom packet 0x84:
                     * Count field (3 bytes) and then data payload:
                     * Encapsulation ID (1 byte) (=0x00) and then length then data packet:
                     * Status (1 byte/8 bits)
                     */
                    //var tmp = BitConverter.GetBytes(0x84).ToList<byte>();
                    //Console.WriteLine("Remove 1st byte from: " + tmp[0] + ", " + tmp[1] + ", " + tmp[2] + ", " + tmp[3]);
                    //tmp.RemoveAt(3); //We only need 3 bytes
                    //new PEPackets(0xC0, new object[] { (short)1, true, tmp.ToArray() }, connection, RemoteIP); //Send ACK
                    var lasttick = Environment.TickCount;
                    do
                    {
                        var tmpret = PEPackets.DecodeDataPacket(buffer);
                        if (tmpret == null)
                            break;
                        Console.WriteLine("Count: " + (int)tmpret[1]);
                        Console.WriteLine("EncapsulationID: " + tmpret[0]);
                        Console.WriteLine("Count2: " + tmpret[3]);
                        byte[] Count = PEPackets.IntTo3Byte((int)tmpret[1]);
                        Console.WriteLine("Count 1st byte: " + Count[0]);
                        new PEPackets(0xC0, new object[] { (short)1, true, Count }, connection, RemoteIP);
                    } while ((buffer = connection.Receive(ref RemoteIP))[0] == 0x84 && Environment.TickCount - lasttick < 6000 * 1000);
                    //-----------------------------
                    var threadvar = new Object[3];

                    connection.Close();
                    connection = new UdpClient(Form1.PortNumber2);
                    threadvar[0] = RemoteIP;
                    threadvar[1] = connection;
                    threadvar[2] = ClientID;
                    new Thread(new ParameterizedThreadStart(new ClientConnectionThread().Run)).Start(threadvar);
                    //new Thread(new ParameterizedThreadStart(new ServerConnectionThread().Run)).Start(threadvar);
                    //new ActionThread().Do(new ClientConnectionThread().Run(threadvar));
                } while (false);
            }
        }
    }
}
