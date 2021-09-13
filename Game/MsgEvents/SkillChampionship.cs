using LightConquer_Project.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LightConquer_Project.Role.Flags;
using static LightConquer_Project.Game.MsgServer.MsgMessage;

namespace LightConquer_Project.Game.MsgEvents
{
    public class SkillChampionship : Events
    {
        //public SkillChampionship()
        //{
        //    EventTitle = "Skill Championship";
        //    Duration = 3;
        //    BaseMap = 700;
        //    NoDamage = true;
        //    MagicAllowed = false;
        //    MeleeAllowed = false;
        //    FriendlyFire = true;
        //    AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
        //    _Duration = 180;
        //    PotionsAllowed = false;

        //}
        public SkillChampionship()
        {
            EventTitle = "Skill Championship";
            Duration = 10;
            BaseMap = 700;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            FriendlyFire = false;
            //ReviveAllowed = false;
            AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 180;
            PotionsAllowed = false;
        }

        public override void TeleportPlayersToMap()
        {
            foreach (GameClient c in PlayerList.Values)
            {
                ChangePKMode(c, PKMode.PK);
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                c.Teleport(x, y, Map.ID, DinamicID, true, true);
                if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                    c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                    c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                    c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                if (c.Player.ContainFlag(MsgServer.MsgUpdate.Flags.DragonSwing))
                    c.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.DragonSwing);
                c.Player.HitPoints = (int)c.Status.MaxHitpoints;
            }
        }

        public override void Kill(GameClient Attacker, GameClient Victim)
        {
            if (PlayerScores.ContainsKey(Attacker.EntityID))
                PlayerScores[Attacker.EntityID]++;
        }

        public override void WaitForWinner()
        {
            base.WaitForWinner();
            if (DateTime.Now >= LastScores.AddMilliseconds(3000))
                DisplayScore();
        }

        public override void CharacterChecks(GameClient client)
        {
            base.CharacterChecks(client);
            if (!client.Player.Alive && DateTime.Now >= client.Player.DeathHit.AddMilliseconds(5000))
            {
                RevivePlayer(client, client.Player.HitPoints);
                TeleAfterRev(client);
            }
            else if (DateTime.Now >= client.Player.LastMove.AddSeconds(60))
                client.EventBase?.RemovePlayer(client);
        }
        public override void RemovePlayer(GameClient client)
        {
            base.RemovePlayer(client);
            //if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.DragonSwing))
            //    DW = false;
            //client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.DragonSwing);
        }
        public override void End()
        {
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

        public override void Reward(GameClient c)
        {
            //Broadcast(c.Name + " has won the " + EventTitle + " receiving the Top FB/SS Halo and a DBScroll!", BroadCastLoc.World);
            //if (c.Inventory.Count < 40)
            //    c.AddItem(720028);
            c.Player.SkillChampionship = 1;
            base.Reward(c);

        }

        //private void TeleAfterRev(GameClient C)
        //{
        //    X = (ushort)(51 + Program.Rnd.Next(1, 21) - Program.Rnd.Next(1, 21));
        //    Y = (ushort)(50 + Program.Rnd.Next(1, 20) - Program.Rnd.Next(1, 20));
        //    C.Teleport(MapEvent, (ushort)X, (ushort)Y);
        //}

        public override void DisplayScore()
        {
            //foreach (var player in PlayerList.Values)
            //{
            //    player.SendSysMesage($"---------{EventTitle}---------", MsgServer.MsgMessage.ChatMode.FirstRightCorner);
            //}
            //byte Score = 2;
            //foreach (var kvp in PlayerScores.OrderByDescending((s => s.Value)))
            //{
            //    if (Score == 7)
            //        break;
            //    if (Score == PlayerScores.Count + 2)
            //        break;
            //    Broadcast($"Nº {Score - 1}: {PlayerList[kvp.Key].Name} - {kvp.Value}", BroadCastLoc.Score, Score);
            //    Score++;
            //}
            //LastScores = DateTime.Now;
            //TimeSpan T = TimeSpan.FromSeconds(_Duration);
            //Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            //if (_Duration > 0)
            //    --_Duration;
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

        public override uint GetDamage(GameClient User, GameClient C/*, Database.MagicType.Magic Info*/)
        {
            return (uint)C.Player.HitPoints;
        }
    }
}