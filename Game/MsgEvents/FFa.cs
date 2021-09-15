using LightConquer_Project.Client;
using LightConquer_Project.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LightConquer_Project.Game.MsgServer.MsgMessage;
using static LightConquer_Project.Role.Flags;

namespace LightConquer_Project.Game.MsgEvents
{
    class FFa : Events
    {
        public FFa()
        {
            EventTitle = "FFa Tournament";
            Duration = 10;
            BaseMap = 1801;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            FriendlyFire = true;
            ReviveAllowed = true;
            AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 600;
            PotionsAllowed = false;
        }

        public override void TeleportPlayersToMap()
        {
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
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Goldbrick))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Goldbrick);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
              
            }
        }

        public override void WaitForWinner()
        {
            if (_Duration <= 0 || PlayerList.Count == 1)
                Finish();
            if (DateTime.Now >= LastScores.AddMilliseconds(3000))
                DisplayScore();
        }

        public override void RemovePlayer(GameClient client)
        {
            base.RemovePlayer(client);

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
            return;
        }

        public override uint GetDamage(GameClient User, GameClient C)
        {

            return 1;
        }


        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            PlayerScores[Attacker.EntityID] += 1;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Attacker.Player.RacePoints += 10;
                Attacker.Player.SendUpdate(stream, Attacker.Player.RacePoints, MsgUpdate.DataType.RaceShopPoints);
                Attacker.Player.SendUpdate(stream, Attacker.Player.RacePoints, MsgUpdate.DataType.BoundConquerPoints);
            }
            Victim.Player.Dead(Victim.Player, Victim.Player.X, Victim.Player.Y, Victim.EntityID);


        }

        public override void DisplayScore()
        {
            DisplayScores = DateTime.Now;
            foreach (var player in PlayerList.Values)
            {
                player.SendSysMesage($"---------{EventTitle}---------", ChatMode.FirstRightCorner);

            }
            byte Score = 2;
            foreach (var kvp in PlayerScores.OrderByDescending((s => s.Value)))
            {
                if (Score == 5)
                    break;
                if (Score == PlayerScores.Count + 2)
                    break;
                Broadcast($"Nº {Score - 1}: {PlayerList[kvp.Key].Player.Name} - {kvp.Value}", BroadCastLoc.Score, Score);
                Score++;
            }

            TimeSpan T = TimeSpan.FromSeconds(_Duration);
            Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            if (_Duration > 0)
                --_Duration;
        }

        public override void Reward(GameClient client)
        {
            client.Player.FFa = 1;

            base.Reward(client);
        }
    }
}
