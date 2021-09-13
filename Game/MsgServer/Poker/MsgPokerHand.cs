using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
   public static class MsgPokerHand
    {
       public enum PokerCallTypes : ushort
       {
           Bet = 1,
           Call = 2,
           Fold = 4,
           Check = 8,
           Rise = 16,
           AllIn = 32
       }

       public static unsafe ServerSockets.Packet PokerHandCreate(this ServerSockets.Packet stream, PokerCallTypes move_type, ulong bet, ulong Requirebet, uint UID)
       {
           stream.InitWriter();
           stream.Write((ushort)0);
           stream.Write((ushort)move_type);
           stream.Write(bet);
           stream.Write(Requirebet);
           stream.Write(UID);
           stream.Finalize(GamePackets.PokerHand);
           return stream;

       }
       public static unsafe void GetPokerHand(this ServerSockets.Packet stream, out PokerCallTypes move_type, out ulong bet, out ulong Requirebet, out uint UID)
       {
           stream.ReadUInt16();
           move_type = (PokerCallTypes)stream.ReadUInt16();
           bet = stream.ReadUInt64();
           Requirebet = stream.ReadUInt64();
           UID = stream.ReadUInt32();

       }
       public static void SetNextHand(Client.GameClient user, ServerSockets.Packet stream)
       {
           try
           {
               lock (user.MyPokerTable.TableMatch.HandPlayer)
               {
                   user.PokerInfo.FinishInRound = (ushort)user.MyPokerTable.TableMatch.Status;
                   if (user.MyPokerTable.TableMatch.ChechFinishGame)
                   {
                       user.MyPokerTable.TableMatch.Finish();
                       return;
                   }
                   if (user.MyPokerTable.TableMatch.CanChangeRound)
                   {
                       switch (user.MyPokerTable.TableMatch.Status)
                       {
                           case Match.StatusType.Pocket:
                               {
                                   user.MyPokerTable.TableMatch.ChangeRound();
                                   user.MyPokerTable.TableMatch.Status = Match.StatusType.Flop;
                                   break;
                               }
                           case Match.StatusType.Flop:
                               user.MyPokerTable.TableMatch.ChangeRound();
                               user.MyPokerTable.TableMatch.Status = Match.StatusType.Turn;
                               break;
                           case Match.StatusType.Turn:
                               user.MyPokerTable.TableMatch.ChangeRound();
                               user.MyPokerTable.TableMatch.Status = Match.StatusType.River;
                               break;
                           case Match.StatusType.River:
                               {

                                   user.MyPokerTable.TableMatch.Status = Match.StatusType.Showdown;
                                   break;
                               }
                       }
                       if (user.MyPokerTable.TableMatch.Status == Match.StatusType.Showdown)
                           return;
                       user.MyPokerTable.TableMatch.CanCheck = true;
                   }
                   var next_player = user.MyPokerTable.TableMatch.HandPlayer = user.MyPokerTable.TableMatch.GetNextPlayer();
                   next_player.PokerInfo.HandType = PokerCallTypes.Fold;
                   switch (user.MyPokerTable.Type)
                   {
                       case Table.TableType.Silver:
                           {
                               if (next_player.Player.Money >= next_player.MyPokerTable.TableMatch.LastBet)
                               {
                                   if (next_player.MyPokerTable.TableMatch.Status > Match.StatusType.Pocket
                                       && next_player.MyPokerTable.TableMatch.CanCheck)
                                   {
                                       next_player.PokerInfo.HandType |= PokerCallTypes.AllIn;
                                       next_player.PokerInfo.HandType |= PokerCallTypes.Check;
                                       if (next_player.Player.Money >= next_player.MyPokerTable.TableMatch.LastBet)
                                           next_player.PokerInfo.HandType |= PokerCallTypes.Bet;
                                   }
                                   else
                                   {
                                       if (next_player.Player.UID == next_player.MyPokerTable.TableMatch.BigPlayer && next_player.MyPokerTable.TableMatch.LastBet <= (long)next_player.PokerInfo.RoundBet)
                                       {
                                           next_player.PokerInfo.HandType |= PokerCallTypes.Check;
                                           next_player.PokerInfo.HandType |= PokerCallTypes.AllIn;
                                           if (next_player.Player.Money >= next_player.MyPokerTable.TableMatch.LastBet * 2)
                                               next_player.PokerInfo.HandType |= PokerCallTypes.Rise;
                                       }
                                       else
                                       {
                                           if (next_player.Player.Money >= next_player.MyPokerTable.TableMatch.LastBet)
                                               next_player.PokerInfo.HandType |= PokerCallTypes.Call;

                                           next_player.PokerInfo.HandType |= PokerCallTypes.AllIn;


                                           if (next_player.Player.Money >= next_player.MyPokerTable.TableMatch.LastBet * 2)
                                               next_player.PokerInfo.HandType |= PokerCallTypes.Rise;
                                       }

                                   }
                               }
                               else
                               {
                                   if (next_player.MyPokerTable.TableMatch.Status > Match.StatusType.Pocket && next_player.PokerInfo.MyBet >= (ulong)next_player.MyPokerTable.TableMatch.LastBet
                                       && next_player.MyPokerTable.TableMatch.CanCheck)
                                       next_player.PokerInfo.HandType |= PokerCallTypes.Check;
                                   next_player.PokerInfo.HandType |= PokerCallTypes.AllIn;
                               }
                               next_player.PokerInfo.HandType |= PokerCallTypes.Fold;
                               next_player.MyPokerTable.TableMatch.HandStamp = Extensions.Time32.Now.AddSeconds(15);
                               next_player.MyPokerTable.Send(stream.PokerPlayerTurnCreate((ushort)next_player.MyPokerTable.TableMatch.GetHandSecounds, (ushort)next_player.PokerInfo.HandType, (ulong)next_player.MyPokerTable.TableMatch.LastBet, (ulong)(next_player.Player.Money), next_player.Player.UID));
                               break;
                           }
                       case Table.TableType.ConquerPoints:
                           {
                               if (next_player.Player.ConquerPoints >= next_player.MyPokerTable.TableMatch.LastBet)
                               {
                                   if (next_player.MyPokerTable.TableMatch.Status > Match.StatusType.Pocket
                                      && next_player.MyPokerTable.TableMatch.CanCheck)
                                   {
                               //        next_player.PokerInfo.HandType |= PokerCallTypes.AllIn;
                                       next_player.PokerInfo.HandType |= PokerCallTypes.Check;
                                       if (next_player.Player.ConquerPoints >= next_player.MyPokerTable.TableMatch.LastBet)
                                           next_player.PokerInfo.HandType |= PokerCallTypes.Bet;
                                   }
                                   else
                                   {
                                       if (next_player.Player.UID == next_player.MyPokerTable.TableMatch.BigPlayer && next_player.MyPokerTable.TableMatch.LastBet <= (long)next_player.PokerInfo.RoundBet)
                                       {
                                           next_player.PokerInfo.HandType |= PokerCallTypes.Check;
                                //           next_player.PokerInfo.HandType |= PokerCallTypes.AllIn;
                                           if (next_player.Player.ConquerPoints >= next_player.MyPokerTable.TableMatch.LastBet * 2)
                                               next_player.PokerInfo.HandType |= PokerCallTypes.Rise;
                                       }
                                       else
                                       {
                                           if (next_player.Player.ConquerPoints >= next_player.MyPokerTable.TableMatch.LastBet)
                                               next_player.PokerInfo.HandType |= PokerCallTypes.Call;
                             //              next_player.PokerInfo.HandType |= PokerCallTypes.AllIn;


                                           if (next_player.Player.ConquerPoints >= next_player.MyPokerTable.TableMatch.LastBet * 2)
                                               next_player.PokerInfo.HandType |= PokerCallTypes.Rise;

                                       }
                                   }
                               }
                               else
                               {
                                   if (next_player.MyPokerTable.TableMatch.Status > Match.StatusType.Pocket && next_player.PokerInfo.MyBet >= (ulong)next_player.MyPokerTable.TableMatch.LastBet
                                       && next_player.MyPokerTable.TableMatch.CanCheck)
                                       next_player.PokerInfo.HandType |= PokerCallTypes.Check;
                                   next_player.PokerInfo.HandType |= PokerCallTypes.AllIn;
                               }
                               next_player.PokerInfo.HandType |= PokerCallTypes.Fold;
                               next_player.MyPokerTable.TableMatch.HandStamp = Extensions.Time32.Now.AddSeconds(15);
                               next_player.MyPokerTable.Send(stream.PokerPlayerTurnCreate((ushort)next_player.MyPokerTable.TableMatch.GetHandSecounds, (ushort)next_player.PokerInfo.HandType, (ulong)next_player.MyPokerTable.TableMatch.LastBet, (ulong)(next_player.Player.ConquerPoints), next_player.Player.UID));
                               break;
                           }
                   }
                  
               }
           }
           catch (Exception e) { Console.WriteLine(e.ToString()); }
       }
       [PacketAttribute(GamePackets.PokerHand)]
       private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
       {
           PokerCallTypes move_type;
           ulong bet;
           ulong Requirebet;
           uint UID;
           stream.GetPokerHand(out move_type, out bet, out Requirebet, out UID);
           switch (move_type)
           {
               case PokerCallTypes.Bet:
                   {
                       if (user.PokerInfo != null && user.MyPokerTable != null && user.MyPokerTable.TableMatch.Players.ContainsKey(user.Player.UID))
                       {
                           if (user.MyPokerTable.TableMatch.HandPlayer == null)
                               break;
                           if (user.MyPokerTable.TableMatch.HandPlayer.Player.UID != user.Player.UID)
                               break;

                           switch (user.MyPokerTable.Type)
                           {
                               case Table.TableType.Silver:
                                   {
                                       if (bet == 0)
                                           bet = (ulong)user.MyPokerTable.TableMatch.LastBet;

                                       if (user.Player.Money >= (long)bet)
                                       {
                                           user.MyPokerTable.TableMatch.CanCheck = false;
                                           user.PokerInfo.WasBetted = true;
                                           user.Player.Money -= (long)bet;
                                           user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                           user.MyPokerTable.TableMatch.TableBet += (long)bet;
                                           user.MyPokerTable.TableMatch.LastBet = (long)bet;
                                           user.PokerInfo.MyBet += bet;
                                           user.PokerInfo.RoundBet += bet;
                                           user.MyPokerTable.TableMatch.UpdateTableBet();
                                           user.MyPokerTable.Send(stream.PokerHandCreate(move_type, (ulong)user.MyPokerTable.TableMatch.LastBet, (ulong)user.PokerInfo.MyBet, user.Player.UID));
                                           SetNextHand(user, stream);
                                       }
                                       break;
                                   }
                               case Table.TableType.ConquerPoints:
                                   {
                                       if (bet == 0)
                                           bet = (ulong)user.MyPokerTable.TableMatch.LastBet + +user.MyPokerTable.MinBet;
                                  //     if (bet >= (ulong)user.MyPokerTable.TableMatch.TableBet)
                                    //       bet = (ulong)user.MyPokerTable.TableMatch.TableBet;
                                       if (user.Player.ConquerPoints >= (uint)bet)
                                       {
                                           user.MyPokerTable.TableMatch.CanCheck = false;
                                           user.PokerInfo.WasBetted = true;
                                           user.Player.ConquerPoints -= (uint)bet;
                                          
                                           user.MyPokerTable.TableMatch.TableBet += (uint)bet;
                                           user.MyPokerTable.TableMatch.LastBet = (long)bet;
                                           user.PokerInfo.MyBet += bet;
                                           user.PokerInfo.RoundBet += bet;
                                           user.MyPokerTable.TableMatch.UpdateTableBet();
                                           user.MyPokerTable.Send(stream.PokerHandCreate(move_type, (ulong)user.MyPokerTable.TableMatch.LastBet, (ulong)user.PokerInfo.MyBet, user.Player.UID));
                                           SetNextHand(user, stream);
                                       }
                                       break;
                                   }
                           }
                       }
                       break;
              
                   }
               case PokerCallTypes.AllIn:
                   {
                       if (user.PokerInfo != null && user.MyPokerTable != null && user.MyPokerTable.TableMatch.Players.ContainsKey(user.Player.UID))
                       {
                           if (user.MyPokerTable.TableMatch.HandPlayer == null)
                               break;
                           if (user.MyPokerTable.TableMatch.HandPlayer.Player.UID != user.Player.UID)
                               break;

                           switch (user.MyPokerTable.Type)
                           {
                               case Table.TableType.Silver:
                                   {
                                       bet = (ulong)user.Player.Money;

                                       if (user.Player.Money >= (long)bet)
                                       {
                                           user.MyPokerTable.TableMatch.CanCheck = false;
                                           user.PokerInfo.WasBetted = true;
                                           user.PokerInfo.AllIn = true;

                                           user.Player.Money = 0;
                                           user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                           user.MyPokerTable.TableMatch.TableBet += (long)bet;
                                           if ((long)bet > user.MyPokerTable.TableMatch.LastBet)
                                               user.MyPokerTable.TableMatch.LastBet = (long)bet;
                                           user.PokerInfo.MyBet += bet;
                                           user.PokerInfo.RoundBet += bet;
                                           user.MyPokerTable.TableMatch.UpdateTableBet();
                                           user.MyPokerTable.Send(stream.PokerHandCreate(move_type, (ulong)bet, (ulong)user.PokerInfo.MyBet, user.Player.UID));
                                           SetNextHand(user, stream);
                                       }
                                       break;
                                   }
                               case Table.TableType.ConquerPoints:
                                   {
                                       bet = user.Player.ConquerPoints;
                                       if (user.Player.ConquerPoints >= (uint)bet)
                                       {
                                           user.MyPokerTable.TableMatch.CanCheck = false;
                                           user.PokerInfo.AllIn = true;
                                           user.PokerInfo.WasBetted = true;
                                           user.Player.ConquerPoints = 0;
             
                                           user.MyPokerTable.TableMatch.TableBet += (uint)bet;
                                           if ((long)bet > user.MyPokerTable.TableMatch.LastBet)
                                               user.MyPokerTable.TableMatch.LastBet = (long)bet;
                                           user.PokerInfo.MyBet += bet;
                                           user.PokerInfo.RoundBet += bet;
                                           user.MyPokerTable.TableMatch.UpdateTableBet();
                                           user.MyPokerTable.Send(stream.PokerHandCreate(move_type, (ulong)bet, (ulong)user.PokerInfo.MyBet, user.Player.UID));
                                           SetNextHand(user, stream);
                                       }
                                       break;
                                   }
                           }
                       }
                       break;
                   }
               case PokerCallTypes.Fold:
                   {

                       if (user.PokerInfo != null && user.MyPokerTable != null && user.MyPokerTable.TableMatch.Players.ContainsKey(user.Player.UID))
                       {
                           if (user.MyPokerTable.TableMatch.HandPlayer == null)
                               break;
                           if (user.MyPokerTable.TableMatch.HandPlayer.Player.UID != user.Player.UID)
                               break;

                           user.MyPokerTable.TableMatch.Fold(user);
                       }
                       break;
                   }
               case PokerCallTypes.Check:
                   {
                       if (user.PokerInfo != null && user.MyPokerTable != null && user.MyPokerTable.TableMatch.Players.ContainsKey(user.Player.UID))
                       {
                           if (user.MyPokerTable.TableMatch.HandPlayer == null)
                               break;
                           if (user.MyPokerTable.TableMatch.HandPlayer.Player.UID != user.Player.UID)
                               break;

                           user.PokerInfo.Check = true;
                           user.PokerInfo.WasBetted = true;
                           user.MyPokerTable.Send(stream.PokerHandCreate(move_type, (ulong)user.MyPokerTable.TableMatch.LastBet, (ulong)user.PokerInfo.MyBet, user.Player.UID));
                           SetNextHand(user, stream);
                       }

                       break;
                   }
               case PokerCallTypes.Rise:
                   {
                       if (user.PokerInfo != null && user.MyPokerTable != null && user.MyPokerTable.TableMatch.Players.ContainsKey(user.Player.UID))
                       {
                           if (user.MyPokerTable.TableMatch.HandPlayer == null)
                               break;
                           if (user.MyPokerTable.TableMatch.HandPlayer.Player.UID != user.Player.UID)
                               break;

                           switch (user.MyPokerTable.Type)
                           {
                               case Table.TableType.Silver:
                                   {
                                       if (bet == 0)
                                           bet = (ulong)(user.MyPokerTable.TableMatch.LastBet);
                                       
                                           bet = (ulong)((long)user.MyPokerTable.TableMatch.LastBet + (long)bet);
                                      // if (user.Player.Money >= (long)bet * 2)
                                      //     bet = bet * 2;

                                       if (user.Player.Money >= (long)bet)
                                       {
                                           user.MyPokerTable.TableMatch.CanCheck = false;
                                           user.PokerInfo.WasBetted = true;
                                           user.Player.Money -= (long)bet;
                                           user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                           user.MyPokerTable.TableMatch.TableBet += (long)bet;
                                           user.MyPokerTable.TableMatch.LastBet = (long)bet;
                                           user.PokerInfo.MyBet += bet;
                                           user.PokerInfo.RoundBet += bet;
                                           user.MyPokerTable.TableMatch.UpdateTableBet();
                                           user.MyPokerTable.Send(stream.PokerHandCreate(move_type, (ulong)user.MyPokerTable.TableMatch.LastBet, (ulong)user.PokerInfo.MyBet, user.Player.UID));
                                           SetNextHand(user, stream);
                                       }
                                       break;
                                   }
                               case Table.TableType.ConquerPoints:
                                   {
                                   //    Console.WriteLine(user.MyPokerTable.TableMatch.TableBet); 
                                       if (bet == 0)
                                           bet = (ulong)(user.MyPokerTable.TableMatch.LastBet + user.MyPokerTable.MinBet);
                                 //      else
                                   //        bet = (ulong)((long)user.MyPokerTable.TableMatch.LastBet + (long)bet);
                                  //     if (user.Player.ConquerPoints >= bet * 2)
                                   //        bet = bet * 2;
                                   //    if (bet >= (ulong)user.MyPokerTable.MinBet * 2)
                                     //      bet = (ulong)user.MyPokerTable.MinBet * 2;
                                       if (user.Player.ConquerPoints >= (uint)bet )//&& bet >= user.MyPokerTable.MinBet)
                                       {
                                           user.MyPokerTable.TableMatch.CanCheck = false;
                                           user.PokerInfo.WasBetted = true;
                                           user.Player.ConquerPoints -= (uint)bet;
                                         
                                           user.MyPokerTable.TableMatch.TableBet += (uint)bet;
                                           user.MyPokerTable.TableMatch.LastBet = (long)bet;
                                           user.PokerInfo.MyBet += bet;
                                           user.PokerInfo.RoundBet += bet;
                                           user.MyPokerTable.TableMatch.UpdateTableBet();
                                           user.MyPokerTable.Send(stream.PokerHandCreate(move_type, (ulong)user.MyPokerTable.TableMatch.LastBet, (ulong)user.PokerInfo.MyBet, user.Player.UID));
                                           SetNextHand(user, stream);
                                       }
                                       break;
                                   }
                           }
                       }
                       break;
                   }
               case PokerCallTypes.Call:
                   {
                       if (user.PokerInfo != null && user.MyPokerTable != null && user.MyPokerTable.TableMatch.Players.ContainsKey(user.Player.UID))
                       {
                           if (user.MyPokerTable.TableMatch.HandPlayer == null)
                               break;
                           if (user.MyPokerTable.TableMatch.HandPlayer.Player.UID != user.Player.UID)
                               break;

                           switch (user.MyPokerTable.Type)
                           {
                               case Table.TableType.Silver:
                                   {
                                       if (bet == 0)
                                           bet = (ulong)user.MyPokerTable.TableMatch.LastBet;
                                       if (user.MyPokerTable.TableMatch.Status == Match.StatusType.Pocket)
                                       {
                                           if (user.MyPokerTable.TableMatch.SmallPlayer == user.Player.UID)
                                           {
                                               if (user.MyPokerTable.TableMatch.LastBet == user.MyPokerTable.MinBet)
                                                   bet = bet / 2;
                                           }
                                       }
                                       if (user.Player.Money >= (long)bet)
                                       {
                                           user.MyPokerTable.TableMatch.CanCheck = false;
                                           user.PokerInfo.WasBetted = true;
                                           user.Player.Money -= (long)bet;
                                           user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                           user.MyPokerTable.TableMatch.TableBet += (long)bet;
                                           user.MyPokerTable.TableMatch.LastBet = (long)bet;
                                           user.PokerInfo.MyBet += bet;
                                           user.PokerInfo.RoundBet += bet;
                                           user.MyPokerTable.TableMatch.UpdateTableBet();
                                           user.MyPokerTable.Send(stream.PokerHandCreate(move_type, (ulong)user.MyPokerTable.TableMatch.LastBet, (ulong)user.PokerInfo.MyBet, user.Player.UID));
                                           SetNextHand(user, stream);
                                       }
                                       break;
                                   }
                               case Table.TableType.ConquerPoints:
                                   {
                                     
                                       if (bet == 0)
                                           bet = (ulong)user.MyPokerTable.TableMatch.LastBet;
                                       if (user.MyPokerTable.TableMatch.Status == Match.StatusType.Pocket)
                                       {
                                           if (user.MyPokerTable.TableMatch.SmallPlayer == user.Player.UID)
                                           {
                                               if (user.MyPokerTable.TableMatch.LastBet == user.MyPokerTable.MinBet)
                                                   bet = bet / 2;
                                           }
                                       }
                                      // if (bet >= (ulong)user.MyPokerTable.TableMatch.TableBet)
                                        //   bet = (ulong)user.MyPokerTable.TableMatch.TableBet;
                                       if (user.Player.ConquerPoints >= (uint)bet)
                                       {
                                           user.MyPokerTable.TableMatch.CanCheck = false;
                                           user.PokerInfo.WasBetted = true;
                                           user.Player.ConquerPoints -= (uint)bet;
                                      
                                           user.MyPokerTable.TableMatch.TableBet += (uint)bet;
                                           user.MyPokerTable.TableMatch.LastBet = (long)bet;
                                           user.PokerInfo.MyBet += bet;
                                           user.PokerInfo.RoundBet += bet;
                                           user.MyPokerTable.TableMatch.UpdateTableBet();
                                           user.MyPokerTable.Send(stream.PokerHandCreate(move_type, (ulong)user.MyPokerTable.TableMatch.LastBet, (ulong) user.PokerInfo.MyBet, user.Player.UID));
                                           SetNextHand(user, stream);
                                       }
                                       break;
                                   }
                           }
                       }

                       break;
                   }

           }
       }
    }
}
