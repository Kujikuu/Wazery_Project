using LightConquer_Project.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LightConquer_Project.Role.Flags;

namespace LightConquer_Project.Game.MsgEvents
{
    public class HalloweenInfection : Events
    {
        private byte _safe = 0;
        public HalloweenInfection()
        {
            EventTitle = "Snow & Sunshine Infection";
            Duration = 10;
            BaseMap = 1508;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            FriendlyFire = false;
            AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 180;
        }

        public override void TeleportPlayersToMap()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var counter = 0;
                Teams = new Dictionary<uint, Dictionary<uint, GameClient>>();
                Dictionary<uint, GameClient> TeamOne = new Dictionary<uint, GameClient>();
                Dictionary<uint, GameClient> TeamTwo = new Dictionary<uint, GameClient>();
                foreach (GameClient c in PlayerList.Values)
                {
                    ChangePKMode(c, PKMode.PK);
                    if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                        c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                    if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                        c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                    if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                        c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                    c.Player.HitPoints = (int)c.Status.MaxHitpoints;
                    if (TeamOne.Count <= TeamTwo.Count)
                    {
                        TeamOne.Add(c.EntityID, c);
                        c.Teleport((ushort)(125 + (ushort)Program.Rnd.Next(3)), (ushort)(165 + (ushort)Program.Rnd.Next(3)), Map.ID, DinamicID, true, true);
                        c.Player.AddSpecialGarment(stream, 187825);
                        c.SendSysMesage($"Welcome to {EventTitle} you're a member of the Glaze!");

                    }
                    else
                    {
                        TeamTwo.Add(c.EntityID, c);
                        c.Teleport((ushort)(113 + (ushort)Program.Rnd.Next(3)), (ushort)(79 + (ushort)Program.Rnd.Next(3)), Map.ID, DinamicID, true, true);
                        c.Player.AddSpecialGarment(stream, 187835);
                        c.SendSysMesage($"Welcome to {EventTitle} you're a member of the Glory!");

                    }
                    counter++;
                    //c.MyClient.AddSend(Packets.GeneralData(c.EntityID, 5855577, 0, 0, 104));
                }
                Teams.Add(187825, TeamOne);
                Teams.Add(187835, TeamTwo);
            }
            //foreach (KeyValuePair<uint, Dictionary<uint, GameClient>> T in Teams)
            //    foreach (GameClient C in T.Value.Values)
            //        C.MyClient.AddSend(Packets.OverwriteGarment(T.Key));
        }

        public override void WaitForWinner()
        {
            base.WaitForWinner();
            if (Infected())
            {
                if (_safe == 2)
                    Finish();
                else
                    _safe++;
            }

            if (DateTime.Now >= DisplayScores.AddMilliseconds(2500))
                DisplayScore();
        }

        private bool Infected()
        {
            foreach (KeyValuePair<uint, Dictionary<uint, GameClient>> T in Teams)
                if (T.Value.Count == 0)
                    return true;
            return false;
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (Stage == EventStage.Fighting)
            {
                //foreach (KeyValuePair<uint, Dictionary<uint, GameClient>> T in Teams)
                //{
                //    if (T.Value.ContainsKey(Attacker.EntityID) && !T.Value.ContainsKey(Victim.EntityID))
                //    {
                //        foreach (Dictionary<uint, GameClient> T2 in Teams.Values)
                //            if (T2.ContainsKey(Victim.EntityID))
                //            {
                //                T2.Remove(Victim.EntityID);
                //                break;
                //            }
                //        T.Value.Add(Victim.EntityID, Victim);
                //        Victim.MyClient.AddSend(Packets.OverwriteGarment(T.Key));
                //    }
                //}
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    if (Teams[187825].ContainsKey(Attacker.EntityID))
                    {
                        if (Teams[187835].ContainsKey(Victim.EntityID))
                            Teams[187835].Remove(Victim.EntityID);

                        Teams[187825].Add(Victim.EntityID, Victim);
                        Victim.SendSysMesage($"You've joined the Glaze!");

                        //Victim.MyClient.AddSend(Packets.OverwriteGarment(TeamOneGarment));
                        Victim.Player.AddSpecialGarment(stream, 187825);

                    }
                    else if (Teams[187835].ContainsKey(Attacker.EntityID))
                    {
                        if (Teams[187825].ContainsKey(Victim.EntityID))
                            Teams[187825].Remove(Victim.EntityID);

                        Teams[187835].Add(Victim.EntityID, Victim);
                        Victim.SendSysMesage($"You've joined the Glory!");
                        Victim.Player.AddSpecialGarment(stream, 187835);

                    }
                }
                if (PlayerScores.ContainsKey(Attacker.EntityID))
                    PlayerScores[Attacker.EntityID]++;

                //foreach (GameClient C in Victim.ScreenChars.Values)
                //    C.MyClient.AddSend(Packets.SpawnEntity(Victim));
            }
        }

        public override void DisplayScore()
        {
            DisplayScores = DateTime.Now;
            foreach (var player in PlayerList.Values)
            {
                player.SendSysMesage($"---------{EventTitle}---------", MsgServer.MsgMessage.ChatMode.FirstRightCorner);
            }
            byte Score = 2;
            foreach (var kvp in PlayerScores.OrderByDescending((s => s.Value)))
            {
                if (Score == 7)
                    break;
                if (Score == PlayerScores.Count + 2)
                    break;
                Broadcast($"Nº {Score - 1}: {PlayerList[kvp.Key].Name} - {kvp.Value}", BroadCastLoc.Score, Score);
                Score++;
            }
            TimeSpan T = TimeSpan.FromSeconds(_Duration);
            Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            if (_Duration > 0)
                --_Duration;
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
            PlayerList.Clear();
            PlayerScores.Clear();
            Teams.Clear();
            return;
        }
        public override void Reward(GameClient client)
        {
            //client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Freeze);
            base.Reward(client);
        }
    }
}