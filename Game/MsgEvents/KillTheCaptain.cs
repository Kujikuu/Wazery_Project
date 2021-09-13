using OdysseyServer_Project.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OdysseyServer_Project.Role.Flags;

namespace OdysseyServer_Project.Game.MsgEvents
{
    class KillTheCaptain : Events
    {
        int WhiteScore = 0, RedScore = 0;
        Dictionary<uint, GameClient> White = new Dictionary<uint, GameClient>();
        Dictionary<uint, GameClient> Red = new Dictionary<uint, GameClient>();
        public KillTheCaptain()
        {
            EventTitle = "Kill The Capitan";
            Duration = 3;
            BaseMap = 1505;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            FriendlyFire = false;
            AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 180;
        }

        public override void BeginTournament()
        {
            Teams = new Dictionary<uint, Dictionary<uint, GameClient>>();
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
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                    client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                    if (counter % 2 == 0)
                    {
                        White.Add(client.EntityID, client);
                        client.Teleport(136, 211, Map.ID, DinamicID, true, true);
                        client.Player.AddSpecialGarment(stream, 181325);
                        client.SendSysMesage($"Welcome to {EventTitle} you're a member of White team!");
                    }
                    else
                    {
                        Red.Add(client.EntityID, client);
                        client.Teleport(187, 207, Map.ID, DinamicID, true, true);
                        client.Player.AddSpecialGarment(stream, 181625);
                        client.SendSysMesage($"Welcome to {EventTitle} you're a member of Red team!");
                    }
                    counter++;
                    if (!IsCapitan)
                    {
                        client.Player.AddFlag(MsgServer.MsgUpdate.Flags.GodlyShield, 6666666, true);
                        IsCapitan = true;
                    }
                }
                Teams.Add(1, White);
                Teams.Add(2, Red);
            }
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (Stage == EventStage.Fighting)
            {
                if (Teams[1].ContainsKey(Attacker.EntityID))
                    WhiteScore++;
                else
                    RedScore++;
                if (Victim.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
                {
                    Victim.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.GodlyShield);
                    Attacker.Player.AddFlag(MsgServer.MsgUpdate.Flags.GodlyShield, 666666, true);
                }
                if (!IsCapitan)
                {
                    Attacker.Player.AddFlag(MsgServer.MsgUpdate.Flags.GodlyShield, 666666, true);
                    IsCapitan = true;
                }
            }
        }

        public override void DisplayScore()
        {
            DisplayScores = DateTime.Now;
            foreach (var player in PlayerList.Values)
                player.SendSysMesage($"---------{EventTitle}---------", MsgServer.MsgMessage.ChatMode.FirstRightCorner);

            foreach (var player in PlayerList.Values)
            {
                if (WhiteScore > RedScore)
                {
                    player.SendSysMesage($"Team White - {WhiteScore}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                    player.SendSysMesage($"Team Red - {RedScore}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                }
                else
                {
                    player.SendSysMesage($"Team Red - {RedScore}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                    player.SendSysMesage($"Team White - {WhiteScore}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                }
                player.SendSysMesage($"My Score - {PlayerScores[player.EntityID]}", MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

                if (Teams[1].ContainsKey(player.EntityID) && player.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
                {
                    WhiteScore += 2;
                    Broadcast($"Captain {player.Player.Name} from White Team", BroadCastLoc.Score);
                }
                else if (Teams[2].ContainsKey(player.EntityID) && player.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
                {
                    RedScore += 2;
                    Broadcast($"Captain {player.Player.Name} from Red Team", BroadCastLoc.Score);
                }
            }
            TimeSpan T = TimeSpan.FromSeconds(_Duration);
            Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            if (_Duration > 0)
                --_Duration;
        }

        public override void End()
        {
            DisplayScore();
            if (RedScore == WhiteScore)
            {
                Broadcast("It's a tie! event Minutes have passed and the teams scored the same points! Better luck next time!", BroadCastLoc.World);
                foreach (GameClient C in PlayerList.Values.ToList())
                    C.EventBase?.RemovePlayer(C);
            }
            else
            {
                if (WhiteScore > RedScore)
                {
                    Broadcast("The White Team has won the " + EventTitle + "! Congratulations to all their members!", BroadCastLoc.World);
                        foreach (GameClient C2 in Teams[2].Values.ToList())
                            C2.EventBase?.RemovePlayer(C2);
                }
                else if (RedScore > WhiteScore)
                {
                    Broadcast("The Red Team has won the " + EventTitle + "! Congratulations to all their members!", BroadCastLoc.World);
                    foreach (GameClient C in Teams[1].Values.ToList())
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
        public override void RemovePlayer(GameClient client)
        {
            base.RemovePlayer(client);
            if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
                IsCapitan = false;
            client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.GodlyShield);
        }
        public override void Reward(GameClient client)
        {
            //client.Player.BoundConquerPoints += 50;
            //client.Player.ConquerPoints += 7000;
            base.Reward(client);

        }
    }
}
