using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project_Terror_v2.AuthClient.Packets
{
    public unsafe struct PasswordCryptographySeed
    {
        public ushort Leng;
        public ushort PacketType;
        public int Seed;

        public static PasswordCryptographySeed Create()
        {
            PasswordCryptographySeed packet = new PasswordCryptographySeed();
            packet.Leng = 8;
            packet.PacketType = 1059;
            return packet;
        }
    }
}
