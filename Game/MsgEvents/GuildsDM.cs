using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OdysseyServer_Project.Client;
using OdysseyServer_Project.Game.MsgServer;
using static OdysseyServer_Project.Role.Flags;

namespace OdysseyServer_Project.Game.MsgEvents
{
    public class GuildsDeathMatch : Events
    {
        public static Dictionary<string, int> PrizesClaimed
             = new Dictionary<string, int>();
        //public GuildsDeathMatch()
        //    : base(8509, "GuildsDeathMatch", 300000, new int[] { 16, 15 }, 23) //prize doesn't count here
        //{
        //}
        public GuildsDeathMatch()
        {
            EventTitle = "GuildsDeathMatch";
            Duration = 10;
            BaseMap = 700;
            NoDamage = false;
            _Duration = 180;
        }
        DateTime lastSent = DateTime.Now;
        public Dictionary<string, int> GuildScores
             = new Dictionary<string, int>();
        List<string> score = new List<string>();
        public override void End()
        {
            base.End();
            score.Clear();
            GuildScores.Clear();
            foreach (GameClient c in PlayerList.Values.ToList())
            {
                if (c.Player.MyGuild == null)
                {
                    c.Teleport(1858, 63, 66);
                    continue;
                }
                if (!GuildScores.ContainsKey(c.Player.MyGuild.GuildName))
                    GuildScores.Add(c.Player.MyGuild.GuildName, 0);
                GuildScores[c.Player.MyGuild.GuildName]++;
            }
            foreach (var p in GuildScores.OrderByDescending(e => e.Value).Take(10))
                score.Add(p.Key + " : " + p.Value + ".");
            if (GuildScores.Count <= 1)
            {
                foreach (GameClient c in PlayerList.Values.ToList())
                    c.Teleport(1858, 63, 66);
                string winnerG = GuildScores.Take(1).SingleOrDefault().Key;
                if (winnerG != null
                    && PrizesClaimed != null)
                {
                    if (!PrizesClaimed.ContainsKey(winnerG))
                        PrizesClaimed.Add(winnerG, 0);
                    PrizesClaimed[winnerG]++;
                    Broadcast($"GuildsDM { winnerG } has won the Guilds Death Match, you can now claim your prize!", BroadCastLoc.World);

                    // End Event
                }
            }
            if (DateTime.Now > lastSent.AddSeconds(5))
            {
                SendScore(score);
                lastSent = DateTime.Now;
            }
        }
        public void SendScore(List<string> text)
        {
            foreach (GameClient C in PlayerList.Values.ToList())
            {
                Broadcast($"Alliance", BroadCastLoc.Score, 2);
                Broadcast($"Guild Player left :: {GuildScores.Count}", BroadCastLoc.Score, 2);

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
