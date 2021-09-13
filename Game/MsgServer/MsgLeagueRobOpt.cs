using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace LightConquer_Project.Game.MsgServer
{
   public static class MsgLeagueRobOpt
    {
       [ProtoContract]
       public class RobOpt
       {
           [ProtoMember(1, IsRequired = true)]
           public uint Type { get; set; }

           [ProtoMember(2, IsRequired = true)]
           public uint Unknown2 { get; set; }

           [ProtoMember(3, IsRequired = false)]
           public uint ID { get; set; }

           [ProtoMember(4, IsRequired = false)]
           public string Name { get; set; }
       }

       public static unsafe ServerSockets.Packet CreateLeagueRobOpt(this ServerSockets.Packet stream, RobOpt obj)
       {
           stream.InitWriter();
           stream.ProtoBufferSerialize(obj);
           stream.Finalize(GamePackets.LeagueRobOpt);

           return stream;
       }
       public static unsafe void GetLeagueRobOpt(this ServerSockets.Packet stream, out RobOpt pQuery)
       {
           pQuery = new RobOpt();
           pQuery = stream.ProtoBufferDeserialize<RobOpt>(pQuery);
       }
    }
}
