using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
  public static  class MsgShowHandLostInfo
    {
      public static unsafe ServerSockets.Packet MsgShowHandLostInfoCreate(this ServerSockets.Packet stream, Match match)
        {
            stream.InitWriter();
            if (match.IsHandTable)
            {
                stream.Write((ushort)0);
                stream.Write((byte)match.AvailablePlayers.Where(p => p.PokerInfo.Fold == false).Count());
                stream.Write((ushort)8);//??? turn timer
                stream.ZeroFill(10);
                if (match.HandPlayer != null)
                    stream.Write(match.HandPlayer.Player.UID);
                else
                    stream.Write(0);
                stream.Write(match.DealerUID);//starter or dealer?
                stream.Write(0);
                stream.Write(0);

               
                foreach (var user in match.AvailablePlayers.Where(p => p.PokerInfo.Fold == false))
                {
                    stream.Write(user.Player.UID);
                    stream.Write((byte)user.PokerInfo.Cards.Count);//2 cards

                    //card 1
                    stream.Write((byte)4);
                    stream.Write((byte)0x0D);
                    stream.Write((byte)0);

                    bool first = true;
                    foreach (var card in user.PokerInfo.Cards)
                    {
                        if (first)
                        {
                            first = false;

                            continue;
                        }
                        stream.Write((byte)card.GameCardType);
                        stream.Write((byte)card.GameCardID);
                       
                        stream.Write((byte)1);//show
                    }
                    stream.Write((ushort)0);
                    stream.Write(2266784752);//??????????? hash cards ??
                }


            }
            else
            {
                stream.Write((ushort)0);
                stream.Write((byte)match.AvailablePlayers.Where(p => p.PokerInfo.Fold == false).Count());
                stream.Write((ushort)0);//??? turn timer
                stream.ZeroFill(10);
                if (match.HandPlayer != null)
                    stream.Write(match.HandPlayer.Player.UID);
                else
                    stream.Write(0);
                stream.Write(match.DealerUID);//starter or dealer?
                stream.Write(match.SmallPlayer);
                stream.Write(match.BigPlayer);

                stream.ZeroFill(10);//unknow
                foreach (var user in match.AvailablePlayers.Where(p => p.PokerInfo.Fold == false))
                {
                    stream.Write((byte)2);//2 cards

                    //card 1
                    stream.Write((byte)4);
                    stream.Write((byte)0x0D);
                    stream.Write((byte)0);
                    //card 2
                    stream.Write((byte)4);
                    stream.Write((byte)0x0D);
                    stream.Write((byte)0);

                    stream.Write(user.Player.UID);
                }
            }
            stream.Finalize(GamePackets.MsgShowHandLostInfo);
           
            return stream;
        }
    }
}
