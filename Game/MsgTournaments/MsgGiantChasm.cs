using OdysseyServer_Project.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdysseyServer_Project.Game.MsgTournaments
{
    public enum ConquererChasmMode
    {
        BloodyBansheeKiller = 0,
        ChillingSpookKiller = 1,
        NetherKiller = 2,
        DragonWraithKiller = 3,
        MobsKiller = 4
    }
    public class MsgGiantChasm
    {
        public Tuple<ConquererChasmMode, string, uint, uint>[] BossChasmConquerers = new Tuple<ConquererChasmMode, string, uint, uint>[4];
        public const ushort MapID = 10166;
        public Fan SafeArea = new Fan(110, 47, 136, 47, 150, 180);
        public ProcesType Process { get; set; }
        public Role.GameMap Map
        {
            get { return Database.Server.ServerMaps[MapID]; }
        }
        public DateTime ScoreStamp = new DateTime();
        public MsgGiantChasm(ProcesType _process)
        {
            Process = _process;
            BossChasmConquerers[0] = Tuple.Create<ConquererChasmMode, string, uint, uint>(ConquererChasmMode.BloodyBansheeKiller, "None", 0, 0);
            BossChasmConquerers[1] = Tuple.Create<ConquererChasmMode, string, uint, uint>(ConquererChasmMode.ChillingSpookKiller, "None", 0, 0);
            BossChasmConquerers[2] = Tuple.Create<ConquererChasmMode, string, uint, uint>(ConquererChasmMode.NetherKiller, "None", 0, 0);
            BossChasmConquerers[3] = Tuple.Create<ConquererChasmMode, string, uint, uint>(ConquererChasmMode.DragonWraithKiller, "None", 0, 0);
        }
        private void AnnounceKillers(ServerSockets.Packet stream)
        {
            Game.MsgServer.MsgMessage msg = null;
            {
#if Arabic
                           msg = new MsgServer.MsgMessage("تصنيف [" + BossChasmConquerers[0].Item1.ToString() + "] الإسم [" + BossChasmConquerers[0].Item2 + "] عدد النقاط [" + BossChasmConquerers[0].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                      
#else
                msg = new MsgServer.MsgMessage("Rank [" + BossChasmConquerers[0].Item1.ToString() + "] Name [" + BossChasmConquerers[0].Item2 + "] Score [" + BossChasmConquerers[0].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);

#endif
                SendMapPacket(msg.GetArray(stream));
            }
            msg = null;
            {
#if Arabic
                           msg = new MsgServer.MsgMessage("تصنيف [" + BossChasmConquerers[1].Item1.ToString() + "] الإسم [" + BossChasmConquerers[1].Item2 + "] عدد النقاط [" + BossChasmConquerers[1].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                      
#else
                msg = new MsgServer.MsgMessage("Rank [" + BossChasmConquerers[1].Item1.ToString() + "] Name [" + BossChasmConquerers[1].Item2 + "] Score [" + BossChasmConquerers[1].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

#endif
                SendMapPacket(msg.GetArray(stream));
            }
            msg = null;
            {
#if Arabic
                           msg = new MsgServer.MsgMessage("تصنيف [" + BossChasmConquerers[2].Item1.ToString() + "] الإسم [" + BossChasmConquerers[2].Item2 + "] عدد النقاط [" + BossChasmConquerers[2].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                      
#else
                msg = new MsgServer.MsgMessage("Rank [" + BossChasmConquerers[2].Item1.ToString() + "] Name [" + BossChasmConquerers[2].Item2 + "] Score [" + BossChasmConquerers[2].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

#endif
                SendMapPacket(msg.GetArray(stream));
            }
            msg = null;
            {
#if Arabic
                           msg = new MsgServer.MsgMessage("تصنيف [" + BossChasmConquerers[3].Item1.ToString() + "] الإسم [" + BossChasmConquerers[3].Item2 + "] عدد النقاط [" + BossChasmConquerers[3].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                      
#else
                msg = new MsgServer.MsgMessage("Rank [" + BossChasmConquerers[3].Item1.ToString() + "] Name [" + BossChasmConquerers[3].Item2 + "] Score [" + BossChasmConquerers[3].Item3.ToString() + "].", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

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
        public bool Attackable(uint Map, ushort X, ushort Y)
        {
            if (Map != MapID)
                return true;
            return SafeArea.IsInFan(X, Y) == true ? false : true;
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream, bool Chasm = false)
        {
            return false;
        }
        public void SortKiller(Client.GameClient client, uint Score, ConquererChasmMode Mode)
        {
            if (Process == ProcesType.Alive)
            {
                if (BossChasmConquerers[(int)Mode].Item4 == client.Player.UID)
                {
                    BossChasmConquerers[(int)Mode] = null;
                    BossChasmConquerers[(int)Mode] = new Tuple<ConquererChasmMode, string, uint, uint>(Mode, client.Player.Name, Score, client.Player.UID);
                }
                else
                {
                    if (BossChasmConquerers[(int)Mode].Item3 < Score)
                    {
                        BossChasmConquerers[(int)Mode] = null;
                        BossChasmConquerers[(int)Mode] = new Tuple<ConquererChasmMode, string, uint, uint>(Mode, client.Player.Name, Score, client.Player.UID);
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
                if (!Map.ContainMobID(29360) && !Map.ContainMobID(29300) && !Map.ContainMobID(29370) && !Map.ContainMobID(29363))
                    Process = ProcesType.Idle;
            }
            if (Process == ProcesType.Idle)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    if (DateTime.Now.Hour != Database.Server.ChasmBloodyBansheeHour.Hour && DateTime.Now.Minute == 10)
                    {
                        if (!Map.ContainMobID(29370))//Bloody Banshee
                        {
                            Database.Server.ChasmBloodyBansheeHour = DateTime.Now;
                            Database.Server.AddMapMonster(stream, Map, 29370, 238, 165, 3, 3, 1, 0, true, OdysseyServer_Project.Game.MsgFloorItem.MsgItemPacket.EffectMonsters.None);
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("[Bloody Banshee] has appeared in Giant Ghasm Go and kill it now.", Game.MsgServer.MsgMessage.MsgColor.orange, Game.MsgServer.MsgMessage.ChatMode.SlideCrosTheServer).GetArray(stream));
                            MyConsole.WriteLine("[GiantGhasm] Bloody Banshee Has Spawned.");
                        }
                    }
                    if (DateTime.Now.Hour != Database.Server.ChasmChillingSpookHour.Hour && DateTime.Now.Minute == 20)
                    {
                        if (!Map.ContainMobID(29360))//Chilling Spook
                        {
                            Database.Server.ChasmChillingSpookHour = DateTime.Now;
                            Database.Server.AddMapMonster(stream, Map, 29360, 214, 96, 3, 3, 1, 0, true, OdysseyServer_Project.Game.MsgFloorItem.MsgItemPacket.EffectMonsters.None);
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("[Chilling Spook] has appeared in Giant Ghasm Go and kill it now.", Game.MsgServer.MsgMessage.MsgColor.orange, Game.MsgServer.MsgMessage.ChatMode.SlideCrosTheServer).GetArray(stream));
                            MyConsole.WriteLine("[GiantGhasm] Chilling Spook Has Spawned.");
                        }
                    }
                    if (DateTime.Now.Hour != Database.Server.ChasmNetherTyrantHour.Hour && DateTime.Now.Minute == 40)
                    {
                        if (!Map.ContainMobID(29300))//Nether Tyrant
                        {
                            Database.Server.ChasmNetherTyrantHour = DateTime.Now;
                            Database.Server.AddMapMonster(stream, Map, 29300, 257, 136, 3, 3, 1, 0, true, OdysseyServer_Project.Game.MsgFloorItem.MsgItemPacket.EffectMonsters.None);
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("[Nether Tyrant] has appeared in Giant Ghasm Go and kill it now.", Game.MsgServer.MsgMessage.MsgColor.orange, Game.MsgServer.MsgMessage.ChatMode.SlideCrosTheServer).GetArray(stream));
                            MyConsole.WriteLine("[GiantGhasm] Nether Tyrant Has Spawned.");
                        }
                    }
                    if (DateTime.Now.Hour != Database.Server.ChasmDragonWraithHour.Hour && DateTime.Now.Minute == 50)
                    {
                        if (!Map.ContainMobID(29363))//Dragon Wraith
                        {
                            Database.Server.ChasmDragonWraithHour = DateTime.Now;
                            Database.Server.AddMapMonster(stream, Map, 29363, 204, 138, 3, 3, 1, 0, true, OdysseyServer_Project.Game.MsgFloorItem.MsgItemPacket.EffectMonsters.None);
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("[Dragon Wraith] has appeared in Giant Ghasm Go and kill it now.", Game.MsgServer.MsgMessage.MsgColor.orange, Game.MsgServer.MsgMessage.ChatMode.SlideCrosTheServer).GetArray(stream));
                            MyConsole.WriteLine("[GiantGhasm] Dragon Wraith Has Spawned.");
                        }
                    }
                }
            }
        }
        public void Calculate()
        {
            if (Process == ProcesType.Alive)
                Process = ProcesType.Idle;
            foreach (var conquqer in BossChasmConquerers)
            {
                if (conquqer.Item1 == ConquererChasmMode.MobsKiller)
                {
                    if (Server.GamePoll.ContainsKey(conquqer.Item4))
                    {
                        Server.GamePoll[conquqer.Item4].Player.ConquerPoints += 10 * conquqer.Item3;
                    }
                }
                else
                {
                    if (Server.GamePoll.ContainsKey(conquqer.Item4))
                    {
                        Server.GamePoll[conquqer.Item4].Player.ConquerPoints += 5000;
                    }
                }
            }
        }
    }
}
