﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightConquer_Project.Game.MsgFloorItem;
using LightConquer_Project.Game.MsgServer;

namespace LightConquer_Project.Game.MsgTournaments
{
    public class MsgDBShower : ITournament
    {
        public string Name = "TC Meteorshower";
        public string Prize = "Meteors & Dragonballs";

        public ProcesType Process { get; set; }
        private DateTime StartTimer = new DateTime();
        private uint DinamicID = 0;
        private Role.GameMap BaseMap = Database.Server.ServerMaps[1002];
        private DateTime DbStamp = new DateTime();
        private DateTime RoundStamp = new DateTime();
        private byte AliveTime = 5;
        private bool PrepareToFinish = false;
        public TournamentType Type { get; set; }
        public MsgDBShower(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
        }
        public bool InTournament(Client.GameClient user)
        {
            return false;
        }
        public void Open()
        {
            if (Process == ProcesType.Dead)
            {
                PrepareToFinish = false;
                Process = ProcesType.Idle;
                StartTimer = DateTime.Now.AddMinutes(1);
                MsgSchedules.SendInvitation(Name, Prize, 430, 380, 1002, 0, 60);
                AliveTime = 5;
            }
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            return true;
        }

        public Client.GameClient[] MapUsers()
        {
            return Database.Server.GamePoll.Values.Where(user => user.Player.Map == 1002).ToArray();
        }

        public void CheckUp()
        {
            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer)
                {
                    Process = ProcesType.Alive;

                    MsgSchedules.SendSysMesage("Meteorshower has started now at twincity come to enjoy!", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        AddMapEffect(stream);
                    }
                    StartTimer = DateTime.Now.AddMinutes(5);
                    RoundStamp = DateTime.Now.AddMinutes(1);
                }
            }
            else if (Process == ProcesType.Alive)
            {

                CheckAddEffect();
                CheckAlivePlayers();

                if (DateTime.Now > StartTimer && PrepareToFinish == false)
                {
                    PrepareToFinish = true;
                    StartTimer = DateTime.Now.AddSeconds(3);

                }
                if (PrepareToFinish)
                {
                    if (DateTime.Now > StartTimer)
                        Process = ProcesType.Dead;
                    return;
                }


                if (DateTime.Now > RoundStamp)
                {
                    RoundStamp = DateTime.Now.AddMinutes(1);
                    AliveTime--;
                    MsgSchedules.SendSysMesage("Metoeshower will be finished in " + AliveTime.ToString() + " minutes", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    FinishRound();
                }
            }

        }
        public void CheckAlivePlayers()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var user in MapUsers())
                {
                    if (user.Player.Alive == false)
                        if (user.Player.DeadStamp.AddSeconds(3) < Extensions.Time32.Now)
                        {
                            TeleportRandom(user, stream);

                            SendMapColor(stream, user);
                        }
                }
            }
        }
        public unsafe void SendMapColor(ServerSockets.Packet stream, Client.GameClient user)
        {
            LightConquer_Project.Game.MsgServer.ActionQuery action = new LightConquer_Project.Game.MsgServer.ActionQuery()
            {
                ObjId = user.Player.UID,
                Type = LightConquer_Project.Game.MsgServer.ActionType.SetMapColor,
                dwParam = 16755370,
                wParam1 = user.Player.X,
                wParam2 = user.Player.Y
            };
            user.Send(stream.ActionCreate(&action));
        }
        public void FinishRound()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var user in MapUsers())
                    KillTarget(user, stream);

                for (int i = 0; i < MapUsers().Length / 2 + 1; i++)
                {
                    ushort x = 0;
                    ushort y = 0;
                    ushort startx = 400;
                    ushort starty = 300;
                    ushort endx = 500;
                    ushort endy = 400;
                    BaseMap.GetRandCoord(ref x, ref y, startx, starty, endx, endy);
                    DropDragonBall(x, y, stream);
                }
            }
        }
        public void KillFullMap()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var user in MapUsers())
                    KillTarget(user, stream);
            }
        }
        public void TeleportRandom(Client.GameClient user, ServerSockets.Packet stream)
        {
            ushort x = 0;
            ushort y = 0;
            BaseMap.GetRandCoord(ref x, ref y);
            user.Teleport(x, y, 1002);

            SendMapColor(stream, user);

        }
        public void CheckAddEffect()
        {
            if (DateTime.Now > DbStamp)
            {
                DbStamp = DateTime.Now.AddSeconds(15);
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    AddMapEffect(stream);
                }
            }
        }
        public void AddMapEffect(ServerSockets.Packet stream)
        {

            ushort x = 0;
            ushort y = 0;
            ushort startx = 400;
            ushort starty = 300;
            ushort endx = 500;
            ushort endy = 400;
            BaseMap.GetRandCoord(ref x, ref y, startx, starty, endx, endy);
            MsgServer.MsgGameItem item = new MsgServer.MsgGameItem();
            item.Color = (Role.Flags.Color)2;

            item.ITEM_ID = 17;

            MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(item, x, y, MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, BaseMap.ID
                   , 0, false, BaseMap, 4);


            if (BaseMap.EnqueueItem(DropItem))
            {
                DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Effect);
            }

        }
        public uint DropDBScroll = 0;
        public void DropDragonBall(ushort effectx, ushort effecty, ServerSockets.Packet stream)
        {

            CheckAttackTarget(effectx, effecty);

            ushort x = effectx;
            ushort y = effecty;

            DropDBScroll++;

            MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();


            uint Itemid = Database.ItemType.Meteor;
            if (DropDBScroll == 8)
            {
                DropDBScroll = 0;
                Itemid = Database.ItemType.DragonBall;
            }
            DataItem.ITEM_ID = Itemid;
            var DBItem = Database.Server.ItemsBase[Itemid];
            DataItem.Durability = DBItem.Durability;
            DataItem.MaximDurability = DBItem.Durability;
            DataItem.Color = Role.Flags.Color.Red;

            if (BaseMap.AddGroundItem(ref x, ref y))
            {

                MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, x, y, MsgFloorItem.MsgItem.ItemType.Item, 0, DinamicID, 700
                    , 0, false, BaseMap);

                if (BaseMap.EnqueueItem(DropItem))
                {
                    DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                }
            }
        }
        public void CheckAttackTarget(ushort x, ushort y)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var user in MapUsers())
                {
                    if (Role.Core.GetDistance(x, y, user.Player.X, user.Player.Y) <= 2)
                    {

                        KillTarget(user, stream);
                    }
                }
            }
        }
        public void KillTarget(Client.GameClient user, ServerSockets.Packet stream)
        {
            MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(434343
                                                      , 0, user.Player.X, user.Player.Y, 10130, 0, 0);
            SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(user.Player.UID, (uint)(user.Player.HitPoints + 100), MsgServer.MsgAttackPacket.AttackEffect.None));
            SpellPacket.SetStream(stream);
            SpellPacket.Send(user);

            user.Player.Dead(null, user.Player.X, user.Player.Y, 0);
        }
    }
}
