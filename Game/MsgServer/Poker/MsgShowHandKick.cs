using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
   public static class MsgShowHandKick
    {
       public static unsafe ServerSockets.Packet MsgShowHandKickCreate(this ServerSockets.Packet stream, ActionType action, uint dwparam1, uint dwparam2, uint dwparam3, byte dwparam4)
        {
            stream.InitWriter();
            stream.Write((byte)action);
            stream.Write(dwparam1);
            stream.Write(dwparam2);
            stream.Write(dwparam3);
            stream.Write(dwparam4);
            stream.Finalize(GamePackets.MsgShowHandKick);
            return stream;

        }
        public enum ActionType : byte
        {
            StartKick = 0,
            SendKick =1,
            AcceptKick = 2,
            Kicked =3, 
            StayHere = 4
        }

        [PacketAttribute(GamePackets.MsgShowHandKick)]
        private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
       //     Console.PrintPacketAdvanced(stream.Memory, stream.Size);

            ActionType Type = (ActionType)stream.ReadUInt8();
            uint dwparam1 = stream.ReadUInt32();
            uint dwparam2 = stream.ReadUInt32();
            uint dwparam3 = stream.ReadUInt32();
            byte Accept = stream.ReadUInt8();
            switch (Type)
            {
                case ActionType.StartKick:
                    {
                        if (user.MyPokerTable == null)
                            return;
                        if (user.MyPokerTable.TableMatch.CanKick == false || user.MyPokerTable.TableMatch.IsStartedKick)
                            return;
                        if (user.MyPokerTable.Players.Count > 2)
                        {
                            Client.GameClient client;
                            if (user.MyPokerTable.Players.TryGetValue(dwparam2, out client))
                            {
                                user.MyPokerTable.ReceiveKick = client;
                                user.MyPokerTable.StarterKick = user.Player.UID;
                                user.MyPokerTable.TableMatch.IsStartedKick = true;
                                user.MyPokerTable.PlayersAcceptKick.Add(user.Player.UID);
                                user.MyPokerTable.Send(stream.MsgShowHandKickCreate(ActionType.SendKick, user.Player.UID, client.Player.UID, (uint)user.MyPokerTable.TableMatch.GetTurnTimer, 0));
                            }
                        }
                        break;
                    }
                case ActionType.AcceptKick:
                    {
                        if (user.MyPokerTable == null)
                            return;

                         Client.GameClient client;
                         if (user.MyPokerTable.Players.TryGetValue(dwparam2, out client))
                         {
                         
                             foreach (var item in user.MyPokerTable.PlayersAcceptKick.GetValues())
                                 if (item == user.Player.UID)
                                     return;

                             if (Accept == 1)
                             {
                                 user.MyPokerTable.PlayersAcceptKick.Add(user.Player.UID);
                             }
                             user.MyPokerTable.Send(stream.MsgShowHandKickCreate(ActionType.AcceptKick, user.MyPokerTable.StarterKick, client.Player.UID, user.Player.UID, Accept));
                         }
                        break;
                    }
            }
        }
    }
}
