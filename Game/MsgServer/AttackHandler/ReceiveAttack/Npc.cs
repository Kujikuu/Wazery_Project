using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.AttackHandler.ReceiveAttack
{
    public class Npc
    {
        public static uint Execute(ServerSockets.Packet stream, MsgSpellAnimation.SpellObj obj, Client.GameClient client, Role.SobNpc attacked)
        {
            //if(MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.FootBall)
            //{
            //    if(MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
            //    {
            //        var tournament = (MsgTournaments.MsgFootball)MsgTournaments.MsgSchedules.CurrentTournament;
            //        tournament.RemoveNpc();
            //        client.Player.AddFlag(MsgServer.MsgUpdate.Flags.lianhuaran04, Role.StatusFlagsBigVector32.PermanentFlag, true);
            //        return 0;
            //    }
            //}
          
            if (obj.Damage >= attacked.HitPoints)
            {
                uint exp = (uint)attacked.HitPoints;
                attacked.Die(stream, client);
                if (attacked.Map == 1039)
                    return exp / 10;
            }
            else
            {
                attacked.HitPoints -= (int)obj.Damage;

                if (attacked.UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules.GuildWar.UpdateScore(client.Player, obj.Damage);
                if (attacked.UID == Game.MsgTournaments.MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].UID)
                    Game.MsgTournaments.MsgSchedules.EliteGuildWar.UpdateScore(client.Player, obj.Damage);

                if (Game.MsgTournaments.MsgSchedules.CaptureTheFlag.Bases.ContainsKey(attacked.UID))
                {
                    Game.MsgTournaments.MsgSchedules.CaptureTheFlag.UpdateFlagScore(client.Player, attacked, obj.Damage, stream);
                }
                
                if (attacked.Map == 1039 || attacked.Map == 1038)
                    return obj.Damage / 1000;
               
            }
            return 0;
        }
    }
}
