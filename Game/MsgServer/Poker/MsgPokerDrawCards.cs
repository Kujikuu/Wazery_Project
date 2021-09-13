using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
    public static class MsgPokerDrawCards
    {
        public static unsafe ServerSockets.Packet PokerDrawCardsCreate(this ServerSockets.Packet stream, ushort Type, ushort Type2, Match _match, uint Small =0, uint big = 0)
        {
       
            stream.InitWriter();
            if (_match.IsHandTable)
            {
                /*5C 00 2B 08 00 00 06 00 03 00 00 00 00 00 08 00      ;\ +         
00 00 00 00 00 00 E8 00 00 00 00 00 00 00 06 00      ;      è        
89 2E 33 00 00 00 00 00 00 00 00 00 09 00 02 00      ;.3         	  
3D 80 33 00 0A 00 01 00 89 2E 33 00 0C 00 03 00      ;=3 
  .3   
3D 80 33 00 07 00 02 00 89 2E 33 00 0C 00 01 00      ;=3   .3   
3D 80 33 00 08 00 01 00 89 2E 33 00 54 51 53 65      ;=3   .3 TQSe
72 76 65 72                                          ;rver*/
                stream.Write((ushort)0);
                stream.Write(6);
                stream.Write((ushort)3);
                stream.Write((uint)0);
                stream.Write((uint)8);
                stream.Write((uint)0);
                stream.Write((uint)236);
                stream.Write((uint)0);
                stream.Write((ushort)_match.Players.Count);
                stream.Write(_match.DealerUID);//starter or dealer?
                stream.Write(0);
                stream.Write(0);//??
                foreach (var user in _match.Players.GetValues())
                {
                    bool first = true;
                    foreach (var _card in user.PokerInfo.Cards)
                    {
                        if (first)
                        {
                            first = false;
                            continue;
                        }
                        stream.Write(_card.GameCardID);
                        stream.Write((ushort)_card.GameCardType);
                    }
                    stream.Write(user.Player.UID);
                }
            }
            else
            {
                stream.Write(Type);
                stream.Write(Type2);
                stream.ZeroFill(22);
                stream.Write((ushort)_match.Players.Count);
                stream.Write(_match.DealerUID);//starter or dealer?
                stream.Write(Small);
                stream.Write(big);//??
                foreach (var user in _match.Players.GetValues())
                {
                    foreach (var _card in user.PokerInfo.Cards)
                    {
                        stream.Write(_card.GameCardID);
                        stream.Write((ushort)_card.GameCardType);
                    }
                    stream.Write(user.Player.UID);
                }
            }
            stream.Finalize(GamePackets.PokerDrawCards);
            return stream;
        }
        public static unsafe ServerSockets.Packet ShowUnknowCard(this ServerSockets.Packet stream, Table table, PlayerInfo user)
        {
            stream.InitWriter();
            if (table.TableMatch.IsHandTable)
            {
                stream.Write((ushort)0);
                stream.Write((ushort)5);//6
                stream.Write((ushort)1);
                if (table.Watchers.ContainsKey(user.UID) || user.State == 3)
                {
                    stream.Write((uint)13);//unknow cards
                    stream.ZeroFill(6);
                    stream.Write((uint)4);//unknow cards
                }
                else
                {
                    stream.Write((uint)user.Cards[0].GameCardID);//unknow cards
                    stream.ZeroFill(6);
                    stream.Write((uint)user.Cards[0].GameCardType);//unknow cards
                }
                    stream.ZeroFill(6);
                stream.Write((ushort)table.TableMatch.Players.Count);
                stream.Write(table.TableMatch.DealerUID);//starter or dealer?
                stream.Write(0);
                stream.Write(0);//??

                foreach (var player in table.TableMatch.Players.GetValues())
                {
                    stream.Write((ushort)13);//unknow cards
                    stream.Write((ushort)4);//unknow cards
                    stream.Write(player.Player.UID);
                }
            }
            stream.Finalize(GamePackets.PokerDrawCards);
            return stream;
        }
        public static unsafe ServerSockets.Packet PokerDrawCardsCreate(this ServerSockets.Packet stream, Table table, PlayerInfo user)
        {
            stream.InitWriter();
            if (table.TableMatch.IsHandTable)
            {
                stream.Write((ushort)0);
                stream.Write((ushort)6);
                stream.Write((ushort)1);
                stream.Write((uint)0);//cards
                stream.ZeroFill(6);
                stream.Write((uint)0);//cards
                stream.ZeroFill(6);
                stream.Write((ushort)table.TableMatch.Players.Count);
                stream.Write(table.TableMatch.DealerUID);//starter or dealer?
                stream.Write(0);
                stream.Write(0);//??



                foreach (var player in table.TableMatch.AvailablePlayers)
                {
                    stream.Write((ushort)player.PokerInfo.Cards.Last().GameCardID);//unknow cards
                    stream.Write((ushort)player.PokerInfo.Cards.Last().GameCardType);//unknow cards
                    stream.Write(player.Player.UID);
                }
            }
            else
            {
        
                    stream.Write(0);
                    stream.Write((ushort)2);
                    if (table.Watchers.ContainsKey(user.UID) || user.State ==3 )
                    {
                        stream.Write((ushort)13);//unknow cards
                        stream.Write((ushort)13);//unknow cards
                        stream.ZeroFill(6);
                        stream.Write((ushort)4);//unknow cards
                        stream.Write((ushort)4);//unknow cards
                    }
                    else
                    {
                        foreach (var _card in user.Cards)
                            stream.Write(_card.GameCardID);
                        stream.ZeroFill(6);
                        foreach (var _card in user.Cards)
                            stream.Write((ushort)_card.GameCardType);
                    }
                    stream.ZeroFill(6);
                    stream.Write((ushort)table.TableMatch.Players.Count);
                    stream.Write(table.TableMatch.DealerUID);//starter or dealer?
                    stream.Write(table.TableMatch.SmallPlayer);
                    stream.Write(table.TableMatch.BigPlayer);//??

                    foreach (var player in table.TableMatch.Players.GetValues())
                    {
                        stream.Write((ushort)13);//unknow cards
                        stream.Write((ushort)4);//unknow cards
                        stream.Write(player.Player.UID);
                    }
                }
         
           


            stream.Finalize(GamePackets.PokerDrawCards);
            return stream;
        }
        public static unsafe ServerSockets.Packet PokerDrawCardsCreate(this ServerSockets.Packet stream, Table table,Card[] cards,  ushort Type )
        {
            stream.InitWriter();

            stream.Write((ushort)0);
            stream.Write((ushort)Type);
            stream.Write((ushort)cards.Length);
            for (int x = 0; x < 5; x++)
            {
                if (cards.Length > x)
                {
                    var card = cards[x];
                    stream.Write((ushort)card.GameCardID);
                }
                else
                    stream.Write((ushort)0);
            }
            for (int x = 0; x < 5; x++)
            {
                if (cards.Length > x)
                {
                    var card = cards[x];
                    stream.Write((ushort)card.GameCardType);
                }
                else
                    stream.Write((ushort)0);
            }

            stream.Write((ushort)0);
            stream.Write(table.TableMatch.DealerUID);//starter or dealer?
            stream.Write(table.TableMatch.SmallPlayer);
            stream.Write(table.TableMatch.BigPlayer);//??
            stream.Finalize(GamePackets.PokerDrawCards);
            return stream;
        }
    }
}
