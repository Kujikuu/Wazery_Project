using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.AttackHandler
{
   public class Riding
    {
       public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
       {
           Database.MagicType.Magic DBSpell;
           MsgSpell ClientSpell;
           if (CheckAttack.CanUseSpell.Verified(Attack,user, DBSpells, out ClientSpell, out DBSpell))
           {
               if (user.Player.ContainFlag(MsgUpdate.Flags.Fly) || user.Player.OnTransform)
               {
#if Arabic
                     user.SendSysMesage("You can`t use this skill right now !");
#else
                   user.SendSysMesage("You can`t use this skill right now !");
#endif
                 
                   return;
               }
               if (user.Player.Map == 1038 || user.Player.Map == 700 ||  MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user))
               {
#if Arabic
                    user.SendSysMesage("You can't use this skill on this map.");
#else
                   user.SendSysMesage("You can't use this skill on this map.");
#endif
                  
                   return;
               }

               MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                                , 0, Attack.X, Attack.Y, ClientSpell.ID
                                , ClientSpell.Level, ClientSpell.UseSpellSoul);

               if (!user.Player.ContainFlag(MsgUpdate.Flags.Ride))
               {
                   user.Player.AddFlag(MsgUpdate.Flags.Ride, Role.StatusFlagsBigVector32.PermanentFlag, true, 1);

                   user.Vigor = user.Status.MaxVigor;

                   user.Send(stream.ServerInfoCreate(MsgServerInfo.Action.Vigor, user.Vigor));

               }
               else
                   user.Player.RemoveFlag(MsgUpdate.Flags.Ride);
               MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
               MsgSpell.SetStream(stream);
               MsgSpell.Send(user);

           }
       }
    }
}
