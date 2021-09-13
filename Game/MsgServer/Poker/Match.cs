using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer.Poker
{
    public class Match
    {
        public enum StatusType : byte
        {
            Unopened = 0,
            Pocket = 1,
            Flop = 2,
            Turn = 3,
            River = 4,
            Showdown = 5
        }

        
        public Extensions.SafeDictionary<uint, Client.GameClient> Players = new Extensions.SafeDictionary<uint, Client.GameClient>();
        public Extensions.SafeDictionary<uint, PlayerInfo> PlayersInfo = new Extensions.SafeDictionary<uint, PlayerInfo>();
        public PocketCardsGenerator Cards = new PocketCardsGenerator();
        public Extensions.MyList<Card> TableCards = new Extensions.MyList<Card>();
        public Extensions.MyList<Card> DeadCards = new Extensions.MyList<Card>();

        public StatusType Status = StatusType.Unopened;
        public Extensions.Time32 PocketStamp = new Extensions.Time32();
        public Extensions.Time32 GiveCardsStamp = new Extensions.Time32();
        public Extensions.Time32 HandStamp = new Extensions.Time32();
        public int GetHandSecounds
        {
            get
            {
                int val = (int)(HandStamp.AllSeconds - Extensions.Time32.Now.AllSeconds);
                if (val < 0)
                    return 0;
                else
                    return val;
            }
        }

        public bool IsHandTable
        {

            get { return table.Noumber > 200; }
        }

        public bool IsStartedKick = false;
        public bool CanKick = false;
        public bool CheckDealer = false;
        public bool CanCheck = false;
        public bool Finished = false;

        public bool IsStarted = false;
        public bool SendCards = false;
        public bool FirstPlayer = false;
        public Table table = null;


        public long TableBet = 0;
        public long LastBet = 0;

        public uint StartPlayer = 0;
        public uint DealerUID = 0;
        public uint SmallPlayer = 0;
        public uint BigPlayer = 0;


        private int Index = 0;


        public Match(Table _table)
        {
            table = _table;
        }

        public Client.GameClient[] AvailablePlayers
        {
            get
            {
                return Players.GetValues().Where(p => p.MyPokerTable != null && p.PokerInfo != null && p.MyPokerTable.UID == table.UID
                    && p.Socket.Alive && p.Player.Map == table.Map).ToArray();
            }
        }


        public Client.GameClient HandPlayer = null;
        public Client.GameClient GetNextPlayer()
        {
            Client.GameClient client = null;

            for (int x = 0; x < (int)(IsHandTable ? 5 : 9); x++)
            {

                foreach (var user in AvailablePlayers)
                {
                    if (user.PokerInfo.Fold || user.PokerInfo.AllIn)
                        continue;

                    if (user.PokerInfo.Location == Index)
                    {
                        client = user;
                    }
                }
                //update index----------
                IncreaseIndex();
                //--------------
                if (client != null)
                {
                    return client;
                }
            }
            return client;
        }
        public void IncreaseIndex()
        {
            if (IsHandTable)
            {
                if (Index == 4)
                    Index = 0;
                else
                    Index++;
            }
            else
            {
                if (Index == 8)
                    Index = 0;
                else
                    Index++;
            }
        }
        public void UpdateTablePlayers()
        {
            if (Players.Count < 2)
            {
                Reset();
                return;
            }
            if (Players.Count == 2)
            {
                HandPlayer = Players.GetValues().Where(p => p.Player.UID != DealerUID).First();
                SmallPlayer = HandPlayer.Player.UID;
                BigPlayer = DealerUID;
                Index = (int)HandPlayer.PokerInfo.Location;

            }
            else
            {
                SmallPlayer = GetNextPlayer().Player.UID;
                BigPlayer = GetNextPlayer().Player.UID;
                HandPlayer = GetNextPlayer();
                Index = (int)HandPlayer.PokerInfo.Location;
            }
        }

        public void PrepareStart()
        {
           
                foreach (var user in Players.GetValues())
                {
                    string _Card = Cards.GetRandomCard();
                    var card = new Card() { StrCard = _Card };
                    user.PokerInfo.Cards.Add(card);
                    user.PokerInfo.DeadCard = card;
                    DeadCards.Add(card);
                }
            
        }
        public void GeneratePlayersCards()
        {
            if (IsHandTable)
            {
                foreach (var user in Players.GetValues())
                {
                    user.PokerInfo.Cards.Clear();
                }
                for (int x = 0; x < 2; x++)
                {
                    foreach (var user in Players.GetValues())
                    {
                        string _Card = Cards.GetRandomCard();
                        user.PokerInfo.Cards.Add(new Card() { StrCard = _Card });
                    }
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    foreach (var user in table.Players.GetValues())
                        user.Send(stream.ShowUnknowCard(table, user.PokerInfo));

                    foreach (var user in table.Watchers.GetValues())
                        user.Send(stream.ShowUnknowCard(table, user.PokerInfo));


             




                    foreach (var user in table.Players.GetValues())
                        user.Send(stream.PokerDrawCardsCreate(table, user.PokerInfo));

                    foreach (var user in table.Watchers.GetValues())
                        user.Send(stream.PokerDrawCardsCreate(table, user.PokerInfo));

                  
                }
            }
            else
            {
                foreach (var user in Players.GetValues())
                {
                    user.PokerInfo.Cards.Clear();
                }
                for (int x = 0; x < 2; x++)
                {
                    foreach (var user in Players.GetValues())
                    {
                        string _Card = Cards.GetRandomCard();
                        user.PokerInfo.Cards.Add(new Card() { StrCard = _Card });
                    }
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    foreach (var user in table.Players.GetValues())
                        user.Send(stream.PokerDrawCardsCreate(table, user.PokerInfo));
                    foreach (var user in table.Watchers.Values)
                        user.Send(stream.PokerDrawCardsCreate(table, user.PokerInfo));

                  
                }
            }
        }
        public void GenerateTableCards()
        {
            if (IsHandTable)
            {
                uint count = 1;
                lock (TableCards)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        for (int x = 0; x < count; x++)
                        {
                            foreach (var PokerUser in AvailablePlayers)
                            {
                                string _Card = Cards.GetRandomCard();
                                var ccard = new Card() { StrCard = _Card };
                                PokerUser.PokerInfo.Cards.Add(new Card() { StrCard = _Card });
                            }
                        }
                        foreach (var user in table.Players.GetValues())
                            user.Send(stream.PokerDrawCardsCreate(table, user.PokerInfo));

                        foreach (var user in table.Watchers.GetValues())
                            user.Send(stream.PokerDrawCardsCreate(table, user.PokerInfo));
                    }
                }
            }
            else
            {
                uint count = 0;
                if (TableCards.Count == 0)
                    count = 3;
                else
                    count = 1;
                lock (TableCards)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        for (int x = 0; x < count; x++)
                        {
                            string _Card = Cards.GetRandomCard();
                            var ccard = new Card() { StrCard = _Card };
                            TableCards.Add(new Card() { StrCard = _Card });
                            if (count == 1)
                            {
                                table.Send(stream.PokerDrawCardsCreate(this.table, new Card[1] { ccard }, (ushort)(TableCards.Count - 2)));
                            }
                        }
                        if (count == 3)
                        {
                            table.Send(stream.PokerDrawCardsCreate(this.table, TableCards.GetValues(), (ushort)(TableCards.Count - 2)));
                        }
                    }
                }
            }
        }
        public void SelectTheDealer()
        {
            if (Players.Count < 2)
            {
                Reset();
                return;
            }
            Client.GameClient Dealear = null;
            if (IsHandTable)
                Dealear = Players.GetValues().OrderByDescending(p => p.PokerInfo.Cards[1].GameCardID).FirstOrDefault();
            else
                Dealear = Players.GetValues().OrderByDescending(p => p.PokerInfo.Cards[0].GameCardID).FirstOrDefault();
            DealerUID = Dealear.Player.UID;
            Index = (int)Dealear.PokerInfo.Location;
            IncreaseIndex();
            UpdateTablePlayers();
        }
        public void CheckUpStart()
        {
            if (table.Players.Count >= 2 && IsStarted == false && Extensions.Time32.Now > PocketStamp)
            {
                IsStarted = true;
                Status = StatusType.Pocket;
            }
        }
        public void JoinInformations(Client.GameClient user, bool WasWather = false)
        {
            if (!WasWather)
            {
                if (CheckDealer && IsHandTable == false)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        user.Send(stream.PokerDrawCardsCreate(7, 4, this));
                    }
                }
                if (TableCards.Count != 0 && IsHandTable == false)
                {
                    lock (TableCards)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            user.Send(stream.PokerDrawCardsCreate(this.table, TableCards.GetValues(), (ushort)(TableCards.Count - 2)));

                        }
                    }
                }
                if (SendCards)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        user.Send(stream.MsgShowHandLostInfoCreate(this));
                    }
                }
            }
            
        }
        public void CheckKickOnLeave(Client.GameClient user)
        {
            if (IsStartedKick)
            {
                 if (table.ReceiveKick != null)
                 {
                     if (table.ReceiveKick.Player.UID == user.Player.UID)
                     {
                         using (var rec = new ServerSockets.RecycledPacket())
                         {
                             var stream = rec.GetStream();

                             user.MyPokerTable.Send(stream.MsgShowHandKickCreate(MsgShowHandKick.ActionType.Kicked, table.StarterKick, user.Player.UID, 0, 0));
                             CanKick = true;
                             IsStartedKick = false;
                             table.PlayersAcceptKick.Clear();
                         }
                     }
                 }
            }
        }
        public void CheckUp()
        {
            Extensions.Time32 Now = Extensions.Time32.Now;

            switch (Status)
            {
                case StatusType.Unopened:
                    {
                        if (Now > PocketStamp)
                        {
                            
                            Status = StatusType.Pocket;
                        }
                        if (table.Players.Count > 2)
                        {
                            if (table.PlayersAcceptKick.Count > table.Players.Count - 1)
                            {
                                var user = table.ReceiveKick;
                                if (user != null)
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        user.MyPokerTable.Send(stream.MsgShowHandKickCreate(MsgShowHandKick.ActionType.Kicked, table.StarterKick, user.Player.UID, 0, 0));
                                        user.Player.View.SendView(stream.PokerLeaveTableCreate(user.PokerInfo), true);
                                        user.Player.View.SendView(stream.PokerUpdateTableLocationCreate(1, user.MyPokerTable.UID, user.Player.UID, user.PokerInfo.Location), true);
                                        user.MyPokerTable.Players.Remove(user.Player.UID);
                                        user.MyPokerTable.PlayersLocation[user.PokerInfo.Location] = null;
                                        user.MyPokerTable.TableMatch.RemovePlayer(user);

                                        CanKick = true;
                                        IsStartedKick = false;
                                        table.PlayersAcceptKick.Clear();
                                    }
                                }
                            }
                        }
                        break;
                    }
                case StatusType.Pocket:
                    {
                        if (IsHandTable)
                        {
                            if (!CheckDealer)
                            {
                                foreach (var user in table.Players.GetValues())
                                {
                                    user.PokerInfo.Cards.Clear();
                                    user.PokerInfo.Check = user.PokerInfo.AllIn = user.PokerInfo.WasBetted = user.PokerInfo.Fold = false;
                                    user.PokerInfo.MyBet = 0;
                                    user.PokerInfo.RoundBet = 0;
                                    user.PokerInfo.MyReward = 0;
                                    user.PokerInfo.MyRewardLost = 0;
                                }
                                if (IsStartedKick)
                                {
                                    var user = table.ReceiveKick;
                                    if (user != null)
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            user.MyPokerTable.Send(stream.MsgShowHandKickCreate(MsgShowHandKick.ActionType.StayHere, table.StarterKick, user.Player.UID, 0, 0));
                                        }
                                    }
                                }
                                IsStartedKick = false;
                                CanKick = false;
                                IsStarted = true;
                                foreach (var user in table.Players.GetValues())
                                {
                                    if (!Players.ContainsKey(user.Player.UID))
                                        Players.Add(user.Player.UID, user);
                                    if (!PlayersInfo.ContainsKey(user.Player.UID))
                                        PlayersInfo.Add(user.Player.UID, user.PokerInfo);
                                    user.PokerInfo.State = 1;
                                }
                                if (Players.Count < 2)
                                {
                                    Reset();
                                    break;
                                }
                                CheckDealer = true;
                               // PrepareStart();
                               
                                
                                
                                
                                /*using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    table.Send(stream.PokerDrawCardsCreate(7, 4, this));
                                }*/

                               // GiveCardsStamp = Extensions.Time32.Now.AddSeconds(5);

                              
                                GeneratePlayersCards();
                                SelectTheDealer();
                                SendCards = true;
                                FirstPlayer = true;

                                if (Players.Count < 2)
                                {
                                    Reset();
                                    break;
                                }

                                LastBet = table.MinBet;

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    foreach (var user in Players.GetValues())
                                    {
                                        switch (table.Type)
                                        {
                                            case Table.TableType.Silver:
                                                {

                                                    if (user.Player.Money >= table.MinBet)
                                                    {
                                                        TableBet += table.MinBet;
                                                        user.PokerInfo.MyBet += table.MinBet;
                                                        user.PokerInfo.RoundBet += table.MinBet;
                                                        user.Player.Money -= table.MinBet;
                                                        user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                                    }
                                                    else
                                                        RemovePlayer(user);
                                                    break;
                                                }
                                            case Table.TableType.ConquerPoints:
                                                {
                                                    if (user.Player.ConquerPoints >= table.MinBet)
                                                    {
                                                        TableBet += table.MinBet;
                                                        user.PokerInfo.RoundBet += table.MinBet;
                                                        user.PokerInfo.MyBet += table.MinBet;
                                                        TableBet += table.MinBet;
                                                        user.Player.ConquerPoints -= table.MinBet;
                                                    }
                                                    else
                                                        RemovePlayer(user);
                                                    break;
                                                }
                                        }
                                    }
                                }
                                UpdateTableBet();

                            }
                           if (SendCards)
                            {
                                if (FirstPlayer)
                                {
                                    StartPlayer = HandPlayer.Player.UID;
                                    HandStamp = Extensions.Time32.Now.AddSeconds(15);
                                    IncreaseIndex();
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        if (table.Type == Table.TableType.Silver)
                                            table.Send(stream.PokerPlayerTurnCreate((ushort)GetHandSecounds, 54, (ulong)table.MinBet, (ulong)(HandPlayer.Player.Money), HandPlayer.Player.UID));
                                        else
                                            table.Send(stream.PokerPlayerTurnCreate((ushort)GetHandSecounds, (ushort)((uint)MsgPokerHand.PokerCallTypes.Fold | (uint)MsgPokerHand.PokerCallTypes.Rise | (uint)MsgPokerHand.PokerCallTypes.Call), (ulong)table.MinBet, (ulong)(HandPlayer.Player.Money), HandPlayer.Player.UID));
                                        UpdateTableBet();
                                    }

                                    FirstPlayer = false;
                                }
                                else
                                {
                                    if (GetHandSecounds <= 0)
                                    {
                                        if (HandPlayer != null)
                                        {
                                            lock (HandPlayer)
                                                Fold(HandPlayer);
                                        }
                                    }
                                    if (ChechFinishGame)
                                    {
                                        Finish();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!CheckDealer)
                            {
                                foreach (var user in table.Players.GetValues())
                                {
                                    user.PokerInfo.Cards.Clear();
                                    user.PokerInfo.Check = user.PokerInfo.AllIn = user.PokerInfo.WasBetted = user.PokerInfo.Fold = false;
                                    user.PokerInfo.MyBet = 0;
                                    user.PokerInfo.RoundBet = 0;
                                    user.PokerInfo.MyReward = 0;
                                    user.PokerInfo.MyRewardLost = 0;
                                }
                                if (IsStartedKick)
                                {
                                    var user = table.ReceiveKick;
                                    if (user != null)
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            user.MyPokerTable.Send(stream.MsgShowHandKickCreate(MsgShowHandKick.ActionType.StayHere, table.StarterKick, user.Player.UID, 0, 0));
                                        }
                                    }
                                }
                                IsStartedKick = false;
                                CanKick = false;
                                IsStarted = true;
                                foreach (var user in table.Players.GetValues())
                                {
                                    if (!Players.ContainsKey(user.Player.UID))
                                        Players.Add(user.Player.UID, user);
                                    if (!PlayersInfo.ContainsKey(user.Player.UID))
                                        PlayersInfo.Add(user.Player.UID, user.PokerInfo);
                                    user.PokerInfo.State = 1;
                                }
                                if (Players.Count < 2)
                                {
                                    Reset();
                                    break;
                                }
                                CheckDealer = true;
                                PrepareStart();
                                SelectTheDealer();
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    table.Send(stream.PokerDrawCardsCreate(7, 4, this));
                                }
                                GiveCardsStamp = Extensions.Time32.Now.AddSeconds(5);


                                LastBet = table.MinBet;

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    foreach (var user in Players.GetValues())
                                    {
                                        switch (table.Type)
                                        {
                                            case Table.TableType.Silver:
                                                {

                                                    if (user.Player.Money >= table.MinBet)
                                                    {
                                                        TableBet += table.MinBet;
                                                        user.PokerInfo.MyBet += table.MinBet;
                                                        user.PokerInfo.RoundBet += table.MinBet;
                                                        user.Player.Money -= table.MinBet;
                                                        user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                                    }
                                                    else
                                                        RemovePlayer(user);
                                                    break;
                                                }
                                            case Table.TableType.ConquerPoints:
                                                {
                                                    if (user.Player.ConquerPoints >= table.MinBet)
                                                    {
                                                        TableBet += table.MinBet;
                                                        user.PokerInfo.RoundBet += table.MinBet;
                                                        user.PokerInfo.MyBet += table.MinBet;
                                                        TableBet += table.MinBet;
                                                        user.Player.ConquerPoints -= table.MinBet;
                                                    }
                                                    else
                                                        RemovePlayer(user);
                                                    break;
                                                }
                                        }
                                    }
                                }
                                UpdateTableBet();

                            }
                            else if (!SendCards)
                            {
                                if (Now > GiveCardsStamp)
                                {
                                    if (Players.Count < 2)
                                    {
                                        Reset();
                                        break;
                                    }
                                    SendCards = true;
                                    GeneratePlayersCards();
                                    FirstPlayer = true;


                                }
                            }
                            else if (SendCards)
                            {
                                if (FirstPlayer)
                                {
                                    StartPlayer = HandPlayer.Player.UID;
                                    HandStamp = Extensions.Time32.Now.AddSeconds(15);
                                    IncreaseIndex();
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        if (table.Type == Table.TableType.Silver)
                                            table.Send(stream.PokerPlayerTurnCreate((ushort)GetHandSecounds, 54, (ulong)table.MinBet, (ulong)(HandPlayer.Player.Money), HandPlayer.Player.UID));
                                        else
                                            table.Send(stream.PokerPlayerTurnCreate((ushort)GetHandSecounds, (ushort)((uint)MsgPokerHand.PokerCallTypes.Fold | (uint)MsgPokerHand.PokerCallTypes.Rise | (uint)MsgPokerHand.PokerCallTypes.Call), (ulong)table.MinBet, (ulong)(HandPlayer.Player.Money), HandPlayer.Player.UID));
                                        UpdateTableBet();
                                    }

                                    FirstPlayer = false;
                                }
                                else
                                {
                                    if (GetHandSecounds <= 0)
                                    {
                                        if (HandPlayer != null)
                                        {
                                            lock (HandPlayer)
                                                Fold(HandPlayer);
                                        }
                                    }
                                    if (ChechFinishGame)
                                    {
                                        Finish();
                                    }
                                }
                            }
                        }
                        break;
                    }
                case StatusType.Showdown:
                    {
                        if (ChechFinishGame)
                        {
                            Finish();
                        }
                        PocketStamp = Extensions.Time32.Now.AddSeconds(20);
                   
                        if (Finished)
                        {
                            Reset();
                            Status = StatusType.Unopened;
                        }
                        CanKick = true;
                        SendTurnTimer();
                        break;
                    }
                default:
                    {
                        if (Players.Count < 2)
                        {
                            Reset();
                            break;
                        }
                        if (GetHandSecounds <= 0)
                        {
                            if (HandPlayer != null)
                            {
                                lock (HandPlayer)
                                    Fold(HandPlayer);
                            }
                        }
                        if (ChechFinishGame)
                        {
                            Finish();
                        }
                        break;
                    }

            }
        }
        public void ChangeRound()
        {
            if (ChechFinishGame)
            {
                Finish();
            }
            GenerateTableCards();
            foreach (var user in AvailablePlayers)
            {
                user.PokerInfo.Check = false;
                user.PokerInfo.RoundBet = 0;
                user.PokerInfo.WasBetted = false;
            }
            CanCheck = true;
            LastBet = table.MinBet;
        }
        public bool CanChangeRound
        {
            get
            {
                int value1 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false).Count();
                int value2 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false && (p.PokerInfo.RoundBet >= (ulong)LastBet || p.PokerInfo.AllIn) && p.PokerInfo.WasBetted == true).Count();
                if (value2 != 0)
                {
                    return value1 == value2;
                }
                int value3 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false && (p.PokerInfo.RoundBet >= (ulong)LastBet || p.PokerInfo.Check) && p.PokerInfo.WasBetted == true).Count();
                if (value3 == value1)
                    return true;
                return false;
                /*   int value1 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false && (p.PokerInfo.RoundBet >= (ulong)LastBet || p.PokerInfo.AllIn || p.PokerInfo.Check) && p.PokerInfo.WasBetted == true).Count();
                   int value2 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false && (p.PokerInfo.RoundBet >= (ulong)LastBet || p.PokerInfo.AllIn || p.PokerInfo.Check)).Count();
                   int value3 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false).Count();
                   int value4 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false && (p.PokerInfo.RoundBet >= (ulong)LastBet || p.PokerInfo.AllIn) && p.PokerInfo.WasBetted == true).Count();
                   if (value4 == 0)//if all its on check
                   {
                       return value1 == value2 && value1 == value3;
                   }
                
                   Console.WriteLine(value1 + " " + value2 + " " + value3);
                   return value1 == value2 && value1 == value3 && value3 == value4;*/
            }
        }
        public bool ChechFinishGame
        {
            get
            {

                int value1 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false).Count();
                int value2 = AvailablePlayers.Where(p => p.PokerInfo.AllIn == true).Count();
                if (value1 == value2 || value1 == 1)
                    return true;
                if (Status == StatusType.River && CanChangeRound)
                    return true;
                /*  int value3 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false && (p.PokerInfo.RoundBet >= (ulong)LastBet || p.PokerInfo.AllIn) && p.PokerInfo.WasBetted == true).Count();
                  if (value3 == value1)
                      return true;
                 */

                int value4 = AvailablePlayers.Where(p => p.PokerInfo.Fold == false && (p.PokerInfo.RoundBet >= (ulong)LastBet || p.PokerInfo.AllIn || p.PokerInfo.Check) && p.PokerInfo.WasBetted == true).Count();
                if (value4 == 0 && value2 != 0)
                    return true;
                return false;
            }
        }

        public unsafe void UpdateTableBet()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                ActionQuery action = new ActionQuery()
                {
                    ObjId = table.UID,
                    Type = ActionType.PoketTableBet,
                    PokerTableBet = TableBet,
                    PokerRoundBet = LastBet
                };
                table.Send(stream.ActionCreate(&action));

            }
        }
        public int GetTurnTimer
        {
            get
            {
                int Secounds = (int)(PocketStamp.AllSeconds - Extensions.Time32.Now.AllSeconds);
                if (Secounds < 0)
                    Secounds = 0;
                return Secounds;
            }
        }
        public void SendTurnTimer()
        {
            if (PocketStamp > Extensions.Time32.Now)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    table.Send(stream.PokerPlayerTurnCreate((ushort)GetTurnTimer, 0, 0, 0, 0));
                }
            }
        }
        public void Reset()
        {
            try
            {
                if (CheckDealer)
                {
                    Finish();
                }
                foreach (var user in AvailablePlayers)
                {
                    user.PokerInfo.Cards.Clear();
                    user.PokerInfo.Check = user.PokerInfo.AllIn = user.PokerInfo.WasBetted = user.PokerInfo.SetMyBet = user.PokerInfo.Fold = false;
                    user.PokerInfo.MyBet = 0;
                    user.PokerInfo.RoundBet = 0;
                    user.PokerInfo.MyReward = 0;
                    user.PokerInfo.MyRewardLost = 0;
                }
                table.PlayersAcceptKick.Clear();
                DeadCards.Clear();
                PlayersInfo.Clear();
                TableCards.Clear();
                table.ReceiveKick = null;
                Players.Clear();
                Status = StatusType.Unopened;
                Cards.Reset();
                Index = 0;
                TableBet = LastBet = 0;
                IsStartedKick = false;
                CanKick = false;
                SendCards = false;
                CheckDealer = false;
                FirstPlayer = false;
                IsStarted = false;
                Finished = false;
                SendTurnTimer();
                UpdateTableBet();
            }
            catch (Exception e)
            {
                MyConsole.SaveException(e);
            }
        }
        public void RemovePlayer(Client.GameClient user)
        {
            if (Players.ContainsKey(user.Player.UID))
            {
              
               
                if (SendCards)
                    Fold(user);

                if (table.Players.ContainsKey(user.Player.UID))
                    table.Players.Remove(user.Player.UID);
                user.MyPokerTable.PlayersLocation[user.PokerInfo.Location] = null;
                Players.Remove(user.Player.UID);
            }
        }
        public void DisconnetPlayer(Client.GameClient user)
        {
            RemovePlayer(user);
            table.Players.Remove(user.Player.UID);
            user.MyPokerTable.PlayersLocation[user.PokerInfo.Location] = null;
        }
        public void Fold(Client.GameClient user)
        {
            if (user == null)
                return;
            if (user.PokerInfo != null && user.MyPokerTable != null)
            {
                if (user.PokerInfo.Fold == false)
                {
                    user.PokerInfo.Fold = true;

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        table.Send(stream.PokerHandCreate(MsgPokerHand.PokerCallTypes.Fold, (ulong)user.MyPokerTable.TableMatch.LastBet, (ulong)user.PokerInfo.MyBet, user.Player.UID));
                        MsgPokerHand.SetNextHand(user, stream);
                    }
                }
            }
            if (ChechFinishGame)
            {
                Finish();
            }
        }

        public void Finish()
        {
            if (IsHandTable)
            {
                string logs = "";
                if (Finished == false && Status > StatusType.Unopened)
                {
                    Finished = true;
                    List<Client.GameClient> RemovePlayers = new List<Client.GameClient>();

                    if (AvailablePlayers.Where(p => p.PokerInfo.Fold == false).Count() == 1)
                    {
                        ulong win = 0;
                        var wiiner = AvailablePlayers.Where(p => p.PokerInfo.Fold == false).First();
                        foreach (var user in PlayersInfo.GetValues())
                        {
                            if (user.UID != wiiner.Player.UID)
                            {
                                win += wiiner.PokerInfo.MyReward += user.MyBet;
                                win += user.MyRewardLost += user.MyBet;

                            }
                        }
                        logs = "[Poker]" + wiiner.Player.Name + " -> win " + (wiiner.PokerInfo.MyReward + (ulong)wiiner.PokerInfo.MyBet) + " " + table.Type.ToString() + "";
                        Database.ServerDatabase.LoginQueue.Enqueue(logs);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            var winners2 = new List<Client.GameClient>();
                            winners2.Add(wiiner);
                            table.Send(stream.PokerRoundResultCreate(table, winners2));
                            switch (table.Type)
                            {
                                case Table.TableType.Silver:
                                    {
#if TEST
                                    Console.WriteLine(wiiner.Player.Name + " win " + wiiner.PokerInfo.MyReward);
#endif
                                        wiiner.PokerInfo.MyReward += (ulong)wiiner.PokerInfo.MyBet;
                                        wiiner.Player.Money += (long)wiiner.PokerInfo.MyReward;
                                        wiiner.Player.SendUpdate(stream, wiiner.Player.Money, MsgUpdate.DataType.Money);
                                        break;
                                    }
                                case Table.TableType.ConquerPoints:
                                    {
                                        wiiner.PokerInfo.MyReward += (ulong)wiiner.PokerInfo.MyBet;
                                        wiiner.Player.ConquerPoints += (uint)wiiner.PokerInfo.MyReward;
                                        break;
                                    }
                            }
                        }

                        foreach (var user in AvailablePlayers)
                        {
                            switch (table.Type)
                            {
                                case Table.TableType.ConquerPoints:
                                    {
                                        if (user.Player.ConquerPoints < table.MinBet * 10)
                                        {
                                            RemovePlayers.Add(user);
                                        }
                                        break;
                                    }
                                case Table.TableType.Silver:
                                    {
                                        if (user.Player.Money < table.MinBet * 10)
                                        {
                                            RemovePlayers.Add(user);
                                        }
                                        break;
                                    }
                            }
                        }
                        foreach (var user in RemovePlayers)
                        {
                            user.MyPokerTable.PlayersLocation[user.PokerInfo.Location] = null;
                            table.Players.Remove(user.Player.UID);
                            Players.Remove(user.Player.UID);
                            table.AddWatcher(user);
                        }
                        Status = StatusType.Showdown;
                        return;
                    }

                    var firstuser = AvailablePlayers.FirstOrDefault();
                    if (firstuser == null)
                    {
                        Reset();
                        return;
                    }
                    for (int x = firstuser.PokerInfo.Cards.Count; x < 5; x++)
                        GenerateTableCards();

                  
                    //int pindex = 0;
              

                    foreach (var user in AvailablePlayers)
                    {
                        double bestev = 0.0;
                        ulong bestmask = 0UL;
                        int bet = 5; // coins bet

                        string[] cardString = new string[5];
                        for (int x = 0; x < user.PokerInfo.Cards.Count; x++)
                            cardString[x] = user.PokerInfo.Cards[x].StrCard;

                        int[] cards = new int[cardString.Length];

                        // Parse Cards
                        for (int i = 0; i < cardString.Length; i++)
                            cards[i] = Game.MsgServer.Poker.Hand.ParseCard(cardString[i]);

                        // Try all possible hold masks
                        for (uint holdmask = 0; holdmask < 32; holdmask++)
                        {
                            double expectedValue = Game.MsgServer.Poker.Hand.ExpectedValue(holdmask, ref cards, bet);
                            if (expectedValue > bestev)
                            {
                                bestev = expectedValue;
                                bestmask = holdmask;
                            }
                        }
                        Console.WriteLine(""+user.Player.Name+" Best EV: {0:0.0###}", bestev);
                        user.PokerInfo.Rank = bestev;
                    }
                  /*  string boardcards = "";
                    string deadcards = "";
                    int count = AvailablePlayers.Where(p => p.PokerInfo.Fold == false).Count();
                    string[] playersCards = new string[count];
                    int[] pocketIndex = { -1, -1, -1, -1, -1 };
                    for (int x = 0; x < count; x++)
                    {
                        pocketIndex[x] = x;


                        playersCards[x] = AvailablePlayers[x].PokerInfo.Cards[0].StrCard + " " + AvailablePlayers[x].PokerInfo.Cards[1].StrCard
                           + " " + AvailablePlayers[x].PokerInfo.Cards[2].StrCard + " " + AvailablePlayers[x].PokerInfo.Cards[3].StrCard
                           + " " + AvailablePlayers[x].PokerInfo.Cards[4].StrCard;

                    }
                    long[] wins = new long[count];
                    long[] losses = new long[count];
                    long[] ties = new long[count];
                    long totalhands = 0;

                    Hand.HandOdds(playersCards, boardcards, deadcards, wins, ties, losses, ref totalhands);
                    for (int i = 0; i < count; i++)
                    {
                        SetPlayerValue(pocketIndex[i], (((double)wins[i]) + ((double)ties[i]) / 2.0) / ((double)totalhands));
                    }
                    */



                    var winners = SetPlayersRewards();

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        table.Send(stream.PokerRoundResultCreate(table, winners));
                        table.Send(stream.PokerShowAllCardsCreate(table, AvailablePlayers.Where(p => p.PokerInfo.Fold == false).ToArray()));
                    }

                    foreach (var user in AvailablePlayers)
                    {
                        switch (table.Type)
                        {
                            case Table.TableType.ConquerPoints:
                                {
                                    if (user.Player.ConquerPoints < table.MinBet * 10)
                                    {
                                        RemovePlayers.Add(user);
                                    }
                                    break;
                                }
                            case Table.TableType.Silver:
                                {
                                    if (user.Player.Money < table.MinBet * 10)
                                    {
                                        RemovePlayers.Add(user);
                                    }
                                    break;
                                }
                        }
                    }
                    foreach (var user in RemovePlayers)
                    {
                        user.MyPokerTable.PlayersLocation[user.PokerInfo.Location] = null;
                        table.Players.Remove(user.Player.UID);
                        Players.Remove(user.Player.UID);
                        table.AddWatcher(user);
                    }
                    Status = StatusType.Showdown;
                }

            }
            else
            {
                string logs = "";
                if (Finished == false && Status > StatusType.Unopened)
                {
                    Finished = true;
                    List<Client.GameClient> RemovePlayers = new List<Client.GameClient>();

                    if (AvailablePlayers.Where(p => p.PokerInfo.Fold == false).Count() == 1)
                    {
                        ulong win = 0;
                        var wiiner = AvailablePlayers.Where(p => p.PokerInfo.Fold == false).First();
                        foreach (var user in PlayersInfo.GetValues())
                        {
                            if (user.UID != wiiner.Player.UID)
                            {
                                win += wiiner.PokerInfo.MyReward += user.MyBet;
                                win += user.MyRewardLost += user.MyBet;

                            }
                        }
                        logs = "[Poker]" + wiiner.Player.Name + " -> win " + (wiiner.PokerInfo.MyReward + (ulong)wiiner.PokerInfo.MyBet) + " " + table.Type.ToString() + "";
                        Database.ServerDatabase.LoginQueue.Enqueue(logs);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            var winners2 = new List<Client.GameClient>();
                            winners2.Add(wiiner);
                            table.Send(stream.PokerRoundResultCreate(table, winners2));
                            switch (table.Type)
                            {
                                case Table.TableType.Silver:
                                    {
#if TEST
                                    Console.WriteLine(wiiner.Player.Name + " win " + wiiner.PokerInfo.MyReward);
#endif
                                        wiiner.PokerInfo.MyReward += (ulong)wiiner.PokerInfo.MyBet;
                                        wiiner.Player.Money += (long)wiiner.PokerInfo.MyReward;
                                        wiiner.Player.SendUpdate(stream, wiiner.Player.Money, MsgUpdate.DataType.Money);
                                        break;
                                    }
                                case Table.TableType.ConquerPoints:
                                    {
                                        wiiner.PokerInfo.MyReward += (ulong)wiiner.PokerInfo.MyBet;
                                        wiiner.Player.ConquerPoints += (uint)wiiner.PokerInfo.MyReward;
                                        break;
                                    }
                            }
                        }

                        foreach (var user in AvailablePlayers)
                        {
                            switch (table.Type)
                            {
                                case Table.TableType.ConquerPoints:
                                    {
                                        if (user.Player.ConquerPoints < table.MinBet * 10)
                                        {
                                            RemovePlayers.Add(user);
                                        }
                                        break;
                                    }
                                case Table.TableType.Silver:
                                    {
                                        if (user.Player.Money < table.MinBet * 10)
                                        {
                                            RemovePlayers.Add(user);
                                        }
                                        break;
                                    }
                            }
                        }
                        foreach (var user in RemovePlayers)
                        {
                            user.MyPokerTable.PlayersLocation[user.PokerInfo.Location] = null;
                            table.Players.Remove(user.Player.UID);
                            Players.Remove(user.Player.UID);
                            table.AddWatcher(user);
                        }
                        Status = StatusType.Showdown;
                        return;
                    }


                    if (TableCards.Count < 5)
                    {
                        if (TableCards.Count == 0)
                        {
                            GenerateTableCards();
                            GenerateTableCards();
                            GenerateTableCards();
                        }
                        else if (TableCards.Count == 3)
                        {
                            GenerateTableCards();
                            GenerateTableCards();
                        }
                        else if (TableCards.Count == 4)
                        {
                            GenerateTableCards();
                        }
                    }
                    string boardcards = "";
                    string deadcards = "";
                    int count = AvailablePlayers.Where(p => p.PokerInfo.Fold == false).Count();
                    string[] playersCards = new string[count];
                    int[] pocketIndex = { -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                    for (int x = 0; x < count; x++)
                    {
                        pocketIndex[x] = x;
                        playersCards[x] = AvailablePlayers[x].PokerInfo.Cards[0].StrCard + " " + AvailablePlayers[x].PokerInfo.Cards[1].StrCard;

                    }
                    long[] wins = new long[count];
                    long[] losses = new long[count];
                    long[] ties = new long[count];
                    long totalhands = 0;

                    foreach (var card in TableCards.GetValues())
                    {
                        boardcards += card.StrCard + " ";
                    }
                    foreach (var card in DeadCards.GetValues())
                        deadcards += card.StrCard + " ";

                    Hand.HandOdds(playersCards, boardcards, deadcards, wins, ties, losses, ref totalhands);
                    for (int i = 0; i < count; i++)
                    {
                        SetPlayerValue(pocketIndex[i], (((double)wins[i]) + ((double)ties[i]) / 2.0) / ((double)totalhands));
                    }




                    var winners = SetPlayersRewards();

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        table.Send(stream.PokerRoundResultCreate(table, winners));
                        table.Send(stream.PokerShowAllCardsCreate(table, AvailablePlayers.Where(p => p.PokerInfo.Fold == false).ToArray()));
                    }

                    foreach (var user in AvailablePlayers)
                    {
                        switch (table.Type)
                        {
                            case Table.TableType.ConquerPoints:
                                {
                                    if (user.Player.ConquerPoints < table.MinBet * 10)
                                    {
                                        RemovePlayers.Add(user);
                                    }
                                    break;
                                }
                            case Table.TableType.Silver:
                                {
                                    if (user.Player.Money < table.MinBet * 10)
                                    {
                                        RemovePlayers.Add(user);
                                    }
                                    break;
                                }
                        }
                    }
                    foreach (var user in RemovePlayers)
                    {
                        user.MyPokerTable.PlayersLocation[user.PokerInfo.Location] = null;
                        table.Players.Remove(user.Player.UID);
                        Players.Remove(user.Player.UID);
                        table.AddWatcher(user);
                    }
                    Status = StatusType.Showdown;
                }
            }
        }
        public void SetPlayerValue2(Poker.PlayerInfo user, double Rank)
        {
            user.Rank = Rank * 100.0;


            Console.WriteLine(" " + Rank * 100.0);
        }
        public void SetPlayerValue(int player_index, double Rank)
        {
            AvailablePlayers[player_index].PokerInfo.Rank = Rank * 100.0;


            Console.WriteLine(AvailablePlayers[player_index].Player.Name + " " + Rank * 100.0);
        }


        public List<Client.GameClient> SetPlayersRewards()
        {
            List<Client.GameClient> Winners = new List<Client.GameClient>();
            double Rank = 0;
            foreach (var user in AvailablePlayers.Where(p => p.PokerInfo.Fold == false))
            {
                if (user.PokerInfo.Rank > Rank)
                    Rank = user.PokerInfo.Rank;
            }
            foreach (var user in AvailablePlayers.Where(p => p.PokerInfo.Fold == false))
            {
                if (user.PokerInfo.Rank == Rank)
                {
                    Winners.Add(user);
                }
            }
            string logs = "[Poker]";

            foreach (var winner in Winners)
            {
                ulong win = 0;
                logs += winner.Player.Name + " ";
                foreach (var user in PlayersInfo.GetValues())
                {
                    if (user.UID != winner.Player.UID && !Winners.Contains(user.Owner))
                    {
                        if (user.MyBet > winner.PokerInfo.MyBet + table.MinBet)
                        {
                            user.MyBet -= winner.PokerInfo.MyBet;
                            user.MyRewardLost += winner.PokerInfo.MyBet;
                         win +=   winner.PokerInfo.MyReward += winner.PokerInfo.MyBet;
                        }
                        else
                        {
                        win +=    winner.PokerInfo.MyReward += user.MyBet;
                            user.MyRewardLost += user.MyBet;
                            user.MyBet = 0;
                        }
                    }
                }
                logs += win + " " + table.Type.ToString() + " ";
            }
            Database.ServerDatabase.LoginQueue.Enqueue(logs);
            foreach (var winner in Winners)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    foreach (var user in AvailablePlayers)
                    {
                        if (user.Player.UID != winner.Player.UID && !Winners.Contains(user))
                        {
                            switch (table.Type)
                            {
                                case Table.TableType.Silver:
                                    {
#if TEST
                                        Console.WriteLine(winner.Player.Name + " received back " + user.PokerInfo.MyBet);
#endif
                                        user.Player.Money += (long)user.PokerInfo.MyBet;
                                        user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                        break;
                                    }
                                case Table.TableType.ConquerPoints:
                                    {
                                        user.Player.ConquerPoints += (uint)user.PokerInfo.MyBet;
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            switch (table.Type)
                            {
                                case Table.TableType.Silver:
                                    {
#if TEST
                                        Console.WriteLine(winner.Player.Name + " win " + user.PokerInfo.MyReward);
#endif
                                        if (user.PokerInfo.SetMyBet == false)
                                        {
                                            user.PokerInfo.MyReward += (ulong)user.PokerInfo.MyBet;
                                            user.Player.Money += (long)user.PokerInfo.MyReward;
                                            user.PokerInfo.SetMyBet = true;
                                        }
                                        user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                        break;
                                    }
                                case Table.TableType.ConquerPoints:
                                    {
                                        if (user.PokerInfo.SetMyBet == false)
                                        {
                                            user.Player.ConquerPoints += (uint)user.PokerInfo.MyReward;
                                            user.Player.ConquerPoints += (uint)user.PokerInfo.MyBet;
                                            user.PokerInfo.SetMyBet = true;
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            return Winners;

        }
    }
}
