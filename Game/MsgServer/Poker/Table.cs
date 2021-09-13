using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
    public class Table : Role.IMapObj
    {
        public enum TableType : uint
        {
            Silver = 0,
            ConquerPoints = 1
        }
        public bool AllowDynamic { get; set; }
        public uint UID { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public uint Map { get; set; }
        public uint DynamicID { get; set; }
        public bool Alive { get; set; }
        public uint Mesh;
        public uint Noumber;
        public uint FixedBet;
        public TableType Type;
        public Match TableMatch;
        public uint MinBet;

        public Extensions.SafeDictionary<uint, Client.GameClient> Players = new Extensions.SafeDictionary<uint, Client.GameClient>();

        public Extensions.SafeDictionary<uint, Client.GameClient> Watchers = new Extensions.SafeDictionary<uint, Client.GameClient>();
        public Extensions.MyList<uint> PlayersAcceptKick = new Extensions.MyList<uint>();
        public Client.GameClient ReceiveKick = null;
        public uint StarterKick = 0;

        public void AddWatcher(Client.GameClient user)
        {
            Watchers.Add(user.Player.UID, user);
        }

        public Client.GameClient[] PlayersLocation = new Client.GameClient[10];

        public Table()
        {
            ObjType = Role.MapObjectType.PokerTable;
            TableMatch = new Match(this);
        }
        public void ResetTable()
        {
            TableMatch = new Match(this);

        }
        public bool IsTrap()
        {
            return false;
        }
        public unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, bool View)
        {
            stream.InitWriter();
            stream.Write(UID);
            stream.ZeroFill(sizeof(uint) * 2);
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Mesh);
            stream.Write((ushort)0);
            stream.Write(Noumber);
            stream.Write(FixedBet);
            stream.Write((uint)Type);
            stream.Write(MinBet);
            stream.Write((byte)TableMatch.Status);//state
            stream.Write((ulong)TableMatch.TableBet);//money
            stream.Write((byte)Players.Count);
            foreach (var user in Players.GetValues())
            {
                if (user.PokerInfo == null)
                    stream.ZeroFill(6);
                else
                {
                    stream.Write(user.Player.UID);
                    stream.Write((byte)user.PokerInfo.Location);
                    stream.Write((byte)1);
                }
            }
            stream.Finalize(GamePackets.PokerTable);
            return stream;
        }
        public uint IndexInScreen { get; set; }
        public Role.MapObjectType ObjType { get; set; }

        public unsafe void Send(ServerSockets.Packet msg)
        {
            foreach (var user in Players.Values)
                user.Send(msg);
            foreach (var user in Watchers.Values)
                user.Send(msg);
        }

    }
}
