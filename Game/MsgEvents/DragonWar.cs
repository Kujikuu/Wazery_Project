using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightConquer_Project.Client;
using LightConquer_Project.Game.MsgServer;
using static LightConquer_Project.Role.Flags;

namespace LightConquer_Project.Game.MsgEvents
{
    class DragonWar : Events
    {
        DateTime LastScore;

        //byte YY = 1;
        public DragonWar()
        {
            EventTitle = "Dragon War";
            Duration = 3;
            BaseMap = 1767;
            Reflect = false;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            AllowedSkills = new System.Collections.Generic.List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 180;
            PotionsAllowed = false;
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
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                    client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                    if (!DW)
                    {
                        client.Player.AddFlag(MsgServer.MsgUpdate.Flags.GodlyShield, 6666666, true);
                        DW = true;
                    }
                }
                LastScore = DateTime.Now;
                DisplayScores = DateTime.Now;
            }
        }

        public override void WaitForWinner()
        {
            base.WaitForWinner();
            if (PlayerScores.ContainsValue(300))
                Finish();
            else if (DateTime.Now >= DisplayScores.AddMilliseconds(3000))
                DisplayScore();
        }

        public override void CharacterChecks(GameClient client)
        {
            base.CharacterChecks(client);
            if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
            {
                if (DateTime.Now >= LastScore.AddMilliseconds(1000))
                {
                    LastScore = DateTime.Now;
                    if (PlayerScores[client.EntityID] + 3 > 300)
                        PlayerScores[client.EntityID] = 300;
                    else
                        PlayerScores[client.EntityID] += 3;
                }
            }
        }

        public override void RemovePlayer(GameClient client)
        {
            base.RemovePlayer(client);
            if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
                DW = false;
            client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.GodlyShield);
        }

        public override void End()
        {
            foreach (GameClient client in PlayerList.Values.ToList())
                client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.GodlyShield);

            DisplayScore();
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
            client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.GodlyShield);
            client.Player.DragonWar = 1;
            base.Reward(client);
        }

        public override uint GetDamage(GameClient User, GameClient C)
        {
            if (User.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
                return Convert.ToUInt32(C.Status.MaxHitpoints);
            else if (!DW)
                return Convert.ToUInt32(C.Status.MaxHitpoints * 0.4);
            else if (C.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
                return Convert.ToUInt32(C.Status.MaxHitpoints * 0.4);
            return 1;
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (Victim.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
            {
                if (Victim.Player.HitPoints < Victim.Status.MaxHitpoints * 0.4)
                {
                    Victim.Player.RemoveFlag(MsgUpdate.Flags.GodlyShield);
                    Attacker.Player.AddFlag(MsgUpdate.Flags.GodlyShield, 666666, true);
                    Attacker.Player.Stamina = 100;
                    if (PlayerScores[Attacker.EntityID] + 5 > 300)
                        PlayerScores[Attacker.EntityID] = 300;
                    else
                        PlayerScores[Attacker.EntityID] += 5;
                }
                else if (PlayerScores[Attacker.EntityID] + 5 > 300)
                    PlayerScores[Attacker.EntityID] = 300;
                else
                    PlayerScores[Attacker.EntityID] += 5;
            }
        }
        public override void Kill(GameClient Attacker, GameClient Victim)
        {
            if (Victim.Player.ContainFlag(MsgUpdate.Flags.GodlyShield))
            {
                Victim.Player.RemoveFlag(MsgUpdate.Flags.GodlyShield);
                Attacker.Player.AddFlag(MsgUpdate.Flags.GodlyShield, 666666, true);
                Attacker.Player.Stamina = 100;
                if (PlayerScores[Attacker.EntityID] + 5 > 300)
                    PlayerScores[Attacker.EntityID] = 300;
                else
                    PlayerScores[Attacker.EntityID] += 5;
            }
            else if (!DW)
            {
                Attacker.Player.AddFlag(MsgUpdate.Flags.GodlyShield, 666666, true);
                PlayerScores[Attacker.EntityID]++;
                DW = true;
            }
            else if (Attacker.Player.ContainFlag(MsgUpdate.Flags.GodlyShield))
            {
                if (PlayerScores[Attacker.EntityID] + 5 > 300)
                    PlayerScores[Attacker.EntityID] = 300;
                else
                    PlayerScores[Attacker.EntityID] += 5;
            }
            else
            {
                if (PlayerScores[Attacker.EntityID]++ > 300)
                    PlayerScores[Attacker.EntityID] = 300;
                else
                    PlayerScores[Attacker.EntityID]++;
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
    }
}