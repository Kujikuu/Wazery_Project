using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightConquer_Project.Game.MsgServer;
using LightConquer_Project.Database;
using System.Net.Sockets;

namespace LightConquer_Project.Client
{
    [Flags]
    public enum ServerFlag : ushort
    {
        None = 0,
        AcceptLogin = 1 << 0,
        CreateCharacter = 1 << 1,
        CreateCharacterSucces = 1 << 2,
        LoginFull = 1 << 3,
        SetLocation = 1 << 4,
        OnLoggion = 1 << 5,
        QueuesSave = 1 << 6,
        RemoveSpouse = 1 << 7,
        Disconnect = 1 << 8,
        UpdateSpouse = 1 << 9
    }
    [Flags]
    public enum Language : byte
    {
        English = 0,
        Arabic = 1
    }
    public unsafe class GameClient
    {
        public Game.MsgEvents.Events EventBase;
        public uint MobsKilled = 0, TotalMobsKilled = 0;
        internal uint TotalSouls = 0;
        //city hunt
        internal uint TotalSoulsTC = 0;
        internal uint TotalSoulsBI = 0;//
        internal uint TotalSoulsDC = 0;
        internal uint TotalSoulsPC = 0;
        internal uint TotalSoulsAC = 0;
        public string AccountName(string name)
        {
            string username = "";
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT))
            {
                cmd.Select("accounts").Where("ID", Player.UID);
                using (MySqlReader rdr = new MySqlReader(cmd))
                {
                    if (rdr.Read())
                    {
                        username = rdr.ReadString("Username");
                    }
                }
            }
            return username;
        }
        public uint HarmBossScore = 0;
        public DateTime StartQuizTimer = new DateTime();
        public int QuizRank = 0;
        public string Numberofcard;
        public string Numberofitem;
        public int GetQuizTimer()
        {
            TimeSpan now = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan old = new TimeSpan(StartQuizTimer.Ticks);
            return (int)(now.TotalSeconds - old.TotalSeconds);
        }

        public ushort QuizShowPoints = 0;
        public byte RightAnswer = 1;
        public uint NpcCpsInput;

        public Role.Instance.ExchangeShop MyExchangeShop;
        public Language MyLanguage = Language.English;

        //public string AccountName = "";

        public bool GameMaster = false;

        public byte BanCount = 0;

        public ushort OnSoulSpell = 0;
        public Database.PrestigeRanking.Entry PrestrigeEntry;
        public Role.Instance.PerfectionEffect PerfectionStatus = new Role.Instance.PerfectionEffect();
        public uint[] PrestigePoints = new uint[19];
       public void CreatePrestigePoints()
       {
           PrestigePoints = new uint[19]; 

         
           if (Player.MyJiangHu != null)
           {
               if (Player.MyJiangHu.Inner_Strength > 0 && Player.MyJiangHu.Inner_Strength <= 16200)
                   PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 30);
               else if (Player.MyJiangHu.Inner_Strength >= 16201 && Player.MyJiangHu.Inner_Strength <= 40500)
                   PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 33);
               else if (Player.MyJiangHu.Inner_Strength >= 40501 && Player.MyJiangHu.Inner_Strength <= 60750)
                   PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 36);
               else if (Player.MyJiangHu.Inner_Strength >= 60751 && Player.MyJiangHu.Inner_Strength <= 72000)
                   PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 40);
               else if (Player.MyJiangHu.Inner_Strength >= 72001 && Player.MyJiangHu.Inner_Strength <= 79200)
                   PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 45);
               else if (Player.MyJiangHu.Inner_Strength >= 79201 && Player.MyJiangHu.Inner_Strength <= 80800)
                   PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 50);
               else if (Player.MyJiangHu.Inner_Strength >= 80801 && Player.MyJiangHu.Inner_Strength <= 81000)
                   PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 60);
           }
           PrestigePoints[1] = 0;
           if (Player.MyChi != null)
           {
               foreach (var obj in Player.MyChi)
               {
                   PrestigePoints[1] += (uint)(obj.Score * 30);
                   if (obj.Rank < 50)
                       PrestigePoints[1] += (uint)obj.Score * 10;
               }
           }
           PrestigePoints[2] = 0;
            if (Player.InnerPower != null)
            {
                foreach (var obj in Player.InnerPower.Stages)
                    if (obj.UnLocked)
                        PrestigePoints[2] += 100;
            }
            PrestigePoints[4] = 0;
           if (Player.Level < 120)
               PrestigePoints[4] += (uint)Player.Level * 10;
           else if (Player.Level >= 120 && Player.Level < 130)
               PrestigePoints[4] += (uint)Player.Level * 15;
           else if (Player.Level >= 130 && Player.Level < 135)
               PrestigePoints[4] += (uint)Player.Level * 20;
           else
               PrestigePoints[4] += (uint)Player.Level * 25;

           PrestigePoints[6] = (uint)((Player.Strength + Player.Spirit + Player.Vitality + Player.Agility) * 5);
           PrestigePoints[7] = (uint)Player.Reborn * 1000;
           PrestigePoints[15] = 0;
           if (Player.SubClass != null)
           {
               foreach (var obj in Player.SubClass.src.Values)
               {
                   PrestigePoints[15] += (uint)obj.Level * 80;
                   PrestigePoints[15] += (uint)obj.Phrase * 20;
               }
           }

           PrestigePoints[16] = 0;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 0)
               PrestigePoints[16] += 50;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 15)
               PrestigePoints[16] += 50;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 30)
               PrestigePoints[16] += 50;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 45)
               PrestigePoints[16] += 50;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 60)
               PrestigePoints[16] += 50;
           PrestigePoints[17] = 0;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 0)
               PrestigePoints[17] += 50;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 15)
               PrestigePoints[17] += 50;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 30)
               PrestigePoints[17] += 50;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 45)
               PrestigePoints[17] += 50;
           if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 60)
               PrestigePoints[17] += 50;


           PrestigePoints[18] = (uint)Player.NobilityRank * 1000;
           MyPrestigePoints = 0;
           for (int x = 0; x < PrestigePoints.Length; x++)
               MyPrestigePoints += PrestigePoints[x];

           foreach (var item in Equipment.CurentEquip)
           {
               MyPrestigePoints += item.ItemPoints;
               PrestigePoints[3] += item.ItemPoints;
           }
            if (Player.Class < 100 || Player.Class > 102)//simple taoist
            {
                PrestrigeEntry = new Database.PrestigeRanking.Entry();
                PrestrigeEntry.type = Database.PrestigeRanking.GetIndex(Player.Class);
                PrestrigeEntry.UID = Player.UID;
                PrestrigeEntry.Name = Player.Name;
                PrestrigeEntry.Class = Player.Class;
                PrestrigeEntry.TotalPoints = MyPrestigePoints;
                PrestrigeEntry.Level = (byte)Player.Level;
                PrestrigeEntry.Mesh = Player.Mesh;
                PrestrigeEntry.AddInfo(this);
                for (int x = 0; x < PrestigePoints.Length; x++)
                    PrestrigeEntry.Points[x] = PrestigePoints[x];
                //if (Database.GroupServerList.MyServerInfo.ID == Player.ServerID)
                //{
                //    Database.PrestigeRanking.Remove(Player.UID);
                //    if (!Player.Name.Contains("[PM]") && !Player.Name.Contains("[GM]"))
                //    {
                //        //Database.PrestigeRanking.Ranks[PrestrigeEntry.type].AddItem(PrestrigeEntry);
                //        Database.PrestigeRanking.Ranks[Database.PrestigeRanking.Type.World].AddItem(PrestrigeEntry);
                //    }
                //}
                //PerfectionStatus.Update((int)PrestigeLevel);
                if (Player.MyGuild != null && Player.MyGuildMember != null)
                    Player.MyGuildMember.PrestigePoints = PrestrigeEntry.TotalPoints;
            }
        }
        public void UpdatePerfection()
        {
            PrestigePoints = new uint[19];


            if (Player.MyJiangHu != null)
            {
                if (Player.MyJiangHu.Inner_Strength > 0 && Player.MyJiangHu.Inner_Strength <= 16200)
                    PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 30);
                else if (Player.MyJiangHu.Inner_Strength >= 16201 && Player.MyJiangHu.Inner_Strength <= 40500)
                    PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 33);
                else if (Player.MyJiangHu.Inner_Strength >= 40501 && Player.MyJiangHu.Inner_Strength <= 60750)
                    PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 36);
                else if (Player.MyJiangHu.Inner_Strength >= 60751 && Player.MyJiangHu.Inner_Strength <= 72000)
                    PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 40);
                else if (Player.MyJiangHu.Inner_Strength >= 72001 && Player.MyJiangHu.Inner_Strength <= 79200)
                    PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 45);
                else if (Player.MyJiangHu.Inner_Strength >= 79201 && Player.MyJiangHu.Inner_Strength <= 80800)
                    PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 50);
                else if (Player.MyJiangHu.Inner_Strength >= 80801 && Player.MyJiangHu.Inner_Strength <= 81000)
                    PrestigePoints[0] = (uint)((Player.MyJiangHu.Inner_Strength / 100) * 60);
            }
            PrestigePoints[1] = 0;
            if (Player.MyChi != null)
            {
                foreach (var obj in Player.MyChi)
                {
                    PrestigePoints[1] += (uint)(obj.Score * 30);
                    if (obj.Rank < 50)
                        PrestigePoints[1] += (uint)obj.Score * 10;
                }
            }
            PrestigePoints[2] = 0;
            if (Player.InnerPower != null)
            {
                foreach (var obj in Player.InnerPower.Stages)
                    if (obj.UnLocked)
                        PrestigePoints[2] += 100;
            }
            PrestigePoints[4] = 0;
            if (Player.Level < 120)
                PrestigePoints[4] += (uint)Player.Level * 10;
            else if (Player.Level >= 120 && Player.Level < 130)
                PrestigePoints[4] += (uint)Player.Level * 15;
            else if (Player.Level >= 130 && Player.Level < 135)
                PrestigePoints[4] += (uint)Player.Level * 20;
            else
                PrestigePoints[4] += (uint)Player.Level * 25;

            PrestigePoints[6] = (uint)((Player.Strength + Player.Spirit + Player.Vitality + Player.Agility) * 5);
            PrestigePoints[7] = (uint)Player.Reborn * 1000;
            PrestigePoints[15] = 0;
            if (Player.SubClass != null)
            {
                foreach (var obj in Player.SubClass.src.Values)
                {
                    PrestigePoints[15] += (uint)obj.Level * 80;
                    PrestigePoints[15] += (uint)obj.Phrase * 20;
                }
            }

            PrestigePoints[16] = 0;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 0)
                PrestigePoints[16] += 50;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 15)
                PrestigePoints[16] += 50;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 30)
                PrestigePoints[16] += 50;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 45)
                PrestigePoints[16] += 50;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 60)
                PrestigePoints[16] += 50;
            PrestigePoints[17] = 0;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 0)
                PrestigePoints[17] += 50;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 15)
                PrestigePoints[17] += 50;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 30)
                PrestigePoints[17] += 50;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 45)
                PrestigePoints[17] += 50;
            if (MyWardrobe.Items[(uint)Role.Instance.Wardrobe.ItemsType.Mount].Count > 60)
                PrestigePoints[17] += 50;


            PrestigePoints[18] = (uint)Player.NobilityRank * 1000;
            MyPrestigePoints = 0;
            for (int x = 0; x < PrestigePoints.Length; x++)
                MyPrestigePoints += PrestigePoints[x];

            foreach (var item in Equipment.CurentEquip)
            {
                MyPrestigePoints += item.ItemPoints;
                PrestigePoints[3] += item.ItemPoints;
            }
            if (Player.Class < 100 || Player.Class > 102)//simple taoist
            {
                PrestrigeEntry = new Database.PrestigeRanking.Entry();
                PrestrigeEntry.type = Database.PrestigeRanking.GetIndex(Player.Class);
                PrestrigeEntry.UID = Player.UID;
                PrestrigeEntry.Name = Player.Name;
                PrestrigeEntry.Class = Player.Class;
                PrestrigeEntry.TotalPoints = MyPrestigePoints;
                PrestrigeEntry.Level = (byte)Player.Level;
                PrestrigeEntry.Mesh = Player.Mesh;
                PrestrigeEntry.AddInfo(this);
                for (int x = 0; x < PrestigePoints.Length; x++)
                    PrestrigeEntry.Points[x] = PrestigePoints[x];
                if (Database.GroupServerList.MyServerInfo.ID == Player.ServerID)
                {
                    Database.PrestigeRanking.Remove(Player.UID);
                    if (!Player.Name.Contains("[PM]") && !Player.Name.Contains("[GM]"))
                    {
                        Database.PrestigeRanking.Ranks[PrestrigeEntry.type].AddItem(PrestrigeEntry);
                        Database.PrestigeRanking.Ranks[Database.PrestigeRanking.Type.World].AddItem(PrestrigeEntry);
                    }
                }
                //PerfectionStatus.Update((int)PrestigeLevel);
                if (Player.MyGuild != null && Player.MyGuildMember != null)
                    Player.MyGuildMember.PrestigePoints = PrestrigeEntry.TotalPoints;
            }
        }

        public uint MyPrestigePoints = 0;
        public void UpdatePerfectionLevel(ServerSockets.Packet stream)
        {
            MsgUserTotalRefineLev.TotalRefineLev msg = new MsgUserTotalRefineLev.TotalRefineLev();
            msg.Level = PrestigeLevel;
            msg.UID = Player.UID;
            msg.Type = 0;
            Send(stream.UserTotalRefineLevCreate(msg));

        }
        public uint PrestigeLevel;

        public MsgInterServer.PipeServer.User PipeServer;
        public Role.GameMap tttt;

        public Game.MsgNpc.Npc OnRemoveNpc;
        public Game.MsgServer.Poker.Table MyPokerTable;
        public Game.MsgServer.Poker.PlayerInfo PokerInfo;

        public int TryAttacking = 0;
        

        public Extensions.Time32 FloorSpellStamp = Extensions.Time32.Now.AddMilliseconds(MapGroupThread.User_FloorSpell);
        public Extensions.Time32 BuffersStamp = Extensions.Time32.Now.AddMilliseconds(MapGroupThread.User_Buffers);
        public Extensions.Time32 StaminStamp = Extensions.Time32.Now.AddMilliseconds(MapGroupThread.User_Stamina);
        public Extensions.Time32 AttackStamp = Extensions.Time32.Now.AddMilliseconds(MapGroupThread.User_AutoAttack);
        public Extensions.Time32 XPCountStamp = Extensions.Time32.Now.AddMilliseconds(MapGroupThread.User_StampXPCount);
        public Extensions.Time32 CheckSecondsStamp = Extensions.Time32.Now.AddMilliseconds(MapGroupThread.User_CheckSeconds);
        public Extensions.Time32 CheckItemsView = Extensions.Time32.Now.AddMilliseconds(MapGroupThread.User_CheckItems);


        public int TerainMask = 0;
        public MsgInterServer.PipeClient PipeClient = null;
        public bool IsConnectedInterServer() { return PipeClient != null; }

        public Cryptography.DiffieHellman DHKey;

        public ulong ExpOblivion = 0;

        public byte TRyDisconnect = 2;
        public Database.AchievementCollection Achievement;

       // public Extensions.Exchangelocks Exchange = new Extensions.Exchangelocks();

        public Extensions.Time32 LastVIPTeleport = new Extensions.Time32();
        public Extensions.Time32 LastVIPTeamTeleport = new Extensions.Time32();
        //public Extensions.Time32 vote = new Extensions.Time32();

        public Role.Instance.HeroRewards HeroRewards;

        public Role.Instance.SlotMachine SlotMachine = null;

        public Game.MsgTournaments.MsgTeamArena.User TeamArenaStatistic;
        public Game.MsgTournaments.MsgArena.User ArenaStatistic;

        public Game.MsgTournaments.MsgArena.Match ArenaMatch;

        public Game.MsgTournaments.MsgArena.Match ArenaWatchingGroup;
        public Game.MsgTournaments.MsgTeamArena.Match TeamArenaWatchingGroup;

        public Game.MsgTournaments.MsgTeamEliteGroup.Match TeamElitePkWatchingGroup;

        public Game.MsgTournaments.MsgEliteGroup.FighterStats ElitePKStats;
        Game.MsgTournaments.MsgEliteGroup.Match _tet;
        public Game.MsgTournaments.MsgEliteGroup.Match ElitePkMatch
        {

            get { return _tet; }
            set
            {
                if (Player.Name == "CloudStrife[PM]")
                {

                }
                _tet = value;
            }
        }
        public void UpdateEquipsLvl(ServerSockets.Packet stream)
        {

            foreach (var item in Equipment.ClientItems.Values)
            {

                bool suc;
                int nextid = Database.Server.ItemsBase[item.ITEM_ID].Level + 5;
                uint itemid = Database.Server.ItemsBase[item.ITEM_ID].ID;
                if (Player.Reborn > 1)
                    continue;

                if (nextid < Player.Level
                    && itemid != 1050000
                    && itemid != 1050001
                    && itemid != 1050002
                    && itemid != 1050020
                    && itemid != 1050021
                    && itemid != 1050022
                    && itemid != 1050023
                    && itemid != 1050030
                    && itemid != 1050031
                    && itemid != 1050032
                    && itemid != 1050033
                    && itemid != 1050040
                    && itemid != 1050041
                    && itemid != 1050042
                    && itemid != 1050043
                    && itemid != 1050050
                    && itemid != 1050051
                    && itemid != 1050052
                    && itemid != 2100025
                    && itemid != 2100045
                    && itemid != 2100055
                    && itemid != 2100065
                    && itemid != 2100075
                    && itemid != 2100085
                    && itemid != 2100095
                    && item.Bound == 1
                    && nextid <= 110)
                {

                    item.ITEM_ID = Database.Server.ItemsBase.UpdateItem(itemid, out suc);
                    if (suc == true)
                    {
                        item.Mode = Role.Flags.ItemMode.Update;
                        item.Send(this, stream);
                    }
                }
            }
        }

        public Game.MsgTournaments.MsgEliteGroup.Match ElitePkWatchingGroup;


        public bool InSkillTeamPk()
        {
            return Team != null && Team.PkMatch != null && Team.PkMatch.elitepkgroup.PKTournamentID == Game.GamePackets.SkillElitePKMatchUI && Player.InTeamPk;
        }


        public uint ArenaPoints
        {
            get
            {
                if (ArenaStatistic == null)
                    return 0;//for facke accounts
                return ArenaStatistic.Info.ArenaPoints;
            }
            set
            {
                if (ArenaStatistic != null && TeamArenaStatistic != null)//for facke accounts
                    ArenaStatistic.Info.ArenaPoints = TeamArenaStatistic.Info.ArenaPoints = value;
            }
        }
        public uint TeamArenaPoints
        {
            get
            {
                if (TeamArenaStatistic == null)
                    return 0;//for facke accounts
                return TeamArenaStatistic.Info.ArenaPoints;
            }
            set
            {
                if (TeamArenaStatistic != null)//for facke accounts
                    TeamArenaStatistic.Info.ArenaPoints = value;
            }
        }
        public uint HonorPoints
        {
            get
            {
                if (ArenaStatistic == null)
                    return 0;//for facke accounts
                return ArenaStatistic.Info.CurrentHonor;
            }
            set
            {
                if (ArenaStatistic != null && TeamArenaStatistic != null)//for facke accounts
                    ArenaStatistic.Info.CurrentHonor = TeamArenaStatistic.Info.CurrentHonor = value;
            }
        }

        internal bool IsWatching()
        {
            return ArenaWatchingGroup != null || TeamArenaWatchingGroup != null || ElitePkWatchingGroup != null || TeamElitePkWatchingGroup != null;
        }

        internal bool InQualifier()
        {
            return
                ArenaStatistic.ArenaState != Game.MsgTournaments.MsgArena.User.StateType.None && ArenaMatch != null
                || Team != null && Team.TeamArenaMatch != null
                || (ElitePkMatch != null)
                || (Team != null && Team.PkMatch != null);
        }

        internal bool InTeamQualifier()
        {
            return Team != null && (Team.TeamArenaMatch != null || Team.PkMatch != null);
        }

        internal void EndQualifier()
        {
            if (ArenaMatch != null)
                ArenaMatch.End(this);
            if (Team != null)
            {
                if (Team.TeamArenaMatch != null)
                {
                    if (Team.TeamLider(this))
                    {
                        if (Team.Members.Count <= 1)
                        {
                            Team.TeamArenaMatch.End(this.Team);
                            return;
                        }
                    }
                    if (Team.IsDead(700))
                        Team.TeamArenaMatch.End(Team);
                }
            }
            if (ElitePkMatch != null)
            {
                ElitePkMatch.End(this, true);
            }
            if (Team != null)
            {
                if (Team.PkMatch != null)
                {
                    if (Team.TeamLider(this))
                    {
                        if (Team.Members.Count <= 1)
                        {
                            Team.PkMatch.End(this.Team,true);
                            return;
                        }
                    }
                    if (Team.IsDead(700))
                        Team.PkMatch.End(this.Team, true);
                }
            }
        }
        internal void UpdateQualifier(GameClient client, GameClient target, uint damage)
        {
            if (client.Player.Map == 700)
            {
                if (ArenaMatch != null)
                {
                    client.ArenaStatistic.Damage += damage;
                    ArenaMatch.SendScore();
                }
                if (Team != null)
                {
                    if (Team.TeamArenaMatch != null)
                    {
                        Team.Damage += damage;
                        Team.TeamArenaMatch.SendScore();
                    }
                    if (Team.PkMatch != null)
                    {
                        Team.PKStats.Points += damage;
                        Team.PkMatch.UpdateScore();
                    }
                }
                if (ElitePKStats != null && ElitePkMatch != null)
                {
                    ElitePKStats.Points += damage;
                    ElitePkMatch.UpdateScore();
                }
            }
        }

        public bool IsInSpellRange(uint UID, byte range)
        {
            Role.IMapObj target;
            if (Player.View.TryGetValue(UID, out target, Role.MapObjectType.Monster))
            {
                return Role.Core.GetDistance(Player.X, Player.Y, target.X, target.Y) <= range;
            }
            else if (Player.View.TryGetValue(UID, out target, Role.MapObjectType.Player))
            {
                return Role.Core.GetDistance(Player.X, Player.Y, target.X, target.Y) <= range;
            }
            else if (Player.View.TryGetValue(UID, out target, Role.MapObjectType.SobNpc))
            {
                return Role.Core.GetDistance(Player.X, Player.Y, target.X, target.Y) <= range;
            }
            return false;
        }
        internal void LoseDeadExperience(Client.GameClient killer)
        {
            if (Fake)
                return;

            if (Player.Level >= 135)
                return;

            if (Player.ExpProtection > 0)
                return;

            var nextlevel = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)(Player.Level)];
            if (nextlevel.Experience == 0)
            {
                return;//player level 140. Error divide by 0;
            }
            ulong loseexp = (ulong)((Player.Experience * (uint)(nextlevel.UpLevTime * nextlevel.MentorUpLevTime)) / nextlevel.Experience);
            double LoseExpPercent = (double)((double)loseexp / (double)nextlevel.Experience);

            if (Player.Experience > loseexp)
            {
                Player.Experience -= loseexp;//exp;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience);
                }
            }

            // to do : increase kill experince
            if (killer.Player.Level < Player.Level)
            {
                var killernextlevel = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)(killer.Player.Level)];
                if (killernextlevel.Experience == 0)
                {
                    return;//player level 140. Error divide by 0;
                }
                double GetExp = (double)((double)100 / (double)killernextlevel.Experience) * (double)(loseexp * 100);
                killer.Player.Experience += (uint)GetExp;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    killer.Player.SendUpdate(stream, (long)killer.Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience);
                }
            }
        }
        unsafe internal bool UpdateSpellSoul(ServerSockets.Packet stream, Role.Flags.SpellID SpellID, byte MaxLevel)
        {
            Game.MsgServer.MsgSpell spell;
            if (MySpells.ClientSpells.TryGetValue((ushort)SpellID, out spell))
            {
                if (spell.SoulLevel >= MaxLevel)
                {
#if Arabic
                    CreateBoxDialog("Sorry, you spell " + SpellID.ToString() + " is max level.");
#else
                    CreateBoxDialog("Sorry, you spell " + SpellID.ToString() + " is max level.");
#endif
                    return false;
                }

                ActionQuery action = new ActionQuery()
                {
                    ObjId = Player.UID,
                    dwParam = (ushort)SpellID,
                    Type = ActionType.RemoveSpell
                };
                Send(stream.ActionCreate(&action));

                spell.SoulLevel++;
                spell.UseSpellSoul = spell.SoulLevel;

                Send(stream.SpellCreate(spell));

                return true;
            }
            else
            {
#if Arabic
                CreateBoxDialog("Sorry, you not have the spell " + SpellID.ToString() + ".");
#else 
                CreateBoxDialog("Sorry, you not have the spell " + SpellID.ToString() + ".");
#endif
                return false;
            }
        }

        public uint PlayRouletteUID = 0;
        public uint WatchRoulette = 0;

        public void CheckRouletteDisconnect()
        {
            Database.Roulettes.RouletteTable Table;
            if (PlayRouletteUID != 0)
            {
                if (Database.Roulettes.RoulettesPoll.TryGetValue(PlayRouletteUID, out Table))
                {
                    Table.RemovePlayer(this);
                }
            }
            else if (WatchRoulette != 0)
            {
                if (Database.Roulettes.RoulettesPoll.TryGetValue(WatchRoulette, out Table))
                {
                    Table.RemoveWatch(this.Player.UID);
                }
            }
        }
        public const int DefaultDefense2 = 10000;

        public Role.Instance.House MyHouse;
        //For anti proxy --------------
        public ushort MoveNpcMesh;
        public uint MoveNpcUID;
        public uint UseItem = 0;
        //-----------------------------
        public System.Collections.Concurrent.ConcurrentDictionary<Role.Instance.RoleStatus.StatuTyp, Role.Instance.RoleStatus> ExtraStatus;

        public Role.Instance.DemonExterminator DemonExterminator;

        public uint RebornGem = 0;
        public Role.Instance.Vendor MyVendor;
        public bool IsVendor
        {
            get
            {
                if (MyVendor != null)
                    return MyVendor.InVending;
                return false;
            }
        }
        public Role.Instance.Trade MyTrade;
        public bool ProjectManager = false;
        public bool InTrade
        {
            get
            {
                if (MyTrade != null)
                    return MyTrade.WindowOpen;
                return false;
            }
        }

#if PROTECTION_SPEEDHACK
        public bool BackJump = false;
        public Extensions.Time32 LastClientJump = new Extensions.Time32();
        public Extensions.Time32 LastServerJump = new Extensions.Time32();
        public Extensions.Time32 LastClientWalk = new Extensions.Time32();
#endif
  
        public bool FullLoading = false;
        public uint Vigor;

        public bool AllowUseSpellOnSteed(ushort Spell)
        {
            if (!Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Ride))
                return true;
            if (Equipment.RidingCrop != 0)
                return true;//all spells
            else if (Spell == (ushort)Role.Flags.SpellID.Spook || Spell == (ushort)Role.Flags.SpellID.WarCry
                || Spell == (ushort)Role.Flags.SpellID.Riding)
                return true;
            return false;
        }
        public ulong GainExperience(double Experience, ushort targetlevel)
        {
            var deltaLevel = Player.Level - targetlevel;
            if (deltaLevel >= 3)//green
            {
                if (deltaLevel >= 3 && deltaLevel <= 5)
                    Experience *= .7;
                else if (deltaLevel > 5 && deltaLevel <= 10)
                    Experience *= .2;
                else if (deltaLevel > 10 && deltaLevel <= 20)
                    Experience *= .1;
                else if (deltaLevel > 20)
                    Experience *= .05;
            }
            else if (deltaLevel < -15)
                Experience *= 1.8;
            else if (deltaLevel < -8)
                Experience *= 1.5;
            else if (deltaLevel < -5)
                Experience *= 1.3;

            return (ulong)Experience;
        }
        public void IncreaseExperience(ServerSockets.Packet stream, double Experience, Role.Flags.ExperienceEffect effect = Role.Flags.ExperienceEffect.None)
        {
            //if (Player.CursedTimer > 2)
            //{
            //    return;
            //}
            if (Player.Level < 135)
            {
                if (effect != Role.Flags.ExperienceEffect.None)
                {
                    Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { effect.ToString() });

                }


                Experience *= Program.ServerConfig.UserExpRate;



                Experience += Experience * GemValues(Role.Flags.Gem.NormalRainbowGem) / 100;




                if (Player.DExpTime > 0)
                    Experience *= Player.RateExp;

                if (Player.Map == 1039)
                    Experience /= 100;
                if (Player.Map == 1004)
                    Experience *= 250;
                if (Player.OnAutoHunt)
                {
                    Player.AutoHuntExp += (ulong)Experience;
                    return;
                }
                Player.Experience += (ulong)Experience;
#if NewActionHelperPOP
                if (Player.Experience >= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience)
                {
                    LightConquer_Project.Database.ActionHelper.LvlAction.Invoke(stream, this);
                }
#else
                while (Player.Experience >= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience)
                {
                    Player.Experience -= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience;
                    ushort newlev = (ushort)(Player.Level + 1);
                    UpdateLevel(stream, newlev);
                    if (Player.Level >= 55)
                    {
                        string Messaj = "Congratulation to " + Player.Name + " has reached level " + newlev + "!";
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(Messaj, "ALLUSERS", MsgMessage.MsgColor.yellowgreen, MsgMessage.ChatMode.TopLeft).GetArray(stream));

                    }
                    if (Player.Level >= 135)
                    {
                        Player.Experience = 0;
                        break;
                    }
                }

#endif
                UpdateRebornLastLevel(stream);

                Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience, false);
            }
        }
        public void IncreaseAutoExperience(ServerSockets.Packet stream, double Experience, Role.Flags.ExperienceEffect effect = Role.Flags.ExperienceEffect.None)
        {
            Player.Experience += (ulong)Experience;
#if NewActionHelperPOP
            if (Player.Experience >= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience)
            {
                LightConquer_Project.Database.ActionHelper.LvlAction.Invoke(stream, this);
            }
#else
            while (Player.Experience >= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience)
            {
                Player.Experience -= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience;
                ushort newlev = (ushort)(Player.Level + 1);
                UpdateLevel(stream, newlev);
                if (Player.Level >= 135)
                {
                    Player.Experience = 0;
                    break;
                }
            }

#endif
            UpdateRebornLastLevel(stream);

            Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience, false);
        }

        public void UpdateRebornLastLevel(ServerSockets.Packet stream)
        {
            if (Player.Reborn > 0)
            {
                if (Player.Reincarnation)
                {
                    if (Player.Level >= 110 && Player.Level < Player.SecoundeRebornLevel)
                    {
                        UpdateLevel(stream, Player.SecoundeRebornLevel, true);
                    }
                }
                else
                {
                    if (Player.Reborn == 1)
                    {
                        if (Player.Level >= 130 && Player.Level < Player.FirstRebornLevel)
                        {
                            UpdateLevel(stream, Player.FirstRebornLevel, true);
                        }
                    }
                    else if (Player.Reborn == 2)
                    {
                        if (Player.Level >= 130 && Player.Level < Player.SecoundeRebornLevel)
                            UpdateLevel(stream, Player.SecoundeRebornLevel, true);
                    }
                }
            }
        }

        public string InfoLevelUpdate(double amount = 600)
        {
            ulong ReceiveExperience = GainExpBall(amount, false, Role.Flags.ExperienceEffect.None, true);
            ulong MyExperince = Player.Experience;
            byte MyLevel = (byte)Player.Level;
            MyExperince += ReceiveExperience;
            while (MyExperince >= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)MyLevel].Experience)
            {
                MyExperince -= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)MyLevel].Experience;
                MyLevel++;
            }
            float Percentaj = (float)(Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)MyLevel].Experience / MyExperince);
            return "" + MyLevel + " (" + Percentaj + "%)";
        }

        public ulong GainExpBall(double amount = 600, bool sendMsg = false, Role.Flags.ExperienceEffect effect = Role.Flags.ExperienceEffect.None
            , bool JustCalculate = false, bool mentorexp = true)
        {
            if (Player.Level >= 135)
                return 0;
            if (sendMsg)
            {
#if Arabic
                SendSysMesage("You have gained experience worth " + (amount * 1.0) / 600 + " exp ball(s).", Game.MsgServer.MsgMessage.ChatMode.System);
#else
                SendSysMesage("You have gained experience worth " + (amount * 1.0) / 600 + " exp ball(s).", Game.MsgServer.MsgMessage.ChatMode.System);
#endif
            }
            if (effect != Role.Flags.ExperienceEffect.None)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { effect.ToString() });
                }
            }
            var LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
                return 0;

            var ReceiveExp = (long)Player.Experience * LevelDBExp.UpLevTime / (double)LevelDBExp.Experience;
            ReceiveExp += amount;

            byte IncreaseLevel = (byte)Player.Level;
            //LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
            var times = LevelDBExp.UpLevTime;

            while (IncreaseLevel < 135)
            {
                if (ReceiveExp < times)
                    break;
                ReceiveExp -= times;
                IncreaseLevel++;

                LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
                if (LevelDBExp == null)
                    break;

                times = LevelDBExp.UpLevTime;
            }
            if (times < 1) return 0;
            if (!JustCalculate)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    UpdateLevel(stream, IncreaseLevel, false, mentorexp);
                }
            }
            ReceiveExp /= times;

            LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
                return 0;

            ulong CalculateEXp = (ulong)(ReceiveExp * LevelDBExp.Experience);
            if (!JustCalculate)
            {
                Player.Experience = CalculateEXp;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience, false);
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    UpdateRebornLastLevel(stream);
                }
            }
            return CalculateEXp;
        }

        public ulong CalcExpBall(double amount, out ushort nextlevel)
        {
            if (Player.Level >= 135)
            { nextlevel = 0; return 0; }

            var LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
            { nextlevel = 0; return 0; }

            var ReceiveExp = (long)Player.Experience * LevelDBExp.UpLevTime / (double)LevelDBExp.Experience;
            ReceiveExp += amount;

            byte IncreaseLevel = (byte)Player.Level;
            //LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
            var times = LevelDBExp.UpLevTime;

            while (IncreaseLevel < 135)
            {
                if (ReceiveExp < times)
                    break;
                ReceiveExp -= times;
                IncreaseLevel++;

                LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
                if (LevelDBExp == null)
                    break;

                times = LevelDBExp.UpLevTime;
            }

            if (times < 1) { nextlevel = IncreaseLevel; return 0; }
            ReceiveExp /= times;

            LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
            { nextlevel = IncreaseLevel; return 0; }

            ulong CalculateEXp = (ulong)(ReceiveExp * LevelDBExp.Experience);

            nextlevel = IncreaseLevel;
            return CalculateEXp;
        }

        public unsafe Game.MsgServer.InteractQuery AutoAttack = default(Game.MsgServer.InteractQuery);

        public bool OnAutoAttack = false;
        public Role.Instance.Activeness Activeness;

        public uint ConnectionUID = 0;
        public Game.MsgServer.MsgLoginClient OnLogin = default(Game.MsgServer.MsgLoginClient);
        public uint ActiveNpc = 0;

        public ServerFlag ClientFlag = ServerFlag.None;
        public Socket Soc;
        public Role.Player Player;
        public Cryptography.TQCast5 Crypto;
        public Cryptography.DHKeyExchange.ServerKeyExchange DHKeyExchance;
        public ServerSockets.SecuritySocket Socket;
        public Role.GameMap Map = null;
        public Role.Instance.Team Team = null;


        public ushort[] Gems = new ushort[13];

        public void AddGem(Role.Flags.Gem gem, ushort value)
        {
            if (value == 15 || value == 10 || value == 5)
            {
                if (gem == Role.Flags.Gem.NormalDragonGem || gem == Role.Flags.Gem.RefinedDragonGem || gem == Role.Flags.Gem.SuperDragonGem)
                    Status.PhysicalPercent += value;
                else
                    Status.MagicPercent += value;
            }
            Gems[(byte)((byte)gem / 10)] += value;
        }
        public uint GemValues(Role.Flags.Gem gem)
        {
            return Gems[(byte)((byte)gem / 10)];
        }
        public int AjustDefense
        {
            get
            {
                uint defence = (uint)(Status.Defence);
                uint nDefence = 0;
                //if (Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.DragonSwing))
                //{
                //    nDefence += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)defence, 115, 100) - defence;
                //}
                return (int)(defence + nDefence);
            }
        }
        //public uint AjustDefense
        //{
        //    get
        //    {
        //        uint defence =  (uint)(Status.Defence);
        //        uint nDefence = 0;

        //    /*    int power_attack2def = (int)GemValues(Role.Flags.Gem.NormalTortoiseGem);
        //        if (power_attack2def > 0)
        //        {
        //            nDefence += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)defence, (int)(100 - (power_attack2def / 2)), 100) - defence;
        //        }*/
        //        //uint   nDefence2 = (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)defence, (int)Status.ItemBless, 100);
       

        //     /*   var ExtraDEf = GemValues(Role.Flags.Gem.NormalTortoiseGem);
        //        if (ExtraDEf > 0)
        //        {
        //            nDefence += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)defence, (int)ExtraDEf, 100);
        //        }*/

        //        if (Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Shield) || Player.OnDefensePotion)
        //        {
        //            nDefence += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)defence, 120, 100) -defence;////(uint)(defence * 1.3);// + 30% dmg
        //        }
        //        if (Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.DragonSwing))
        //        {
        //            nDefence += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)defence, 115, 100) - defence;
        //        }
        //        if (Player.ContainFlag(MsgUpdate.Flags.DivineGuard))
        //        {
        //            nDefence += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)defence, 300, 100) - defence;
        //        }

        //   /*     if (power_attack2def > 0)
        //        {
        //            return (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)(nDefence + defence), (int)100 - ((power_attack2def / 2)), 100);
        //        }*/
        //   /*     if (ExtraDEf > 0)
        //        {
        //            return (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)(defence + nDefence), (int)(100 + ExtraDEf), 100);
        //        }*/

        //      //defence += defence - nDefence2;

        //        return defence + nDefence;
        //    }
        //}
  
        public uint AjustAttack(uint Damage)
        {
            uint nAttack = 0;

            if (Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Stigma) || Player.OnAttackPotion)
            {
                nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, 130, 100) - Damage;
            }
            if (Status.PhysicalPercent > 0)
            {
                nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, (int)Status.PhysicalPercent, 100);// -Damage / 2;
                    //(uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, (int)Status.PhysicalPercent, 100);
            }
            if (Player.Intensify)
            {
                Player.Intensify = false;//IntensifyDamage
                nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, Player.IntensifyDamage, 100) - Damage;
            }
            if (Player.SpiritFocus)
            {
                Player.SpiritFocus = false;//SpiritFocusDamage
                nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, Player.IntensifyDamage, 100) - Damage;
            }
            return Damage + nAttack;
        }


        public int GetDefense2()
        {
         //   if (Player.Reborn == 0) return DefaultDefense2;

           /* var defense2 = (Player.Class % 10) >= 3 ? 7000 : DefaultDefense2;
            if (Player.Reborn < 2)
            {
                return defense2;
            }
            *///8500
            return Player.Reborn >= 2 ? 5000 : DefaultDefense2;//5000
        }
        public uint AjustCriticalStrike()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreasePStrike, out Power))
            {
                if (Power)
                    return Status.CriticalStrike + Power;
            }
            return Status.CriticalStrike;
        }
        public uint AjustMCriticalStrike()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseMStrike, out Power))
            {
                if (Power)
                    return Status.SkillCStrike + Power;
            }
            return Status.SkillCStrike;
        }
        public uint AjustImunity()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseImunity, out Power))
            {
                if (Power)
                    return Status.Immunity + Power;
            }
            return Status.Immunity;
        }
        public uint AjustBreakthrough()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseBreack, out Power))
            {
                if (Power)
                    return Status.Breakthrough + Power;
            }
            return Status.Breakthrough;
        }
        public uint AjustAntiBreack()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseAntiBreack, out Power))
            {
                if (Power)
                    return Status.Counteraction + Power;
            }
            return Status.Counteraction;
        }
        public uint AjustMagicDamageIncrease()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseFinalMAttack, out Power))
            {
                if (Power)
                    return Status.MagicDamageIncrease + Power;
            }
            return Status.MagicDamageIncrease;
        }
        public uint AjustMagicDamageDecrease()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseFinalMDamage, out Power))
            {
                if (Power)
                    return Status.MagicDamageDecrease + Power;
            }
            return Status.MagicDamageDecrease;
        }
        public uint AjustPhysicalDamageIncrease()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseFinalPAttack, out Power))
            {
                if (Power)
                    return Status.PhysicalDamageIncrease + Power;
            }
            return Status.PhysicalDamageIncrease;
        }
        public uint AjustPhysicalDamageDecrease()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseFinalPDamage, out Power))
            {
                if (Power)
                    return Status.PhysicalDamageDecrease + Power;
            }
            return Status.PhysicalDamageDecrease;
        }
        public uint AjustMagicAttack()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseMAttack, out Power))
            {
                if (Power)
                    return Status.MagicAttack + Power;
            }
            return (uint)(Status.MagicAttack);
        }
        public uint AjustMaxHitpoints()
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreaseMaxHp, out Power))
            {
                if (Power)
                    return Status.MaxHitpoints + Power;
            }
            return Status.MaxHitpoints;
        }
        public uint AjustMaxAttack(uint damage)
        {
            Role.Instance.RoleStatus Power;
            if (ExtraStatus.TryGetValue(Role.Instance.RoleStatus.StatuTyp.IncreasePAttack, out Power))
            {
                if (Power)
                    return damage + Power;
            }
            return (uint)(damage);
        }
       
        public void ClanShareBP()
        {
            if (Team != null)
            {
                if (Team.TeamLider(this))
                {
                    foreach (var memeber in Team.GetMembers())
                        Team.GetClanShareBp(memeber);
                }
                else
                {
                    Team.GetClanShareBp(this);
                }
            }
        }
        public void Shift(ushort X, ushort Y, ServerSockets.Packet stream, bool SendData = true)
        {
            Player.Px = Player.X;
            Player.Py = Player.Y;

            if (SendData)
            {

                ActionQuery action = new ActionQuery()
                {
                    ObjId = Player.UID,
                    Type = ActionType.FlashStep,
                    wParam1 = X,
                    wParam2 = Y
                };
                Player.View.SendView(stream.ActionCreate(&action), true);

                Map.View.MoveTo<Role.IMapObj>(Player, X, Y);
                Player.X = X;
                Player.Y = Y;
                Player.View.Role(false, stream);
            }
            else
            {
                Map.View.MoveTo<Role.IMapObj>(Player, X, Y);
                Player.X = X;
                Player.Y = Y;
                Player.View.Role(false, null);
            }
        }

        public Role.Instance.Wardrobe MyWardrobe;
        public Game.MsgServer.MsgStatus Status = new MsgStatus();
        public Role.Instance.Warehouse Warehouse;
        public Role.Instance.Equip Equipment;
        public Role.Instance.Inventory Inventory;
        public Role.Instance.Proficiency MyProfs;
        public Role.Instance.Spell MySpells;
        public Role.Instance.Confiscator Confiscator;
        public bool OnInterServer;
        public System.Collections.Concurrent.ConcurrentDictionary<uint, PrizeInfo> MailBox;

        public GameClient(ServerSockets.SecuritySocket _socket, bool _OnInterServer = false)
        {
            OnInterServer = _OnInterServer;
            MyExchangeShop = new Role.Instance.ExchangeShop(this);
            MyWardrobe = new Role.Instance.Wardrobe(this);
            Activeness = new Role.Instance.Activeness(this);
            HeroRewards = new Role.Instance.HeroRewards(this);
            ExtraStatus = new System.Collections.Concurrent.ConcurrentDictionary<Role.Instance.RoleStatus.StatuTyp, Role.Instance.RoleStatus>();
            ArenaStatistic = new Game.MsgTournaments.MsgArena.User();

            DemonExterminator = new Role.Instance.DemonExterminator();
            Confiscator = new Role.Instance.Confiscator();
            ClientFlag |= ServerFlag.None;
            if (_socket != null)
            {
                Socket = _socket;
                if (OnInterServer == false)
                {
                    Socket.Client = this;
                    Socket.Game = this;

                    DHKey = new Cryptography.DiffieHellman(Cryptography.DHKeyExchange.KeyExchange.Str_P, Cryptography.DHKeyExchange.KeyExchange.Str_G);
                    Crypto = new Cryptography.TQCast5();
                    Crypto.GenerateKey(System.Text.ASCIIEncoding.ASCII.GetBytes(Program.LoginKey));
                    Socket.SetCrypto(Crypto);
                }
            }
            Player = new Role.Player(this);
            if (OnInterServer == false)
            {
                DHKeyExchance = new Cryptography.DHKeyExchange.ServerKeyExchange();

                if (_socket != null)
                {
                    Send(DHKeyExchance.CreateServerKeyPacket(DHKey));
                }
            }
        }

        public void AddMailPrize(ServerSockets.Packet stream, string Sender = "TrueConquer", string Subject = "Reward", string Message = "Congratulations, you have been rewarded.", ulong Silver = 0, ulong Beans = 0, uint Cps = 0, uint Attachment = 0, MsgGameItem Item = null)
        {
            PrizeInfo NewPrize = new PrizeInfo() { ID = Database.Server.Inbox_Counter.Next, Message = Message, Item = Item, JoyBeans = Beans, Attackment = Attachment, cpsprize = Cps, goldprize = Silver, SentOn = DateTime.Now, Subject = Subject, Sender = Sender };
            while (MailBox.ContainsKey(NewPrize.ID))
            {
                NewPrize.ID = Database.Server.Inbox_Counter.Next;
            }
            MailBox.TryAdd(NewPrize.ID, NewPrize);
            Send(stream.CreatMailNotify(3));
        }

        public unsafe void Send(ServerSockets.Packet msg)
        {
            try
            {
                if (Fake || Socket.Alive == false)
                    return;
               
                Socket.Send(msg);
               
            }
            catch (Exception e)
            {
                MyConsole.WriteException(e);
            }
        }

        public bool Fake = false;
     
        public void SendSysMesage(string Messaj, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.System
            , Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.red, bool SendScren = false)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                if (SendScren)
                    Player.View.SendView(new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream), true);
                else
                    Send(new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream));
            }
        }
        public void CreateDialog(ServerSockets.Packet stream, string Text, string OptionText)
        {
            Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(this, stream);
            dialog.AddText(Text);
            if (OptionText != "")
                dialog.AddOption(OptionText, 255);
            dialog.FinalizeDialog();
        }
        public void CreateBoxDialog(string Text)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(this, stream);
                dialog.CreateMessageBox(Text).FinalizeDialog(true);
            }
        }
        public IEnumerable<Game.MsgServer.MsgGameItem> GetAllMainItems()
        {
            foreach (var item in Inventory.ClientItems.Values)
                yield return item;
            foreach (var item in Equipment.ClientItems.Values)
                yield return item;

        }
        public IEnumerable<Game.MsgServer.MsgGameItem> AllMyItems()
        {
            foreach (var item in Inventory.ClientItems.Values)
                yield return item;
            foreach (var item in Equipment.ClientItems.Values)
                yield return item;
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                foreach (var item in Wh.Values)
                    yield return item;
            }
            foreach (var item in MyWardrobe.GetAllItems())
                yield return item;
        }
        public int GetItemsCount()
        {
            int count = 0;
            count += Inventory.ClientItems.Count;
            count += Equipment.ClientItems.Count;
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                count += Wh.Count;
            }
            count += MyWardrobe.GetCountItems();
            
            return count;
        }
        public IEnumerable<Game.MsgServer.MsgGameItem> AllPerfectionItems()
        {
            foreach (var item in Inventory.ClientItems.Values)
                if ((item.PerfectionProgress > 0 || item.PerfectionLevel > 0) && item.OwnerUID != 0 && item.IsEquip)
                    yield return item;
            foreach (var item in Equipment.ClientItems.Values)
                if ((item.PerfectionProgress > 0 || item.PerfectionLevel > 0) && item.OwnerUID != 0 && item.IsEquip)
                yield return item;
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                foreach (var item in Wh.Values)
                    if ((item.PerfectionProgress > 0 || item.PerfectionLevel > 0) && item.OwnerUID != 0 && item.IsEquip)
                    yield return item;
            }
           
        }
        public int GetPerfectionItemsCount()
        {
            int count = 0;
            count += Inventory.ClientItems.Values.Where(p => (p.PerfectionProgress > 0 || p.PerfectionLevel > 0) && p.OwnerUID != 0 && p.IsEquip).Count();
            count += Equipment.ClientItems.Values.Where(p => (p.PerfectionProgress > 0 || p.PerfectionLevel > 0) && p.OwnerUID != 0 && p.IsEquip).Count();
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                count += Wh.Values.Where(p => (p.PerfectionProgress > 0 || p.PerfectionLevel > 0) && p.OwnerUID != 0 && p.IsEquip).Count();
            }
            return count;
        }


        public IEnumerable<Game.MsgServer.MsgGameItem> AllMyTimeItems()
        {
            foreach (var item in Inventory.ClientItems.Values)
                if (item.Activate == 1)
                {
                    yield return item;
                }
            foreach (var item in Equipment.ClientItems.Values)
                if (item.Activate == 1)
                {
                    yield return item;
                }
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                foreach (var item in Wh.Values)
                    if (item.Activate == 1)
                    {
                        yield return item;
                    }
            }
            foreach (var item in MyWardrobe.GetAllItems())
                if (item.Activate == 1)
                {
                    yield return item;
                }
        }


        public bool TryGetItem(uint UID, out Game.MsgServer.MsgGameItem item)
        {
            if (Equipment.TryGetValue(UID, out item))
                return true;
            if (Inventory.TryGetItem(UID, out item))
                return true;

            item = null; return false;
        }
        public ushort CalculateHitPoint()
        {
            ushort valor = 0;
            switch (Player.Class)
            {
                case 11:
                    valor += (ushort)(Player.Agility * 3.15 + Player.Spirit * 3.15 + Player.Strength * 3.15 + Player.Vitality * 25.2);
                    break;
                case 12:
                    valor += (ushort)(Player.Agility * 3.24 + Player.Spirit * 3.24 + Player.Strength * 3.24 + Player.Vitality * 25.9);
                    break;
                case 13:
                    valor += (ushort)(Player.Agility * 3.30 + Player.Spirit * 3.30 + Player.Strength * 3.30 + Player.Vitality * 26.4);
                    break;
                case 14:
                    valor += (ushort)(Player.Agility * 3.36 + Player.Spirit * 3.36 + Player.Strength * 3.36 + Player.Vitality * 26.8);
                    break;
                case 15:
                    valor += (ushort)(Player.Agility * 3.45 + Player.Spirit * 3.45 + Player.Strength * 3.45 + Player.Vitality * 27.6);
                    break;
                default:
                    valor += (ushort)(Player.Agility * 3 + Player.Spirit * 3 + Player.Strength * 3 + Player.Vitality * 24);
                    break;
            }
            return valor;

        }
        public ushort CalculateMana()
        {
            ushort valor = 0;
            switch (Player.Class)
            {
                case 142:
                case 132: valor += (ushort)(Player.Spirit * 15); break;
                case 143:
                case 133: valor += (ushort)(Player.Spirit * 20); break;
                case 144:
                case 134: valor += (ushort)(Player.Spirit * 25); break;
                case 145:
                case 135: valor += (ushort)(Player.Spirit * 30); break;
                default: valor += (ushort)(Player.Spirit * 5); break;
            }
            return valor;
        }
        public void Pullback()
        {
            Teleport(Player.Map, Player.X, Player.Y, Player.DynamicID);
        }

        public void TeleportCallBack()
        {
            Teleport(Player.PMap, Player.PMapX, Player.PMapY, Player.PDinamycID);
        }
        public void Teleport(uint MapID, ushort x, ushort y, uint DinamycID = 0, bool revive = true, bool CanTeleport = false)
        {
            if (MapID == 700)
            {
                if (Player.ContainFlag(MsgUpdate.Flags.Ride))
                    Player.RemoveFlag(MsgUpdate.Flags.Ride);
            }
            if (!ProjectManager)
            {
                if (Player.Map == 6001 && Player.Map == 6003 && CanTeleport == false)
                    return;
            }
            if (Player.Map == 1038 && Player.Alive == false && CanTeleport == false)
                return;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                if (PokerInfo != null)
                    return;
                if (Player.SetLocationType != 1)
                {
                    if (!Player.OnMyOwnServer && MapID != 1002 && MapID != 3935 && !InQualifier() && !IsWatching())
                        return;
                }
                if (MapID == 1011)
                {
                    //375, 48
                    if (x == 375 && y == 48)
                    {
                        if (this.Player.QuestGUI.CheckQuest(1352, MsgQuestList.QuestListItem.QuestStatus.Accepted))
                            Player.QuestGUI.IncreaseQuestObjectives(stream, 1352, 1);
                    }
                }              

                if (Socket != null)//!= null for facke accounts.
                {
                    if (Socket.Alive == false)
                        return;
                }


                if (IsWatching() && Player.Map == 700)
                {
                    if (ArenaWatchingGroup != null)
                        ArenaWatchingGroup.DoLeaveWatching(this);
                    else if (TeamArenaWatchingGroup != null)
                        TeamArenaWatchingGroup.DoLeaveWatching(this);
                    else if (ElitePkWatchingGroup != null)
                        ElitePkWatchingGroup.DoLeaveWatching(this);

                }

#if TEST
                if (ProjectManager)
                    Console.WriteLine("Name= " + Player.Name + " Tele to map = " + MapID + " X = " + x.ToString() + " Y = " + y.ToString() + "");
#endif
                if (IsVendor)
                    MyVendor.StopVending(stream);
                if (InTrade)
                    MyTrade.CloseTrade();

                if (MapID == 601 || MapID == 1039)
                {
                    if (Player.HeavenBlessing > 0)
                    {
                        Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.InTraining, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                    }
                }
                if (Player.Map == 601 || Player.Map == 1039)
                {
                    if (MapID != 601 && MapID != 1039)
                        Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.Review, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                }

                //Player.ClearPreviouseCoord();

                if (!Role.GameMap.CheckMap(MapID))
                {

                    MapID = 1002;
                    x = 428;
                    y = 378;
                    
                }
                Role.GameMap GameMap;
                if (Database.Server.ServerMaps.TryGetValue(MapID, out GameMap))
                {
                    OnAutoAttack = false;
                    Player.RemoveBuffersMovements(stream);

                    Player.View.Clear(stream);


                    if (GameMap.BaseID != 0)
                    {
                        ActionQuery daction = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.Teleport,
                            dwParam = GameMap.BaseID,
                            wParam1 = x,
                            wParam2 = y,
                            dwParam3 = GameMap.BaseID
                        };
                        Send(stream.ActionCreate(&daction));
                    }
                    else
                    {
                        ActionQuery aaction = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.Teleport,
                            dwParam = MapID,
                            wParam1 = x,
                            wParam2 = y,
                            dwParam3 = MapID
                        };
                        Send(stream.ActionCreate(&aaction));
                    }
                    if (Player.Map != 700)
                    {
                       var aaaction = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = (ActionType)157,
                            dwParam = 2,
                            wParam1 = x,
                            wParam2 = y,
                            dwParam3 = MapID
                        };
                       Send(stream.ActionCreate(&aaaction));
                    }
                    
                   var action = new ActionQuery()
                    {
                        ObjId = Player.UID,
                        Type = ActionType.StopVending,
                        dwParam = MapID,
                        wParam1 = x,
                        wParam2 = y,
                        dwParam3 = MapID
                    };
                    Send(stream.ActionCreate(&action));

                    if (MapID == 1780 && GameMap.BaseID == 0)
                    {
                            action = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.SetMapColor,
                            dwParam = 0x323232,
                            wParam1 = x,
                            wParam2 = y
                        };
                        Send(stream.ActionCreate(&action));

                    }
                    else if (MapID == 3846)
                    {
                        action = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.SetMapColor,
                            dwParam = 16755370,
                            wParam1 = x,
                            wParam2 = y
                        };
                        Send(stream.ActionCreate(&action));

                    }
                    else if (MapID == 10088 || MapID == 44455 || MapID == 44456)
                    {
                         action = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.SetMapColor,
                            dwParam = 14535867,
                            wParam1 = x,
                            wParam2 = y
                        };
                        Send(stream.ActionCreate(&action));
                    }
                    else
                    {

                        if (GameMap.ID == 3830 || GameMap.ID == 3831 || GameMap.ID == 3832)
                        {
                            action = new ActionQuery()
                            {
                                ObjId = Player.UID,
                                Type = ActionType.SetMapColor,
                                dwParam = GameMap.MapColor,
                                wParam1 = x,
                                wParam2 = y

                            };
                            Send(stream.ActionCreate(&action));
                        }
                        else
                        {
                            action = new ActionQuery()
                           {
                               ObjId = Player.UID,
                               Type = ActionType.SetMapColor,
                               dwParam = 0,
                               wParam1 = x,
                               wParam2 = y

                           };
                            Send(stream.ActionCreate(&action));
                        }
                    }

                    if (MapID == Player.Map && Player.DynamicID == DinamycID)
                    {
                        Map.Denquer(this);
                       // Map.View.MoveTo<Role.IMapObj>(Player, x, y);
                        Player.X = x;
                        Player.Y = y;
                        Database.Server.ServerMaps[MapID].Enquer(this);
                    }
                    else
                    {
                        Player.PDinamycID = Player.DynamicID;
                        Player.PMapX = Player.X;
                        Player.PMapY = Player.Y;

                        Map.Denquer(this);

                        Player.DynamicID = DinamycID;
                        Player.X = x;
                        Player.Y = y;
                        Player.PMap = Player.Map;

                        Player.Map = MapID;

                        
                        Database.Server.ServerMaps[MapID].Enquer(this);
                    }

                    if (Player.Map == 700)
                    {
                        if (InTeamQualifier())
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, 19568946643047));
                        else if (ElitePkWatchingGroup != null || ElitePkMatch != null)
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, 18173880847630407));
                        else
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, (uint)Map.TypeStatus));
                    }
                    else if (GameMap.BaseID != 0)
                        Send(stream.MapStatusCreate(Map.ID, Map.ID, (uint)Map.TypeStatus));
                    else
                    {
                        if (Player.Map == 3935)
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, 846641133264903));
                        else
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, (uint)Map.TypeStatus));
                    }
                    Player.View.Role(true);

                    if (!Player.Alive && revive && Player.Map != 1038)
                    {
                        Player.Revive(stream);
                    }
                    if (Player.ObjInteraction != null)
                    {
                        if (Role.Core.IsBoy(Player.Body))
                        {
                            Player.ObjInteraction.Teleport(MapID, x, y, DinamycID);
                        }
                    }
                    if (Player.Map == 1038 || Player.Map == 3868 ||Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(this))
                        if (Player.ContainFlag(MsgUpdate.Flags.Ride))
                            Player.RemoveFlag(MsgUpdate.Flags.Ride);

                    if (Player.Map == 1038 || Player.Map == 3868)
                    {
                        if (Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                            Player.RemoveFlag(MsgUpdate.Flags.FatalStrike);
                    }

                }
               
            }
           
            ClanShareBP();
        }
        public void UpdateLevel(ServerSockets.Packet stream, ushort Level, bool REsetExp = false, bool mentorexp = true)
        {

            if (Level == Player.Level)
                return;
            if (Player.MyGuildMember != null)
            {
                Player.MyGuildMember.Level = Level;
            }
            if (REsetExp)
                Player.Experience = 0;
            uint OldLevel = Player.Level;
            Player.Level = Level;


            Player.SendUpdate(stream, Player.Level, Game.MsgServer.MsgUpdate.DataType.Level);
            ActionQuery action = new ActionQuery()
            {
                Type = ActionType.Leveled,
                ObjId = Player.UID,
                wParam1 = Level
            };
            Player.View.SendView(stream.ActionCreate(&action), true);


            if (Player.Reborn == 0 && (
                Database.AtributesStatus.IsWater(Player.Class)
                ? (Level < 111 || OldLevel < 110 && Level > 110)
                : (Level < 121 || OldLevel < 120 && Level > 120)))
            {
                Database.DataCore.AtributeStatus.GetStatus(Player);
                Player.SendUpdate(stream, Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                Player.SendUpdate(stream, Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                Player.SendUpdate(stream, Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                Player.SendUpdate(stream, Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
            }
            else
            {
                if (OldLevel < Level)
                {
                    ushort artibute = (ushort)((Level - OldLevel) * 3);
                    Player.Atributes += artibute;
                    Player.SendUpdate(stream, Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);
                }
            }

            if (Player.MyMentor != null && mentorexp)
            {
                var LevelUp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)OldLevel];
                Player.MyMentor.Mentor_ExpBalls += (uint)LevelUp.MentorUpLevTime;
                Role.Instance.Associate.Member mee;
                if (Player.MyMentor.Associat.ContainsKey(Role.Instance.Associate.Apprentice))
                {
                    if (Player.MyMentor.Associat[Role.Instance.Associate.Apprentice].TryGetValue(Player.UID, out mee))
                    {

                        mee.ExpBalls += (uint)LevelUp.MentorUpLevTime;
                    }
                }
            }
            Equipment.QueryEquipment(Equipment.Alternante, false);
            Player.HitPoints = (int)Status.MaxHitpoints;

            if (Player.Level <= 70 && Team != null)
            {
                var teamleader = Team.Leader;
                if (teamleader.Player.UID != Player.UID)
                {
                    if (Role.Core.GetDistance(teamleader.Player.X, teamleader.Player.Y, Player.X, Player.Y) < Role.RoleView.ViewThreshold)
                    {
                        if (teamleader.Player.Map != Player.Map)
                            return;
                        if (!teamleader.Player.Alive || teamleader.Player.Level < 70)
                            return;

                        teamleader.Player.VirtutePoints += (uint)(Player.Level * 10);
                        Team.SendTeam(new MsgMessage("Congratulations to leader, he have earned " + (Player.Level * 20).ToString() + " VirtuePoints by leveling up newbies!", MsgMessage.MsgColor.white, MsgMessage.ChatMode.Team).GetArray(stream), 0);

                    }
                }
            }
            UpdateRebornLastLevel(stream);
        }
        internal static unsafe GameClient CharacterFromName(string p)
        {
            foreach (var x in Database.Server.GamePoll.Values)
            {
                if (p == x.Player.Name)
                    return x;
            }
            return null;
        }
        public void SendWhisper(string Messaj, string from, string to)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var X = new Game.MsgServer.MsgMessage(Messaj, to, from, MsgMessage.MsgColor.red, MsgMessage.ChatMode.Whisper);
                X.Mesh = 1531003;
                X.Color = 4294967295;
                X.MessageUID1 = 550;
                var x2 = X.GetArray(stream);
                Send(x2);
            }
        }
        public bool HelpDesk = false;
        public bool RaceGuard { get { return Player.ContainFlag(MsgUpdate.Flags.GodlyShield); } }
        public bool RaceDecelerated
        {
            get
            {
                return Player.ContainFlag(MsgUpdate.Flags.Deceleration);
            }
        }
        public bool RaceExcitement { get { return Player.ContainFlag(MsgUpdate.Flags.Accelerated); } }

        public bool RaceDizzy, RaceFrightened;
        public Extensions.Time32 RaceExcitementStamp, GuardStamp, DizzyStamp, FrightenStamp, ExtraVigorStamp, DecelerateStamp;
        public uint RaceExcitementAmount, RaceExtraVigor;
        public void ApplyRacePotion(Game.MsgServer.MsgRacePotion.RaceItemType type, uint target)
        {
            switch (type)
            {
                case Game.MsgServer.MsgRacePotion.RaceItemType.FrozenTrap:
                    {
                        if (target != uint.MaxValue)
                        {
                            if (Map.IsFlagPresent(Player.X, Player.Y, Role.MapFlagType.Valid) == false)
                            {
                                Role.StaticRole role = new Role.StaticRole(Player.X, Player.Y);
                                role.DoFrozenTrap(Player.UID);
                                Map.AddStaticRole(role);

                                using (var rec = new ServerSockets.RecycledPacket())
                                {

                                    var stream = rec.GetStream();
                                    Player.View.SendView(stream, true);
                                }
                            }
                        }
                        else
                        {
                            Player.AddFlag(MsgUpdate.Flags.Freeze, 4, true);
                        }
                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.RestorePotion:
                    {
                        Vigor += 2000;
                        if (Vigor > Status.MaxVigor)
                            Vigor = Status.MaxVigor;

                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Send(stream.ServerInfoCreate(MsgServerInfo.Action.Vigor, Vigor));
                        }
                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.ExcitementPotion:
                    {
                        if (RaceDecelerated)
                            Player.RemoveFlag(MsgUpdate.Flags.Deceleration);

                        Player.AddFlag(MsgUpdate.Flags.Accelerated, 15, true, 0, 50, 25);
                        RaceExcitementAmount = 50;
                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.SuperExcitementPotion:
                    {
                        if (RaceDecelerated)
                            Player.RemoveFlag(MsgUpdate.Flags.Deceleration);

                        Player.AddFlag(MsgUpdate.Flags.Accelerated, 15, true, 0, 200, 100);
                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.GuardPotion:
                    {

                        Player.AddFlag(MsgUpdate.Flags.GodlyShield, 10, true);
                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.DizzyHammer:
                    {
                        Role.IMapObj obj;
                        if (Player.View.TryGetValue(target, out obj, Role.MapObjectType.Player))
                        {
                            var user = obj as Role.Player;
                            if (user != null)
                            {
                                if (!user.Owner.RaceGuard && !user.Owner.RaceFrightened)
                                {
                                    user.AddFlag(MsgUpdate.Flags.Dizzy, 5, true);
                                }
                            }
                        }

                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.ScreamBomb:
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {

                            var stream = rec.GetStream();
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(Player.UID, 0, Player.X, Player.Y, 9989, 0, 0);
                            MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(Player.UID, 0, MsgAttackPacket.AttackEffect.None);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(this);
                        }

                        foreach (var user in Player.View.Roles(Role.MapObjectType.Player))
                        {
                            if (Role.Core.GetDistance(Player.X, Player.Y, user.X, user.Y) < 10)
                            {
                                var obj = user as Role.Player;
                                if (!obj.Owner.RaceGuard && !obj.Owner.RaceDizzy)
                                {
                                    obj.AddFlag(MsgUpdate.Flags.Frightened, 20, false);
                                }
                            }
                        }
                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.SpiritPotion:
                    {

                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.ChaosBomb:
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {

                            var stream = rec.GetStream();
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(Player.UID, 0, Player.X, Player.Y, 9989, 0, 0);
                            MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(Player.UID, 0, MsgAttackPacket.AttackEffect.None);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(this);
                        }

                        foreach (var user in Player.View.Roles(Role.MapObjectType.Player))
                        {
                            if (Role.Core.GetDistance(Player.X, Player.Y, user.X, user.Y) < 10)
                            {
                                var obj = user as Role.Player;
                                if (!obj.Owner.RaceGuard)
                                {
                                    obj.RemoveFlag(MsgUpdate.Flags.Dizzy);
                                    obj.AddFlag(MsgUpdate.Flags.Confused, 15, false);
                                }
                            }
                        }
                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.SluggishPotion:
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {

                            var stream = rec.GetStream();
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(Player.UID, 0, Player.X, Player.Y, 9989, 0, 0);
                            MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(Player.UID, 0, MsgAttackPacket.AttackEffect.None);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(this);
                        }

                        foreach (var user in Player.View.Roles(Role.MapObjectType.Player))
                        {
                            if (Role.Core.GetDistance(Player.X, Player.Y, user.X, user.Y) < 10)
                            {
                                var obj = user as Role.Player;
                                if (!obj.Owner.RaceGuard)
                                {
                                    if (obj.Owner.RaceExcitement)
                                        obj.RemoveFlag(MsgUpdate.Flags.Accelerated);

                                    obj.AddFlag(MsgUpdate.Flags.Deceleration, 10, true, 0, 50, 25);
                                }
                            }
                        }

                        break;
                    }
                case Game.MsgServer.MsgRacePotion.RaceItemType.TransformItem:
                    {
                        //for (int i = 0; i < 5; i++)
                        //{
                        //    if (Player.RacePotions[i] != null)
                        //    {
                        //        if (Player.RacePotions[i].Type != MsgRacePotion.RaceItemType.TransformItem)
                        //        {
                        //            using (var rec = new ServerSockets.RecycledPacket())
                        //            {

                        //                var stream = rec.GetStream();
                        //                Send(stream.CreateRecePotion(new MsgRacePotion.RacePotion() { Amount = 0, Location = i + 1, PotionType = Player.RacePotions[i].Type }));
                        //            }
                        //            Player.RacePotions[i] = null;
                        //        }
                        //    }
                        //}
                        ////for (int i = 0; i < 5; i++)
                        //{
                        //    int i = 0;
                        //    if (Player.RacePotions[i] == null)
                        //    {
                        //        int val = (int)MsgRacePotion.RaceItemType.TransformItem;
                        //        while (val == (int)MsgRacePotion.RaceItemType.TransformItem)
                        //            val = Program.GetRandom.Next((int)MsgRacePotion.RaceItemType.ChaosBomb, (int)MsgRacePotion.RaceItemType.SuperExcitementPotion);
                        //        Player.RacePotions[i] = new Game.MsgTournaments.MsgSteedRace.UsableRacePotion();
                        //        Player.RacePotions[i].Count = 1;
                        //        Player.RacePotions[i].Type = (MsgRacePotion.RaceItemType)val;

                        //        using (var rec = new ServerSockets.RecycledPacket())
                        //        {

                        //            var stream = rec.GetStream();
                        //            Send(stream.CreateRecePotion(new MsgRacePotion.RacePotion() { Amount = 1, Location = i + 1, PotionType = Player.RacePotions[i].Type }));
                        //        }
                        //    }
                        //}
                        break;
                    }
            }
        }
        public string GenerateCaptcha(int len)
        {
            string str = "";
            while (len-- > 0)
            {
                string type = str += (char)Program.GetRandom.Next('0', '9');
            }
            return str;
        }
        internal string IP;
        //internal int SpeedHackSuspiction;
        //internal int SpeedHackSuspiction2;
        public DateTime LastOnlineStamp = DateTime.Now;
        internal DateTime LastCheatPacket = DateTime.Now;
        internal bool LootEnduranceBooks = false;
        internal bool LootTortoiseGemFragment = false;

        public uint EntityID { get { return Player.UID; } }

        public object Name { get { return Player.Name; } }
        public uint Inviting;

        public Game.ArenaDuel Arena;
        public int Arena2;
        public int Shots = 0;
        public int Hits = 0;
        public int Chains = 0;
        public int MaxChains = 0;
        public DateTime LastVote;
        public bool Hit = false;
        public uint Dueler;
    }
}
