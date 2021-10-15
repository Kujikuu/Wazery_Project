using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using LightConquer_Project.Game.MsgServer;
using LightConquer_Project.Game;
using LightConquer_Project.Cryptography;
using ProtoBuf;
using LightConquer_Project.Database;
using System.Net;


namespace LightConquer_Project
{
    using PacketInvoker = CachedAttributeInvocation<Action<Client.GameClient, ServerSockets.Packet>, PacketAttribute, ushort>;

    class Program
    {
        public static string LoginKey = "MW3PeM8ZGUbLopBZ";
        public static Client.GameClient CharacterFromName2(string Name)
        {
            foreach (Client.GameClient C in Database.Server.GamePoll.Values)
                if (C.Player.Name.ToLower() == Name.ToLower())
                    return C;
            return null;
        }
        public static Extensions.Time32 CurrentTime
        {
            get
            {
                return new Extensions.Time32();
            }
        }
        public static List<byte[]> LoadPackets = new List<byte[]>();
        public static ServerSockets.SocketThread SocketsGroup;
        public static List<uint> ProtectMapSpells = new List<uint>() { 1038 };
        public static List<uint> MapCounterHits = new List<uint>() { 1005, 6000, 2500 };
        public static bool OnMainternance = false;
        public static List<uint> EventsMaps = new List<uint>() { (uint)1801, (uint)1767, (uint)700, (uint)701, (uint)1763, (uint)1080, (uint)1844, (uint)1505, (uint)1506, (uint)1507, (uint)1508 };
        public static int WorldEvent = 0;
        public static byte CurrentEvent = 0;
        public static MyRandom Rnd = new MyRandom();
        public static string ExcAdd = "";
        public static List<Game.MsgEvents.Events> Events = new List<Game.MsgEvents.Events>();
        public static ulong BCPsHuntedSinceRestart = 0;
        public static ulong CPsHuntedSinceRestart = 0;
        public static Extensions.Time32 SaveDBStamp = Extensions.Time32.Now.AddMilliseconds(KernelThread.SaveDatabaseStamp);
        public static List<uint> RankableFamilyIds = new List<uint>() { 20300, 20160, 20070, 29370, 29360, 29300, 29363, 20070, 20060 };
        public static List<uint> NoDropItems = new List<uint>() { 1801,1767, 2510, 1764, 9998, 9999, 9997, 700, 3954, 3820, 2500, 9990 };
        public static List<uint> FreePkMap = new List<uint>() { 1801,1767, 3868, 2510, 3998,3071, 6000, 6001,1505, 1005, 1038, 700,1508/*PkWar*/, Game.MsgTournaments.MsgCaptureTheFlag.MapID};
        public static List<uint> BlockAttackMap = new List<uint>() { 1616, 6001, 6002, 3995, 3024, 601, 2080, 1099, 1098, 3825,3830, 3831, 3832,3834,3826,3827,3828,3829,3833, 9995,1068, 4020, 4000, 4003, 4006, 4008, 4009 , 1860 ,1858, 1801, 1780, 1779/*Ghost Map*/, 9972, 1806, 1002, 3954, 3081, 1036, 1004, 1008, 601, 1006, 1511, 1039, 700, Game.MsgTournaments.MsgEliteGroup.WaitingAreaID};
        public static List<uint> BlockTeleportMap = new List<uint>() { 601, 6000, 6001, 6002, 1005, 700, 1858, 1860, 3852, Game.MsgTournaments.MsgEliteGroup.WaitingAreaID, 1768 };
        public static Role.Instance.Nobility.NobilityRanking NobilityRanking = new Role.Instance.Nobility.NobilityRanking();
        public static Role.Instance.ChiRank ChiRanking = new Role.Instance.ChiRank();
        public static Role.Instance.Flowers.FlowersRankingToday FlowersRankToday = new Role.Instance.Flowers.FlowersRankingToday();
        public static Role.Instance.Flowers.FlowerRanking GirlsFlowersRanking = new Role.Instance.Flowers.FlowerRanking();
        public static Role.Instance.Flowers.FlowerRanking BoysFlowersRanking = new Role.Instance.Flowers.FlowerRanking(false);

        public static ShowChatItems GlobalItems;
        public static SendGlobalPacket SendGlobalPackets;
        public static PacketInvoker MsgInvoker;
        public static ServerSockets.ServerSocket GameServer;
        public static string LastBoss;

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleHandlerDelegate handler, bool add);
        private delegate bool ConsoleHandlerDelegate(int type);
        private static ConsoleHandlerDelegate handlerKeepAlive;

        public static bool ProcessConsoleEvent(int type)
        {
            try
            {
                if (ServerConfig.IsInterServer)
                {
                    foreach (var client in Database.Server.GamePoll.Values)
                    {
                        try
                        {
                            if (client.Socket != null)//for my fake accounts !
                                client.Socket.Disconnect();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                    return true;
                }
                try
                {
                  
                 
                    if (GameServer != null)
                        GameServer.Close();


                }
                catch (Exception e) { MyConsole.SaveException(e); }

               
                Console.WriteLine("Saving Database...");
                // anta m3'ertsh el namespace  et3ml

                foreach (var client in Database.Server.GamePoll.Values)
                {
                    try
                    {
                        if (client.Socket != null)//for my fake accounts !
                            client.Socket.Disconnect();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                Role.Instance.Clan.ProcessChangeNames();

                Database.Server.SaveDatabase();
                if (Database.ServerDatabase.LoginQueue.Finish())
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Database Save Succefull.");
                }
            }
            catch (Exception e)
            {
                MyConsole.SaveException(e);
            }
            return true;
        }

        public static Extensions.Time32 ResetRandom = new Extensions.Time32();
        public static FastRandom Random = new FastRandom();
        public static Extensions.SafeRandom GetRandom = new Extensions.SafeRandom();
        public static Extensions.RandomLite LiteRandom = new Extensions.RandomLite();

        public static class ServerConfig
        {
            public static string CO2Folder = "";
            public static string XtremeTopLink = "http://www.xtremetop100.com/in.php?site=1132351362";
            public static string FBStoreLink = "https://www.facebook.com/groups/VampiresCoo/";
            public static string RedeemLink = "https://www.facebook.com/groups/VampiresCoo/";

            public static uint ServerID = 0;

            public static string IPAddres = "196.221.144.230";
            public static ushort AuthPort = 9960;
            public static ushort GamePort = 5816;
            public static string ServerName = "LightConquer";

            public static string OfficialWebSite = "https://discord.gg/fVR92ar/";
            //InternetPort
            public static ushort Port_BackLog;
            public static ushort Port_ReceiveSize = 8191;
            public static ushort Port_SendSize = 8191;

            //Database
            public static string DbLocation = "";

            //web Server
            public static ushort WebPort = 9900;
            public static string AccServerIPAddres = "";

            public static uint ExpRateSpell = 100;//1
            public static uint ExpRateProf = 100;//1
            public static uint UserExpRate = 270;//70
            public static uint PhysicalDamage = 100;// + 150%  

            //loader
            public static string LoaderIP = "196.221.144.230";
            public static ushort LoaderPort = 9960;

            //interServer
            public static string InterServerAddress = "196.221.144.230";
            public static ushort InterServerPort = 0;
            public static bool IsInterServer = false;
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }

        static int CutTrail(int x, int y) { return (x >= y) ? x : y; }
        static int AdjustDefence(int nDef, int power2, int bless)
        {
            int nAddDef = 0;
            nAddDef += Game.MsgServer.AttackHandler.Calculate.Base.MulDiv(nDef, 100 - power2, 100) - nDef;            
            return Game.MsgServer.AttackHandler.Calculate.Base.MulDiv(nDef + nAddDef, 100 - power2, 100);
        }


        public static void TESTT()
        {
            double base_d_factor = 130;
            double scaled_d_factor = 0.5;
            double dif = 139500 - 25000;
            double sign_dif = Math.Sign(dif);
            double scale = 1.0 + (-1.0 / (sign_dif + dif / (base_d_factor + 25000 * scaled_d_factor)) + sign_dif);
            double ttt = 139500 * scale;
        }


        static byte[] DecryptString(char[] str)
        {
            int i = 0;
            byte[] nstr = new byte[1000];
            do
            {
                nstr[i] = Convert.ToByte(str[i + 1] ^ 0x34);
            } while (nstr[i++] != 0);
            return nstr;
        }
        public static void writetext(string tes99)
        {
            char[] tg = new char[tes99.Length];
            for (int x = 0; x < tes99.Length; x++)
                tg[x] = tes99[x];
            var hhhh = DecryptString(tg);
            Console.WriteLine(ASCIIEncoding.ASCII.GetString(hhhh));
        }
        public static int n = 0;
        public static int sol;
        public static int[] v = new int[100];
        public static TransferCipher transferCipher;

        public static void afisare()
        {
            Console.WriteLine("");
            int i;
            sol++;

            Console.WriteLine("sol: " + sol);
            for (i = 1; i <= n; i++)
            {
                MyConsole.Write(v[i] + " ");

            }
            MyConsole.Write(Environment.NewLine);
        }
        public static int valid(int k)
        {
            int i;
            for (i = 1; i <= k - 1; i++)
                if ((v[k] <= v[i]))
                    return 0;
            return 1;
        }
        public static int solutie(int k)
        {
            if (k == n)
                return 1;
            return 0;
        }
        public static void BK(int k)
        {
            for (int i = 1; i <= n; i++)
            {
                v[k] = i;
                if (valid(k) == 1)
                {
                    if (solutie(k) == 1)
                        afisare();
                    else
                        BK(k + 1);
                }
            }
        }
        public static unsafe void Main(string[] args)
        {

            byte[] proto = new byte[]
            {
                0x0A ,0x06 ,0x08 ,0xBE ,0x56 ,0x10 ,0xAD ,0x2B
            };

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                stream.InitWriter();
                for (int x = 0; x < proto.Length; x++)
                    stream.Write((byte)proto[x]);
                stream.Finalize(1153);
                MsgMagicColdTime.MagicColdTime obj = new MsgMagicColdTime.MagicColdTime();
                obj = stream.ProtoBufferDeserialize<MsgMagicColdTime.MagicColdTime>(obj, proto);


            }
            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                ServerSockets.Packet.SealString = "TQServer";


                MyConsole.Title = "RiviaConquer --- AccountServer";
                Console.ForegroundColor = ConsoleColor.Red;
                MyConsole.WriteLines("################################################");
                MyConsole.WriteLines("||  Project RiviaConquer Online Private Server Emulator   ||");
                MyConsole.WriteLines("||        April 2020 - All Rights Reserved.        ||");
                MyConsole.WriteLines("################################################");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("");

                MsgInvoker = new PacketInvoker(PacketAttribute.Translator);
                Cryptography.DHKeyExchange.KeyExchange.CreateKeys();

                Game.MsgTournaments.MsgSchedules.Create();

                Database.Server.Initialize();

                SendGlobalPackets = new SendGlobalPacket();

                Cryptography.AuthCryptography.PrepareAuthCryptography();

                Database.Server.LoadDatabase();

                handlerKeepAlive = ProcessConsoleEvent;
                SetConsoleCtrlHandler(handlerKeepAlive, true);




               
               // WebServer.Proces.Init();
                TransferCipher.Key = Encoding.ASCII.GetBytes("Eyp999vYJ3zdLCTyz9Ak8RAgM78tY5F32b7CUXDuLDJDFBH8H67BWy9QThmaN5VS");
                TransferCipher.Salt = Encoding.ASCII.GetBytes("Myq999f3ytALHWLXbJxSUX4uFEu3Xmz2UAY9sTTm8AScB7Kk2uwqDSnuNJske4BJ");
                transferCipher = new TransferCipher("127.0.0.1");
                if (ServerConfig.IsInterServer == false)
                {
                    GameServer = new ServerSockets.ServerSocket(
                        new Action<ServerSockets.SecuritySocket>(p => new Client.GameClient(p))
                        , Game_Receive, Game_Disconnect);
                    GameServer.Initilize(ServerConfig.Port_SendSize, ServerConfig.Port_ReceiveSize, 1, 3);
                    GameServer.Open(ServerConfig.IPAddres, ServerConfig.GamePort, ServerConfig.Port_BackLog);

                }


                GlobalItems = new ShowChatItems();
                Console.WriteLine("Starting the server...");
                Database.NpcServer.LoadServerTraps();                
                ThreadPoll.Create();                
                SocketsGroup = new ServerSockets.SocketThread("ConquerServer", GameServer, MsgInterServer.PipeServer.Server 
                  );
                SocketsGroup.Start();
                MsgInterServer.StaticConnexion.Create();
                Game.MsgTournaments.MsgSchedules.ClanWar = new Game.MsgTournaments.MsgClanWar();
                new KernelThread(1000, "ConquerServer2").Start();
                new MapGroupThread(300, "ConquerServer3").Start();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("The server is ready for incoming connections!\n", ConsoleColor.Green);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception e) { MyConsole.WriteException(e); }

            for (;;)
                ConsoleCMD(MyConsole.ReadLine());
        }

        public static void SaveDBPayers(Extensions.Time32 clock)
        {

            if (clock > SaveDBStamp)
            {
                if (Database.Server.FullLoading && !Program.ServerConfig.IsInterServer)
                {
                    foreach (var user in Database.Server.GamePoll.Values)
                    {
                        if (user.OnInterServer)
                            continue;
                        if ((user.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                        {
                            user.ClientFlag |= Client.ServerFlag.QueuesSave;
                            Database.ServerDatabase.LoginQueue.TryEnqueue(user);
                        }
                    }
                    Database.Server.SaveDatabase();
                }
                SaveDBStamp.Value = clock.Value + KernelThread.SaveDatabaseStamp;
            }

        }
        public unsafe static void ConsoleCMD(string cmd)
        {
            try
            {
                string[] line = cmd.Split(' ');

                switch (line[0])
                {
                    case "clear":
                        {
                            System.Console.Clear();
                            break;
                        }
                    case "save":
                        {
                            Database.Server.SaveDatabase();
                            if (Database.Server.FullLoading && !Program.ServerConfig.IsInterServer)
                            {
                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if (user.OnInterServer)
                                        continue;
                                    if ((user.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                                    {
                                        user.ClientFlag |= Client.ServerFlag.QueuesSave;
                                        Database.ServerDatabase.LoginQueue.TryEnqueue(user);
                                    }
                                }
                                Console.WriteLine("Database got saved ! ");
                            }
                            if (Database.ServerDatabase.LoginQueue.Finish())
                            {
                                System.Threading.Thread.Sleep(1000);
                                Console.WriteLine("Database saved successfully.");
                            }
                            break;
                        }

                    case "ctfon":
                        {
                            Game.MsgTournaments.MsgSchedules.CaptureTheFlag.Start();
                            break;
                        }
                    case "ctfend":
                        {
                            Game.MsgTournaments.MsgSchedules.CaptureTheFlag.CheckFinish();
                            break;
                        }
                    case "kick":
                        {

                            foreach (var user in Database.Server.GamePoll.Values)
                            {
                                if (user.Player.Name.Contains(line[1]))
                                {
                                    user.EndQualifier();
                                }
                            }
                            break;
                        }
                    case "TeamPkTournament":
                        {
                            Game.MsgTournaments.MsgSchedules.TeamPkTournament.Start();
                            break;
                        }
                    case "pk":
                        {
                            Game.MsgTournaments.MsgSchedules.ElitePkTournament.Start();

                            foreach (var clients in Database.Server.GamePoll.Values)
                            {
                                Game.MsgTournaments.MsgSchedules.ElitePkTournament.SignUp(clients);
                            }
                            break;
                        }
                    case "teampk":
                        {
                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.Start();
                            var array = Database.Server.GamePoll.Values.ToArray();


                            for (int x = 0; x < array.Length - 5; x += 5)
                            {
                                if (array[x].Team == null)
                                {
                                    try
                                    {
                                        array[x].Team = new Role.Instance.Team(array[x]);
                                        Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x]);
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            array[x + 1].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 1]);
                                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x + 1]);

                                            array[x + 2].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 2]);
                                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x + 2]);

                                            array[x + 3].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 3]);
                                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x + 3]);

                                            array[x + 4].Team = array[0].Team;
                                            array[x].Team.Add(stream, array[x + 4]);
                                            Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.SignUp(array[x + 4]);
                                        }

                                    }
                                    catch { }
                                }
                            }
                            break;
                        }
                    case "CouplesPKWar":
                        {
                            Game.MsgTournaments.MsgSchedules.CouplesPKWar.Open();
                            break;
                        }
                    case "search":
                        {
                            WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
                            foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
                            {
                                ini.FileName = fname;

                                string Name = ini.ReadString("Character", "Name", "None");
                                if (Name.ToLower() == line[1].ToLower() || Name.Contains(line[1]))
                                {
                                    Console.WriteLine(ini.ReadUInt32("Character", "UID", 0));
                                    break;
                                }

                            }
                            break;
                        }
                    case "resetnobility":
                        {
                            WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
                            foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
                            {
                                ini.FileName = fname;

                                ulong nobility = ini.ReadUInt64("Character", "DonationNobility", 0);
                                nobility = nobility * 30 / 100;
                                ini.Write<ulong>("Character", "DonationNobility", nobility);
                            }

                            break;
                        }
                    case "resetonline":
                        {
                            WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
                            foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
                            {
                                ini.FileName = fname;

                                ulong OnlineMinutes = ini.ReadUInt64("Character", "OnlineMinutes", 0);
                                OnlineMinutes = 0;
                                ini.Write<ulong>("Character", "OnlineMinutes", OnlineMinutes);
                            }
                            Console.WriteLine("Database onlinePoints reset successfully.");

                            break;
                        }
                    case "cp":
                        {
                            Controlpanel cp = new Controlpanel();
                            cp.ShowDialog();
                            break;
                        }
                    case "check":
                        {
                            WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
                            foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
                            {
                                ini.FileName = fname;

                                long nobility = ini.ReadInt64("Character", "Money", 0);
                                if (nobility < 0)
                                {
                                    Console.WriteLine("");
                                }

                            }
                            break;
                        }
                    case "fixedgamemap":
                        {
                            Dictionary<int, string> maps = new Dictionary<int, string>();
                            using (var gamemap = new BinaryReader(new FileStream(Path.Combine(Program.ServerConfig.CO2Folder, "ini/gamemap.dat"), FileMode.Open)))
                            {

                                var amount = gamemap.ReadInt32();
                                for (var i = 0; i < amount; i++)
                                {

                                    var id = gamemap.ReadInt32();
                                    var fileName = Encoding.ASCII.GetString(gamemap.ReadBytes(gamemap.ReadInt32()));
                                    var puzzleSize = gamemap.ReadInt32();
                                    if (id == 1017)
                                    {
                                        Console.WriteLine(puzzleSize);
                                    }
                                    if (!maps.ContainsKey(id))
                                        maps.Add(id, fileName);
                                    else
                                        maps[id] = fileName;
                                }
                            }
                            break;
                        }

                 
                    case "clan":
                        {
                            Game.MsgTournaments.MsgSchedules.ClanWar.Open();
                            break;
                        }
                    case "DisCity":
                        {
                            Game.MsgTournaments.MsgSchedules.DisCity.Open();
                            
                            break;
                        }
                    case "startgw":
                        {
                            Game.MsgTournaments.MsgSchedules.GuildWar.Proces = Game.MsgTournaments.ProcesType.Alive;
                            Game.MsgTournaments.MsgSchedules.GuildWar.Start();
                            break;
                        }
                    
                    case "finishgw":
                        {
                            Game.MsgTournaments.MsgSchedules.GuildWar.Proces = Game.MsgTournaments.ProcesType.Dead;
                            Game.MsgTournaments.MsgSchedules.GuildWar.CompleteEndGuildWar();
                            break;
                        }

                    
                    case "estartgw":
                        {
                            Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces = Game.MsgTournaments.ProcesType.Alive;
                            Game.MsgTournaments.MsgSchedules.EliteGuildWar.Start();
                            break;
                        }

                    case "efinishgw":
                        {
                            Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces = Game.MsgTournaments.ProcesType.Dead;
                            Game.MsgTournaments.MsgSchedules.EliteGuildWar.CompleteEndEliteGuildWar();
                            break;
                        }

                    case "sWeeklyPKChampion":
                        {
                            Game.MsgTournaments.MsgSchedules.WeeklyPKChampion.Start();
                            break;
                        }

                   
                    case "sMonthlyPKChampion":
                        {
                            Game.MsgTournaments.MsgSchedules.MonthlyPKChampion.Start();
                            break;
                        }

                   
                    case "sClassPkWar":
                        {
                            Game.MsgTournaments.MsgSchedules.ClassPkWar.Start();
                            break;
                        }

                  
                    case "exit":
                        {
                            new Thread(new ThreadStart(Maintenance)).Start();
                            break;
                        }
                    case "forceexit":
                        {
                            ProcessConsoleEvent(0);

                            Environment.Exit(0);
                            break;
                        }
                    case "restart":
                        {
                            ProcessConsoleEvent(0);

                            System.Diagnostics.Process hproces = new System.Diagnostics.Process();
                            hproces.StartInfo.FileName = "LightConquer_Project.exe";
                            hproces.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                            hproces.Start();

                            Environment.Exit(0);

                            break;
                        }

                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static void Maintenance()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                OnMainternance = true;
                Console.WriteLine("The server will be brought down for maintenance in (5 Minutes). Please log off immediately to avoid data loss.");
#if Arabic
                  MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 5minute0second. Please exitthe game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
              
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (5 Minutes). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (4 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
#if Arabic
                  MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 4minute30second. Please exit the game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
               
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (4 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (4 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
#if Arabic
                  MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 4minute0second. Please exit the game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
              
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (4 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (3 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
#if Arabic
                       MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 3minute30second. Please exit the game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
         
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (3 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (3 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
#if Arabic
                  MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 3minute0second. Please exit the game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
              
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (3 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (2 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
#if Arabic
                  MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 2minute30second. Please exit the game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
              
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (2 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (2 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
#if Arabic
                        MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 2minute0second. Please exit the game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
         
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (2 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (1 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
#if Arabic
                   MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 1minute30second. Please exit the game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
             
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (1 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (1 Minutes & 00 Seconds). Please log off immediately to avoid data loss.");
#if Arabic
                 MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 1minute0second. Please exit the game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
               
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (1 Minutes & 00 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 30);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Console.WriteLine("The server will be brought down for maintenance in (0 Minutes & 30 Seconds). Please log off immediately to avoid data loss.");
#if Arabic
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in 0minute30second. Please exit the game now.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                
#else
                MsgMessage msg = new MsgMessage("The server will be brought down for maintenance in (0 Minutes & 30 Seconds). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 20);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
#if Arabic
                  MsgMessage msg = new MsgMessage("Server maintenance(2 minutes). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
              
#else
                MsgMessage msg = new MsgMessage("Server maintenance(few minutes). Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);

#endif
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }
            Thread.Sleep(1000 * 10);
            ProcessConsoleEvent(0);

            Environment.Exit(0);
        }

        public unsafe static void Game_Receive(ServerSockets.SecuritySocket obj, ServerSockets.Packet stream)//ServerSockets.Packet data)
        {
            if (!obj.SetDHKey)
                CreateDHKey(obj, stream);
            else
            {
                try
                {
                    if (obj.Game == null)
                        return;
                    ushort PacketID = stream.ReadUInt16();

                    if (obj.Game.Player.CheckTransfer)
                        goto jmp;
                    if (obj.Game.PipeClient != null && PacketID != Game.GamePackets.Achievement)
                    {
                        if (PacketID == (ushort)Game.GamePackets.MsgOsShop
                      || PacketID == (ushort)Game.GamePackets.SecondaryPassword
                      || PacketID >= (ushort)Game.GamePackets.LeagueOpt && PacketID <= (ushort)Game.GamePackets.LeagueConcubines
                      || PacketID == (ushort)Game.GamePackets.LeagueRobOpt)
                            goto jmp;

                        stream.Seek(stream.Size);
                        obj.Game.PipeClient.Send(stream);

                        if (PacketID != 1009)
                        {

                            return;
                        }
                        stream.Seek(4);
                    }

#if TEST
                    Console.WriteLine("Receive -> PacketID: " + PacketID);
#endif

                    jmp:
                    Action<Client.GameClient, ServerSockets.Packet> hinvoker;
                    if (MsgInvoker.TryGetInvoker(PacketID, out hinvoker))
                    {
                        hinvoker(obj.Game, stream);
                    }
                    else
                    {
#if TEST
                        Console.WriteLine("Not found the packet ----> " + PacketID);
#endif

                    }

                }
                catch (Exception e) { MyConsole.WriteException(e); }
                finally
                {
                    ServerSockets.PacketRecycle.Reuse(stream);
                }
            }

        }
        public unsafe static void CreateDHKey(ServerSockets.SecuritySocket obj, ServerSockets.Packet Stream)
        {
            try
            {
                byte[] buffer = new byte[36];
                bool extra = false;
                string text = System.Text.ASCIIEncoding.ASCII.GetString(obj.DHKeyBuffer.buffer, 0, obj.DHKeyBuffer.Length());
                if (!text.EndsWith("TQClient"))
                {
                    System.Buffer.BlockCopy(obj.EncryptedDHKeyBuffer.buffer, obj.EncryptedDHKeyBuffer.Length() - 36, buffer, 0, 36);
                    extra = true;
                }

                string key;
                if (Stream.GetHandshakeReplyKey(out key))
                {
                    obj.SetDHKey = true;
                    obj.Game.DHKey.HandleResponse(key);
                    var compute_key = obj.Game.DHKeyExchance.PostProcessDHKey(obj.Game.DHKey.ToBytes());
                    obj.Game.Crypto.GenerateKey(compute_key);
                    obj.Game.Crypto.Reset();
                }
                else
                {
                    obj.Disconnect();
                    return;
                }
                if (extra)
                {

                    Stream.Seek(0);
                    obj.Game.Crypto.Decrypt(buffer, 0, Stream.Memory, 0, 36);
                    Stream.Size = buffer.Length;





                    Stream.Size = buffer.Length;
                    Stream.Seek(2);
                    ushort PacketID = Stream.ReadUInt16();
                    Action<Client.GameClient, ServerSockets.Packet> hinvoker;
                    if (MsgInvoker.TryGetInvoker(PacketID, out hinvoker))
                    {
                        hinvoker(obj.Game, Stream);
                    }
                    else
                    {
                        obj.Disconnect();

                        Console.WriteLine("DH KEY Not found the packet ----> " + PacketID);

                    }
                }

            }
            catch (Exception e) { MyConsole.WriteException(e); }
        }
        public unsafe static void Game_Disconnect(ServerSockets.SecuritySocket obj)
        {

            if (obj.Game != null && obj.Game.Player != null)
            {
                try
                {
                    Client.GameClient client;
                    if (Database.Server.GamePoll.TryGetValue(obj.Game.Player.UID, out client))
                    {
                        if (client.OnInterServer)
                            return;
                        if ((client.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                        {
                            if (obj.Game.PipeClient != null)
                                obj.Game.PipeClient.Disconnect();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("# Account : " + client.Player.Name + " has logged Out successful.", ConsoleColor.Yellow);
                            Console.ForegroundColor = ConsoleColor.White;

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                try
                                {
                                    if (client.Player.InUnion)
                                    {
                                        client.Player.UnionMemeber.Owner = null;
                                    }
                                    client.CheckRouletteDisconnect();
                                    client.EndQualifier();
                                    if (client.MyPokerTable != null)
                                    {
                                        client.MyPokerTable.TableMatch.DisconnetPlayer(client);
                                    }
                                    if (client.Team != null)
                                        client.Team.Remove(client, true);
                                    if (client.Player.MyClanMember != null)
                                        client.Player.MyClanMember.Online = false;
                                    if (client.IsVendor)
                                        client.MyVendor.StopVending(stream);
                                    if (client.InTrade)
                                        client.MyTrade.CloseTrade();
                                    if (client.Player.MyGuildMember != null)
                                        client.Player.MyGuildMember.IsOnline = false;

                                    if (client.Player.ObjInteraction != null)
                                    {
                                        client.Player.InteractionEffect.AtkType = Game.MsgServer.MsgAttackPacket.AttackID.InteractionStopEffect;

                                        InteractQuery action = InteractQuery.ShallowCopy(client.Player.InteractionEffect);

                                        client.Send(stream.InteractionCreate(&action));

                                        client.Player.ObjInteraction.Player.OnInteractionEffect = false;
                                        client.Player.ObjInteraction.Player.ObjInteraction = null;
                                    }


                                    client.Player.View.Clear(stream);


                                }
                                catch (Exception e)
                                {
                                    MyConsole.WriteException(e);
                                    client.Player.View.Clear(stream);
                                }
                                finally
                                {
                                    client.ClientFlag &= ~Client.ServerFlag.LoginFull;
                                    client.ClientFlag |= Client.ServerFlag.Disconnect;
                                    client.ClientFlag |= Client.ServerFlag.QueuesSave;
                                    Database.ServerDatabase.LoginQueue.TryEnqueue(client);
                                }

                                try
                                {
                                    client.Player.Associate.OnDisconnect(stream, client);

                                    //remove mentor and apprentice
                                    if (client.Player.MyMentor != null)
                                    {
                                        Client.GameClient me;
                                        client.Player.MyMentor.OnlineApprentice.TryRemove(client.Player.UID, out me);
                                        client.Player.MyMentor = null;
                                    }
                                    client.Player.Associate.Online = false;
                                    lock (client.Player.Associate.MyClient)
                                        client.Player.Associate.MyClient = null;
                                    foreach (var clien in client.Player.Associate.OnlineApprentice.Values)
                                        clien.Player.SetMentorBattlePowers(0, 0);
                                    client.Player.Associate.OnlineApprentice.Clear();
                                    //done remove
                                }
                                catch (Exception e) { Console.WriteLine(e.ToString()); }
                            }
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
            else if (obj.Game != null)
            {
                if (obj.Game.ConnectionUID != 0)
                {
                    Client.GameClient client;
                    Database.Server.GamePoll.TryRemove(obj.Game.ConnectionUID, out client);
                }
            }
        }


        public static bool NameStrCheck(string name, bool ExceptedSize = true)
        {
            if (name == null)
                return false;
            if (name == "")
                return false;
            string ValidChars = "[^A-Za-z0-9ء-ي*~.&.$]$";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(ValidChars);
            if (r.IsMatch(name))
                return false;

            if (name.Contains('/'))
                return false;
            if (name.Contains(@"\"))
                return false;
            if (name.Contains(@"'"))
                return false;
            if (name.Contains('#'))
                return false;
            if (name.Contains('~'))
                return false;
            if (name.Contains("GM") ||
                name.Contains("PM") ||
                name.Contains("None") ||
                name.Contains("none") ||
                name.Contains("SYSTEM") ||
                name.Contains("{") ||
                name.Contains("}") ||
                name.Contains("[") ||
                name.Contains("]"))
                return false;
            if (name.Length > 16 && ExceptedSize)
                return false;
            for (int x = 0; x < name.Length; x++)
                if (name[x] == 25)
                    return false;
            return true;
        }
        public static bool StringCheck(string pszString)
        {
            for (int x = 0; x < pszString.Length; x++)
            {
                if (pszString[x] > ' ' && pszString[x] <= '~')
                    return false;
            }
            return true;
        }
    }
}
