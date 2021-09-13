using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdysseyServer_Project.Game.MsgTournaments
{
    public class MsgMrConquer
    {
        public const ushort MapID = 9998;
        public const uint RewardConquerPoints = 20000;
        public const string FilleName = "\\MrConquer.ini";

        public enum TournamentType : byte
        {
            MrConquer = 0,            
            Count = 1
        }
        public enum TournamentLevel : byte
        {
            Level_1_137 = 0,           
            Count = 1
        }
        public class top_typ
        {
            public const byte
            MrConquer = 31;
        }

        public War[][] PkWars;
        public ProcesType Proces;

        public MsgMrConquer(ProcesType _Proces)
        {
            Proces = _Proces;

            PkWars = new War[(byte)TournamentType.Count][];

            for (TournamentType i = TournamentType.MrConquer; i < TournamentType.Count; i++)
            {
                PkWars[(byte)i] = new War[(byte)TournamentLevel.Count];

                for (TournamentLevel x = TournamentLevel.Level_1_137; x < TournamentLevel.Count; x++)
                {
                    PkWars[(byte)i][(byte)x] = new War(i, x, ProcesType.Dead);
                }
            }
        }
        internal void Start()
        {
            if (Proces == ProcesType.Dead)
            {
                for (TournamentType i = TournamentType.MrConquer; i < TournamentType.Count; i++)
                {
                    PkWars[(byte)i] = new War[(byte)TournamentLevel.Count];

                    for (TournamentLevel x = TournamentLevel.Level_1_137; x < TournamentLevel.Count; x++)
                    {
                        PkWars[(byte)i][(byte)x] = new War(i, x, ProcesType.Dead);
                    }
                }
                foreach (var tournament in PkWars)
                {
                    foreach (var war in tournament)
                    {
                        war.Start(Database.Server.ServerMaps[MapID]);
                    }
                }
                Proces = ProcesType.Idle;
            }
        }
        internal void Stop()
        {
            if (Proces != ProcesType.Dead)
            {
                foreach (var tournament in PkWars)
                {
                    foreach (var war in tournament)
                    {
                        war.Stop();
                    }
                }
                Proces = ProcesType.Dead;
            }
        }
       
        internal MsgServer.MsgUpdate.Flags ObtainAura(TournamentType typ)
        {
            switch (typ)
            {
                case TournamentType.MrConquer: return MsgServer.MsgUpdate.Flags.TopMrConquer;                
            }
            return MsgServer.MsgUpdate.Flags.Normal;
        }

        public War GetMyWar(Client.GameClient client)
        {
            foreach (var tournament in PkWars)
            {
                foreach (var war in tournament)
                {
                    if (war.DinamicID == client.Player.DynamicID)
                        return war;
                }
            }
            return null;
        }
        internal static TournamentType GetMyTournamentType(Client.GameClient client)
        {
            if (Role.Core.IsBoy(client.Player.Body))
                return TournamentType.MrConquer;            

            return TournamentType.Count;
        }
        internal static TournamentLevel GetMyTournamentLevel(Client.GameClient client)
        {
            if (client.Player.Level <= 137)
                return TournamentLevel.Level_1_137;           
            return TournamentLevel.Count;
        }
        internal ProcesType GetWar(Client.GameClient client, out War mywar)
        {
            var typ = GetMyTournamentType(client);
            var level = GetMyTournamentLevel(client);
            var tournament = PkWars[(byte)typ];
            foreach (var war in tournament)
            {
                if (war.Typ == typ && war.Level == level)
                {
                    mywar = war;
                    return war.Proces;
                }
            }
            mywar = null;
            return ProcesType.Dead;
        }
        internal void LoginClient(Client.GameClient client)
        {
            foreach (var tournament in PkWars)
            {
                foreach (var war in tournament)
                {
                    if (war.Winner == client.Player.UID)
                    {
                        client.Player.AddTitle(top_typ.MrConquer, true);
                    }
                }
            }
        }
        internal void Save()
        {
            Database.DBActions.Write writer = new Database.DBActions.Write(FilleName);
            foreach (var tournament in PkWars)
            {
                foreach (var war in tournament)
                {
                    Database.DBActions.WriteLine line = new Database.DBActions.WriteLine('/');
                    line.Add((byte)war.Typ).Add((byte)war.Level).Add(war.Winner).Add((int)war.LastFlag);
                    writer.Add(line.Close());
                }
            }
            writer.Execute(Database.DBActions.Mode.Open);
        }
        internal void Load()
        {
            Database.DBActions.Read reader = new Database.DBActions.Read(FilleName);
            if (reader.Reader())
            {
                for (int x = 0; x < reader.Count; x++)
                {
                    Database.DBActions.ReadLine line = new Database.DBActions.ReadLine(reader.ReadString(""), '/');
                    byte typ = line.Read((byte)0);
                    byte level = line.Read((byte)0);
                    uint Winner = line.Read((uint)0);
                    MsgServer.MsgUpdate.Flags LastFlag = (MsgServer.MsgUpdate.Flags)line.Read((int)0);

                    PkWars[typ][level].Winner = Winner;
                    PkWars[typ][level].LastFlag = LastFlag;
                }
            }
        }
        public class War
        {
            public uint Winner = 0;
            public byte ReceiveReward = 0;
            public MsgServer.MsgUpdate.Flags LastFlag = 0;

            public TournamentLevel Level;
            public TournamentType Typ;
            public ProcesType Proces;
            public uint DinamicID;
            public DateTime FinishTimer = new DateTime();
          
            public War(TournamentType _typ, TournamentLevel _level, ProcesType proces)
            {
                Level = _level;
                Proces = proces;
                Typ = _typ;
            }
            public void Start(Role.GameMap map)
            {
                if (Proces == ProcesType.Dead)
                {
                    

                    Proces = ProcesType.Alive;
                    FinishTimer = DateTime.Now.AddMinutes(1);
                    DinamicID = map.GenerateDynamicID();

                    foreach (var client in Database.Server.GamePoll.Values)
                    {
                        if (Role.Core.IsBoy(client.Player.Body))
                        {
                            client.Player.MessageBox("MrConquer PK War is about to begin! Will you join it? \n you only have 1 Minutes",
                                  new Action<Client.GameClient>(p => p.Teleport(296, 192, 1002)), null);
                        }
                    }
                }
            }
            internal void Stop()
            {
                Proces = ProcesType.Dead;
            }
            public bool CheckJoin()
            {
                if (Proces == ProcesType.Dead)
                    return false;
                if (DateTime.Now < FinishTimer)
                    return true;
                else
                {
                    Proces = ProcesType.Alive;
                    return false;
                }
            }
            public bool IsFinish(Client.GameClient client)
            {
                if (Proces == ProcesType.Alive)
                {
                    var arrayMap = client.Map.Values.Where(p => p.Player.DynamicID == DinamicID && p.Player.Alive).ToArray();
                    if (arrayMap.Length == 1)
                    {
                        return true;
                    }
                }
                return false;
            }
            public void GetMyReward(Client.GameClient client,ServerSockets.Packet stream)
            {
                if (IsFinish(client))
                {
                    Proces = ProcesType.Dead;

                    //Remove old winner -----------------------
                    foreach (var user in Database.Server.GamePoll.Values)
                    {
                        if (user.Player != null)
                        {
                            if (user.Player.UID == Winner)
                            {
                                user.Player.RemoveFlag(LastFlag);
                            }
                        }
                    }
                    //------------------------------------------

                    var aura = MsgSchedules.MrConquer.ObtainAura(Typ);
                    if (aura != MsgServer.MsgUpdate.Flags.Normal)
                        client.Player.AddTitle(top_typ.MrConquer, true);

                    client.Player.ConquerPoints += RewardConquerPoints;
                    client.Player.PVEPoints += 10;

                    client.SendSysMesage("You received " + RewardConquerPoints.ToString() + " ConquerPoints. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                    string reward = "[MrConquer]" + client.Player.Name + " has received " + RewardConquerPoints + " from class pk.";
                    Program.DiscordEventsAPI.Enqueue($"``{reward}``");
                    Database.ServerDatabase.LoginQueue.Enqueue(reward);
                    LastFlag = aura;
                    Winner = client.Player.UID;
                    MsgSchedules.SendSysMesage("" + client.Player.Name + " Won " + Typ.ToString() + " MrConquer, he received Top " + Typ.ToString() + ", " + RewardConquerPoints.ToString() + " ConquerPoints and  [10] PVE Points!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                    client.Teleport(430, 269, 1002, 0);
                }
            }
        }
    }
}
