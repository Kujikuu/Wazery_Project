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
    class LastManStand : Events
    {
        public LastManStand()
        {
            EventTitle = "Last Man Standing";
            Duration = 10;
            BaseMap = 700;
            NoDamage = false;
            _Duration = 180;
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
        public override void CharacterChecks(GameClient C)
        {
            base.CharacterChecks(C);
            if (!C.Player.Alive)
                C.EventBase?.RemovePlayer(C);
        }
        public override void DisplayScore()
        {
            DisplayScores = System.DateTime.Now;
            foreach (var player in PlayerList.Values)
            player.SendSysMesage($"---------{EventTitle}---------", MsgServer.MsgMessage.ChatMode.FirstRightCorner);


            Broadcast($"Players left: {PlayerList.Count}", BroadCastLoc.Score, 2);
        }
        public override void Kill(GameClient Attacker, GameClient Victim)
        {
            RemovePlayer(Victim);
        }
        public override void Reward(GameClient client)
        {
            client.Player.LastManStand = 1;

            base.Reward(client);
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
    }
}