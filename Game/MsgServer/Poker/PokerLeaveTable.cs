using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
   public static class PokerLeaveTable
   {
       public static unsafe ServerSockets.Packet PokerLeaveTableCreate(this ServerSockets.Packet stream, PlayerInfo user)
       {
           stream.InitWriter();
           stream.Write((uint)1);//user.State);
           stream.Write(user.Owner.MyPokerTable.Noumber);
           stream.Write(user.Owner.Player.UID);
           stream.Finalize(GamePackets.PokerLeaveTable);
           return stream;

       }
       [PacketAttribute(GamePackets.PokerLeaveTable)]
       private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
       {
           if (user.MyPokerTable != null && user.PokerInfo != null)
           {
               if (user.MyPokerTable.Players.ContainsKey(user.Player.UID))
               {
                   user.MyPokerTable.TableMatch.CheckKickOnLeave(user);
                   user.Player.View.SendView(stream.PokerLeaveTableCreate(user.PokerInfo),true);
                   user.Player.View.SendView(stream.PokerUpdateTableLocationCreate(1, user.MyPokerTable.UID, user.Player.UID, user.PokerInfo.Location), true);
                   user.MyPokerTable.Players.Remove(user.Player.UID);
                   if(user.MyPokerTable.TableMatch.Players.ContainsKey(user.Player.UID))
                       user.MyPokerTable.TableMatch.Players.Remove(user.Player.UID);
                   user.MyPokerTable.PlayersLocation[user.PokerInfo.Location] = null;
                   user.MyPokerTable.TableMatch.RemovePlayer(user);
                   user.Send(user.MyPokerTable.GetArray(stream, true));
                   if (user.MyPokerTable.TableMatch.Players.Count() < 2)
                   {
                       user.MyPokerTable.TableMatch.Reset();
                   }
                   user.MyPokerTable = null;
                   user.PokerInfo = null;
               }
               else if(user.MyPokerTable.Watchers.ContainsKey(user.Player.UID))
               {

                   user.Player.View.SendView(stream.PokerLeaveTableCreate(user.PokerInfo), true);
                   user.Player.View.SendView(stream.PokerUpdateTableLocationCreate(1, user.MyPokerTable.UID, user.Player.UID, user.PokerInfo.Location), true);
                   user.MyPokerTable.Watchers.Remove(user.Player.UID);
                   user.MyPokerTable = null;
                   user.PokerInfo = null;
               }
           }
       }
    }
}
