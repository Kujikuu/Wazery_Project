using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
   public static class MsgHandBrickInfo
   {
       public enum BrickInfo
       {
           ShowSubmitDialog = 2,
       }
       public static unsafe void GetHandBrickInfo(this ServerSockets.Packet stream, out BrickInfo type, out ulong DwParam, out ushort DwParam2)
        {
            type = (BrickInfo) stream.ReadUInt16();
            DwParam = stream.ReadUInt64();
            DwParam2 = stream.ReadUInt16();
        }

       public static unsafe ServerSockets.Packet HandBrickInfoCreate(this ServerSockets.Packet stream, BrickInfo type, ulong DwParam, ushort DwParam2)
        {
            stream.InitWriter();

            stream.Write((ushort)type);
            stream.Write((uint)DwParam);
            stream.Write((uint)1);
            stream.Write(DwParam2);

            stream.Finalize(GamePackets.MsgHandBrickInfo);
            return stream;
        }
       [PacketAttribute(GamePackets.MsgHandBrickInfo)]
       private static void Process(Client.GameClient user, ServerSockets.Packet stream)
       {
           //points from plunder war
       /*    if (user.Player.InUnion)
           {
               user.Player.MyUnion.GoldBrick = user.Player.KingDomGold;
               user.SendSysMesage("You have submit " + user.Player.KingDomGold + " Gold Bricks !");
               
           }
           else
               user.SendSysMesage("You have submit 0 Gold Bricks ! Please join in union.");*/
       }
    }
}
