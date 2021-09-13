using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.AttackHandler.CheckAttack
{
   public class CanAttackNpc
    {
       public static bool Verified(Client.GameClient client, Role.SobNpc attacked
    , Database.MagicType.Magic DBSpell)
       {
           
           if (attacked.HitPoints == 0)
               return false;
           if (attacked.IsStatue)
           {
               if (attacked.HitPoints == 0)
                   return false;
               if (client.Player.PkMode == Role.Flags.PKMode.PK)
                   return true;
               else
                   return false;
           }
           if (attacked.UID == 890)
           {
               if (client.Player.MyClan == null)
                   return false;
               var tournament = Game.MsgTournaments.MsgSchedules.ClanWar.CurentWar;
               if (tournament == null)
                   return false;

               if(!tournament.InWar(client))
                   return false;
               if (tournament.Winner == null)
                   return false;
               if ( tournament.Winner.ClainID == client.Player.ClanUID)
                   return false;
            
           }
           if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
           {
               if (client.Player.MyGuild == null)
                   return false;
               if (Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                   return false;
               if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.GuildWar.Winner.GuildID)
                   return false;
               if (Game.MsgTournaments.MsgSchedules.GuildWar.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.GuildWar.Proces == MsgTournaments.ProcesType.Idle)
                   return false;
           }
            if (attacked.UID == Game.MsgTournaments.MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
            {
                if (client.Player.MyGuild == null)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    return false;
                if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.EliteGuildWar.Winner.GuildID)
                    return false;
                if (Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces == MsgTournaments.ProcesType.Dead || Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces == MsgTournaments.ProcesType.Idle)
                    return false;
            }
           
            MsgTournaments.MsgCaptureTheFlag.Basse Bas;
           if (MsgTournaments.MsgSchedules.CaptureTheFlag.Bases.TryGetValue(attacked.UID, out Bas))
           {
               if (MsgTournaments.MsgSchedules.CaptureTheFlag.Proces != MsgTournaments.ProcesType.Alive)
                   return false;
               if (client.Player.MyGuild == null)
                   return false;
               if (Bas.Npc.HitPoints == 0)
                   return false;
               if (Bas.CapturerID == client.Player.GuildID)
                   return false;

           }
           return true;
       }
    }
}
