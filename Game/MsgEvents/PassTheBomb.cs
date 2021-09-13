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
    public class PTB : Events
    {
        byte PTBC = 0;
        bool Bomb = false;
        DateTime Timer;

        public PTB()
        {
            EventTitle = "Pass the Bomb";
            Duration = 10;
            BaseMap = 700;
            NoDamage = true;
            MagicAllowed = false;
            MeleeAllowed = false;
            AllowedSkills = new System.Collections.Generic.List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
            _Duration = 180;
            //Bomb = false;

        }

        public override void TeleportPlayersToMap()
        {
            foreach (GameClient client in PlayerList.Values)
            {
                ChangePKMode(client, PKMode.PK);
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                client.Teleport(x, y, Map.ID, DinamicID, true, true);
                client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                if (!Bomb)
                {
                    client.Player.AddFlag(MsgServer.MsgUpdate.Flags.Top4Weekly, 6666666, true);
                    Bomb = true;
                }
            }
            PTBC = 0;
        }
        /*
         * if (!DW)
                    {
                        client.Player.AddFlag(MsgServer.MsgUpdate.Flags.DragonSwing, 6666666, true);
                        DW = true;
                    }
         */
        public override void Hit(GameClient Attacker, GameClient Victim)
        {
            if (Stage == EventStage.Fighting)
            {
                if (Attacker.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Top4Weekly) && PTBC < 7)
                {
                    Broadcast(Attacker.Name + " has passed the bomb to " + Victim.Name + "! Be careful!", BroadCastLoc.Map);
                    PTBC = 0;
                    Attacker.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Top4Weekly);
                    Victim.Player.AddFlag(MsgServer.MsgUpdate.Flags.Top4Weekly, 6666666, true);
                }
                else if (Victim.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Top4Weekly) && PTBC < 7)
                    ReduceTimer(Victim);
            }
        }

        public override void RemovePlayer(GameClient C)
        {
            base.RemovePlayer(C);
            if (C.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Top4Weekly))
                Randomize();
            C.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Top4Weekly);
        }

        public override void WaitForWinner()
        {
            if (!Bomb)
            {
                Timer = DateTime.Now;
                Randomize();
                Bomb = true;
            }
            base.WaitForWinner();
        }

        public override void CharacterChecks(GameClient C)
        {
            base.CharacterChecks(C);
            if (!C.Player.Alive && DateTime.Now > C.Player.DeathHit.AddSeconds(2))
                C.EventBase?.RemovePlayer(C);
            else if (C.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Top4Weekly))
            {
                if (DateTime.Now >= Timer.AddMilliseconds(1000))
                {
                    ReduceTimer(C);
                    Timer = DateTime.Now;
                }
            }
        }

        public override void End()
        {
            foreach (GameClient C in PlayerList.Values)
                C.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Top4Weekly);
            base.End();
        }

        public void Randomize()
        {
            if (PlayerList.Count == 1)
                return;
            int Number = Program.Rnd.Next(1, (PlayerList.Count + 1));
            int MyPlace = 1;
            foreach (GameClient C in PlayerList.Values)
            {
                if (MyPlace == Number)
                {
                    C.Player.AddFlag(MsgServer.MsgUpdate.Flags.Top4Weekly, 6666666, true);
                    Broadcast(C.Name + " has the bomb! Be careful!", BroadCastLoc.Map);
                    break;
                }
                else
                    MyPlace++;
            }
        }

        public void ReduceTimer(GameClient C)
        {
            if (PTBC >= 0 && PTBC < 9)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    C.Player.SendString(stream, MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { $"downnumber{(9 - PTBC)}" });
                    PTBC++;
                }
            }

            else if (PTBC == 9)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    C.Player.SendString(stream, MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { $"attach_accept05" });
                    PTBC++;
                }
            }
            else if (PTBC == 10)
            {
                PTBC = 0;
                C.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Top4Weekly);
                RemovePlayer(C);
                Randomize();
            }
        }
        public override void DisplayScore()
        {
            //foreach (var player in PlayerList.Values)
            //{
            //    Broadcast($"You have [{PlayerScores[player.Player.UID]}] Hits Left.", BroadCastLoc.Title);
            //}
            TimeSpan T = TimeSpan.FromSeconds(_Duration);
            //Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            if (_Duration > 0)
                --_Duration;
            LastScores = DateTime.Now;
        }
        public override void Reward(GameClient client)
        {
            client.Player.PassTheBomb = 1;

            base.Reward(client);
        }
    }
}