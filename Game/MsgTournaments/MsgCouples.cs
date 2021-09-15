﻿using LightConquer_Project.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgTournaments
{
    public class MsgCouples
    {

        public const uint RewardConquerPoints = 50000000;
        public ProcesType Process { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        public uint Seconds = 60;
        public Role.GameMap Map;
        public uint DinamicMap = 0;
        public KillerSystem KillSystem;
        public MsgCouples()
        {
            Process = ProcesType.Dead;
        }

        public void Open()
        {
            if (Process == ProcesType.Dead)
            {
                KillSystem = new KillerSystem();
                StartTimer = DateTime.Now;
                foreach (var client in Database.Server.GamePoll.Values)
                {
                    client.Player.MessageBox("", new Action<Client.GameClient>(p => p.Teleport(1002, 312, 194, 0)), null, 60, MsgServer.MsgStaticMessage.Messages.Couple);

                }
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
                bool canJoin = false;
                if (user.Team != null && user.Team.Members.Count == 2)
                {
                    var teammates = user.Team.GetMembers().ToList();
                    if (teammates[0].Player.Spouse == teammates[1].Player.Name)
                        canJoin = true;
                }
                if (!canJoin)
                {
                    user.SendSysMesage("this event for Couples Only");
                    return false;
                }
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                var teammates2 = user.Team.GetMembers().ToList();
                if (teammates2[0].Player.Spouse == teammates2[1].Player.Name)
                {
                    teammates2[0].Teleport(Map.ID, x, y, DinamicMap);
                    teammates2[1].Teleport(Map.ID, x, y, DinamicMap);
                }
                return true;
            }
            return false;
        }
        public string Winner1 = "", Winner2 = "";
        public void CheckUp()
        {
            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(1))
                {
                    MsgSchedules.SendSysMesage("[Couple PK Tournament] Event has started! May the best player win!", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    Process = ProcesType.Alive;
                    StartTimer = DateTime.Now;
                }
                else if (DateTime.Now > InfoTimer.AddSeconds(10))
                {
                    Seconds -= 10;

                    MsgSchedules.SendSysMesage("[Couple PK Tournament] Event is about to start in " + Seconds.ToString() + " Seconds.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    InfoTimer = DateTime.Now;
                }
            }
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddMinutes(2))
                {
                    foreach (var user in MapPlayers())
                    {
                        user.Teleport(1002, 428, 378);
                    }
                    MsgSchedules.SendSysMesage("Couple PK Tournament has ended. All Players of Tournament has teleported to TwinCity.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    Process = ProcesType.Dead;
                }
                var players = MapPlayers();

                if (players.Length == 1 || players.Length == 2)
                {
                    bool claim = false;
                    if (players.Length == 2)
                    {
                        var p1 = players[0];
                        var p2 = players[1];
                        if (p1.Player.Spouse == p2.Player.Name)
                            claim = true;
                    }
                    else if (players.Length == 1)
                        claim = true;
                    if (claim)
                    {
                        Process = ProcesType.Dead;

                        var winner = MapPlayers().First();

                        MsgSchedules.SendSysMesage("" + winner.Player.Name + " has won the Couple PK Tournament, they received " + RewardConquerPoints.ToString() + " Silvers.", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);

                        winner.Player.Money += RewardConquerPoints;
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            winner.Player.Money += 1000000;
                            winner.Player.SendUpdate(stream, winner.Player.Money, MsgUpdate.DataType.Money);
                        }
                        string reward = "[EVENT]" + winner.Player.Name + " has received " + RewardConquerPoints + " from Couple PK Tournament .";
                        Database.ServerDatabase.LoginQueue.Enqueue(reward);


                        winner.SendSysMesage("You received " + RewardConquerPoints.ToString() + " Silvers. ", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                        foreach (var player in MapPlayers())
                            player.Teleport(1002, 428, 378, 0);
                        Winner1 = winner.Player.Name;
                        Winner2 = winner.Player.Spouse;


                        winner.Player.AddFlag(MsgUpdate.Flags.TopSpouse, Role.StatusFlagsBigVector32.PermanentFlag, false);

                        var spouse = Database.Server.GamePoll.Values.Where(e => e.Player.Name == winner.Player.Spouse).FirstOrDefault();
                        if (spouse != null)
                            spouse.Player.AddFlag(MsgUpdate.Flags.TopSpouse, Role.StatusFlagsBigVector32.PermanentFlag, false);
                        Save();
                    }
                }

                Extensions.Time32 Timer = Extensions.Time32.Now;
                foreach (var user in MapPlayers())
                {
                    if (user.Player.Alive == false)
                    {
                        if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                            user.Teleport(1002, 428, 378);
                    }
                }
            }


        }
        public const string FilleName = "\\CouplesPK.ini";

        internal void Save()
        {
            Database.DBActions.Write writer = new Database.DBActions.Write(FilleName);
            Database.DBActions.WriteLine line = new Database.DBActions.WriteLine('/');
            line.Add(Winner1).Add(Winner2);
            writer.Add(line.Close());
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
                    Winner1 = line.Read("NONE");
                    Winner2 = line.Read("NONE");
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
