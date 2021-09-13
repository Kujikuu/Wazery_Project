using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
   public static class PokerRoundResult
   {
       public static unsafe ServerSockets.Packet PokerRoundResultCreate(this ServerSockets.Packet stream, Table table,List<Client.GameClient> Winners)
       {
           /*26 00 2F 08 0A 00 02 00 00 00 00 D5 78 33 00 1C
0F 05 00 00 00 00 00 00 01 00 0D D9 2D 00 D0 A8
FA FF FF FF FF FF 54 51 53 65 72 76 65 72            ;úÿÿÿÿÿTQServer
            */
           stream.InitWriter();
           if (table.TableMatch.IsHandTable)
           {
               stream.Write((ushort)10);//timer
               stream.Write((ushort)table.TableMatch.PlayersInfo.Count);
               foreach (var user in Winners)
               {
                   stream.ZeroFill(3);
                  // stream.Write((byte)1);
                   //stream.Write((byte)255);
                  // stream.Write((byte)0);
                  // Console.WriteLine(user.Player.Name);
                   stream.Write(user.Player.UID);
                   stream.Write((ulong)(user.PokerInfo.MyReward));
               }
               int index = 1;
               foreach (var user in table.TableMatch.PlayersInfo.Values)
               {
                   if (ContainWinner(user, Winners))
                       continue;
                   bool contin = false;
                 switch (table.Type)
                   {

                       case Table.TableType.Silver:
                           {
                               if (user.Money >= table.MinBet * 10)
                               {
                                   contin = true;
                                   stream.Write((byte)0);
                               }
                               else
                                   stream.Write((byte)1);
                               break;
                           }
                       case Table.TableType.ConquerPoints:
                           {
                               if (user.ConquerPoints >= table.MinBet * 10)
                                   stream.Write((byte)0);
                               else
                                   stream.Write((byte)1);
                               break;
                           }

                   }

                 //  stream.Write((byte)255);
                 if (contin == false)
                     stream.Write((byte)255);
                   else
                     stream.Write((byte)2);
                 stream.Write((byte)0);
                   stream.Write(user.UID);
                //   if(contin)
                  //     stream.Write((ulong)(user.MyRewardLost));
                    //   else
                   stream.Write((ulong)(ulong.MaxValue - user.MyRewardLost));
                   index++;
               }

           }
           else
           {
               stream.Write((ushort)20);//timer
               stream.Write((ushort)table.TableMatch.PlayersInfo.Count);
               foreach (var user in Winners)
               {
                   stream.ZeroFill(3);
                   stream.Write(user.Player.UID);
                   stream.Write((ulong)(user.PokerInfo.MyReward));
               }
               foreach (var user in table.TableMatch.PlayersInfo.Values)
               {
                   if (ContainWinner(user, Winners))
                       continue;
                   bool contin = false;
                   switch (table.Type)
                   {

                       case Table.TableType.Silver:
                           {
                               if (user.Money >= table.MinBet * 10)
                               {
                                   contin = true;
                                   stream.Write((byte)0);
                               }
                               else
                                   stream.Write((byte)1);
                               break;
                           }
                       case Table.TableType.ConquerPoints:
                           {
                               if (user.ConquerPoints >= table.MinBet * 10)
                                   stream.Write((byte)0);
                               else
                                   stream.Write((byte)1);
                               break;
                           }

                   }
                   if (contin == false)
                       stream.Write((byte)255);
                   else
                       stream.Write((byte)2);
                   stream.Write((byte)0);
                   stream.Write(user.UID);
                   stream.Write((ulong)(ulong.MaxValue - user.MyRewardLost));
               }
           }

           stream.Finalize(GamePackets.PokerRoundResult);
        
           return stream;

       }
       public static bool ContainWinner(PlayerInfo user, List<Client.GameClient> winners)
       {
           foreach (var client in winners)
               if (client.Player.UID == user.UID)
                   return true;
           return false;
       }
    }
}
