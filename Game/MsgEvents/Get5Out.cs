using LightConquer_Project.Client;
using LightConquer_Project.Role;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightConquer_Project.Game.MsgEvents
{
    class Get5Out : Events
    {
        public Get5Out()
        {
            EventTitle = "Five(n)Out";
            Duration = 35;
            BaseMap = 700;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            FriendlyFire = true;
            AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 180;
            PotionsAllowed = false;
        }

        public override bool AddPlayer(GameClient client)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                if (Stage == EventStage.Inviting)
                {
                    if (!GameMap.EventMaps.ContainsKey(client.Player.Map))
                    {
                        if (!client.InQualifier())
                        {
                            if (client.Player.Map != 1038 && client.Player.Map != 6001)
                            {
                                if (!PlayerList.ContainsKey(client.Player.UID))
                                {
                                    //Map.GetRandCoord(ref X, ref Y);
                                    client.Player.PMap = client.Player.Map;
                                    client.Player.PMapX = client.Player.X;
                                    client.Player.PMapY = client.Player.Y;
                                    client.Teleport(53, 65, 1616);
                                    PlayerList.Add(client.Player.UID, client);
                                    PlayerScores.Add(client.Player.UID, 5);
                                    //client.EventBase = Program.Events[0];
                                    client.SendSysMesage($"You have sucessfully joined the {EventTitle} Event!");
                                    if (!client.Player.Alive)
                                        client.Player.Revive(stream);
                                    return true;
                                }
                                else
                                    client.SendSysMesage($"You can't join a PVP event while you're in {EventTitle} Event!");
                            }
                            else
                                client.SendSysMesage("You can't join a PVP event while you're in guild war.");
                        }
                        else
                            client.SendSysMesage("You can't join a PVP Event while you're fighting at the Arena Qualifier!");
                    }
                    else
                        client.SendSysMesage("You can't join a PVP Event while you're fighting at other event!");
                }
                else
                    client.SendSysMesage("There are no events running");
            }
            return false;
        }

        public override void End()
        {
            //DisplayScore();
            byte NO = 1;
            foreach (var player in PlayerScores.OrderByDescending(s => s.Value).ToList())
            {
                if (NO == 1)
                {
                    Reward(PlayerList[player.Key]);
                    RemovePlayer(PlayerList[player.Key]);
                    NO++;
                }
                else
                {
                    if (PlayerList.ContainsKey(player.Key))
                    {
                        RemovePlayer(PlayerList[player.Key]);
                        NO++;
                    }
                }
            }

            PlayerList.Clear();
            PlayerScores.Clear();
            return;
        }

        public override void Reward(GameClient client)
        {
            //client.Player.BoundConquerPoints += 50;
            //client.Player.ConquerPoints += 7000;
            client.Player.Get5Out = 1;

            base.Reward(client);
        }
        public void DisplayScore(GameClient client)
        {
            foreach (var player in PlayerList.Values)
            {
                if (player == client)
                    //Broadcast($"You have [{PlayerScores[player.Player.UID]}] Hits Left.", BroadCastLoc.Title);
                    player.SendSysMesage($"You have [{PlayerScores[player.Player.UID]}] Hits Left.", MsgServer.MsgMessage.ChatMode.FirstRightCorner);
            }
            TimeSpan T = TimeSpan.FromSeconds(_Duration);
            //Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            if (_Duration > 0)
                --_Duration;
        }
        public override void DisplayScore()
        {
            
        }
        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (PlayerScores.ContainsKey(Victim.Player.UID) && PlayerScores[Victim.Player.UID] > 1)
                PlayerScores[Victim.Player.UID]--;
            else
                RemovePlayer(Victim);
            DisplayScore(Attacker);
            DisplayScore(Victim);
        }
    }
}
