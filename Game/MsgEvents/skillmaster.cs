using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightConquer_Project.Client;
using LightConquer_Project.Game.MsgServer;
using static LightConquer_Project.Role.Flags;
using LightConquer_Project.Role;


namespace LightConquer_Project.Game.MsgEvents
{
    public class skillmaster : Events
    {
        private List<uint> Kings = new List<uint>();
        DateTime LastScore;
        public skillmaster()
        {
            EventTitle = "Skill Master";
            Duration = 3;
            BaseMap = 1505;
            //MagicAllowed = false;
            NoDamage = true;
            //AllowedSkills = new List<ushort>() { 1000, 1001, 1002, 1005, 1055, 1075, 1085, 1090,
            //    1095, 1100, 1105, 1120, 1150, 1160, 1165, 1170, 1175, 1180, 1015, 1010, 1020, 1040,
            //    1050, 1125, 1270, 1280, 1320, 1360, 5001, 1045, 1046, 1047, 1190, 1195, 1115, 3050,
            //    3090, 1250, 1260, 1290, 1300, 5020, 5030, 5040, 5050, 7000, 7010, 7020, 7030, 7040 };
            _Duration = 600;

            MagicAllowed = false;
            MeleeAllowed = false;
            AllowedSkills = new System.Collections.Generic.List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
        }

        public override void TeleportPlayersToMap()
        {
            base.TeleportPlayersToMap();
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (GameClient client in PlayerList.Values)
                {
                    ChangePKMode(client, PKMode.PK);
                    ushort x = 0;
                    ushort y = 0;
                    Map.GetRandCoord(ref x, ref y);
                    client.Teleport(x, y, Map.ID, DinamicID, true, true);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                    client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                }
                LastScore = DateTime.Now;
                DisplayScores = DateTime.Now;
            }
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
                                    client.Teleport(50, 50, 1616);
                                    PlayerList.Add(client.Player.UID, client);
                                    PlayerScores.Add(client.Player.UID, 1000);
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
        public override void WaitForWinner()
        {
            base.WaitForWinner();
            if (_Duration <= 0 || PlayerList.Count == 1)
                Finish();

            if (DateTime.Now >= DisplayScores.AddMilliseconds(2500))
                DisplayScore();

        }
        public override uint GetDamage(GameClient User, GameClient C)
        {
            return Convert.ToUInt32(C.Status.MaxHitpoints * 0.4);
        }

        public void TeleafterRev(GameClient C)
        {
            int RndX = Program.Rnd.Next(0, 2);
            int RndY = Program.Rnd.Next(0, 2);
            int X = 50;
            int Y = 50;
            switch (RndX)
            {
                case 0:
                    X = 50 + Program.Rnd.Next(5, 19);
                    break;
                case 1:
                    X = 50 - Program.Rnd.Next(4, 18);
                    break;
            }
            switch (RndY)
            {
                case 0:
                    Y = 50 - Program.Rnd.Next(4, 18);
                    break;
                case 1:
                    Y = 50 + Program.Rnd.Next(5, 19);
                    break;
            }


            C.Teleport((ushort)X, (ushort)Y, Map.ID, DinamicID, true, true);
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (PlayerScores.ContainsKey(Victim.Player.UID) && PlayerScores[Victim.Player.UID] > 1)
            {
                uint Receiveextremepoints = (uint)(Program.GetRandom.Next(10, 100));
                PlayerScores[Victim.Player.UID] -= (int)Receiveextremepoints;
                PlayerScores[Attacker.Player.UID] += (int)Receiveextremepoints;
                Attacker.SendSysMesage("You gained " + Receiveextremepoints.ToString() + " points for killing " + Victim.Player.Name + "", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                Victim.SendSysMesage("You lost " + Receiveextremepoints.ToString() + " points when " + Attacker.Player.Name.ToString() + " has killed you.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                Victim.Player.Dead(Victim.Player, Victim.Player.X, Victim.Player.Y, Victim.EntityID);

            }
            else
                RemovePlayer(Victim);
        }

        public override void End()
        {
            DisplayScore();
            int NO = 1;
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
            TimeSpan T = TimeSpan.FromSeconds(_Duration);
            //Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            if (_Duration > 0)
                --_Duration;
            PlayerList.Clear();
            PlayerScores.Clear();
            return;
        }
        public override void Reward(GameClient client)
        {
            //client.Player.BoundConquerPoints += 50;
            //client.Player.ConquerPoints += 7000;
            client.Player.skillmaster = 1;

            base.Reward(client);
        }

    }
}