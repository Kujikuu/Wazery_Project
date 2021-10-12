using LightConquer_Project.Game.MsgNpc;
using LightConquer_Project.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static LightConquer_Project.Game.MsgServer.MsgMessage;
using LightConquer_Project.Game.MsgServer.AttackHandler;
using LightConquer_Project.Database;
using LightConquer_Project.Game.MsgEvents;

namespace LightConquer_Project.Game.MsgTournaments
{
    public class MsgSchedules
    {
        public static List<MsgServer.AttackHandler.coords> HideNSeek = new List<MsgServer.AttackHandler.coords>();
        public static Extensions.Time32 Stamp = Extensions.Time32.Now.AddMilliseconds(KernelThread.TournamentsStamp);


        public static Dictionary<TournamentType, ITournament> Tournaments = new Dictionary<TournamentType, ITournament>();
        public static ITournament CurrentTournament;

        internal static MsgGuildWar GuildWar;
        //internal static MsgCitywar Citywar;
        internal static MsgEliteGuildWar EliteGuildWar;

        internal static MsgArena Arena;
        internal static MsgTeamArena TeamArena;
        internal static MsgClassPKWar ClassPkWar;
        internal static MsgEliteTournament ElitePkTournament;
        internal static MsgTeamPkTournament TeamPkTournament;
        internal static MsgSkillTeamPkTournament SkillTeamPkTournament;
        internal static MsgCaptureTheFlag CaptureTheFlag;
        internal static MsgClanWar ClanWar;
        internal static MsgWeeklyPKChampion WeeklyPKChampion;
        internal static MsgMonthlyPKChampion MonthlyPKChampion;
        internal static MsgDisCity DisCity;
        internal static MsgTowerOfMystery TowerOfMystery;

        internal static MsgCouples CouplesPKWar;



        internal static void Create()
        {

            Tournaments.Add(TournamentType.None, new MsgNone(TournamentType.None));
            Tournaments.Add(TournamentType.TreasureThief, new MsgTreasureThief(TournamentType.TreasureThief));
            Tournaments.Add(TournamentType.SpeedHunterGame, new MsgSpeedHunterGame(TournamentType.SpeedHunterGame));

            CurrentTournament = Tournaments[TournamentType.None];

            GuildWar = new MsgGuildWar();
            EliteGuildWar = new MsgEliteGuildWar();

            Arena = new MsgArena();
            TeamArena = new MsgTeamArena();
            ClassPkWar = new MsgClassPKWar(ProcesType.Dead);
            ElitePkTournament = new MsgEliteTournament();
            CaptureTheFlag = new MsgCaptureTheFlag();
            WeeklyPKChampion = new MsgWeeklyPKChampion(ProcesType.Dead);
            MonthlyPKChampion = new MsgMonthlyPKChampion(ProcesType.Dead);
            DisCity = new MsgDisCity();
            CouplesPKWar = new MsgCouples();
            TeamPkTournament = new MsgTeamPkTournament();
            SkillTeamPkTournament = new MsgSkillTeamPkTournament();
            TowerOfMystery = new MsgTowerOfMystery();


            MsgBroadcast.Create();
        }
        internal static void SendInvitation(string Name, string Prize, ushort X, ushort Y, ushort map, ushort DinamicID, int Secounds, Game.MsgServer.MsgStaticMessage.Messages messaj = Game.MsgServer.MsgStaticMessage.Messages.None)
        {
            string Message = " " + Name + " is about to begin! Will you join it? Prize[" + Prize + "]";
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                var packet = new Game.MsgServer.MsgMessage(Message, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.System).GetArray(stream);
                foreach (var client in Database.Server.GamePoll.Values)
                {
                    if (!client.Player.OnMyOwnServer || client.IsConnectedInterServer())
                        continue;
                    if (client.Player.Map == 1860 || client.Player.Map == 1858 || client.Player.Map == 1038)
                        continue;
                    client.Send(packet);
                    client.Player.MessageBox(Message, new Action<Client.GameClient>(user => user.Teleport(map, X, Y, DinamicID)), null, Secounds, messaj);
                }
            }
        }

        internal unsafe static void SendSysMesage(string Messaj, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.TopLeft
           , Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.red, bool SendScren = false)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var packet = new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream);
                foreach (var client in Database.Server.GamePoll.Values)
                    client.Send(packet);
            }
        }

        internal static void CheckUp(Extensions.Time32 clock)
        {


            if (clock > Stamp)
            {
                DateTime Now64 = DateTime.Now;

                if (Program.ServerConfig.IsInterServer)
                {
                    return;
                }
                if (!Database.Server.FullLoading)
                    return;
                if (Arena.Proces == ProcesType.Dead)
                {
                    Arena.Proces = ProcesType.Alive;
                }
                if (TeamArena.Proces == ProcesType.Dead)
                {
                    TeamArena.Proces = ProcesType.Alive;
                }
                try
                {
                    CurrentTournament.CheckUp();
                    CouplesPKWar.CheckUp();
                    ClanWar.CheckUp(Now64);



                    #region CaptureTheFlag
                    //if (CaptureTheFlag.Proces == ProcesType.Alive)
                    //{
                    //    CaptureTheFlag.UpdateMapScore();
                    //    CaptureTheFlag.CheckUpX2();
                    //    CaptureTheFlag.SpawnFlags();
                    //}
                    #endregion

                    #region Events list
                    if (Program.Events != null)
                        foreach (Game.MsgEvents.Events E in Program.Events.ToList())
                            E.ActionHandler();
                    #endregion
                    #region Ganodema
                    if (Now64.Minute == 45 && Now64.Second < 1)//re-spawn ganoderma
                    {
                        var map = Database.Server.ServerMaps[1011];
                        if (!map.ContainMobID(3130))
                        {

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Database.Server.AddMapMonster(stream, map, 3130, 667, 753, 18, 18, 1);
#if Arabic
                                   Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("The Ganodema & Titan have spawned in the forest/canyon! Hurry to kill them. Drop [Special Items 50% change.].", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                     
#else
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("The Ganodema have spawned in the forest/canyon! Hurry to kill them. Drop [Special Items 50% change.].", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

#endif
                            }
                        }
                    }
                    #endregion
                    #region Titan
                    else if (Now64.Minute == 46 && Now64.Second < 1)//re-spawn titan
                    {
                        var map = Database.Server.ServerMaps[1020];
                        if (!map.ContainMobID(3134))
                        {

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Database.Server.AddMapMonster(stream, map, 3134, 419, 618, 18, 18, 1);
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("The Titan have spawned in the forest/canyon! Hurry to kill them. Drop [Special Items 50% change.].", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                            }

                        }
                    }
                    #endregion
                    #region UnlimitedArena
                    //if ((DateTime.Now.Hour == 12 && DateTime.Now.Minute == 1 && DateTime.Now.Second <= 1))
                    //{
                    //    using (var rec = new ServerSockets.RecycledPacket())
                    //    {
                    //        var stream = rec.GetStream();
                    //        Role.Core.SendGlobalMessage(stream, $"[UnlimitedArena] Hello everyone, the Arena is available now #06", MsgMessage.ChatMode.TopLeftSystem);
                    //    }
                    //}
                    #endregion
                    #region ArenaDuel
                    //if ((DateTime.Now.Hour == 18 && DateTime.Now.Minute == 1 && DateTime.Now.Second <= 1))
                    //{
                    //    using (var rec = new ServerSockets.RecycledPacket())
                    //    {
                    //        var stream = rec.GetStream();
                    //        Role.Core.SendGlobalMessage(stream, $"[ArenaDuel] Hello everyone, the Arena is available now #06", MsgMessage.ChatMode.TopLeftSystem);
                    //    }
                    //}
                    #endregion




                    #region  Monsters Boss 



                    #region TeratoDragon
                    if (Now64.Minute == 44 && Now64.Second < 2 && Program.LastBoss == "Snow")
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            string msg = "TeratoDragon is about to spawn and terrify the world!";
                            Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                        }
                    }
                    if (Now64.Minute == 45 && Now64.Second < 2 && Program.LastBoss == "Snow")
                    {
                        var Map = Database.Server.ServerMaps[1645];
                        Program.LastBoss = "Dragon";
                        if (!Map.ContainMobID(20060))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                string msg = "TeratoDragon has spawned and terrify the world!";
                                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                Database.Server.AddMapMonster(stream, Map, 20060, 217, 214, 1, 1, 1);
                            }
                            SendInvitation("TeratoDragon has spawned and terrify the world!", " \nWould you like to join the fight against it?", 336, 241, 1645, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                            Console.WriteLine("TeratoDragon has spawned at" + DateTime.Now);

                        }
                        else
                        {
                            var loc = Map.GetMobLoc(20060);
                            if (loc != "")
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    string msg = "TeratoDragon is still alive at " + loc;
                                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                                }
                        }
                    }
                    #endregion



                    #region SnowBashee
                    if (Now64.Minute == 44 && Now64.Second < 2 && Program.LastBoss == "Dragon")
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            string msg = "SnowBashee will be spawned in 120 seconds in TwinCity (658.670)";
                            Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                        }

                    }

                    if (!Server.ServerMaps[1004].ContainMobID(21060))
                        using (var rec = new ServerSockets.RecycledPacket())
                            Server.AddMapMonster(rec.GetStream(), Server.ServerMaps[1004], 21060, 51, 50, 1, 1, 1);


                    if (Now64.Minute == 45 && Now64.Second < 2 && Program.LastBoss == "Dragon")
                    {
                        Program.LastBoss = "Snow";
                        var Map = Database.Server.ServerMaps[1002];
                        if (!Map.ContainMobID(20070))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                string msg = "SnowBashee has spawned and terrify the world!";
                                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                Database.Server.AddMapMonster(stream, Map, 20070, 658, 670, 1, 1, 1);
                            }
                            SendInvitation("SnowBashee has spawned and terrify the world!", " \nWould you like to join the fight against it?", 660, 670, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                            Console.WriteLine("SnowBashee has spawned at" + DateTime.Now);
                        }
                        else
                        {
                            var loc = Map.GetMobLoc(20070);
                            if (loc != "")
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    string msg = "SnowBashee is still alive at " + loc;
                                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                                }
                        }
                    }
                    #endregion
                    #endregion

                    #region Rand Tournament
                    if (DateTime.Now.Minute == 15 && Now64.Second < 0.5)
                    {
                        byte _totalEvents = 6;
                        int _nextEvent = Program.Rnd.Next(0, _totalEvents);

                        while (Program.WorldEvent == _nextEvent)
                            _nextEvent = Program.Rnd.Next(0, _totalEvents);

                        Program.WorldEvent = _nextEvent;
                        LightConquer_Project.Game.MsgEvents.Events NextEvent = new LightConquer_Project.Game.MsgEvents.Events();
                        switch (_nextEvent)
                        {
                            case 0:
                                NextEvent = new DragonWar();
                                break;
                            case 1:
                                NextEvent = new FFa();
                                break;
                            case 2:
                                NextEvent = new FreezeWar();
                                break;
                            case 3:
                                NextEvent = new Get5Out();
                                break;
                            case 4:
                                NextEvent = new skillmaster();
                                break;
                            case 5:
                                NextEvent = new LadderTournament();
                                break;


                        }
                        NextEvent.StartTournament();
                    }
                    #endregion
                    #region SpeedHunterGame & TreasureThief
                    Random Rand = new Random();

                    if (CurrentTournament.Process == ProcesType.Dead)
                    {

                        if (Now64.Minute == 52 && Now64.Second < 4)
                        {
                            var X = Rand.Next(0, 3);
                            if (X == 0)
                                CurrentTournament = Tournaments[TournamentType.TreasureThief];
                            if (X == 1)
                                CurrentTournament = Tournaments[TournamentType.DBShower];
                            if (X == 2)
                                CurrentTournament = Tournaments[TournamentType.SpeedHunterGame];
                            CurrentTournament.Open();
                            Console.WriteLine("Started Tournament " + CurrentTournament.Type.ToString() + "at " + DateTime.Now);

                        }
                    }

                    //if (CurrentTournament.Process == ProcesType.Dead)
                    //{

                    //    if (Now64.Minute == 25 && Now64.Second < 4)
                    //    {
                    //        CurrentTournament = Tournaments[TournamentType.DBShower];
                    //        CurrentTournament.Open();
                    //        Console.WriteLine("Started Tournament " + CurrentTournament.Type.ToString() + "at " + DateTime.Now);

                    //    }
                    //}
                    #endregion
                    #region Rand Tournament
                    if (DateTime.Now.Minute == 25 || DateTime.Now.Minute == 15 && Now64.Second < 5)
                    {
                        //byte _totalEvents = 8;
                        //int _nextEvent = Program.Rnd.Next(0, _totalEvents);

                        //while (Program.WorldEvent == _nextEvent)
                        //    _nextEvent = Program.Rnd.Next(0, _totalEvents);


                        int _nextEvent = 0;
                        _nextEvent = Program.CurrentEvent + 1;
                        if (_nextEvent > 7)
                            _nextEvent = 0;
                        LightConquer_Project.Game.MsgEvents.Events NextEvent = new LightConquer_Project.Game.MsgEvents.Events();
                        switch (_nextEvent)
                        {
                            case 0:
                                NextEvent = new DragonWar();
                                Program.CurrentEvent = 0;
                                break;
                            case 1:
                                NextEvent = new FFa();
                                Program.CurrentEvent = 1;
                                break;
                            case 2:
                                NextEvent = new FreezeWar();
                                Program.CurrentEvent = 2;
                                break;
                            case 3:
                                NextEvent = new Get5Out();
                                Program.CurrentEvent = 3;
                                break;
                            case 4:
                                NextEvent = new LastManStand();
                                Program.CurrentEvent = 4;
                                break;
                            case 5:
                                NextEvent = new PTB();
                                Program.CurrentEvent = 5;
                                break;
                            case 6:
                                NextEvent = new SkillChampionship();
                                Program.CurrentEvent = 6;
                                break;
                            case 7:
                                NextEvent = new skillmaster();
                                Program.CurrentEvent = 7;
                                break;
                        }
                        NextEvent.StartTournament();
                        Console.WriteLine("Started Tournament " + NextEvent.ToString() + "at " + DateTime.Now);

                    }
                    #endregion

                }
                catch (Exception e)
                {
                    MyConsole.SaveException(e);
                }

                #region Daily Events
                switch (Now64.DayOfWeek)
                {
                    #region Saturday
                    case DayOfWeek.Saturday:
                        {
                            #region Elite Ladder Tournament
                            //if (DateTime.Now.Hour == 16 && DateTime.Now.Minute == 30 && Now64.Second < 0.5)
                            //{
                            //	LightConquer_Project.Game.MsgEvents.Events NextEvent = new LightConquer_Project.Game.MsgEvents.Events();
                            //	NextEvent = new EliteLadderTournament();
                            //	NextEvent.StartTournament();
                            //}
                            #endregion
                            #region ElitePkTournament
                            //if ((Now64.Hour == 18 && Now64.Minute == 59))
                            //{
                            //    ElitePkTournament.Start();
                            //}
                            #endregion

                            #region GuildWar
                            if (Now64.Hour >= 11)
                            {
                                if (GuildWar.Proces == ProcesType.Dead)
                                    GuildWar.Start();
                                if (GuildWar.Proces == ProcesType.Idle)
                                {
                                    if (Now64 > GuildWar.StampRound)
                                        GuildWar.Began();
                                }
                                if (GuildWar.Proces != ProcesType.Dead)
                                {
                                    if (DateTime.Now > GuildWar.StampShuffleScore)
                                    {
                                        GuildWar.ShuffleGuildScores();
                                    }
                                }
                                if (GuildWar.SendInvitation == false && Now64.Hour == 11)
                                {
#if Arabic
                                     SendInvitation("GuildWar", "ConquerPoints", 200, 254, 1038, 0, 60, MsgServer.MsgStaticMessage.Messages.GuildWar);
#else
                                    SendInvitation("GuildWar", "ConquerPoints", 200, 254, 1038, 0, 60, MsgServer.MsgStaticMessage.Messages.GuildWar);
#endif

                                    GuildWar.SendInvitation = true;
                                }
                            }
                            #endregion
                            break;
                        }
                    #endregion

                    #region sunday
                    case DayOfWeek.Sunday:
                        {

                            #region GuildWar
                            if (Now64.Hour < 17)
                            {
                                if (GuildWar.Proces == ProcesType.Dead)
                                    GuildWar.Start();
                                if (GuildWar.Proces == ProcesType.Idle)
                                {
                                    if (Now64 > GuildWar.StampRound)
                                        GuildWar.Began();
                                }
                                if (GuildWar.Proces != ProcesType.Dead)
                                {
                                    if (DateTime.Now > GuildWar.StampShuffleScore)
                                    {
                                        GuildWar.ShuffleGuildScores();
                                    }
                                }
                                if (GuildWar.SendInvitation == false && Now64.Hour == 12)
                                {
#if Arabic
                                     SendInvitation("GuildWar", "ConquerPoints", 200, 254, 1038, 0, 60, MsgServer.MsgStaticMessage.Messages.GuildWar);
#else
                                    SendInvitation("GuildWar", "ConquerPoints", 200, 254, 1038, 0, 60, MsgServer.MsgStaticMessage.Messages.GuildWar);
#endif

                                    GuildWar.SendInvitation = true;
                                }

                            }
                            else
                            {
                                if (GuildWar.Proces == ProcesType.Alive || GuildWar.Proces == ProcesType.Idle)
                                    GuildWar.CompleteEndGuildWar();
                            }
                            #endregion

                            break;
                        }
                    #endregion

                    #region Monday
                    case DayOfWeek.Monday:
                        {
                            #region CouplesPKWar
                            if ((Now64.Hour == 18 && Now64.Minute == 59))
                            {
                                CouplesPKWar.Open();
                            }
                            #endregion

                            #region EliteGuildWar
                            //if (Now64.Hour >= 16 && Now64.Hour < 17)
                            //{
                            //    if (EliteGuildWar.Proces == ProcesType.Dead)
                            //        EliteGuildWar.Start();
                            //    if (EliteGuildWar.Proces == ProcesType.Idle)
                            //    {
                            //        if (Now64 > EliteGuildWar.StampRound)
                            //            EliteGuildWar.Began();
                            //    }
                            //    if (EliteGuildWar.Proces != ProcesType.Dead)
                            //    {
                            //        if (DateTime.Now > EliteGuildWar.StampShuffleScore)
                            //        {
                            //            EliteGuildWar.ShuffleGuildScores();
                            //        }
                            //    }

                            //    if (EliteGuildWar.SendInvitation == false && (Now64.Hour == 16) && Now64.Minute == 30 && Now64.Second >= 30)
                            //    {
                            //        SendInvitation("EliteGuildWar", "ConquerPoints", 437, 248, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                            //        EliteGuildWar.SendInvitation = true;
                            //    }
                            //}
                            //else
                            //{
                            //    if (EliteGuildWar.Proces == ProcesType.Alive || EliteGuildWar.Proces == ProcesType.Idle)
                            //        EliteGuildWar.CompleteEndGuildWar();
                            //}
                            #endregion

                            break;
                        }
                    #endregion

                    #region tuesday
                    case DayOfWeek.Tuesday:
                        {
                            #region WeeklyPKChampion
                            if ((Now64.Hour == 18 && Now64.Minute == 59))
                            {
                                WeeklyPKChampion.Start();
                            }
                            #endregion
                            #region TeamPkTournament
                            //if ((Now64.Hour == 18 && Now64.Minute == 15))
                            //    TeamPkTournament.Start();
                            #endregion
                            break;
                        }
                    #endregion

                    #region wednesday
                    case DayOfWeek.Wednesday:
                        {

                            #region SkillTeamPkTournament
                            //if ((Now64.Hour == 19 && Now64.Minute == 55) || (Now64.Hour == 11 && Now64.Minute == 55))
                            //{
                            //    SkillTeamPkTournament.Start();
                            //}
                            #endregion
                            #region ClassPkWar
                            if ((Now64.Hour == 18 && Now64.Minute == 59))
                            {
                                ClassPkWar.Start();
                            }
                            #endregion
                            break;
                        }
                    #endregion

                    #region thursday
                    case DayOfWeek.Thursday:
                        {

                            #region CaptureTheFlag
                            //if ((Now64.Hour == 16 && Now64.Minute == 00))
                            //{
                            //    CaptureTheFlag.Start();
                            //}
                            //if ((Now64.Hour == 17 && Now64.Minute == 00))
                            //{
                            //    CaptureTheFlag.CheckFinish();
                            //}
                            #endregion
                            #region Elite Ladder Tournament
                            //if (DateTime.Now.Hour == 16 && DateTime.Now.Minute == 30 && Now64.Second < 0.5)
                            //{
                            //    LightConquer_Project.Game.MsgEvents.Events NextEvent = new LightConquer_Project.Game.MsgEvents.Events();           
                            //    NextEvent = new EliteLadderTournament();
                            //    NextEvent.StartTournament();
                            //}
                            #endregion
                            break;

                        }
                    #endregion

                    #region Friday
                    case DayOfWeek.Friday:
                        {




                            break;
                        }
                        #endregion

                }
                Stamp.Value = clock.Value + KernelThread.TournamentsStamp;
                #endregion
            }
        }
    }
}