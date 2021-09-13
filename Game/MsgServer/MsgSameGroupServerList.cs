using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace LightConquer_Project.Game.MsgServer
{
    public static class MsgSameGroupServerList
    {
        [ProtoContract]
        public class GroupServer
        {
            [ProtoMember(1, IsRequired = true)]
            public Server[] Servers;
        }
        [ProtoContract]
        public class Server
        {
            [ProtoMember(1, IsRequired = true)]
            public uint ServerID;
            [ProtoMember(2, IsRequired = true)]
            public uint MapID;
            [ProtoMember(3, IsRequired = true)]
            public uint X;
            [ProtoMember(4, IsRequired = true)]
            public uint Y;
            [ProtoMember(5, IsRequired = true)]
            public uint GroupID;
            [ProtoMember(6, IsRequired = true)]
            public string Name;
        }
        public static unsafe ServerSockets.Packet CreateGroupServerList(this ServerSockets.Packet stream, GroupServer obj)
        {
            stream.InitWriter();
            stream.ProtoBufferSerialize(obj);
            stream.Finalize(GamePackets.MsgSameGroupServerList);
            return stream;
        }
        
        public static unsafe void GetGroupServerList(this ServerSockets.Packet stream, out GroupServer pQuery)
        {
            pQuery = new GroupServer();
            pQuery = stream.ProtoBufferDeserialize<GroupServer>(pQuery);
        }
    }
}
