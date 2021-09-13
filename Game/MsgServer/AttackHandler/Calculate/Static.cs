using LightConquer_Project.Game.MsgServer;
using LightConquer_Project.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConquer_Project.AttackHandler.Custom_Calculate
{
    public static class Static
    {

        public static Extensions.SafeRandom MyRandom = new Extensions.SafeRandom();
        public static bool Rate(int value)
        {

            return value > MyRandom.Next() % 100;
        }
        public static int GetCrit(Player attacker, IMapObj attacked, int Damage, out MsgSpellAnimation.SpellObj SpellObj)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None);
            #region CritcalStrike
            #region PlayerToMonster
            if (attacked is Game.MsgMonster.MonsterRole)
            {

                int Cirtical = (int)(attacker.Owner.Status.CriticalStrike / 100);
                if (Rate(Cirtical))
                {
                    SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;
                    return Damage = (int)(Damage * 1.5);
                }
            }
            #endregion
            #region PlayerToSob
            if (attacked is Role.SobNpc)
            {
                int critical = (int)(attacker.Owner.Status.CriticalStrike);
                if (Rate(critical))
                {
                    SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;
                    return Damage = (int)(Damage * 1.5);
                }
            }
            #endregion
            #region PlayerToPlayer
            else if (attacked is Player)
            {
                if (Core.Rate((attacker.Owner.Status.CriticalStrike - (attacked as Player).Owner.Status.Immunity) / 100) && (attacker.Owner.Status.CriticalStrike / 100) > 0)
                {
                    if (attacker.Owner.Status.CriticalStrike > (attacked as Player).Owner.Status.Immunity)
                    {
                        int rateCriticalStrike = (int)((attacker.Owner.Status.CriticalStrike - (attacked as Player).Owner.Status.Immunity) / 100);
                        if (Core.ChanceSuccess(rateCriticalStrike))
                        {
                            SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;
                            return Damage = (int)(Damage * 1.5);
                        }
                    }
                }
            }
            #endregion
            return Damage;
            #endregion
        }
        public static int GetSCrit(Player attacker, IMapObj attacked, int Damage, out MsgSpellAnimation.SpellObj SpellObj)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None);
            #region SkillCritcalStrike

            #region PlayerToMonster
            if (attacked is Game.MsgMonster.MonsterRole)
            {
                int SkillCirtical = (int)(attacker.Owner.Status.SkillCStrike / 100);
                if (Core.Rate(SkillCirtical))
                    if (attacker.Owner.Status.SkillCStrike > 0)
                    {
                        SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;
                        return Damage = (int)(Damage * 1.5);
                    }
            }
            #endregion

            #region PlayerToPlayer
            else if (attacked is Player)
            {
                if (Core.Rate((attacker.Owner.Status.SkillCStrike - (attacked as Player).Owner.Status.Immunity) / 100) && (attacker.Owner.Status.SkillCStrike / 100) > 0)
                {
                    if (attacker.Owner.Status.SkillCStrike > (attacked as Player).Owner.Status.Immunity)
                    {
                        SpellObj.Effect |= MsgAttackPacket.AttackEffect.CriticalStrike;
                        return Damage = (int)(Damage * 1.5);
                    }
                }
            }
            #endregion

            return Damage;
            #endregion
        }
        public static bool CanBreak(Player attacker, IMapObj Attacked, out MsgSpellAnimation.SpellObj SpellObj)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(Attacked.UID, 0, MsgAttackPacket.AttackEffect.None);
            #region BreakThroght
            if (Attacked is Player)
            {
                var attacked = Attacked as Player;
                int rateBreakthrough = (int)((attacker.Owner.Status.Breakthrough - (attacked.Owner.Status.Counteraction)) / 10);
                if (attacker.BattlePower < attacked.BattlePower)
                {
                    if (Core.ChanceSuccess(rateBreakthrough))
                    {
                        SpellObj.Effect |= MsgAttackPacket.AttackEffect.Break;
                        return true;

                    }
                }
            }
            return false;
            #endregion
        }
        public static int CalculateADDPotencyDamage(int AttackerBattlePower, int AttackedBattlePower, int damage)
        {
            int main = 0;

            if (AttackerBattlePower == AttackedBattlePower)
                return 0;
            if (AttackerBattlePower > AttackedBattlePower)
            {
                int power = (int)(AttackerBattlePower - AttackedBattlePower);

                if (power > 12)
                    power = 12;
                main += (damage * power) / 15;
            }
            return main;
        }
        public static double CalculateRemovePotencyDamage(int AttackerBattlePower, int AttackedBattlePower, double damage)
        {
            int main = AttackedBattlePower - AttackerBattlePower;
            if (AttackerBattlePower == AttackedBattlePower)
                return damage;
            if (AttackerBattlePower < AttackedBattlePower)
            {
                damage = damage / main;
                return damage;
            }
            return damage;
        }
        public static int CalculateRemovePotencyDamageBreak(int damage, int perc)
        {
            return damage * perc / 100;
        }
        public static int AdjustBlesseDefence(int nDef, uint vlaue)
        {
            var value = (vlaue);
            return (int)(nDef * value) / 100;
        }
        public static int StigAttack(Player attacker, int Damage)
        {
            int nAttack = 0;

            if (attacker.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Stigma) || attacker.OnAttackPotion)
            {
                nAttack += Game.MsgServer.AttackHandler.Calculate.Base.MulDiv(Damage, 130, 100) - Damage;
            }
            return Damage + nAttack;
        }
        public static int shiledDef(Player attacker, int Damage)
        {
            int ndef = 0;

            if (attacker.ContainFlag(Game.MsgServer.MsgUpdate.Flags.MagicShield) || attacker.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Shield) || attacker.OnDefensePotion)
            {
                ndef += Game.MsgServer.AttackHandler.Calculate.Base.MulDiv(Damage, 120, 100) - Damage;
            }
            return Damage - ndef;
        }
        public static short GetDistance(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            short x = 0;
            short y = 0;
            if (X >= X2) x = (short)(X - X2);
            else if (X2 >= X) x = (short)(X2 - X);
            if (Y >= Y2) y = (short)(Y - Y2);
            else if (Y2 >= Y) y = (short)(Y2 - Y);
            if (x > y) return x;
            else return y;
        }
        public static int ShieldDefence(Player Player, int nDef)
        {
            if (Player.ContainFlag(MsgUpdate.Flags.MagicShield))
            {
                var val = nDef;
                return (int)(val);

            }
            return (int)(nDef);
        }
    }
}
