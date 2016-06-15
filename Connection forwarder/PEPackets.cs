using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Connection_forwarder
{
    class PEPackets
    {
        public static readonly byte[] MAGIC = new byte[16] { 0, 255, 255, 0, 254, 254, 254, 254, 253, 253, 253, 253, 18, 52, 86, 120 };
        public const byte ProtocolVer = 5;
        /// <summary>
        /// <para>Sent when the server doesn't support the RakNet protocol specified in 0x05.</para>
        /// <para>Protocol Version	byte	5	</para>
        /// <para>MAGIC	MAGIC		</para>
        /// <para>Server ID	int64	0x00000000372cdc9e	</para>
        /// </summary>
        public static readonly byte TOCLIENT_ID_INCOMPATIBLE_PROTOCOL_VERSION = 0x1A;
        /// <summary>
        /// <para>If the version is different than yours, reply with a ID_INCOMPATIBLE_PROTOCOL_VERSION (0x1A)</para>
        /// <para>Sent from the client after the player taps on the server in the world list The client will repeatedly send this with reducing sizes until it successfully receives a reply. Observed behaviour is that the client will send packets ~0.5s apart in the following way, until it gets a 0x06 response packet, or reaches the end of these:</para>
        /// <para>4 packets of Null Payload length of 1447</para>
        /// <para>4 packets of Null Payload length of 1155</para>
        /// <para>5 packets of Null Payload length of 531</para>
        /// <para>If the server doesnt't reply the client, the client will display a "Connect Error" window</para>
        /// <para>MAGIC	MAGIC		</para>
        /// <para>RakNet Protocol Version	byte	5	Check the Data Packet section for the current version</para>
        /// <para>Null Payload	many 0x00 bytes	0x00 * 1447	MTU (Maximum Transport Unit)</para>
        /// <para>Total Size:	18 Bytes + length of Null Payload</para>
        /// </summary>
        public static readonly byte TOSERVER_ID_OPEN_CONNECTION_REQUEST_1 = 0x05;
        public static readonly byte TOCLIENT_ID_OPEN_CONNECTION_REPLY_1 = 0x06;
        public static readonly byte TOSERVER_ID_OPEN_CONNECTION_REQUEST_2 = 0x07;
        public static readonly byte TOCLIENT_ID_OPEN_CONNECTION_REPLY_2 = 0x08;
        public static readonly byte TOSERVER_ReadyPacket = 0x84;
        public static readonly byte TOCLIENT_LoginStatusPacket = 0x83;
        
        public PEPackets(byte packetid, object[] data, UdpClient sock, IPEndPoint ep)
        {
            var tmp = new List<byte>();
            tmp.Add(packetid);
            for (int i = 0; i < data.Length; i++)
            {
                if (!data[i].GetType().IsSubclassOf(typeof(Array)))
                {
                    if (data[i].GetType() == typeof(byte))
                        tmp.Add((byte)data[i]);
                    else if (data[i].GetType() == typeof(bool))
                        tmp.AddRange(BitConverter.GetBytes((bool)data[i]));
                    else if (data[i].GetType() == typeof(char))
                        tmp.AddRange(BitConverter.GetBytes((char)data[i]));
                    else if (data[i].GetType() == typeof(short))
                        tmp.AddRange(BitConverter.GetBytes((short)data[i]));
                    else if (data[i].GetType() == typeof(int))
                        tmp.AddRange(BitConverter.GetBytes((int)data[i]));
                    else if (data[i].GetType() == typeof(long))
                        tmp.AddRange(BitConverter.GetBytes((long)data[i]));
                    else if (data[i].GetType() == typeof(double))
                        tmp.AddRange(BitConverter.GetBytes((double)data[i]));
                    else if (data[i].GetType() == typeof(float))
                        tmp.AddRange(BitConverter.GetBytes((float)data[i]));
                    else if (data[i].GetType() == typeof(uint))
                        tmp.AddRange(BitConverter.GetBytes((uint)data[i]));
                    else if (data[i].GetType() == typeof(ulong))
                        tmp.AddRange(BitConverter.GetBytes((ushort)data[i]));
                    else if (data[i].GetType() == typeof(ushort))
                        tmp.AddRange(BitConverter.GetBytes((ushort)data[i]));
                }
                else
                {
                    for (int j = 0; j < ((Array)data[i]).Length; j++)
                    {
                        if (((Array)data[i]).GetValue(j).GetType() == typeof(byte))
                            tmp.Add((byte)((Array)data[i]).GetValue(j));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(bool))
                            tmp.AddRange(BitConverter.GetBytes((bool)((Array)data[i]).GetValue(j)));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(char))
                            tmp.AddRange(BitConverter.GetBytes((char)((Array)data[i]).GetValue(j)));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(double))
                            tmp.AddRange(BitConverter.GetBytes((double)((Array)data[i]).GetValue(j)));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(float))
                            tmp.AddRange(BitConverter.GetBytes((float)((Array)data[i]).GetValue(j)));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(int))
                            tmp.AddRange(BitConverter.GetBytes((int)((Array)data[i]).GetValue(j)));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(long))
                            tmp.AddRange(BitConverter.GetBytes((long)((Array)data[i]).GetValue(j)));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(short))
                            tmp.AddRange(BitConverter.GetBytes((short)((Array)data[i]).GetValue(j)));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(uint))
                            tmp.AddRange(BitConverter.GetBytes((uint)((Array)data[i]).GetValue(j)));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(ulong))
                            tmp.AddRange(BitConverter.GetBytes((ushort)((Array)data[i]).GetValue(j)));
                        else if (((Array)data[i]).GetValue(j).GetType() == typeof(ushort))
                            tmp.AddRange(BitConverter.GetBytes((ushort)((Array)data[i]).GetValue(j)));
                        else Console.WriteLine("Unknown type in array: " + ((Array)data[i]).GetValue(j).GetType());
                    }
                }
            }
            try
            {
                sock.Send(tmp.ToArray(), tmp.Count, ep);
            }
            catch
            {
                sock.Send(tmp.ToArray(), tmp.Count);
            }
        }

        internal static Int64 GetClientID(byte[] buffer)
        { //ClientID is in open connection request 2 and in data packets
            switch(buffer[0])
            {
                case 0x84:
                    byte[] tmp = new byte[8];
                    Array.Copy(buffer, 17, tmp, 0, 8);
                    return BitConverter.ToInt64(tmp, 0);
                default:
                    Console.WriteLine("Unable to get ClientID: " + buffer);
                    return 0;
            }
        }
        public static object[] DecodeDataPacket(byte[] data)
        {
            byte[] tmp;
            //Array.Copy(data, 1, tmp = new byte[4], 1, 3); //Leave the first byte 0 so it will (hopefully) give the correct 4-bit representation
            Array.Copy(data, 1, tmp = new byte[4], 0, 3); //Nope, it starts at the first byte
            int Count = BitConverter.ToInt32(tmp, 0);
            byte EncapsulationID = data[4];
            short length;
            int Count2;
            int Unknown;
            switch(EncapsulationID)
            {
                case 0x00:
                    length = BitConverter.ToInt16(data, 5);
                    Array.Copy(data, 7, tmp=new byte[data.Length-8], 0, data.Length-8); //7. byte: data +1
                    return new object[] { EncapsulationID, Count, length, tmp };
                case 0x40:
                    length = BitConverter.ToInt16(data, 5);
                    Array.Copy(data, 7, tmp = new byte[4], 0, 3); //7. byte: Count(?)
                    Count2 = BitConverter.ToInt32(tmp, 0);
                    Array.Copy(data, 10, tmp=new byte[data.Length-11], 0, data.Length - 11); //10. byte: data + Count +1
                    return new object[] { EncapsulationID, Count, length, Count2, tmp };
                case 0x60:
                    length = BitConverter.ToInt16(data, 5);
                    Array.Copy(data, 7, tmp = new byte[4], 0, 3); //7. byte: Count(?)
                    Count2 = BitConverter.ToInt32(tmp, 0);
                    Unknown = BitConverter.ToInt32(data, 10); //10. byte: Unknown
                    Array.Copy(data, 14, tmp=new byte[data.Length-15], 0, data.Length - 15); //14. byte: data +1
                    return new object[] { EncapsulationID, Count, length, Count2, tmp };
                default:
                    return null;
            }
        }
        public static byte[] IntTo3Byte(int i)
        {
            byte[] tmp = BitConverter.GetBytes(i);
            byte[] ret = new byte[3];
            //Array.Copy(tmp, 1, ret, 0, 3);
            Array.Copy(tmp, 0, ret, 0, 3);
            return ret;
        }
    }
}
