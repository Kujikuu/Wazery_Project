using System;
using System.Collections.Generic;
using System.Linq;
using LightConquer_Project.Game.MsgServer;
using LightConquer_Project.Game.MsgFloorItem;
using LightConquer_Project.Game.MsgNpc;
using LightConquer_Project.Database;
using LightConquer_Project.Game.MsgTournaments;
using LightConquer_Project.Client;

namespace LightConquer_Project.Game.MsgMonster
{
    public enum ConquererMode
    {
        MobsKiller = 0,
        BansheeKiller = 1,
        SpookKiller = 2,
        NemsisKiller = 3
    }
    public unsafe class MonsterRole : Role.IMapObj
    {
        public const ushort
    UintStamp = 4,
    Uint_Mesh = 8,
    Uint_UID = 12,
    BitVector32 = 26,
    Uint16_HitPoints = 94,//90
    Uint16_Level = 100,
    Uint16_X = 102,
    Uint16_Y = 104,
    Byte_Fascing = 108,
    Byte_Action = 109,
    Byte_Boss = 203,

    Str_Count = 257,//246,//244,
    Str_NameLenght = 258,//247,//245,
    Str_Name = 259,//248,//246,
    Uint16_PLenght = 260;//249;//247;

        public DateTime RemoveFloor = DateTime.Now;
        public int StampFloorSeconds = 0;
        public class ScoreBoard
        {
            public string Name;
            public uint ScoreDmg;
        }
        public Dictionary<uint, ScoreBoard> Scores = new Dictionary<uint, ScoreBoard>();

        public static List<uint> SpecialMonsters = new List<uint>()
        {
            20070,
            3130,
            3134,
            20300,
            213883,
            20300,
            20160,
            29370,
            29360,
            29300,
            29363
        };

        public Client.GameClient AttackerScarofEarthl;
        public Database.MagicType.Magic ScarofEarthl;

        public int ExtraDamage { get { return Family.extra_damage; } }
        public int BattlePower { get { return Family.extra_battlelev; } }
        public bool AllowDynamic { get; set; }
        public bool IsTrap() { return false; }
        public uint IndexInScreen { get; set; }

        public Client.GameClient OwnerFloor;
        public Database.MagicType.Magic DBSpell;
        public ushort SpellLevel = 0;
        public DateTime FloorStampTimer = new DateTime();
        public bool IsFloor = false;
        public Game.MsgFloorItem.MsgItemPacket FloorPacket;
        public Tuple<ConquererMode, string, uint, uint>[] BossConquerers = new Tuple<ConquererMode, string, uint, uint>[4];
        public ProcesType Process { get; set; }


        public bool BlackSpot = false;
        public Extensions.Time32 Stamp_BlackSpot = new Extensions.Time32();


        public int SizeAdd { get { return Family.AttackRange; } }

        public byte PoisonLevel = 0;

        private Extensions.Time32 DeadStamp = new Extensions.Time32();
        private Extensions.Time32 FadeAway = new Extensions.Time32();
        public Extensions.Time32 RespawnStamp = new Extensions.Time32();
        public Extensions.Time32 MoveStamp = new Extensions.Time32();
        public Dictionary<uint, MsgBossHarmRankingEntry> Hunters = new Dictionary<uint, MsgBossHarmRankingEntry>();

        public bool CanRespawn(Role.GameMap map)
        {
            Extensions.Time32 Now = Extensions.Time32.Now;
            if (Now > RespawnStamp)
            {
                if (!map.MonsterOnTile(RespawnX, RespawnY))
                {
                    return true;
                }
            }
            return false;

        }
        public void SendScore(ServerSockets.Packet stream, Client.GameClient killer, uint Damage)
        {
            try
            {
                if (!Program.RankableFamilyIds.Contains(Family.ID))
                    return;
                if (this.Hunters == null)
                    this.Hunters = new Dictionary<uint, MsgBossHarmRankingEntry>();
                if (this.Hunters.ContainsKey(killer.Player.UID))
                {
                    this.Hunters[killer.Player.UID].HunterScore += Damage;
                }
                else if (!this.Hunters.ContainsKey(killer.Player.UID))
                {
                    this.Hunters.Add(killer.Player.UID, new MsgBossHarmRankingEntry() { HunterUID = killer.Player.UID, HunterScore = Damage, Rank = 0, HunterName = killer.Player.Name, ServerID = killer.Player.ServerID });
                }
                if (this.Hunters.Count == 0)
                    return;
                var array = this.Hunters.Values.OrderByDescending(p => p.HunterScore).ToArray();
                for (int x = 0; x < array.Length; x++)
                {
                    array[x].Rank = (uint)(x + 1);
                }
                MsgServer.MsgBossHarmRanking Rank = new MsgServer.MsgBossHarmRanking();
                Rank.MonsterID = this.Family.ID;
                Rank.Type = MsgBossHarmRanking.RankAction.ShowRespondForTheRest;
                Rank.Hunters = array;
                this.Send(stream.CreateBossHarmRankList(Rank));
                array = null;
            }
            catch (Exception ex)
            {
                MyConsole.WriteException(ex);
                MyConsole.SaveException(ex);
            }
        }

        public void Respawn(bool SendEffect = true)
        {
            using (var rev = new ServerSockets.RecycledPacket())
            {
                var stream = rev.GetStream();

                ClearFlags(false);




                HitPoints = (uint)Family.MaxHealth;
                State = MobStatus.Idle;

                Game.MsgServer.ActionQuery action;

                action = new MsgServer.ActionQuery()
                {
                    ObjId = UID,
                    Type = MsgServer.ActionType.RemoveEntity
                };

                Send(stream.ActionCreate(&action));

                Send(GetArray(stream, false));

                if (SendEffect)
                {
                    action.Type = ActionType.ReviveMonster;
                    Send(stream.ActionCreate(&action));
                }



                if (Family.MaxHealth > ushort.MaxValue)
                {
                    Game.MsgServer.MsgUpdate Upd = new Game.MsgServer.MsgUpdate(stream, UID, 2);
                    stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, Family.MaxHealth);
                    stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, HitPoints);
                    Send(Upd.GetArray(stream));
                }

            }
        }
        public void SendSysMesage(string Messaj, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.TopLeft
          , Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.red)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream));
            }
        }
        public void SendBossSysMesage(string KillerName, int StudyPoints, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.Center
          , Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.red)
        {
#if Arabic
             SendSysMesage("The " + Name.ToString() + " has been destroyed by the team " + KillerName.ToString() + "`s ! All team members received " + StudyPoints.ToString() + " Study Points!", ChatType, color);
#else
            SendSysMesage("The " + Name.ToString() + " has been destroyed by the team " + KillerName.ToString() + "`s ! All team members received " + StudyPoints.ToString() + " Study Points!", ChatType, color);
#endif

        }
        public void Dead(ServerSockets.Packet stream, Client.GameClient killer, uint aUID, Role.GameMap GameMap)
        {
            if (Alive)
            {

                if (IsFloor)
                {

                    FloorPacket.DropType = MsgFloorItem.MsgDropID.RemoveEffect;
                    if (FloorPacket.m_ID == Game.MsgFloorItem.MsgItemPacket.Thundercloud)
                    {
                        ActionQuery _action;

                        _action = new ActionQuery()
                        {
                            ObjId = this.FloorPacket.m_UID,
                            Type = ActionType.RemoveEntity
                        };

                        this.View.SendScreen(stream.ActionCreate(&_action), this.GMap);


                        GMap.View.LeaveMap<Role.IMapObj>(this);
                        HitPoints = 0;
                        GameMap.SetMonsterOnTile(X, Y, false);
                    }
                    else if (FloorPacket.m_ID == Game.MsgFloorItem.MsgItemPacket.AuroraLotus)
                    {
                        byte revivers = SpellLevel >= 6 ? (byte)2 : (byte)1;
                        foreach (var user in View.Roles(GameMap, Role.MapObjectType.Player))
                        {
                            if (revivers == 0)
                                break;
                            if (user.Alive == false)
                            {
                                if (Role.Core.GetDistance(user.X, user.Y, X, Y) < 5)
                                {
                                    revivers--;
                                    var player = user as Role.Player;
                                    if (player.ContainFlag(MsgUpdate.Flags.SoulShackle) == false)
                                        player.Revive(stream);

                                }
                            }
                            user.Send(GetArray(stream, false));
                        }
                        GMap.View.LeaveMap<Role.IMapObj>(this);
                    }
                    else if (FloorPacket.m_ID == Game.MsgFloorItem.MsgItemPacket.FlameLotus)
                    {
                        FloorPacket.DropType = MsgFloorItem.MsgDropID.RemoveEffect;

                        foreach (var user in View.Roles(GameMap, Role.MapObjectType.Player))
                        {
                            if (user.UID != OwnerFloor.Player.UID && Role.Core.GetDistance(user.X, user.Y, this.X, this.Y) < 5)
                            {
                                var player = user as Role.Player;

                                Game.MsgServer.MsgSpellAnimation.SpellObj AnimationObj;
                                Game.MsgServer.AttackHandler.Calculate.Magic.OnPlayer(this.OwnerFloor.Player, player, this.DBSpell, out AnimationObj);
                                Game.MsgServer.AttackHandler.ReceiveAttack.Player.Execute(AnimationObj, this.OwnerFloor, player);
                                AnimationObj.Damage = Game.MsgServer.AttackHandler.Calculate.Base.CalculateSoul(AnimationObj.Damage, 0);

                                InteractQuery Attack = new InteractQuery();
                                Attack.UID = this.UID;
                                Attack.OpponentUID = player.UID;
                                Attack.Damage = (int)AnimationObj.Damage;
                                Attack.Effect = AnimationObj.Effect;
                                Attack.X = player.X;
                                Attack.Y = player.Y;
                                Attack.AtkType = MsgAttackPacket.AttackID.Physical;

                                stream.InteractionCreate(&Attack);

                                player.View.SendView(stream, true);

                            }
                            user.Send(this.GetArray(stream, false));
                        }
                        foreach (var obj in View.Roles(GameMap, Role.MapObjectType.Monster))
                        {
                            if (obj.UID != this.UID && Role.Core.GetDistance(obj.X, obj.Y, this.X, this.Y) < 5)
                            {
                                var monster = obj as Game.MsgMonster.MonsterRole;

                                Game.MsgServer.MsgSpellAnimation.SpellObj AnimationObj;
                                Game.MsgServer.AttackHandler.Calculate.Magic.OnMonster(this.OwnerFloor.Player, monster, this.DBSpell, out AnimationObj);
                                Game.MsgServer.AttackHandler.ReceiveAttack.Monster.Execute(stream, AnimationObj, monster.OwnerFloor, monster);
                                AnimationObj.Damage = Game.MsgServer.AttackHandler.Calculate.Base.CalculateSoul(AnimationObj.Damage, 0);

                                InteractQuery Attack = new InteractQuery();
                                Attack.UID = this.UID;
                                Attack.OpponentUID = this.UID;
                                Attack.Damage = (int)AnimationObj.Damage;
                                Attack.Effect = AnimationObj.Effect;
                                Attack.X = this.X;
                                Attack.Y = this.Y;
                                Attack.AtkType = MsgAttackPacket.AttackID.Physical;

                                stream.InteractionCreate(&Attack);


                                monster.View.SendScreen(stream, this.OwnerFloor.Map);

                            }

                        }
                        GMap.View.LeaveMap<Role.IMapObj>(this);
                    }
                    HitPoints = 0;
                    GameMap.SetMonsterOnTile(X, Y, false);
                    return;
                }


                RespawnStamp = Extensions.Time32.Now.AddSeconds(8 + Family.RespawnTime);

                if (BlackSpot)
                {
                    Send(stream.BlackspotCreate(false, UID));
                    BlackSpot = false;
                }
                ClearFlags(false);
                HitPoints = 0;
                AddFlag(MsgServer.MsgUpdate.Flags.Dead, Role.StatusFlagsBigVector32.PermanentFlag, true);
                DeadStamp = Extensions.Time32.Now;

                InteractQuery action = new InteractQuery()
                {
                    UID = aUID,
                    KilledMonster = true,
                    X = this.X,
                    Y = this.Y,
                    AtkType = MsgAttackPacket.AttackID.Death,
                    OpponentUID = UID
                };



                if (killer != null)
                {
                    //if (Map == 1015)//dis city map 2
                    //{
                    //    killer.TotalSoulsBI++;
                    //    if (killer.Inventory.Contain(721716, 1, 1))
                    //    {
                    //        killer.TotalSoulsBI++;
                    //    }
                    //    if (killer.TotalSoulsBI % 10000 == 0)
                    //    {
                    //        killer.SendSysMesage("You`ve killed " + killer.TotalSoulsBI + " so far, when you reach 25,000, u can claim your reward!", MsgMessage.ChatMode.TopLeft);
                    //    }
                    //    if (killer.TotalSoulsBI >= 25000)
                    //    {
                    //        killer.SendSysMesage("You`ve killed 25,000 soul, Go find the PointsRewards in TwinCity (463,366) to claim your reward!", MsgMessage.ChatMode.TopLeft);

                    //    }
                    //}
                    if (Map == 1081)
                    {
                        if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.SpeedHunterGame
                            && Game.MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                            killer.Player.SpeedHunterGamePoints += 1;

#if Arabic
                         killer.SendSysMesage("You received 10 SpeedHunterGame.");
#else
                        killer.SendSysMesage("You received 1 SpeedHunterGame, You`ve: " + killer.Player.SpeedHunterGamePoints + "");
#endif

                    }
                    //                    if (Map == 1080)
                    //                    {
                    //                        if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.SpeedHunterGame
                    //                            && Game.MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                    //                            killer.Player.SpeedHunterGamePoints += 20;
                    //#if Arabic
                    //                           killer.SendSysMesage("You received 20 SpeedHunterGame.");
                    //#else
                    //                        killer.SendSysMesage("You received 20 SpeedHunterGame, You`ve: " + killer.Player.SpeedHunterGamePoints + "");
                    //#endif

                    //                    }
                    //                    if (Map == 2060)
                    //                    {
                    //                        if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.SpeedHunterGame
                    //                             && Game.MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                    //                            killer.Player.SpeedHunterGamePoints += 30;
                    //#if Arabic
                    //                        killer.SendSysMesage("You received 30 SpeedHunterGame.");
                    //#else
                    //                        killer.SendSysMesage("You received 30 SpeedHunterGame, You`ve: " + killer.Player.SpeedHunterGamePoints + "");
                    //#endif

                    //                    }
                    #region BigHunt
                    if (killer.Inventory.Contain(3304694, 1, 1))
                    {
                        if (Map == 1011)//dis city map 2
                        {
                            killer.Player.bigphoenix++;
                        }
                        if (Map == 1020)//dis city map 2
                        {
                            killer.Player.bigape++;
                        }
                        if (Map == 1000)//dis city map 2
                        {
                            killer.Player.bigdesert++;
                        }
                        if (Map == 1015)//dis city map 2
                        {
                            killer.Player.bigbird++;
                        }
                        if (killer.Player.bigphoenix >= 30000 && killer.Player.bigape >= 30000 && killer.Player.bigdesert >= 30000 && killer.Player.bigbird >= 30000)
                            killer.SendSysMesage("congratulations! you have completed this stage get your next token by right click on this one.");
                    }
                    if ((killer.Inventory.Contain(3301294, 1)) && (killer.Inventory.Contain(3301294, 1, 1)))
                    {
                        if (killer.Player.Map == 1001)
                            killer.Player.bigmystic++;
                        if (Family.ID == 6)
                            killer.Player.bigwinged++;
                        if (Family.ID == 11)
                            killer.Player.biggiant++;
                        if (Family.ID == 15)
                            killer.Player.bighill++;
                        if (killer.Player.bighill >= 30000 && killer.Player.biggiant >= 30000 && killer.Player.bigwinged >= 30000 && killer.Player.bigmystic >= 30000)
                            killer.SendSysMesage("congratulations! you have completed this stage get your next token by right click on this one.");
                    }
                    if ((killer.Inventory.Contain(3301295, 1)) && (killer.Inventory.Contain(3301295, 1, 1)))
                    {
                        if (Family.ID == 19 && killer.Player.bighawk < 30000)
                            killer.Player.bighawk++;
                        if (Family.ID == 113 && killer.Player.bigfire < 30000)
                            killer.Player.bigfire++;
                        if (Family.ID == 56 && killer.Player.bigbloody < 30000)
                            killer.Player.bigbloody++;
                        if (killer.Player.bighawk >= 30000 && killer.Player.bighawk >= 30000 && killer.Player.bighawk >= 30000)
                            killer.SendSysMesage("congratulations! you have completed all stage, come back tomorrow to get you other tokens or quest.");
                    }
                    #endregion
                    /*                    #region TotalSouls

                                        killer.TotalSouls++;

                                        //if (killer.TotalSouls % 10000 == 0)
                                        //{
                                        //    killer.SendSysMesage("You`ve killed " + killer.TotalSouls + " so far, when you reach 25,000, u can claim your reward!", MsgMessage.ChatMode.TopLeft);
                                        //}
                                        //if (killer.TotalSouls >= 25000)
                                        //{
                                        //    killer.SendSysMesage("You`ve killed 25,000 soul, Go find the PointsRewards in TwinCity (463,366) to claim your reward!", MsgMessage.ChatMode.TopLeft);

                                        //}
                                        #endregion

                                        #region TotalSoulsBI
                                        if (Map == 1015)//dis city map 2
                                        {
                                            killer.TotalSoulsBI++;
                                            if (killer.Inventory.Contain(721716, 1, 1))
                                            {
                                                killer.TotalSoulsBI++;
                                            }
                                            if (killer.TotalSoulsBI % 10000 == 0)
                                            {
                                                killer.SendSysMesage("You`ve killed " + killer.TotalSoulsBI + " so far, when you reach 25,000, u can claim your reward!", MsgMessage.ChatMode.TopLeft);
                                            }
                                            if (killer.TotalSoulsBI >= 25000)
                                            {
                                                killer.SendSysMesage("You`ve killed 25,000 soul, Go find the PointsRewards in TwinCity (463,366) to claim your reward!", MsgMessage.ChatMode.TopLeft);

                                            }
                                        }
                                        #endregion
                                        #region TotalSoulsTC
                                        if (Map == 1002)//dis city map 2
                                        {
                                            if (killer.Inventory.Contain(721715, 1, 1))
                                            {
                                                killer.TotalSoulsTC++;


                                                if (killer.TotalSoulsTC >= 25000)
                                                {
                                                    killer.SendSysMesage("You`ve killed 25,000 soul, Go find the PointsRewards in TwinCity (463,366) to claim your reward!", MsgMessage.ChatMode.TopLeft);

                                                }
                                            }
                                        }
                                        #endregion
                                        #region TotalSoulsDC
                                        if (Map == 1000)//dis city map 2
                                        {
                                            if (killer.Inventory.Contain(721720, 1, 1))
                                            {
                                                killer.TotalSoulsDC++;


                                                if (killer.TotalSoulsDC >= 25000)
                                                {
                                                    killer.SendSysMesage("You`ve killed 25,000 soul, Go find the PointsRewards in TwinCity (463,366) to claim your reward!", MsgMessage.ChatMode.TopLeft);

                                                }
                                            }
                                        }
                                        #endregion
                                        #region TotalSoulsPC
                                        if (Map == 1011)//dis city map 2
                                        {
                                            if (killer.Inventory.Contain(721718, 1, 1))
                                            {
                                                killer.TotalSoulsPC++;


                                                if (killer.TotalSoulsPC >= 25000)
                                                {
                                                    killer.SendSysMesage("You`ve killed 25,000 soul, Go find the PointsRewards in TwinCity (463,366) to claim your reward!", MsgMessage.ChatMode.TopLeft);

                                                }
                                            }
                                        }
                                        #endregion
                                        #region TotalSoulsAC
                                        if (Map == 1020)//dis city map 2
                                        {
                                            if (killer.Inventory.Contain(721719, 1, 1))
                                            {
                                                killer.TotalSoulsAC++;

                                                if (killer.TotalSoulsAC >= 25000)
                                                {
                                                    killer.SendSysMesage("You`ve killed 25,000 soul, Go find the PointsRewards in TwinCity (463,366) to claim your reward!", MsgMessage.ChatMode.TopLeft);

                                                }
                                            }
                                        }
                                        #endregion*/
                    #region Emerald
                    if (Family.ID == 0015)
                    {
                        if (Role.MyMath.Success(5))
                        {
                            DropItemID(killer, 1080001, stream);

                        }
                    }
                    #endregion
                    #region DemonBox
                    if (killer.MyHouse != null && killer.Player.DynamicID == killer.Player.UID)
                    {
                        if (Family.ID == 2435)//HeavenDemonBox
                        {
                            if (Role.Core.Rate(1, 10000))
                            {
                                DropItemID(killer, 720679, stream);
                                killer.CreateBoxDialog("You killed a Heaven Demon and found a Frost Gold Pack (69,000,000 Silvers)!");
                                Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Frost Gold Pack (69,000,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));
                            }
                            else
                            {
                                if (Role.Core.Rate(10, 9999))
                                {
                                    DropItemID(killer, 720678, stream);
                                    killer.CreateBoxDialog("You killed a Heaven Demon and found a Life Gold Pack (13,500,000 Silvers)!");
                                    Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Life Gold Pack (13,500,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));
                                }
                                else
                                {
                                    if (Role.Core.Rate(3700, 9989))
                                    {
                                        DropItemID(killer, 720677, stream);
                                        killer.CreateBoxDialog("You killed a Heaven Demon and found a Blood Gold Pack (1,000,000 Silvers)!");
                                    }
                                    else
                                    {
                                        if (Role.Core.Rate(1289, 6289))
                                        {
                                            DropItemID(killer, 720676, stream);
                                            killer.CreateBoxDialog("You killed a Heaven Demon and found a Soul Gold Pack (500,000 Silvers)!");
                                        }
                                        else
                                        {
                                            if (Role.Core.Rate(1000, 5000))
                                            {
                                                DropItemID(killer, 720675, stream);

                                                killer.CreateBoxDialog("You killed a Heaven Demon and found a Ghost Gold Pack (250,000 Silvers)!");
                                            }
                                            else
                                            {
                                                DropItemID(killer, 720680, stream);
                                                killer.CreateBoxDialog("You killed a Heaven Demon and found a Heaven Pill equal to the EXP of 2 and a half EXP Balls!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (Family.ID == 2436)//ChaosDemonBox
                        {
                            if (Role.Core.Rate(1, 10000))
                            {
                                DropItemID(killer, 720685, stream);

                                killer.CreateBoxDialog("You killed a Chaos Demon and found a Nimbus Gold Pack (138,000,000 Silvers)!");
                                Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Nimbus Gold Pack (138,000,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));

                            }
                            else
                            {
                                if (Role.Core.Rate(10, 9999))
                                {
                                    DropItemID(killer, 720684, stream);


                                    killer.CreateBoxDialog("You killed a Chaos Demon and found a Butterfly Gold Pack (27,000,000 Silvers)!");
                                    Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Butterfly Gold Pack (27,000,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));

                                }
                                else
                                {
                                    if (Role.Core.Rate(3700, 9989))
                                    {
                                        DropItemID(killer, 720683, stream);
                                        killer.CreateBoxDialog("You killed a Chaos Demon and found a Heart Gold Pack (2,000,000 Silvers)!");
                                    }
                                    else
                                    {
                                        if (Role.Core.Rate(1289, 6289))
                                        {
                                            DropItemID(killer, 720682, stream);
                                            killer.CreateBoxDialog("You killed a Chaos Demon and found a Flower Gold Pack (1,000,000 Silvers)!");
                                        }
                                        else
                                        {
                                            if (Role.Core.Rate(1000, 5000))
                                            {
                                                DropItemID(killer, 720681, stream);
                                                killer.CreateBoxDialog("You killed a Chaos Demon and found a Deity Gold Pack (500,000 Silvers)!");
                                            }
                                            else
                                            {
                                                DropItemID(killer, 720686, stream);
                                                killer.CreateBoxDialog("You killed a Chaos Demon and found a Mystery Pill equal to the EXP of 2 and 1/3 EXP Balls!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (Family.ID == 2437)//sacreddemon
                        {
                            if (Role.Core.Rate(1, 10000))
                            {
                                DropItemID(killer, 720691, stream);
                                killer.CreateBoxDialog("You killed a Sacred Demon and found a Kylin Gold Pack (276,000,000 Silvers)");
                                Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Kylin Gold Pack (276,000,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));
                            }
                            else
                            {
                                if (Role.Core.Rate(10, 9999))
                                {
                                    DropItemID(killer, 720690, stream);
                                    killer.CreateBoxDialog("You killed a Sacred Demon and found a Rainbow Gold Pack (54,000,000 Silvers)!");
                                    Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Rainbow Gold Pack (54,000,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));
                                }
                                else
                                {
                                    if (Role.Core.Rate(3700, 9989))
                                    {
                                        DropItemID(killer, 720689, stream);

                                        killer.CreateBoxDialog("You killed a Sacred Demon and found a Shadow Gold Pack (4,000,000 Silvers)!");
                                    }
                                    else
                                    {
                                        if (Role.Core.Rate(1289, 6289))
                                        {
                                            DropItemID(killer, 720688, stream);
                                            killer.CreateBoxDialog("You killed a Sacred Demon and found a Jewel Gold Pack (2,000,000 Silvers)!");
                                        }
                                        else
                                        {
                                            if (Role.Core.Rate(1000, 5000))
                                            {
                                                DropItemID(killer, 720687, stream);

                                                killer.CreateBoxDialog("You killed a Sacred Demon and found a Cloud Gold Pack (1,000,000 Silvers)!");
                                            }
                                            else
                                            {
                                                DropItemID(killer, 720692, stream);
                                                killer.CreateBoxDialog("You killed a Sacred Demon and found a Wind Pill equal to the EXP of 5 EXP Balls!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (Family.ID == 2438)//aurorademonbox
                        {
                            if (Role.Core.Rate(1, 10000))
                            {
                                DropItemID(killer, 720697, stream);

                                killer.CreateBoxDialog("You killed an Aurora Demon and found a Pilgrim Gold Pack (69,000,0000 Silvers)!");
                                Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " got a Pilgrim Gold Pack (69,000,0000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));

                            }
                            else
                            {
                                if (Role.Core.Rate(10, 9999))
                                {
                                    DropItemID(killer, 720696, stream);
                                    killer.CreateBoxDialog("You killed an Aurora Demon and found a Zephyr Gold Pack (135,000,000 Silvers)!");
                                    Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Zephyr Gold Pack (135,000,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));

                                }
                                else
                                {
                                    if (Role.Core.Rate(3700, 9989))
                                    {
                                        DropItemID(killer, 720695, stream);
                                        killer.CreateBoxDialog("You killed an Aurora Demon and found an Earth Gold Pack (1,000,0000 Silvers)!");
                                        Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found an Earth Gold Pack (1,000,0000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));

                                    }
                                    else
                                    {
                                        if (Role.Core.Rate(1289, 6289))
                                        {
                                            DropItemID(killer, 720694, stream);
                                            killer.CreateBoxDialog("You killed an Aurora Demon and found a Moon Gold Pack (5,000,000 Silvers)!");
                                        }
                                        else
                                        {
                                            if (Role.Core.Rate(1000, 5000))
                                            {
                                                DropItemID(killer, 720693, stream);
                                                killer.CreateBoxDialog("You killed an Aurora Demon and found a Fog Gold Pack (2,500,000 Silvers)!");
                                            }
                                            else
                                            {
                                                DropItemID(killer, 720698, stream);
                                                killer.CreateBoxDialog("You killed an Aurora Demon and got a Wind Pill equal to the EXP of 8 and 1/3 EXP Balls!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (Family.ID == 2420)//demon
                        {
                            if (Role.Core.Rate(1, 10000))
                            {
                                DropItemID(killer, 720654, stream);

                                killer.CreateBoxDialog("You killed a Demon and found a Joy Gold Pack (1,380,000 Silvers)!");
                                Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Joy Gold Pack (1,380,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));

                            }
                            else
                            {
                                if (Role.Core.Rate(10, 9999))
                                {
                                    DropItemID(killer, 720653, stream);

                                    killer.CreateBoxDialog("You killed a Demon and found a Dream Gold Pack (270,000 Silvers)!");
                                }
                                else
                                {
                                    if (Role.Core.Rate(3700, 9989))
                                    {
                                        DropItemID(killer, 720655, stream);
                                        killer.CreateBoxDialog("You killed a Demon and found a Mammon Gold Pack (20,000 Silvers)!");
                                    }
                                    else
                                    {
                                        if (Role.Core.Rate(1289, 6289))
                                        {
                                            DropItemID(killer, 720656, stream);
                                            killer.CreateBoxDialog("You killed a Demon and found a Mascot Gold Pack (10,000 Silvers)!");
                                        }
                                        else
                                        {
                                            if (Role.Core.Rate(1000, 5000))
                                            {
                                                DropItemID(killer, 720657, stream);
                                                killer.CreateBoxDialog("You killed a Demon and found a Hope Gold Pack (5,000 Silvers)!");
                                            }
                                            else
                                            {
                                                DropItemID(killer, 720668, stream);
                                                killer.CreateBoxDialog("You killed a Demon and found a Magic Ball equal to the EXP of 1/6 of an EXP Ball!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (Family.ID == 2421)//ancient
                        {
                            if (Role.Core.Rate(1, 10000))
                            {
                                killer.CreateBoxDialog("You killed a Ancient Demon and found a Mystic Gold Pack (6,900,000 Silvers)!");
                                DropItemID(killer, 720662, stream);
                                Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Mystic Gold Pack (6,900,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));

                            }
                            else
                            {
                                if (Role.Core.Rate(10, 9999))
                                {
                                    DropItemID(killer, 720661, stream);

                                    killer.CreateBoxDialog("You killed a Ancient Demon and found a Pure Gold Pack (1,350,000 Silvers)!");
                                }
                                else
                                {
                                    if (Role.Core.Rate(3700, 9989))
                                    {
                                        DropItemID(killer, 720660, stream);
                                        killer.CreateBoxDialog("You killed a Ancient Demon and found a Legend Gold Pack (100,000 Silvers)!");
                                    }
                                    else
                                    {
                                        if (Role.Core.Rate(1289, 6289))
                                        {
                                            DropItemID(killer, 720659, stream);

                                            killer.CreateBoxDialog("You killed a Ancient Demon and found a Sweet Gold Pack (50,000 Silvers)!");
                                        }
                                        else
                                        {
                                            if (Role.Core.Rate(1000, 5000))
                                            {
                                                DropItemID(killer, 720658, stream);

                                                killer.CreateBoxDialog("You killed a Ancient Demon and found a Festival Gold Pack (25,000 Silvers)!");
                                            }
                                            else
                                            {
                                                DropItemID(killer, 720669, stream);
                                                killer.CreateBoxDialog("You killed the Ancient Demon and found a Super Ball equal to the EXP of 5/6 of an EXP Ball!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (Family.ID == 2422)
                        {
                            if (Role.Core.Rate(1, 10000))
                            {
                                DropItemID(killer, 720667, stream);
                                killer.CreateBoxDialog("You killed a Flood Demon and found a Fantasy Gold Pack (13,800,000 Silvers)!");
                                Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Fantasy Gold Pack (13,800,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));
                            }
                            else
                            {
                                if (Role.Core.Rate(10, 9999))
                                {
                                    DropItemID(killer, 720666, stream);

                                    killer.CreateBoxDialog("You killed a Flood Demon and found a Star Gold Pack (2,700,000 Silvers)!");
                                    Program.SendGlobalPackets.Enqueue(new MsgMessage("" + killer.Player.Name + " found a Star Gold Pack (2,700,000 Silvers)!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center).GetArray(stream));
                                }
                                else
                                {
                                    if (Role.Core.Rate(1289, 6289))
                                    {
                                        DropItemID(killer, 720664, stream);

                                        killer.CreateBoxDialog("You killed a Flood Demon and found a Flare Gold Pack (100,000 Silvers)!");
                                    }
                                    else
                                    {
                                        if (Role.Core.Rate(1000, 5000))
                                        {
                                            DropItemID(killer, 720663, stream);
                                            killer.CreateBoxDialog("You killed a Flood Demon and found a Violet Gold Pack (50,000 Silvers)!");
                                        }
                                        else
                                        {
                                            DropItemID(killer, 720670, stream);
                                            killer.CreateBoxDialog("You killed the Flood Demon and found an Ultra Ball equal to EXP worth 1 and 2/3 EXP Balls!");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    if (killer.Player.QuestGUI.CheckQuest(2375, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                    {
                        uint spirites = 0;
                        if (Level < 70)
                            spirites = 1;
                        else if (Level >= 70 && Level <= 99)
                            spirites = 2;
                        else if (Level >= 100 && Level <= 119)
                            spirites = 3;
                        else if (Level >= 120 && Level < 140)
                            spirites = 4;
                        else if (Boss == 1 && Family.MaxHealth >= 1000000)
                            spirites = 1000;

                        killer.Player.DailySpiritBeadCount += spirites;
#if Arabic
                          killer.SendSysMesage("You received " + spirites + " spirites.", MsgMessage.ChatMode.System);
#else
                        killer.SendSysMesage("You received " + spirites + " spirites.", MsgMessage.ChatMode.System);
#endif

                        if (Game.MsgNpc.NpcHandler.GetDailySpiritBeadKills(killer) <= killer.Player.DailySpiritBeadCount)
                        {
                            if (!killer.Player.QuestGUI.CheckObjectives(2375, 1))
                            {
                                killer.Player.QuestGUI.IncreaseQuestObjectives(stream, 2375, 1, 1);
#if Arabic
                                 killer.CreateBoxDialog("You`ve~collected~enough~spirits,~and~you~can~use~the~bead~to~claim~a~reward,~now!");
#else
                                killer.CreateBoxDialog("You`ve~collected~enough~spirits,~and~you~can~use~the~bead~to~claim~a~reward,~now!");
#endif

                            }
                        }
                    }
                    #region DemonExterminator
                    if (killer.DemonExterminator != null)
                        killer.DemonExterminator.UppdateJar(killer, Family.ID);
                    #endregion



                    #region
                    //jmp:
                    if (killer.Player.OnXPSkill() == MsgUpdate.Flags.SuperCyclone || killer.Player.OnXPSkill() == MsgUpdate.Flags.Cyclone
                           || killer.Player.OnXPSkill() == MsgUpdate.Flags.Superman)
                    {
                        killer.Player.XPCount++;
                        killer.Player.KillCounter++;

                        if (killer.Player.OnXPSkill() != MsgServer.MsgUpdate.Flags.Normal)
                        {

                            action.KillCounter = killer.Player.KillCounter;
                            killer.Player.UpdateXpSkill();
                        }

                    }
                    else if (killer.Player.OnXPSkill() == MsgUpdate.Flags.Normal)
                    {
                        killer.Player.XPCount++;
                    }
                    else if (killer.Player.OnXPSkill() != MsgUpdate.Flags.BladeFlurry)
                    {
                        if (killer.Player.OnXPSkill() != MsgUpdate.Flags.Omnipotence)
                        {
                            killer.Player.KillCounter++;
                            if (killer.Player.KillCounter % 4 == 0)
                                killer.Player.XPCount++;
                        }
                    }
                    #endregion
                }
                Send(stream.InteractionCreate(&action));
                #region TowerOfMystery
                if (RemoveOnDead)
                {
                    AddFlag(MsgUpdate.Flags.FadeAway, 10, false);
                    GMap.View.LeaveMap<Role.IMapObj>(this);
                    if (GMap.IsFlagPresent(X, Y, Role.MapFlagType.Monster))
                        GMap.cells[X, Y] &= ~Role.MapFlagType.Monster;

                    if (killer != null)
                    {
                        if (killer.Player.TOM_StartChallenge)
                        {
                            bool finished = true;
                            foreach (var mob in killer.Player.View.Roles(Role.MapObjectType.Monster))
                                if (mob.Alive)
                                    finished = false;
                            if (finished)
                            {

                                if (killer.Player.TOMChallengeToday == 0)
                                {
                                    if (killer.Player.MyTowerOfMysteryLayer <= killer.Player.JoinTowerOfMysteryLayer)
                                        killer.Player.MyTowerOfMysteryLayer = (byte)Math.Min(killer.Player.MyTowerOfMysteryLayer + 1, 9);
                                }
                                else
                                {

                                    if (killer.Player.MyTowerOfMysteryLayerElite <= killer.Player.JoinTowerOfMysteryLayer)
                                        killer.Player.MyTowerOfMysteryLayerElite = (byte)Math.Min(killer.Player.MyTowerOfMysteryLayerElite + 1, 9);
                                }
                                killer.CreateBoxDialog("You`ve successfully defeated the devil on Tower of Mystery " + (killer.Player.JoinTowerOfMysteryLayer + 1).ToString() + "F. Hurry and go claim the Bright Tribe`s reward for you.");
                                killer.Player.TOM_FinishChallenge = true;
                                foreach (var npc in killer.Player.View.Roles(Role.MapObjectType.Npc))
                                    killer.Player.View.SendView(stream.NpcCreate(npc as Npc, 40150), true);
                            }
                        }
                    }
                }
                #endregion
                #region Map == 3935 return
                if (Map == 3935)
                    return;
                #endregion

                if (killer != null)
                {
                    #region DisCity Quest
                    if (Map == 2022)//dis city map 2
                    {
                        killer.Player.KillersDisCity += 1;
                    }
                    if (Map == 2024)// dis city map 4
                    {
                        if (Family.ID == 66432)//ultimate pluto
                        {

                            MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                            DataItem.ITEM_ID = 790001;
                            Database.ItemType.DBItem DBItem;
                            if (Database.Server.ItemsBase.TryGetValue(3004181, out DBItem))
                            {
                                DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                            }
                            DataItem.Color = Role.Flags.Color.Red;
                            ushort xx = X;
                            ushort yy = Y;
                            if (killer.Map.AddGroundItem(ref xx, ref yy, 3))
                            {
                                MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                                if (killer.Map.EnqueueItem(DropItem))
                                {
                                    DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                }
                            }
                            MsgTournaments.MsgSchedules.DisCity.KillTheUltimatePluto(killer);

                        }
                    }
                    #endregion

                    #region Titan & ganoderma
                    else if (Map == 1011 || Map == 1020)
                    {
                        if (Family.ID == 3130 || Family.ID == 3134)//titan/ ganoderma
                        {
                            if (Role.Core.Rate(50))
                            {
                                uint[] DropSpecialItems = new uint[] { Database.ItemType.MoonBox, Database.ItemType.PowerExpBall, Database.ItemType.DragonBallScroll, 3005126/*chi 500*/
                                ,3005125/*study 500*/};

                                ushort xx = X;
                                ushort yy = Y;
                                if (killer.Map.AddGroundItem(ref xx, ref yy, 4))
                                {
                                    uint IDDrop = DropSpecialItems[Program.GetRandom.Next(0, DropSpecialItems.Length)];
                                    DropItem(stream, killer.Player.UID, killer.Map, DropSpecialItems[Program.GetRandom.Next(0, DropSpecialItems.Length)], xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0);

                                    string drop_name = Database.Server.ItemsBase[IDDrop].Name;
#if Arabic
                                     SendSysMesage("The " + Name.ToString() + " has been destroyed by the " + killer.Player.Name.ToString() + " ! he received one " + drop_name + " ", Game.MsgServer.MsgMessage.ChatMode.Center, MsgMessage.MsgColor.red);
                         
#else
                                    SendSysMesage("The " + Name.ToString() + " has been destroyed by the " + killer.Player.Name.ToString() + " ! he received one " + drop_name + " ", Game.MsgServer.MsgMessage.ChatMode.Center, MsgMessage.MsgColor.red);

#endif
                                }

                            }
                        }
                    }
                    #endregion

                    #region SnowBanshee && ThrillingSpook && TeratoDragon && ChaosKing && NemesisTyrant
                    if (Boss > 0 && (Family.ID == 6643/*SwordMaster*/ || Family.ID == 20070/*SnowBashee*/ || Family.ID == 20160/*Thrilling Spook*/ || Family.ID == 20060/*TeratoDragon*/))
                    {
                        #region Drop = DragonBallScroll
                        for (int x = 0; x < 2; x++)
                        {
                            if (x <= 1 || (x > 2 && Role.Core.Rate(45)))
                            {
                                uint id = Database.ItemType.DragonBallScroll;

                                MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                                DataItem.ITEM_ID = id;
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(id, out DBItem))
                                {
                                    DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                                }
                                DataItem.Color = Role.Flags.Color.Red;
                                ushort xx = (ushort)Program.GetRandom.Next(X - 7, X + 7);
                                ushort yy = (ushort)Program.GetRandom.Next(Y - 7, Y + 7);
                                if (killer.Map.AddGroundItem(ref xx, ref yy, 3))
                                {
                                    MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                                    if (killer.Map.EnqueueItem(DropItem))
                                    {

                                        DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Drop = DragonBall
                        for (int x = 0; x < 5; x++)
                        {
                            if (x <= 5 || (x > 5 && Role.Core.Rate(45)))
                            {
                                uint id = Database.ItemType.DragonBall;

                                MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                                DataItem.ITEM_ID = id;
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(id, out DBItem))
                                {
                                    DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                                }
                                DataItem.Color = Role.Flags.Color.Red;
                                ushort xx = (ushort)Program.GetRandom.Next(X - 7, X + 7);
                                ushort yy = (ushort)Program.GetRandom.Next(Y - 7, Y + 7);
                                if (killer.Map.AddGroundItem(ref xx, ref yy, 3))
                                {
                                    MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                                    if (killer.Map.EnqueueItem(DropItem))
                                    {

                                        DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Drop = Meteor
                        for (int x = 0; x < 20; x++)
                        {
                            if (x <= 20 || (x > 20 && Role.Core.Rate(60)))
                            {
                                uint id = Database.ItemType.Meteor;

                                MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                                DataItem.ITEM_ID = id;
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(id, out DBItem))
                                {
                                    DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                                }
                                DataItem.Color = Role.Flags.Color.Red;
                                ushort xx = (ushort)Program.GetRandom.Next(X - 7, X + 7);
                                ushort yy = (ushort)Program.GetRandom.Next(Y - 7, Y + 7);
                                if (killer.Map.AddGroundItem(ref xx, ref yy, 3))
                                {
                                    MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                                    if (killer.Map.EnqueueItem(DropItem))
                                    {

                                        DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Drop = MeteorScroll
                        for (int x = 0; x < 5; x++)
                        {
                            if (x <= 5 || (x > 5 && Role.Core.Rate(60)))
                            {
                                uint id = Database.ItemType.MeteorScroll;

                                MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                                DataItem.ITEM_ID = id;
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(id, out DBItem))
                                {
                                    DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                                }
                                DataItem.Color = Role.Flags.Color.Red;
                                ushort xx = (ushort)Program.GetRandom.Next(X - 7, X + 7);
                                ushort yy = (ushort)Program.GetRandom.Next(Y - 7, Y + 7);
                                if (killer.Map.AddGroundItem(ref xx, ref yy, 3))
                                {
                                    MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                                    if (killer.Map.EnqueueItem(DropItem))
                                    {

                                        DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Drop = PowerExpBall
                        for (int x = 0; x < 1; x++)
                        {
                            if (x <= 1)
                            {
                                uint id = Database.ItemType.PowerExpBall;

                                MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                                DataItem.ITEM_ID = id;
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(id, out DBItem))
                                {
                                    DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                                }
                                DataItem.Color = Role.Flags.Color.Red;
                                ushort xx = (ushort)Program.GetRandom.Next(X - 7, X + 7);
                                ushort yy = (ushort)Program.GetRandom.Next(Y - 7, Y + 7);
                                if (killer.Map.AddGroundItem(ref xx, ref yy, 3))
                                {
                                    MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                                    if (killer.Map.EnqueueItem(DropItem))
                                    {

                                        DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Drop = Moonbox
                        for (int x = 0; x < 2; x++)
                        {
                            if (x <= 2)
                            {
                                uint id = 721080;//Moonbox

                                MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                                DataItem.ITEM_ID = id;
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(id, out DBItem))
                                {
                                    DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                                }
                                DataItem.Color = Role.Flags.Color.Red;
                                ushort xx = (ushort)Program.GetRandom.Next(X - 7, X + 7);
                                ushort yy = (ushort)Program.GetRandom.Next(Y - 7, Y + 7);
                                if (killer.Map.AddGroundItem(ref xx, ref yy, 3))
                                {
                                    MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                                    if (killer.Map.EnqueueItem(DropItem))
                                    {

                                        DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Drop = ProficiencyToken
                        for (int x = 0; x < 2; x++)
                        {
                            if (x <= 2)
                            {
                                uint id = 722384;//ProficiencyToken

                                MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                                DataItem.ITEM_ID = id;
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(id, out DBItem))
                                {
                                    DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                                }
                                DataItem.Color = Role.Flags.Color.Red;
                                ushort xx = (ushort)Program.GetRandom.Next(X - 7, X + 7);
                                ushort yy = (ushort)Program.GetRandom.Next(Y - 7, Y + 7);
                                if (killer.Map.AddGroundItem(ref xx, ref yy, 3))
                                {
                                    MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                                    if (killer.Map.EnqueueItem(DropItem))
                                    {

                                        DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                    }
                                }
                            }
                        }
                        #endregion
                        DistributeBossPoints();
                        return;
                    }
                    #endregion




                    if ((Family.Settings & MonsterSettings.DropItemsOnDeath) == MonsterSettings.DropItemsOnDeath)
                    {
                        ushort rand = (ushort)(killer.Player.MyRandom.Next() % 1000);
                        byte count = 1;
                        #region SurpriseBox
                        if (Common.PercentSuccess(0.1))//0.1
                        {
                            Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                            np.UID = uint.MaxValue - 61;
                            np.NpcType = Role.Flags.NpcType.Talker;
                            np.Mesh = 12290;
                            np.Map = killer.Player.Map;
                            np.X = (ushort)(killer.Player.X + 3);
                            np.Y = (ushort)(killer.Player.Y + 3);
                            Database.Server.ServerMaps[np.Map].AddNpc(np);
                            killer.Player.View.Role(false);
                            killer.SendSysMesage("You have found a Surprisebox check the ground.");
                            //killer.Player.AddMapEffect(stream, 55, 109, "eddy");
                        }
                        #endregion
                        #region GenerateBossFamily
                        //if (Family.MaxHealth > 100000 && Family.MaxHealth < 7000000|| Boss == 1)
                        //{
                        //    List<uint> DropIems = Family.ItemGenerator.GenerateBossFamily();
                        //    foreach (var ids in DropIems)
                        //    {
                        //        MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                        //        DataItem.ITEM_ID = ids;
                        //        Database.ItemType.DBItem DBItem;
                        //        if (Database.Server.ItemsBase.TryGetValue(ids, out DBItem))
                        //        {
                        //            DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                        //        }
                        //        DataItem.Color = Role.Flags.Color.Red;
                        //        ushort xx = X;
                        //        ushort yy = Y;
                        //        if (killer.Map.AddGroundItem(ref xx, ref yy))
                        //        {
                        //            MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                        //            if (killer.Map.EnqueueItem(DropItem))
                        //            {
                        //                DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                        //            }
                        //        }
                        //    }
                        //    return;
                        //}
                        #endregion
                        #region GenerateGold

                        if (rand > 40 && rand < 60)
                        {
                            ushort xx = X;
                            ushort yy = Y;
                            for (byte i = 0; i < count; i++)
                            {
                                if (killer.Player.VipLevel == 6)
                                {
                                    uint ItemID = 0;
                                    uint Amount = 0;
                                    if (Map == 1002)
                                    {
                                        Amount = Family.ItemGenerator.GenerateGold(out ItemID, false, true);
                                    }
                                    else
                                    {
                                        if (Map == 1700)
                                            Amount = Family.ItemGenerator.GenerateGold(out ItemID, true);
                                        else
                                            Amount = Family.ItemGenerator.GenerateGold(out ItemID);
                                    }
                                    killer.Player.Money += Amount;
                                    killer.SendSysMesage("You`ve received " + Amount + " Money.");
                                    killer.Player.SendUpdate(stream, killer.Player.Money, Game.MsgServer.MsgUpdate.DataType.Money);

                                }
                                else if (killer.Map.AddGroundItem(ref xx, ref yy))
                                {
                                    uint ItemID = 0;
                                    uint Amount = 0;
                                    if (Map == 1002)
                                    {
                                        Amount = Family.ItemGenerator.GenerateGold(out ItemID, false, true);
                                    }
                                    else
                                    {
                                        if (Map == 1700)
                                            Amount = Family.ItemGenerator.GenerateGold(out ItemID, true);
                                        else
                                            Amount = Family.ItemGenerator.GenerateGold(out ItemID);
                                    }

                                    DropItem(stream, killer.Player.UID, killer.Map, ItemID, xx, yy, MsgFloorItem.MsgItem.ItemType.Money, Amount, false, 0);
                                }
                            }
                        }
                        #endregion
                        #region DropHPItem
                        //else if (rand > 500 && rand < 600)
                        //{

                        //    ushort xx = X;
                        //    ushort yy = Y;
                        //    for (byte i = 0; i < count; i++)
                        //    {
                        //        if (killer.Map.AddGroundItem(ref xx, ref yy))
                        //        {
                        //            uint ItemID = Family.DropHPItem;

                        //            DropItem(stream, killer.Player.UID, killer.Map, ItemID, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0);
                        //        }
                        //    }
                        //}
                        #endregion
                        #region DropMPItem
                        //else if (rand > 600 && rand < 700)
                        //{
                        //    ushort xx = X;
                        //    ushort yy = Y;
                        //    for (byte i = 0; i < count; i++)
                        //    {
                        //        if (killer.Map.AddGroundItem(ref xx, ref yy))
                        //        {
                        //            uint ItemID = Family.DropMPItem;

                        //            DropItem(stream, killer.Player.UID, killer.Map, ItemID, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0);
                        //        }
                        //    }
                        //}
                        #endregion
                        #region GenerateItemId
                        else if (rand > 700)//&& rand < 770)
                        {
                            ushort xx = X;
                            ushort yy = Y;
                            for (byte i = 0; i < count; i++)
                            {

                                Database.ItemType.DBItem DbItem = null;
                                byte ID_Quality;
                                bool ID_Special;
                                uint ID = Family.ItemGenerator.GenerateItemId(Map, out ID_Quality, out ID_Special, out DbItem);
                                if (ID != 0)
                                {
                                    bool drop = true;

                                    #region DragonBall
                                    if (ID == 1088000 && killer != null)
                                    {
                                        if (killer.Player.VipLevel == 6 && killer.Player.LootDragonBall)
                                        {
                                            if (killer.Inventory.HaveSpace(1))
                                            {
                                                killer.Inventory.Add(stream, 1088000, 1);
                                                drop = false;
                                                if (killer.Inventory.Contain(1088000, 10) && killer.Player.VipLevel >= 1)
                                                {
                                                    killer.Inventory.Remove(1088000, 10, stream);
                                                    killer.Inventory.Add(stream, 720028, 1);
                                                    killer.SendSysMesage("[VIP] DragonBall got autopacked.", MsgMessage.ChatMode.TopLeft);
                                                }
                                            }
                                            else
                                            {
                                                ActionQuery action2;
                                                action2 = new ActionQuery()
                                                {
                                                    ObjId = killer.Player.UID,
                                                    Type = ActionType.DragonBall
                                                };
                                                killer.Send(stream.ActionCreate(&action2));
                                                killer.SendSysMesage("A DragonBall dropped at at " + xx + "," + yy + "!");
                                                killer.Player.AddMapEffect(stream, xx, yy, "zf2-e248");

                                            }
                                        }
                                        SendSysMesage($"A DragonBall has dropped from {Family.Name} Killed by ~{killer.Name}~");
                                    }
                                    #endregion

                                    #region mets
                                    if (ID == 1088001 && killer != null)
                                    {
                                        if (killer.Player.VipLevel == 6 && killer.Player.LootMetero)
                                        {
                                            if (killer.Inventory.HaveSpace(1))
                                            {
                                                killer.Inventory.Add(stream, 1088001, 1);
                                                drop = false;
                                                if (killer.Inventory.Contain(1088001, 10) && killer.Player.VipLevel >= 1)
                                                {
                                                    killer.Inventory.Remove(1088001, 10, stream);
                                                    killer.Inventory.Add(stream, 720027, 1);
                                                    killer.SendSysMesage("[VIP] Meteors got autopacked.", MsgMessage.ChatMode.TopLeft);
                                                }
                                            }
                                            else
                                                killer.SendSysMesage("A monster you killed has dropped a Meteors at (" + xx + "," + yy + ")!", MsgMessage.ChatMode.Talk);
                                            killer.Player.AddMapEffect(stream, xx, yy, "zf2-e248");
                                        }
                                        SendSysMesage($"A Meteor has dropped from {Family.Name} Killed by {killer.Name}");
                                    }
                                    #endregion
                                    if (killer.Map.AddGroundItem(ref xx, ref yy) && drop)
                                    {
                                        DropItem(stream, killer.Player.UID, killer.Map, ID, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, ID_Special, ID_Quality, killer, DbItem);
                                        if (ID_Special)
                                            break;
                                    }
                                }

                            }
                        }
                        #endregion

                    }
                }
            }

        }
        private void DropItem(ServerSockets.Packet stream, uint OwnerItem, Role.GameMap map, uint ItemID, ushort XX, ushort YY, MsgFloorItem.MsgItem.ItemType typ
            , uint amount, bool special, byte ID_Quality, Client.GameClient user = null, Database.ItemType.DBItem DBItem = null)
        {
            MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();

            DataItem.ITEM_ID = ItemID;
            if (DataItem.Durability > 100)
            {
                DataItem.Durability = (ushort)Program.GetRandom.Next(100, DataItem.Durability / 10);
                DataItem.MaximDurability = DataItem.Durability;
            }

            else
            {
                DataItem.Durability = (ushort)Program.GetRandom.Next(1, 10);
                DataItem.MaximDurability = 10;
            }

            DataItem.Color = Role.Flags.Color.Red;
            if (typ == MsgFloorItem.MsgItem.ItemType.Item)
            {
                byte sockets;
                bool lucky = false;
                if (DataItem.IsEquip)
                {
                    if (!special)
                    {

                        lucky = (ID_Quality > 7); // q>unique
                        if (!lucky)
                            lucky = (DataItem.Plus = Family.ItemGenerator.GeneratePurity()) != 0;
                        if (!lucky)
                            lucky = (DataItem.Bless = Family.ItemGenerator.GenerateBless()) != 0;
                        if (!lucky)
                        {
                            if (DataItem.IsWeapon)
                            {
                                sockets = Family.ItemGenerator.GenerateSocketCount(DataItem.ITEM_ID);

                                if (sockets >= 1)
                                    DataItem.SocketOne = Role.Flags.Gem.EmptySocket;
                                else if (sockets == 2)
                                    DataItem.SocketTwo = Role.Flags.Gem.EmptySocket;
                            }
                        }
                        if (DataItem.Plus == 1)
                        {

                            user.SendSysMesage("A +1 " + Server.ItemsBase[DataItem.ITEM_ID].Name + " dropped at " + XX + "," + YY + "!");
                            user.Player.AddMapEffect(stream, XX, YY, "stipple_bdh");
                            user.Player.AddMapEffect(stream, XX, YY, "zf2-e248");


                        }
                        if (DataItem.Bless >= 1)
                        {

                            user.SendSysMesage("A - " + DataItem.Bless + " " + Server.ItemsBase[DataItem.ITEM_ID].Name + " dropped at " + XX + "," + YY + "!");
                            user.Player.AddMapEffect(stream, XX, YY, "stipple_bdh");
                            user.Player.AddMapEffect(stream, XX, YY, "zf2-e248");

                        }
                    }
                    if (DBItem != null)
                    {
                        DataItem.Durability = (ushort)Program.GetRandom.Next(1, DBItem.Durability / 10 + 10);
                        DataItem.MaximDurability = (ushort)Program.GetRandom.Next(DataItem.Durability, DBItem.Durability);
                    }
                }
                else
                {
                    if (DBItem != null)
                        DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                }

            }
            if (user != null)
            {

                if (user.Player.VipLevel == 6)
                {
                    {
                        if (user.Inventory.HaveSpace(1))
                        {
                            if (DataItem.IsEquip)
                            {
                                if (DataItem.Plus > 0 && user.Player.LootPlusItems)
                                {
                                    user.Inventory.Update(DataItem, Role.Instance.AddMode.ADD, stream);
                                    user.SendSysMesage("You`ve received +1 " + Server.ItemsBase[DataItem.ITEM_ID].Name + " !");
                                    return;
                                }
                            }

                        }
                        else
                        {
                            if (DataItem.Plus > 0)
                            {
                                user.SendSysMesage("A +1 " + Server.ItemsBase[DataItem.ITEM_ID].Name + " dropped at " + XX + "," + YY + "!");

                                return;
                            }
                            if (DataItem.Bless >= 1)
                            {
                                user.SendSysMesage("A - " + DataItem.Bless + " " + Server.ItemsBase[DataItem.ITEM_ID].Name + " dropped at " + XX + "," + YY + "!");

                                return;
                            }
                        }
                    }
                }


            }
            MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, XX, YY, typ, amount, DynamicID, Map, OwnerItem, true, map);

            if (map.EnqueueItem(DropItem))
            {
                DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
            }
        }
        public void AddFadeAway(int time, Role.GameMap map)
        {
            if (!Alive)
            {
                Extensions.Time32 timer = new Extensions.Time32(time);
                if (timer > DeadStamp.AddSeconds(5))
                {
                    if (AddFlag(MsgServer.MsgUpdate.Flags.FadeAway, Role.StatusFlagsBigVector32.PermanentFlag, true))
                    {
                        FadeAway = timer;

                    }
                }
            }
        }
        public unsafe bool RemoveView(int time, Role.GameMap map)
        {
            if (ContainFlag(MsgServer.MsgUpdate.Flags.FadeAway) && State != MobStatus.Respawning)
            {
                Extensions.Time32 timer = new Extensions.Time32(time);
                if (timer > FadeAway.AddSeconds(3))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        ActionQuery action;

                        action = new ActionQuery()
                        {
                            ObjId = UID,
                            Type = ActionType.RemoveEntity
                        };

                        Send(stream.ActionCreate(&action));
                    }

                    State = MobStatus.Respawning;

                    map.View.MoveTo<Role.IMapObj>(this, RespawnX, RespawnY);

                    X = RespawnX;
                    Y = RespawnY;
                    Target = null;

                    return true;
                }
            }
            return false;
        }

        public void DropItemID(Client.GameClient killer, uint itemid, ServerSockets.Packet stream, byte range = 3, int mins = 0)
        {
            MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
            DataItem.ITEM_ID = itemid;
            Database.ItemType.DBItem DBItem;
            if (Database.Server.ItemsBase.TryGetValue(itemid, out DBItem))
            {
                DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
            }
            if (mins > 0)
            {
                DataItem.EndDate = DateTime.Now.AddMinutes(mins);
            }
            DataItem.Color = Role.Flags.Color.Red;
            ushort xx = X;
            ushort yy = Y;
            if (killer.Map.AddGroundItem(ref xx, ref yy, range))
            {
                MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, killer.Player.UID, true, GMap);

                if (killer.Map.EnqueueItem(DropItem))
                {
                    DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                }
            }
        }
        public MonsterFamily Family;
        public MonsterView View;
        public MobStatus State;
        public Role.Player Target = null;
        public Extensions.Time32 AttackSpeed = new Extensions.Time32();

        public Role.StatusFlagsBigVector32 BitVector;
        public void AddSpellFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool RemoveOnDead, int Secondstamp = 0)
        {
            if (BitVector.ContainFlag((int)Flag))
                BitVector.TryRemove((int)Flag);
            AddFlag(Flag, Seconds, RemoveOnDead, Secondstamp);
        }
        public bool AddFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool RemoveOnDead, int StampSeconds = 0)
        {
            if (!BitVector.ContainFlag((int)Flag))
            {
                BitVector.TryAdd((int)Flag, Seconds, RemoveOnDead, StampSeconds);
                UpdateFlagOffset();
                return true;
            }
            return false;
        }
        public bool RemoveFlag(Game.MsgServer.MsgUpdate.Flags Flag, Role.GameMap map)
        {
            if (BitVector.ContainFlag((int)Flag))
            {
                BitVector.TryRemove((int)Flag);
                UpdateFlagOffset();
                return true;
            }
            return false;
        }
        public bool UpdateFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool SetNewTimer, int MaxTime)
        {
            return BitVector.UpdateFlag((int)Flag, Seconds, SetNewTimer, MaxTime);
        }
        public void ClearFlags(bool SendScreem = false)
        {
            BitVector.GetClear();
            UpdateFlagOffset(SendScreem);
        }
        public bool ContainFlag(Game.MsgServer.MsgUpdate.Flags Flag)
        {
            return BitVector.ContainFlag((int)Flag);
        }
        private unsafe void UpdateFlagOffset(bool SendScreem = true)
        {
            if (SendScreem)
                SendUpdate(BitVector.bits, Game.MsgServer.MsgUpdate.DataType.StatusFlag);
        }

        public byte OpenBoss = 0;
        public uint Map { get; set; }
        public uint DynamicID { get; set; }

        public short GetMyDistance(ushort X2, ushort Y2)
        {
            return Role.Core.GetDistance(X, Y, X2, Y2);
        }
        public short OldGetDistance(ushort X2, ushort Y2)
        {
            return Role.Core.GetDistance(PX, PY, X2, Y2);
        }
        public bool InView(ushort X2, ushort Y2, byte distance)
        {
            return (!(OldGetDistance(X2, Y2) < distance) && GetMyDistance(X2, Y2) < distance);
        }


        public unsafe void Send(ServerSockets.Packet msg)
        {
            View.SendScreen(msg, GMap);
        }
        public void UpdateMonsterView(Role.RoleView Target, ServerSockets.Packet stream)
        {
            foreach (var player in View.Roles(GMap, Role.MapObjectType.Player))
            {
                if (InView(player.X, player.Y, MonsterView.ViewThreshold))
                    player.Send(GetArray(stream, false));
            }
        }
        public bool UpdateMapCoords(ushort New_X, ushort New_Y, Role.GameMap _map)
        {
            if (!_map.IsFlagPresent(New_X, New_Y, Role.MapFlagType.Monster))
            {
                _map.SetMonsterOnTile(X, Y, false);
                _map.SetMonsterOnTile(New_X, New_Y, true);
                _map.View.MoveTo<MonsterRole>(this, New_X, New_Y);
                X = New_X;
                Y = New_Y;
                return true;
            }
            return false;
        }
        public void RemoveRole(Role.IMapObj obj)
        {

        }
        public Role.MapObjectType ObjType { get; set; }
        public unsafe string Name = "";

        public byte Boss = 0;
        public uint Mesh = 0;
        public uint UID { get; set; }
        public byte Level = 0;
        public uint HitPoints;

        public ushort RespawnX;
        public ushort RespawnY;


        public ushort PX = 0;
        public ushort PY = 0;
        public ushort _xx;
        public ushort _yy;

        public ushort X { get { return _xx; } set { PX = _xx; _xx = value; } }
        public ushort Y { get { return _yy; } set { PY = _yy; _yy = value; } }
        public Role.Flags.ConquerAction Action = Role.Flags.ConquerAction.None;
        public Role.Flags.ConquerAngle Facing = Role.Flags.ConquerAngle.East;
        public string LocationSpawn = "";
        public Role.GameMap GMap;
        public bool RemoveOnDead = false;
        public uint PetFlag = 0;


        public unsafe void SendString(ServerSockets.Packet stream, Game.MsgServer.MsgStringPacket.StringID id, params string[] args)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = id;
            packet.UID = UID;

            packet.Strings = args;
            Send(stream.StringPacketCreate(packet));
        }
        public MonsterRole(MonsterFamily Famil, uint _UID, string locationspawn, Role.GameMap _map)
        {
            AllowDynamic = false;
            GMap = _map;
            LocationSpawn = locationspawn;
            ObjType = Role.MapObjectType.Monster;
            Name = Famil.Name;
            Family = Famil;
            UID = _UID;
            Mesh = Famil.Mesh;
            Level = (byte)Famil.Level;
            HitPoints = (uint)Famil.MaxHealth;
            View = new MonsterView(this);
            State = MobStatus.Idle;
            BitVector = new Role.StatusFlagsBigVector32(32 * 7);//5
            Boss = Family.Boss;
            Facing = (Role.Flags.ConquerAngle)Program.GetRandom.Next(0, 8);

        }
        public bool Alive { get { return HitPoints > 0; } }


        public unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, bool view)
        {
            if (IsFloor && Mesh != 980)
            {
                return stream.ItemPacketCreate(this.FloorPacket);

            }
            stream.InitWriter();

            stream.Write(Extensions.Time32.Now.Value);
            stream.Write(Mesh);
            stream.Write(UID);
            stream.ZeroFill(10);

            for (int x = 0; x < BitVector.bits.Length; x++)
                stream.Write(BitVector.bits[x]);



            stream.ZeroFill(57);



            if (Boss > 0)
            {
                if (IsFloor)
                {
                    stream.Write(StampFloorSeconds);
                }
                else
                {
                    uint key = (uint)(Family.MaxHealth / 10000);
                    if (key != 0)
                        stream.Write((uint)(HitPoints / key));
                    else
                        stream.Write((uint)(HitPoints * Family.MaxHealth));
                }
            }
            else
            {
                if (IsFloor)
                {
                    stream.Write(StampFloorSeconds);
                }
                else
                    stream.Write(HitPoints);
            }
            stream.Write((ushort)0);
            stream.Write((ushort)Level);


            stream.Write(X);
            stream.Write(Y);
            stream.Write((ushort)0);
            stream.Write((byte)Facing);
            stream.Write((byte)Action);
            stream.ZeroFill(93);

            stream.Write((byte)Boss);
            stream.ZeroFill(50);




            if (IsFloor)
            {
                stream.Write((ushort)FloorPacket.m_ID);
                stream.Write((byte)0);
                stream.Write((uint)(OwnerFloor.Player.UID));
                stream.Write((ushort)9);
            }
            else
            {
                stream.ZeroFill(7);
                stream.Write((ushort)0);
            }


            stream.Write(0);
            stream.Write(0);

            stream.Write(0);
            //

            stream.Write(0);
            stream.Write(0);
            stream.Write(0);

            stream.Write(0);
            if (IsFloor)
            {
                stream.Write(PetFlag);//3?
            }
            else
                stream.Write(0);
            stream.Write(Name, string.Empty, string.Empty, string.Empty);
            stream.Finalize(Game.GamePackets.SpawnPlayer);
            //    MyConsole.PrintPacketAdvanced(stream.Memory, stream.Size);

            return stream;
        }
        public unsafe void SendUpdate(uint[] Value, Game.MsgServer.MsgUpdate.DataType datatype)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                stream = packet.Append(stream, datatype, Value);
                stream = packet.GetArray(stream);
                Send(stream);
            }
        }
        DateTime LastScore;
        internal unsafe void SendScores(ServerSockets.Packet stream)
        {
            if (DateTime.Now < LastScore.AddSeconds(2))
                return;
            LastScore = DateTime.Now;
            if (ConfirmBoss())
            {
                View.SendScreen(new MsgMessage("*Top 5 ScoreBoard: " + Name + " *", MsgMessage.MsgColor.red, MsgMessage.ChatMode.FirstRightCorner).GetArray(stream), GMap);
                int counter = 1;
                foreach (var player in Scores.OrderByDescending(e => e.Value.ScoreDmg).Take(5))
                {
                    View.SendScreen(new MsgMessage("N° " + counter++ + ": " + player.Value.Name + " - " + player.Value.ScoreDmg, MsgMessage.MsgColor.red, MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream), GMap);
                }
            }
        }
        bool ConfirmBoss()
        {
            if (Family.ID == 6643/*SwordMaster || Family.ID == 20070/*SnowBashee*/ || Family.ID == 20160/*Thrilling Spook*/ || Family.ID == 20060/*TeratoDragon*/ || Family.ID == 20300 /*Nemmesy*/) return true;
            return false;
        }
        internal unsafe void DistributeBossPoints()
        {
            if (ConfirmBoss())
            {
                var scores = Scores.OrderByDescending(e => e.Value.ScoreDmg).Take(1).FirstOrDefault();
                if (scores.Value == null) return;
                MsgSchedules.SendSysMesage("Player " + scores.Value.Name + " has made the most damage on the " + Name + " and gained 1 boss point.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                GameClient player;
                if (Server.GamePoll.TryGetValue(scores.Key, out player))
                {
                    player.Player.BossPoints += 1;
                    player.SendSysMesage("You got 1 Boss Point!", MsgMessage.ChatMode.TopLeft);
                }
            }
        }
        internal unsafe void UpdateScores(Role.Player player, uint p)
        {
            if (ConfirmBoss())
            {
                if (!Scores.ContainsKey(player.UID))
                    Scores.Add(player.UID, new ScoreBoard() { Name = player.Name, ScoreDmg = p });
                else Scores[player.UID].ScoreDmg += p;
            }
        }


    }
}
