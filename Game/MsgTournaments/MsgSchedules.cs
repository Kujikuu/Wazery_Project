using OdysseyServer_Project.Database;
using OdysseyServer_Project.Game.MsgNpc;
using OdysseyServer_Project.Game.MsgServer;
using OdysseyServer_Project.Game.MsgServer.AttackHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdysseyServer_Project.Game.MsgTournaments
{
    public class MsgSchedules
    {
        public static List<coords> HideNSeek = new List<coords>() { };
        
        public static Extensions.Time32 Stamp = Extensions.Time32.Now.AddMilliseconds(KernelThread.TournamentsStamp);
        public static Dictionary<TournamentType, ITournament> Tournaments = new Dictionary<TournamentType, ITournament>();
        public static ITournament CurrentTournament;
        internal static MsgGuildWar GuildWar;
        //internal static MsgGuildContest GuildContest;
        internal static MsgSuperGuildWar SuperGuildWar;
        //internal static MsgPoleDomination PoleDomination;
        internal static MsgArena Arena;
        internal static MsgTeamArena TeamArena;
        internal static MsgClassPKWar ClassPkWar;
        internal static MsgCouples CouplesPKWar;
        internal static MsgEliteTournament ElitePkTournament;
        internal static MsgTeamPkTournament TeamPkTournament;
        internal static MsgSkillTeamPkTournament SkillTeamPkTournament;
        internal static MsgCaptureTheFlag CaptureTheFlag;
        internal static MsgClanWar ClanWar;
        internal static MsgWeeklyPKChampion WeeklyPKChampion;
        internal static MsgDisCity DisCity;
        internal static MsgSteedRace SteedRace;
        internal static MsgPowerArena PowerArena;
        internal static MsgChristmasAnimation ChristmasAnimation;
        internal static MsgMsConquer MsConquer;
        internal static MsgMrConquer MrConquer;
        internal static MsgTowerOfMystery TowerOfMystery;

        internal static MsgSquidwardOctopus SquidwardOctopus;

        private static int NextBoss = 0;
        public static bool TestEvent = false;
        
        internal static void Create()
        {
            Tournaments.Add(TournamentType.None, new MsgNone(TournamentType.None));
            Tournaments.Add(TournamentType.LastManStand, new MsgLastManStand(TournamentType.LastManStand));//Hourly
            Tournaments.Add(TournamentType.FiveNOut, new MsgFiveNOut(TournamentType.FiveNOut));//Hourly            
            Tournaments.Add(TournamentType.Confused, new MsgConfused(TournamentType.Confused));//Hourly
            Tournaments.Add(TournamentType.DragonWar, new MsgDragonWar(TournamentType.DragonWar));//Hourly
            Tournaments.Add(TournamentType.FreezeWar, new MsgFreezeWar(TournamentType.FreezeWar));//Hourly
            Tournaments.Add(TournamentType.ExtremePk, new MsgExtremePk(TournamentType.ExtremePk));//Hourly
            Tournaments.Add(TournamentType.SoulShackle, new MsgSoulShackle(TournamentType.SoulShackle));//Hourly
            Tournaments.Add(TournamentType.TreasureThief, new MsgTreasureThief(TournamentType.TreasureThief));//Funny
            Tournaments.Add(TournamentType.RushTime, new MsgRushTime(TournamentType.RushTime));//Funny
            Tournaments.Add(TournamentType.TeamDeathMatch, new MsgTeamDeathMatch(TournamentType.TeamDeathMatch));
            Tournaments.Add(TournamentType.FootBall, new MsgFootball(TournamentType.FootBall));

            //Tournaments.Add(TournamentType.BettingCPs, new MsgBettingCompetition(TournamentType.BettingCPs));
            // not work yet
            //Tournaments.Add(TournamentType.KillTheCaptain, new MsgKillTheCaptain(TournamentType.KillTheCaptain));
            //Tournaments.Add(TournamentType.KillTheDragon, new MsgKillTheDragon(TournamentType.KillTheDragon));
            //
            CurrentTournament = Tournaments[TournamentType.None];
            GuildWar = new MsgGuildWar();
            //GuildContest = new MsgGuildContest();
            SuperGuildWar = new MsgSuperGuildWar();
            //PoleDomination = new MsgPoleDomination();
            Arena = new MsgArena();
            TeamArena = new MsgTeamArena();
            ClassPkWar = new MsgClassPKWar(ProcesType.Dead);
            ElitePkTournament = new MsgEliteTournament();
            CaptureTheFlag = new MsgCaptureTheFlag();
            WeeklyPKChampion = new MsgWeeklyPKChampion(ProcesType.Dead);
            MsConquer = new MsgMsConquer(ProcesType.Dead);
            MrConquer = new MsgMrConquer(ProcesType.Dead);
            CouplesPKWar = new MsgCouples();
            DisCity = new MsgDisCity();
            SteedRace = new MsgSteedRace();
            TeamPkTournament = new MsgTeamPkTournament();
            SkillTeamPkTournament = new MsgSkillTeamPkTournament();
            PowerArena = new MsgPowerArena();
            ChristmasAnimation = new MsgChristmasAnimation();
            TowerOfMystery = new MsgTowerOfMystery();
            SquidwardOctopus = new MsgSquidwardOctopus();

            MsgBroadcast.Create();

        }
        public static void SpawnLavaBeast()
        {
            Random Rand = new Random();
            var Map = Database.Server.ServerMaps[2056];
            ushort x = 0;
            ushort y = 0;
            Map.GetRandCoord(ref x, ref y);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                string msg = "LavaBeast has spawned in FrozenGrotto6! Hurry find it and kill it.";
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.TopLeft).GetArray(stream));
                Database.Server.AddMapMonster(stream, Map, 20055, (ushort)x, (ushort)y, 1, 1, 1);
                Console.WriteLine($"LavaBeast location: {x}, {y}");
                Program.DiscordEventsAPI.Enqueue($"``{msg}``");
            }
        }
        internal static void SendInvitation(string EventTitle, string Prize, ushort X, ushort Y, ushort map, ushort DinamicID, int Seconds, Game.MsgServer.MsgStaticMessage.Messages messaj = Game.MsgServer.MsgStaticMessage.Messages.None)
        {
            string Message = "" + EventTitle + " is about to begin!\nWill you join it?";
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                var packet = new Game.MsgServer.MsgMessage(Message, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream);
                foreach (var client in Database.Server.GamePoll.Values)
                {
                    if (!client.Player.OnMyOwnServer || client.IsConnectedInterServer())
                        continue;
                    client.Send(packet);
                    client.Player.MessageBox(Message, new Action<Client.GameClient>(user => user.Teleport(X, Y, map, DinamicID)), null, Seconds, messaj);
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
        static bool hideNSeek = false;
        static List<string> SystemMsgs = new List<string>() {
            "Selling/trading cps outside the game will lead to your accounts banned forever.",
            "Join our discord group to be in touch with the community and suggest/report stuff.",
            "Administrators have [GM/PM] in their names,do not trust anyone else claiming to be a [GM/PM].",
            "Refer our server and gain rewards! (contact GM/PM).",
            "Thanks for supporting us! we will keep on working to provide the best for you!",
            "Check out Guide in TwinCity for information about the game.",
            "Like our Facebook page to support us! Facebook.com/RiViaConquer."
        };
        static List<string> TOS = new List<string>() {
            "Sharing accounts is done at your own risk. You alone are responsible for your own accounts, Support will not be given on cases for shared accounts.",
            "Always treat the STAFF of RiViaConquer with the utmost respect. No insulting/cursing about them or the server.",
            "It's forbidden to advertise any other servers. Your account will be permanently banned without prior notice/warning, Repeated offenses will result in your IP Address being permanently banned.",
            "It's forbidden to abuse bugs or any kind of bug/glitch found in the game, If a player discovers a bug/glitch in the game, it must be reported in Facebook or to the first STAFF member you can find.",
            "It's forbidden to use Bots/Hacks/Cheats in-game. If you find any working Bots/Hacks/Cheats please report them to our STAFF.",
            "Mouse clickers are allowed as long as you're not away-from-keyboard. If you're found using any mouse clicker or macro while away you'll be botjailed.",
            "Only English is allowed in the world chat.",
            "Selling/Trading accounts/items/gold outside the game for real life currencies, for items in other servers or for any other exchange or just the attempt of doing so, will result in all your accounts being permanently banned."
        };
        public static bool TCBossInv = false, TCBossLaunched = false, TeratoINV = false, TeratoLaunched = false, HourlyBossInv = false, HourlyBossLaunched = false;
        internal static DateTime NextLavaCheck;
        internal static void CheckUp(Extensions.Time32 clock)
        {

            if (clock > Stamp)
            {
                DateTime Now64 = DateTime.Now;

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
                    PowerArena.CheckUp();

                    #region CaptureTheFlag
                    if (CaptureTheFlag.Proces == ProcesType.Alive)
                    {
                        CaptureTheFlag.UpdateMapScore();
                        CaptureTheFlag.CheckUpX2();
                        CaptureTheFlag.SpawnFlags();
                    }
                    #endregion
                    #region hideNSeek
                    if (Now64.Minute == 0 && Now64.Second <= 5)
                    {
                        hideNSeek = false;
                    }
                    #endregion
                    
                    
                    CurrentTournament.CheckUp();
                    CouplesPKWar.CheckUp();
                   


                    #region SystemMsgs in game
                    if (Now64.Minute % 10 == 0 && Now64.Second > 58)
                    {
                        var rndMsg = SystemMsgs[Program.GetRandom.Next(0, SystemMsgs.Count)];
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(rndMsg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                        }
                    }
                    #endregion
                    #region Total Online in discord
                    if (Now64.Minute % 10 == 0 && Now64.Second > 58)
                    {
                        Program.DiscordOnlineAPI.Enqueue($"```Total Online: {Database.Server.GamePoll.Count} - Max Online: {KernelThread.MaxOnline} ```");
                    }
                    #endregion
                    #region Discord TOS info
                    if (Now64.Minute % 15 == 0 && Now64.Second > 58)
                    {
                        var rndMsg = TOS[Program.GetRandom.Next(0, SystemMsgs.Count)];
                        Program.DiscordTOSAPI.Enqueue($"```{rndMsg}```");
                    }
                    #endregion

                    
                    #region LavaBeasts
                    if (DateTime.Now > NextLavaCheck)
                    {
                        NextLavaCheck = DateTime.Now.AddMinutes(3);
                        var MapFr = Database.Server.ServerMaps[2056];
                        if (MapFr.MobsCount(20055) < 10)
                            SpawnLavaBeast();
                    }
                    #endregion

                    #region bosses

                    #region SwordMaster
                    if (Now64.Minute == 15 && Now64.Second < 2)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            string msg = "SwordMaster is about to spawn and terrify the world!";
                            Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                            Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                        }

                    }
                    if (Now64.Minute == 16 && Now64.Second <= 2)
                        if (NextBoss == 0)
                    {
                        var Map = Database.Server.ServerMaps[1000];

                        if (!Map.ContainMobID(6643))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                string msg = "SwordMaster has spawned and terrify the world!";
                                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                Database.Server.AddMapMonster(stream, Map, 6643, 293, 458, 1, 1, 1);
                                Program.DiscordBossesAPI.Enqueue($"``{msg}``");
                            }
                            SendInvitation("SwordMaster has spawned and terrify the world!", " \nWould you like to join the fight against it?", 291, 449, 1000, 0, 60, MsgServer.MsgStaticMessage.Messages.None);//bahaa
                        }
                        else
                        {
                            var loc = Map.GetMobLoc(20070);
                            if (loc != "")
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    string msg = "SwordMaster is still alive in DesertCity at " + loc;
                                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                    Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                                }
                        }
                    }
                    #endregion

                    #region SnowBashee
                    if (Now64.Minute == 30 && Now64.Second < 2)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            string msg = "SnowBashee is about to spawn and terrify the world!";
                            Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                            Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                        }

                    }
                    if (Now64.Minute == 31 && Now64.Second <= 2)
                    {
                        var Map = Database.Server.ServerMaps[2054];

                        if (!Map.ContainMobID(20070))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                string msg = "SnowBashee has spawned and terrify the world!";
                                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                Database.Server.AddMapMonster(stream, Map, 20070, 407, 433, 1, 1, 1);
                                Program.DiscordBossesAPI.Enqueue($"``{msg}``");
                            }
                            SendInvitation("SnowBashee has spawned and terrify the world!", " \nWould you like to join the fight against it?", 396, 429, 2054, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                        }
                        else
                        {
                            var loc = Map.GetMobLoc(2054);
                            if (loc != "")
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    string msg = "SnowBashee is still alive in FrozenGrotto2. at " + loc;
                                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                    Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                                }
                        }
                    }
                    #endregion

                    #region Thrilling Spook
                    //if (Now64.Minute == 44 && Now64.Second < 2)
                    //{
                    //    using (var rec = new ServerSockets.RecycledPacket())
                    //    {
                    //        var stream = rec.GetStream();
                    //        string msg = "Thrilling Spook is about to spawn and terrify the world!";
                    //        Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                    //        Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                    //    }

                    //}
                    //if (Now64.Minute == 45 && Now64.Second <= 2)
                    //{
                    //    var Map = Database.Server.ServerMaps[2090];

                    //    if (!Map.ContainMobID(20160))
                    //    {
                    //        using (var rec = new ServerSockets.RecycledPacket())
                    //        {
                    //            var stream = rec.GetStream();
                    //            string msg = "Thrilling Spook has spawned and terrify the world!";
                    //            Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                    //            Database.Server.AddMapMonster(stream, Map, 20160, 41, 40, 1, 1, 1);
                    //            Program.DiscordBossesAPI.Enqueue($"``{msg}``");
                    //        }
                    //        SendInvitation("Thrilling Spook has spawned and terrify the world!", " \nWould you like to join the fight against it?", 245, 153, 1036, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                    //    }
                    //    else
                    //    {
                    //        var loc = Map.GetMobLoc(2090);
                    //        if (loc != "")
                    //            using (var rec = new ServerSockets.RecycledPacket())
                    //            {
                    //                var stream = rec.GetStream();
                    //                string msg = "Thrilling Spook is still alive in the Spook's land (Join from market). at " + loc;
                    //                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                    //                Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                    //            }
                    //    }
                    //}
                    #endregion

                    #region TeratoDragon
                    if (Now64.Minute == 45 && Now64.Second < 2)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            string msg = "TeratoDragon is about to spawn and terrify the world!";
                            Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                            Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                        }

                    }
                    if (Now64.Minute == 46 && Now64.Second <= 2)
                    {
                        var Map = Database.Server.ServerMaps[2056];

                        if (!Map.ContainMobID(20060))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                string msg = "TeratoDragon has spawned and terrify the world!";
                                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                Database.Server.AddMapMonster(stream, Map, 20060, 328, 354, 1, 1, 1);
                                Program.DiscordBossesAPI.Enqueue($"``{msg}``");
                            }
                            SendInvitation("TeratoDragon has spawned and terrify the world!", " \nWould you like to join the fight against it?", 322, 341, 2056, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                        }
                        else
                        {
                            var loc = Map.GetMobLoc(20060);
                            if (loc != "")
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    string msg = "TeratoDragon is still alive in FrozenGrotto [6] at " + loc;
                                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                    Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                                }
                        }
                    }
                    #endregion

                    #region NemesisTyrant
                    if (Now64.Minute == 58 && Now64.Second < 2)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            string msg = "NemesisTyrant is about to spawn and terrify the world!";
                            Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                            Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                        }

                    }
                    if (Now64.Minute == 59 && Now64.Second <= 2)
                    {
                        var Map = Database.Server.ServerMaps[3846];

                        if (!Map.ContainMobID(20300))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                string msg = "NemesisTyrant has spawned and terrify the world!";
                                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                Database.Server.AddMapMonster(stream, Map, 20300, 118, 187, 1, 1, 1);
                                Program.DiscordBossesAPI.Enqueue($"``{msg}``");
                            }
                            SendInvitation("NemesisTyrant has spawned and terrify the world!", " \nWould you like to join the fight against it?", 120, 195, 3846, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                        }
                        else
                        {
                            var loc = Map.GetMobLoc(20060);
                            if (loc != "")
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    string msg = "NemesisTyrant is still alive in BloodShedSea on (118, 187)";
                                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                    Program.DiscordBossesAPI.Enqueue($"``{msg}``");

                                }
                        }
                    }
                    #endregion

                    #endregion                    
                    #region Hourly

                    #region Rand Tournament
                    Random Rand = new Random();
                    if (CurrentTournament.Process == ProcesType.Dead)
                    {
                        if (Now64.Minute == 20 && Now64.Second < 4 || Now64.Minute == 40 && Now64.Second < 4)
                        {
                            TestEvent = false;
                            switch (Rand.Next(0, 6))
                            {
                                case 0:
                                    CurrentTournament = Tournaments[TournamentType.LastManStand];
                                    break;
                                case 1:
                                    CurrentTournament = Tournaments[TournamentType.FiveNOut];
                                    break;
                                case 2:
                                    CurrentTournament = Tournaments[TournamentType.Confused];
                                    break;
                                case 3:
                                    CurrentTournament = Tournaments[TournamentType.FreezeWar];
                                    break;
                                case 4:
                                    CurrentTournament = Tournaments[TournamentType.DragonWar];
                                    break;
                                case 5:
                                    CurrentTournament = Tournaments[TournamentType.ExtremePk];
                                    break;
                                //case 6:
                                //    CurrentTournament = Tournaments[TournamentType.FootBall];
                                //    break;
                                default:
                                    CurrentTournament = Tournaments[TournamentType.TeamDeathMatch];
                                    break;
                            }
                            CurrentTournament.Open();
                            Console.WriteLine("Started Tournament " + CurrentTournament.Type.ToString());
                            Program.DiscordEventsAPI.Enqueue("``Tournament " + CurrentTournament.Type.ToString() + " has started!``");
                        }
                    }
                    #endregion
                    #region Daily event Tournament
                    //if (CurrentTournament.Process == ProcesType.Dead)
                    //{
                    //    if (Now64.Hour == 19 && Now64.Second < 2)
                    //    {

                    //        CurrentTournament = Tournaments[TournamentType.TreasureThief];
                    //        CurrentTournament.Open();
                    //        Console.WriteLine("Started Tournament " + CurrentTournament.Type.ToString());
                    //        Program.DiscordEventsAPI.Enqueue("``Tournament " + CurrentTournament.Type.ToString() + " has started!``");
                    //    }
                    //}
                    #endregion

                    #region Hide(n)Seek
                    if (Now64.Minute == 00 && Now64.Second >= 20 && !hideNSeek)
                    {
                        hideNSeek = true;
                        var map = Server.ServerMaps[1036];
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Role.Core.SendGlobalMessage(stream, $"Hide(n)Seek event has started! Find the [GM] Npc in market to claim a prize.", MsgMessage.ChatMode.TopLeftSystem);
                            Role.Core.SendGlobalMessage(stream, $"Hide(n)Seek event has started! Find the [GM] Npc in market to claim a prize.", MsgMessage.ChatMode.Center);
                            Program.DiscordEventsAPI.Enqueue("``Hide(n)Seek event has started! Find the [GM] Npc in market to claim a prize.``");
                        }
                        ushort x = 0, y = 0;
                        map.GetRandCoord(ref x, ref y);
                        var npc = Game.MsgNpc.Npc.Create();
                        npc.UID = (uint)NpcID.HideNSeek;
                        map.GetRandCoord(ref x, ref y);
                        npc.X = (ushort)x;
                        npc.Y = (ushort)y;
                        npc.Mesh = 29681;
                        npc.NpcType = Role.Flags.NpcType.Talker;
                        npc.Map = 1036;
                        map.AddNpc(npc);
                        Console.WriteLine($"Hide(N)Seek location: {npc.X}, {npc.Y}");
                    }
                    #endregion
                    #region GuildContest
                    //if (DateTime.Now.Hour == 19 && Now64.Second < 2)
                    //{
                    //    if (GuildContest.Proces == ProcesType.Dead)
                    //        GuildContest.Start();
                    //    if (GuildContest.Proces == ProcesType.Idle)
                    //    {
                    //        if (Now64 > GuildContest.StampRound)
                    //            GuildContest.Began();
                    //    }
                    //    if (GuildContest.Proces != ProcesType.Dead)
                    //    {
                    //        if (DateTime.Now > GuildContest.StampShuffleScore)
                    //        {
                    //            GuildContest.ShuffleGuildScores();
                    //        }
                    //    }                        

                    //}
                    //else if (DateTime.Now.Hour == 20 && Now64.Second < 2)
                    //{
                    //    if (GuildContest.Proces == ProcesType.Alive || GuildContest.Proces == ProcesType.Idle)
                    //        GuildContest.CompleteEndGuildContest();
                    //}
                    #endregion
                    #region  SuperGuildWar
                    if (DateTime.Now.Hour == 19 && Now64.Second < 2)
                    {

                        if (SuperGuildWar.Proces == ProcesType.Dead)
                            SuperGuildWar.Start();
                        if (SuperGuildWar.Proces == ProcesType.Idle)
                        {
                            if (Now64 > SuperGuildWar.StampRound)
                                SuperGuildWar.Began();
                        }
                        if (SuperGuildWar.Proces != ProcesType.Dead)
                        {
                            if (DateTime.Now > SuperGuildWar.StampShuffleScore)
                            {
                                SuperGuildWar.ShuffleGuildScores();
                            }
                            SuperGuildWar.CheckFlags();
                        }
                    }
                    if (DateTime.Now.Hour == 20 && Now64.Second < 2)
                    {
                        if (SuperGuildWar.Proces == ProcesType.Alive || SuperGuildWar.Proces == ProcesType.Idle)
                            SuperGuildWar.CompleteEndGuildWar();
                    }
                    #endregion
                    #region PowerArena
                    //if (Now64.Hour == 12 && Now64.Minute == 55)
                    //    PowerArena.Start();
                    #endregion
                    #endregion
                }
                catch (Exception e)
                {
                    Console.SaveException(e);
                }

                

                #region Daily Events
                switch (Now64.DayOfWeek)
                {
                    #region Saturday
                    case DayOfWeek.Saturday://sambata
                        {

                            #region TeamPkTournament
                            if (Now64.Hour == 19 && Now64.Minute == 0)
                                TeamPkTournament.Start();
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

                            #region CaptureTheFlag
                            if (Now64.Hour == 21)
                            {
                                CaptureTheFlag.Start();
                            }
                            if (Now64.Hour == 22)
                            {
                                CaptureTheFlag.CheckFinish();
                            }
                            #endregion

                            #region WeeklyPKChampion
                            if (Now64.Hour == 20 && Now64.Minute == 0)
                            {
                                WeeklyPKChampion.Start();
                            }
                            #endregion

                            #region CouplesPKWar
                            if (Now64.Hour == 14 && Now64.Minute == 45)
                            {
                                CouplesPKWar.Open();
                            }
                            #endregion

                            
                            break;
                        }
                    #endregion

                    #region sunday
                    case DayOfWeek.Sunday://duminica
                        {
                           
                            #region GuildWar
                            if (Now64.Hour < 20)
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
                                if (GuildWar.SendInvitation == false && Now64.Hour == 18)
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
                            #region MrsConquer
                            if (Now64.Hour == 16 && Now64.Minute == 36)
                            {
                                MsConquer.Start();
                            }
                            #endregion

                            #region ClanWar start
                            if (DateTime.Now.Hour == 21 && DateTime.Now.Minute == 0)
                                ClanWar.Open();
                            ClanWar.CheckUp(Now64);
                            #endregion

                            #region ClassPkWar
                            if (Now64.Hour == 20 && Now64.Minute == 30)
                            {
                                ClassPkWar.Start();
                            }
                            #endregion

                            break;
                        }
                    #endregion

                    #region tuesday
                    case DayOfWeek.Tuesday://marti
                        {
                            #region ClanWar start
                            if (DateTime.Now.Hour == 21 && DateTime.Now.Minute == 0)
                                ClanWar.Open();
                            ClanWar.CheckUp(Now64);
                            #endregion



                            #region MrConquer
                            if (Now64.Hour == 16 && Now64.Minute == 40)
                            {
                                MrConquer.Start();
                            }
                            #endregion

                            break;
                        }
                    #endregion

                    #region wednesday
                    case DayOfWeek.Wednesday://miercuri
                        {
                            #region ClanWar start
                            if (DateTime.Now.Hour == 21 && DateTime.Now.Minute == 2)
                                ClanWar.Open();
                            ClanWar.CheckUp(Now64);
                            #endregion
                            
                            #region SkillTeamPkTournament
                            if (Now64.Hour == 20 && Now64.Minute == 00)
                            {
                                SkillTeamPkTournament.Start();
                            }
                            #endregion


                            break;
                        }
                    #endregion

                    #region thursday
                    case DayOfWeek.Thursday:
                        {
                            #region ClanWar
                            if (DateTime.Now.Hour == 21 && DateTime.Now.Minute == 0)
                                ClanWar.Open();
                                ClanWar.CheckUp(Now64);
                            #endregion


                            break;

                        }
                    #endregion

                    #region Friday
                    case DayOfWeek.Friday:
                        {
                            #region ClanWar
                            if (DateTime.Now.Hour == 21 && DateTime.Now.Minute == 0)
                                ClanWar.Open();
                            ClanWar.CheckUp(Now64);
                            #endregion

                            #region ElitePkTournament
                            if (Now64.Hour == 19 && Now64.Minute == 55)
                            {
                                ElitePkTournament.Start();
                            }
                            #endregion

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
