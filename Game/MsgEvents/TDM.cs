using LightConquer_Project.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LightConquer_Project.Role.Flags;

namespace LightConquer_Project.Game.MsgEvents
{
    public class TDM : Events
    {
        int ScoreOne, ScoreTwo = 0;
        public TDM()
        {
            //EventTitle = "Team Deathmatch";
            //Duration = 10;
            //NoDamage = false;
            //BaseMap = 1505;
            //FriendlyFire = false;
            //_Duration = 180;
            EventTitle = "Team Deathmatch";
            Duration = 3;
            BaseMap = 1767;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            FriendlyFire = false;
            //ReviveAllowed = false;
            AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 600;
            PotionsAllowed = false;
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
                        TeamOne.Add(client.EntityID, client);
                        client.Player.AddSpecialGarment(stream, 183425);
                        client.SendSysMesage($"Welcome to {EventTitle} you're a member of team one");
                    }
                    else
                    {
                        TeamTwo.Add(client.EntityID, client);
                        client.Player.AddSpecialGarment(stream, 191305);
                        client.SendSysMesage($"Welcome to {EventTitle} you're a member of team two!");
                    }
                    counter++;
                }
                Teams.Add(183425, TeamOne);
                Teams.Add(191305, TeamTwo);
            }
        }

        public void TeleafterRev(GameClient C)
        {
            if (Teams[183425].ContainsKey(C.EntityID))
                C.Teleport(Map.ID, 136, 211, DinamicID);
            else
                C.Teleport(Map.ID, 187, 207, DinamicID);
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (Stage == EventStage.Fighting)
            {
                Victim.Player.Dead(Attacker.Player, Victim.Player.X, Victim.Player.Y, Attacker.EntityID);
                if (Teams[183425].ContainsKey(Attacker.EntityID))
                    ScoreOne++;
                else
                    ScoreTwo++;
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
                if (ScoreOne > ScoreTwo)
                {
                    player.SendSysMesage($"Team 1 - {ScoreOne}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                    player.SendSysMesage($"Team 2 - {ScoreTwo}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                }
                else
                {
                    player.SendSysMesage($"Team 1 - {ScoreOne}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                    player.SendSysMesage($"Team 2 - {ScoreTwo}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                }
                player.SendSysMesage($"My Score - {PlayerScores[player.EntityID]}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

            }
            TimeSpan T = TimeSpan.FromSeconds(_Duration);
            Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            if (_Duration > 0)
                --_Duration;
        }

        public override void Reward(GameClient c)
        {
            base.Reward(c);
        }

        public override void End()
        {
            DisplayScore();
            if (ScoreOne == ScoreTwo)
            {
                Broadcast("It's a tie! event Minutes have passed and the teams scored the same points! Better luck next time!", BroadCastLoc.World);
                foreach (GameClient C in PlayerList.Values.ToList())
                    C.EventBase?.RemovePlayer(C);
            }
            else
            {
                if (ScoreOne > ScoreTwo)
                {
                    Broadcast("The Red Team has won the " + EventTitle + "! Congratulations to all their members!", BroadCastLoc.World);

                    foreach (GameClient C in Teams[191305].Values.ToList())
                        C.EventBase?.RemovePlayer(C);
                }
                else if (ScoreTwo > ScoreOne)
                {
                    Broadcast("The Blue Team has won the " + EventTitle + "! Congratulations to all their members", BroadCastLoc.World);

                    foreach (GameClient C in Teams[183425].Values.ToList())
                        C.EventBase?.RemovePlayer(C);
                }
                foreach (var c in PlayerList.Values.ToList())
                {
                    Reward(c);
                    RemovePlayer(c);
                }
            }
            PlayerList.Clear();
            PlayerScores.Clear();
            Teams.Clear();
            return;
        }
    }
}