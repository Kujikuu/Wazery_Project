﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LightConquer_Project.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet RouletteScreenCreate(this ServerSockets.Packet stream, uint UID)
        {
            stream.InitWriter();

            stream.Write((byte)0);
            stream.Write(UID);

            stream.Finalize(GamePackets.MsgRouletteScreen);
            return stream;
        }
    }
}
