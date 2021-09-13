using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace LightConquer_Project.Game.MsgServer
{
   public static class MsgLeagueToken
    {
       [ProtoContract]
       public class LeagueToken
       {
           [ProtoMember(1, IsRequired = true)]
           public uint Type;
           [ProtoMember(2, IsRequired = true)]
           public uint dwpram2;
           [ProtoMember(3, IsRequired = true)]
           public uint dwpram3;
           [ProtoMember(4, IsRequired = true)]
           public uint Time;
           [ProtoMember(5, IsRequired = true)]
           public string text;
       }
       public static unsafe ServerSockets.Packet CreateLeagueToken(this ServerSockets.Packet stream, LeagueToken obj)
       {
           stream.InitWriter();
           stream.ProtoBufferSerialize(obj);
           stream.Finalize(GamePackets.LeagueToken);
           return stream;
       }
       public static unsafe void GetLeagueToken(this ServerSockets.Packet stream, out LeagueToken pQuery)
       {
           pQuery = new LeagueToken();
           pQuery = stream.ProtoBufferDeserialize<LeagueToken>(pQuery);
       }


       [PacketAttribute(GamePackets.LeagueToken)]
       private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
       {
         

           LeagueToken pQuery;
           stream.GetLeagueToken(out pQuery);

         /*  for (int x = 0; x < 10; x++)
           {
               pQuery.dwpram1 = (uint)x;
               // pQuery.dwpram2 = user.Player.UID;
               //  pQuery.dwpram1 = 10000;
               // pQuery.dwpram3 = 10000;
               pQuery.dwpram4 = 60;
               pQuery.text = "alexx";

               user.Send(stream.CreateLeagueToken(pQuery));
           }
          */
       }
    }
}
