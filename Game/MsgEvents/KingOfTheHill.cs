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
    public class KOTH : Events
    {
        private List<uint> Kings = new List<uint>();
        DateTime LastScore;
        public KOTH()
        {
            EventTitle = "King Of The Hill";
            Duration = 10;
            BaseMap = 1767;
            //MagicAllowed = false;
            NoDamage = true;
            //AllowedSkills = new List<ushort>() { 1000, 1001, 1002, 1005, 1055, 1075, 1085, 1090,
            //    1095, 1100, 1105, 1120, 1150, 1160, 1165, 1170, 1175, 1180, 1015, 1010, 1020, 1040,
            //    1050, 1125, 1270, 1280, 1320, 1360, 5001, 1045, 1046, 1047, 1190, 1195, 1115, 3050,
            //    3090, 1250, 1260, 1290, 1300, 5020, 5030, 5040, 5050, 7000, 7010, 7020, 7030, 7040 };
            _Duration = 180;

            MagicAllowed = false;
            MeleeAllowed = false;
            AllowedSkills = new System.Collections.Generic.List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
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
                    ushort x = 0;
                    ushort y = 0;
                    Map.GetRandCoord(ref x, ref y);
                    client.Teleport(x, y, Map.ID, DinamicID, true, true);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                    if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                        client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                    client.Player.HitPoints = (int)client.Status.MaxHitpoints;
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

            if (DateTime.Now >= DisplayScores.AddMilliseconds(2500))
                DisplayScore();

            if (DateTime.Now >= LastScore.AddMilliseconds(1000))
            {
                Kings.Clear();
                LastScore = DateTime.Now;
            }
        }

        public override void CharacterChecks(GameClient C)
        {
            base.CharacterChecks(C);
             if (!Kings.Contains(C.EntityID) && C.Player.X >= 47 && C.Player.X <= 54 && C.Player.Y >= 47 && C.Player.Y <= 54)
            {
                Kings.Add(C.EntityID);
                if (PlayerScores.ContainsKey(C.EntityID))
                    if (PlayerScores[C.EntityID] + 5 > 300)
                        PlayerScores[C.EntityID] = 300;
                    else
                        PlayerScores[C.EntityID] += 5;
            }
        }

        public void TeleafterRev(GameClient C)
        {
            int RndX = Program.Rnd.Next(0, 2);
            int RndY = Program.Rnd.Next(0, 2);
            int X = 50;
            int Y = 50;
            switch (RndX)
            {
                case 0:
                    X = 50 + Program.Rnd.Next(5, 19);
                    break;
                case 1:
                    X = 50 - Program.Rnd.Next(4, 18);
                    break;
            }
            switch (RndY)
            {
                case 0:
                    Y = 50 - Program.Rnd.Next(4, 18);
                    break;
                case 1:
                    Y = 50 + Program.Rnd.Next(5, 19);
                    break;
            }


            C.Teleport((ushort)X, (ushort)Y, Map.ID, DinamicID, true, true);
        }

        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (Victim.Player.X >= 47 && Victim.Player.X <= 54 && Victim.Player.Y >= 47 && Victim.Player.Y <= 54)
            {
                byte ToDir = (byte)(7 - (Math.Floor(MyMath.PointDirecton(Attacker.Player.X, Attacker.Player.Y, Victim.Player.X, Victim.Player.Y) / 45 % 8)) - 1 % 8);
                byte Direction = (byte)((int)ToDir % 8);
                if (Direction == 0)//sw
                    Victim.Player.Y += 6;
                else if (Direction == 2)//nw
                    Victim.Player.X -= 6;
                else if (Direction == 4)//ne
                    Victim.Player.Y -= 6;
                else if (Direction == 6)//se
                    Victim.Player.X += 6;
                else if (Direction == 1)//w
                {
                    Victim.Player.X -= 6;
                    Victim.Player.Y += 6;
                }
                else if (Direction == 3)//n
                {
                    Victim.Player.X -= 6;
                    Victim.Player.Y -= 6;
                }
                else if (Direction == 5)//e
                {
                    Victim.Player.X += 6;
                    Victim.Player.Y -= 6;
                }
                else if (Direction == 7)//s
                {
                    Victim.Player.X += 6;
                    Victim.Player.Y += 6;
                }
                Victim.Player.AddFlag(MsgServer.MsgUpdate.Flags.Dizzy, 10, true);

            }
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

       
    }
}