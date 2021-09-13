using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Project_Terror_v2.AuthClient.Packets
{

    public unsafe struct AuthResponsePacket
    {

        public ushort Size;

        public ushort PacketType;

        public int Identifier;

        public uint Type;

        public uint Port;

        public uint Unknow;

        private fixed sbyte szIPAddress[16];

        public unsafe string IPAddress
        {
            get { fixed (sbyte* bp = szIPAddress) { return new string(bp); } }
            set
            {
                string ip = value;
                fixed (sbyte* bp = szIPAddress)
                {
                    for (int i = 0; i < ip.Length; i++)
                        bp[i] = (sbyte)ip[i];
                }
            }
        }

        public static AuthResponsePacket Create()
        {
            AuthResponsePacket retn = new AuthResponsePacket();
            retn.Size = 36;
            retn.PacketType = 0x41F;
            return retn;
        }
    }
}
