using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightConquer_Project.Game.MsgFloorItem;
using LightConquer_Project.Game.MsgNpc;
using LightConquer_Project.Database;
using System.IO;
using LightConquer_Project.Game.MsgTournaments;

namespace LightConquer_Project.Game.MsgServer
{
    public class MsgMessage
    {


        //  public static unsafe byte* UpdateProfiniency = new MsgMessage("You have just leveled your proficiency.", MsgColor.red, ChatMode.System).GetArray();
        //  public static unsafe byte* FULL_Inventory = new MsgMessage("Your Inventory Is Full!", MsgColor.red, ChatMode.TopLeft).GetArray();

        public enum MsgColor : uint
        {
            black = 0x000000,// 	0,0,0
            blue = 0x0000ff,// 	0,0,255
            orange = 0xffa500,// 	255,165,0

            white = 0xffffff,//	255,255,255
            whitesmoke = 0xf5f5f5,// 	245,245,245
            yellow = 0xffff00,// 	255,255,0
            yellowgreen = 0x9acd32,//	154,205,50
            violet = 0xee82ee,//	238,130,238
            purple = 0x800080,//	128,0,128
            red = 0xff0000,//	255,0,0
            pink = 0xffc0cb,// 	255,192,203
            lightyellow = 0xffffe0,// 	255,255,224
            cyan = 0x00ffff,// 	0,255,255
            blueviolet = 0x8a2be2,// 	138,43,226
            antiquewhite = 0xfaebd7,// 	250,235,215
        }
        public enum ChatMode : uint
        {
            Talk = 2000,
            Whisper = 2001,
            Team = 2003,
            Guild = 2004,
            TopLeftSystem = 2005,
            Clan = 2006,
            System = 2000,//2007,
            Friend = 2009,
            Center = 2011,
            TopLeft = 2012,
            Service = 2014,
            Tip = 2015,
            CrossServerIcon = 2016,
            Ally = 2025,
            WebSite = 2105,
            World = 2021,
            Qualifier = 2022,
            Study = 2024,
            JianHu = 2026,
            InnerPower = 2027,
            PopUP = 2100,
            Dialog = 2101,
            CrosTheServer = 2402,
            FirstRightCorner = 2108,
            ContinueRightCorner = 2109,
            SystemWhisper = 2110,
            GuildAnnouncement = 2111,
            SlideCrosTheServer = 2401,

            Agate = 2115,
            BroadcastMessage = 2500,
            Monster = 2600,
            SlideFromRight = 100000,
            HawkMessage = 2104,
            SlideFromRightRedVib = 1000000,
            WhiteVibrate = 10000000
        }

        public string _From;
        public string _To;
        public ChatMode ChatType;
        public uint Color;
        public string __Message;
        public string ServerName = string.Empty;

        public uint Mesh;
        public uint MessageUID1 = 0;
        public uint MessageUID2 = 0;

        public MsgMessage(string _Message, MsgColor _Color, ChatMode _ChatType)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = "ALL";
            this._From = "SYSTEM";
            this.Color = (uint)_Color;
            this.ChatType = _ChatType;
        }
        public MsgMessage(string _Message, string __To, MsgColor _Color, ChatMode _ChatType)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = __To;
            this._From = "SYSTEM";
            this.Color = (uint)_Color;
            this.ChatType = _ChatType;
        }
        public MsgMessage(string _Message, string __To, string __From, MsgColor _Color, ChatMode _ChatType)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = __To;
            this._From = __From;
            this.Color = (uint)_Color;
            this.ChatType = _ChatType;
        }
        public MsgMessage()
        {
            this.Mesh = 0;
        }
        public unsafe void Deserialize(ServerSockets.Packet stream)
        {
            stream.ReadUInt32();
            Color = stream.ReadUInt32();
            ChatType = (ChatMode)stream.ReadUInt32();
            MessageUID1 = stream.ReadUInt32();
            MessageUID2 = stream.ReadUInt32();
            Mesh = stream.ReadUInt32();//24
            uint unknow = stream.ReadUInt32();//28
            byte unknow2 = stream.ReadUInt8();//32
            byte unknow3 = stream.ReadUInt8();//33
            string[] str = stream.ReadStringList();//34

            _From = str[0];
            _To = str[1];
            __Message = str[3];
        }
        public unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, uint Rank = 0)
        {
            stream.InitWriter();

            stream.Write(Extensions.Time32.Now.Value);//4
            stream.Write(this.Color);//8
            stream.Write((uint)this.ChatType);//12

            stream.Write(MessageUID1);//16
            stream.Write(MessageUID2);//20
            stream.Write(Mesh);//24
            stream.Write((uint)Rank);//28 
            stream.Write((byte)0);//32
            stream.Write((byte)0);
            stream.Write(_From, _To, string.Empty, __Message, string.Empty, string.Empty, ServerName);
            stream.Finalize(GamePackets.Chat);
            return stream;

        }

        [PacketAttribute(GamePackets.Chat)]
        public unsafe static void MsgHandler(Client.GameClient client, ServerSockets.Packet packet)
        {
            MsgMessage msg = new MsgMessage();
            msg.Deserialize(packet);
            if (!ChatCommands(client, msg))
            {
                try
                {
                    string[] lines = msg.__Message.Split(new string[] { "[" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int x = 0; x < lines.Length; x++)
                    {
                        string str = lines[x];
                        if (str.Contains("Item "))
                        {
                            string[] line = str.Split(' ');//"[Item ", StringSplitOptions.None);
                            if (line != null && line.Length > 2)
                            {
                                uint UID = 0;
                                if (uint.TryParse(line[2], out UID))
                                {
                                    MsgGameItem msg_item;
                                    if (client.TryGetItem(UID, out msg_item))
                                    {
                                        Program.GlobalItems.Add(msg_item);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MyConsole.WriteException(e);
                }


                msg.Mesh = client.Player.Mesh;
                switch (msg.ChatType)
                {
                    case ChatMode.CrosTheServer:
                        {
                            if (client.Inventory.Contain(3002218, 1))
                            {
                                packet.Seek(packet.Size - 8);
                                packet.Finalize(1004);
                                if (client.Player.InUnion)
                                    MsgInterServer.StaticConnexion.Send(packet);//messag.GetArray(packet, (uint)Role.Instance.Union.Member.GetRank(client.Player.UnionMemeber.Rank)));
                                else
                                    MsgInterServer.StaticConnexion.Send(packet);//messag.GetArray(packet, (uint)Role.Instance.Union.Member.GetRank(client.Player.UnionMemeber.Rank)));

                                client.Inventory.Remove(3002218, 1, packet);

                            }
                            break;
                        }
                    case ChatMode.Ally:
                        {
                            if (client.Player.MyGuild != null)
                            {
                                foreach (var guild in client.Player.MyGuild.Ally.Values)
                                    guild.SendPacket(msg.GetArray(packet));
                            }
                            break;
                        }
                    case ChatMode.HawkMessage:
                        {
                            if (client.IsVendor)
                            {
                                client.MyVendor.HalkMeesaje = msg;

                                client.Player.View.SendView(msg.GetArray(packet), true);
                            }
                            break;
                        }
                    case ChatMode.Team:
                        {
                            if (client.Team != null)
                                client.Team.SendTeam(msg.GetArray(packet), client.Player.UID);
                            break;
                        }
                    case MsgMessage.ChatMode.Talk:
                        {
                            client.Player.View.SendView(msg.GetArray(packet), false);
                            break;
                        }
                    case MsgMessage.ChatMode.World:
                        {
                            if (Extensions.Time32.Now > client.Player.LastWorldMessaj.AddSeconds(15))
                            {
                                client.Player.LastWorldMessaj = Extensions.Time32.Now;
                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if (user.Player.UID != client.Player.UID)
                                    {
                                        if (user.Player.InUnion)
                                            user.Send(msg.GetArray(packet, (uint)Role.Instance.Union.Member.GetRank(user.Player.UnionMemeber.Rank)));
                                        else
                                            user.Send(msg.GetArray(packet));
                                    }
                                }
                                //  Program.SendGlobalPackets.Enqueue(msg.GetArray());
                            }
                            break;
                        }
                    case ChatMode.Whisper:
                        {
                            bool send = false;
                            foreach (var user in Database.Server.GamePoll.Values)
                            {
                                if (user.Player.Name == msg._To)
                                {
                                    msg.Mesh = client.Player.Mesh;
                                    user.Send(msg.GetArray(packet));
                                    send = true;
                                    break;
                                }
                            }
                            if (!send)
                            {
#if Arabic
                                client.SendSysMesage("The player is not online.", ChatMode.System, MsgColor.white);
#else
                                client.SendSysMesage("The player is not online.", ChatMode.System, MsgColor.white);
#endif

                            }
                            break;
                        }
                    case ChatMode.Guild:
                        {
                            if (client.Player.MyGuild != null)
                                client.Player.MyGuild.SendPacket(msg.GetArray(packet));
                            if (client.Player.MyGuild != null && client.Player.SendAllies)
                            {
                                msg._To = "[ALLIES]";
                                foreach (var guild in client.Player.MyGuild.Ally.Values)
                                    guild.SendPacket(msg.GetArray(packet));
                            }
                            break;
                        }
                    case ChatMode.Friend:
                        {
                            System.Collections.Concurrent.ConcurrentDictionary<uint, Role.Instance.Associate.Member> friends;
                            if (client.Player.Associate.Associat.TryGetValue(Role.Instance.Associate.Friends, out friends))
                            {
                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if (friends.ContainsKey(user.Player.UID))
                                        user.Send(msg.GetArray(packet));
                                }
                            }
                            break;
                        }
                    case ChatMode.Clan:
                        {
                            if (client.Player.MyClan != null)
                                client.Player.MyClan.Send(msg.GetArray(packet));
                            break;
                        }
                }

            }
        }
        public static uint TestGui = 0;


        public static unsafe bool ChatCommands(Client.GameClient client, MsgMessage msg)
        {
            string logss = "[Chat]" + msg._From + " to " + msg._To + " " + msg.__Message + "";
            Database.ServerDatabase.LoginQueue.Enqueue(logss);
            msg.__Message = msg.__Message.Replace("#60", "").Replace("#61", "").Replace("#62", "").Replace("#63", "").Replace("#64", "").Replace("#65", "").Replace("#66", "").Replace("#67", "").Replace("#68", "");

            if (msg.__Message.StartsWith("@"))
            {
                string logs = "[GMLogs]" + client.Player.Name + " ";

                string Message = msg.__Message.Substring(1);//.ToLower();
                string[] data = Message.Split(' ');
                for (int x = 0; x < data.Length; x++)
                    logs += data[x] + " ";
                Database.ServerDatabase.LoginQueue.Enqueue(logs);
                switch (data[0])
                #region Normal Players
                {
                    //case "uplev":
                    //    {
                    //        using (var rec = new ServerSockets.RecycledPacket())
                    //        {
                    //            var stream = rec.GetStream();
                    //            client.Team.AddPlayer(stream, client, client.Player);
                    //        }
                    //        break;
                    //    }
                    case "duel":
                        if (client.Dueler != 0 && Database.Server.GamePoll.ContainsKey(client.Dueler) && Database.Server.GamePoll[client.Dueler].Player.Map == client.Player.Map)
                        {
                            if (Database.Server.GamePoll[client.Dueler].Arena != null)
                            {
                                if (!Database.Server.GamePoll[client.Dueler].Arena.Wager)
                                {
                                    client.Arena = Database.Server.GamePoll[client.Dueler].Arena;
                                    if (client.Arena.MapID != client.Player.Map)
                                        client.Arena.AcceptDuel(client, Database.Server.GamePoll[client.Dueler]);
                                }
                                else
                                    client.SendSysMesage("To accept duels with waggers please type @duel");
                            }
                            else
                            {
                                client.Dueler = 0;
                                client.SendSysMesage("Duel invitation expired! Send a new invitation!");
                            }
                        }
                        else
                        {
                            client.Dueler = 0;
                            client.SendSysMesage("Your opponent is either offline or in a different map!");
                        }
                        break;
                    case "quitduel":
                        if (client.Arena != null && client.Arena.MapID == client.Player.Map)
                            client.Arena.RemovePlayer(client);
                        else
                            client.SendSysMesage("You can't quit a duel if you're not in one.");
                        break;
                    case "acceptwager":
                        if (client.Dueler != 0 && Database.Server.GamePoll.ContainsKey(client.Dueler) && Database.Server.GamePoll[client.Dueler].Player.Map == client.Player.Map)
                        {
                            if (Database.Server.GamePoll[client.Dueler].Arena != null)
                            {
                                if (Database.Server.GamePoll[client.Dueler].Arena.Wager)
                                {
                                    client.Arena = Database.Server.GamePoll[client.Dueler].Arena;
                                    if (client.Arena.MapID != client.Player.Map)
                                        client.Arena.AcceptDuel(client, Database.Server.GamePoll[client.Dueler]);
                                }
                                else
                                    client.SendSysMesage("To accept normal duels please type @duel");
                            }
                            else
                            {
                                client.Dueler = 0;
                                client.SendSysMesage("Duel invitation expired! Send a new invitation!");
                            }
                        }
                        else
                        {
                            client.Dueler = 0;
                            client.SendSysMesage("Your opponent is either offline or in a different map!");
                        }
                        break;
                    case "pack":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (client.Inventory.Contain(Database.ItemType.Meteor, 10, 0))
                                {
                                    client.Inventory.Remove(Database.ItemType.Meteor, 10, stream);
                                    client.Inventory.Add(stream, Database.ItemType.MeteorScroll, 1);
                                    client.SendSysMesage("Your Meteors packed! ");

                                }
                                else if (client.Inventory.Contain(Database.ItemType.DragonBall, 10, 0))
                                {
                                    client.Inventory.Remove(Database.ItemType.DragonBall, 10, stream);
                                    client.Inventory.Add(stream, Database.ItemType.DragonBallScroll, 1);
                                    client.SendSysMesage("Your DragonBall packed! ");

                                }
                                break;
                            }
                        }
                    case "leave":
                        {
                            if (UnlimitedArenaRooms.Maps.ContainsValue(client.Player.DynamicID))
                                client.Teleport(1002, 428, 378);
                            break;
                        }
                    case "stuck":
                        {
                            if (client.Player.Map == 1002)
                                client.Teleport(1002, 428, 378);
                            break;
                        }
                    case "allieschat":
                        client.Player.SendAllies = !client.Player.SendAllies;
                        client.SendSysMesage($"Allies Chat mode: {client.Player.SendAllies}");
                        break;
                    case "resetscores":
                        client.Player.TotalHits = client.Player.Hits = client.Player.Chains = client.Player.MaxChains = 0;
                        break;
                    case "joinpvp":
                        if (client.EventBase == null)
                        {
                            if (Program.Events.Count > 0)
                            {
                                if (Program.Events.Count == 1)
                                {
                                    if (Program.Events[0].AddPlayer(client))
                                        client.EventBase = Program.Events[0];
                                }
                            }
                            else
                                client.SendSysMesage("There are no PVP Events running!");
                        }
                        break;
                    case "dc":
                        client.Socket.Disconnect();
                        break;
                    case "visualeffects":
                        client.Player.ShowGemEffects = !client.Player.ShowGemEffects;
                        client.SendSysMesage("Visual effects status: " + client.Player.ShowGemEffects);
                        break;
                    case "clearinventory":
                    case "clearinv":
                    case "clear":
                        {
                            client.Player.MessageBox("Clearing your inventory would delete your items! You cant undo it. Are you sure?", (p) =>
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Inventory.Clear(rec.GetStream());
                            }, null, 60);
                            break;
                        }
                    case "agi":
                        {
                            ushort atr = 0;
                            ushort.TryParse(data[1], out atr);
                            if (atr > 540)
                            {
                                client.SendSysMesage("You have write a wrong number try again with a correct number ?!.");
                                break;
                            }
                            if (client.Player.Atributes >= (ushort)atr)
                            {
                                client.Player.Agility += (ushort)atr;
                                client.Player.Atributes -= (ushort)atr;
                                client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                            }
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                            }
                            break;
                        }
                    case "str":
                        {
                            ushort atr = 0;
                            ushort.TryParse(data[1], out atr);
                            if (atr > 540)
                            {
                                client.SendSysMesage("You have write a wrong number try again with a correct number ?!.");
                                break;
                            }
                            if (client.Player.Atributes >= (ushort)atr)
                            {
                                client.Player.Strength += (ushort)atr;
                                client.Player.Atributes -= (ushort)atr;
                                client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                            }
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                            }
                            break;
                        }
                    case "vit":
                        {
                            ushort atr = 0;
                            ushort.TryParse(data[1], out atr);
                            if (atr > 540)
                            {
                                client.SendSysMesage("You have write a wrong number try again with a correct number ?!.");
                                break;
                            }
                            if (client.Player.Atributes >= (ushort)atr)
                            {
                                client.Player.Vitality += (ushort)atr;
                                client.Player.Atributes -= (ushort)atr;
                                client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                            }
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                            }
                            break;
                        }
                    case "spi":
                        {
                            ushort atr = 0;
                            ushort.TryParse(data[1], out atr);
                            if (atr > 540)
                            {
                                client.SendSysMesage("You have write a wrong number try again with a correct number ?!.");
                                break;
                            }
                            if (client.Player.Atributes >= (ushort)atr)
                            {
                                client.Player.Spirit += (ushort)atr;
                                client.Player.Atributes -= (ushort)atr;
                                client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                            }
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                            }
                            break;
                        }
                    case "vipinfo":
                        {
                            if (client.Player.VipLevel >= 5)
                            {
                                TimeSpan timer1 = new TimeSpan(client.Player.ExpireVip.Ticks);
                                TimeSpan Now2 = new TimeSpan(DateTime.Now.Ticks);
                                int days_left = (int)(timer1.TotalDays - Now2.TotalDays);
                                int hour_left = (int)(timer1.TotalHours - Now2.TotalHours);
                                int left_minutes = (int)(timer1.TotalMinutes - Now2.TotalMinutes);
                                if (days_left > 0)
                                    client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + days_left + " days and " + hour_left + " hours and " + left_minutes + " minutes.", MsgMessage.ChatMode.System);
                                else if (hour_left > 0)
                                    client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + hour_left + " hours and " + left_minutes + " minutes.", MsgMessage.ChatMode.System);
                                else if (left_minutes > 0)
                                    client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + left_minutes + " minutes.", MsgMessage.ChatMode.System);

                            }
                            else client.SendSysMesage("You`re not VIP-5.");
                            break;
                        }
                }
                #endregion
                #region VipLevel == 6
                if (client.Player.VipLevel == 6)
                {
                    switch (data[0])
                    {
                        case "vipinfo":
                            {
                                if (client.Player.VipLevel >= 1)
                                {
                                    TimeSpan timer1 = new TimeSpan(client.Player.ExpireVip.Ticks);
                                    TimeSpan Now2 = new TimeSpan(DateTime.Now.Ticks);
                                    int days_left = (int)(timer1.TotalDays - Now2.TotalDays);
                                    int hour_left = (int)(timer1.TotalHours - Now2.TotalHours);
                                    int left_minutes = (int)(timer1.TotalMinutes - Now2.TotalMinutes);
                                    if (days_left > 0)
                                        client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + days_left + " days and " + hour_left + " hours and " + left_minutes + " minutes.", MsgMessage.ChatMode.System);
                                    else if (hour_left > 0)
                                        client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + hour_left + " hours and " + left_minutes + " minutes.", MsgMessage.ChatMode.System);
                                    else if (left_minutes > 0)
                                        client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + left_minutes + " minutes.", MsgMessage.ChatMode.System);

                                }
                                else client.SendSysMesage("You`re not VIP.");
                                break;
                            }
                        case "joinpvp":
                            if (client.EventBase == null)
                            {
                                if (Program.Events.Count > 0)
                                {
                                    if (Program.Events.Count == 1)
                                    {
                                        if (Program.Events[0].AddPlayer(client))
                                            client.EventBase = Program.Events[0];
                                    }
                                }
                                else
                                    client.SendSysMesage("There are no PVP Events running!");
                            }
                            break;

                        case "summonguild":
                            {
                                if (!client.Player.OnMyOwnServer)
                                    break;

                                if (Program.BlockTeleportMap.Contains(client.Player.Map) || client.Player.Map == 1038)
                                {
                                    client.SendSysMesage("You can`t use it in " + client.Map.Name + " ");
                                    break;
                                }
                                if (client.Player.MyGuild == null || client.Player.GuildRank != Role.Flags.GuildMemberRank.GuildLeader)
                                {
                                    client.SendSysMesage("You need to be GuildLeader to use this!");
                                    break;
                                }
                                if (DateTime.Now < client.Player.SummonGuild.AddMinutes(5))
                                {
                                    client.SendSysMesage("You need to wait 5 minutes before summoning again.");
                                    break;
                                }

                                client.Player.SummonGuild = DateTime.Now;
                                foreach (var member in Server.GamePoll.Values.Where(e => e.Player.GuildID != 0 && e.Player.GuildID == client.Player.GuildID
                                    && e.Player.UID != client.Player.UID))
                                {
                                    member.Player.MessageBox("Your guild leader has summoned you to " + client.Map.Name + "! Would you like to go?",
                                        new Action<Client.GameClient>(user => user.Teleport(client.Player.Map,
                                            (ushort)(client.Player.X + Role.Core.Random.Next(0, 5)), (ushort)(client.Player.Y + Role.Core.Random.Next(0, 5)), client.Player.DynamicID)),
                                        null, 60);
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(
                                        client.Player.Name + " guild leader of " + client.Player.MyGuild.GuildName + " has summoned his members to " + client.Map.Name, "ALLUSERS",
                                        MsgColor.red, ChatMode.TopLeft).GetArray(stream));
                                }
                                break;
                            }

                    }
                }
                #endregion

            }

            if (!client.ProjectManager && client.GameMaster == false)
            {
                msg.__Message = msg.__Message.Replace("#60", "").Replace("#61", "").Replace("#62", "").Replace("#63", "").Replace("#64", "").Replace("#65", "").Replace("#66", "").Replace("#67", "").Replace("#68", "");

                if (msg.__Message.StartsWith("@"))
                {
                    string logs = "[GMLogs]" + client.Player.Name + " ";

                    string Message = msg.__Message.Substring(1);//.ToLower();
                    string[] data = Message.Split(' ');
                    for (int x = 0; x < data.Length; x++)
                        logs += data[x] + " ";
                    Database.ServerDatabase.LoginQueue.Enqueue(logs);
                    switch (data[0])
                    {
                        case "pass":
                            {
                                if (client.Player.Name.Contains("[GM]"))
                                {
                                    if (data[1] == "1989")
                                        client.GameMaster = true;
                                    client.SendSysMesage(client.Player.Name + "  Commands work now ur a GameMaster");

                                }
                                if (client.Player.Name.Contains("[PM]"))
                                {

                                    if (data[1] == "1989")
                                        client.ProjectManager = true;
                                    client.SendSysMesage(client.Player.Name + "  Commands work now ur a ProjectManager");

                                }
                                return true;
                            }
                    }
                }
                return false;
            }
            if (client.ProjectManager == false && client.GameMaster == false)
                return false;
            msg.__Message = msg.__Message.Replace("#60", "").Replace("#61", "").Replace("#62", "").Replace("#63", "").Replace("#64", "").Replace("#65", "").Replace("#66", "").Replace("#67", "").Replace("#68", "");
            if (msg.__Message.StartsWith("@"))
            {
                string Message = msg.__Message.Substring(1);//.ToLower();
                string[] data = Message.Split(' ');
                string logs = "[GMLogs]" + client.Player.Name + " ";
                for (int x = 0; x < data.Length; x++)
                    logs += data[x] + " ";
                Database.ServerDatabase.LoginQueue.Enqueue(logs);
                #region GameMaster
                if (client.GameMaster)
                {

                    switch (data[0])
                    {
                        case "opengui"://@opengui 3250
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    ActionQuery action = new ActionQuery()
                                    {
                                        ObjId = client.Player.UID,
                                        Type = ActionType.OpenCustom,
                                        Timestamp = (int)Extensions.Time32.Now.Value,
                                        dwParam = uint.Parse(data[1]),
                                        wParam1 = client.Player.X,
                                        wParam2 = client.Player.Y,

                                    };
                                    client.Send(stream.ActionCreate(&action));


                                }
                                break;
                            }
                        case "reborn":
                            {
                                client.Player.Reborn = byte.Parse(data[1]);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Reborn, MsgUpdate.DataType.Reborn);
                                }
                                break;
                            }
                        case "class":
                            {
                                client.Player.Class = byte.Parse(data[1]);
                                break;
                            }
                        case "spell":
                            {
                                ushort ID = 0;
                                if (!ushort.TryParse(data[1], out ID))
                                {
                                    client.SendSysMesage("Invlid spell ID !");
                                    break;
                                }
                                byte level = 0;
                                if (!byte.TryParse(data[2], out level))
                                {
                                    client.SendSysMesage("Invlid spell Level ! ");
                                    break;
                                }
                                byte levelHu = 0;

                                if (!byte.TryParse(data[3], out levelHu))
                                {
                                    client.SendSysMesage("Invlid spell Level Souls ! ");
                                    break;
                                }

                                int Experience = 0;
                                if (!int.TryParse(data[4], out Experience))
                                {
                                    client.SendSysMesage("Invlid spell Experience ! ");
                                    break;
                                }

                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.MySpells.Add(rec.GetStream(), ID, level, levelHu, 0, Experience);
                                break;
                            }
                        case "prof":
                            {
                                ushort ID = 0;
                                if (!ushort.TryParse(data[1], out ID))
                                {
                                    client.SendSysMesage("Invlid prof ID !");
                                    break;
                                }
                                byte level = 0;
                                if (!byte.TryParse(data[2], out level))
                                {
                                    client.SendSysMesage("Invlid prof Level ! ");
                                    break;
                                }
                                uint Experience = 0;
                                if (!uint.TryParse(data[3], out Experience))
                                {
                                    client.SendSysMesage("Invlid prof Experience ! ");
                                    break;
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.MyProfs.Add(rec.GetStream(), ID, level, Experience);
                                break;
                            }
                        case "clearinventory":
                        case "clearinv":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Inventory.Clear(rec.GetStream());
                                break;
                            }
                        case "level":
                            {
                                byte amount = 0;
                                if (byte.TryParse(data[1], out amount))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.UpdateLevel(stream, amount, true);
                                    }
                                }
                                break;
                            }

                        case "superman":
                            {
                                client.Player.Vitality += 1000;
                                client.Player.Strength += 1000;
                                client.Player.Spirit += 1000;
                                client.Player.Agility += 1000;

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);

                                }
                                break;
                            }
                        case "resetstats":
                            {
                                client.Player.Vitality = 0;
                                client.Player.Strength = 0;
                                client.Player.Spirit = 0;
                                client.Player.Agility = 0;

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);

                                }
                                break;
                            }

                        case "info":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    foreach (var user in Database.Server.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower() == data[1].ToLower())
                                        {

                                            client.Send(new MsgMessage("[Info" + user.Player.Name + "]", MsgColor.yellow, ChatMode.FirstRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("UID = " + user.Player.UID + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("IP = " + user.Socket.RemoteIp + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("ConquerPoints = " + user.Player.ConquerPoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("BoundConquerPoints = " + user.Player.BoundConquerPoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("Money = " + user.Player.Money + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("DonationPoints = " + user.Player.RacePoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("Dollars = " + user.Player.DonatePoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("VIP info = VIPlevel" + user.Player.VipLevel + "\n VIP Expire" + user.Player.ExpireVip + "\n", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            var list = MsgLoginClient.PlayersIP.Where(e => e.Key == user.IP).FirstOrDefault();
                                            client.Send(new MsgMessage("----- \n", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            foreach (var pl in list.Value)
                                                client.Send(new MsgMessage(pl, MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        case "scroll":
                            {
                                switch (data[1].ToLower())
                                {
                                    case "tc": client.Teleport(1002, 428, 378); break;
                                    case "pc": client.Teleport(1011, 195, 260); break;
                                    case "ac":
                                    case "am": client.Teleport(1020, 566, 563); break;
                                    case "dc": client.Teleport(1000, 500, 645); break;
                                    case "bi": client.Teleport(1015, 723, 573); break;
                                    case "pka": client.Teleport(1005, 050, 050); break;
                                    case "ma": client.Teleport(1036, 211, 196); break;
                                    case "ja": client.Teleport(6000, 100, 100); break;
                                }
                                break;
                            }

                        case "invisible":
                            {
                                client.SendSysMesage("you are invisible now !");
                                client.Player.Invisible = true;
                                break;

                            }
                        case "visible":
                            {
                                client.SendSysMesage("you are visible now !");
                                client.Player.Invisible = false;
                                break;
                            }
                        case "revive":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Player.Revive(rec.GetStream());

                                break;
                            }

                        case "tele":
                            {

                                client.TerainMask = 0;
                                uint mapid = 0;
                                if (!uint.TryParse(data[1], out mapid))
                                {
                                    client.SendSysMesage("Invlid Map ID !");
                                    break;
                                }
                                ushort X = 0;
                                if (!ushort.TryParse(data[2], out X))
                                {
                                    client.SendSysMesage("Invlid X !");
                                    break;
                                }
                                ushort Y = 0;
                                if (!ushort.TryParse(data[3], out Y))
                                {
                                    client.SendSysMesage("Invlid Y !");
                                    break;
                                }
                                uint DinamicID = 0;
                                if (!uint.TryParse(data[4], out DinamicID))
                                {
                                    client.SendSysMesage("Invlid DinamicID !");
                                    break;
                                }
                                client.Teleport((ushort)mapid, X, Y, DinamicID);

                                break;
                            }
                        case "trace":
                            {
                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower().Contains(data[1].ToLower()))
                                    {
                                        client.Teleport(user.Player.Map, user.Player.X, user.Player.Y, user.Player.DynamicID);
                                        break;
                                    }
                                }

                                break;
                            }
                        case "bring":
                            {
                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower() == data[1].ToLower())
                                    {
                                        user.Teleport(client.Player.Map, client.Player.X, client.Player.Y);
                                        break;
                                    }
                                }
                                break;
                            }
                    }
                    return true;
                }
                #endregion

                #region ProjectManager
                switch (data[0])
                {
                    case "opengui":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                ActionQuery action = new ActionQuery()
                                {
                                    ObjId = client.Player.UID,
                                    Type = ActionType.OpenCustom,
                                    Timestamp = (int)Extensions.Time32.Now.Value,
                                    dwParam = uint.Parse(data[1]),
                                    wParam1 = client.Player.X,
                                    wParam2 = client.Player.Y,

                                };
                                client.Send(stream.ActionCreate(&action));


                            }
                            break;
                        }
                    #region addspawns
                    case "addspawns":
                        {
                            ushort mobid = ushort.Parse(data[1]);
                            byte amount = byte.Parse(data[2]);
                            byte radius = byte.Parse(data[3]);
                            byte freq = byte.Parse(data[4]);

                            ushort X = (ushort)(client.Player.X - radius / 2.0);
                            ushort Y = (ushort)(client.Player.Y - radius / 2.0);

                            if (!client.Map.ValidLocation(X, Y))
                            {
                                client.SendSysMesage("Invalid (X,Y)");
                                break;
                            }
                            ushort BoundX = (ushort)(radius * 2), BoundY = (ushort)(radius * 2);
                            var MapId = client.Player.Map;
                            Game.MsgMonster.MobCollection colletion = new Game.MsgMonster.MobCollection(MapId);
                            if (MapId == 8800)
                            {

                            }
                            if (colletion.ReadMap())
                            {

                                colletion.LocationSpawn = "";
                                Game.MsgMonster.MonsterFamily famil;
                                if (!Server.MonsterFamilies.TryGetValue(mobid, out famil))
                                {
                                    client.SendSysMesage("Invalid Monster Id");
                                    break;
                                }
                                if (Game.MsgMonster.MonsterRole.SpecialMonsters.Contains(famil.ID))
                                {
                                    client.SendSysMesage("You cant add spawns for this boss.");
                                    break;
                                }
                                Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                                Monster.SpawnX = X;
                                Monster.SpawnY = Y;
                                Monster.MaxSpawnX = (ushort)(Monster.SpawnX + BoundX);
                                Monster.MaxSpawnY = (ushort)(Monster.SpawnY + BoundY);
                                Monster.MapID = MapId;
                                Monster.SpawnCount = amount;//"maxnpc", 0);//max_per_gen", 0);
                                                            //if (Monster.ID == 18)
                                                            //    Monster.SpawnCount *= 2;
                                Monster.rest_secs = freq;

                                Monster.SpawnCount = amount;
                                colletion.Add(Monster);
                                using (var stream = new StreamWriter(Program.ServerConfig.DbLocation + "\\Spawns.txt", true))
                                {
                                    stream.WriteLine($"{mobid},{MapId},{X},{Y},{BoundX},{BoundY},{amount},{freq},{amount}");
                                    stream.Close(); ;
                                    client.SendSysMesage("Saved Spawn.");
                                }
                            }
                            else
                                client.SendSysMesage("Failed to make this spawn.");

                            break;
                        }
                    #endregion
                    case "xpm":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var map = Database.Server.ServerMaps[1004];
                                var stream = rec.GetStream();
                                Database.Server.AddMapMonster(stream, map, 36380, 51, 49, 0, 0, 1);
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("The XPMob have spawned in the PromotionCenter ! Hurry to get more XP.", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                            }
                            break;
                        }
                    case "bringall":
                        {
                            foreach (var user in Database.Server.GamePoll.Values)
                            {
                                if (client.Player.UID != client.Player.UID)
                                {
                                    user.Teleport(client.Player.Map, client.Player.X, client.Player.Y);
                                    break;
                                }
                            }
                            break;
                        }
                    case "quest":
                        {
                            client.Player.DragonWar = 1;
                            client.Player.FFa = 1;
                            client.Player.FreezeWar = 1;
                            client.Player.Get3Out = 1;
                            client.Player.Get5Out = 1;
                            client.Player.LastManStand = 1;
                            client.Player.PassTheBomb = 1;
                            client.Player.SkillChampionship = 1;
                            client.Player.skillmaster = 1;
                            client.Player.ZombieWar = 1;
                            break;
                        }
                    case "tpvp":
                        {
                            Game.MsgEvents.Events _S = new Game.MsgEvents.Events();
                            switch (data[1])
                            {

                                case "DragonWar":
                                    _S = new Game.MsgEvents.DragonWar();//done 1
                                    break;

                                case "FreezeWar":
                                    _S = new Game.MsgEvents.FreezeWar();//done 3
                                    break;
                                case "Get5Out":
                                    _S = new Game.MsgEvents.Get5Out();//done 4
                                    break;


                                case "LadderTournament":
                                    _S = new Game.MsgEvents.EliteLadderTournament();//done 7
                                    break;
                                case "LastManStand":
                                    _S = new Game.MsgEvents.LastManStand();//done 8
                                    break;
                                case "PassTheBomb":
                                    _S = new Game.MsgEvents.PTB();//done 9111111111111111111111111
                                    break;

                                case "TDM":
                                    _S = new Game.MsgEvents.TDM();//done 12
                                    break;

                                case "HalloweenInfection":
                                    _S = new Game.MsgEvents.HalloweenInfection();//done 15111111111111111111111111111
                                    break;

                                case "skillmaster":
                                    _S = new Game.MsgEvents.skillmaster();//done 17
                                    break;
                                case "Get3Out":
                                    _S = new Game.MsgEvents.Get3Out();//done 18
                                    break;
                                case "FFa":
                                    _S = new Game.MsgEvents.FFa();//done 19
                                    break;
                            }
                            _S.StartTournament(19);
                            break;
                        }

                    case "bc":
                    case "sm":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(data[1], "ALLUSERS", MsgColor.red, ChatMode.Center).GetArray(stream));
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(data[1], "ALLUSERS", MsgColor.red, ChatMode.System).GetArray(stream));
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(data[1], "ALLUSERS", MsgColor.red, ChatMode.SystemWhisper).GetArray(stream));

                            }
                            break;
                        }
                    case "link":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(data[1], "ALLUSERS", MsgColor.white, ChatMode.WebSite).GetArray(stream));
                            }
                            break;
                        }
                    case "inv":
                        {
                            client.SendSysMesage("you are invisible now !");
                            client.Player.Invisible = true;
                            break;

                        }
                    case "uninv":
                        {
                            client.SendSysMesage("you are visible now !");
                            client.Player.Invisible = false;
                            break;
                        }



                    case "clearspells":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                foreach (var spell in client.MySpells.ClientSpells.Values)
                                    client.MySpells.Remove(spell.ID, stream);
                            }
                            break;
                        }

                    case "superman":
                        {
                            client.Player.Vitality += 1500;
                            client.Player.Strength += 1500;
                            client.Player.Spirit += 1500;
                            client.Player.Agility += 1500;

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);

                            }
                            break;
                        }
                    case "resetstats":
                        {
                            client.Player.Vitality = 0;
                            client.Player.Strength = 0;
                            client.Player.Spirit = 0;
                            client.Player.Agility = 0;

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);

                            }
                            break;
                        }

                    case "give":
                        {
                            foreach (var user in Database.Server.GamePoll.Values)
                            {
                                if (user.Player.Name.ToLower() == data[1].ToLower())
                                {

                                    switch (data[2])
                                    {
                                        case "innerpotency":
                                            {
                                                int amount = 0;
                                                if (int.TryParse(data[3], out amount))
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        user.Player.InnerPower.AddPotency(stream, user, amount);
                                                        user.CreateBoxDialog("You receive " + amount + " InnerPower Potency.");
                                                    }
                                                }
                                                break;
                                            }
                                        case "level":
                                            {
                                                byte amount = 0;
                                                if (byte.TryParse(data[3], out amount))
                                                {
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        user.UpdateLevel(stream, amount, true);
                                                    }
                                                }
                                                break;
                                            }
                                        case "money":
                                            {
                                                user.Player.Money += long.Parse(data[3]); using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();
                                                    user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                                }
                                                break;
                                            }
                                        case "cps":
                                            {
                                                user.Player.ConquerPoints += uint.Parse(data[3]);

                                                break;
                                            }

                                        case "item":
                                            {
                                                uint ID = 0;
                                                if (!uint.TryParse(data[3], out ID))
                                                {
                                                    client.SendSysMesage("Invlid item ID !");
                                                    break;
                                                }
                                                byte plus = 0;
                                                if (!byte.TryParse(data[4], out plus))
                                                {
                                                    client.SendSysMesage("Invlid item plus !");
                                                    break;
                                                }
                                                byte bless = 0;
                                                if (!byte.TryParse(data[5], out bless))
                                                {
                                                    client.SendSysMesage("Invlid item Enchant !");
                                                    break;
                                                }
                                                byte enchant = 0;
                                                if (!byte.TryParse(data[6], out enchant))
                                                {
                                                    client.SendSysMesage("Invlid item Enchant !");
                                                    break;
                                                }
                                                byte sockone = 0;
                                                if (!byte.TryParse(data[7], out sockone))
                                                {
                                                    client.SendSysMesage("Invlid item Socket One !");
                                                    break;
                                                }
                                                byte socktwo = 0;
                                                if (!byte.TryParse(data[8], out socktwo))
                                                {
                                                    client.SendSysMesage("Invlid item Socket Two !");
                                                    break;
                                                }
                                                byte count = 1;
                                                if (data.Length > 9)
                                                {
                                                    if (!byte.TryParse(data[9], out count))
                                                    {
                                                        client.SendSysMesage("Invlid item count !");
                                                        break;
                                                    }
                                                }
                                                byte Effect = 0;
                                                if (data.Length > 10)
                                                {
                                                    if (!byte.TryParse(data[10], out Effect))
                                                    {
                                                        client.SendSysMesage("Invlid Effect Type !");
                                                        break;
                                                    }
                                                }
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                    user.Inventory.Add(rec.GetStream(), ID, count, plus, bless, enchant, (Role.Flags.Gem)sockone, (Role.Flags.Gem)socktwo, false, (Role.Flags.ItemEffect)Effect);

                                                break;
                                            }
                                    }
                                    break;
                                }
                            }
                            break;
                        }

                    case "botjail":
                        {
                            foreach (var user in Database.Server.GamePoll.Values)
                            {
                                if (user.Player.Name.ToLower() == data[1].ToLower())
                                {
                                    if (user.BanCount == 0)
                                    {
                                        user.Teleport(6002, 036, 079);
                                    }
                                    else
                                    {
                                        client.SendSysMesage("this not the 1st time he used cheats");
                                        break;
                                    }
                                    user.BanCount += 1;
                                    Console.WriteLine("" + user.Player.Name + " Sent to BotJail, because was found using programs that are illegal in game.");
                                    if (user.BanCount == 0)
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("" + user.Player.Name + " Sent to BotJail, because was found using programs that are illegal in game.", "ALLUSERS", MsgColor.red, ChatMode.Center).GetArray(stream));
                                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("" + user.Player.Name + " Sent to BotJail, because was found using programs that are illegal in game.", "ALLUSERS", MsgColor.red, ChatMode.System).GetArray(stream));
                                        }
                                    }
                                    else
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("" + user.Player.Name + " Sent to BotJail, because was found using programs that are illegal in game.", "ALLUSERS", MsgColor.red, ChatMode.Center).GetArray(stream));
                                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("" + user.Player.Name + " Sent to BotJail, because was found using programs that are illegal in game.", "ALLUSERS", MsgColor.red, ChatMode.System).GetArray(stream));
                                        }
                                    }
                                }
                            }
                            break;
                        }

                    case "kick":
                        {
                            foreach (var user in Database.Server.GamePoll.Values)
                            {
                                if (user.Player.Name.ToLower() == data[1].ToLower())
                                {
                                    user.Socket.Disconnect();
                                    break;
                                }
                            }
                            break;
                        }
                    case "revive":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                                client.Player.Revive(rec.GetStream());

                            break;
                        }
                    case "online":
                        {
                            client.SendSysMesage("Online Players : " + Database.Server.GamePoll.Count + " ");
                            client.SendSysMesage("Max Online Players : " + KernelThread.MaxOnline + " ");
                            break;
                        }
                    case "info":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower() == data[1].ToLower())
                                    {

                                        client.Send(new MsgMessage("[Info" + user.Player.Name + "]", MsgColor.yellow, ChatMode.FirstRightCorner).GetArray(stream));
                                        client.Send(new MsgMessage("UID = " + user.Player.UID + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        client.Send(new MsgMessage("IP = " + user.Socket.RemoteIp + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        client.Send(new MsgMessage("ConquerPoints = " + user.Player.ConquerPoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        client.Send(new MsgMessage("BoundConquerPoints = " + user.Player.BoundConquerPoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        client.Send(new MsgMessage("Money = " + user.Player.Money + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        client.Send(new MsgMessage("DonationPoints = " + user.Player.RacePoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        client.Send(new MsgMessage("Dollars = " + user.Player.DonatePoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        client.Send(new MsgMessage("VIP info = VIPlevel" + user.Player.VipLevel + "\n VIP Expire" + user.Player.ExpireVip + "\n", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        var list = MsgLoginClient.PlayersIP.Where(e => e.Key == user.IP).FirstOrDefault();
                                        client.Send(new MsgMessage("----- \n", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        foreach (var pl in list.Value)
                                            client.Send(new MsgMessage(pl, MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case "scroll":
                        {
                            switch (data[1].ToLower())
                            {
                                case "tc": client.Teleport(1002, 428, 378); break;
                                case "pc": client.Teleport(1011, 195, 260); break;
                                case "ac":
                                case "am": client.Teleport(1020, 566, 563); break;
                                case "dc": client.Teleport(1000, 500, 645); break;
                                case "bi": client.Teleport(1015, 723, 573); break;
                                case "pka": client.Teleport(1005, 050, 050); break;
                                case "ma": client.Teleport(1036, 211, 196); break;
                                case "ja": client.Teleport(6000, 100, 100); break;
                            }
                            break;
                        }
                    case "trace":
                        {
                            foreach (var user in Database.Server.GamePoll.Values)
                            {
                                if (user.Player.Name.ToLower().Contains(data[1].ToLower()))
                                {
                                    client.Teleport(user.Player.Map, user.Player.X, user.Player.Y, user.Player.DynamicID);
                                    break;
                                }
                            }

                            break;
                        }

                    case "bring":
                        {
                            foreach (var user in Database.Server.GamePoll.Values)
                            {
                                if (user.Player.Name.ToLower() == data[1].ToLower())
                                {
                                    user.Teleport(client.Player.Map, client.Player.X, client.Player.Y);
                                    break;
                                }
                            }
                            break;
                        }

                    case "arenapoints":
                        {
                            client.HonorPoints += uint.Parse(data[1]);
                            break;
                        }
                    case "dp":
                        {
                            client.Player.RacePoints = uint.Parse(data[1]);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.RacePoints, MsgUpdate.DataType.RaceShopPoints);
                                client.Player.SendUpdate(stream, client.Player.RacePoints, MsgUpdate.DataType.BoundConquerPoints);

                            }
                            break;
                        }
                    case "fake":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                string[] names = new string[] { "Gool", "Hooy", "Thor", "Mido", "3tra", "omyy", "anaa", "a7aa", "Fuck", "MArk", "Lucy", "Tomy", "Dark", "Noop", "Hway", "Bosy", "Nosa", "Whits", "Rober", "Kota", "Contesa", "Kory", "Moko", "Hayato", "Adams"
                                                            , "Jask", "hasky", "mask", "masr", "balas", "moza", "a7mos", "bardis", "therock", "herok", "bsbopsa", "bate5a", "manga", "moza", "logy", "rosy", "hohoz", "troz", "bika", "kora", "kola", "afion", "7chicha", "bango", "shata"};
                                byte[] levels = new byte[] { 15, 37, 50, 55, 101, 111, 112, 19, 78, 99, 107, 103, 48, 46, 88, 95, 53, 83, 119, 121, 110, 80, 60, 69, 62, 105,
                                                                33, 44, 104, 37, 50, 55, 101, 111, 112, 19, 78, 99, 107, 103, 48, 46, 88, 95, 53, 83, 119, 121, 110, 80, 60, 69, 62, 105, 33, 44, 104 };
                                uint[] garment = new uint[] { 192695, 188335, 192615, 194210, 193075, 193625, 188705, 187975, 192345, 188945, 193355, 194320, 192575, 187575, 193115, 192785, 193195, 192635, 187315, 188175, 188165, 188265, 187965, 192435, 193015, 187775 };
                                for (int i = 0; i < ushort.Parse(data[1]); i++)
                                {
                                    Client.GameClient pclient = new Client.GameClient(null);
                                    pclient.Fake = true;

                                    pclient.Player = new Role.Player(pclient);
                                    pclient.Inventory = new Role.Instance.Inventory(pclient);
                                    pclient.Equipment = new Role.Instance.Equip(pclient);
                                    pclient.Warehouse = new Role.Instance.Warehouse(pclient);
                                    pclient.MyProfs = new Role.Instance.Proficiency(pclient);
                                    pclient.MySpells = new Role.Instance.Spell(pclient);
                                    pclient.Achievement = new Database.AchievementCollection();
                                    pclient.Status = new MsgStatus();

                                    pclient.Player.ServerID = (ushort)Database.GroupServerList.MyServerInfo.ID;

                                    pclient.Player.Name = "" + names[i] + "";
                                    pclient.Player.Body = client.Player.Body;
                                    pclient.Player.Face = client.Player.Face;
                                    pclient.Player.Hair = client.Player.Hair;
                                    pclient.Player.UID = Database.Server.ClientCounter.Next;
                                    pclient.Player.HitPoints = client.Player.HitPoints;
                                    pclient.Status.MaxHitpoints = client.Status.MaxHitpoints;
                                    //pclient.Status.MaxAttack = client.Status.MaxAttack;

                                    ushort x = client.Player.X;
                                    ushort y = client.Player.Y;
                                    pclient.Player.X = (ushort)Program.GetRandom.Next((int)x - 15, x + 15);
                                    pclient.Player.Y = (ushort)Program.GetRandom.Next((int)y - 15, y + 15);
                                    pclient.Player.Map = client.Player.Map;
                                    pclient.Player.Level = 140; //levels[i];
                                    pclient.Player.Class = 45;
                                    pclient.Player.Action = Role.Flags.ConquerAction.Jump;
                                    pclient.Player.ServerID = (ushort)Database.GroupServerList.MyServerInfo.ID;
                                    pclient.Player.GarmentId = garment[i];
                                    pclient.Player.RightWeaponId = 420339;
                                    pclient.Player.LeftWeaponId = 420339;
                                    //pclient.MySpells.ClientSpells.ContainsKey((ushort)8001);
                                    pclient.Player.Vitality = 300;
                                    pclient.Player.Strength = 100;
                                    pclient.Player.Agility = 256;
                                    pclient.Status.MaxAttack = uint.MaxValue;
                                    pclient.Status.MinAttack = uint.MaxValue;

                                    //#region Revive
                                    //if (pclient.Player.ContainFlag(MsgUpdate.Flags.Ghost) && Time32.Now > client.Player.DeadStamp.AddSeconds(20))
                                    //{
                                    //    pclient.Player.Action = Flags.ConquerAction.None;
                                    //    pclient.Player.TransformationID = 0;
                                    //    pclient.Player.RemoveFlag(MsgUpdate.Flags.Dead);
                                    //    pclient.Player.RemoveFlag(MsgUpdate.Flags.Ghost);
                                    //    pclient.Player.HitPoints = (int)client.Status.MaxHitpoints;
                                    //}
                                    //#endregion
                                    client.Send(pclient.Player.GetArray(stream, false));

                                    pclient.Map = client.Map;
                                    pclient.Map.Enquer(pclient);
                                    Database.Server.GamePoll.TryAdd(pclient.Player.UID, pclient);
                                    pclient.Player.OnAutoHunt = true;

                                    pclient.Player.View.SendView(pclient.Player.GetArray(stream, false), false);

                                }
                            }
                            break;
                        }
                    case "dis":
                        {
                            Game.MsgTournaments.MsgSchedules.DisCity.Open();
                            break;
                        }

                    case "onlineminutes":
                        {
                            client.Player.OnlineMinutes = uint.Parse(data[1]);
                            break;
                        }

                    case "dragon":
                        {
                            var Map = Database.Server.ServerMaps[1645];
                            Program.LastBoss = "Dragon";
                            if (!Map.ContainMobID(20060))
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    string msg_ = "TeratoDragon has spawned and terrify the world!";
                                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg_, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                    Database.Server.AddMapMonster(stream, Map, 20060, 217, 214, 1, 1, 1);
                                }
                                MsgSchedules.SendInvitation("TeratoDragon has spawned and terrify the world!", " \nWould you like to join the fight against it?", 336, 241, 1645, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                                Console.WriteLine("TeratoDragon has spawned at" + DateTime.Now);

                            }
                            else
                            {
                                var loc = Map.GetMobLoc(20060);
                                if (loc != "")
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        string msg_ = "TeratoDragon is still alive at " + loc;
                                        Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg_, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                                    }
                            }
                            break;
                        }

                    case "snow":
                        {
                            Program.LastBoss = "Snow";
                            var Map = Database.Server.ServerMaps[1002];
                            if (!Map.ContainMobID(20070))
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    string msg_ = "SnowBashee has spawned and terrify the world!";
                                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg_, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                    Database.Server.AddMapMonster(stream, Map, 20070, 658, 670, 1, 1, 1);
                                }
                                MsgSchedules.SendInvitation("SnowBashee has spawned and terrify the world!", " \nWould you like to join the fight against it?", 660, 670, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                                Console.WriteLine("SnowBashee has spawned at" + DateTime.Now);
                            }
                            else
                            {
                                var loc = Map.GetMobLoc(20070);
                                if (loc != "")
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        string msg_ = "SnowBashee is still alive at " + loc;
                                        Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(msg_, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                                    }
                            }
                            break;
                        }

                    case "bossrank":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                MsgServer.MsgBossHarmRanking Rank = new MsgServer.MsgBossHarmRanking();
                                Rank.Type = (MsgServer.MsgBossHarmRanking.RankAction)int.Parse(data[1]);
                                Rank.MonsterID = uint.Parse(data[2]);
                                Rank.Hunters = new MsgServer.MsgBossHarmRankingEntry[1];
                                Rank.Hunters[0] = new MsgBossHarmRankingEntry();
                                Rank.Hunters[0].HunterName = client.Player.Name;
                                Rank.Hunters[0].HunterUID = client.Player.UID;
                                Rank.Hunters[0].Rank = uint.Parse(data[3]);
                                Rank.Hunters[0].ServerID = Database.GroupServerList.MyServerInfo.ID;
                                Rank.Hunters[0].HunterScore = uint.Parse(data[4]);
                                client.Send(stream.CreateBossHarmRankList(Rank));
                            }
                            break;
                        }

                    case "max_attack":
                        {
                            client.Status.Defence = uint.MaxValue;
                            client.Status.MaxAttack = uint.Parse(data[1]);
                            client.Status.MinAttack = uint.Parse(data[1]) - 1;
                            break;
                        }

                    case "pkp":
                        {
                            client.Player.PKPoints = ushort.Parse(data[1]);
                            break;
                        }

                    case "gui":
                        {
                            TestGui = ushort.Parse(data[1]);
                            break;
                        }

                    case "map":
                        {
                            client.SendSysMesage("MapID = " + client.Player.Map, ChatMode.System);
                            break;
                        }
                    case "studypoints":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                                client.Player.SubClass.AddStudyPoints(client, ushort.Parse(data[1]), rec.GetStream());
                            break;
                        }

                    case "xp":
                        {
                            client.Player.AddFlag(MsgUpdate.Flags.XPList, 20, true);
                            break;
                        }
                    case "addflag":
                        {
                            client.Player.AddFlag((MsgUpdate.Flags)int.Parse(data[1]), 10, true, 0, 50, 39);
                            break;
                        }
                    case "remflag":
                        {
                            client.Player.RemoveFlag((MsgUpdate.Flags)int.Parse(data[1]));
                            break;
                        }
                    case "level":
                        {
                            byte amount = 0;
                            if (byte.TryParse(data[1], out amount))
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.UpdateLevel(stream, amount, true);
                                }
                            }
                            break;
                        }
                    case "money":
                        {
                            long amount = 0;
                            if (long.TryParse(data[1], out amount))
                            {
                                client.Player.Money = amount;
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                }
                            }
                            break;
                        }
                    case "cps":
                        {
                            uint amount = 0;
                            if (uint.TryParse(data[1], out amount))
                            {
                                client.Player.ConquerPoints = amount;

                            }
                            break;
                        }

                    case "remspell":
                        {
                            ushort ID = 0;
                            if (!ushort.TryParse(data[1], out ID))
                            {
                                client.SendSysMesage("Invlid spell ID !");
                                break;
                            }
                            using (var rec = new ServerSockets.RecycledPacket())
                                client.MySpells.Remove(ID, rec.GetStream());
                            break;
                        }
                    case "spell":
                        {
                            ushort ID = 0;
                            if (!ushort.TryParse(data[1], out ID))
                            {
                                client.SendSysMesage("Invlid spell ID !");
                                break;
                            }
                            byte level = 0;
                            if (!byte.TryParse(data[2], out level))
                            {
                                client.SendSysMesage("Invlid spell Level ! ");
                                break;
                            }
                            byte levelHu = 0;
                            if (data.Length >= 3)
                            {
                                if (!byte.TryParse(data[3], out levelHu))
                                {
                                    client.SendSysMesage("Invlid spell Level Souls ! ");
                                    break;
                                }
                            }
                            int Experience = 0;
                            if (!int.TryParse(data[4], out Experience))
                            {
                                client.SendSysMesage("Invlid spell Experience ! ");
                                break;
                            }

                            using (var rec = new ServerSockets.RecycledPacket())
                                client.MySpells.Add(rec.GetStream(), ID, level, levelHu, 0, Experience);
                            break;
                        }
                    case "prof":
                        {
                            ushort ID = 0;
                            if (!ushort.TryParse(data[1], out ID))
                            {
                                client.SendSysMesage("Invlid prof ID !");
                                break;
                            }
                            byte level = 0;
                            if (!byte.TryParse(data[2], out level))
                            {
                                client.SendSysMesage("Invlid prof Level ! ");
                                break;
                            }
                            uint Experience = 0;
                            if (!uint.TryParse(data[3], out Experience))
                            {
                                client.SendSysMesage("Invlid prof Experience ! ");
                                break;
                            }
                            using (var rec = new ServerSockets.RecycledPacket())
                                client.MyProfs.Add(rec.GetStream(), ID, level, Experience);
                            break;
                        }
                    case "clearinventory":
                    case "clearinv":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                                client.Inventory.Clear(rec.GetStream());
                            break;
                        }

                    case "tele":
                        {
                            client.TerainMask = 0;
                            uint mapid = 0;
                            if (!uint.TryParse(data[1], out mapid))
                            {
                                client.SendSysMesage("Invlid Map ID !");
                                break;
                            }
                            ushort X = 0;
                            if (!ushort.TryParse(data[2], out X))
                            {
                                client.SendSysMesage("Invlid X !");
                                break;
                            }
                            ushort Y = 0;
                            if (!ushort.TryParse(data[3], out Y))
                            {
                                client.SendSysMesage("Invlid Y !");
                                break;
                            }
                            uint DinamicID = 0;
                            if (!uint.TryParse(data[4], out DinamicID))
                            {
                                client.SendSysMesage("Invlid DinamicID !");
                                break;
                            }
                            client.Teleport(mapid, X, Y, DinamicID);

                            break;
                        }

                    case "tele2":
                        {
                            uint mapid = 0;
                            if (!uint.TryParse(data[1], out mapid))
                            {
                                client.SendSysMesage("Invlid Map ID !");
                                break;
                            }
                            ushort X = 0;
                            if (!ushort.TryParse(data[2], out X))
                            {
                                client.SendSysMesage("Invlid X !");
                                break;
                            }
                            ushort Y = 0;
                            if (!ushort.TryParse(data[3], out Y))
                            {
                                client.SendSysMesage("Invlid Y !");
                                break;
                            }
                            foreach (var map in Database.Server.ServerMaps.Values)
                            {
                                mapid = map.ID;
                                Console.WriteLine(map.ID);
                                ActionQuery action = new ActionQuery()
                                {
                                    ObjId = client.Player.UID,
                                    Type = ActionType.Teleport,
                                    dwParam = mapid,
                                    wParam1 = X,
                                    wParam2 = Y,
                                    dwParam3 = mapid
                                };
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    client.Send(rec.GetStream().ActionCreate(&action));
                                    client.Send(rec.GetStream().MapStatusCreate(mapid, mapid, 8));
                                }
                                System.Threading.Thread.Sleep(1000);
                            }
                            break;
                        }

                    case "addgarment":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                                client.Player.AddSpecialGarment(rec.GetStream(), uint.Parse(data[1]));
                            break;
                        }
                    case "remgarment":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                                client.Player.RemoveSpecialGarment(rec.GetStream());
                            break;
                        }
                    case "AddItemWitchStack":
                        {
                            uint ID = 0;
                            if (!uint.TryParse(data[1], out ID))
                            {
                                client.SendSysMesage("Invlid item ID !");
                                break;
                            }
                            using (var rec = new ServerSockets.RecycledPacket())
                                client.Inventory.AddItemWitchStack(ID, 0, 100, rec.GetStream(), false);

                            break;
                        }
                    case "itemm":
                        {
                            uint ID = 0;
                            if (!uint.TryParse(data[1], out ID))
                            {
                                client.SendSysMesage("Invlid item ID !");
                                break;
                            }
                            using (var rec = new ServerSockets.RecycledPacket())
                                client.Inventory.AddItemWitchStack(ID, 0, 1, rec.GetStream(), false);

                            break;
                        }
                    case "item":
                        {
                            uint ID = 0;
                            if (!uint.TryParse(data[1], out ID))
                            {
                                client.SendSysMesage("Invlid item ID !");
                                break;
                            }
                            byte plus = 0;
                            if (!byte.TryParse(data[2], out plus))
                            {
                                client.SendSysMesage("Invlid item plus !");
                                break;
                            }
                            byte bless = 0;
                            if (!byte.TryParse(data[3], out bless))
                            {
                                client.SendSysMesage("Invlid item Enchant !");
                                break;
                            }
                            byte enchant = 0;
                            if (!byte.TryParse(data[4], out enchant))
                            {
                                client.SendSysMesage("Invlid item Enchant !");
                                break;
                            }
                            byte sockone = 0;
                            if (!byte.TryParse(data[5], out sockone))
                            {
                                client.SendSysMesage("Invlid item Socket One !");
                                break;
                            }
                            byte socktwo = 0;
                            if (!byte.TryParse(data[6], out socktwo))
                            {
                                client.SendSysMesage("Invlid item Socket Two !");
                                break;
                            }
                            byte count = 1;
                            if (data.Length > 7)
                            {
                                if (!byte.TryParse(data[7], out count))
                                {
                                    client.SendSysMesage("Invlid item count !");
                                    break;
                                }
                            }
                            byte Effect = 0;
                            if (data.Length > 8)
                            {
                                if (!byte.TryParse(data[8], out Effect))
                                {
                                    client.SendSysMesage("Invlid Effect Type !");
                                    break;
                                }
                            }
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                client.Inventory.Add(rec.GetStream(), ID, count, plus, bless, enchant, (Role.Flags.Gem)sockone, (Role.Flags.Gem)socktwo, false, (Role.Flags.ItemEffect)Effect);
                            }
                            break;
                        }

                    //case "chr":
                    //    {
                    //        Game.MsgTournaments.MsgSchedules.ChristmasAnimation.Start();
                    //        break;
                    //    }
                    case "championpoints":
                        {
                            client.Player.AddChampionPoints(uint.Parse(data[1]));
                            break;
                        }


                    case "wea":
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var packet = rec.GetStream();
                                packet.WeatherCreate((MsgWeather.WeatherType)ushort.Parse(data[1]), uint.Parse(data[2]), uint.Parse(data[3]), (uint)uint.Parse(data[4]), uint.Parse(data[5]));
                                client.Send(packet);
                            }
                            break;
                        }
                    case "switch":
                        {

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SwitchWingWalkerAttack(stream);
                            }
                            break;
                        }

                    case "reborn":
                        {
                            client.Player.Reborn = byte.Parse(data[1]);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.Reborn, MsgUpdate.DataType.Reborn);
                            }
                            break;
                        }
                    case "class":
                        {
                            client.Player.Class = byte.Parse(data[1]);
                            break;
                        }
                    case "tour":
                        {
                            Game.MsgTournaments.MsgSchedules.CurrentTournament = Game.MsgTournaments.MsgSchedules.Tournaments[(MsgTournaments.TournamentType)ushort.Parse(data[1])];
                            Game.MsgTournaments.MsgSchedules.CurrentTournament.Open();
                            break;
                        }
                    case "SpeedHunterGame":
                        {
                            client.Player.SpeedHunterGamePoints = ushort.Parse(data[1]);
                            break;
                        }

                    #region drop
                    case "drop":

                        uint DropID = 0;
                        //uint DinamicID = 0;
                        Random Rnd = new Random();
                        string DropWhat = data[1].ToLower();
                        byte HowMany = (byte)Math.Min(ushort.Parse(data[2]), (ushort)255);
                        switch (DropWhat)
                        {
                            case "dragonball": DropID = 1088000; break;
                            case "dbscroll": DropID = 720028; break;
                            case "meteor": DropID = 1088001; break;
                            case "meteorscroll": DropID = 720027; break;
                            case "donationpoints": DropID = 3303695; break;//5 donatepoints
                            case "toughdrill": DropID = 1200005; break;
                            case "tortoisegemr": DropID = 700072; break;
                            case "tgf": DropID = 3305485; break;// TortoiseGemFragment
                            case "vip": DropID = 780000; break;//VIP-6 (1-Hour)
                            case "6stone": DropID = 730006; break;
                            case "powerexpball": DropID = 722057; break;
                            case "cps": DropID = 721027; break;//StarCPBag 100 CPs
                        }
                        //Database.ItemType.DBItem DbItem = null;
                        for (int x = 0; x < HowMany; x++)
                        {
                            MsgServer.MsgGameItem DataItem = new MsgServer.MsgGameItem();
                            DataItem.ITEM_ID = DropID;
                            Database.ItemType.DBItem DBItem;
                            if (Database.Server.ItemsBase.TryGetValue(DropID, out DBItem))
                            {
                                DataItem.Durability = DataItem.MaximDurability = DBItem.Durability;
                            }
                            DataItem.Color = Role.Flags.Color.Red;
                            ushort xx = (ushort)(client.Player.X + Rnd.Next(20) - Rnd.Next(20));
                            ushort yy = (ushort)(client.Player.Y + Rnd.Next(20) - Rnd.Next(20));
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(DataItem, xx, yy, MsgFloorItem.MsgItem.ItemType.Item, 0, client.Player.DynamicID, client.Player.Map, client.Player.UID, false, client.Map);
                                if (client.Map.EnqueueItem(DropItem))
                                {
                                    DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                }

                            }

                        }
                        break;
                        #endregion
                }
                #endregion


                return true;
            }
            return false;
        }
    }
}
