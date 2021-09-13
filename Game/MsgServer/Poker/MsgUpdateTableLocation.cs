using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
    public static class MsgUpdateTableLocation
    {
        public static unsafe ServerSockets.Packet PokerUpdateTableLocationCreate(this ServerSockets.Packet stream, uint dwparam1, uint dwparam2, uint dwparam3, uint dwparam4)
        {
            stream.InitWriter();
            stream.Write(dwparam1);
            stream.Write(dwparam2);
            stream.Write(dwparam3);
            stream.Write(dwparam4);
            stream.Finalize(GamePackets.PokerUpdateTableLocation);
            return stream;

        }

        [PacketAttribute(GamePackets.PokerUpdateTableLocation)]
        private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
            uint Type = stream.ReadUInt32();
            uint TableID = stream.ReadUInt32();
            uint PlayerID = stream.ReadUInt32();
            uint Location = stream.ReadUInt32();
          
            switch (Type)
            {
                case 0://join table
                    {
                        Role.IMapObj obj;
                        if (user.Player.View.TryGetValue(TableID, out obj, Role.MapObjectType.PokerTable))
                        {
                            var PokerTable = obj as Game.MsgServer.Poker.Table;
                            if (!PokerTable.Players.ContainsKey(user.Player.UID))
                            {
                                if (PokerTable.Noumber >= 200)
                                {
                                    user.CreateBoxDialog("Handtable coming soon.");
                                   // break;
                                }
                                if (PokerTable.PlayersLocation[Math.Min(9, Location)] != null)
                                {
                                    break;
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
                                user.Player.View.SendView(stream.PokerUpdateTableLocationCreate(Type, TableID, PlayerID, Location),true);
                                PokerTable.Send(MsgPokerPlayerInfo.PokerPlayerInfoCreate(stream, user.PokerInfo));
                                foreach (var client in PokerTable.Players.GetValues())
                                {
                                    user.Send(MsgPokerPlayerInfo.PokerPlayerInfoCreate(stream, client.PokerInfo));
                                }
                                user.MyPokerTable.Send(user.MyPokerTable.GetArray(stream, false));
                                user.MyPokerTable.TableMatch.CheckUpStart();
                                user.MyPokerTable.TableMatch.JoinInformations(user);

                             //   if(user.PokerInfo.State == 3)
                             //       user.Send(stream.MsgShowHandLostInfoCreate(PokerTable.TableMatch));
                                user.MyPokerTable.TableMatch.UpdateTableBet();
                            }
                        }
                        break;
                    }
                case 4:////watch table
                    {
                        Role.IMapObj obj;
                        if (user.Player.View.TryGetValue(TableID, out obj, Role.MapObjectType.PokerTable))
                        {
                            
                            var PokerTable = obj as Game.MsgServer.Poker.Table;
                            user.PokerInfo = new PlayerInfo(user);
                            user.MyPokerTable = PokerTable;
                            user.PokerInfo.State = 2;
                            user.PokerInfo.Location = Math.Min(9, Location);
                            PokerTable.Watchers.Add(user.Player.UID, user);

                            user.Player.View.SendView(stream.PokerUpdateTableLocationCreate(Type, TableID, PlayerID, Location), true);
                            PokerTable.Send(MsgPokerPlayerInfo.PokerPlayerInfoCreate(stream, user.PokerInfo));
                            foreach (var client in PokerTable.Players.GetValues())
                            {
                                user.Send(MsgPokerPlayerInfo.PokerPlayerInfoCreate(stream, client.PokerInfo));
                            }
                            user.MyPokerTable.TableMatch.UpdateTableBet();
                            user.MyPokerTable.TableMatch.JoinInformations(user);
                        }
                        break;
                    }
            }
        }
    }
}
