using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.AttackHandler.Calculate
{
    public class Range 
    {
        public static void OnMonster(Role.Player player, MsgMonster.MonsterRole monster, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj, byte MultipleDamage = 0)
        {
           
            SpellObj = new MsgSpellAnimation.SpellObj(monster.UID, 0, MsgAttackPacket.AttackEffect.None);
            if (monster.IsFloor)
            {
                SpellObj.Damage = 1;
                return;
            }
            int Damage = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);

            Damage = (int)player.Owner.AjustAttack((uint)Damage);
            Damage = (int)player.Owner.AjustMaxAttack((uint)Damage);
            if (player.Level > monster.Level)
                Damage *= 2;
            if (MultipleDamage != 0)
            {
                Damage = Damage * MultipleDamage;
            }
            if (DBSpell != null)
            {
                if (DBSpell.ID == (ushort)Role.Flags.SpellID.TripleBlasts)
                {
                    Damage = Base.AdjustDataEx(Damage, DBSpell.DamageOnMonster, 100);
                  
                }
                else
                    Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage : Program.ServerConfig.PhysicalDamage), 100);
            }
            else
            {
                Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage : Program.ServerConfig.PhysicalDamage), 100);
                //  Damage = (int)Base.BigMulDiv((int)Damage, Client.GameClient.DefaultDefense2, player.Owner.GetDefense2());
            }
           
            var rawDefense = monster.Family.Defense;

            Damage = Math.Max(0, Damage - rawDefense);

            Damage = (int)Base.BigMulDiv(Damage, monster.Family.Defense2, Client.GameClient.DefaultDefense2);
            Damage = Base.MulDiv((int)Damage, (int)(100 - (int)(monster.Family.Dodge * 0.4)), 100); 

           // if (monster.Boss == 0)
            {
                Damage = Base.CalcDamageUser2Monster(Damage, monster.Family.Defense, player.Level, monster.Level, true);
                Damage = Base.AdjustMinDamageUser2Monster(Damage, player.Owner);

            }
           
                Damage = (int)Calculate.Base.CalculateExtraAttack((uint)Damage, player.Owner.Status.PhysicalDamageIncrease, 0);
                if (monster.Family.Defense2 == 0)
                    Damage = 1;
         

            SpellObj.Damage = (uint)Math.Max(1, Damage);
          //  MyConsole.WriteLine("My Range Damage 1 -> Monster " + SpellObj.Damage.ToString());
#if TEST
            MyConsole.WriteLine("My Range Damage -> Monster " + SpellObj.Damage.ToString());
#endif

            if (monster.Boss == 0)
            {
                if (player.ContainFlag(MsgUpdate.Flags.Superman))
                    SpellObj.Damage *= 10;
            }
            if (Base.Rate(player.Owner.PerfectionStatus.LuckyStrike))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    player.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                    {
                        Effect = MsgRefineEffect.RefineEffects.LuckyStrike,
                        Id = player.UID
                    }), true);
                }
                SpellObj.Effect = MsgAttackPacket.AttackEffect.LuckyStrike;
                SpellObj.Damage = (uint)Base.MulDiv((int)SpellObj.Damage, 200, 100);

            }
                if (Base.GetRefinery())
                {
                    if (player.Owner.Status.CriticalStrike > 0)
                    {
                        SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;

                        SpellObj.Damage += (SpellObj.Damage * (player.Owner.AjustCriticalStrike() / 100)) / 100;
                    }
                }
           
         //   MyConsole.WriteLine("My Range Damage 2 -> Monster " + SpellObj.Damage.ToString());
         //   MyConsole.WriteLine("My Range Damage 2 -> Monster " + SpellObj.Effect.ToString());
         //   SpellObj.Damage += player.Owner.Status.PhysicalDamageIncrease;
            if (monster.Family.ID == 20211)
                SpellObj.Damage = 1;
        }
        public static void OnPlayer(Role.Player player, Role.Player target, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj, int increasedmg = 0)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(target.UID, 0, MsgAttackPacket.AttackEffect.None);
            if (target.ContainFlag(MsgUpdate.Flags.ShurikenVortex) || target.ContainFlag(MsgUpdate.Flags.ManiacDance))
            {
                SpellObj.Damage = (uint)Calculate.Base.CalculateExtraAttack((uint)SpellObj.Damage, player.Owner.Status.PhysicalDamageIncrease, target.Owner.Status.PhysicalDamageDecrease);
                return;
            }
            if (DBSpell == null)
            {
                if (Base.Dodged(player.Owner, target.Owner))
                {
                    SpellObj.Damage = 0;
                    return;
                }
            }
            int Damage = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);
          

            Damage = (int)player.Owner.AjustAttack((uint)Damage);
            bool update = false;
            if (DBSpell != null)
            {
                if (DBSpell.ID == (ushort)Role.Flags.SpellID.BladeFlurry)
                {
                    if (Role.Core.GetDistance(player.X, player.Y, target.X, target.Y) < 3)
                    {
                        Damage = Base.MulDiv((int)Damage, 200, 100);
                        update = true;
                    }
                }
                else if (DBSpell.ID == (ushort)Role.Flags.SpellID.SwirlingStorm)
                {
                    Damage = Base.MulDiv((int)Damage, Program.GetRandom.Next(150, 330), 100);
                    update = true;
                }
                else if (DBSpell.ID == (ushort)Role.Flags.SpellID.ThundercloudAttack)
                {
                    Damage = Base.MulDiv((int)Damage, (int)(DBSpell.Damage + increasedmg), 100);
                    update = true;
                }
            }
            if (!update)
            {

                Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage : Program.ServerConfig.PhysicalDamage), 100);

            }
  

            if (player.Owner.GemValues(Role.Flags.Gem.NormalTortoiseGem) > 0)
                Damage = Base.MulDiv(Damage, (int)(100 - Math.Min(50, player.Owner.GemValues(Role.Flags.Gem.NormalTortoiseGem) * 2)), 100);

          
            if (target.Owner.GemValues(Role.Flags.Gem.NormalTortoiseGem) > 0)
            {
                int reduction = Base.MulDiv((int)target.Owner.GemValues(Role.Flags.Gem.NormalTortoiseGem), 50, 100);

                Damage = Base.MulDiv((int)Damage, (int)(100 - Math.Min(67, reduction)), 100);
            }


            Damage = Damage * (int)(110 - target.Owner.Status.Dodge) / 100;

            Damage = Base.MulDiv((int)Damage, 65, 100);
            Damage = (int)Base.BigMulDiv((int)Damage, player.Owner.GetDefense2(), Client.GameClient.DefaultDefense2);

            bool onbreak = false;
          
                if (player.Owner.Status.CriticalStrike > 0)
                {
                    if (Base.Rate(player.Owner.PerfectionStatus.LuckyStrike))
                    {
                        if (Base.Rate(target.Owner.PerfectionStatus.StrikeLock))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                target.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                                {
                                    Effect = MsgRefineEffect.RefineEffects.StrikeLockLevel,
                                    Id = player.UID
                                }), true);
                            }
                        }
                        else
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                target.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                                {
                                    Effect = MsgRefineEffect.RefineEffects.LuckyStrike,
                                    Id = player.UID
                                }), true);
                            }
                            SpellObj.Effect |= MsgAttackPacket.AttackEffect.LuckyStrike;
                            Damage = Base.MulDiv((int)Damage, 200, 100);
                        }
                    }
                    else if (Base.GetRefinery(player.Owner.Status.CriticalStrike / 100, target.Owner.Status.Immunity / 100))
                    {
                        SpellObj.Effect  |= MsgAttackPacket.AttackEffect.CriticalStrike;
                        Damage  = Base.MulDiv((int)Damage, 150, 100);
                    }
                }
       
                if (player.Owner.Status.Breakthrough > 0)
                {
                    if (Base.GetRefinery(player.Owner.Status.Breakthrough / 10, target.Owner.Status.Counteraction / 10))
                    {
                        onbreak = true;
                        SpellObj.Effect |= MsgAttackPacket.AttackEffect.Break;
                      //Damage = Base.MulDiv((int)Damage, 150, 100);
                    }
                }
        
            if (!onbreak && player.Owner.InSkillTeamPk() == false)
                Damage = Base.CalculatePotencyDamage(Damage, player.BattlePower, target.BattlePower, true);

            Damage = (int)Calculate.Base.CalculateExtraAttack((uint)Damage, player.Owner.Status.PhysicalDamageIncrease, target.Owner.Status.PhysicalDamageDecrease);



            SpellObj.Damage = (uint)Math.Max(1, Damage);
            //Console.WriteLine(Damage);
            if (target.ContainFlag(MsgUpdate.Flags.AzureShield))
            {
                if (SpellObj.Damage > target.AzureShieldDefence)
                {
                    Calculate.AzureShield.CreateDmg(player, target, target.AzureShieldDefence);
                    target.RemoveFlag(MsgUpdate.Flags.AzureShield);
                    SpellObj.Damage -= target.AzureShieldDefence;

                }
                else
                {
                    target.AzureShieldDefence -= (ushort)SpellObj.Damage;
                    Calculate.AzureShield.CreateDmg(player, target, SpellObj.Damage);
                    SpellObj.Damage = 1;
                }
            }

            if (target.ContainFlag(MsgUpdate.Flags.DefensiveStance))
            {
                SpellObj.Damage = Calculate.Base.CalculateBless(SpellObj.Damage, 40);
                SpellObj.Effect = MsgAttackPacket.AttackEffect.Block;
                return;
            }

            MsgSpellAnimation.SpellObj InRedirect;
            if (BackDmg.Calculate(player, target, DBSpell, SpellObj.Damage, out InRedirect))
                SpellObj = InRedirect;

            if (target.Owner.Equipment.ShieldID != 0)
            {
                int Block = (int)(target.Owner.Status.Block / 100);
                Block += (int)((target.ShieldBlockDamage * Block) / 100);
                uint Change = (uint)Math.Min(70, Block / 2);
                if (player.Owner.PerfectionStatus.ShieldBreak > 0)
                    if (Base.Rate(player.Owner.PerfectionStatus.ShieldBreak))
                    {
                        if (!target.ContainFlag(MsgUpdate.Flags.ShieldBreak))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                target.SendUpdate(stream, MsgUpdate.Flags.ShieldBreak, 15, 0, 0, MsgUpdate.DataType.AzureShield);
                            }
                            target.AddFlag(MsgUpdate.Flags.ShieldBreak, 15, true);
                        }
                    }
                if (target.ContainFlag(MsgUpdate.Flags.ShieldBreak))
                {
                    if (Change > 20)
                        Change -= 20;
                    else
                        Change = 0;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        player.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                        {
                            Effect = MsgRefineEffect.RefineEffects.ShiledBreak,
                            Id = player.UID,
                            dwParam = player.UID
                        }), true);
                    }
                }
                if (Base.Rate((byte)Change))
                {
                    SpellObj.Effect |= MsgAttackPacket.AttackEffect.Block;
                    SpellObj.Damage = 0;
                }
            }
            /*

            if (player.Owner.Status.PhysicalPercent > 0)
            {
                   Damage += (int)(Damage * player.Owner.Status.PhysicalPercent / 100);
             //   Damage = Base.MulDiv((int)Damage, (int)Math.Min(210, 100 + player.Owner.Status.GemPercents), 100) /3;
            }

            Damage = (int)player.Owner.AjustAttack((uint)Damage);

            Damage = (int)Base.BigMulDiv((int)Damage, Client.GameClient.DefaultDefense2, player.Owner.GetDefense2());
         
            Damage = Base.MulDiv((int)Damage, (int)(100 - Math.Min(65, target.Owner.Status.ItemBless)), 100);

            var rawDefense = target.Owner.RealDefense;
            var defense = Base.AdjustDefense((int)rawDefense, (int)rawDefense, 0);

            Damage = Math.Max(0, Damage - defense);


            if (defense <= rawDefense)
            {
                var reduce = 10;
                switch (target.Owner.Equipment.ArmorID % 10)
                {
                    case 9: reduce = 1000; break;
                    case 8: reduce = 500; break;
                    case 7: reduce = 200; break;
                    case 6: reduce = 100; break;
                }

                var n = Math.Max(1, player.Strength / reduce);
                if (n > Damage)
                {
                    Damage = n;
                }
            }
            Damage = (int)Base.BigMulDiv(Damage, target.Owner.GetDefense2(), Client.GameClient.DefaultDefense2);

            //calculate tortoise gem

            Damage = Base.MulDiv((int)Damage, (int)(100 - Math.Min(65, target.Owner.GemValues(Role.Flags.Gem.NormalTortoiseGem))), 100);

            /* var dodge = target.Owner.Status.Dodge - 50;
                if (dodge > 0)
                {
                    Damage -= Base.MulDiv(Damage, (int)dodge, 100);
                }
            

            var targetLevel = target.Level;
            if (targetLevel < 22) Damage = Base.MulDiv(Damage, 35, 100);
            else if (targetLevel < 50) Damage = Base.MulDiv(Damage, 25, 100);
            else if (targetLevel < 90) Damage = Base.MulDiv(Damage, 20, 100);
            else if (targetLevel < 110) Damage = Base.MulDiv(Damage, 15, 100);
            else Damage = Base.MulDiv(Damage, 12, 100);*

            // Damage = Base.CalculatePotencyDamage(Damage, player.BattlePower, target.BattlePower);

            // Damage = Base.MulDiv((int)Damage, (int)(100 - (int)(target.Owner.Status.Dodge * 0.8)), 100);
            Damage = Damage * (int)(110 - target.Owner.Status.Dodge) / 100;



            

            
#if TEST
            MyConsole.WriteLine("My Range Damage -> player " + SpellObj.Damage.ToString());
#endif
                bool onbreak = false;
            byte Rand = (byte)Base.MyRandom.Next(1, 10);
            if (Rand > 1 && Rand < 5)
            {
                if (player.Owner.Status.CriticalStrike > 0)
                {
                    bool striker;
                    if (Base.GetRefinery(player.Owner.Status.CriticalStrike, target.Owner.Status.Immunity,player.Owner.PerfectionStatus.LuckyStrike, out striker))
                    {
                        if (striker)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                target.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                                {
                                    Effect = MsgRefineEffect.RefineEffects.LuckyStrike,
                                    Id = player.UID
                                }), true);
                            }
                            SpellObj.Effect = MsgAttackPacket.AttackEffect.LuckyStrike;
                            Damage = Base.MulDiv((int)Damage, 200, 100);
                        }
                        if (Base.Rate(target.Owner.PerfectionStatus.StrikeLock))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                target.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                                      {
                                          Effect = MsgRefineEffect.RefineEffects.StrikeLockLevel,
                                          Id = player.UID
                                      }), true);
                            }
                        }
                        else
                        {
                            SpellObj.Effect = MsgAttackPacket.AttackEffect.CriticalStrike;
                            Damage = Base.MulDiv((int)Damage, 150, 100);
                        }
                    }
                }
            }
            else if (Rand > 5 && Rand < 9)
            {
                if (player.Owner.Status.Breakthrough > 0)
                {
                    if (Base.GetRefinery(player.Owner.Status.Breakthrough, target.Owner.Status.Counteraction))
                    {
                        onbreak = true;
                     //   SpellObj.Effect = MsgAttackPacket.AttackEffect.Break;
                   //     Damage = Base.MulDiv((int)Damage, 150, 100);
                    }
                }
            }
           if (!onbreak && player.Owner.InSkillTeamPk() == false)
            Damage = Base.CalculatePotencyDamage(Damage, player.BattlePower, target.BattlePower);

            //  Damage = Base.CalculateSoulsDamage(Damage, (int)player.Owner.Equipment.SoulsPotency, (int)target.Owner.Equipment.SoulsPotency);
            Damage = (int)Calculate.Base.CalculateExtraAttack((uint)Damage, player.Owner.Status.PhysicalDamageIncrease, target.Owner.Status.PhysicalDamageDecrease);

            SpellObj.Damage = (uint)Math.Max(1, Damage);
            if (target.ContainFlag(MsgUpdate.Flags.AzureShield))
            {
                if (SpellObj.Damage > target.AzureShieldDefence)
                {
                    Calculate.AzureShield.CreateDmg(player, target, target.AzureShieldDefence);
                    target.RemoveFlag(MsgUpdate.Flags.AzureShield);
                    SpellObj.Damage -= target.AzureShieldDefence;

                }
                else
                {
                    target.AzureShieldDefence -= (ushort)SpellObj.Damage;
                    Calculate.AzureShield.CreateDmg(player, target, SpellObj.Damage);
                    SpellObj.Damage = 1;
                }
            }

            if (target.ContainFlag(MsgUpdate.Flags.DefensiveStance))
            {
                SpellObj.Damage = Calculate.Base.CalculateBless(SpellObj.Damage, 40);
                SpellObj.Effect = MsgAttackPacket.AttackEffect.Block;
                return;
            }

            MsgSpellAnimation.SpellObj InRedirect;
            if (BackDmg.Calculate(player, target, DBSpell, SpellObj.Damage, out InRedirect))
                SpellObj = InRedirect;

            if (target.Owner.Equipment.ShieldID != 0)
            {
                int Block = (int)(target.Owner.Status.Block / 100);
                Block += (int)((target.ShieldBlockDamage * Block) / 100);
                uint Change = (uint)Math.Min(70, Block / 2);
                if (player.Owner.PerfectionStatus.ShieldBreak > 0)
                    if (Base.Rate(player.Owner.PerfectionStatus.ShieldBreak))
                    {
                        if (!target.ContainFlag(MsgUpdate.Flags.ShieldBreak))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                target.SendUpdate(stream, MsgUpdate.Flags.ShieldBreak, 15, 0, 0, MsgUpdate.DataType.AzureShield);
                            }
                            target.AddFlag(MsgUpdate.Flags.ShieldBreak, 15, true);
                        }
                    }
                if (target.ContainFlag(MsgUpdate.Flags.ShieldBreak))
                {
                    if (Change > 20)
                        Change -= 20;
                    else
                        Change = 0;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        player.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                        {
                            Effect = MsgRefineEffect.RefineEffects.ShiledBreak,
                            Id = player.UID,
                            dwParam = player.UID
                        }), true);
                    }
                }
                if (Base.Rate((byte)Change))
                {
                    SpellObj.Effect = MsgAttackPacket.AttackEffect.Block;
                    SpellObj.Damage = 0;
                }
            }*/
        }
        public static void OnNpcs(Role.Player player, Role.SobNpc target, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(target.UID, 0, MsgAttackPacket.AttackEffect.None);

            int Damage = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);

            Damage = (int)player.Owner.AjustAttack((uint)Damage);

            Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage : Program.ServerConfig.PhysicalDamage), 100);
            Damage = (int)Base.BigMulDiv((int)Damage, Client.GameClient.DefaultDefense2, player.Owner.GetDefense2());

       
            SpellObj.Damage = (uint)Math.Max(1, Damage);

            if (Base.GetRefinery())
            {
        
                    if (player.Owner.Status.CriticalStrike > 0)
                    {
                        SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;
                        SpellObj.Damage = Base.CalculateArtefactsDmg(SpellObj.Damage, player.Owner.Status.CriticalStrike, 0);
                    }
            
                 
                
            }
          
                SpellObj.Damage = Calculate.Base.CalculateExtraAttack((uint)SpellObj.Damage, player.Owner.Status.PhysicalDamageIncrease, 0);


            if (target.ContainFlag(MsgUpdate.Flags.AzureShield))
                SpellObj.Damage = 100;
           
        }
     
    }
}
