using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
   public static class MsgPokerPlayerInfo
    {
       public static unsafe ServerSockets.Packet PokerPlayerInfoCreate(this ServerSockets.Packet stream, PlayerInfo user)
       {
           stream.InitWriter();
           stream.Write((byte)1);
           stream.Write((ushort)user.State);
           stream.Write((ushort)user.Location);
           stream.Write(user.Owner.MyPokerTable.Noumber);
           stream.Write(user.Owner.Player.UID);
           stream.Write(0);
           stream.Write(0);
           stream.Write((uint)(user.Owner.MyPokerTable.Noumber > 200 ? 2 : 1));
           stream.Finalize(GamePackets.PokerPlayerInfo);
           return stream;
       }
       
    }
   public class PlayerInfo
   {
       
       public Client.GameClient Owner;
       public bool AllIn = false;
       public bool Check = false;
       public bool Fold = false;

       public bool WasBetted = false;
       public ushort State = 0;
       public uint Location = 0;
       public ulong MyBet = 0;
       public ulong RoundBet = 0;
       public double Rank = 0;
       public bool SetMyBet = false;
       public ulong MyReward = 0;
       public ushort FinishInRound = 0;
       public ulong MyRewardLost = 0;

       public uint UID = 0;
       public MsgPokerHand.PokerCallTypes HandType = MsgPokerHand.PokerCallTypes.Call;

       public List<Card> Cards = new List<Card>();
       public Card DeadCard = null;

       public uint ConquerPoints
       {
           get { return Owner.Player.ConquerPoints; }
       }
       public long Money
       {
           get { return Owner.Player.Money; }
       }
       public PlayerInfo(Client.GameClient _Onwer)
       {
           UID = _Onwer.Player.UID;
           Owner = _Onwer;
       }

   }
}
