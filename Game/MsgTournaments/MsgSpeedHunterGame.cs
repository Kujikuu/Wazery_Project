using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightConquer_Project.Game.MsgServer;
namespace LightConquer_Project.Game.MsgTournaments
{
    public class Participant
    {
        public string Name = "";
        public uint UID = 0;
        public uint SpeedHunterGame;

        public Participant()
        {

        }

        public Participant(Client.GameClient user)
        {
            Name = user.Player.Name;
            UID = user.Player.UID;
            SpeedHunterGame = user.Player.SpeedHunterGamePoints;
        }
        public override string ToString()
        {
            Database.DBActions.WriteLine line = new Database.DBActions.WriteLine('/');
            line.Add(UID).Add(SpeedHunterGame).Add(Name);
            return line.Close();
        }
    }
    public class MsgSpeedHunterGame : ITournament
    {
        public uint SecondsTillEnd = 300;
        public ProcesType Process { get; set; }
        private DateTime StartTimer = new DateTime();
        public DateTime ScoreStamp = new DateTime();
        public Role.GameMap Map;

        public List<Participant> Rank3 = new List<Participant>();
        public TournamentType Type { get; set; }
        public MsgSpeedHunterGame(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
            StartTimer = DateTime.Now;
        }

        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Idle)
            {
                user.Player.SpeedHunterGamePoints = 0;
                user.Teleport(1081, 196, 214);
                user.SendSysMesage("Good luck hunting these monsters, you must be the fastest! To get out of here teleport or log out!");
                return true;
            }
            return false;
        }
        public void Open()
        {
            if (Process == ProcesType.Dead)
            {
                StartTimer = DateTime.Now;
                Process = ProcesType.Idle;
                Map = Database.Server.ServerMaps[1081];
                SecondsTillEnd = 300;
                foreach (var client in Database.Server.GamePoll.Values)
                {
                    client.Player.MessageBox(
#if Arabic
                        "SpeedHunterGame is about to begin! Will you join it?"
#else
"SpeedHunterGame is about to begin! Will you join it?"
#endif

                        , new Action<Client.GameClient>(p =>
                    {
                        p.Teleport(1036, 192, 223);
                    }), null, 60);
                }
            }

        }
        public bool InTournament(Client.GameClient user)
        {
            return
                user.Player.Map == 1081
                || user.Player.Map == 2060
                || user.Player.Map == 1080;
        }
        private List<Client.GameClient> Participants()
        {
            List<Client.GameClient> Participants = new List<Client.GameClient>();

            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.Map == 2060 || user.Player.Map == 1080 || user.Player.Map == 1081)
                {
                    if (user.Player.DynamicID == 0)
                        Participants.Add(user);
                }
            }
            return Participants;
        }
        public void GetOut(Client.GameClient client)
        {
            var array = MapPlayers().OrderByDescending(p => p.Player.eveSpeedHunterGamePoints).ToArray();
            if (array.Length > 0)
            {
                var Winner = array.First();

                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    //client.Player.SpeedHunterGamePoints = 0;

                    if(array[0] == client)
                    {
                        if (client.Inventory.HaveSpace(2))
                            client.Inventory.Add(stream, Database.ItemType.DragonBall, 1);
                        client.Player.Money += 6000000;
                        client.SendSysMesage("You received 6KK silvers and Dragonball. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                    }
                    else if (array[1] == client)
                    {
                        client.Player.Money += 5000000;
                        client.SendSysMesage("You received 5KK silvers. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                    }
                    else if (array[2] == client)
                    {
                        client.Player.Money += 4000000;
                        client.SendSysMesage("You received 4KK silvers. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                    }
                    else if (array[3] == client)
                    {
                        client.Player.Money += 3000000;
                        client.SendSysMesage("You received 3KK silvers. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                    }
                    else if (array[4] == client)
                    {
                        client.Player.Money += 2000000;
                        client.SendSysMesage("You received 2KK silvers. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                    }
                }
            }

            client.Teleport(1002, 430, 380);
        }

        public void CheckUp()
        {
            if (Process == ProcesType.Idle)
            {
                if (StartTimer.AddSeconds(5) < DateTime.Now)
                {
                    Process = ProcesType.Alive;
                    //using (var rec = new ServerSockets.RecycledPacket())
                    //{
                    //    var stream = rec.GetStream();
                    //    ShuffleGuildScores(stream);
                    //}
                    var map = Database.Server.ServerMaps[2060];
                    if (!map.ContainMobID(20300))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Database.Server.AddMapMonster(stream, map, 20300, 123, 129, 18, 18, 1);
#if Arabic
                                  Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("The NemesisTyrant have spawned in the SpeedHunterGame Map 3 on (123, 129) ! Hurry to kill them. Drop [SavageBone, DragonBalls].", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                      
#else
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("SpeedHunterGame is about to begin!.", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

#endif
                        }
                    }
                }

            }
            if (Process == ProcesType.Alive)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    SecondsTillEnd--;
                    ShuffleGuildScores(stream);
                }
                if (StartTimer.AddMinutes(5) < DateTime.Now)
                {
                    Process = ProcesType.Dead;

                    List<Client.GameClient> Users = Participants();
                    CreateRanks(Users);
                    foreach (var user in Users)
                        GetOut(user);

                }

            }

        }
        public void ShuffleGuildScores(ServerSockets.Packet stream)
        {
            foreach (var user in Map.Values)
            {
                Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("--SpeedHunterGame--", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                user.Send(msg.GetArray(stream));
            }
            var array = Map.Values.OrderByDescending(p => p.Player.eveSpeedHunterGamePoints).ToArray();
            for (int x = 0; x < Math.Min(5, Map.Values.Length); x++)
            {
                var element = array[x];
                Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + "- " + element.Player.Name + " / " + element.Player.SpeedHunterGamePoints.ToString() + " Points!", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                Send(msg.GetArray(stream));
            }
            Send(new MsgServer.MsgMessage("--------------------", MsgMessage.MsgColor.white, MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
            Send(new MsgServer.MsgMessage($"Time left: {0} Seconds!", MsgMessage.MsgColor.white, MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
        }
        public void Send(ServerSockets.Packet stream)
        {
            foreach (var user in Map.Values)
                user.Send(stream);
        }
        public void CreateRanks(List<Client.GameClient> Users)
        {
            Rank3.Clear();

            List<Participant> Rank = new List<Participant>();
            foreach (var user in Users)
                Rank.Add(new Participant(user));

            var array = Rank.OrderByDescending(p => p.SpeedHunterGame);

            int count = 0;
            foreach (var user in array)
            {
                if (count == 5)
                    break;
                Rank3.Add(user);
                count++;
            }

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                for (int x = 0; x < Rank3.Count; x++)
                {
                    var element = Rank3[x];
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("SpeedHunterGame has ended: TOP " + (x + 1).ToString() + " " + element.Name + " with " + element.SpeedHunterGame.ToString() + " Points has won.", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.Talk).GetArray(stream));
                }
            }
        }
        public void SendMapPacket(ServerSockets.Packet stream)
        {
            foreach (var user in MapPlayers())
                user.Send(stream);
        }
        public Client.GameClient[] MapPlayers()
        {
            return Map.Values.Where(p => InTournament(p)).ToArray();
        }
        public void Save()
        {
            using (Database.DBActions.Write write = new Database.DBActions.Write("SpeedHunterGame.txt"))
            {
                foreach (var user in Rank3)
                    write.Add(user.ToString());
                write.Execute(Database.DBActions.Mode.Open);
            }
        }
        public void Load()
        {
            using (Database.DBActions.Read reader = new Database.DBActions.Read("SpeedHunterGame.txt"))
            {
                if (reader.Reader())
                {
                    for (int x = 0; x < reader.Count; x++)
                    {
                        Participant part = new Participant();
                        Database.DBActions.ReadLine readline = new Database.DBActions.ReadLine(reader.ReadString(""), '/');
                        part.UID = readline.Read((uint)0);
                        part.SpeedHunterGame = readline.Read((uint)0);
                        part.Name = readline.Read("None");
                        Rank3.Add(part);
                    }
                }
            }
        }
    }
}
