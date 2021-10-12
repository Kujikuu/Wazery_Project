using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgTournaments
{
    public class MsgTreasureThief : ITournament
    {
        public const ushort
            MapID = 3820;
        public ProcesType Process { get; set; }
        public int CurrentBoxes = 0;
        public DateTime StartTimer = new DateTime();
        public DateTime BoxesStamp = new DateTime();
        public uint SecondsToEnd = 180;
        Role.GameMap _map;
        public Role.GameMap Map
        {
            get
            {
                if (_map == null)
                    _map = Database.Server.ServerMaps[MapID];
                return _map;
            }
        }
        public TournamentType Type { get; set; }
        public MsgTreasureThief(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
        }
        public bool InTournament(Client.GameClient user)
        {
            return user.Player.Map == MapID;
        }
        public void Open()
        {
            if (Process != ProcesType.Alive)
            {
                Create();
                foreach (var user in Database.Server.GamePoll.Values)
                    user.Player.CurrentTreasureBoxes = 0;
                Process = ProcesType.Alive;
                StartTimer = DateTime.Now.AddMinutes(3);
                BoxesStamp = DateTime.Now.AddSeconds(30);
                SecondsToEnd = 180;
                MsgSchedules.SendInvitation("TreasureThief", "ExpBall(Event),Money,DB,Meteor and others treasures", 460, 366, 1002, 0, 60);
            }
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Alive)
            {
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(MapID, x, y);
                return true;
            }
            return false;
        }
        private void Create()
        {
            GenerateBoxes();
        }
        private void GenerateBoxes()
        {
            for (int i = CurrentBoxes; i < 6; i++)
            {
                byte rand = (byte)Program.GetRandom.Next(0, 5);
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);

                Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                while (true)
                {
                    np.UID = (uint)Program.GetRandom.Next(10000, 100000);
                    if (Map.View.Contain(np.UID, x, y) == false)
                        break;
                }
                np.NpcType = Role.Flags.NpcType.Talker;
                switch (rand)
                {
                    case 0: np.Mesh = 9296; break;
                    case 1: np.Mesh = 9296; break;
                    case 2: np.Mesh = 9296; break;
                    case 3: np.Mesh = 9296; break;
                    case 4: np.Mesh = 9296; break;
                    default: np.Mesh = 9296; break;
                }
                np.Map = MapID;
                np.X = x;
                np.Y = y;
                Map.AddNpc(np);
            }
            CurrentBoxes = 6;
        }
        public void CheckUp()
        {
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer)
                {
                    MsgSchedules.SendSysMesage("TreasureThief has ended! Congratulations to the winners!", MsgServer.MsgMessage.ChatMode.Talk, MsgServer.MsgMessage.MsgColor.red);
                    var Map2 = Database.Server.ServerMaps[1767];
                    foreach (var user in Map.Values)
                    {
                        ushort x = 0;
                        ushort y = 0;
                        Map2.GetRandCoord(ref x, ref y);
                        user.Teleport(3954, x, y);
                    }
                    if (!Map2.ContainMobID(20060))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Database.Server.AddMapMonster(stream, Map2, 20060, 48, 38, 5, 5, 1, 0, true, MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                        }
                    }
                    Process = ProcesType.Dead;
                }
                else if (DateTime.Now > BoxesStamp)
                {
                    GenerateBoxes();
                    BoxesStamp = DateTime.Now.AddSeconds(30);
                }

                if (SecondsToEnd > 0)
                {
                    SecondsToEnd--;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        ShuffleGuildScores(stream);
                    }
                }
            }
        }
        public void Reward(Client.GameClient user, Game.MsgNpc.Npc npc, ServerSockets.Packet stream)
        {
            CurrentBoxes -= 1;
        jmp:
            byte rand = (byte)Program.GetRandom.Next(0, 5);
            switch (rand)
            {
                case 0://money
                    {
                        uint value = (uint)Program.GetRandom.Next(10000, 50000);
                        user.Player.Money += value;
                        user.Player.SendUpdate(stream, user.Player.Money, MsgServer.MsgUpdate.DataType.Money);
#if Arabic
                         user.CreateBoxDialog("You've received "+value+" Money.");
                        MsgSchedules.SendSysMesage(user.Player.Name + " got " + value.ToString() + " Money while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                       
#else
                        user.CreateBoxDialog("You've received " + value + " Money.");
                        MsgSchedules.SendSysMesage("[TreasureBOX] " + user.Player.Name + " got " + value.ToString() + " Money while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.TopLeft, MsgServer.MsgMessage.MsgColor.red);

#endif
                        break;
                    }
                case 1://experience
                    {
                        if (user.Player.Level == 140)
                            goto jmp;
                        user.GainExpBall(600 * 2, true, Role.Flags.ExperienceEffect.angelwing);
#if Arabic
                            MsgSchedules.SendSysMesage(user.Player.Name + " got 2xExpBalls while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                     
#else
                        MsgSchedules.SendSysMesage("[TreasureBOX] " + user.Player.Name + " got 2xExpBalls while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.TopLeft, MsgServer.MsgMessage.MsgColor.red);

#endif
                        break;
                    }
                case 2://cps
                    {
                        uint[] Items = new uint[]
                        {
                            Database.ItemType.OneStone,
                            Database.ItemType.OneStone+1,
                        };
                        uint ItemID = Items[Program.GetRandom.Next(0, Items.Length)];
                        Database.ItemType.DBItem DBItem;
                        if (Database.Server.ItemsBase.TryGetValue(ItemID, out DBItem))
                        {
                            if (user.Inventory.HaveSpace(1))
                                user.Inventory.Add(stream, DBItem.ID);
                            else
                                user.Inventory.AddReturnedItem(stream, DBItem.ID);
                            MsgSchedules.SendSysMesage("[TreasureBOX] " + user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.TopLeft, MsgServer.MsgMessage.MsgColor.red);
                        }
                        break;
                    }
                case 3://dead.
                    {
                        user.Player.Dead(null, user.Player.X, user.Player.Y, 0);
                        MsgSchedules.SendSysMesage("[TreasureBOX] " + user.Player.Name + " found DEATH! while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.TopLeft, MsgServer.MsgMessage.MsgColor.red);
                        break;
                    }
                case 4://item.
                    {
                        uint[] Items = new uint[]
                        {
                            Database.ItemType.DragonBall,
                            Database.ItemType.PowerExpBall,
                            711083,
                            Database.ItemType.MeteorScroll,
                            Database.ItemType.MeteorTearPacket,
                            Database.ItemType.Meteor,
                            Database.ItemType.DragonBall,
                            Database.ItemType.MoonBox,
                            Database.ItemType.DragonBall
                        };
                        uint ItemID = Items[Program.GetRandom.Next(0, Items.Length)];
                        Database.ItemType.DBItem DBItem;
                        if (Database.Server.ItemsBase.TryGetValue(ItemID, out DBItem))
                        {
                            if (user.Inventory.HaveSpace(1))
                                user.Inventory.Add(stream, DBItem.ID);
                            else
                                user.Inventory.AddReturnedItem(stream, DBItem.ID);
                            MsgSchedules.SendSysMesage("[TreasureBOX] " + user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.TopLeft, MsgServer.MsgMessage.MsgColor.red);
                        }
                        break;
                    }

            }
            user.Player.CurrentTreasureBoxes += 1;
            user.Player.SendString(stream, MsgServer.MsgStringPacket.StringID.Effect, true, "accession1");
            Map.RemoveNpc(npc, stream);

            //ShuffleGuildScores(stream);

        }
        public void ShuffleGuildScores(ServerSockets.Packet stream)
        {
            foreach (var user in Map.Values)
            {
                Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("--TreasureThief--", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                user.Send(msg.GetArray(stream));
                Game.MsgServer.MsgMessage msg2 = new MsgServer.MsgMessage($"TimeLeft: {SecondsToEnd} Seconds!", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                user.Send(msg2.GetArray(stream));
            }
        }
        public void Send(ServerSockets.Packet stream)
        {
            foreach (var user in Map.Values)
                user.Send(stream);
        }
    }
}
