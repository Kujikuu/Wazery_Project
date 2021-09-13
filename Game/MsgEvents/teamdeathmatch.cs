using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OdysseyServer_Project.Client;
using OdysseyServer_Project.Game.MsgServer;
using OdysseyServer_Project.MsgInterServer.Instance;
using OdysseyServer_Project.Role.Instance;
using static OdysseyServer_Project.Role.Flags;

namespace OdysseyServer_Project.Game.MsgEvents
{
    public class teamdeathmatch : Events
    {
        int ScoreTeamOne = 0, ScoreTeamTwo = 0, ScoreTeamThree = 0, ScoreTeamFour = 0;
        Dictionary<uint, GameClient> TeamOne = new Dictionary<uint, GameClient>();
        Dictionary<uint, GameClient> TeamTwo = new Dictionary<uint, GameClient>();
        Dictionary<uint, GameClient> TeamThree = new Dictionary<uint, GameClient>();
        Dictionary<uint, GameClient> TeamFour = new Dictionary<uint, GameClient>();
        public teamdeathmatch()
        {
            EventTitle = "4 Team DeathMatch";
            Duration = 10;
            BaseMap = 1507;
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
                Dictionary<uint, GameClient> TeamThree = new Dictionary<uint, GameClient>();
                Dictionary<uint, GameClient> TeamFour = new Dictionary<uint, GameClient>();
                foreach (GameClient c in PlayerList.Values)
                {
                    ChangePKMode(c, PKMode.Team);
                    if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                        c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                    if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                        c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                    if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                        c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                    if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.DragonSwing))
                        c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.DragonSwing);
                    c.Player.HitPoints = (int)c.Status.MaxHitpoints;
                    if (counter % 4 == 0)
                    {
                        TeamOne.Add(c.EntityID, c);
                        c.Teleport((ushort)(57 + (ushort)Program.Rnd.Next(5)), (ushort)(85 + (ushort)Program.Rnd.Next(6)), Map.ID, DinamicID, true, true);
                        c.Player.AddSpecialGarment(stream, 183425);
                        c.SendSysMesage($"Welcome to {EventTitle} you're a member of Blue Team!");
                    }
                    else if (counter % 4 == 1)
                    {
                        TeamTwo.Add(c.EntityID, c);
                        c.Teleport((ushort)(106 + (ushort)Program.Rnd.Next(7)), (ushort)(85 + (ushort)Program.Rnd.Next(6)), Map.ID, DinamicID, true, true);
                        c.Player.AddSpecialGarment(stream, 191605);
                        c.SendSysMesage($"Welcome to {EventTitle} you're a member of Red Team!");
                    }
                    else if (counter % 4 == 2)
                    {
                        TeamThree.Add(c.EntityID, c);
                        c.Teleport((ushort)(57 + (ushort)Program.Rnd.Next(5)), (ushort)(136 + (ushort)Program.Rnd.Next(6)), Map.ID, DinamicID, true, true);
                        c.Player.AddSpecialGarment(stream, 181525);
                        c.SendSysMesage($"Welcome to {EventTitle} you're a member of Black Team!");
                    }
                    else if (counter % 4 == 3)
                    {
                        TeamFour.Add(c.EntityID, c);
                        c.Teleport((ushort)(106 + (ushort)Program.Rnd.Next(7)), (ushort)(136 + (ushort)Program.Rnd.Next(6)), Map.ID, DinamicID, true, true);
                        c.Player.AddSpecialGarment(stream, 181325);
                        c.SendSysMesage($"Welcome to {EventTitle} you're a member of White Team!");
                    }
                    counter++;
                }
            }
            Teams.Add(183425, TeamOne);
            Teams.Add(191605, TeamTwo);
            Teams.Add(181525, TeamThree);
            Teams.Add(181325, TeamFour);

         
        }

        public override void WaitForWinner()
        {
            base.WaitForWinner();

            if (Infected())
            {
                if (DateTime.Now >= DisplayScores.AddMilliseconds(1500))
                    Finish();
            }
            else if (DateTime.Now >= DisplayScores.AddMilliseconds(2500))
                DisplayScore();
        }

        private bool Infected()
        {
            byte Count = 0;
            foreach (Dictionary<uint, GameClient> T in Teams.Values)
                if (T.Count == 0)
                    Count++;
            if (Count == 3)
                return true;
            else
                return false;
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            //if (Stage == EventStage.Fighting)
            //{
            //    foreach (KeyValuePair<uint, Dictionary<uint, GameClient>> T in Teams)
            //    {
            //        if (T.Value.ContainsKey(Attacker.EntityID) && !T.Value.ContainsKey(Victim.EntityID))
            //        {
            //            foreach (Dictionary<uint, GameClient> T2 in Teams.Values)
            //                if (T2.ContainsKey(Victim.EntityID))
            //                {
            //                    T2.Remove(Victim.EntityID);
            //                    break;
            //                }
            //            T.Value.Add(Victim.EntityID, Victim);
            //            Victim.MyClient.AddSend(Packets.OverwriteGarment(T.Key));
            //        }
            //    }
            //    if (PlayerScores.ContainsKey(Attacker.EntityID))
            //        PlayerScores[Attacker.EntityID]++;
            //    foreach (GameClient C in Victim.ScreenChars.Values)
            //        C.MyClient.AddSend(Packets.SpawnEntity(Victim));
            //}           int ScoreTeamOne = 0, ScoreTeamTwo = 0, ScoreTeamThree = 0, ScoreTeamFour = 0;

            if (Stage == EventStage.Fighting)
            {
                Victim.Player.Dead(Attacker.Player, Victim.Player.X, Victim.Player.Y, Attacker.EntityID);
                if (Teams[183425].ContainsKey(Attacker.EntityID))
                    ScoreTeamOne++;
               else if (Teams[191605].ContainsKey(Attacker.EntityID))
                    ScoreTeamTwo++;
                else if (Teams[181525].ContainsKey(Attacker.EntityID))
                    ScoreTeamThree++;
                else
                    ScoreTeamFour++;
                PlayerScores[Attacker.EntityID]++;
                //RevivePlayer(Victim);
                //TeleafterRev(Victim);
            }
        }

        public override void DisplayScore()
        {
            DisplayScores = DateTime.Now;
            foreach (var player in PlayerList.Values)
                player.SendSysMesage($"---------{EventTitle}---------", MsgServer.MsgMessage.ChatMode.FirstRightCorner);

            foreach (var player in PlayerList.Values)
            {
                
                    player.SendSysMesage($"Team Blue - {ScoreTeamOne}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                    player.SendSysMesage($"Team Red - {ScoreTeamTwo}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                    player.SendSysMesage($"Team Black - {ScoreTeamThree}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                    player.SendSysMesage($"Team White - {ScoreTeamFour}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

                player.SendSysMesage($"My Score - {PlayerScores[player.EntityID]}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
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
    }
}
