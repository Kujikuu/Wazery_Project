using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet RoulettedAddNewPlayerCreate(this ServerSockets.Packet stream, uint UID, uint Mesh, MsgRouletteOpenGui.Color Color, string Name)
        {
            stream.InitWriter();

            stream.Write(UID);
            stream.Write(Mesh);
            stream.Write((byte)Color);
            stream.Write(Name,16);

            stream.Finalize(GamePackets.MsgRoulettedAddNewPlayer);
            return stream;
        }
    }
}
