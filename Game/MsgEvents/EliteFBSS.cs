using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OdysseyServer_Project.Client;
using static OdysseyServer_Project.Game.MsgServer.MsgMessage;

namespace OdysseyServer_Project.Game.MsgEvents
{
    class EliteFBSS : Events
    {
       public EliteFBSS()
        {
            EventTitle = "Skills Tournament";
            Duration = 10;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            FriendlyFire = true;
            BaseMap = 1801;
            AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 180;
            PotionsAllowed = false;
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            PlayerScores[Attacker.EntityID]++;
        }

        public override void WaitForWinner()
        {
            if (_Duration <= 0 || PlayerList.Count == 1)
                Finish();
            if (DateTime.Now >= LastScores.AddMilliseconds(1000))
                DisplayScore();
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
            
            base.Reward(client);
        }
    }
}
