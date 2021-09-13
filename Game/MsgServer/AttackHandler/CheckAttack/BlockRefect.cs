using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.AttackHandler.CheckAttack
{
  public  class BlockRefect
    {
      public static bool CanUseReflect(Client.GameClient user)
      {
          if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.SkillTournament)
          {
              if (MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                  if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user))
                      return false;
          }
            else if (user.EventBase != null)
                return false;
            return true;


        }
    }
}
