using LightConquer_Project.Client;
using LightConquer_Project.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LightConquer_Project.Game.MsgServer.MsgMessage;
using static LightConquer_Project.Role.Flags;

namespace LightConquer_Project.Game
{
    public class ArenaDuel
    {
        public enum DuelType
        {
            Standard,
            Leech,
            UnlimitedStamina
        }
        public enum Opponent
        {
            Single,
            Team
        }
        public enum Hits
        {
            Ten,
            Hundred
        }
        public enum BroadCastLoc
        {
            World,
            Map,
            Score,
            Title
        }
        public List<ushort> AllowedSkills = new List<ushort> { (ushort)1045, (ushort)1046, (ushort)1047 };
        public uint MapID = 700;
        public bool Wager = false;
        public uint WagerAmount = 0;
        public DuelType Type;
        public Opponent Against;
        public Hits Count;
        public ushort x = 0;
        public ushort y = 0;
        public uint Inviter = 0;
        public uint DinamicID = 0;
        public static Role.GameMap Map;
        public Dictionary<uint, GameClient> PlayerList = new Dictionary<uint, GameClient>();
        public Dictionary<uint, GameClient> TeamOne;
        public Dictionary<uint, GameClient> TeamTwo;

        /// <summary>
        /// Used to send messages related to the current Duel
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="loc"></param>
        /// <param name="index"></param>
        /// 
        public void Broadcast(string msg, BroadCastLoc loc, uint Map = 0)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                switch (loc)
                {
                    case BroadCastLoc.World:
                        Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg, MsgColor.white, ChatMode.TopLeft).GetArray(stream));
                        break;
                    case BroadCastLoc.Map:
                        foreach (GameClient client in PlayerList.Values.ToList())
                        {
                            client.SendSysMesage(msg);
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
                }
            }
        }

        /// <summary>
        /// Used to Accept duels received by players
        /// </summary>
        /// <param name="C"></param>
        /// <param name="C2"></param>
        /// 
        public void AcceptDuel(GameClient C, GameClient C2)
        {
            if (C.EventBase == null && !C.InQualifier() || C2.EventBase == null && !C2.InQualifier())
            {
                if (C2.Player.Map == C.Player.Map)
                {
                    PlayerList.Add(C.EntityID, C);
                    PlayerList.Add(C2.EntityID, C2);
                    C.Dueler = C2.EntityID;
                    C2.Dueler = C.EntityID;

                    if (Wager)
                    {
                        if (C.Player.ConquerPoints >= WagerAmount && C2.Player.ConquerPoints >= WagerAmount)
                        {
                            C.Player.ConquerPoints -= WagerAmount;
                            C2.Player.ConquerPoints -= WagerAmount;
                        }
                        else
                        {
                            Broadcast("One of the parties doesn't have enough gold to start the duel!", BroadCastLoc.Map, 2000);
                            C.Arena = null;
                            C2.Arena = null;
                            return;
                        }
                    }
                    if (Against == Opponent.Team)
                    {
                        if (C.Team != null && C.Team.Leader.Player.UID == C.EntityID && C.Team.Members.Count <= 3 && C2.Team != null && C2.Team.Leader.Player.UID == C2.EntityID && C2.Team.Members.Count <= 3)
                        {
                            TeamOne = new Dictionary<uint, GameClient>();
                            TeamTwo = new Dictionary<uint, GameClient>();
                            foreach (GameClient C3 in C.Team.GetMembers())
                            {
                                if (!PlayerList.ContainsKey(C3.EntityID))
                                {
                                    C3.Arena = this;
                                    C3.Dueler = C.Dueler;
                                    PlayerList.Add(C3.EntityID, C3);
                                }
                                TeamOne.Add(C3.EntityID, C3);
                            }
                            foreach (GameClient C3 in C2.Team.GetMembers())
                            {
                                if (!PlayerList.ContainsKey(C3.EntityID))
                                {
                                    C3.Arena = this;
                                    C3.Dueler = C2.Dueler;
                                    PlayerList.Add(C3.EntityID, C3);
                                }
                                TeamTwo.Add(C3.EntityID, C3);
                            }
                        }
                        else
                        {
                            Broadcast("One of the parties didn't have 3 or less members in their teams or the player invited wasn't the team leader!", BroadCastLoc.Map, 2000);
                            C.Arena = null;
                            C2.Arena = null;
                            return;
                        }
                    }

                    Initialize();
                }
                else
                {
                    Broadcast("You and your opponent were not in the same map!", BroadCastLoc.Map, 2000);
                    C.Arena = null;
                    C2.Arena = null;
                }
            }
            else
            {
                Broadcast("Either you or your opponent are in a PVP Event or dueling at the Arena Qualifier!", BroadCastLoc.Map, 2000);
                C.Arena = null;
                C2.Arena = null;
            }
        }

        /// <summary>
        /// Once the duel is accepted by both players, map is created and duel starts
        /// </summary>
        public void Initialize()
        {
            Map = Database.Server.ServerMaps[700];
            DinamicID = Map.GenerateDynamicID();
            //GenerateDinamicId
            foreach (GameClient C in PlayerList.Values.ToList())
            {
                ChangePKMode(C, PKMode.PK);
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                C.Teleport(x, y, Map.ID, DinamicID);
                //using (var rec = new ServerSockets.RecycledPacket())
                //{
                //    var stream = rec.GetStream();
                //    C.Send(stream.ArenaSignupCreate(MsgArenaSignup.DialogType.StartTheFight, MsgArenaSignup.DialogButton.SignUp, C));
                //    C.Send(stream.ArenaSignupCreate(MsgArenaSignup.DialogType.Match, MsgArenaSignup.DialogButton.MatchOn, C));
                //}
                C.SendSysMesage("Duel has started! Please type @quitduel if you want to give up!");
            }
        }

        /// <summary>
        /// Determines what we're supposed to do when someone is shot
        /// </summary>
        /// <param name="User"></param>
        public void Shot(GameClient User, Database.MagicType.Magic Info)
        {
            if (Against == Opponent.Team)
                User.Team.Leader.Shots++;
            else
                User.Shots++;
            if (User.Hit)
            {
                User.Hit = false;
                User.Chains++;
                if (User.Chains > User.MaxChains)
                    User.MaxChains = User.Chains;
            }
            else
                User.Chains = 0;

            //if (Type != DuelType.UnlimitedStamina)
            //    User.Player.Stamina -= Info.UseStamina;

            DisplayScore();

            if (Count == Hits.Ten && User.Hits >= 10)
            {
                if (Database.Server.GamePoll.ContainsKey((User.Dueler)))
                    Reward(User, Database.Server.GamePoll[User.Dueler]);
                Finish();
                return;
            }
            else if (Count == Hits.Hundred && User.Hits >= 100)
            {
                if (Database.Server.GamePoll.ContainsKey((User.Dueler)))
                    Reward(User, Database.Server.GamePoll[User.Dueler]);
                Finish();
                return;
            }
        }

        /// <summary>
        /// Determines wether we're supposed to do something when a player gets hitted
        /// </summary>
        /// <param name="Attacker"></param>
        /// <param name="Victim"></param>
        public void Hit(GameClient Attacker, GameClient Victim)
        {
            if (Against == Opponent.Team)
            {
                if ((TeamOne.ContainsKey(Attacker.EntityID) && TeamTwo.ContainsKey(Victim.EntityID)) || (TeamTwo.ContainsKey(Attacker.EntityID) && TeamOne.ContainsKey(Victim.EntityID)))
                    Attacker.Team.Leader.Hits++;
            }
            else
                Attacker.Hits++;
            Attacker.Hit = true;
        }

        /// <summary>
        /// Overrides the damage dealt by a certain skill
        /// </summary>
        /// <param name="User"></param>
        /// <param name="C"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public uint GetDamage(GameClient User, GameClient C, Database.MagicType.Magic Info)
        {
            if (Type == DuelType.Leech)
            {
                if (C.Player.Stamina >= Info.UseStamina)
                {
                    if (User.Player.Stamina + Info.UseStamina > 100)
                        User.Player.Stamina = 100;
                    else
                        User.Player.Stamina += Info.UseStamina;
                }
            }
            return 1;
        }

        public uint GetDamage(GameClient User, Database.MagicType.Magic Info)
        {
            if (Type == DuelType.Leech)
            {
                if (User.Player.Stamina >= Info.UseStamina)
                {
                    if (User.Player.Stamina + Info.UseStamina > 100)
                        User.Player.Stamina = 100;
                    else
                        User.Player.Stamina += Info.UseStamina;
                }
            }
            return 1;
        }

        public uint GetDamage(GameClient User, GameClient C)
        {
            return 1;
        }

        /// <summary>
        /// Announces the winner and rewards in case players are dueling for a wagger
        /// </summary>
        /// <param name="C"></param>
        /// <param name="C2"></param>
        public void Reward(GameClient C, GameClient C2)
        {
            if (C != null && C2 != null)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    //C2.Send(stream.ArenaSignupCreate(MsgArenaSignup.DialogType.Dialog2, MsgArenaSignup.DialogButton.SignUp, C2));
                    //C.Send(stream.ArenaSignupCreate(MsgArenaSignup.DialogType.Dialog2, MsgArenaSignup.DialogButton.Win, C));
                }
                if (C.Shots > 0 && C2.Shots > 0)
                    Broadcast($"{C.Name} has beat {C2.Name} in a {Type.ToString()} Duel with the following Score: {C.Hits}-{C2.Hits}, Ratio: {((C.Hits * 100) / C.Shots)}%-{((C2.Hits * 100) / C2.Shots)}% and Max Chains: {C.MaxChains}-{C2.MaxChains}", BroadCastLoc.World);
                else if (C.Shots > 0)
                    Broadcast($"{C.Name} has beat {C2.Name} in a {Type.ToString()} Duel with the following Score: {C.Hits}-{C2.Hits}, Ratio: {((C.Hits * 100) / C.Shots)}%-{C2.Shots}% and Max Chains: {C.MaxChains}-{C2.MaxChains}", BroadCastLoc.World);
                else if (C2.Shots > 0)
                    Broadcast($"{C.Name} has beat {C2.Name} in a {Type.ToString()} Duel with the following Score: {C.Hits}-{C2.Hits}, Ratio: {C.Shots}%-{((C2.Hits * 100) / C2.Shots)}% and Max Chains: {C.MaxChains}-{C2.MaxChains}", BroadCastLoc.World);
                else
                    Broadcast($"{C.Name} has beat {C2.Name} in a {Type.ToString()} Duel with the following Score: {C.Hits}-{C2.Hits}, Ratio: {C.Shots}%-{C2.Shots}% and Max Chains: {C.MaxChains}-{C2.MaxChains}", BroadCastLoc.World);

                if (Wager)
                {
                    if (C.Player.ConquerPoints + (WagerAmount * 2) < 2000000000)
                        C.Player.ConquerPoints += WagerAmount * 2;
                    else
                    {
                        C.SendSysMesage("WARNING: You can't have more than 2,000,000,000 in your inventory! Please take a screenshot and contact us!");
                    }
                }
            }
        }

        /// <summary>
        /// Called when duel is over, teleports players to old location
        /// </summary>
        public void Finish()
        {
            DisplayScore();
            foreach (GameClient C in PlayerList.Values.ToList())
            {
                ChangePKMode(C, PKMode.Capture);
                C.Teleport(C.Player.PMapX, C.Player.PMapY, C.Player.PMap, 0, true, true);
                C.Dueler = 0;
                C.Hits = 0;
                C.Shots = 0;
                C.Chains = 0;
                C.MaxChains = 0;
                C.Hit = false;
                C.Arena = null;
                C.Arena2 = 1;
            }
            return;
        }

        /// <summary>
        /// Displays the score inside the map
        /// </summary>
        public void DisplayScore()
        {
            Broadcast("---------Score---------", BroadCastLoc.Title);
            byte count = 1;
            if (Against == Opponent.Single)
            {
                foreach (GameClient C in PlayerList.Values.ToList())
                {
                    if (C.Hits > 0 && C.Shots > 0)
                        Broadcast($"{C.Name} - Hits: {C.Hits} Ratio: {((C.Hits * 100) / C.Shots)}% Max Chain: {C.MaxChains}", BroadCastLoc.Score, count);
                    else
                        Broadcast($"{C.Name} - Hits: {C.Hits} Ratio: 0% Max Chain: {C.MaxChains}", BroadCastLoc.Score, count);
                    count++;
                }
            }
            else
            {
                foreach (GameClient C in PlayerList.Values.ToList())
                {
                    if (C.Team.Leader.Player.UID == C.EntityID)
                    {
                        if (C.Hits > 0 && C.Shots > 0)
                            Broadcast($"{C.Name} - Hits: {C.Hits} Ratio: {((C.Hits * 100) / C.Shots)}% Max Chain: {C.MaxChains}", BroadCastLoc.Score, count);
                        else
                            Broadcast($"{C.Name} - Hits: {C.Hits} Ratio: 0% Max Chain: {C.MaxChains}", BroadCastLoc.Score, count);
                    }
                    count++;
                }
            }
        }

        /// <summary>
        /// Removes a player from the current match
        /// </summary>
        /// <param name="C"></param>
        public void RemovePlayer(GameClient C)
        {
            if (Against == Opponent.Single)
            {
                if (Database.Server.GamePoll.ContainsKey((C.Dueler)))
                    Reward(Database.Server.GamePoll[C.Dueler], C);
                ChangePKMode(C, PKMode.Capture);
                C.Teleport(C.Player.PMapX, C.Player.PMapY, C.Player.PMap);
                Finish();
            }
            else
            {
                if (C.Team.Leader.Player.UID == C.EntityID)
                {
                    if (Database.Server.GamePoll.ContainsKey((C.Dueler)))
                        Reward(Database.Server.GamePoll[C.Dueler], C);
                    Finish();
                }
                else
                {
                    if (TeamOne.ContainsKey(C.EntityID))
                        TeamOne.Remove(C.EntityID);
                    else if (TeamTwo.ContainsKey(C.EntityID))
                        TeamTwo.Remove(C.EntityID);
                    if (TeamOne.Count == 0 || TeamTwo.Count == 0)
                    {
                        foreach (GameClient C2 in TeamOne.Values)
                            if (C2.Team.Leader.Player.UID == C2.EntityID)
                                Reward(C2, C);
                        foreach (GameClient C2 in TeamTwo.Values)
                            if (C2.Team.Leader.Player.UID == C2.EntityID)
                                Reward(C2, C);
                        Finish();
                    }
                    C.Teleport(C.Player.PMapX, C.Player.PMapY, C.Player.PMap);
                }
            }
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
