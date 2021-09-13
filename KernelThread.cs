using LightConquer_Project.Game.MsgMonster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project
{
    public class KernelThread
    {
        public const int TournamentsStamp = 1000,
            ChatItemsStamp = 180000,
            RouletteStamp = 1000,
            TeamArena_CreateMatches = 900,
            TeamArena_VerifyMatches = 980,
            TeamArena_CheckGroups = 960,
            Arena_CreateMatches = 1100,
            Arena_VerifyMatches = 1200,
            Arena_CheckGroups = 1150,
            TeamPkStamp = 1000,
            ElitePkStamp = 1000,
            AccServerStamp = 3300,
            LoaderServerStamp = 3000,
            BroadCastStamp = 1000,
            ResetDayStamp = 6000,
            SaveDatabaseStamp = 180000,
            RespawnMapMobs = 500;

        //The Snow Banshee appeared in Frozen Grotto 2(540,430)! Defeat it!
        public static DateTime StartDate;
        public Extensions.Time32 UpdateServerStatus = Extensions.Time32.Now;
        public Extensions.ThreadGroup.ThreadItem Thread;
        public KernelThread(int interval, string name)
        {
            Thread = new Extensions.ThreadGroup.ThreadItem(interval, name, OnProcess);
        }
        public void Start()
        {
            Thread.Open();
        }
        static int _last = 0;
        public int Online
        {
            get
            {
                int current = Database.Server.GamePoll.Count;
                if (current > _last)
                    _last = current;
                return current;
            }
        }
        public static int MaxOnline
        {
            get { return _last; }
        }
        public void OnProcess()
        {
            Extensions.Time32 clock = Extensions.Time32.Now;
        //    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
         //   timer.Start();
            try
            {
                StartDate = DateTime.Now;

                if (clock > UpdateServerStatus.AddSeconds(1))
                {
                    if(Program.ServerConfig.IsInterServer)
                       MyConsole.Title = "[" + Database.GroupServerList.MyServerInfo.Name + "] Online Count " + Database.Server.GamePoll.Count + " Start time: " + StartDate.ToString("[hh:mm | dd/MM/yyyy]");
                    else
                        MyConsole.Title = Program.ServerConfig.ServerName + " - Online: " + Online + " - Max " + MaxOnline;
                    UpdateServerStatus = Extensions.Time32.Now.AddSeconds(5);
                }
                if (clock > Program.ResetRandom)
                {

                    Program.GetRandom.SetSeed(Environment.TickCount);

                    Program.ResetRandom = Extensions.Time32.Now.AddMinutes(30);
                }
                Game.MsgTournaments.MsgSchedules.CheckUp(clock);
                Program.GlobalItems.Work(clock);

                foreach (var roullet in Database.Roulettes.RoulettesPoll.Values)
                    roullet.work(clock);


                Game.MsgTournaments.MsgSchedules.TeamArena.CheckGroups(clock);
                Game.MsgTournaments.MsgSchedules.TeamArena.CreateMatches(clock);
                Game.MsgTournaments.MsgSchedules.TeamArena.VerifyMatches(clock);

                Game.MsgTournaments.MsgSchedules.Arena.CheckGroups(clock);
                Game.MsgTournaments.MsgSchedules.Arena.CreateMatches(clock);
                Game.MsgTournaments.MsgSchedules.Arena.VerifyMatches(clock);

                foreach (var elitegroup in Game.MsgTournaments.MsgTeamPkTournament.EliteGroups)
                    elitegroup.timerCallback(clock);
                foreach (var elitegroup in Game.MsgTournaments.MsgSkillTeamPkTournament.EliteGroups)
                    elitegroup.timerCallback(clock);

                foreach (var elitegroup in Game.MsgTournaments.MsgEliteTournament.EliteGroups)
                    elitegroup.timerCallback(clock);

                //WebServer.Proces.work(clock);
                Game.MsgTournaments.MsgBroadcast.Work(clock);
                Database.Server.Reset(clock);
                Program.SaveDBPayers(clock);

                DateTime DateNow = DateTime.Now;

                ///Removed From Here Tyrant

            }
            catch (Exception e) { MyConsole.WriteException(e); }
        }
    }
}
