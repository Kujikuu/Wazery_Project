using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CheckLineSpells
    {
        public static bool CheckUp(Client.GameClient user, ushort spellid)
        {
            if (user.EventBase != null)
            {
                if (!user.EventBase.MagicAllowed && user.EventBase?.Stage == MsgEvents.EventStage.Fighting)
                {
                    if (user.EventBase?.AllowedSkills != null)
                    {

                        if (user.EventBase?.AllowedSkills.Count > 0)
                        {
                            if (!user.EventBase.AllowedSkills.Contains(spellid))
                            {
                                user.SendSysMesage("This skill cannot be used in this event!");
                                return false;
                            }
                        }
                        else
                            return false;
                    }
                    else
                    {
                        user.SendSysMesage("This skill cannot be used in this event!");
                        return false;
                    }
                }
            }
            if (user.Arena != null)
            {
                if (user.Arena?.AllowedSkills != null)
                {

                    if (user.Arena?.AllowedSkills.Count > 0)
                    {
                        if (!user.Arena.AllowedSkills.Contains(spellid))
                        {
                            user.SendSysMesage("This skill cannot be used in this event!");
                            return false;
                        }
                    }
                    else
                        return false;
                }
                else
                {
                    user.SendSysMesage("This skill cannot be used in this event!");
                    return false;
                }
            }
            //if ((MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.FreezeWar
            //    //|| MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.SoulShackle
            //    //|| MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.FiveNOut
            //    //|| MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.KillTheCaptain
            //    //|| MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.RushTime
            //    || MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.TeamDeathMatch
            //    || MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.DragonWar)
            // && MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
            //{
            //    if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user) || UnlimitedArenaRooms.Maps.ContainsValue(user.Player.DynamicID))
            //    {
            //        if (spellid != 1045 && spellid != 1046 && spellid != 11005)
            //        {
            //            user.SendSysMesage("You have to use manual linear skills(FastBlade/ScentSword)");
            //            return false;
            //        }
            //    }
            //}
            return true;
        }
    }
}
