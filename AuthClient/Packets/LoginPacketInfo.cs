using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project_Terror_v2.AuthClient.Packets
{
    public unsafe struct LoginPacketInfo
    {
        public const ushort cSize = 100;
        public const ushort cType = 1542;

        public ushort Size;
        public ushort PacketType;
        public uint UnKnow;
        public fixed sbyte szUser[64];
        public fixed sbyte szPassword[16];

        public string User { get { fixed (sbyte* ptr = szUser) { return new string(ptr); } } }
        public string Password { get { fixed (sbyte* ptr = szPassword) { return new string((sbyte*)ptr); } } }
    }
}
