using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace LightConquer_Project.Database
{
    public class RefinaryBoxes : Dictionary<uint, RefinaryBoxes.Boxe>
    {
        public class Boxe
        {
            public UInt32 Identifier, Position;
            public Boolean Untradable;
            public Rifinery.RefineryType Type;
        }

        public RefinaryBoxes()
        {
            using (Database.DBActions.Read reader = new DBActions.Read("databaserefineryboxes.txt"))
            {
                if (reader.Reader())
                {
                    for (int x = 0; x < reader.Count; x++)
                    {
                        DBActions.ReadLine line = new DBActions.ReadLine(reader.ReadString("0,0"), ',');
                        Boxe box = new Boxe();
                        box.Identifier = line.Read((uint)0);
                        box.Position = line.Read((uint)0);
                        box.Type = (Rifinery.RefineryType)line.Read((byte)0);
                        box.Untradable = line.Read((byte)0) == 1;

                        Add(box.Identifier, box);
                    }
                }

            }

        }

        public uint GainRefineryItem(uint ID)
        {
            Boxe RefineryB = null;
            if (TryGetValue(ID, out RefineryB))
            {
                List<Rifinery.Item> Possible = new List<Rifinery.Item>();
                foreach (Rifinery.Item RefineryI in Database.Server.RifineryItems.Values)
                {
                    if (RefineryI.Type == RefineryB.Type)
                    {
                        if (RefineryI.ForItemPosition == RefineryB.Position)
                        {
                            if (RefineryB.Identifier >= 3004197 && RefineryB.Identifier <= 3004226
                                || RefineryB.Identifier >= 3004266 && RefineryB.Identifier <= 3004280)
                            {
                                if (RefineryI.Level > 3)
                                    Possible.Add(RefineryI);
                            }
                            else
                                if (RefineryI.Level < 6)
                                Possible.Add(RefineryI);
                        }
                    }
                }
                if (Possible.Count > 0)
                {
                    Random Rand = new Random();
                    Int32 x = Rand.Next(1, Possible.Count);
                    Rifinery.Item Refinery = Possible[x];

                    if (Refinery != null)
                    {
                        return Refinery.ItemID;
                    }
                }
            }
            return 0;

        }
    }
    public class Rifinery : Dictionary<uint, Rifinery.Item>
    {
        public enum RefineryType
        {
            None = 0,
            MDefence = 1,
            CriticalStrike = 2,
            SkillCriticalStrike = 3,
            Immunity = 4,
            Break = 5,
            Counteraction = 6,
            Detoxication = 7,
            Block = 8,
            Penetration = 9,
            Intensification = 10,

            FinalMDamage = 11,
            FinalMAttack = 12
        }
        public Rifinery()
        {
            string[] baseText = File.ReadAllLines(Program.ServerConfig.DbLocation + "Rifinery.txt");
            foreach (string aline in baseText)
            {
                string[] line = aline.Split(' ');
                Item ite = new Item();
                ite.ItemID = uint.Parse(line[0]);
                ite.Level = CalculateLevel(line[1]);
                ite.Procent = uint.Parse(line[2]);
                ite.Type = (RefineryType)byte.Parse(line[3]);


                ite.Type2 = (RefineryType)byte.Parse(line[4]);
                ite.Procent2 = uint.Parse(line[5]);

                string UseItemName = line[53];
                if (ite.ItemID >= 3006165 && ite.ItemID <= 3006170)
                {
                    UseItemName = UseItemName.Replace("(", "");
                    UseItemName = UseItemName.Split(')')[0];
                }
                else if (ite.ItemID >= 3004136)
                {
                    UseItemName = UseItemName.Replace("[", "");
                    UseItemName = UseItemName.Split(']')[0];
                }

                ite.Name = UseItemName;
                ite.ForItemPosition = ForItemPosition(UseItemName);
                //  if(ite.ItemID >= 3004136)
                //      MyConsole.WriteLine(ite.ItemID + " level " + ite.Level + " foritem= " + ite.ForItemPosition + " ite.Type= " + ite.Type + " procent" + ite.Procent + " " + UseItemName);
                if (!ContainsKey(ite.ItemID))
                    Add(ite.ItemID, ite);

                if (!ItemType.Refinary.ContainsKey(ite.Level))
                    ItemType.Refinary.Add(ite.Level, new Dictionary<uint, Item>());
                if (!ItemType.Refinary[ite.Level].ContainsKey(ite.ItemID))
                    ItemType.Refinary[ite.Level].Add(ite.ItemID, ite);

            }
            Console.WriteLine(" # Load [" + baseText.Length + "] Refinery items !");
            GC.Collect();
        }

        public uint ForItemPosition(string name)
        {
            uint pos = 0;
            if (name == "Bow" || name == "2-Handed" || name == "1-Handed" || name == "Backsword" || name == "2-handed" || name == "1-handed")
                pos = (ushort)Role.Flags.ConquerItem.RightWeapon;
            if (name == "Shield" || name == "Hossu")
                pos = (ushort)Role.Flags.ConquerItem.LeftWeapon;
            if (name == "Ring" || name == "Bracelet")
                pos = (ushort)Role.Flags.ConquerItem.Ring;
            if (name == "Armor")
                pos = (ushort)Role.Flags.ConquerItem.Armor;
            if (name == "Boots")
                pos = (ushort)Role.Flags.ConquerItem.Boots;
            if (name == "Headgear")
                pos = (ushort)Role.Flags.ConquerItem.Head;
            if (name == "Necklace" || name == "Bag")
                pos = (ushort)Role.Flags.ConquerItem.Necklace;
            return pos;
        }

        public static uint CalculateLevel(string name)
        {
            byte level = 0;
            if (name.Contains("Normal")) level = 1;
            if (name.Contains("Refined")) level = 2;
            if (name.Contains("Unique")) level = 3;
            if (name.Contains("Elite")) level = 4;
            if (name.Contains("Super")) level = 5;
            if (name.Contains("Sacred")) level = 6;
            return level;
        }
        public class Item
        {
            public string Name = "";
            public uint ItemID = 0;
            public uint Level = 0;
            public uint ForItemPosition = 0;
            public uint Procent = 0;
            public uint Procent2 = 0;
            public RefineryType Type = 0;
            public RefineryType Type2 = 0;

        }
    }
    public class ItemType : Dictionary<uint, ItemType.DBItem>
    {
        public static Dictionary<ushort, Dictionary<uint, ItemType.DBItem>> PurificationItems = new Dictionary<ushort, Dictionary<uint, DBItem>>();
        public static Dictionary<uint, ItemType.DBItem> Accessorys = new Dictionary<uint, DBItem>();
        #region TopAccesorys
        public static List<uint> TopAccesorys = new List<uint>()
        {
                    360071, //CaribbeanFlag
                    360072, //Blackbeard`sFlag
                    360073, //PirateKing`sFlag
                    360074, //CaptainMorgan`sFlag
                    360150, //FloralHeart
                    360151, //CucumberHammer
                    380028, //DoorPanel
                    200501, //HalloweenBroom
                    360102, //LoveBlossom
                    360085, //DaintyGoat
                    360084, //SwiftHorse
                    360091, //CunningSpirit
                    360092, //CloudDragon
                    350081, //CarpFishPole
                    350082, //SpecialOar
                    360152, //CatHammer
                    360153, //RacoonRacket
                    360154, //UltraRacket
                    360101, //LoveConfession
                    360144, //GoldenDragonHammer
                    350054, //DivineGun
                    360149, //RodofRoses
                    360150, //FloralHeart
                    360159, //PhoenixLegend
                    360186, //KingCatStaff(Charm)
                    380046, //MewMewShield(Charm)
                    360176, //StarCatStaff(Charm)
                    360185, //KingCatStaff
                    350103, //SilverDragonslayer(2-handed)
                    350104, //GoldenDragonslayer(2-handed)
                    350105, //BronzeDragonslayer(2-handed)
                    360201, //SilverDragonslayer(1-handed)
                    360202, //GoldenDragonslayer(1-handed)
                    360203, //BronzeDragonslayer(1-handed)
                    360208, //PhoenixDream(Sparkle)
                    360216, //TomatoUmbrella
                    360217, //BubbleUmbrella
                    370050, //BubbleofSeahorse
                    370048, //LovesickBow(Sparkle)
                    380054, //IdealShield(Sparkle)
                    380056};//BubbleWaves
        #endregion
        #region NormalAccessorys
        public static List<uint> NormalAccessorys = new List<uint>()
        {
            350001,//Broom 2-HandedAccessory
            350002,//BlueGuitar 2-HandedAccessory
            350004,//DragonBone 2-HandedAccessory
            350005,//Oar 2-HandedAccessory
            350006,//Pipa 2-HandedAccessory
            350007,//PalmLeafFan 2-HandedAccessory
            350008,//IronShovel 2-HandedAccessory
            350009,//FrozenTuna 2-HandedAccessory
            350010,//IceStick 2-HandedAccessory
            350011,//Broom 2-HandedAccessory
            350012,//BlueGuitar 2-HandedAccessory
            350013,//IronWillFlag WeaponAccessory
            350014,//DragonBone 2-HandedAccessory
            350015,//Oar 2-HandedAccessory
            350016,//Pipa 2-HandedAccessory
            350017,//PalmLeafFan 2-HandedAccessory
            350018,//IronShovel 2-HandedAccessory
            350019,//FrozenTuna 2-HandedAccessory
            350020,//IceStick 2-HandedAccessory
            360001,//Wrench 1-HandedAccessory
            360002,//WoodenClub 1-HandedAccessory
            360003,//Umbrella 1-HandedAccessory
            360004,//Blowfish 1-HandedAccessory
            360005,//FeatherDuster 1-HandedAccessory
            360006,//Spatula 1-HandedAccessory
            360007,//InvincibleFist 1-HandedAccessory
            360008,//FishPole 1-HandedAccessory
            360009,//Pan 1-HandedAccessory
            360010,//Handbag 1-HandedAccessory
            360011,//Backpack 1-HandedAccessory
            360012,//SportsBag 1-HandedAccessory
            360013,//Bunny 1-HandedAccessory
            360014,//GoodEveningBear 1-HandedAccessory
            360015,//Rod 1-HandedAccessory
            360016,//Clap 1-HandedAccessory
            360017,//HeavyHammer 1-HandedAccessory
            360018,//LightSaber 1-HandedAccessory
            360019,//TennisRacket 1-HandedAccessory
            360020,//ApeCityHam 1-HandedAccessory
            360021,//Wrench 1-HandedAccessory
            360022,//WoodenClub 1-HandedAccessory
            360023,//Umbrella 1-HandedAccessory
            360024,//Blowfish 1-HandedAccessory
            360025,//FeatherDuster 1-HandedAccessory
            360026,//Spatula 1-HandedAccessory
            360027,//InvincibleFist 1-HandedAccessory
            360028,//FishPole 1-HandedAccessory
            360029,//Pan 1-HandedAccessory
            360030,//Handbag 1-HandedAccessory
            360031,//Backpack 1-HandedAccessory
            360032,//SportsBag 1-HandedAccessory
            360033,//Bunny 1-HandedAccessory
            360034,//GoodEveningBear 1-HandedAccessory
            360035,//Rod 1-HandedAccessory
            360036,//Clap 1-HandedAccessory
            360037,//HeavyHammer 1-HandedAccessory
            360038,//LightSaber 1-HandedAccessory
            360039,//TennisRacket 1-HandedAccessory
            360040,//ApeCityHam 1-HandedAccessory
            370001,//WoodenBow BowAccessory
            370002,//LoveBow BowAccessory
            370003,//SeaHorse BowAccessory
            370004,//Harp BowAccessory
            370005,//ForceBow BowAccessory
            370011,//WoodenBow BowAccessory
            370012,//LoveBow BowAccessory
            370013,//SeaHorse BowAccessory
            370014,//Harp BowAccessory
            370015,//ForceBow BowAccessory
            380001,//Wok ShieldAccessory
            380002,//TurtleShell ShieldAccessory
            380003,//LoveShield ShieldAccessory
            380004,//SunFlower ShieldAccessory
            380005,//Wheel ShieldAccessory
            380011,//Wok ShieldAccessory
            380012,//TurtleShell ShieldAccessory
            380013,//LoveShield ShieldAccessory
            380014,//SunFlower ShieldAccessory
            380015,//Wheel ShieldAccessory
            380016//ArcaneShield WeaponAccessory
        };
        #endregion
        
        
        #region NormalGarments
        public static List<uint> NormalGarments = new List<uint>()
        {
            181305,// WhitePhoenix
            181315,// WhiteElegance
            181325,// WhiteCelestial
            181335,// WeddingGown
            181345,// ColorfulDress
            181355,// DarkWizard
            181365,// PrairieWind
            181375,// SongofTianshan
            181385,// RoyalDignity
            181395,// UglyDuck
            181405,// BrownPhoenix
            181415,// BrownElegance
            181425,// BrownCelestial
            181435,// WeddingGown
            181445,// ColorfulDress
            181455,// DarkWizard
            181465,// PrairieWind
            181475,// SongofTianshan
            181485,// RoyalDignity
            181505,// BlackPhoenix
            181515,// BlackElegance
            181525,// BlackCelestial
            181535,// WeddingGown
            181545,// ColorfulDress
            181555,// DarkWizard
            181565,// PrairieWind
            181575,// SongofTianshan
            181585,// RoyalDignity
            181605,// RedPhoenix
            181615,// RedElegance
            181625,// RedCelestial
            181635,// WeddingGown
            181645,// ColorfulDress
            181655,// DarkWizard
            181665,// PrairieWind
            181675,// SongofTianshan
            181685,// RoyalDignity
            181705,// GreenPhoenix
            181715,// GreenElegance
            181725,// CyanCelestial
            181735,// WeddingGown
            181745,// ColorfulDress
            181755,// DarkWizard
            181765,// PrairieWind
            181775,// SongofTianshan
            181785,// RoyalDignity
            181805,// BluePhoenix
            181815,// BlueElegance
            181825,// BlueCelestial
            181835,// WeddingGown
            181845,// ColorfulDress
            181855,// DarkWizard
            181865,// PrairieWind
            181875,// SongofTianshan
            181885,// RoyalDignity
            181905,// PurplePhoenix
            181915,// PurpleElegance
            181925,// PurpleCelestial
            181935,// WeddingGown
            181945,// ColorfulDress
            181955,// DarkWizard
            181965,// PrairieWind
            181975,// SongofTianshan
            181985,// RoyalDignity
            182305,// SouthofCloud
            182315,// BonfireNight
            182325,// AngelicalDress
            182335,// BlueDream
            182345,// MoonOrchid
            182355,// FeatherDress
            182365,// HeavenScent
            182375,// CherryBlossom
            182385,// DreaminFlowers
            182405,// SouthofCloud
            182415,// BonfireNight
            182425,// AngelicalDress
            182435,// BlueDream
            182445,// MoonOrchid
            182465,// HeavenScent
            182485,// DreaminFlowers
            182505,// SouthofCloud
            182515,// BonfireNight
            182525,// AngelicalDress
            182535,// BlueDream
            182545,// MoonOrchid
            182565,// HeavenScent
            182585,// DreaminFlowers
            182605,// SouthofCloud
            182615,// BonfireNight
            182625,// AngelicalDress
            182635,// BlueDream
            182645,// MoonOrchid
            182665,// HeavenScent
            182685,// DreaminFlowers
            182705,// SouthofCloud
            182715,// BonfireNight
            182725,// AngelicalDress
            182735,// BlueDream
            182745,// MoonOrchid
            182765,// HeavenScent
            182785,// DreaminFlowers
            182805,// SouthofCloud
            182815,// BonfireNight
            182825,// AngelicalDress
            182835,// BlueDream
            182845,// MoonOrchid
            182865,// HeavenScent
            182885,// DreaminFlowers
            182905,// SouthofCloud
            182915,// BonfireNight
            182925,// AngelicalDress
            182935,// BlueDream
            182945,// MoonOrchid
            182965,// HeavenScent
            182985,// DreaminFlowers
            183305,// SandRaider
            183315,// SamuraiLegacy
            183325,// DuskRomance
            183345,// TaekwondoUniform
            183365,// PartyDress(Lady)
            183375,// CaribbeanPirate
            183385,// Doctor`sJacket
            183395,// Delight~of~Speed
            184305,// FatalAllure
            184315,// ChristmasCostume
            184325,// Evernight
            184335,// PunkRocker
            184355,// EveningWear
            184365,// WhiteCollarSuit
            185305,// PhoenixGown
            185315,// SwallowtailGown
            185355,// WarlockRobe
            185525,// CeletialRobe
            187315,// MonkeyKingGown
            191305,// GoodLuck
            191405,// DivineRobe
            191505,// GoodLuck
            191605,// GoodLuck
            191705,// GoodLuck
            191805,// GoodLuck
            191905,// GoodLuck
            193300,// Beggar`sRags
            194300// SchoolUniform 
        };
        #endregion
        
        
        
        
        public static Dictionary<uint, Dictionary<uint, Rifinery.Item>> Refinary = new Dictionary<uint, Dictionary<uint, Rifinery.Item>>();
        public static Dictionary<uint, ItemType.DBItem> Garments = new Dictionary<uint, ItemType.DBItem>();
        public static Dictionary<uint, ItemType.DBItem> SteedMounts = new Dictionary<uint, ItemType.DBItem>();
        public static List<uint> unabletradeitem = new List<uint>()
        {
            750000,
            722700,
            3005975,
            3005976,
            3005979,
            3005974,
            3005977,
            3005978,
            3600025,
            3200000,
            729549,
            3008994,
            3200005,
            3600029,
            3007108,
            3007109,
            3007110,
            3200004,
            3600031,
            3600027,
            3600024


        };
        public const uint
            MaxPlus = 12,
            MaxEnchant = 255,
            MaxDurability = 7099,
            MemoryAgate = 720828,
            GoldPrize = 2100075,
            ExpBall = 723700,
            MeteorTearPacket = 723711,
            MeteorTear = 1088002,
            OneStonePacket = 723712,
            ArenaExperience = 723912,
            OneStone = 730001,
            SuperDragonBallScroll = 3302770,
            SuperDragonBallScroll_B_ = 3302896,
            DragonBallScroll = 720028,
            DragonBall = 1088000,
            MeteorScroll = 720027,
            SuperMeteorScroll = 3302769,
            Meteor = 1088001,
            MoonBox = 721080,
            ExperiencePotion = 723017,
            NinjaAmulet = 723583,
            PowerExpBall = 722057,
            DoubleExp = 723017,
            SkillTeamPKPack = 720981,
            ToughDrill = 1200005,
            StarDrill = 1200006,
            SuperToroiseGem = 700073,
            /////
            P7WeaponSoulScrap = 3004243,
            P7EquipmentSoulScrap = 3004244,
            SacredRefineryPack = 3004249,
            SacredRefineryScrap = 3004245,
            P7WeaponSoulPack = 3004247,
            P7EquipmentSoulPack = 3004248,
            /////
            LoveHatMale = 3005975,
            ////


            ExpBall2 = 723834;

        public static bool IsCPPack(uint ID)
        {
            return ID >= 720653 && ID <= 720667 || ID >= 720675 && ID <= 720679 || ID >= 720681 && ID <= 720685 || ID >= 720687 && ID <= 720691 || ID >= 720693 && ID <= 720697;
        }
        public static bool IsHairItem(uint ID)
        {
            return ID == LoveHatMale;
        }
        public static bool IsMoneyBag(uint ID)
        {
            return ID >= 723713 && ID <= 723723 || ID == 3005945 || ID == 3008452;
        }
        public static bool IsMagicSpar(uint ID)
        {
            return ID >= 3006587 && ID <= 3006616 || ID == 3006582;
        }
        public static bool CheckAddGemFan(Role.Flags.Gem gem)
        {
            return gem == Role.Flags.Gem.NormalThunderGem || gem == Role.Flags.Gem.RefinedThunderGem || gem == Role.Flags.Gem.SuperThunderGem;
        }
        public static bool CheckAddGemTower(Role.Flags.Gem gem)
        {
            return gem == Role.Flags.Gem.NormalGloryGem || gem == Role.Flags.Gem.RefinedGloryGem || gem == Role.Flags.Gem.SuperGloryGem;
        }
        

        public static uint GetItemPoints(DBItem _dbitem, Game.MsgServer.MsgGameItem item)
        {

            uint Points = (uint)(item.GetPerfectionPosition == 9 ? 0 : 50);
            if (_dbitem.Level <= 120)
                Points += (uint)((double)((double)_dbitem.Level / 10d) * 30);
            else if (_dbitem.Level > 120 && _dbitem.Level <= 130)
                Points += (uint)((double)((double)_dbitem.Level / 10d) * 50);
            else
                Points += (uint)((double)((double)_dbitem.Level / 10d) * 60);

            switch (_dbitem.ID % 10)
            {
                case 6: Points += 50; break;
                case 7: Points += 150; break;
                case 8: Points += 250; break;
                case 9: Points += 450; break;

            }
            switch (item.Bless)
            {
                case 1: Points += 100; break;
                case 3: Points += 300; break;
                case 5: Points += 500; break;
                case 7: Points += 700; break;
            }
            if (item.SocketOne != Role.Flags.Gem.NoSocket)
            {
                Points += 1000;
                switch ((ushort)item.SocketOne % 10)
                {
                    case 1: Points += 200; break;
                    case 2: Points += 500; break;
                    case 3: Points += 800; break;
                }
            }
            if (item.SocketTwo != Role.Flags.Gem.NoSocket)
            {
                Points += 2500;
                switch ((ushort)item.SocketTwo % 10)
                {
                    case 1: Points += 200; break;
                    case 2: Points += 500; break;
                    case 3: Points += 800; break;
                }
            }
            switch (item.Plus)
            {
                case 1: Points += 200; break;
                case 2: Points += 600; break;
                case 3: Points += 1200; break;
                case 4: Points += 1800; break;
                case 5: Points += 2600; break;
                case 6: Points += 3500; break;
                case 7: Points += 4800; break;
                case 8: Points += 5800; break;
                case 9: Points += 6800; break;
                case 10: Points += 7800; break;
                case 11: Points += 8800; break;
                case 12: Points += 10000; break;

            }
            switch (item.Purification.PurificationLevel)
            {
                case 7: Points += 2000; break;
                case 6: Points += 1600; break;
                case 5: Points += 1200; break;
                case 4:
                    {
                        if (ItemPosition(item.ITEM_ID) == (ushort)Role.Flags.ConquerItem.Ring)
                            Points += 2000;
                        else
                            Points += 800;
                        break;
                    }
                case 3:
                    {
                        Points += 500; break;

                    }
            }
            switch (item.Refinary.EffectLevel)
            {

                case 6: Points += 2000; break;
                case 5: Points += 1600; break;
                case 4: Points += 1200; break;
                case 3: Points += 800; break;
                case 2: Points += 200; break;
                case 1: Points += 100; break;
            }
            if (item.GetPerfectionPosition != 9)//steed
                Points += (uint)(item.Enchant * 2);

            if (item.PerfectionLevel >= 0 && item.PerfectionLevel < 10) Points += (uint)(item.PerfectionLevel * 180);
            if (item.PerfectionLevel >= 10 && item.PerfectionLevel < 19) Points += (uint)(item.PerfectionLevel * 400);
            if (item.PerfectionLevel >= 19 && item.PerfectionLevel < 28) Points += (uint)(item.PerfectionLevel * 650);
            if (item.PerfectionLevel >= 28)
            {
                Points += (uint)((item.PerfectionLevel - 28) * 100) + 28 * 650;
                Points -= 550;
                //Points -= (uint)(item.PerfectionLevel + 1) * 10;
            }
            if (item.PerfectionLevel > 0)
            {
                if (item.IsWeapon)
                    Points += 200;
            }
            return Points;
        }
        public static uint GetGemID(Role.Flags.Gem Gem)
        {
            return (uint)(700000 + (byte)Gem);
        }

        public static uint[] TalismanExtra = new uint[13] { 0, 6, 30, 70, 240, 740, 2240, 6670, 20000, 60000, 62000, 67000, 73000 };

        public static ushort PurifyStabilizationPoints(byte plevel)
        {
            return purifyStabilizationPoints[Math.Min(plevel - 1, (byte)5)];
        }

        static ushort[] purifyStabilizationPoints = new ushort[6] { 10, 30, 60, 100, 150, 200 };

        public static ushort RefineryStabilizationPoints(byte elevel)
        {
            return refineryStabilizationPoints[Math.Min(elevel - 1, (byte)4)];
        }
        static ushort[] refineryStabilizationPoints = new ushort[5] { 10, 30, 70, 150, 270 };

        public string GetItemName(uint ID)
        {
            DBItem item;
            if (Server.ItemsBase.TryGetValue(ID, out item))
            {
                return item.Name;
            }
            return "";
        }

        public static uint ComposePlusPoints(byte plus)
        {
            return ComposePoints[Math.Min(plus, (byte)12)];
        }
        public static uint StonePlusPoints(byte plus)
        {
            return StonePoints[Math.Min((int)plus, 8)];
        }

        public static bool IsWarriorEpicWeapons(uint ID)
        {
            return ID / 1000 == 624;
        }
        static ushort[] StonePoints = new ushort[9] { 1, 10, 40, 120, 360, 1080, 3240, 9720, 29160 };
        static ushort[] ComposePoints = new ushort[13] { 20, 20, 80, 240, 720, 2160, 6480, 19440, 58320, 2700, 5500, 9000, 0 };

        public static Role.Flags.SoulTyp GetSoulPosition(uint ID)
        {
            if (ID >= 820001 && ID <= 820072)
                return Role.Flags.SoulTyp.Headgear;
            if (ID >= 821002 && ID <= 821032)
                return Role.Flags.SoulTyp.Necklace;
            if (ID >= 824002 && ID <= 824017)
                return Role.Flags.SoulTyp.Boots;
            if (ID >= 823000 && ID <= 823057)
                return Role.Flags.SoulTyp.Ring;
            if (ID >= 800000 && ID <= 800019 || ID >= 800050 && ID <= 800110 || ID >= 800130 && ID <= 800141 /**/|| ID >= 800701 && ID <= 800724 || ID >= 800800 && ID <= 800809) //800916
                return Role.Flags.SoulTyp.OneHandWeapon;
            if (ID >= 800200 && ID <= 800617)
                return Role.Flags.SoulTyp.TwoHandWeapon;
            if (ID >= 822001 && ID <= 822070)
                return Role.Flags.SoulTyp.Armor;

            return Role.Flags.SoulTyp.None;
        }
        public static bool CompareSoul(uint ITEMID, uint SoulID)
        {
            Role.Flags.SoulTyp soul = GetSoulPosition(SoulID);
            var positionit = GetItemSoulTYPE(ITEMID);
            if (positionit == soul)
                return true;
            return false;
        }
        public static Role.Flags.SoulTyp GetItemSoulTYPE(UInt32 itemid)
        {
            UInt32 iType = itemid / 1000;

            if (iType >= 111 && iType <= 118 || iType == 123 || iType >= 141 && iType <= 148)
                return Role.Flags.SoulTyp.Headgear;

            else if (iType >= 120 && iType <= 121)
                return Role.Flags.SoulTyp.Necklace;

            else if (iType >= 130 && iType <= 139)
                return Role.Flags.SoulTyp.Armor;

            else if (iType >= 150 && iType <= 152)
                return Role.Flags.SoulTyp.Ring;

            else if (iType == 160)
                return Role.Flags.SoulTyp.Boots;

            else if (IsTwoHand(itemid) || itemid >= 421003 && itemid <= 421439)
                return Role.Flags.SoulTyp.TwoHandWeapon;

            else if ((iType >= 410 && iType <= 490) || (iType >= 500 && iType <= 580) || (iType >= 601 && iType <= 613) || iType == 616 || iType == 614 || iType == 617 || iType == 622
                || iType == 624 || iType == 619)
                return Role.Flags.SoulTyp.OneHandWeapon;

            else
                return Role.Flags.SoulTyp.TwoHandWeapon;
        }

        public static uint MoneyItemID(uint value)
        {
            if (value < 100)
                return 1090000;
            else if (value < 399)
                return 1090010;
            else if (value < 5099)
                return 1090020;
            else if (value < 8099)
                return 1091000;
            else if (value < 12099)
                return 1091010;
            else
                return 1091020;
        }
        public static ulong CalculateExpBall(byte Level)
        {
            if (Level < 130)
                return (ulong)(680462.7536 + (3479.5308 * (Level / 2)) * Level);
            else if (Level < 135)
                return (ulong)((680462.7536 + (3479.5308 * (Level / 2)) * Level) * ((Level % 10) + 6));
            else
                return (ulong)((680462.7536 + (3479.5308 * (Level / 2)) * Level) * ((Level % 10) + 8));
        }
        public void Loading()
        {
            Dictionary<uint, ITPlus[]> itemsplus = new Dictionary<uint, ITPlus[]>();

            string[] baseplusText = File.ReadAllLines(Program.ServerConfig.DbLocation + "ItemAdd.ini");
            foreach (string line in baseplusText)
            {
                string _item_ = line.Trim();
                ITPlus pls = new ITPlus();
                pls.Parse(_item_);
                if (itemsplus.ContainsKey(pls.ID))
                {
                    itemsplus[pls.ID][pls.Plus] = pls;
                }
                else
                {
                    ITPlus[] a_pls = new ITPlus[13];
                    a_pls[pls.Plus] = pls;
                    itemsplus.Add(pls.ID, a_pls);
                }
            }
            string[] baseText = File.ReadAllLines(Program.ServerConfig.DbLocation + "itemtype.txt");
            foreach (string line in baseText)
            {
                string _item_ = line.Trim();
                if (_item_.Length > 11)
                {
                    if (_item_.IndexOf("//", 0, 2) != 0)
                    {
                        DBItem item = new DBItem();
                        item.Parse(line);
                        // if (item.ID == PowerExpBall)
                        //     continue;
                        if (itemsplus.ContainsKey(GetBaseID(item.ID)) || itemsplus.ContainsKey(GetBaseID(item.ID) + 10) || itemsplus.ContainsKey(GetBaseID(item.ID) + 20))
                        {
                            item.AllowUpgradePlus = true;
                            if (!itemsplus.TryGetValue(GetBaseID(item.ID), out item.Plus))
                            {
                                if (!itemsplus.TryGetValue(GetBaseID(item.ID) + 10, out item.Plus))
                                {
                                    if (!itemsplus.TryGetValue(GetBaseID(item.ID) + 20, out item.Plus))
                                    {
                                        int pos = ItemPosition(item.ID);
                                        if (pos < 6)
                                            Console.WriteLine("eroror item " + item.ID + " " + item.Name);
                                    }
                                }
                            }
                        }
                        if (!ContainsKey(item.ID))
                            Add(item.ID, item);
                    }
                }
            }
            baseText = File.ReadAllLines(Program.ServerConfig.DbLocation + "ItemtypeSub.txt");
            foreach (string line in baseText)
            {
                string _item_ = line.Trim();
                if (_item_.Length > 11)
                {
                    if (_item_.IndexOf("//", 0, 2) != 0)
                    {
                        DBItem item = new DBItem();
                        item.Parse(line);
                        // if (item.ID == PowerExpBall)
                        //     continue;
                        if (itemsplus.ContainsKey(GetBaseID(item.ID)) || itemsplus.ContainsKey(GetBaseID(item.ID) + 10) || itemsplus.ContainsKey(GetBaseID(item.ID) + 20))
                        {
                            item.AllowUpgradePlus = true;
                            if (!itemsplus.TryGetValue(GetBaseID(item.ID), out item.Plus))
                            {
                                if (!itemsplus.TryGetValue(GetBaseID(item.ID) + 10, out item.Plus))
                                {
                                    if (!itemsplus.TryGetValue(GetBaseID(item.ID) + 20, out item.Plus))
                                    {
                                        int pos = ItemPosition(item.ID);
                                        if (pos < 6)
                                            Console.WriteLine("eroror item " + item.ID + " " + item.Name);
                                    }
                                }
                            }
                        }
                        if (!ContainsKey(item.ID))
                            Add(item.ID, item);
                    }
                }
            }
            Console.WriteLine(" # Load [" + Count + "] items !");
            itemsplus = null;
            GC.Collect();
        }
        public uint DowngradeItem(uint ID)
        {
            try
            {
                ushort Tryng = 0;
                ushort firstposition = ItemPosition(ID);
                uint rebornid = ID;
                while (true)
                {
                    if (Tryng > 1000)
                        break;
                    Tryng++;
                    //shield !
                    if (ID >= 900000 && ID <= 900309)
                    {
                        if (this[rebornid].Level <= 40)
                            break;
                    }
                    if (this[rebornid].Level <= ItemMinLevel((Role.Flags.ConquerItem)ItemPosition(ID)))
                    {
                        break;
                    }
                    if (this.ContainsKey(rebornid - 10))
                    {
                        rebornid -= 10;
                    }
                    else if (this.ContainsKey(rebornid - 20))
                    {
                        rebornid -= 20;
                    }
                    else if (this.ContainsKey(rebornid - 30))
                    {
                        rebornid -= 30;
                    }
                    else if (this.ContainsKey(rebornid - 40))
                    {
                        rebornid -= 40;
                    }
                }
                if (firstposition == ItemPosition(rebornid) && this.ContainsKey(rebornid))
                    return rebornid;
                else
                    return ID;

            }
            catch (Exception e) { MyConsole.WriteLine(e.ToString()); return ID; }
        }
        public uint UpdateItem(uint id, out bool succesed)
        {
            ushort firstposition = ItemPosition(id);
            uint nextId = 0;
            if (this[id].Level < ItemMaxLevel((Role.Flags.ConquerItem)ItemPosition(id)))
            {
                if (this.ContainsKey(id + 10))
                {
                    nextId = id + 10;
                }
                else if (this.ContainsKey(id + 20))
                {
                    nextId = id + 20;
                }
                else if (this.ContainsKey(id + 30))
                {
                    nextId = id + 30;
                }
                else if (this.ContainsKey(id + 40))
                {
                    nextId = id + 40;
                }
            }
            if (firstposition == ItemPosition(nextId) && this.ContainsKey(nextId))
            {
                succesed = true;
                return nextId;
            }
            else
            {
                succesed = false;
                return id;
            }
        }
        public uint UpdateItem(uint id, Client.GameClient client)
        {
            uint nextId = id;
            if ((this[id].Level < ItemMaxLevel((Role.Flags.ConquerItem)ItemPosition(id))) && this[id].Level < client.Player.Level)
            {
                if (this.ContainsKey(id + 10))
                {
                    nextId = id + 10;
                }
            }
            return nextId;
        }
        public static byte ItemMinLevel(Role.Flags.ConquerItem postion)
        {
            switch (postion)
            {
                case 0: return 0;
                case Role.Flags.ConquerItem.Head: return 15;
                case Role.Flags.ConquerItem.Necklace: return 7;
                case Role.Flags.ConquerItem.Armor: return 15;
                case Role.Flags.ConquerItem.LeftWeapon: return 15;
                case Role.Flags.ConquerItem.RightWeapon: return 15;
                case Role.Flags.ConquerItem.Boots: return 10;
                case Role.Flags.ConquerItem.Ring: return 10;
                //case Role.Flags.ConquerItem.Tower: return 0;
                //case Role.Flags.ConquerItem.Fan: return 0;
                //case Role.Flags.ConquerItem.Steed: return 0;
                case Role.Flags.ConquerItem.Garment: return 0;
                //case Role.Flags.ConquerItem.RidingCrop: return 0;
            }
            return 0;
        }
        public static byte GetSashCounts(uint ID)
        {
            if (ID == 1100009) return 12;
            if (ID == 1100006) return 6;
            if (ID == 1100003) return 3;
            return 0;
        }
        public static bool IsSash(uint ID)
        {
            if (ID == 1100009 || ID == 1100006 || ID == 1100003) return true;
            return false;
        }
        public static bool IsPistol(uint ID)
        {
            return ID >= 612000 && ID <= 612439;
        }
        public static bool IsPickAxe(uint ID)
        {
            return ID == 562000;
        }
        public static bool IsRapier(uint ID)
        {
            return ID >= 611000 && ID <= 611439;
        }
        public static bool IsKnife(uint ID)
        {
            return ID >= 613000 && ID <= 613429;
        }
        public static bool IsShield(uint ID)
        {
            return ID >= 900000 && ID <= 900309;
        }
        /*   public static bool IsBow(uint ID)
           {
               return ID >= 4 && ID <= 900309;
           }*/
        public static bool IsTrojanEpicWeapon(uint ID)
        {
            return ID >= 614000 && ID <= 614439;
        }
        public static bool IsNinjaEpicWeapon(uint ID)
        {
            return ID >= 616000 && ID <= 616439;
        }
        public static bool IsPrayedBead(uint ID)
        {
            return ID >= 610000 && ID <= 610439;
        }
        public static bool IsMonkEpicWeapon(uint ID)
        {
            return ID >= 622000 && ID <= 622439;

        }
        public static bool IsTalisman(uint ID)
        {
            return ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Tower || ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Fan;
        }

        public static bool AllowToUpdate(Role.Flags.ConquerItem Position)
        {
            if (/*Position == Role.Flags.ConquerItem.RidingCrop*/
                 //|| Position == Role.Flags.ConquerItem.Fan
                  //|| Position == Role.Flags.ConquerItem.Tower
                 Position == Role.Flags.ConquerItem.Garment
                || Position == Role.Flags.ConquerItem.AleternanteGarment
                || Position == Role.Flags.ConquerItem.AleternanteBottle
                || Position == Role.Flags.ConquerItem.Bottle
                || Position == Role.Flags.ConquerItem.LeftWeaponAccessory
                || Position == Role.Flags.ConquerItem.RightWeaponAccessory
                //|| Position == Role.Flags.ConquerItem.SteedMount
                /*|| Position == Role.Flags.ConquerItem.Steed*/)
                return false;
            return true;
        }
        public static int TypeofItem(uint ID)
        {
            int ret = -1;
            uint Type1 = ID % 1000000;
            uint Type2 = Type1 / 1000;
            switch (Type2)
            {
                case 123:
                    return 1;
                case 203:
                    return 21;
                case 204:
                    return 22;
                case 300:
                    if (ID % 10 != 0)
                        return 14;
                    break;
                case 601:
                    return 4;
                case 201:
                case 202:
                    return 12;
                case 200:
                    return 20;
                case 350:
                    return 16;
                case 360:
                    return 15;
                case 370:
                    return 17;
                case 380:
                    return 18;
            }
            uint Type3 = ID % 10000000 / 100000;
            uint Type4 = Type1 / 10000;
            switch (Type3)
            {
                case 1:
                    switch (Type4)
                    {
                        case 11:
                        case 14:
                        case 17:
                            return 1;
                        case 18:
                        case 19:
                            return 11;
                        case 12:
                            return 2;
                        case 13:
                            return 3;
                        case 15:
                            return 7;
                        case 16:
                            return 8;
                    }
                    break;
                case 4:
                    return 4;
                case 5:
                    return 5;
                case 7:
                    return 9;
                case 9:
                    return 6;
                case 10:
                    return 0;
                default:
                    if (Type3 <= 29 && Type3 > 7)
                        return 9;
                    if (Type3 != 6)
                        return ret;
                    break;
            }
            return ret;
        }
        public static bool IsWindWalkerWeapon(uint ID)
        {
            return ID >= 626003 && ID <= 626439;
        }
        //public static ushort ItemPosition(uint ID)
        //{

        //    UInt32 iType = ID / 1000;
        //    if (iType == 622 || iType == 624 || iType == 626)
        //        return (ushort)Role.Flags.ConquerItem.RightWeapon;
        //    if (iType == 620)
        //        return (ushort)Role.Flags.ConquerItem.RightWeapon;
        //    if (iType == 619)
        //        return (ushort)Role.Flags.ConquerItem.LeftWeapon;
        //    if (iType == 617)
        //        return (ushort)Role.Flags.ConquerItem.RightWeapon;
        //    if (iType == 148)
        //        return (ushort)Role.Flags.ConquerItem.Head;
        //    if (iType == 614)
        //        return (ushort)Role.Flags.ConquerItem.RightWeapon;
        //    if (iType == 615 || iType == 616)
        //        return (ushort)Role.Flags.ConquerItem.RightWeapon;
        //    if (iType >= 111 && iType <= 118 || iType == 123 || iType >= 141 && iType <= 144 || iType == 145 || iType == 170)
        //        return (ushort)Role.Flags.ConquerItem.Head;
        //    else if (iType >= 120 && iType <= 121)
        //        return (ushort)Role.Flags.ConquerItem.Necklace;
        //    else if ((iType >= 130 && iType <= 139) || (iType == 101))
        //        return (ushort)Role.Flags.ConquerItem.Armor;
        //    else if (iType >= 150 && iType <= 152)
        //        return (ushort)Role.Flags.ConquerItem.Ring;
        //    else if (iType == 160)
        //        return (ushort)Role.Flags.ConquerItem.Boots;
        //    else if (iType >= 181 && iType <= 199)
        //        return (ushort)Role.Flags.ConquerItem.Garment;
        //    else if (iType == 201)
        //        return (ushort)Role.Flags.ConquerItem.Fan;
        //    else if (iType == 202)
        //        return (ushort)Role.Flags.ConquerItem.Tower;
        //    else if (iType == 203)
        //        return (ushort)Role.Flags.ConquerItem.RidingCrop;
        //    else if (iType == 200)
        //        return (ushort)Role.Flags.ConquerItem.SteedMount;
        //    else if (iType == 300)
        //        return (ushort)Role.Flags.ConquerItem.Steed;
        //    else if (iType == 2100)
        //        return (ushort)Role.Flags.ConquerItem.Bottle;
        //    else if (iType == 1050 || iType == 900)
        //        return (ushort)Role.Flags.ConquerItem.LeftWeapon;
        //    else if ((iType >= 410 && iType <= 490) || (iType >= 500 && iType <= 580) || (iType >= 601 && iType <= 613))
        //        return (ushort)Role.Flags.ConquerItem.RightWeapon;
        //    else if (iType >= 350 && iType <= 370)
        //        return (ushort)Role.Flags.ConquerItem.RightWeaponAccessory;
        //    else if (iType >= 723 && iType <= 726)
        //        return (ushort)Role.Flags.ConquerItem.RightWeaponAccessory;
        //    else if (iType == 380)
        //        return (ushort)Role.Flags.ConquerItem.LeftWeaponAccessory;
        //    else return 0;
        //}
        public static ushort ItemPosition(uint ID)
        {
            UInt32 iType = ID / 1000;

            if (ID >= 203003 && ID <= 203009)
                return (ushort)Role.Flags.ConquerItem.RidingCrop;
            if (ID >= 200000 && ID <= 200614)
                return (ushort)Role.Flags.ConquerItem.SteedMount;


            if (ID >= 350001 && ID <= 370050)
                return (ushort)Role.Flags.ConquerItem.RightWeaponAccessory;
            if (ID >= 380001 && ID <= 380056)//380046 
                return (ushort)Role.Flags.ConquerItem.LeftWeaponAccessory;

            //if (ID == 134155 || ID == 131155 || ID == 133155 || ID == 130155 || ID == 194865 || ID == 194875 || ID == 195210)
            //    return (ushort)Role.Flags.ConquerItem.Garment;

            if ((ID >= 111003 && ID <= 118309) || (ID >= 123000 && ID <= 123309) || (ID >= 141003 && ID <= 145309) || (ID >= 148000 && ID <= 148309)
              || ID >= 170000 && ID <= 170309)
                return (ushort)Role.Flags.ConquerItem.Head;

            else if (ID >= 120001 && ID <= 121269)
                return (ushort)Role.Flags.ConquerItem.Necklace;

            else if (ID >= 130003 && ID <= 139309 || ID >= 101000 && ID <= 101309)
                return (ushort)Role.Flags.ConquerItem.Armor;

            else if (ID >= 150000 && ID <= 152279)
                return (ushort)Role.Flags.ConquerItem.Ring;

            else if (ID >= 160013 && ID <= 160249)
                return (ushort)Role.Flags.ConquerItem.Boots;

            //else if (ID >= 181305 && ID <= 194300)
            //    return (ushort)Role.Flags.ConquerItem.Garment;
            //else if (ID >= 193015 && ID <= 194875)
            //    return (ushort)Role.Flags.ConquerItem.Garment;
            else if (iType >= 181 && iType <= 199)//193255
                return (ushort)Role.Flags.ConquerItem.Garment;
            else if (ID >= 201003 && ID <= 201009)
                return (ushort)Role.Flags.ConquerItem.Fan;

            else if (ID >= 202003 && ID <= 202009)
                return (ushort)Role.Flags.ConquerItem.Tower;

            else if (ID == 300000)
                return (ushort)Role.Flags.ConquerItem.Steed;

            else if (ID >= 410003 && ID <= 613429 || IsTrojanEpicWeapon(ID) || IsMonkEpicWeapon(ID) || IsNinjaEpicWeapon(ID) || (ID >= 617000 && ID <= 617439) || IsTaoistEpicWeapon(ID)
                || IsWarriorEpicWeapons(ID) || IsWindWalkerWeapon(ID))
                return (ushort)Role.Flags.ConquerItem.RightWeapon;

            else if ((ID >= 900000 && ID <= 900309) || (ID >= 1050000 && ID <= 1051000) || (ID >= 612000 && ID <= 612439)
                || (ID >= 612000 && ID <= 613429) || IsWarriorEpicWeapons(ID) || IsWindWalkerWeapon(ID)
                || (ID >= 611000 && ID <= 611439) || IsTrojanEpicWeapon(ID) || IsNinjaEpicWeapon(ID) || IsMonkEpicWeapon(ID) || (ID >= 617000 && ID <= 617439) || IsHossu(ID))
                return (ushort)Role.Flags.ConquerItem.LeftWeapon;

            else if (ID >= 2100025 && ID <= 2100125 || ID == 2100165 || ID == 2100175 ||
                ID == 2100185 || ID == 2100195 || ID == 2100205 || ID == 2100245)
                return (ushort)Role.Flags.ConquerItem.Bottle;


            return 0;
        }
        public static bool IsHossu(uint ID)
        {
            return ID >= 619000 && ID <= 619439;
        }
        public static bool IsTaoistEpicWeapon(uint ID)
        {
            return ID >= 620003 && ID <= 620439;
        }
        public static bool IsArrow(uint ID)
        {
            if (ID >= 1050000 && ID <= 1051000)
                return true;
            return false;
        }
        public static bool IsTwoHand(uint ID)
        {
            return ((ID.ToString()[0] == '5' || ID >= 421003 && ID <= 421439) ? true : false);
        }
        public static bool IsBow(uint ID)
        {
            return ID >= 500003 && ID <= 500429;
        }
        public static bool IsAccessory(uint ID)
        {
            return ID >= 350001 && ID <= 380015;
        }
        public static byte ItemMaxLevel(Role.Flags.ConquerItem Position)
        {
            switch (Position)
            {
                case 0: return 0;
                case Role.Flags.ConquerItem.Head: return 120;
                case Role.Flags.ConquerItem.Necklace: return 130;
                case Role.Flags.ConquerItem.Armor: return 120;
                case Role.Flags.ConquerItem.LeftWeapon: return 130;
                case Role.Flags.ConquerItem.RightWeapon: return 130;
                case Role.Flags.ConquerItem.Boots: return 129;
                case Role.Flags.ConquerItem.Ring: return 126;
                //case Role.Flags.ConquerItem.Tower: return 100;
                //case Role.Flags.ConquerItem.Fan: return 100;
                //case Role.Flags.ConquerItem.Steed: return 0;
                //case Role.Flags.ConquerItem.SteedMount: return 30;
            }
            return 0;
        }
        public static bool IsBacksword(uint ID)
        {
            return ID >= 421003 && ID <= 421439;
        }
        public static bool IsKatana(uint ID)
        {
            return ID >= 601000 && ID <= 601439;
        }
        public static bool IsMonkWeapon(uint ID)
        {
            return ID >= 610000 && ID <= 610439;
        }
        public static bool IsWeapon(uint ID)
        {
            return ID >= 601000 && ID <= 601439;
        }

        public static bool EquipPassStatsReq(DBItem baseInformation, Client.GameClient client)
        {
            if (client.Player.Strength >= baseInformation.Strength && client.Player.Agility >= baseInformation.Agility)
                return true;
            else
                return false;
        }
        public static bool EquipPassJobReq(DBItem baseInformation, Client.GameClient client)
        {
            switch (baseInformation.Class)
            {
                
                #region Trojan
                case 10: if (client.Player.Class <= 15 && client.Player.Class >= 10) return true; break;
                case 11: if (client.Player.Class <= 15 && client.Player.Class >= 11) return true; break;
                case 12: if (client.Player.Class <= 15 && client.Player.Class >= 12) return true; break;
                case 13: if (client.Player.Class <= 15 && client.Player.Class >= 13) return true; break;
                case 14: if (client.Player.Class <= 15 && client.Player.Class >= 14) return true; break;
                case 15: if (client.Player.Class == 15) return true; break;
                #endregion
                #region Warrior
                case 20: if (client.Player.Class <= 25 && client.Player.Class >= 20) return true; break;
                case 21: if (client.Player.Class <= 25 && client.Player.Class >= 21) return true; break;
                case 22: if (client.Player.Class <= 25 && client.Player.Class >= 22) return true; break;
                case 23: if (client.Player.Class <= 25 && client.Player.Class >= 23) return true; break;
                case 24: if (client.Player.Class <= 25 && client.Player.Class >= 24) return true; break;
                case 25: if (client.Player.Class == 25) return true; break;
                #endregion
                #region Archer
                case 40: if (client.Player.Class <= 45 && client.Player.Class >= 40) return true; break;
                case 41: if (client.Player.Class <= 45 && client.Player.Class >= 41) return true; break;
                case 42: if (client.Player.Class <= 45 && client.Player.Class >= 42) return true; break;
                case 43: if (client.Player.Class <= 45 && client.Player.Class >= 43) return true; break;
                case 44: if (client.Player.Class <= 45 && client.Player.Class >= 44) return true; break;
                case 45: if (client.Player.Class == 45) return true; break;
                #endregion
                
                
                #region Taoist
                case 190: if (client.Player.Class >= 100) return true; break;
                #endregion
                
                
                case 0: return true;
                default: return false;
            }
            return false;
        }
        public static byte GetNextRefineryItem()
        {
            if (Extensions.BaseFunc.RandGet(100, false) < 30)
                return 2;
            if (Extensions.BaseFunc.RandGet(100, false) < 30)
                return 1;
            return 0;
        }

        public static byte GetLevel(uint ID)
        {
            if (ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Armor || ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Head || IsShield(ID))
                return (byte)((ID % 100) / 10);
            else
                return (byte)((ID % 1000) / 10);
        }

        public static int GetUpEpLevelInfo(uint ID)
        {
            int cost = 0;
            int nLev = GetLevel(ID);

            if (ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Armor || ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Head || IsShield(ID))
            {
                switch (nLev)
                {
                    case 5: cost = 50; break;
                    case 6: cost = 40; break;
                    case 7: cost = 30; break;
                    case 8:
                    case 9: cost = 20; break;
                    default: cost = 500; break;
                }

                int nQuality = (int)(ID % 10);
                switch (nQuality)
                {
                    case 6: cost = cost * 90 / 100; break;
                    case 7: cost = cost * 70 / 100; break;
                    case 8: cost = cost * 30 / 100; break;
                    case 9: cost = cost * 10 / 100; break;
                    default:
                        break;
                }
            }
            else
            {
                switch (nLev)
                {
                    case 11: cost = 95; break;
                    case 12: cost = 90; break;
                    case 13: cost = 85; break;
                    case 14: cost = 80; break;
                    case 15: cost = 75; break;
                    case 16: cost = 70; break;
                    case 17: cost = 65; break;
                    case 18: cost = 60; break;
                    case 19: cost = 55; break;
                    case 20: cost = 50; break;
                    case 21: cost = 45; break;
                    case 22: cost = 40; break;
                    default:
                        cost = 500;
                        break;
                }

                int nQuality = (int)(ID % 10);
                switch (nQuality)
                {
                    case 6: cost = cost * 90 / 100; break;
                    case 7: cost = cost * 70 / 100; break;
                    case 8: cost = cost * 30 / 100; break;
                    case 9: cost = cost * 10 / 100; break;
                    default:
                        break;
                }
            }
            return (100 / cost + 1) * 12 / 10;
        }
        public static int GetUpEpQualityInfo(uint ID)
        {
            var item = Server.ItemsBase[ID];
            int change = 100;
            switch (ID % 10)
            {
                case 6: change = 50; break;
                case 7: change = 33; break;
                case 8: change = 20; break;
                default: change = 100; break;
            }
            int level = item.Level;
            if (level > 70)
                change = (int)(change * (100 - (level - 70) * 1.0) / 100);

            return Math.Max(1, 100 / change);
        }
        public static bool UpQualityDB(uint ID, uint DBs)
        {
            int cost = GetUpEpQualityInfo(ID);
            if (DBs >= cost)
                return true;
            else
            {
                double percent = 100 / cost;
                double MyCost = DBs * percent;
                return Extensions.BaseFunc.RandGet(100, true) < MyCost;
            }
        }
        public static bool UpItemMeteors(uint ID, uint Meteors)
        {
            int CompleteCost = GetUpEpLevelInfo(ID);
            if (Meteors >= CompleteCost)
                return true;
            else
            {
                double percent = 100 / CompleteCost;
                double MyCost = Meteors * percent;
                return Extensions.BaseFunc.RandGet(100, true) < MyCost;
            }
        }
        public static int RateSuccForQuality(uint wQuality)
        {
            if (wQuality <= 6) return 30;
            else if (wQuality == 7) return 12;
            else if (wQuality == 8) return 6;//6
            else if (wQuality == 9) return 5;//4

            return 0;
        }

        public static bool EquipPassSexReq(DBItem baseInformation, Client.GameClient client)
        {
            int ClientGender = client.Player.Body % 10000 < 1005 ? 1 : 2;
            if (baseInformation.Gender == 2 && ClientGender == 2)
                return true;
            if (baseInformation.Gender != 2)
                return true;
            return false;
        }
        public static bool EquipPassRbReq(DBItem baseInformation, Client.GameClient client)
        {
            if (baseInformation.Level < 71 && client.Player.Reborn > 0 && client.Player.Level >= 70)
                return true;
            else
                return false;
        }
        public static bool EquipPassLvlReq(DBItem baseInformation, Client.GameClient client)
        {
            if (client.Player.Level < baseInformation.Level)
                return false;
            else
                return true;
        }
        public static bool Equipable(Game.MsgServer.MsgGameItem item, Client.GameClient client)
        {
            DBItem BaseInformation = null;
            if (Server.ItemsBase.TryGetValue((uint)item.ITEM_ID, out BaseInformation))
            {
                bool pass = false;
                if (!EquipPassSexReq(BaseInformation, client))
                    return false;
                if (EquipPassRbReq(BaseInformation, client))
                    pass = true;
                else
                    if (EquipPassJobReq(BaseInformation, client) && EquipPassStatsReq(BaseInformation, client) && EquipPassLvlReq(BaseInformation, client))
                    pass = true;
                if (!pass)
                    return false;

                if (client.Player.Reborn > 0)
                {
                    if (client.Player.Level >= 70 && BaseInformation.Level <= 70)
                        return pass;
                }
                return pass;
            }
            return false;
        }
        //public static bool Equipable(uint ItemID, Client.GameClient client)
        //{
        //    DBItem BaseInformation = null;
        //    if (Server.ItemsBase.TryGetValue(ItemID, out BaseInformation))
        //    {
        //        bool pass = false;

        //        if (EquipPassRbReq(BaseInformation, client))
        //            pass = true;
        //        else
        //            if (EquipPassJobReq(BaseInformation, client) && EquipPassLvlReq(BaseInformation, client))
        //            pass = true;
        //        if (!pass)
        //            return false;

        //        if (client.Player.Reborn > 0)
        //        {
        //            if (client.Player.Level >= 70 && BaseInformation.Level <= 70)
        //                return pass;
        //        }
        //        return pass;
        //    }
        //    return false;
        //}
        public static bool Equipable(uint ItemID, Client.GameClient client, bool UpgradeByPass = false)
        {
            DBItem BaseInformation = null;
            if (Server.ItemsBase.TryGetValue(ItemID, out BaseInformation))
            {
                bool pass = false;

                if (EquipPassRbReq(BaseInformation, client))
                    pass = true;
                else if (UpgradeByPass)
                    if ((EquipPassJobReq(BaseInformation, client) || UpgradeByPass) && EquipPassLvlReq(BaseInformation, client))
                        pass = true;
                if (!pass)
                    return false;

                if (client.Player.Reborn > 0)
                {
                    if (client.Player.Level >= 70 && BaseInformation.Level <= 70)
                        return pass;
                }
                return pass;
            }
            return false;
        }
        public static uint GetBaseID(uint ID)
        {
            int itemtype = (int)(ID / 1000);
            if (ID == 300000)//steed
                return ID;
            switch ((byte)(ID / 10000))
            {
                case 20://tower/ Fan / RidingCrop
                case 14://hood`s. cap`s . band`s
                case 11://helment / hat / cap...
                case 90://shields
                case 13://armors
                case 12://necklace / bag / hood
                case 15://ring
                case 16://boots
                case 42:/*BackswordID*/
                    if (itemtype == 420)//Normal Sword
                        goto default;
                    ID = (uint)(ID - (ID % 10));
                    break;
                case 60:/*NinjaSwordID*/
                case 61:/*AssasinKnifeID*/
                case 62://epic tao backsword
                    ID = (uint)(ID - (ID % 10));
                    break;
                default://someting weapon`s / someting coronet`s earing`s / bow`s
                    {
                        byte def_val = (byte)(ID / 100000);
                        ID = (uint)(((def_val * 100000) + (def_val * 10000) + (def_val * 1000)) + ((ID % 1000) - (ID % 10)));
                        break;
                    }
            }
            return ID;
        }
        public class ITPlus
        {
            public uint ID;
            public byte Plus;
            public ushort ItemHP;
            public uint MinAttack;
            public uint MaxAttack;
            public ushort PhysicalDefence;
            public ushort MagicAttack;
            public ushort MagicDefence;
            public ushort Agility;
            public ushort Vigor { get { return Agility; } }
            public byte Dodge;
            public ushort SpeedPlus { get { return Dodge; } }
            public void Parse(string Line)
            {
                string[] Info = Line.Split(' ');
                ID = uint.Parse(Info[0]);
                Plus = byte.Parse(Info[1]);
                ItemHP = ushort.Parse(Info[2]);
                MinAttack = uint.Parse(Info[3]);
                MaxAttack = uint.Parse(Info[4]);
                PhysicalDefence = ushort.Parse(Info[5]);
                MagicAttack = ushort.Parse(Info[6]);
                MagicDefence = ushort.Parse(Info[7]);
                Agility = ushort.Parse(Info[8]);
                Dodge = byte.Parse(Info[9]);

            }
        }
        public class DBItem
        {
            public uint ID;
            public ITPlus[] Plus;
            public bool AllowUpgradePlus = false;
            public string Name;
            public byte Class;
            public byte Proficiency;
            public byte Level;
            public byte Gender;
            public ushort Strength;
            public ushort Agility;
            public uint GoldWorth;
            public ushort MinAttack;
            public ushort MaxAttack;
            public ushort PhysicalDefence;
            public ushort MagicDefence;
            public ushort MagicAttack;
            public byte Dodge;
            public ushort Frequency;
            public uint ConquerPointsWorth;
            public ushort Durability;
            public ushort StackSize;
            public ushort ItemHP;
            public ushort ItemMP;
            public ushort AttackRange;
            public ItemType Type;
            public string Description;
            public string LongDescription;
            public int GradeKey;

            public uint Crytical;
            public uint SCrytical;
            public uint Imunity;
            public uint Penetration;
            public uint Block;
            public uint BreackTrough;
            public uint ConterAction;
            public uint Detoxication;

            public uint MetalResistance = 0;
            public uint WoodResistance = 0;
            public uint WaterResistance = 0;
            public uint FireResistance = 0;
            public uint EarthResistance = 0;

            public ushort PurificationLevel;
            public ushort PurificationMeteorNeed;

            public uint Time;

            public void Parse(string Line)
            {
                Plus = new ITPlus[13];
                //string[] data = Line.Split(' ')(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
                string[] data = Line.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
                if (data[2] == "")
                {
                    for (int x = 2; x < data.Length - 1; x++)
                        data[x] = data[x + 1];
                }
                try
                {
                    if (data.Length > 52 && data[0] != "\0")
                    {
                        ID = Convert.ToUInt32(data[0]);

                        Name = data[1].Trim();
                        Class = Convert.ToByte(data[2]);
                        Proficiency = Convert.ToByte(data[3]);
                        Level = Convert.ToByte(data[4]);
                        Gender = Convert.ToByte(data[5]);
                        Strength = Convert.ToUInt16(data[6]);
                        Agility = Convert.ToUInt16(data[7]);
                        Type = Convert.ToUInt32(data[10]) == 0 ? ItemType.Dropable : ItemType.Others;
                        GoldWorth = Convert.ToUInt32(data[12]);
                        MaxAttack = Convert.ToUInt16(data[14]);
                        MinAttack = Convert.ToUInt16(data[15]);
                        PhysicalDefence = Convert.ToUInt16(data[16]);
                        Frequency = Convert.ToUInt16(data[17]);
                        Dodge = Convert.ToByte(data[18]);
                        ItemHP = Convert.ToUInt16(data[19]);
                        ItemMP = Convert.ToUInt16(data[20]);
                        Durability = Convert.ToUInt16(data[22]);
                        MagicAttack = Convert.ToUInt16(data[30]);
                        MagicDefence = Convert.ToUInt16(data[31]);
                        AttackRange = Convert.ToUInt16(data[32]);
                        ConquerPointsWorth = Convert.ToUInt32(data[37]);

                        Time = Convert.ToUInt32(data[39]);
                        /*if (Time > 0)
                            MyConsole.WriteLine(data[0]);*/
                        // if (ID == 117309)
                        //     return;

                        Crytical = Convert.ToUInt32(data[40]);
                        SCrytical = Convert.ToUInt32(data[41]);
                        Imunity = Convert.ToUInt32(data[42]);
                        Penetration = Convert.ToUInt32(data[43]);
                        Block = Convert.ToUInt32(data[44]);
                        BreackTrough = Convert.ToUInt32(data[45]);
                        ConterAction = Convert.ToUInt32(data[46]);
                        Detoxication = Convert.ToUInt32(data[47]);

                        MetalResistance = Convert.ToByte(data[48]);
                        WoodResistance = Convert.ToByte(data[49]);
                        WaterResistance = Convert.ToByte(data[50]);
                        FireResistance = Convert.ToByte(data[51]);
                        EarthResistance = Convert.ToByte(data[52]);
                        //    StackSize = Convert.ToUInt16(data[47]);
                        ushort.TryParse(data[47].ToString(), out StackSize);


                        Description = data[53].Replace("`s", "");
                        if (Description == "NinjaKatana")
                            Description = "NinjaWeapon";
                        if (Description == "Earrings")
                            Description = "Earring";
                        if (Description == "Bow")
                            Description = "ArcherBow";
                        if (Description == "Backsword")
                            Description = "TaoistBackSword";
                        Description = Description.ToLower();
                        LongDescription = data[54];
                        if (ID >= 730001 && ID <= 730009)
                        {
                            Name = "(+" + (ID % 10).ToString() + ")Stone";
                        }

                        if (data.Length >= 58)
                        {
                            ushort.TryParse(data[56].ToString(), out PurificationLevel);
                            ushort.TryParse(data[57].ToString(), out PurificationMeteorNeed);

                            if (PurificationLevel != 0 && ID != 729305 && ID != 727465)
                            {
                                if (!PurificationItems.ContainsKey(PurificationLevel))
                                    PurificationItems.Add(PurificationLevel, new Dictionary<uint, DBItem>());

                                if (!PurificationItems[PurificationLevel].ContainsKey(ID))
                                    PurificationItems[PurificationLevel].Add(ID, this);
                            }
                        }
                        if (ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.LeftWeaponAccessory || ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.RightWeaponAccessory)
                        {
                            if (!Accessorys.ContainsKey(ID))
                                Accessorys.Add(ID, this);
                            StackSize = 1;
                        }
                        if (ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.SteedMount)
                            if (!SteedMounts.ContainsKey(ID))
                                SteedMounts.Add(ID, this);
                        if (ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Garment)
                            if (!Garments.ContainsKey(ID))
                                Garments.Add(ID, this);
                        if (ID == 754099)
                            ConquerPointsWorth = 299;
                        if (ID == 754999 || ID == 753999 || ID == 751999)
                            ConquerPointsWorth = 1699;

                        if (ID == 619028)
                            ConquerPointsWorth = 489;

                        if (ID == 723723)
                            ConquerPointsWorth = 7100;
                        if (ID == 3005945)
                            ConquerPointsWorth = 3100;
                    }
                    else
                        MyConsole.WriteLine(data[0]);
                }
                catch (Exception e) { MyConsole.WriteLine(e.ToString()); }
            }
            public enum ItemType : byte
            {
                Dropable = 0,
                Others
            }
        }
    }
}
