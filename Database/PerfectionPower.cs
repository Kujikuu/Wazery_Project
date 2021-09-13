using LightConquer_Project.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConquer_Project.Database
{
    public class PerfectionPower
    {
        public class PerfectionStats
        {
            public int Level;
            public int PAttack;
            public int PDefense;
            public int MAttack;
            public int MDefense;
            public int ToxinEraserLevel;
            public int StrikeLockLevel;
            public int LuckyStrike;
            public int CalmWind;
            public int DrainingTouch;
            public int BloodSpawn;
            public int LightOfStamina;
            public int ShiledBreak;
            public int KillingFlash;
            public int MirrorOfSin;
            public int DivineGuard;
            public int CoreStrike;
            public int InvisableArrow;
            public int FreeSoul;
            public int StraightLife;
            public int AbsoluteLuck;
        }
        public static Dictionary<int, PerfectionStats> Stats = new Dictionary<int, PerfectionStats>();
        public static void Load()
        {
            string[] TDs = System.IO.File.ReadAllLines(Program.ServerConfig.DbLocation + "item_refine_effect.txt");
            PerfectionStats T = new PerfectionStats();
            int count = 0;
            foreach (string Tinfo in TDs)
            {
                T = new PerfectionStats();
                string[] data = Tinfo.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);

                T.Level = Convert.ToInt32(data[0]);
                T.PAttack = Convert.ToInt32(data[1]);
                T.PDefense = Convert.ToInt32(data[2]);
                T.MAttack = Convert.ToInt32(data[3]);
                T.MDefense = Convert.ToInt32(data[4]);
                T.ToxinEraserLevel = Convert.ToInt32(data[5]) % 100;
                T.StrikeLockLevel = Convert.ToInt32(data[6]) % 100;
                T.LuckyStrike = Convert.ToInt32(data[7]) % 100;
                T.CalmWind = Convert.ToInt32(data[8]) % 100;
                T.DrainingTouch = Convert.ToInt32(data[9]) % 100;
                T.BloodSpawn = Convert.ToInt32(data[10]) % 100;
                T.LightOfStamina = Convert.ToInt32(data[11]) % 100;
                T.ShiledBreak = Convert.ToInt32(data[12]) % 100;
                T.KillingFlash = Convert.ToInt32(data[13]) % 100;
                T.MirrorOfSin = Convert.ToInt32(data[14]) % 100;
                T.DivineGuard = Convert.ToInt32(data[15]) % 100;
                T.CoreStrike = Convert.ToInt32(data[16]) % 100;
                T.InvisableArrow = Convert.ToInt32(data[17]) % 100;
                T.FreeSoul = Convert.ToInt32(data[18]) % 100;
                T.StraightLife = Convert.ToInt32(data[19]) % 100;
                T.AbsoluteLuck = Convert.ToInt32(data[20]) % 100;
                Stats.Add(T.Level, T);
                count++;
            }

        }
        internal static unsafe void CreateStatusAtributes(Client.GameClient Owner)
        {
            ushort PerfLevel;
            Database.PerfectionPower.PerfectionStats PerfSts;
            PerfLevel = (ushort)Owner.PrestigeLevel;
            if (PerfLevel > 0 && PerfLevel < 649)
            {
                PerfSts = Database.PerfectionPower.Stats[(int)PerfLevel];
                Owner.Status.MagicAttack += (uint)PerfSts.MAttack;
                Owner.Status.MagicDefence += (ushort)PerfSts.MDefense;
                Owner.Status.Defence += (ushort)PerfSts.PDefense;
                Owner.Status.MinAttack += (uint)PerfSts.PAttack;
                Owner.Status.MaxAttack += (uint)PerfSts.PAttack;
                Owner.Status.ToxinEraserLevel = PerfSts.ToxinEraserLevel * 2; //Convert.ToInt32(data[5]) % 100;
                Owner.Status.StrikeLockLevel = PerfSts.StrikeLockLevel * 2; //Convert.ToInt32(data[6]) % 100;
                Owner.Status.LuckyStrike = PerfSts.LuckyStrike * 2; //Convert.ToInt32(data[7]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.CalmWind = PerfSts.CalmWind * 2; //Convert.ToInt32(data[8]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.DrainingTouch = PerfSts.DrainingTouch * 2; //Convert.ToInt32(data[9]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.BloodSpawn = PerfSts.BloodSpawn * 2; //Convert.ToInt32(data[1PerfSts.PAttack]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.LightOfStamina = PerfSts.LightOfStamina * 2; //Convert.ToInt32(data[11]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.ShiledBreak = PerfSts.ShiledBreak * 2; //Convert.ToInt32(data[12]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.KillingFlash = PerfSts.KillingFlash * 2; //Convert.ToInt32(data[13]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.MirrorOfSin = PerfSts.MirrorOfSin * 2; //Convert.ToInt32(data[14]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.DivineGuard = PerfSts.DivineGuard * 2; //Convert.ToInt32(data[15]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.CoreStrike = PerfSts.CoreStrike * 2; //Convert.ToInt32(data[16]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.InvisableArrow = PerfSts.InvisableArrow * 2; //Convert.ToInt32(data[17]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.FreeSoul = PerfSts.FreeSoul * 2; //Convert.ToInt32(data[18]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.StraightLife = PerfSts.StraightLife * 2; //Convert.ToInt32(data[19]) % 1PerfSts.PAttackPerfSts.PAttack;
                Owner.Status.AbsoluteLuck = PerfSts.AbsoluteLuck * 2; //Convert.ToInt32(data[2PerfSts.PAttack]) % 1PerfSts.PAttackPerfSts.PAttack;*/
            }
            #region Items~Perfection
            MsgGameItem Bottle;
            if (Owner.Equipment.TryGetEquip(LightConquer_Project.Role.Flags.ConquerItem.Bottle, out Bottle))
            {
                switch (Bottle.ITEM_ID)
                {
                    case 2100245:// HolyGrail
                        {
                            if (PerfLevel >= 300)
                            {
                                Owner.Status.Defence += 300;
                            }
                            if (PerfLevel >= 648)
                            {
                                Owner.Status.MinAttack += 400;
                                Owner.Status.MaxAttack += 400;
                                Owner.Status.MaxHitpoints += 400;
                            }
                            
                            break;
                        }
                    case 2100075:// GoldPrize
                        {
                            if (PerfLevel >= 300)
                            {
                                Owner.Status.Defence += 300;
                            }
                            if (PerfLevel >= 648)
                            {
                                Owner.Status.MinAttack += 300;
                                Owner.Status.MaxAttack += 300;
                                Owner.Status.MaxHitpoints += 300;
                            }
                            break;
                        }
                    case 2100065:// SilverPrize
                        {
                            if (PerfLevel >= 648)
                            {
                                Owner.Status.MinAttack += 300;
                                Owner.Status.MaxAttack += 300;
                                Owner.Status.MaxHitpoints += 300;
                            }
                            break;
                        }
                    case 2100055:// Bronze~Prize
                        {                           
                            if (PerfLevel >= 648)
                            {
                                Owner.Status.MinAttack += 200;
                                Owner.Status.MaxAttack += 200;
                                Owner.Status.MaxHitpoints += 200;
                            }
                            break;
                        }                    
                }
            }
            #endregion
        }
    }
}
