using LightConquer_Project.Client;
using LightConquer_Project.Game.MsgNpc;
using LightConquer_Project.Game.MsgServer;
using LightConquer_Project.Role;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LightConquer_Project.Game.MsgServer.MsgMessage;
using static LightConquer_Project.Role.Flags;

namespace LightConquer_Project.Game.MsgEvents
{
    public enum EventStage
    {
        None,
        Inviting,
        Countdown,
        Starting,
        Fighting,
        Over
    }
    public enum BroadCastLoc
    {
        World,
        Map,
        Score,
        Title,
        SpecificMap,
        YellowMap,
        WorldY
    }
    public class Events
    {
        #region Properties
        public string EventTitle = "Base Event";
        public DateTime LastScores;
        public bool isTerr = false;
        //public bool Bomb = false;
        public bool IsCapitan = false;
        public bool DW = false;
        public EventStage Stage = EventStage.None;
        Extensions.Time32 timer = Extensions.Time32.Now;
        public Role.GameMap Map;
        public bool NoDamage = false;
        public ushort BaseMap = 700;
        public uint MapEvent = 11000;
        public uint skillscore = 1000;
        public uint DinamicID = 0;
        public bool FFADamage = false;
        public uint _Duration = 0;
        public bool Reflect = false;
        public ushort X = 0;
        public ushort Y = 0;
        public bool MagicAllowed = true;
        public bool ReviveAllowed = true;
        public bool MeleeAllowed = true;
        public bool PotionsAllowed = true;
        public bool ReviveTele = false;
        public bool FriendlyFire = false;
        public Dictionary<uint, GameClient> PlayerList = new Dictionary<uint, GameClient>();
        public readonly Dictionary<uint, int> PlayerScores = new Dictionary<uint, int>();
        public Dictionary<uint, Dictionary<uint, GameClient>> Teams = new Dictionary<uint, Dictionary<uint, GameClient>>();
        public List<ushort> AllowedSkills = new List<ushort>();
        private byte minplayers = 2;
        public int CountDown;
        public int DialogID = 0;
        public double Duration = 20;
        public DateTime EndTime;
        public DateTime DisplayScores;
        public static ArrayList IPs = new ArrayList();

        #endregion
        /// <summary>
        /// Used to send messages related to the current PVP Event
        /// </summary>
        /// 
        public void Broadcast(string msg, BroadCastLoc loc, uint Map = 0)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                switch (loc)
                {
                    case BroadCastLoc.World:
                        Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, MsgColor.white, ChatMode.BroadcastMessage).GetArray(stream));
                        break;
                    case BroadCastLoc.WorldY:
                        Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, MsgColor.white, ChatMode.TopLeft).GetArray(stream));
                        break;
                    case BroadCastLoc.Map:
                        foreach (GameClient client in PlayerList.Values.ToList())
                        {
                            client.SendSysMesage(msg);
                        }
                        break;
                    case BroadCastLoc.YellowMap:
                        foreach (GameClient client in PlayerList.Values.ToList())
                        {
                            client.SendSysMesage(msg, ChatMode.TopLeft, MsgColor.yellow);
                        }
                        break;
                    case BroadCastLoc.Title:
                        foreach (GameClient client in PlayerList.Values.ToList())
                        {
                            client.SendSysMesage(msg, ChatMode.FirstRightCorner);
                        }
                        break;
                    case BroadCastLoc.Score:
                        foreach (GameClient client in PlayerList.Values.ToList())
                        {
                            client.SendSysMesage(msg, ChatMode.ContinueRightCorner);
                        }
                        break;
                    case BroadCastLoc.SpecificMap:
                        foreach (GameClient client in PlayerList.Values.ToList())
                        {
                            if (client.Player.Map == Map)
                                client.SendSysMesage(msg, ChatMode.ContinueRightCorner);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Adds a player to the current PVP Event
        /// </summary>
        /// <param name="c"></param>
        public virtual bool AddPlayer(GameClient client)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                if (Stage == EventStage.Inviting)
                {
                    if (!GameMap.EventMaps.ContainsKey(client.Player.Map))
                    {
                        //if (!IPs.Contains(IP))
                        //{
                        //    IPs.Add(IP);

                            if (!client.InQualifier())
                        {
                            if (client.Player.Map != 1038 && client.Player.Map != 6001 && client.Player.Map != 6003 && client.Player.Map != 1036)
                            {
                                if (!PlayerList.ContainsKey(client.Player.UID))
                                {
                                    //Map.GetRandCoord(ref X, ref Y);
                                    client.Player.PMap = client.Player.Map;
                                    client.Player.PMapX = client.Player.X;
                                    client.Player.PMapY = client.Player.Y;
                                    client.Teleport(1616, 53, 65);
                                    PlayerList.Add(client.Player.UID, client);
                                    PlayerScores.Add(client.Player.UID, 0);
                                    //client.EventBase = Program.Events[0];
                                    client.SendSysMesage($"Just wait 60 Seconds and we gonna send you to the event map automatically");
                                    if (!client.Player.Alive)
                                        client.Player.Revive(stream);
                                    return true;
                                }
                                else
                                    client.SendSysMesage($"You can't join a PVP event while you're in {EventTitle} Event!");
                            }
                            else
                                client.SendSysMesage("You can't join a PVP event while you're in guild war.");
                            }
                            else
                                client.SendSysMesage("You can't join a PVP Event while you're fighting at the Arena Qualifier!");
                        //}
                        //else
                        //    client.SendSysMesage("You can't join a PVP Event while you're other account in event!");
                    }
                    else
                        client.SendSysMesage("You can't join a PVP Event while you're fighting at other event!");
                }
                else
                    client.SendSysMesage("There are no events running");
            }
            return false;
        }

        /// <summary>
        /// Removes a player from the current PVP Event
        /// </summary>
        /// 
        public virtual void RemovePlayer(GameClient client)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                PlayerList.Remove(client.Player.UID);
                PlayerScores.Remove(client.Player.UID);

                
                foreach (KeyValuePair<uint, Dictionary<uint, GameClient>> T in Teams)
                    if (T.Value.ContainsKey(client.Player.UID))
                        T.Value.Remove(client.Player.UID);

                client.EventBase = null;
                client.Player.Revive(stream);

                client.Teleport(1002, 428, 378, 0, true, true);
                //client.TeleportCallBack();

                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Backfire))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Backfire);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Freeze);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Top4Weekly))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Top4Weekly);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.GodlyShield);
               ChangePKMode(client, PKMode.Capture);
                client.SendSysMesage("", ChatMode.FirstRightCorner);
                try { client.Player.RemoveSpecialGarment(stream); } catch { }
            }
        }

        /// <summary>
        /// Used to teleport players to event map
        /// </summary>
        public virtual void TeleportPlayersToMap()
        {
            foreach (GameClient client in PlayerList.Values)
            {
                ChangePKMode(client, PKMode.PK);
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                client.Teleport(Map.ID, x, y, DinamicID, true, true);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Cyclone))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Cyclone);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Superman))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Superman);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.GodlyShield))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.GodlyShield);
                if (client.Player.ContainFlag(MsgServer.MsgUpdate.Flags.Top4Weekly))
                    client.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Top4Weekly);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
            }
        }
        /// <summary>
        /// Handles the logic for event protection countdown
        /// </summary>
        /// 

        public virtual void ChechMoveFlag(Client.GameClient user)
        {
            if (Stage != EventStage.Fighting)
                return;
            if (!user.Player.ContainFlag(MsgUpdate.Flags.CTF_Flag))
            {
                foreach (var flag in user.Map.View.Roles(Role.MapObjectType.StaticRole, user.Player.X, user.Player.Y))
                {
                    if (Role.Core.GetDistance(user.Player.X, user.Player.Y, flag.X, flag.Y) < 2)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            user.Player.AddFlag(MsgServer.MsgUpdate.Flags.CTF_Flag, 60, true);

                            stream.CaptureTheFlagUpdateCreate(MsgCaptureTheFlagUpdate.Mode.GenerateTimer, 60, user.Player.UID);
                            stream.CaptureTheFlagUpdateFinalize();
                            user.Send(stream);

                            stream.CaptureTheFlagUpdateCreate(MsgCaptureTheFlagUpdate.Mode.GenerateEffect, user.Player.UID);
                            stream.CaptureTheFlagUpdateFinalize();
                            user.Send(stream);

                            user.Map.View.LeaveMap<Role.IMapObj>(flag);

                            ActionQuery action;

                            action = new ActionQuery()
                            {
                                ObjId = flag.UID,
                                Type = ActionType.RemoveEntity
                            };
                            unsafe
                            {
                                user.Player.View.SendView(stream.ActionCreate(&action), true);
                            }

                        }
                        break;
                    }
                }
            }
        }

        public virtual void Countdown()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                if (CountDown == 0 && Stage == EventStage.Countdown)
                {
                    CountDown = 5;
                    Stage = EventStage.Starting;
                    Broadcast(EventTitle + " Event is about to start!", BroadCastLoc.Map);
                }
                else if (CountDown > 0)
                {
                    foreach (GameClient client in PlayerList.Values)
                    {
                        client.Player.SendString(stream, MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { $"downnumber{CountDown}" });
                        client.SendSysMesage("", ChatMode.FirstRightCorner);
                    }
                    CountDown--;
                }
                else
                {
                    foreach (GameClient client in PlayerList.Values)
                        if (!client.Player.Alive)
                            client.Player.Revive(stream);
                    Stage = EventStage.Fighting;
                    EndTime = DateTime.Now.AddMinutes(Duration);
                    Broadcast(EventTitle + " has started! signup are now closed.", BroadCastLoc.World);
                }
            }
        }

        /// <summary>
        /// Handles all the logic during the events and determines conditions to find a winner
        /// </summary>
        public virtual void WaitForWinner()
        {
            if (DateTime.Now >= EndTime || PlayerList.Count == 1 || _Duration <= 0)
                Finish();
            if (DateTime.Now >= LastScores.AddMilliseconds(3000))
                DisplayScore();
        }

        public virtual void TeleAfterRev(GameClient client)
        {
            client.Teleport(Map.ID, 0, 0, DinamicID);
        }

        /// <summary>
        /// Handles all the character related checks during the event
        /// </summary>
        /// <param name="C"></param>
        public virtual void CharacterChecks(GameClient client)
        {
            if (client.Player.Map != Map.ID)
                RemovePlayer(client);
        }

        /// <summary>
        /// Announces the event end. Gives each player a small protection and changes stage to over
        /// </summary>
        public void Finish()
        {
            Stage = EventStage.Over;

        }

        /// <summary>
        /// Here we choose who we want to reward and such, may depend on teams or w/e... Should add support for teams
        /// </summary>
        public virtual void End()
        {
            if (PlayerList.Count == 1)
                foreach (var client in PlayerList.Values.ToList())
                    Reward(client);
            else
                Broadcast(Duration + " minutes have passed and no one won the " + EventTitle + " Event! Better luck next time!", BroadCastLoc.World);

            foreach (var client in PlayerList.Values.ToList())
                RemovePlayer(client);
            PlayerList.Clear();
            PlayerScores.Clear();
            return;
        }

        /// <summary>
        /// Used to choose which rewards we want to give
        /// </summary>
        public virtual void Reward(GameClient client)
        {

            //bool ThunderGem = false;
            //bool GloryGem = false;
            //bool ChiPack500Points = false;
            //bool SoulTokenFragment = false;
            //bool RefinedTortoiseGem = false;
            //bool ChiPack250Points = false;
            //bool TortoiseGemFragment = false;
            //bool Stone = false;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                //client.Inventory.Add(stream, 3007287, 1);//+6Stone
                client.Player.PVPPoints += 10;
                Broadcast($"{EventTitle} has ended! {client.Name}  wins!", BroadCastLoc.YellowMap);

            }

            if (client.Player.Level < 135)
                client.GainExpBall(100, false, Role.Flags.ExperienceEffect.angelwing);
        }


        /// <summary>
        /// Display the score on the top right corner
        /// </summary>
        public virtual void DisplayScore()
        {
            DisplayScores = DateTime.Now;
            foreach (var player in PlayerList.Values)
            {
                player.SendSysMesage($"---------{EventTitle}---------", ChatMode.FirstRightCorner);
            }

            byte Score = 2;
            foreach (var kvp in PlayerScores.OrderByDescending((s => s.Value)))
            {
                if (Score == 7)
                    break;
                if (Score == PlayerScores.Count + 2)
                    break;
                Broadcast($"Nº {Score - 1}: {PlayerList[kvp.Key].Player.Name} - {kvp.Value}", BroadCastLoc.Score, Score);
                Score++;
            }
            //TimeSpan T = TimeSpan.FromSeconds(_Duration);
            //Broadcast($"Time left {T.ToString(@"mm\:ss")}", BroadCastLoc.Score);
            //if (_Duration > 0)
            //    --_Duration;
        }

        /// <summary>
        /// Determines if we're supposed to do something when a player gets killed
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="Victim"></param>
        public virtual void Kill(GameClient Attacker, GameClient Victim)
        {
            if (PlayerScores.ContainsKey(Attacker.Player.UID))
                PlayerScores[Attacker.Player.UID]++;
        }

        /// <summary>
        /// Determines if we're supposed to do something when a NPC gets killed
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="Victim"></param>
        public virtual void Kill(GameClient Attacker, SobNpc Victim)
        {
            if (PlayerScores.ContainsKey(Attacker.Player.UID))
                PlayerScores[Attacker.Player.UID]++;
        }

        /// <summary>
        /// Determines wether if an event has unlimited stamina or not and allow us to track number of sent FB/SS
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="SU"></param>
        public virtual void Shot(GameClient Attacker, Database.MagicType.Magic SU)
        {
            Attacker.Player.Stamina -= SU.UseStamina;
        }

        /// <summary>
        /// Determines what we're supposed to do when a player gets hit
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="Victim"></param>
        public virtual void Hit(GameClient Attacker, GameClient Victim)
        {

        }

        public virtual void Hit(GameClient client, SobNpc npc, uint Damage)
        {

        }

        /// <summary>
        /// Overrides the melee damage dealt
        /// </summary>
        /// <param name="User"></param>
        /// <param name="C"></param>
        /// <param name="AttackType"></param>
        /// <returns></returns>
        public virtual uint GetDamage(GameClient User, GameClient C)//, AttackType AttackType
        {
            return 1;
        }

        /// <summary>
        /// Sets events' configuration and starts it, possible to determine how long we want signup to last
        /// </summary>
        /// <param name="_signuptime"></param>
        public void StartTournament(int _signuptime = 60)
        {
            PlayerList.Clear();
            PlayerScores.Clear();
            CountDown = _signuptime;
            Stage = EventStage.Inviting;
            Program.Events.Add(this);
            if (Map == null)
            {
                if (this.EventTitle == "Class Pk Tournament")
                    Map = Database.Server.ServerMaps[(uint)Program.GetRandom.Next(1730, 1738)];
                else
                    Map = Database.Server.ServerMaps[this.BaseMap];
            }
            DinamicID = Map.GenerateDynamicID();
            BeginTournament();
        }

        /// <summary>
        /// Initializes tournament
        /// </summary>
        public virtual void BeginTournament()
        {
            Broadcast($"[PVPEVENTS]The sign up for {EventTitle} has been enebled. Type @joinpvp to sign up.", BroadCastLoc.World);
            SendInvitation(60);
        }

        /// <summary>
        /// Tells the server which part of the PVP Event we want to execute next
        /// </summary>
        public void ActionHandler()
        {
            if (Stage == EventStage.Inviting)
            {
                Inviting();
            }
            else if (Stage == EventStage.Countdown || Stage == EventStage.Starting)
                Countdown();
            else if (Stage == EventStage.Fighting)
                WaitForWinner();
            else if (Stage == EventStage.Over)
            {
                Stage = EventStage.None;
                End();
                Program.Events.Remove(this);
            }
        }

        /// <summary>
        /// Handles the logic while the event is on sign-up
        /// </summary>
        public virtual void Inviting()
        {
            if (CountDown > 0)
            {
                if (CountDown == 120)
                    Broadcast(EventTitle + " Event will start in 2 minutes!", BroadCastLoc.World);
                else if (CountDown == 30)
                    Broadcast(EventTitle + " Fight starts in 30 secounds!", BroadCastLoc.World);

                if (!CanStart() && CountDown == 10)
                {
                    Broadcast("[" + EventTitle + "] has ended and there is no winner", BroadCastLoc.WorldY);
                    Broadcast($"---------{EventTitle}---------", BroadCastLoc.Title, 0);
                    Broadcast("Event cancelled", BroadCastLoc.Score, 2);
                    foreach (GameClient client in PlayerList.Values)
                    {
                        if (client.Player.PMapX <= 0 || client.Player.PMapX >= 1400 || client.Player.PMapY <= 0 || client.Player.PMapY >= 3000)
                            client.Teleport(1002, 300, 280, 0, true, true);
                        else if (client.Player.PMap == 1038 || client.Player.PMap == 1616 || client.Player.PMap == 700 || Program.EventsMaps.Contains(client.Player.Map))
                            client.Teleport(1002, 300, 280, 0, true, true);
                        else
                                    client.Teleport(client.Player.PMap, client.Player.PMapX, client.Player.PMapY, 0, true, true);
                        client.EventBase = null;
                    }
                    PlayerList.Clear();
                    Stage = EventStage.None;
                    Program.Events.Remove(this);
                    return;
                }               
                else if (CountDown < 6)
                    Broadcast(CountDown.ToString() + " seconds until start", BroadCastLoc.YellowMap);
                Broadcast($"---------{EventTitle}---------", BroadCastLoc.Title, 0);
                TimeSpan T = TimeSpan.FromSeconds(CountDown);
                Broadcast($"Start in: {T.ToString(@"mm\:ss")}", BroadCastLoc.Score, 2);
                --CountDown;
            }
            else
            {
                Stage = EventStage.Countdown;
                TeleportPlayersToMap();
            }

        }
        public virtual void AddPlayerTitle(GameClient client)
        {
            if (client.EventBase == null)
            {
                var events = Program.Events.Find(x => x.EventTitle == EventTitle);
                client.EventBase = events;
                AddPlayer(client);
            }
        }

        public virtual void SendInvitation(int Secounds, Game.MsgServer.MsgStaticMessage.Messages messaj = Game.MsgServer.MsgStaticMessage.Messages.None)
        {
            string Message = $"The {EventTitle} is about to start !\nReady to sign up within 60 seconds ?";
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                foreach (var client in Database.Server.GamePoll.Values)
                {
                    if (!client.Player.OnMyOwnServer || client.IsConnectedInterServer())
                        continue;
                    if (client.Player.Map == 1616 || client.Player.Map == Map.ID)
                        continue;
                    if (this.EventTitle == "DragonWar")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.DragonWar);
                    }
                   else if (this.EventTitle == "FFA Tournament")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.FFa);
                    }
                    else if(this.EventTitle == "Freeze War")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.FreezeWar);
                    }
                    else if (this.EventTitle == "SS/FB Tournament")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.SSFBTournament);
                    }
                    else if (this.EventTitle == "Five(n)Out")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.FiveNOut);
                    }
                    else if (this.EventTitle == "Snow & Sunshine Infection")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.Infection);
                    }
                    else if (this.EventTitle == "Kill The Capitan")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.KillTheCaptain);
                    }
                    else if (this.EventTitle == "King Of The Hill")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.KOTH);
                    }
                    else if (this.EventTitle == "Skill Ladder")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.LadderTournament);
                    }
                    else if (this.EventTitle == "Last Man Standing")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.LastManStand);
                    }
                    else if (this.EventTitle == "Pass the Bomb")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.PassTheBomb);
                    }
                    else if (this.EventTitle == "Skill Championship")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.SkillChampionship);
                    }
                    else if (this.EventTitle == "Skill Master")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.skillmaster);
                    }
                    else if (this.EventTitle == "Kungfu School")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.Spacelnvasion);
                    }
                    else if (this.EventTitle == "Team Deathmatch")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.TDM);
                    }
                    else if (this.EventTitle == "4 Team DeathMatch")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.TDMFour);
                    }
                    else if (this.EventTitle == "Infection War")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.VampireWar);
                    }
                    else if (this.EventTitle == "Whack The Thief")
                    {
                        client.Player.MessageBox("", new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.WhackTheThief);
                    }
                    else
                        client.Player.MessageBox(Message, new Action<Client.GameClient>(user => AddPlayerTitle(user)), null, 60, MsgServer.MsgStaticMessage.Messages.None);

                }
            }
        }

        public bool InTournament(GameClient user)
        {
            if (Map == null)
                return false;
            return user.Player.Map == Map.ID && user.Player.DynamicID == DinamicID;
        }

        public void RevivePlayer(GameClient client, int amount = 4)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                if (client.Player.DeadStamp.AddSeconds(amount) < timer)
                {
                    ushort x = 0; ushort y = 0;
                    client.Map.GetRandCoord(ref x, ref y);
                    client.Teleport(Map.ID, x, y, DinamicID);
                    client.Player.Revive(stream);
                }
            }
        }

        /// <summary>
        /// Do all the requirement checks to start the event in here
        /// </summary>
        /// <returns></returns>
        public virtual bool CanStart()
        {
            return PlayerList.Count >= minplayers;
        }

        /// <summary>
        /// Chane PK Mode
        /// </summary>
        /// 
        public void ChangePKMode(GameClient client, PKMode Mode)
        {
            client.Player.SetPkMode(Mode);
        }
    }
}
