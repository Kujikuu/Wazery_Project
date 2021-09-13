using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgMonster
{
    public class MobRateWatcher
    {
        private int tick;
        private int count;
        public static implicit operator bool(MobRateWatcher q)
        {
            bool result = false;
            q.count++;
            if (q.count == q.tick)
            {
                q.count = 0;
                result = true;
            }
            return result;
        }
        public MobRateWatcher(int Tick)
        {
            tick = Tick;
            count = 0;
        }
    }

    public struct SpecialItemWatcher
    {
        public uint ID;
        public MobRateWatcher Rate;
        public SpecialItemWatcher(uint ID, int Tick)
        {
            this.ID = ID;
            Rate = new MobRateWatcher(Tick);
        }
    }

    public class MobItemGenerator
    {
        private static ushort[] NecklaceType = new ushort[] { 120/*Necklace*/, 121 /*Bag*/};
        private static ushort[] BootsType = new ushort[] { 160 /*Boots*/};
        private static ushort[] RingType = new ushort[] { 150/*ring*/, 151/*heavyring*/, 152 /*brack*/};
        private static ushort[] ArmetType = new ushort[] { 111/*Warrior`sHelmet*/, 113/*Archer`sHat*/, 114/*Taoist`sCap*/, 117/*Earring*/, 118/*Coronet*/ };
        private static ushort[] ArmorType = new ushort[] { 130/*Trojan`sArmor*/, 131/*Warrior`sArmor*/, 133/*Archer`sCoat*/, 134/*Taoist`sRobe*/};
        private static ushort[] OneHanderType = new ushort[] { 410/*blade*/, 420/*sword*/, 421/*BackSword*/, 430/*Hook*/, 440/*Whip*/, 450/*Axe*/, 460/*Hammer*/, 480/*Club*/, 481/*Scepter*/, 490/*Dagger*/ };
        private static ushort[] TwoHanderType = new ushort[] { 510/*Glaive*/, 530/*PoleAxe*/, 560/*Spear*/, 561/*Wand*/, 580/*Halbert*/ , 900/*Sheild*/, 500/*bow*/ };
        //private static uint[] SeaPotions = new uint[] { 3004230, 3004231, 3004232, 3004233, 3004234, 3004235, 3004236, 3004237, 3004238 };
        private MonsterFamily Family;

        private MobRateWatcher Refined;
        private MobRateWatcher Unique;
        private MobRateWatcher Elite;
        private MobRateWatcher Super;
        private MobRateWatcher PlusOne;
        private MobRateWatcher PlusTwo;

        private MobRateWatcher DropHp;
        private MobRateWatcher DropMp;

        //private MobRateWatcher Study20;
        private MobRateWatcher DragonBalls;
        //private MobRateWatcher Stone;
        private MobRateWatcher Meteor;
        private MobRateWatcher ExpBallEvent;
        //private MobRateWatcher MoonBox;

        //gems
        private MobRateWatcher PhoenixGem;//700001
        private MobRateWatcher DragonGem;//700011
        private MobRateWatcher FuryGem;//700021
        private MobRateWatcher RainbowGem;//700031
        private MobRateWatcher KylinGem;//700041
        private MobRateWatcher VioletGem;//700051
        private MobRateWatcher MoonGem;//700061
        //private MobRateWatcher TortoiseGem;//700071
        //cps
        //private MobRateWatcher ScarletCPBag;//
        //private MobRateWatcher GarnetCPBag; //
        //private MobRateWatcher SunCPBag; //
        //private MobRateWatcher RainbowCPBag;//

        private MobRateWatcher DropSpecialPotions;
        //private MobRateWatcher ChiPointPack20;
        // lotto tickets
        //private MobRateWatcher SmallLotteryTicketPack;
        //
        // TortoiseGemFragment
        //private MobRateWatcher TortoiseGemFragment;
        //
        // exchange items
        //private MobRateWatcher TCExchange;
        //private MobRateWatcher ACExchange;
        //private MobRateWatcher PCExchange;
        //private MobRateWatcher DCExchange;
        //private MobRateWatcher BIExchange;

        public MobItemGenerator(MonsterFamily family)
        {
            Family = family;
            Refined = new MobRateWatcher(10);//500 / Family.Level);//1000 / Family.Level);
            Unique = new MobRateWatcher(20);//2000 / Family.Level);//4000 / Family.Level);
            Elite = new MobRateWatcher(25);//4000 / Family.Level);//8000 / Family.Level);
            Super = new MobRateWatcher(30);//5000 / Family.Level);//10000 / Family.Level);
            PlusOne = new MobRateWatcher(11);//2000 / Family.Level);//3000 / Family.Level);
            PlusTwo = new MobRateWatcher(15);//4000 / Family.Level);//6000 / Family.Level);


            DropHp = new MobRateWatcher(50);
            DropMp = new MobRateWatcher(50);

            //MoonBox = new MobRateWatcher(700);
            //Study20 = new MobRateWatcher(70);
            DragonBalls = new MobRateWatcher(130);//5000 / Family.Level);
            //Stone = new MobRateWatcher(140);//5000 / Family.Level);
            ExpBallEvent = new MobRateWatcher(30);//25
            Meteor = new MobRateWatcher(75);//18

            //gems
            PhoenixGem = new MobRateWatcher(70);//2000 / Family.Level);//3000 / Family.Level);
            DragonGem = new MobRateWatcher(80);//2000 / Family.Level);//3000 / Family.Level);
            FuryGem = new MobRateWatcher(50);//2000 / Family.Level);//3000 / Family.Level);
            RainbowGem = new MobRateWatcher(51);//2000 / Family.Level);//3000 / Family.Level);
            KylinGem = new MobRateWatcher(52);//2000 / Family.Level);//3000 / Family.Level);
            VioletGem = new MobRateWatcher(53);//2000 / Family.Level);//3000 / Family.Level);
            MoonGem = new MobRateWatcher(54);//2000 / Family.Level);//3000 / Family.Level);
            //TortoiseGem = new MobRateWatcher(100);//2000 / Family.Level);//3000 / Family.Level);

            ////cps
            //ScarletCPBag = new MobRateWatcher(70);// 50 CPS
            //GarnetCPBag = new MobRateWatcher(100);//100 CPS
            //SunCPBag = new MobRateWatcher(125);//250 CPS
            //RainbowCPBag = new MobRateWatcher(175);//450 CPS
            //
            DropSpecialPotions = new MobRateWatcher(50);
            //ChiPointPack20 = new MobRateWatcher(215);
            //
            //TCExchange = new MobRateWatcher(105);
            //ACExchange = new MobRateWatcher(205);
            //PCExchange = new MobRateWatcher(305);
            //DCExchange = new MobRateWatcher(405);
            //BIExchange = new MobRateWatcher(505);
            // lotto ticket
            //SmallLotteryTicketPack = new MobRateWatcher(130);
            //
            // TortoiseGemFragment
            //TortoiseGemFragment = new MobRateWatcher(206);
            //
        }
        public uint GenerateItemCPID(uint map, out bool Special, out Database.ItemType.DBItem DbItem, out uint Value)
        //        public uint GenerateItemId(uint map, out byte dwItemQuality, out bool Special, out Database.ItemType.DBItem DbItem)

        {
            //if (ScarletCPBag)
            //{
            //    if (Database.Server.ItemsBase.TryGetValue(721057, out DbItem))
            //    {
            //        Value = 5;
            //        Special = true;
            //        return 721057;
            //    }
            //}
            //if (GarnetCPBag)
            //{
            //    if (Database.Server.ItemsBase.TryGetValue(721058, out DbItem))
            //    {
            //        Value = 10;
            //        Special = true;
            //        return 721058;
            //    }
            //}
            //if (SunCPBag)
            //{
            //    if (Database.Server.ItemsBase.TryGetValue(721059, out DbItem))
            //    {
            //        Value = 20;
            //        Special = true;
            //        return 721059;
            //    }
            //}
            //if (RainbowCPBag)
            //{
            //    if (Database.Server.ItemsBase.TryGetValue(721060, out DbItem))
            //    {
            //        Value = 30;
            //        Special = true;
            //        return 721060;
            //    }
            //}
            Value = 0;
            DbItem = null;
            Special = true;
            return 0;
        }

        //public uint GeneratePotionExtra(bool Special = false)
        //{
        //    //if (Special)
        //    //{
        //    //    return SeaPotions[Program.GetRandom.Next(0, SeaPotions.Length)];
        //    //}

        //    //if (DropSpecialPotions)
        //    //{
        //    //    return SeaPotions[Program.GetRandom.Next(0, SeaPotions.Length)];
        //    //}
        //    //return 0;
        //}

        public List<uint> GenerateSoulsItems(ushort level)
        {
            if (Role.Core.Rate(5))
            {
                if (level == 0)
                    level = 6;
            }
            else
            {
                if (level == 0)
                    level = (ushort)Role.Core.Random.Next(3, 5);
            }
            List<uint> items = new List<uint>();
            byte count = 1;
            if (Database.ItemType.PurificationItems.ContainsKey(level))
            {
                var array = Database.ItemType.PurificationItems[level].Values.ToArray();
                for (int x = 0; x < (int)(count == 0 ? 1 : count); x++)
                {
                    int position = Program.GetRandom.Next(0, array.Length);
                    items.Add(array[position].ID);
                }
            }
            if (level <= 3)
                items.Add(723341);//20 study points
            else if (level > 3)
                items.Add(723342);//500 study points

            return items;
        }
        public List<uint> GenerateRefineryItems(ushort level)
        {
            if (level == 0)
                level = (ushort)Role.Core.Random.Next(1, 5);
            if (Role.MyMath.Success(0.001))
                level = 5;
            List<uint> items = new List<uint>();
            byte count = 1;
            if (Database.ItemType.Refinary.ContainsKey(level))
            {
                var array = Database.ItemType.Refinary[level].Values.ToArray();
                for (int x = 0; x < (int)(count == 0 ? 1 : count); x++)
                {
                    int position = Program.GetRandom.Next(0, array.Length);
                    items.Add(array[position].ItemID);
                }
            }

            //genereate accessory
            var Accessorys = Database.ItemType.Accessorys.Values.ToArray();
            var rand = (ushort)(Program.GetRandom.Next() % 1000);
            count = (byte)(rand % 3);
            for (int x = 0; x < count; x++)
            {
                int position = Program.GetRandom.Next(0, Accessorys.Length);
                items.Add(Accessorys[position].ID);
            }

            //--------------------------

            if (level <= 3)
                items.Add(723341);//20 study points
            else if (level > 3)
                items.Add(723342);//500 study points

            return items;
        }
        
        public uint GenerateItemId(uint map, out byte dwItemQuality, out bool Special, out Database.ItemType.DBItem DbItem)
        {
            Special = false;
            foreach (SpecialItemWatcher sp in Family.DropSpecials)
            {
                if (sp.Rate)
                {
                    Special = true;
                    dwItemQuality = (byte)(sp.ID % 10);
                    if (Database.Server.ItemsBase.TryGetValue(sp.ID, out DbItem))
                        return sp.ID;
                }
            }

            if (DropHp)
            {
                dwItemQuality = 0;
                Special = false;
                if (Database.Server.ItemsBase.TryGetValue(Family.DropHPItem, out DbItem))
                    return Family.DropHPItem;
            }
            if (DropMp)
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(Family.DropMPItem, out DbItem))
                return Family.DropMPItem;
            }
            //gems
            if (PhoenixGem)//
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(700001, out DbItem))
                    return 700002;

            }
            if (DragonGem)//
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(700011, out DbItem))
                    return 700012;

            }
            if (FuryGem)//
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(700021, out DbItem))
                    return 700022;

            }
            if (RainbowGem)//
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(700031, out DbItem))
                    return 700032;

            }
            if (KylinGem)//
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(700041, out DbItem))
                    return 700042;

            }
            if (VioletGem)//
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(700051, out DbItem))
                    return 700052;

            }
            if (MoonGem)//
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(700061, out DbItem))
                    return 700062;

            }
            
            if (Meteor)
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(1088001, out DbItem))
                    return 1088001;
            }
            if (DragonBalls)
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(1088000, out DbItem))
                    return 1088000;
            }
            if (ExpBallEvent)
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(722136, out DbItem))
                    return 722136;
            }

            dwItemQuality = GenerateQuality();
            uint dwItemSort = 0;
            uint dwItemLev = 0;

            int nRand = Extensions.BaseFunc.RandGet(1200, false);
            if (nRand >= 0 && nRand < 20) // 0.17%
            {
                dwItemSort = 160;
                dwItemLev = Family.DropBoots;
            }
            else if (nRand >= 20 && nRand < 50) // 0.25%
            {
                dwItemSort = NecklaceType[Extensions.BaseFunc.RandGet(NecklaceType.Length, false)];
                dwItemLev = Family.DropNecklace;
            }
            else if (nRand >= 50 && nRand < 100) // 4.17%
            {
                dwItemSort = RingType[Extensions.BaseFunc.RandGet(RingType.Length, false)];
                dwItemLev = Family.DropRing;
            }
            else if (nRand >= 100 && nRand < 400) // 25%
            {
                dwItemSort = ArmetType[Extensions.BaseFunc.RandGet(ArmetType.Length, false)];
                dwItemLev = Family.DropArmet;
            }
            else if (nRand >= 400 && nRand < 700) // 25%
            {
                dwItemSort = ArmorType[Extensions.BaseFunc.RandGet(ArmorType.Length, false)];
                dwItemLev = Family.DropArmor;
            }
            else // 45%
            {
                int nRate = Extensions.BaseFunc.RandGet(100, false);
                if (nRate >= 0 && nRate < 20) // 20% of 45% (= 9%) - Backswords
                {
                    dwItemSort = 421;
                }
                else if (nRate >= 40 && nRate < 80)	// 40% of 45% (= 18%) - One handers
                {
                    dwItemSort = OneHanderType[Extensions.BaseFunc.RandGet(OneHanderType.Length, false)];
                    dwItemLev = Family.DropWeapon;
                }
                else if (nRand >= 80 && nRand < 100)// 20% of 45% (= 9%) - Two handers (and shield)
                {
                    dwItemSort = TwoHanderType[Extensions.BaseFunc.RandGet(TwoHanderType.Length, false)];
                    dwItemLev = ((dwItemSort == 900) ? Family.DropShield : Family.DropWeapon);
                }
            }
            /*if (dwItemLev == 99)
            {
               dwItemLev =0;
            }*/

            if (dwItemLev != 99)
            {
                dwItemLev = AlterItemLevel(dwItemLev, dwItemSort);

                uint idItemType = (dwItemSort * 1000) + (dwItemLev * 10) + dwItemQuality;
                // Database.ItemType.DBItem DbItem;
                if (Database.Server.ItemsBase.TryGetValue(idItemType, out DbItem))
                {
                    ushort position = Database.ItemType.ItemPosition(idItemType);
                    byte level = Database.ItemType.ItemMaxLevel((Role.Flags.ConquerItem)position);
                    if (DbItem.Level > level)
                        return 0;
                    return idItemType;
                }
            }
            DbItem = null;
            return 0;
        }
        public byte GeneratePurity()
        {
            if (PlusOne)
                return 1;
            if (PlusTwo)
                return 2;
            return 0;
        }
        public byte GenerateBless()
        {
            if (Role.MyMath.Success(0.10))
            {
                int selector =  Program.GetRandom.Next(0, 100);
                if (selector < 1)
                    return 5;
                else if (selector < 6)
                    return 3;
            }
            return 0;
        }
        public byte GenerateSocketCount(uint ItemID)
        {
            if (ItemID >= 410000 && ItemID <= 601999)
            {
                int nRate =  Program.GetRandom.Next(0, 1000) % 100;
                if (nRate < 5) // 5%
                    return 2;
                else if (nRate < 20) // 15%
                    return 1;
            }
            return 0;
        }
        private byte GenerateQuality()
        {
            if (Refined)
                return 6;
            else if (Unique)
                return 7;
            else if (Elite)
                return 8;
            else if (Super)
                return 9;
            return 3;
        }
        public uint GenerateGold(out uint ItemID, bool normal = false, bool twin = false)
        {
            uint amount = 0;
            if (twin)
            {
                amount = (uint)Program.GetRandom.Next(0, (int)(((100 * Family.Level / 2) / 2) + 1));
            }
            else
            {
                if (normal)
                    amount = (uint)Program.GetRandom.Next(Family.DropMoney, Family.DropMoney * 10);
                else
                {
                    amount = (uint)Program.GetRandom.Next(0, (int)(((1000 * Family.Level / 2) / 2) + 1));

                    amount /= 2;
                }
            }
            ItemID = Database.ItemType.MoneyItemID((uint)amount);
            return amount;
        }
        private uint AlterItemLevel(uint dwItemLev, uint dwItemSort)
        {
            int nRand = Extensions.BaseFunc.RandGet(100, true);

            if (nRand < 50) // 50% down one level
            {
                uint dwLev = dwItemLev;
                dwItemLev = (uint)(Extensions.BaseFunc.RandGet((int)(dwLev / 2 + dwLev / 3),false));

                if (dwItemLev > 1)
                    dwItemLev--;
            }
            else if (nRand > 80) // 20% up one level
            {
                if ((dwItemSort >= 110 && dwItemSort <= 114) ||
                    (dwItemSort >= 130 && dwItemSort <= 134) ||
                    (dwItemSort >= 900 && dwItemSort <= 999))
                {
                    dwItemLev = Math.Min(dwItemLev + 1, 9);
                }
                else
                {
                    dwItemLev = Math.Min(dwItemLev + 1, 23);
                }
            }

            return dwItemLev;
        }
    }
}
