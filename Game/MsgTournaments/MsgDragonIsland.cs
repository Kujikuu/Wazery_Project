using OdysseyServer_Project.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdysseyServer_Project.Game.MsgTournaments
{
    public enum ConquererMode
    {
        MobsKiller = 0,
        BansheeKiller = 1,
        SpookKiller = 2,
        NemsisKiller = 3
    }
    public class MsgDragonIsland
    {
        public Tuple<ConquererMode, string, uint, uint>[] BossConquerers = new Tuple<ConquererMode, string, uint, uint>[4];
        public const ushort MapID = 10137;
        public ProcesType Process { get; set; }
        public Fan SafeArea = new Fan(0, 400, 150, 419, 150, 180);
        public Role.GameMap Map
        {
            get { return Database.Server.ServerMaps[MapID]; }
        }
        public DateTime ScoreStamp = new DateTime();
        public MsgDragonIsland(ProcesType _process)
        {
            Process = _process;
            //Map = Database.Server.ServerMaps[MapID];
            BossConquerers[0] = Tuple.Create<ConquererMode, string, uint, uint>(ConquererMode.MobsKiller, "None", 0, 0);
            BossConquerers[1] = Tuple.Create<ConquererMode, string, uint, uint>(ConquererMode.BansheeKiller, "None", 0, 0);
            BossConquerers[2] = Tuple.Create<ConquererMode, string, uint, uint>(ConquererMode.SpookKiller, "None", 0, 0);
            BossConquerers[3] = Tuple.Create<ConquererMode, string, uint, uint>(ConquererMode.NemsisKiller, "None", 0, 0);
        }
        public bool Attackable(uint Map, ushort X, ushort Y)
        {
            if (Map != MapID)
                return true;
            return SafeArea.IsInFan(X, Y) == true ? false : true;
        }
        private void AnnounceKillers(ServerSockets.Packet stream)
        {
            Game.MsgServer.MsgMessage msg = null;
            {
#if Arabic
                           msg = new MsgServer.MsgMessage("تصنيف [" + BossConquerers[0].Item1.ToString() + "] الإسم [" + BossConquerers[0].Item2 + "] عدد النقاط [" + BossConquerers[0].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                      
#else
                msg = new MsgServer.MsgMessage("Rank [" + BossConquerers[0].Item1.ToString() + "] Name [" + BossConquerers[0].Item2 + "] Score [" + BossConquerers[0].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);

#endif
                SendMapPacket(msg.GetArray(stream));
            }
            msg = null;
            {
#if Arabic
                           msg = new MsgServer.MsgMessage("تصنيف [" + BossConquerers[1].Item1.ToString() + "] الإسم [" + BossConquerers[1].Item2 + "] عدد النقاط [" + BossConquerers[1].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                      
#else
                msg = new MsgServer.MsgMessage("Rank [" + BossConquerers[1].Item1.ToString() + "] Name [" + BossConquerers[1].Item2 + "] Score [" + BossConquerers[1].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

#endif
                SendMapPacket(msg.GetArray(stream));
            }
            msg = null;
            {
#if Arabic
                           msg = new MsgServer.MsgMessage("تصنيف [" + BossConquerers[2].Item1.ToString() + "] الإسم [" + BossConquerers[2].Item2 + "] عدد النقاط [" + BossConquerers[2].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                      
#else
                msg = new MsgServer.MsgMessage("Rank [" + BossConquerers[2].Item1.ToString() + "] Name [" + BossConquerers[2].Item2 + "] Score [" + BossConquerers[2].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

#endif
                SendMapPacket(msg.GetArray(stream));
            }
            msg = null;
            {
#if Arabic
                           msg = new MsgServer.MsgMessage("تصنيف [" + BossConquerers[3].Item1.ToString() + "] الإسم [" + BossConquerers[3].Item2 + "] عدد النقاط [" + BossConquerers[3].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                      
#else
                msg = new MsgServer.MsgMessage("Rank [" + BossConquerers[3].Item1.ToString() + "] Name [" + BossConquerers[3].Item2 + "] Score [" + BossConquerers[3].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

#endif
                SendMapPacket(msg.GetArray(stream));
            }
            msg = null;
            ScoreStamp = DateTime.Now.AddSeconds(3);
        }
        public void SendMapPacket(ServerSockets.Packet stream)
        {
            foreach (var user in MapPlayers())
                user.Send(stream);
        }
        public Client.GameClient[] MapPlayers()
        {
            return Map.Values.Where(p => InTournament(p)).ToArray();
        }
        public bool InTournament(Client.GameClient user)
        {
            if (Map == null)
                return false;
            return user.Player.Map == Map.ID;
        }
        public void BossKilled()
        {
            if (Process == ProcesType.Alive)
            {
                Process = ProcesType.Idle;
            }
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream, bool Dragon = false)
        {
            if (user.Player.Level < 120 || user.Player.Reborn < 1)
                return false;
            //if (Process == ProcesType.Idle)
            {
                if (Dragon)
                {
                    user.Teleport((ushort)(455 - Program.GetRandom.Next(0, 5)), (ushort)(479 - Program.GetRandom.Next(0, 5)), 10137);
                }
                else
                {
                    user.Teleport((ushort)(95 - Program.GetRandom.Next(0, 5)), (ushort)(412 - Program.GetRandom.Next(0, 5)), MapID);
                }
                return true;
            }
            /*else if (Process == ProcesType.Alive)
            {
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map.ID);
                if (user.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Ride))
                    user.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Ride);
                return true;
            }*/
            //return false;
        }
        public void SortKiller(Client.GameClient client, uint Score, ConquererMode Mode)
        {
            if (Process == ProcesType.Alive)
            {
                if (BossConquerers[(int)Mode].Item4 == client.Player.UID)
                {
                    BossConquerers[(int)Mode] = null;
                    BossConquerers[(int)Mode] = new Tuple<ConquererMode, string, uint, uint>(Mode, client.Player.Name, Score, client.Player.UID);
                }
                else
                {
                    if (BossConquerers[(int)Mode].Item3 < Score)
                    {
                        BossConquerers[(int)Mode] = null;
                        BossConquerers[(int)Mode] = new Tuple<ConquererMode, string, uint, uint>(Mode, client.Player.Name, Score, client.Player.UID);
                    }
                }
            }
        }
        public void CheckUp()
        {
            if (DateTime.Now > ScoreStamp)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    AnnounceKillers(stream);
                }
            }
            if (Process == ProcesType.Alive)
            {
                if (!Map.ContainMobID(20160) && !Map.ContainMobID(20300) && !Map.ContainMobID(20070))
                    Process = ProcesType.Idle;
            }
            if (Process == ProcesType.Idle)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    if (DateTime.Now.Hour != Database.Server.DragonIslandSpookHour.Hour && DateTime.Now.Minute == 00)
                    {
                        if (!Map.ContainMobID(20160))//Thrilling Spook
                        {
                            Database.Server.DragonIslandSpookHour = DateTime.Now;
                            Database.Server.AddMapMonster(stream, Map, 20160, 349, 635, 3, 3, 1, 0, true, OdysseyServer_Project.Game.MsgFloorItem.MsgItemPacket.EffectMonsters.None);
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("[Thrilling Spook] has appeared in Dragon Island Go and kill it now.", Game.MsgServer.MsgMessage.MsgColor.orange, Game.MsgServer.MsgMessage.ChatMode.SlideCrosTheServer).GetArray(stream));
                            MyConsole.WriteLine("[DragonIsland] Thrilling Spook Has Spawned.");
                            Process = ProcesType.Alive;
                        }
                    }
                    if (DateTime.Now.Hour != Database.Server.DragonIslandNemsisHour.Hour && DateTime.Now.Minute == 20)
                    {
                        if (!Map.ContainMobID(20300))//Nemesis Tyrant
                        {
                            Database.Server.DragonIslandNemsisHour = DateTime.Now;
                            Database.Server.AddMapMonster(stream, Map, 20300, 568, 372, 3, 3, 1, 0, true, OdysseyServer_Project.Game.MsgFloorItem.MsgItemPacket.EffectMonsters.None);
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("[Nemesis Tyrant] has appeared in Dragon Island Go and kill it now.", Game.MsgServer.MsgMessage.MsgColor.orange, Game.MsgServer.MsgMessage.ChatMode.SlideCrosTheServer).GetArray(stream));
                            MyConsole.WriteLine("[DragonIsland] Nemesis Tyrant Has Spawned.");
                            Process = ProcesType.Alive;
                        }
                    }
                    if (DateTime.Now.Hour != Database.Server.DragonIslandBansheeHour.Hour && DateTime.Now.Minute == 40)
                    {
                        if (!Map.ContainMobID(20070))//Snow Banshee
                        {
                            Database.Server.DragonIslandBansheeHour = DateTime.Now;
                            Database.Server.AddMapMonster(stream, Map, 20070, 658, 718, 3, 3, 1, 0, true, OdysseyServer_Project.Game.MsgFloorItem.MsgItemPacket.EffectMonsters.None);
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("[Snow Banshee] has appeared in Dragon Island Go and kill it now.", Game.MsgServer.MsgMessage.MsgColor.orange, Game.MsgServer.MsgMessage.ChatMode.SlideCrosTheServer).GetArray(stream));
                            MyConsole.WriteLine("[DragonIsland] Snow Banshee Has Spawned.");
                            Process = ProcesType.Alive;
                        }
                    }
                }
            }
        }
        public void GivePrizes()
        {
            if (Process == ProcesType.Alive)
                Process = ProcesType.Idle;
            //foreach (var conquqer in BossConquerers)
            //{
            //    if (conquqer.Item1 == ConquererMode.MobsKiller)
            //    {
            //        if (Server.GamePoll.ContainsKey(conquqer.Item4))
            //        {
            //            Server.GamePoll[conquqer.Item4].Player.ConquerPoints += 10 * conquqer.Item3;
            //        }
            //    }
            //    else
            //    {
            //        if (Server.GamePoll.ContainsKey(conquqer.Item4))
            //        {
            //            Server.GamePoll[conquqer.Item4].Player.ConquerPoints += 7000;
            //        }
            //    }
            //}
        }
    }
}
