using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
   public static class MsgPokerPlayerTurn
    {
       public static unsafe ServerSockets.Packet PokerPlayerTurnCreate(this ServerSockets.Packet stream, ushort dwparam1, ushort dwparam2, uint dwparam3, uint dwparam4,uint dwparam5)
       {
           stream.InitWriter();
           stream.Write(dwparam1);
           stream.Write(dwparam2);
           stream.Write(dwparam3);
           stream.Write(dwparam4);
           stream.Write(dwparam5);
           stream.Finalize(GamePackets.PokerPlayerTurn);
           return stream;

       }
       public static unsafe ServerSockets.Packet PokerPlayerTurnCreate(this ServerSockets.Packet stream, ushort dwparam1, ushort dwparam2, ulong dwparam3, ulong dwparam4, uint dwparam5)
       {
           stream.InitWriter();
           stream.Write(dwparam1);
           stream.Write(dwparam2);
           stream.Write(dwparam3);
           stream.Write(dwparam4);
           stream.Write(dwparam5);
           stream.Finalize(GamePackets.PokerPlayerTurn);
           return stream;

       }

    }
}
