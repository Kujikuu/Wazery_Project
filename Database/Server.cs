using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using LightConquer_Project.Game.MsgServer;
using LightConquer_Project.Game.MsgFloorItem;
using Extensions;
using System.IO;

namespace LightConquer_Project.Database
{
    public class Server
    {
        ///Fiend's Lair
        public static ushort AncientBeadX = 196, AncientBeadY = 252;
        public static bool GatesOpened = false;
        ///
        public static ushort[] ExpBallProf = new ushort[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 };
        public static ushort[] PriceUpdatePorf = new ushort[] { 600, 600, 600, 600, 600, 600, 1200, 1800, 3000, 3600, 6000, 7200, 7200, 7200, 7200, 8318, 12170, 17735, 25639, 31537 };

        public static DateTime GoldenTreeExpirationDate = DateTime.FromBinary(0);
        public static uint GoldenTreeClaimed = 0;
        public static uint MaxAvaliableGoldenTreeClaim = 0;
        public static List<uint> NoAgateMap = new List<uint>() { 9996, 2054, 2071, 1005, 1036, 2056, 700, 1038, 1039, 6000, 6001, 6002, 601, 6000, 6001, 1005, 1767, 700, 1858, 1860, 3852, Game.MsgTournaments.MsgEliteGroup.WaitingAreaID, 1768, 1038 };
        public static List<uint> RedeemActivated = new List<uint>();
        public static int DB = 0, Plus3Stone = 0, Parement = 0, ChiPack = 0, ToughDrill = 0, StarDrill = 0;
        public static uint lostlady = 0;
        public static DateTime RetailDuration;
        public static byte LordTokenCount = 3;

        public static DateTime DragonIslandBansheeHour, DragonIslandSpookHour, DragonIslandNemsisHour;
        public static DateTime ChasmBloodyBansheeHour, ChasmChillingSpookHour, ChasmNetherTyrantHour, ChasmDragonWraithHour;
        public static Extensions.Time32 ResetStamp = Extensions.Time32.Now.AddMilliseconds(KernelThread.ResetDayStamp);

        public static ushort[] PriceUpdatePorf2 = new ushort[] { 36000, 36000, 36000, 36000, 36000, 36000, 18367, 12328, 7377, 6164, 3688, 3082, 3082, 3082, 3082, 2670, 1825, 1251, 866, 704 };
        public static Dictionary<DBLevExp.Sort, Dictionary<byte, DBLevExp>> LevelInfo = new Dictionary<DBLevExp.Sort, Dictionary<byte, DBLevExp>>();

        public static ConcurrentDictionary<uint, TheCrimeTable> TheCrimePoll = new ConcurrentDictionary<uint, TheCrimeTable>();
        public static Database.ActivityTask ActivityTasks = new ActivityTask();
        public static Dictionary<uint, Dictionary<uint, Dictionary<uint, Tuple<uint, uint, uint, string>>>> ClientAgates = new Dictionary<uint, Dictionary<uint, Dictionary<uint, Tuple<uint, uint, uint, string>>>>();

        public static InfoHeroReward TableHeroRewards = new InfoHeroReward();

        public static Dictionary<ushort, ushort> WeaponSpells = new Dictionary<ushort, ushort>();
        public static MagicType Magic = new MagicType();

        public static LotteryTable Lottery = new LotteryTable();
        public static SubProfessionInfo SubClassInfo = new SubProfessionInfo();
        public static Dictionary<uint, Game.MsgMonster.MonsterFamily> MonsterFamilies = new Dictionary<uint, Game.MsgMonster.MonsterFamily>();
        public static Extensions.Counter ITEM_Counter = new Extensions.Counter(1);
        public static Extensions.Counter Inbox_Counter = new Extensions.Counter(1);

        public static Rifinery RifineryItems;
        public static RefinaryBoxes DBRerinaryBoxes;
        public static ItemType ItemsBase;
        public static Dictionary<uint, Role.GameMap> ServerMaps;
        public static ConcurrentDictionary<uint, Client.GameClient> GamePoll;
        public static List<int> NameUsed;
        public static RebornInfomations RebornInfo;

        public static ArenaTable Arena = new ArenaTable();
        public static TeamArenaTable TeamArena = new TeamArenaTable();

        public static Extensions.Counter ClientCounter = new Extensions.Counter(1000000);
        public static ConfiscatorTable QueueContainer = new ConfiscatorTable();

        public static bool FullLoading = false;
#if Arabic
              public static Dictionary<ushort, string> MapName = new Dictionary<ushort, string>() { { 1015, "BirdIsland" }, { 1011, "PhoenixCastle" }, { 1000, "DesertCity" }, { 1020, "ApeMountain" }, { 1001, "MysticCastle" } };
   
#else
        public static Dictionary<uint, string> MapName = new Dictionary<uint, string>() { { 1015, "BirdIsland" }, { 1011, "PhoenixCastle" }, { 1000, "DesertCity" }, { 1020, "ApeMountain" }, { 1001, "MysticCastle" } };
   
#endif
       public static uint ResetServerDay = 0;
        public static unsafe void Reset(Extensions.Time32 Clock)
        {
            if (Clock > ResetStamp)
            {
                if (DateTime.Now.DayOfYear != ResetServerDay)
                {
                    try
                    {
                     

                        Arena.ResetArena();
                        TeamArena.ResetArena();

                        foreach (var flowerclient in Role.Instance.Flowers.ClientPoll.Values)
                        {
                            foreach (var flower in flowerclient)
                                flower.Amount2day = 0;
                        }
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            foreach (var client in GamePoll.Values)
                            {


                                client.Player.TodayChampionPoints = 0;

                                client.Player.ChangeEpicTrojan = client.Player.ChangeArrayEpicTrojan =
                        client.Player.ChangeMr_MirrorEpicTrojan = client.Player.ChangeGeneralPakEpicTrojan = 0;
                                client.Player.CanChangeEpicMaterial = client.Player.CanChangeArrayEpicMaterial =
                                    client.Player.CanChangeMr_MirrorEpicMaterial = client.Player.CanChangGeneralPakMaterial = 1;

                                client.Player.UseChiToken = 0;
                                //
                                client.Player.skillmaster = 0;
                                client.Player.SkillChampionship = 0;
                                client.Player.PassTheBomb = 0;
                                client.Player.LastManStand = 0;
                                client.Player.Get5Out = 0;
                                client.Player.Get3Out = 0;
                                client.Player.FreezeWar = 0;
                                client.Player.FFa = 0;
                                client.Player.DragonWar = 0;
                                client.Player.ZombieWar = 0;
                                //
                                client.Player.TowerOfMysterychallenge = 3;
                                client.Player.TOMChallengeToday = 0;
                                client.Player.TowerOfMysteryChallengeFlag = 0;
                                client.Player.TOMSelectChallengeToday = 0;
                                client.Player.ClaimTowerAmulets = 0;
                                client.Player.TOMClaimTeamReward = 0;
                                client.Player.TOMRefreshReward = 0;
                                client.Player.QuestGUI.RemoveQuest(6126);
                                client.Player.GiantGhasomKill = 0;
                                client.Player.GiantGhasomKillClaimed = false;
                                client.Player.ClaimedBCPToday = false;
                                client.Player.FateFruitUsed = 0;
                                client.Player.WrongAnswers = new int[5] { 3, 3, 3, 3, 3 };
                                client.Player.OpenHousePack = 0;
                                client.MyExchangeShop.Reset();
                                if (client.Player.DailyMonth == 0)
                                    client.Player.DailyMonth = (byte)DateTime.Now.Month;
                                if (client.Player.DailyMonth != DateTime.Now.Month)
                                {
                                    client.Player.DailySignUpRewards = 0;
                                    client.Player.DailySignUpDays = 0;
                                    client.Player.DailyMonth = (byte)DateTime.Now.Month;
                                }
                                if (client.Player.MyJiangHu != null)
                                {
                                    client.Player.MyJiangHu.FreeCourse = 10000;
                                    client.Player.MyJiangHu.FreeTimeToday = 0; 
                                    client.Player.MyJiangHu.RoundBuyPoints = 0;
                                }
                                client.Player.ArenaKills = client.Player.ArenaDeads = 0;
                                client.Player.HitShoot = client.Player.MisShoot = 0;

                                client.Player.DbTry = false;
                                client.Player.QuestTry = false;
                                client.Player.LotteryEntries = 0;
                                client.Player.Day = DateTime.Now.DayOfYear;
                                client.Player.BDExp = 0;
                                client.Player.TCCaptainTimes = 0;
                                client.Player.ExpBallUsed = 0;
                                client.DemonExterminator.FinishToday = 0;

                                if (client.Player.MyChi != null)
                                {
                                    client.Player.MyChi.ChiPoints = client.Player.MyChi.ChiPoints + 300;
                                    Game.MsgServer.MsgChiInfo.MsgHandleChi.SendInfo(client, Game.MsgServer.MsgChiInfo.Action.Upgrade);
                                }
                                client.Player.Flowers.FreeFlowers = 1;
                                foreach (var flower in client.Player.Flowers)
                                    flower.Amount2day = 0;


                                if (client.Player.Flowers.FreeFlowers > 0)
                                {
                                    client.Send(stream.FlowerCreate(Role.Core.IsBoy(client.Player.Body)
                                        ? Game.MsgServer.MsgFlower.FlowerAction.FlowerSender
                                        : Game.MsgServer.MsgFlower.FlowerAction.Flower
                                        , 0, 0, client.Player.Flowers.FreeFlowers));
                                }

                                if (client.Player.Level >= 90)
                                {
                                    client.Player.Enilghten = ServerDatabase.CalculateEnlighten(client.Player);
                                    client.Player.SendUpdate(stream, client.Player.Enilghten, Game.MsgServer.MsgUpdate.DataType.EnlightPoints);
                                }

                                client.Player.BuyKingdomDeeds = 0;
                                client.Player.QuestGUI.RemoveQuest(35024);
                                client.Player.QuestGUI.RemoveQuest(35007);
                                client.Player.QuestGUI.RemoveQuest(35025);
                                client.Player.QuestGUI.RemoveQuest(35028);
                                client.Player.QuestGUI.RemoveQuest(35034);

                                //---- reset Quests
                                client.Player.QuestGUI.RemoveQuest(6390);
                                client.Player.QuestGUI.RemoveQuest(6329);
                                client.Player.QuestGUI.RemoveQuest(6245);
                                client.Player.QuestGUI.RemoveQuest(6049);
                                client.Player.QuestGUI.RemoveQuest(6366);
                                client.Player.QuestGUI.RemoveQuest(6014);
                                client.Player.QuestGUI.RemoveQuest(2375);
                                client.Player.QuestGUI.RemoveQuest(6126);
                                client.Player.DailyHeavenChance = client.Player.DailyMagnoliaChance
                                    = client.Player.DailyMagnoliaItemId
                                    = client.Player.DailyHeavenChance = client.Player.DailySpiritBeadCount = client.Player.DailyRareChance = 0;
                                //
                            }
                        }
                        ResetServerDay = (uint)DateTime.Now.DayOfYear;
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                }
                ResetStamp.Value = Clock.Value + KernelThread.ResetDayStamp;
            }
        }
        public static void Initialize()
        {
            ServerMaps = new Dictionary<uint, Role.GameMap>();
            GamePoll = new ConcurrentDictionary<uint, Client.GameClient>();
            NameUsed = new List<int>();

            WindowsAPI.IniFile IniFile = new WindowsAPI.IniFile(System.IO.Directory.GetCurrentDirectory() + "\\shell.ini");
            Program.ServerConfig.IPAddres = IniFile.ReadString("ServerInfo", "AddresIP","");
            Program.ServerConfig.AuthPort = IniFile.ReadUInt16("ServerInfo", "Auth_Port",9960);
            Program.ServerConfig.GamePort = IniFile.ReadUInt16("ServerInfo", "Game_Port",5816);
            Program.ServerConfig.ServerName = IniFile.ReadString("ServerInfo", "ServerName","FastConquer");
            Program.ServerConfig.LoaderPort = IniFile.ReadUInt16("ServerInfo", "LoaderPort", 9901);
            Program.ServerConfig.LoaderIP = IniFile.ReadString("ServerInfo", "LoaderIP", "");
            Program.ServerConfig.OfficialWebSite = IniFile.ReadString("ServerInfo", "WebSite", "");

            Program.ServerConfig.WebPort = IniFile.ReadUInt16("ServerInfo", "WebPort", 9900);
            Program.ServerConfig.AccServerIPAddres = IniFile.ReadString("ServerInfo", "AccServerIPAddres", "");

            Program.ServerConfig.Port_BackLog = IniFile.ReadUInt16("InternetPort", "BackLog",100);
            Program.ServerConfig.Port_ReceiveSize = IniFile.ReadUInt16("InternetPort", "ReceiveSize",8194);
            Program.ServerConfig.Port_SendSize = IniFile.ReadUInt16("InternetPort", "SendSize",1024);

            Program.ServerConfig.DbLocation = IniFile.ReadString("Database", "Location","");
            Program.ServerConfig.CO2Folder = IniFile.ReadString("Database", "CO2FOLDER","");

            Program.ServerConfig.InterServerAddress = IniFile.ReadString("InterServer", "AddresIP", "");
            Program.ServerConfig.InterServerPort = IniFile.ReadUInt16("InterServer", "Port", 9915);
            Program.ServerConfig.IsInterServer = IniFile.ReadUInt32("InterServer", "IsInterServer", 0) > 0 ? true : false;
            RetailDuration = DateTime.FromBinary(IniFile.ReadInt64("RetailRedeem", "Duration", 0));




            RebornInfo = new RebornInfomations();
            RebornInfo.Load();

            ITEM_Counter.Set(IniFile.ReadUInt32("Database", "ItemUID", 0));
            uint nextitem = ITEM_Counter.Next;
            ClientCounter.Set(IniFile.ReadUInt32("Database", "ClientUID", 1000000));
            uint nextclient = ClientCounter.Next;
            ResetServerDay = IniFile.ReadUInt32("Database", "Day", 0);
            
            ItemsBase = new ItemType();
            RifineryItems = new Rifinery();
            DBRerinaryBoxes = new RefinaryBoxes();
            ItemsBase.Loading();

            //-------------------------- Load shops -------------------
            Shops.ChampionShop.Load();
            Shops.EShopFile.Load();
            Shops.EShopV2File.Load();
            Shops.HonorShop.Load();
            Shops.RacePointShop.Load();
            Shops.ShopFile.Load();
            //--------------------------

            SystemBanned.Load();
            SystemBannedAccount.Load();
            LoadExpInfo();
            DataCore.AtributeStatus.Load();
            Role.GameMap.LoadMaps();
            NpcServer.LoadNpcs();
            Magic.Load();
            LoadMonsters();
            Tranformation.Int();
            QuestInfo.Init();
            LoadPortals();
            SubClassInfo.Load();
            ChiTable.Load();
            FlowersTable.Load();
            NobilityTable.Load();
            Role.Instance.Associate.Load();
            CoatStorage.Load();
            //PerfectionPower.Load();
            NpcServer.LoadSobNpcs();

            LeagueTable.Load();

            GuildTable.Load();
            Database.ClanTable.Load();
            JianHuTable.LoadStatus();
            JianHuTable.LoadJiangHu();
            TaskRewards.Load();
            DBEffects.Load();

            //InnerPowerTable.LoadDBInformation();
            ExchangeShop.LoadDBInfo();

            QuizShow.Load();


            Game.MsgTournaments.MsgSchedules.ClassPkWar.Load();
            Game.MsgTournaments.MsgSchedules.WeeklyPKChampion.Load();
            Game.MsgTournaments.MsgSchedules.MonthlyPKChampion.Load();
            Game.MsgTournaments.MsgSchedules.ElitePkTournament.Load();
            Game.MsgTournaments.MsgSchedules.TeamPkTournament.Load();
            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.Load();
            //Database.PrestigeRanking.Load();
            RankItems.LoadAllItems();
            TitleStorage.LoadDBInformation();
            ItemRefineUpgrade.Load();

            Roulettes.Load();
            TheCrimeTable.Load();
            ActivityTasks.Load();
            Role.Statue.Load();
            Role.KOBoard.KOBoardRanking.Load();
            TableHeroRewards.LoadInformations();
            Database.Disdain.Load();

            RechargeShop.Load();
            Arena.Load();
            TeamArena.Load();
            //InnerPowerTable.Load();

            var tournament = Game.MsgTournaments.MsgSchedules.Tournaments[Game.MsgTournaments.TournamentType.SpeedHunterGame];
            (tournament as Game.MsgTournaments.MsgSpeedHunterGame).Load();

            MsgInterServer.Instance.CrossElitePKTournament.Load();

            Database.TutorInfo.Load();
            LoadAgates();
            Database.PocketInformations.LoadPokerTables();

            InfoDemonExterminators.Create();
            Lottery.LoadLotteryItems();
            QueueContainer.Load();
            LoadMapName();
            GroupServerList.Load();
            VoteSystem.Load();
            TaskReward.Load();
            Database.RanksTable.Initialize();
            FullLoading = true;
        }
        public static void ColletResource()
        {
          /*  Console.WriteLine("Memory used before collection:       {0:N0}",
                      GC.GetTotalMemory(false));

            // Collect all generations of memory.
            GC.Collect();
            Console.WriteLine("Memory used after full collection:   {0:N0}",
                              GC.GetTotalMemory(true));*/
        }
        public static void LoadMapName()
        {
  
            if (System.IO.File.Exists(Program.ServerConfig.DbLocation + "GameMapEx.ini"))
            {
                foreach (var map in ServerMaps.Values)
                {
                    WindowsAPI.IniFile ini = new WindowsAPI.IniFile("GameMapEx.ini");
                    map.Name = ini.ReadString(map.ID.ToString(), "Name", Program.ServerConfig.ServerName);
                }
            }
        }
        public static void LoadExpInfo()
        {
            if (System.IO.File.Exists(Program.ServerConfig.DbLocation + "levexp.txt"))
            {
                using (System.IO.StreamReader read = System.IO.File.OpenText(Program.ServerConfig.DbLocation + "levexp.txt"))
                {
                    while (true)
                    {
                        string GetLine = read.ReadLine();
                        if (GetLine == null) return;
                        string[] line = GetLine.Split(' ');
                        DBLevExp exp = new DBLevExp();
                        exp.Action = (DBLevExp.Sort)byte.Parse(line[0]);
                        exp.Level = byte.Parse(line[1]);
                        exp.Experience = ulong.Parse(line[2]);
                        exp.UpLevTime = int.Parse(line[3]);
                        exp.MentorUpLevTime = int.Parse(line[4]);

                        if (!LevelInfo.ContainsKey(exp.Action))
                            LevelInfo.Add(exp.Action, new Dictionary<byte, DBLevExp>());

                        LevelInfo[exp.Action].Add(exp.Level, exp);

                    }
                }
            }
            GC.Collect();
        }
        #region new load monsters
        

        #endregion
        public static void LoadAgates()
        {
            try
            {
                if (!System.IO.Directory.Exists(Program.ServerConfig.DbLocation + "\\Agates\\"))
                    System.IO.Directory.CreateDirectory(Program.ServerConfig.DbLocation + "\\Agates\\");
                WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
                foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Agates\\"))
                {
                    ini.FileName = fname;
                    uint UID = ini.ReadUInt32("Character", "UID", 0);
                    ClientAgates.Add(UID, new Dictionary<uint, Dictionary<uint, Tuple<uint, uint, uint, string>>>());
                    int CountGates = ini.ReadInt32("Character", "Count", 0);
                    for (int y = 0; y < CountGates; y++)
                    {
                        uint ItemUID = ini.ReadUInt32("Item" + y.ToString(), "UID", 0);
                        int Count = ini.ReadInt32("Item" + y.ToString(), "Count", 0);
                        ClientAgates[UID].Add(ItemUID, new Dictionary<uint, Tuple<uint, uint, uint, string>>(Count));
                        for (int x = 0; x < Count; x++)
                        {
                            string[] strs = ini.ReadString("Item" + y.ToString(), "Gate" + x.ToString(), "").Split('~');
                            ClientAgates[UID][ItemUID].Add((uint)x, Tuple.Create(uint.Parse(strs[0]), uint.Parse(strs[1]), uint.Parse(strs[2]), strs[3]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MyConsole.WriteException(e);
            }
        }
        public static void SaveAgates()
        {
            try
            {
                foreach (var agate in ClientAgates)
                {
                    WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\Agates\\" + agate.Key + ".ini");
                    write.Write<uint>("Character", "UID", agate.Key);
                    write.Write<int>("Character", "Count", agate.Value.Count);
                    int x = 0;
                    foreach (var gate in agate.Value)
                    {
                        write.Write<uint>("Item" + x.ToString(), "UID", gate.Key);
                        write.Write<int>("Item" + x.ToString(), "Count", gate.Value.Count);
                        foreach (var coors in gate.Value)
                        {
                            write.Write<string>("Item" + x.ToString(), "Gate" + coors.Key.ToString(), coors.Value.Item1 + "~" + coors.Value.Item2 + "~" + coors.Value.Item3 + "~" + coors.Value.Item4);
                        }
                        x++;
                    }
                }
            }
            catch (Exception e)
            {
                MyConsole.WriteException(e);
            }
        }
        public static void LoadMonsters()
        {
            try
            {
                DragonIslandBansheeHour = DateTime.Now;
                DragonIslandNemsisHour = DateTime.Now;
                DragonIslandSpookHour = DateTime.Now;
                ChasmBloodyBansheeHour = DateTime.Now;
                ChasmChillingSpookHour = DateTime.Now;
                ChasmNetherTyrantHour = DateTime.Now;
                ChasmDragonWraithHour = DateTime.Now;
                WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
                foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Monsters\\"))
                {
                    ini.FileName = fname;
                    Game.MsgMonster.MonsterFamily Family = new Game.MsgMonster.MonsterFamily();
                    Family.ID = ini.ReadUInt32("cq_monstertype", "id", 0);
                    Family.Name = ini.ReadString("cq_monstertype", "name", "INVALID_MOB");

                    Family.Level = ini.ReadUInt16("cq_monstertype", "level", 0);
                    Family.MaxAttack = ini.ReadInt32("cq_monstertype", "attack_max", 0);
                    Family.MinAttack = ini.ReadInt32("cq_monstertype", "attack_min", 0);
                    if (Family.Name == "INVALID_MOB" || Family.Level == 0 || Family.ID == 0 || Family.MinAttack > Family.MaxAttack)
                    {
                        Console.WriteLine("MONSTER FILE CORRUPT: \r\n" + fname + "\r\n");
                        continue;
                    }
                    Family.Defense = ini.ReadUInt16("cq_monstertype", "defence", 0);
                    Family.Mesh = ini.ReadUInt16("cq_monstertype", "lookface", 0);
                    Family.MaxHealth = ini.ReadInt32("cq_monstertype", "life", 0);
                    Family.ViewRange = 16;
                    Family.AttackRange = ini.ReadSByte("cq_monstertype", "attack_range", 0);
                    Family.Dodge = ini.ReadByte("cq_monstertype", "dodge", 0);
                    Family.DropBoots = ini.ReadByte("cq_monstertype", "drop_shoes", 0);
                    Family.DropNecklace = ini.ReadByte("cq_monstertype", "drop_necklace", 0);
                    Family.DropRing = ini.ReadByte("cq_monstertype", "drop_ring", 0);
                    Family.DropArmet = ini.ReadByte("cq_monstertype", "drop_armet", 0);
                    Family.DropArmor = ini.ReadByte("cq_monstertype", "drop_armor", 0);
                    Family.DropShield = ini.ReadByte("cq_monstertype", "drop_shield", 0);
                    Family.DropWeapon = ini.ReadByte("cq_monstertype", "drop_weapon", 0);
                    Family.DropMoney = ini.ReadUInt16("cq_monstertype", "drop_money", 0);
                    Family.DropHPItem = ini.ReadUInt32("cq_monstertype", "drop_hp", 0);
                    Family.DropMPItem = ini.ReadUInt32("cq_monstertype", "drop_mp", 0);
                    Family.Boss = ini.ReadByte("cq_monstertype", "Boss", 0);
                    Family.Defense2 = ini.ReadInt32("cq_monstertype", "defence2", 0);
                    if (Family.Boss != 0)
                        Family.AttackRange = 3;
                    //defence2

                    //if (Family.Boss != 0)
                    //    Console.WriteLine(Family.Name);
                    //if (Family.Dodge > 50)
                    //    Console.WriteLine(Family.Name);
                    Family.MoveSpeed = ini.ReadInt32("cq_monstertype", "move_speed", 0);
                    Family.AttackSpeed = ini.ReadInt32("cq_monstertype", "attack_speed", 0);
                    Family.SpellId = ini.ReadUInt32("cq_monstertype", "magic_type", 0);

                    Family.ExtraCritical = ini.ReadUInt32("cq_monstertype", "critical", 0);
                    Family.ExtraBreack = ini.ReadUInt32("cq_monstertype", "break", 0);

                    Family.extra_battlelev = ini.ReadInt32("cq_monstertype", "extra_battlelev", 0);
                    Family.extra_exp = ini.ReadInt32("cq_monstertype", "extra_exp", 0);
                    Family.extra_damage = ini.ReadInt32("cq_monstertype", "extra_damage", 0);


                    if (Family.Boss == 0 && Family.MaxAttack > 3000)
                    {
                        Family.MaxAttack = Family.MaxAttack / 2;
                        Family.MinAttack = Family.MinAttack / 2;
                    }
                    Family.DropSpecials = new Game.MsgMonster.SpecialItemWatcher[ini.ReadInt32("SpecialDrop", "Count", 0)];
                    for (int i = 0; i < Family.DropSpecials.Length; i++)
                    {
                        string[] Data = ini.ReadString("SpecialDrop", i.ToString(), "", 32).Split(',');

                        Family.DropSpecials[i] = new Game.MsgMonster.SpecialItemWatcher(uint.Parse(Data[0]), int.Parse(Data[1]));
                    }

                    Family.CreateItemGenerator();
                    Family.CreateMonsterSettings();

                    MonsterFamilies.Add(Family.ID, Family);
                }
                using (var reader = new StreamReader(Program.ServerConfig.DbLocation + "\\Spawns.txt"))
                {
                    var values = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in values)
                    {
                        var data = line.Split(',');
                        uint ID = uint.Parse(data[0]);
                        uint MapId = uint.Parse(data[1]);
                        Game.MsgMonster.MobCollection colletion = new Game.MsgMonster.MobCollection(MapId);
                        if (MapId == 8800)
                        {

                        }
                        if (colletion.ReadMap())
                        {

                            colletion.LocationSpawn = "";
                            Game.MsgMonster.MonsterFamily famil;
                            if (!MonsterFamilies.TryGetValue(ID, out famil))
                            {
                                continue;
                            }
                            if (Game.MsgMonster.MonsterRole.SpecialMonsters.Contains(famil.ID))
                                continue;
                            Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                            Monster.SpawnX = ushort.Parse(data[2]);
                            Monster.SpawnY = ushort.Parse(data[3]);
                            Monster.MaxSpawnX = (ushort)(Monster.SpawnX + ushort.Parse(data[4]));
                            Monster.MaxSpawnY = (ushort)(Monster.SpawnY + ushort.Parse(data[5]));
                            Monster.MapID = MapId;
                            Monster.SpawnCount = (byte)int.Parse(data[6]);//"maxnpc", 0);//max_per_gen", 0);
                            //if (Monster.ID == 18)
                            //    Monster.SpawnCount *= 2;
                            Monster.rest_secs = int.Parse(data[7]);


                            if (Monster.MapID == 1011 || Monster.MapID == 3071 || Monster.MapID == 1770 || Monster.MapID == 1771 || Monster.MapID == 1772
                                || Monster.MapID == 1773 || Monster.MapID == 1774 || Monster.MapID == 1775 || Monster.MapID == 1777
                                || Monster.MapID == 1782 || Monster.MapID == 1785 || Monster.MapID == 1786 || Monster.MapID == 1787
                                || Monster.MapID == 1794)
                                Monster.SpawnCount = (byte)(int.Parse(data[8]) / 3);
                            //if (Monster.MapID == 1020 || Monster.MapID == 1011 || Monster.MapID == 1000)
                            //{
                            //    if (Monster.SpawnCount > 5)
                            //    {
                            //        if (Monster.MapID == 1020)
                            //        {
                            //            Monster.SpawnCount = Monster.SpawnCount * 3;
                            //            Monster.MaxSpawnX += 20;
                            //            Monster.MaxSpawnY += 20;
                            //        }
                            //        else
                            //            Monster.SpawnCount = Monster.SpawnCount * 2;
                            //    }
                            //}
                            if (Monster.ID == 18)
                                Monster.SpawnCount *= 2;
                            colletion.Add(Monster);
                        }


                    }
                }

                foreach (string fmap in System.IO.Directory.GetDirectories(Program.ServerConfig.DbLocation + "\\MonsterSpawns\\"))
                {
                    uint tMapID;
                    if (!uint.TryParse(fmap.Remove(0, (Program.ServerConfig.DbLocation + "\\MonsterSpawns\\").Length), out tMapID))
                        continue;
                    if (tMapID == 1002 || tMapID == 1020 || tMapID == 1038 || tMapID == 1000 || tMapID == 3935 || tMapID == 1015)
                        continue;
                    Game.MsgMonster.MobCollection colletion = new Game.MsgMonster.MobCollection(tMapID);
                    if (colletion.ReadMap())
                    {
                        foreach (string fmobtype in System.IO.Directory.GetDirectories(fmap))
                        {
                            foreach (string ffile in System.IO.Directory.GetFiles(fmobtype))
                            {
                                ini.FileName = ffile;
                                colletion.LocationSpawn = ffile;

                                uint ID = ini.ReadUInt32("cq_generator", "npctype", 0);

                                Game.MsgMonster.MonsterFamily famil;
                                if (!MonsterFamilies.TryGetValue(ID, out famil))
                                {
                                    continue;
                                }
                                if (Game.MsgMonster.MonsterRole.SpecialMonsters.Contains(famil.ID))
                                    continue;
                                Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                                Monster.SpawnX = ini.ReadUInt16("cq_generator", "bound_x", 0);
                                Monster.SpawnY = ini.ReadUInt16("cq_generator", "bound_y", 0);
                                Monster.MaxSpawnX = (ushort)(Monster.SpawnX + ini.ReadUInt16("cq_generator", "bound_cx", 0));
                                Monster.MaxSpawnY = (ushort)(Monster.SpawnY + ini.ReadUInt16("cq_generator", "bound_cy", 0));
                                Monster.MapID = ini.ReadUInt32("cq_generator", "mapid", 0);
                                Monster.SpawnCount = ini.ReadByte("cq_generator", "max_per_gen", 0);//"maxnpc", 0);//max_per_gen", 0);
                                Monster.rest_secs = ini.ReadInt32("cq_generator", "rest_secs", 0);

                                if (/*Monster.MapID == 1011 || */Monster.MapID == 3071 || Monster.MapID == 1770 || Monster.MapID == 1771 || Monster.MapID == 1772
                                    || Monster.MapID == 1773 || Monster.MapID == 1774 || Monster.MapID == 1775 || Monster.MapID == 1777
                                    || Monster.MapID == 1782 || Monster.MapID == 1785 || Monster.MapID == 1786 || Monster.MapID == 1787
                                    || Monster.MapID == 1794)
                                    Monster.SpawnCount = ini.ReadByte("cq_generator", "maxnpc", 0);
                                //if (Monster.MapID == 1020
                                //    || Monster.MapID == 1000
                                //    || Monster.MapID == 1015)
                                //    Monster.SpawnCount *= 4;
                                //if (Monster.MapID == 1002)
                                //    Monster.SpawnCount *= 2;
                                //if (Monster.MapID == 1013)
                                //    Monster.SpawnCount *= 9;
                                //if (Monster.ID == 0008 && Monster.MapID == 1011)
                                //    Monster.SpawnCount /= 2;
                                colletion.Add(Monster);
                            }
                        }
                    }
                }

                //LoadMapMonsters("Monsters1768.txt");
                //LoadMapMonsters("Monsters1779.txt");
                //LoadMapMonsters("Monsters1780.txt");
                //LoadMapMonsters("Monsters3935.txt");
                //LoadMapMonsters("Monsters3998.txt");
                //LoadMapMonsters("Monsters10089.txt");//need
                //LoadMapMonsters("Monsters10137.txt");//dragonisland
                //LoadMapMonsters("Monsters10166.txt");//Gosem

                //LoadMapMonsters("Monsters8822.txt");
                GC.Collect();
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static void LoadMapMonsters(string file)
        {
            if (System.IO.File.Exists(Program.ServerConfig.DbLocation + "MonsterSpawns\\" + file + ""))
            {
                using (System.IO.StreamReader read = System.IO.File.OpenText(Program.ServerConfig.DbLocation + "MonsterSpawns\\" + file + ""))
                {
                    while (true)
                    {

                        string aline = read.ReadLine();
                        if (aline != null && aline != "")
                        {
                            try
                            {
                                string[] line = aline.Split(',');
                                uint body = uint.Parse(line[1]);
                                string name = line[2];
                                if (name.Contains("Titan"))
                                    continue;
                                if (name == "WhiteTiger")
                                {

                                }
                                ushort X = ushort.Parse(line[3]);
                                ushort Y = ushort.Parse(line[4]);
                                uint Map = uint.Parse(line[5]);
                                var GMap = Database.Server.ServerMaps[Map];
                                if (GMap.MonstersColletion == null)
                                {
                                    GMap.MonstersColletion = new Game.MsgMonster.MobCollection(GMap.ID);
                                }
                                else if (GMap.MonstersColletion.DMap == null)
                                    GMap.MonstersColletion.DMap = GMap;
                                foreach (var _monster in MonsterFamilies.Values)
                                {
                                    if (_monster.Name == name)
                                    {

                                        Game.MsgMonster.MonsterFamily Monster = _monster.Copy();

                                        Monster.SpawnX = X;
                                        Monster.SpawnY = Y;
                                        Monster.MaxSpawnX = (ushort)(X + 1);
                                        Monster.MaxSpawnY = (ushort)(Y + 1);
                                        Monster.MapID = GMap.ID;
                                        Monster.SpawnCount = 1;

                                        Game.MsgMonster.MonsterRole rolemonster = GMap.MonstersColletion.Add(Monster, false, 0, true);
                                        break;
                                    }


                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                break;
                            }
                        }
                        else
                            break;

                    }
                }
            }
        }

        public unsafe static void AddMapMonster(ServerSockets.Packet stream, Role.GameMap map, uint ID, ushort x, ushort y, ushort max_x, ushort max_y, byte count, uint DinamicID = 0, bool RemoveOnDead = true
            , Game.MsgFloorItem.MsgItemPacket.EffectMonsters m_effect = Game.MsgFloorItem.MsgItemPacket.EffectMonsters.None,  string streffect = "")
        {
            if (map.MonstersColletion == null)
            {
                map.MonstersColletion = new Game.MsgMonster.MobCollection(map.ID);
            }
            if (map.MonstersColletion.ReadMap())
            {

                Game.MsgMonster.MonsterFamily famil;
                if (MonsterFamilies.TryGetValue(ID, out famil))
                {
                    Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                    Monster.SpawnX = x;
                    Monster.SpawnY = y;
                    Monster.MaxSpawnX = (ushort)(x + max_x);
                    Monster.MaxSpawnY = (ushort)(y + max_y);
                    Monster.MapID = map.ID;
                    Monster.SpawnCount = count;
                    Game.MsgMonster.MonsterRole rolemonster = map.MonstersColletion.Add(Monster, RemoveOnDead, DinamicID, true);
                    Monster.LavaX = x;
                    Monster.LavaY = y;
                    if (rolemonster == null)
                    {
                        Console.WriteLine("Eror monster spawn. Server.");
                        Console.WriteLine("Mob MapID " + Monster.MapID + "!");
                        Console.WriteLine("Mob Name " + Monster.Name + "!");
                        return;
                    }

                    Game.MsgServer.ActionQuery action = new Game.MsgServer.ActionQuery()
                    {
                        ObjId = rolemonster.UID,
                        Type = Game.MsgServer.ActionType.RemoveEntity
                    };
                    rolemonster.Send(stream.ActionCreate(&action));
                    rolemonster.Send(rolemonster.GetArray(stream, false));

                    if (streffect != null)
                    {
                        rolemonster.SendString(stream, MsgStringPacket.StringID.Effect, streffect);
                    }
                        
                        
                   
                    if (m_effect != Game.MsgFloorItem.MsgItemPacket.EffectMonsters.None && rolemonster != null)
                    {
                        Game.MsgFloorItem.MsgItemPacket effect = Game.MsgFloorItem.MsgItemPacket.Create();
                        effect.m_UID = (uint)m_effect;
                        effect.m_X = rolemonster.X;
                        effect.m_Y = rolemonster.Y;
                        effect.DropType = MsgDropID.Earth;
                        rolemonster.Send(stream.ItemPacketCreate(effect));
                        rolemonster.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, "glebesword");
                    }
                    if (rolemonster.HitPoints > 65535)
                    {
                        Game.MsgServer.MsgUpdate Upd = new Game.MsgServer.MsgUpdate(stream, rolemonster.UID, 2);
                        stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, rolemonster.Family.MaxHealth);
                        stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, rolemonster.HitPoints);
                        stream = Upd.GetArray(stream);
                        rolemonster.Send(stream);
                    }
                }
            }
        }
        public unsafe static bool AddFloor(ServerSockets.Packet stream, Role.GameMap map, uint ID, ushort x, ushort y, ushort spelllevel, Database.MagicType.Magic dbspell,Client.GameClient Owner,  uint GuildID, uint OwnerUID, uint DinamicID = 0,string Name = "", bool RemoveOnDead = true)
        {
            try
            {
                if (map.MonstersColletion == null)
                {
                    map.MonstersColletion = new Game.MsgMonster.MobCollection(map.ID);
                }
                if (map.MonstersColletion.ReadMap())
                {

                    Game.MsgMonster.MonsterFamily famil;
                    if (MonsterFamilies.TryGetValue(1, out famil))
                    {
                        Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                        Monster.SpawnX = x;
                        Monster.SpawnY = y;
                        Monster.MaxSpawnX = (ushort)(x + 1);
                        Monster.MaxSpawnY = (ushort)(y + 1);
                        Monster.MapID = map.ID;
                        Monster.SpawnCount = 1;
                        Game.MsgMonster.MonsterRole rolemonster = map.MonstersColletion.Add(Monster, RemoveOnDead, DinamicID, true);
                        if (rolemonster == null)
                        {
                            //invalid x ,y
                            return false;
                        }
                        rolemonster.Family.ID = ID;
                        rolemonster.IsFloor = true;
                        rolemonster.FloorStampTimer = DateTime.Now.AddSeconds(7);
                        rolemonster.Family.Settings = Game.MsgMonster.MonsterSettings.Lottus;

                        rolemonster.FloorPacket = new MsgItemPacket();
                        rolemonster.FloorPacket.m_UID = rolemonster.UID;
                        rolemonster.FloorPacket.m_ID = ID;
                        rolemonster.FloorPacket.m_X = x;
                        rolemonster.FloorPacket.m_Y = y;
                        rolemonster.FloorPacket.MaxLife = 25;
                        rolemonster.FloorPacket.Life = 25;
                        rolemonster.FloorPacket.DropType = MsgDropID.Effect;
                        rolemonster.FloorPacket.m_Color = 13;
                        rolemonster.FloorPacket.ItemOwnerUID = OwnerUID;
                        rolemonster.FloorPacket.GuildID = GuildID;
                        rolemonster.FloorPacket.FlowerType = 2;//2;
                        rolemonster.FloorPacket.Timer = Role.Core.TqTimer(rolemonster.FloorStampTimer);
                        rolemonster.FloorPacket.Name = Name;

                        rolemonster.DBSpell = dbspell;
                        rolemonster.Family.MaxHealth = 25;
                        rolemonster.HitPoints = 25;
                        rolemonster.OwnerFloor = Owner;
                        rolemonster.SpellLevel = spelllevel;
                        

                        if (rolemonster == null)
                        {
                            Console.WriteLine("Eror monster spawn. Server.");
                            return  false;
                        }
                        map.View.EnterMap<Role.IMapObj>(rolemonster);
                        rolemonster.Send(rolemonster.GetArray(stream, false));
                        return true;
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
            return false;

        }
        public unsafe static void LoadDatabase()
        {
            try
            {
                foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
                {
                    WindowsAPI.IniFile IniFile = new WindowsAPI.IniFile(fname);
                    IniFile.FileName = fname;
                    string name = IniFile.ReadString("Character", "Name", "");
                    NameUsed.Add(name.GetHashCode());
                }
            }
            catch (Exception e)
            {
                MyConsole.WriteException(e);
            }
        }
        public unsafe static void SaveDatabase()
        {
            if (!FullLoading)
                return;
            try
            {
                try
                {
                    Save(new Action(Database.JianHuTable.SaveJiangHu));
                }
                catch (Exception e) { MyConsole.SaveException(e); }
                try
                {
                    Save(new Action(Role.Instance.Associate.Save));
                }
                catch (Exception e) { MyConsole.SaveException(e); }
                try
                {
                    Save(new Action(Database.GuildTable.Save));
                }
                catch (Exception e) { MyConsole.SaveException(e); }
                try
                {
                    Save(new Action(Database.AttackCompatetor.Save));
                }
                catch (Exception e) { MyConsole.SaveException(e); }
                WindowsAPI.IniFile IniFile = new WindowsAPI.IniFile("");
                IniFile.FileName = System.IO.Directory.GetCurrentDirectory() + "\\shell.ini";
                IniFile.Write<uint>("Database", "ItemUID", ITEM_Counter.Count);
                IniFile.Write<uint>("Database", "ClientUID", ClientCounter.Count);
                IniFile.Write<uint>("Database", "Day", ResetServerDay);
                Save(new Action(Database.ClanTable.Save));
                Save(new Action(QueueContainer.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.GuildWar.Save));
                Save(new Action(SaveAgates));
                //Save(new Action(SaveFestival));
                Save(new Action(TheCrimeTable.Save));
                Save(new Action(Arena.Save));
                Save(new Action(TeamArena.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.ClassPkWar.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.WeeklyPKChampion.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.MonthlyPKChampion.Save));

                Save(new Action(Game.MsgTournaments.MsgSchedules.ElitePkTournament.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.TeamPkTournament.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.Save));
                Save(new Action((Game.MsgTournaments.MsgSchedules.Tournaments[Game.MsgTournaments.TournamentType.SpeedHunterGame]
                    as Game.MsgTournaments.MsgSpeedHunterGame).Save));
                Save(new Action(SystemBanned.Save));
                Save(new Action(SystemBannedAccount.Save));
                //Save(new Action(InnerPowerTable.Save));
                Save(new Action(VoteSystem.Save));
                Save(new Action(TaskReward.Save));
                Save(new Action(LeagueTable.Save));

                Save(new Action(RechargeShop.Save));
                Save(new Action(Game.MsgTournaments.MsgSchedules.ClanWar.Save));
          
            
               IniFile = new WindowsAPI.IniFile("");
                IniFile.FileName = System.IO.Directory.GetCurrentDirectory() + "\\shell.ini";
                IniFile.Write<uint>("Database", "ItemUID", ITEM_Counter.Count);
                IniFile.Write<uint>("Database", "ClientUID", ClientCounter.Count);
                IniFile.Write<uint>("Database", "Day", ResetServerDay);
                Save(new Action (RankItems.SaveRanks));
                Save(new Action(Role.Statue.Save));
               
                Save(new Action(PrestigeRanking.Save));
                Save(new Action(Role.KOBoard.KOBoardRanking.Save));
                Save(new Action(MsgInterServer.Instance.CrossElitePKTournament.Save));
            }
            catch (Exception e) { MyConsole.WriteException(e); }
        }
        public static void Save(Action obj)
        {
            try
            {
                obj.Invoke();
            }
            catch (Exception e) { MyConsole.SaveException(e); }
        }
        public static void LoadPortals()
        {

            if (System.IO.File.Exists(Program.ServerConfig.DbLocation + "portals.ini"))
            {
                using (System.IO.StreamReader read = System.IO.File.OpenText(Program.ServerConfig.DbLocation + "portals.ini"))
                {
                    ushort count = 0;
                    while (true)
                    {
                        string lines = read.ReadLine();
                        if (lines == null)
                            break;
                        ushort Map = ushort.Parse(lines.Split('[')[1].ToString().Split(']')[0]);
                        ushort Count = ushort.Parse(read.ReadLine().Split('=')[1]);
                        for (ushort x = 0; x < Count; x++)
                        {
                            Role.Portal portal = new Role.Portal();
                            string[] line = read.ReadLine().Split('=')[1].Split(' ');
                            portal.MapID = ushort.Parse(line[0]);
                            portal.X = ushort.Parse(line[1]);
                            portal.Y = ushort.Parse(line[2]);

                            string[] dline = read.ReadLine().Split('=')[1].Split(' ');
                            portal.Destiantion_MapID = ushort.Parse(dline[0]);
                            portal.Destiantion_X = ushort.Parse(dline[1]);
                            portal.Destiantion_Y = ushort.Parse(dline[2]);
                            if (ServerMaps.ContainsKey(portal.MapID))
                                ServerMaps[portal.MapID].Portals.Add(portal);
                            count++;
                        }
                    }
                    Console.WriteLine(" # Load [" + count + "] portals !");
                }
            }
            GC.Collect();
        }
    }
}
