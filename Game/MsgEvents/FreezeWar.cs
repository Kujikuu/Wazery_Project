using LightConquer_Project.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LightConquer_Project.Role.Flags;

namespace LightConquer_Project.Game.MsgEvents
{
    class FreezeWar : Events
    {
        byte Freeze = 0;
        int ScoreBlue = 0, ScoreRed = 0;
        Dictionary<uint, GameClient> Blue = new Dictionary<uint, GameClient>();
        Dictionary<uint, GameClient> Red = new Dictionary<uint, GameClient>();
        public FreezeWar()
        {
            EventTitle = "Freeze War";
            Duration = 3;
            BaseMap = 1505;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            FriendlyFire = true;
            AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 180;
        }

        public override void BeginTournament()
        {
            Teams = new Dictionary<uint, Dictionary<uint, GameClient>>();
            Freeze = 5;
            base.BeginTournament();
        }

        public override void TeleportPlayersToMap()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var counter = 0;

                foreach (GameClient client in PlayerList.Values)
                {
                    ChangePKMode(client, PKMode.PK);
                    ushort x = 0;
                    ushort y = 0;
                    Map.GetRandCoord(ref x, ref y);
                    client.Teleport(Map.ID, x, y, DinamicID, true, true);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.DragonSwing))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.DragonSwing);
                    client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                    if (counter % 2 == 0)
                    {
                        Blue.Add(client.EntityID, client);
                        client.Player.AddSpecialGarment(stream, 183425);
                        client.SendSysMesage($"Welcome to {EventTitle} you're a member of Blue team!");
                    }
                    else
                    {
                        Red.Add(client.EntityID, client);
                        client.Player.AddSpecialGarment(stream, 191605);
                        client.SendSysMesage($"Welcome to {EventTitle} you're a member of Red team!");
                    }
                    counter++;
                }
                Teams.Add(183425, Blue);
                Teams.Add(191605, Red);
            }
        }

        public override void DisplayScore()
        {
            DisplayScores = DateTime.Now;
            foreach (var player in PlayerList.Values)
                player.SendSysMesage($"---------{EventTitle}---------", MsgServer.MsgMessage.ChatMode.FirstRightCorner);

            foreach (var player in PlayerList.Values)
            {
                if (ScoreBlue > ScoreRed)
                {
                    player.SendSysMesage($"Team Blue - {ScoreBlue}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                    player.SendSysMesage($"Team Red - {ScoreRed}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                }
                else
                {
                    player.SendSysMesage($"Team Red - {ScoreRed}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                    player.SendSysMesage($"Team Blue - {ScoreBlue}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                }
                player.SendSysMesage($"My Score - {PlayerScores[player.EntityID]}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
            }
            TimeSpan T = TimeSpan.FromSeconds(_Duration);
            Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            if (_Duration > 0)
                --_Duration;
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (Stage == EventStage.Fighting)
            {
                if (Teams[183425].ContainsKey(Attacker.EntityID) && Teams[183425].ContainsKey(Victim.EntityID) || Teams[191605].ContainsKey(Attacker.EntityID) && Teams[191605].ContainsKey(Victim.EntityID))
                {
                    if (Victim.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                    {
                        Victim.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Freeze);
                        PlayerScores[Attacker.EntityID]++;
                        if (Teams[183425].ContainsKey(Attacker.EntityID))
                            ScoreBlue++;
                        else if (Teams[191605].ContainsKey(Attacker.EntityID))
                            ScoreRed++;
                    }
                }
                else
                {
                    if (!Victim.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                    {
                        Victim.Player.AddFlag(MsgServer.MsgUpdate.Flags.Freeze, 10, false);
                        PlayerScores[Attacker.EntityID]++;
                        if (Teams[183425].ContainsKey(Attacker.EntityID))
                            ScoreBlue++;
                        else if (Teams[191605].ContainsKey(Attacker.EntityID))
                            ScoreRed++;
                    }
                }
            }
        }

        public override void RemovePlayer(GameClient client)
        {
            base.RemovePlayer(client);
            client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Freeze);
        }

        public override void WaitForWinner()
        {
            base.WaitForWinner();
            if ((OneAllFrozen() || TwoAllFrozen()) && Freeze != 5)
            {
                if (Freeze < 1)
                    Freeze++;
                else if (Freeze == 1)
                    Finish();
            }
            else if (Freeze == 5)
            {
                foreach (GameClient C in PlayerList.Values)
                    C.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Freeze);
                Freeze = 0;
            }
            else if (Teams[183425].Count == 0 || Teams[191605].Count == 0)
                Finish();
        }

        public bool OneAllFrozen()
        {
            try
            {
                if (Teams[183425].Count == 0)
                    return true;
                foreach (GameClient p in Teams[183425].Values)
                    if (!p.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                        return false;
                return true;
            }
            catch { return false; }
        }
        public bool TwoAllFrozen()
        {
            try
            {
                if (Teams[191605].Count == 0)
                    return true;
                foreach (GameClient p in Teams[191605].Values)
                    if (!p.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                        return false;
                return true;
            }
            catch { return false; }
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
            client.Player.FreezeWar = 1;

            base.Reward(client);
        }
    }
}