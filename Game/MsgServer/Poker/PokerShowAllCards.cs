using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
   public static class PokerShowAllCards
    {
       public static unsafe ServerSockets.Packet PokerShowAllCardsCreate(this ServerSockets.Packet stream, Table table, Client.GameClient[] array)
       {
           stream.InitWriter();
           stream.Write((ushort)0);
           stream.Write((ushort)array.Length);
           if (table.TableMatch.IsHandTable)
           {


               bool first = true;
               foreach (var user in array)
               {
                   string MyCard = "";
                   int cards = 0;
                   foreach (var card in user.PokerInfo.Cards)
                   {
                       MyCard += card.StrCard  + " ";
                   }
                 ulong mask =  Hand.ParseHand(MyCard + "", ref cards);


                 stream.Write((ushort)(user.PokerInfo.Cards[0].GameCardID));
                 stream.Write((ushort)28576);//???
                 stream.Write((ushort)(user.PokerInfo.Cards[0].GameCardType));
                 stream.Write((ushort)(0));
                 stream.Write(user.Player.UID);

                    //stream.Write(10592780300);

                    if (first)
                    {
                        stream.Write((ushort)5);//(user.PokerInfo.Cards[0].GameCardID));
                        stream.Write((ushort)28576);//???
                        stream.Write((ushort)(2));
                        stream.Write((ushort)(0));
                        stream.Write(user.Player.UID);
                        first = false;
                    }
                    else
                    {
                        stream.Write((ushort)6);//(user.PokerInfo.Cards[0].GameCardID));
                        stream.Write((ushort)28576);//???
                        stream.Write((ushort)(1));
                        stream.Write((ushort)(0));
                        stream.Write(user.Player.UID);
                    }
                    //stream.Write(mask);
                }
    
           }
           else
           {
               foreach (var user in array)
               {
                   foreach (var card in user.PokerInfo.Cards)
                       stream.Write((ushort)card.GameCardID);
                   foreach (var card in user.PokerInfo.Cards)
                       stream.Write((ushort)card.GameCardType);
                   stream.Write(user.Player.UID);
               }
           }

           stream.Finalize(GamePackets.PokerShowAllCards);
           
           return stream;
       }




    }
}
