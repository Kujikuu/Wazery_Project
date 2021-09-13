using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
   public class MsgPokerJoinTable
    {


       [PacketAttribute(GamePackets.PokerPlayerInfo)]
       private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
       {
           stream.ReadUInt8();
           ushort Type = stream.ReadUInt16();
           uint Location = stream.ReadUInt8();

           switch (Type)
           {
               case 1:
                   {
                       var PokerTable = user.MyPokerTable;
                       if (!PokerTable.Players.ContainsKey(user.Player.UID))
                       {
                           if (PokerTable.PlayersLocation[Math.Min(9, Location)] != null)
                           {
                            
                               break;
                           }
                           if (PokerTable.Noumber >= 200)
                           {
                               user.CreateBoxDialog("Handtable coming soon.");
                             //  break;
                           }
                           bool was_watcher = false;
                           if (user.MyPokerTable.Watchers.ContainsKey(user.Player.UID))
                           {
                               was_watcher = true;

                           //    user.Player.View.SendView(stream.PokerLeaveTableCreate(user.PokerInfo), true);
                           //    user.Player.View.SendView(stream.PokerUpdateTableLocationCreate(1, user.MyPokerTable.UID, user.Player.UID, user.PokerInfo.Location), true);
                               user.MyPokerTable.Watchers.Remove(user.Player.UID);
                           }

                           user.PokerInfo = new PlayerInfo(user);
                           user.MyPokerTable = PokerTable;
                           PokerTable.Players.Add(user.Player.UID, user);
                           PokerTable.PlayersLocation[Math.Min(9, Location)] = user;
                           if (PokerTable.TableMatch.IsStarted)
                               user.PokerInfo.State = 3;//waiting
                           else
                               user.PokerInfo.State = 1;

                           user.PokerInfo.Location = Math.Min(9, Location);
                           user.Player.View.SendView(stream.PokerUpdateTableLocationCreate(Type, user.MyPokerTable.UID, user.Player.UID, Location), true);
                           PokerTable.Send(MsgPokerPlayerInfo.PokerPlayerInfoCreate(stream, user.PokerInfo));
                           foreach (var client in PokerTable.Players.GetValues())
                           {
                               user.Send(MsgPokerPlayerInfo.PokerPlayerInfoCreate(stream, client.PokerInfo));
                           }
                           user.MyPokerTable.Send(user.MyPokerTable.GetArray(stream, false));
                           user.MyPokerTable.TableMatch.CheckUpStart();

                           user.MyPokerTable.TableMatch.JoinInformations(user, was_watcher);
                        //   if (user.PokerInfo.State == 3)
                           //    user.Send(stream.MsgShowHandLostInfoCreate(PokerTable.TableMatch));
                       }
                       break;
                   }
           }
       }
    }
}
