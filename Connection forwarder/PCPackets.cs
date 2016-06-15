using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Connection_forwarder
{
    class PCPackets
    {
        //public static List<PCPacket> Packets = new List<PCPacket>();
        //public static readonly Dictionary<string, byte> PacketNames = new Dictionary<string, byte>();
        /*                    http://wiki.vg/Pre-release_protocol                    */
        /// <summary>
        /// <para>Gamemode	 Unsigned Byte	 0: survival, 1: creative, 2: adventure. Bit 3 (0x8) is the hardcore flag</para>
        /// <para>Dimension	 Byte	 -1: nether, 0: overworld, 1: end</para>
        /// <para>Difficulty	 Unsigned Byte	 0 thru 3 for Peaceful, Easy, Normal, Hard</para>
        /// <para>Max Players	 Unsigned Byte	 Used by the client to draw the player list</para>
        /// <para>Level Type	 String	 default, flat, largeBiomes, amplified, default_1_1</para>
        /// <para>Reduced Debug Info	 Boolean</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_JOIN_GAME = 0x01;
        /// <summary>
        /// <para>Json data: string; Chat, Limited to 32767 bytes</para>
        /// <para>Position: byte; 0 - Chat (chat box) ,1 - System Message (chat box), 2 - Above action bar</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_CHAT_MESSAGE = 0x02;
        /// <summary>
        /// <para>EntityID	 VarInt	 Entity's ID</para>
        /// <para>Slot	 Short	 Equipment slot: 0=held, 1-4=armor slot (1 - boots, 2 - leggings, 3 - chestplate, 4 - helmet</para>
        /// <para>Item	Slot	 Item in slot format</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_ENTITY_EQUIPMENT = 0x04;
        /// <summary>
        /// <para>Sent by the server after login to specify the coordinates of the spawn point (the point at which players spawn at, and which the compass points to). It can be sent at any time to update the point compasses point at.</para>
        /// <para>0x05	 Location	 Position	 Spawn location</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_SPAWN_POSITION = 0x05;
        /// <summary>
        /// <para>Sent by the server to update/set the health of the player it is sent to.</para>
        /// <para>Food saturation acts as a food "overcharge". Food values will not decrease while the saturation is over zero. Players logging in automatically get a saturation of 5.0. Eating food increases the saturation as well as the food bar.</para>
        /// <para>Health	 Float	 0 or less = dead, 20 = full HP</para>
        /// <para>Food	 VarInt	 0 - 20</para>
        /// <para>Food Saturation	 Float	 Seems to vary from 0.0 to 5.0 in integer increments</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_UPDATE_HEALTH = 0x06;
        /// <summary>
        /// <para>Updates the players position on the server. If the distance between the last known position of the player on the server and the new position set by this packet is greater than 100 units will result in the client being kicked for "You moved too quickly :( (Hacking?)" Also if the fixed-point number of X or Z is set greater than 3.2E7D the client will be kicked for "Illegal position"</para>
        /// <para>Yaw is measured in degrees, and does not follow classical trigonometry rules. The unit circle of yaw on the XZ-plane starts at (0, 1) and turns counterclockwise, with 90 at (-1, 0), 180 at (0,-1) and 270 at (1, 0). Additionally, yaw is not clamped to between 0 and 360 degrees; any number is valid, including negative numbers and numbers greater than 360.</para>
        /// <para>Pitch is measured in degrees, where 0 is looking straight ahead, -90 is looking straight up, and 90 is looking straight down.</para>
        /// <para>The yaw of player (in degrees), standing at point (x0,z0) and looking towards point (x,z) one can be calculated with:</para>
        /// <para>    l = x-x0</para>
        /// <para>    w = z-z0</para>
        /// <para>    c = sqrt( l*l + w*w )</para>
        /// <para>    alpha1 = -arcsin(l/c)/PI*180</para>
        /// <para>    alpha2 =  arccos(w/c)/PI*180</para>
        /// <para>    if alpha2 > 90 then</para>
        /// <para>    yaw = 180 - alpha1</para>
        /// <para>    else</para>
        /// <para>    yaw = alpha1</para>
        /// <para>You can get a unit vector from a given yaw/pitch via:</para>
        /// <para>    x = -cos(pitch) * sin(yaw)</para>
        /// <para>    y = -sin(pitch)</para>
        /// <para>    z =  cos(pitch) * cos(yaw)</para>
        /// <para>About the flags field:</para>
        /// <para>    (Dinnerbone) It's a bitfield, X/Y/Z/Y_ROT/X_ROT. If X is set, the x value is relative and not absolute.</para>
        /// <para>X	 Double	 Absolute/Relative position</para>
        /// <para>Y	 Double	 Absolute/Relative position</para>
        /// <para>Z	 Double	 Absolute/Relative position</para>
        /// <para>Yaw	 Float	 Absolute/Relative rotation on the X Axis, in degrees</para>
        /// <para>Pitch	 Float	 Absolute/Relative rotation on the Y Axis, in degrees</para>
        /// <para>Flags	 Byte	</para>
        /// <para>X	 0x01</para>
        /// <para>Y	 0x02</para>
        /// <para>Z	 0x04</para>
        /// <para>Y_ROT	 0x08</para>
        /// <para>X_ROT	 0x10;</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_PLAYER_POSITION_AND_LOOK = 0x08;
        /// <summary>
        /// <para>This packet tells that a player goes to bed.</para>
        /// <para>The client with the matching Entity ID will go into bed mode.</para>
        /// <para>This Packet is sent to all nearby players including the one sent to bed.</para>
        /// <para>Entity ID	 VarInt	 Player ID</para>
        /// <para>Location	 Position	 Block location of the head part of the bed</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_USE_BED = 0x0A;
        /// <summary>
        /// <para>This packet tells that a player goes to bed.</para>
        /// <para>This packet is sent by the server when a player comes into visible range, not when a player joins.</para>
        /// <para>Servers can, however, safely spawn player entities for players not in visible range. The client appears to handle it correctly.</para>
        /// <para>When in online-mode the uuids must be valid and have valid skin blobs, in offline-mode uuid v3 is used.</para>
        /// <para>For NPCs UUID v2 should be used. Note:</para>
        /// <para>(+Grum) i will never confirm this as a feature you know that :)</para>
        /// <para>Entity ID	 VarInt	 Player's Entity ID</para>
        /// <para>Player UUID	 UUID	 Player's UUID</para>
        /// <para>X	 Int	 Player X as a Fixed-Point number</para>
        /// <para>Y	 Int	 Player X as a Fixed-Point number</para>
        /// <para>Z	 Int	 Player X as a Fixed-Point number</para>
        /// <para>Yaw	 Byte	 Player rotation as a packed byte</para>
        /// <para>Pitch	 Byte	 Player rotation as a packet byte</para>
        /// <para>Current Item	 Short	 The item the player is currently holding. Note that this should be 0 for "no item", unlike -1 used in other packets. A negative value crashes clients.</para>
        /// <para>Metadata	Metadata	 The client will crash if no metadata is sent</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_SPAWN_PLAYER = 0x0C;
        /// <summary>
        /// <para>This packet tells that a player goes to bed.</para>
        /// <para>Sent by the server when someone picks up an item lying on the ground - its sole purpose appears to be the animation of the item flying towards you. It doesn't destroy the entity in the client memory, and it doesn't add it to your inventory. The server only checks for items to be picked up after each Player Position and [Player Position & Look packet sent by the client.</para>
        /// <para>Collected Entity ID	 VarInt</para>
        /// <para>Collector Entity ID	 VarInt</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_COLLECT_ITEM = 0x0A;
        /// <summary>
        /// <para>This packet shows location, name, and type of painting.</para>
        /// <para>Calculating the center of an image: given a (width x height) grid of cells, with (0, 0) being the top left corner, the center is (max(0, width / 2 - 1), height / 2). E.g.</para>
        /// <para>2x1 (1, 0) 4x4 (1, 2)</para>
        /// <para>Entity ID	VarInt	Entity's ID</para>
        /// <para>Title	String	Name of the painting. Max length 13</para>
        /// <para>Location	Position	Center coordinates</para>
        /// <para>Direction	Unsigned Byte	Direction the painting faces (0 -z, 1 -x, 2 +z, 3 +x)</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_SPAWN_PAINTING = 0x10;
        /// <summary>
        /// <para>Velocity is believed to be in units of 1/8000 of a block per server tick (50ms); for example, -1343 would move (-1343 / 8000) = −0.167875 blocks per tick (or −3,3575 blocks per second).</para>
        /// <para>Entity ID	VarInt	Entity's ID</para>
        /// <para>Velocity X	Short	Velocity on the X axis</para>
        /// <para>Velocity Y	Short	Velocity on the Y axis</para>
        /// <para>Velocity Z	Short	Velocity on the Z axis</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_ENTITY_VELOCITY = 0x12;
        /// <summary>
        /// <para>Sent by the server when an list of Entities is to be destroyed on the client.</para>
        /// <para>Entity ID	VarInt	Entity's ID</para>
        /// <para>Count	VarInt	Length of following array</para>
        /// <para>Entity IDs	Array of VarInt	The list of entities of destroy</para>
        /// </summary>
        public static readonly byte PLAY_TOCLIENT_DESTROY_ENTITIES = 0x13;

        public PCPackets(byte packetid, object[] data, Socket sock)
        {
            //Prepare all possible packets
            //Packets.Add(new PCPacket(0x02, "Chat"));
            //Packets[0].Function = Packets[0].Callback;
            /*Packets[0].Function = delegate(object[] asd)
            {
                return;
            };*/
            var tmp=new List<byte>();
            tmp.Add(packetid);
            for(int i=0; i<data.Length; i++)
            {
                if (data.GetType() == typeof(bool))
                    tmp.AddRange(BitConverter.GetBytes((bool)data[i]));
                else if (data.GetType() == typeof(char))
                    tmp.AddRange(BitConverter.GetBytes((char)data[i]));
                else if (data.GetType() == typeof(double))
                    tmp.AddRange(BitConverter.GetBytes((double)data[i]));
                else if (data.GetType() == typeof(float))
                    tmp.AddRange(BitConverter.GetBytes((float)data[i]));
                else if (data.GetType() == typeof(int))
                    tmp.AddRange(BitConverter.GetBytes((int)data[i]));
                else if (data.GetType() == typeof(long))
                    tmp.AddRange(BitConverter.GetBytes((long)data[i]));
                else if (data.GetType() == typeof(short))
                    tmp.AddRange(BitConverter.GetBytes((short)data[i]));
                else if (data.GetType() == typeof(uint))
                    tmp.AddRange(BitConverter.GetBytes((uint)data[i]));
                else if (data.GetType() == typeof(ulong))
                    tmp.AddRange(BitConverter.GetBytes((ushort)data[i]));
                else if (data.GetType() == typeof(ushort))
                    tmp.AddRange(BitConverter.GetBytes((ushort)data[i]));
            }
            sock.Send(tmp.ToArray());
        }
    }
    /*class PCPacket
    {
        public byte PacketID { get; private set; }
        public object[] Contents { get; set; }
        public string PacketName { get; private set; }
        internal delegate void FunctionT(params object[] asd);
        public FunctionT Function;

        //public PCPacket(byte packetid, params object[] contents)
        public PCPacket(byte packetid, string packetname)
        {
            PacketID = packetid;
            PacketName = packetname;
        }

        internal bool Callback(Type[] types, object[] parameters)
        {
            return true;
        }
    }*/
}
