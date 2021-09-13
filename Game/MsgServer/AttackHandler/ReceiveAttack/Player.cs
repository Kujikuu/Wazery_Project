using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.AttackHandler.ReceiveAttack
{
    public class Player
    {
        public static void Execute(MsgSpellAnimation.SpellObj obj, Client.GameClient client, Role.Player attacked)
        {

            if (attacked.Owner.PerfectionStatus.MirrorOfSin > 0)
            {
                if (AttackHandler.Calculate.Base.Rate(attacked.Owner.PerfectionStatus.MirrorOfSin))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        attacked.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                        {
                            Effect = MsgRefineEffect.RefineEffects.MirrorOfSin,
                            Id = attacked.UID,
                            dwParam = attacked.UID
                        }), true);
                        if (attacked.OnXPSkill() == MsgUpdate.Flags.Normal && !client.Player.ContainFlag(MsgUpdate.Flags.XPList))
                            attacked.AddFlag(MsgUpdate.Flags.XPList, 20, true);
                    }
                }
            }
            if (attacked.Owner.PerfectionStatus.LightOfStamina > 0)
            {
                if (attacked.Owner.PrestigeLevel >= client.PrestigeLevel)
                {
                    if (AttackHandler.Calculate.Base.Rate(attacked.Owner.PerfectionStatus.LightOfStamina))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            attacked.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                            {
                                Effect = MsgRefineEffect.RefineEffects.LightOfStamina,
                                Id = attacked.UID,
                                dwParam = attacked.UID
                            }), true);
                            if (attacked.Stamina < 100)
                            {
                                attacked.Stamina = 100;
                                attacked.SendUpdate(stream, attacked.Stamina, MsgUpdate.DataType.Stamina);
                            }
                        }
                    }
                }
            }

            if (attacked.Owner.PerfectionStatus.BloodSpawn > 0)
            {
                if (AttackHandler.Calculate.Base.Rate(attacked.Owner.PerfectionStatus.BloodSpawn))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        attacked.View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                        {
                            Effect = MsgRefineEffect.RefineEffects.BloodSpawn,
                            Id = attacked.UID,
                            dwParam = attacked.UID
                        }), true);
                        bool update = false;
                        if (attacked.HitPoints < attacked.Owner.Status.MaxHitpoints)
                        {
                            update = true;
                            attacked.HitPoints = (int)attacked.Owner.Status.MaxHitpoints;
                        }
                        if (attacked.Mana < attacked.Owner.Status.MaxMana)
                        {
                            update = true;
                            attacked.Mana = (ushort)attacked.Owner.Status.MaxMana;
                        }
                        if (update)
                        {
                            attacked.SendUpdateHP();
                            attacked.SendUpdate(stream, attacked.Mana, MsgUpdate.DataType.Mana, false);
                        }
                    }
                }
            }
            if (Calculate.Base.Rate(10))
            {
                CheckAttack.CheckItems.RespouseDurability(client);
            }
            if (client.EventBase != null)
                if (client.EventBase.NoDamage && client.EventBase.Stage == MsgEvents.EventStage.Fighting)
                    obj.Damage = client.EventBase.GetDamage(client, attacked.Owner);
            if (client.EventBase != null)
                if (client.EventBase.Stage != MsgEvents.EventStage.Fighting)
                    obj.Damage = 0;
            if (client.Arena != null)
                if (client.Player.Map == client.Arena.MapID)
                    obj.Damage = client.Arena.GetDamage(client, attacked.Owner);
            ushort X = attacked.X;
            ushort Y = attacked.Y;


            if (attacked.HitPoints <= obj.Damage)
            {
                if (client.Player.OnTransform)
                {
                    client.Player.TransformInfo.FinishTransform();
                }
                attacked.Dead(client.Player, X, Y, 0);

            }
            else
            {
                CheckAttack.CheckGemEffects.CheckRespouseDamage(attacked.Owner);
                client.UpdateQualifier(client, attacked.Owner, obj.Damage);
                attacked.HitPoints -= (int)obj.Damage;
                if (client.EventBase != null)
                    if (client.EventBase.Stage == MsgEvents.EventStage.Fighting)
                        client.EventBase.Hit(client, attacked.Owner);
                if (client.Arena != null)
                    if (client.Player.Map == client.Arena.MapID)
                        client.Arena.Hit(client, attacked.Owner);
            }


        }
    }
}
