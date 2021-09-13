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
                user.Teleport(196, 214, 1081);
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
                        p.Teleport(192, 223, 1036);
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
                    if (Winner.Inventory.HaveSpace(2))
                        Winner.Inventory.Add(stream, Database.ItemType.PowerExpBall, 4);
                    else
                        Winner.Inventory.AddReturnedItem(stream, Database.ItemType.PowerExpBall, 4);
#if Arabic
                                    Winner.SendSysMesage("You received " + RewardConquerPoints.ToString() + " ConquerPoints and 2PowerExpBalls. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                    
#else
                    Winner.SendSysMesage("You received 2 PowerExpBalls. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);

#endif
                    client.Player.SpeedHunterGamePoints = 0;

                }
            }

            client.Teleport(300, 278, 1002);
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
                    ShuffleGuildScores(stream);
                }
                if (StartTimer.AddMinutes(3) < DateTime.Now)
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
#if Arabic
                 Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("---Your Score: " + user.Player.CurrentTreasureBoxes + "---", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                
#else
                Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("---Your Score: " + user.Player.SpeedHunterGamePoints + "---", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);

#endif
                user.Send(msg.GetArray(stream));
            }
            var array = Map.Values.OrderByDescending(p => p.Player.eveSpeedHunterGamePoints).ToArray();
            for (int x = 0; x < Math.Min(10, Map.Values.Length); x++)
            {
                var element = array[x];
#if Arabic
                   Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + "- " + element.Player.Name + " Opened " + element.Player.CurrentTreasureBoxes.ToString() + " Boxes!", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
             
#else
                Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + "- " + element.Player.Name + " have " + element.Player.SpeedHunterGamePoints.ToString() + " Points!", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

#endif
                Send(msg.GetArray(stream));
                
            }

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
                if (count == 3)
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
#if Arabic
                         Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulation! Rank " + (x + 1).ToString() + " " + element.Name + " with " + element.SpeedHunterGame.ToString() + " SpeedHunterGamePoints. .", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
               
#else
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulation! Rank " + (x + 1).ToString() + " " + element.Name + " with " + element.SpeedHunterGame.ToString() + " SpeedHunterGame Points. .", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
               
#endif
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
