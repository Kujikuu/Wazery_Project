using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.Game.MsgTournaments
{
    public class MsgBettingCompetition : ITournament
    {

        public static uint RewardConquerPoints = 0, MinimumBet = 5000;
        public ProcesType Process { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        public uint Seconds = 60;
        public Role.GameMap Map;
        public uint DinamicMap = 0;
        public KillerSystem KillSystem;
        public TournamentType Type { get; set; }
        public MsgBettingCompetition(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
        }

        public void Open()
        {
            if (Process == ProcesType.Dead)
            {
                RewardConquerPoints = 0;
                KillSystem = new KillerSystem();
                StartTimer = DateTime.Now;

                MsgSchedules.SendInvitation("[Betting Tournament]", "All the betted CPs", 453, 353, 1002, 0, 60);

                if (Map == null)
                {
                    Map = Database.Server.ServerMaps[700];
                    DinamicMap = Map.GenerateDynamicID();
                }
                InfoTimer = DateTime.Now;
                Seconds = 60;
                Process = ProcesType.Idle;
            }
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Idle)
            {
                if (user.Player.ConquerPoints >= MinimumBet)
                {
                    user.Player.ConquerPoints -= MinimumBet;
                    RewardConquerPoints += MinimumBet;
                    user.Player.MessageBox("You`ve betted " + MinimumBet + " ConquerPoints and current total bet is " + RewardConquerPoints, null, null);

                }
                else
                {
                    user.Player.MessageBox("Minimum bet is " + MinimumBet + " ConquerPoints and you dont have that amount.", null, null);
                    return false;
                }
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map.ID, DinamicMap);
                return true;
            }
            return false;
        }
        public void CheckUp()
        {
            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(1))
                {

                    MsgSchedules.SendSysMesage("[Betting Tournament] Event has started! May the best player win!", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);

                    Process = ProcesType.Alive;
                    StartTimer = DateTime.Now;
                }
                else if (DateTime.Now > InfoTimer.AddSeconds(10))
                {
                    Seconds -= 10;
                    MsgSchedules.SendSysMesage("[Betting Tournament] Event is about to start in " + Seconds.ToString() + " Seconds.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    InfoTimer = DateTime.Now;
                }
            }
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddMinutes(10))
                {
                    foreach (var user in MapPlayers())
                    {
                        user.Teleport(428, 378, 1002);
                    }
                    MsgSchedules.SendSysMesage("[Betting Tournament] has ended. All Players of [Betting Tournament] has teleported to TwinCity.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);

                    Process = ProcesType.Dead;
                }
                if (MapPlayers().Length == 1)
                {
                    var winner = MapPlayers().First();

                    MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won the [Betting Tournament] , he received " + RewardConquerPoints.ToString() + " ConquerPoints.", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                    winner.Player.ConquerPoints += RewardConquerPoints;
                    string reward = "[EVENT]" + winner.Player.Name + " has won and received " + RewardConquerPoints + " from [Betting Tournament].";
                    Program.DiscordEventsAPI.Enqueue($"``{reward}``");

                    Database.ServerDatabase.LoginQueue.Enqueue(reward);
                    winner.SendSysMesage("You received " + RewardConquerPoints.ToString() + " ConquerPoints. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                    winner.Teleport(428, 378, 1002, 0);

                    Process = ProcesType.Dead;
                }

                Extensions.Time32 Timer = Extensions.Time32.Now;
                foreach (var user in MapPlayers())
                {
                    if (user.Player.Alive == false)
                    {
                        if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                            user.Teleport(428, 378, 1002);
                    }
                }
            }


        }

        public Client.GameClient[] MapPlayers()
        {
            return Map.Values.Where(p => p.Player.DynamicID == DinamicMap && p.Player.Map == Map.ID).ToArray();
        }

        public bool InTournament(Client.GameClient user)
        {
            if (Map == null) return false;
            return user.Player.Map == Map.ID && user.Player.DynamicID == DinamicMap;
        }
    }
}
