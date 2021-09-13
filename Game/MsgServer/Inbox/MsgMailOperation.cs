using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe void GetMailOperation(this ServerSockets.Packet stream, out ushort Act, out uint ID)
        {
            Act = stream.ReadUInt16();
            ID = stream.ReadUInt32();
        }
        public static unsafe ServerSockets.Packet CreateMailContent(this ServerSockets.Packet stream, uint ID, string Message)
        {
            stream.InitWriter();
            stream.Write(ID);
            stream.Write(Message, 64);
            stream.Finalize(1048);
            return stream;
        }
        public static unsafe ServerSockets.Packet CreatMailNotify(this ServerSockets.Packet stream, byte Operation)
        {
            /*Operation being 3(ON) or 2(OFF)*/
            stream.InitWriter();
            stream.Write(Operation);
            stream.ZeroFill(15);
            stream.Finalize(1047);
            return stream;
        }
    }
    public unsafe struct MsgMailOperation
    {
        public enum Action : uint
        {
            Show = 1,
            Delete = 2,
            ClaimMoney = 3,
            ClaimCP = 4,
            ClaimItem = 5,
            ClaimAttach = 6,
            DeleteOpt = 7,
            ClaimOpt = 8
        }
        [PacketAttribute(GamePackets.MsgMailOperation)]
        public static void Handler(Client.GameClient user, ServerSockets.Packet stream)
        {
            ushort Type;
            uint ID;
            stream.GetMailOperation(out Type,out ID);
            PrizeInfo prize;
            switch ((Action)Type)
            {
                case Action.Show:
                    {
                        if (user.MailBox.ContainsKey(ID))
                        {
                            user.Send(stream.CreateMailContent(ID, user.MailBox[ID].Message));
                        }
                        break;
                    }
                case Action.Delete:
                    {
                        if (user.MailBox.ContainsKey(ID))
                        {
                            user.MailBox.TryRemove(ID, out prize);
                            if (user.MailBox.Count == 0)
                            {
                                user.Send(stream.CreatMailNotify(2));
                            }
                        }
                        break;
                    }
                case Action.ClaimMoney:
                    {
                        if (user.MailBox.TryRemove(ID, out prize))
                        {
                            if (prize.goldprize > 0)
                            {
                                user.Player.Money += (long)prize.goldprize;
                                user.SendSysMesage("You recieved " + prize.goldprize + " Silver from Inbox.");
                                prize.goldprize = 0;
                                if (user.MailBox.Count == 0)
                                {
                                    user.Send(stream.CreatMailNotify(2));
                                }
                            }
                            if (prize.cpsprize > 0)
                            {
                                user.Player.ConquerPoints += prize.cpsprize;
                                user.SendSysMesage("You recieved " + prize.cpsprize + " CPS from Inbox.");
                                prize.cpsprize = 0;
                                if (user.MailBox.Count == 0)
                                {
                                    user.Send(stream.CreatMailNotify(2));
                                }
                            }
                            if (prize.Attackment != 0)
                            {
                                if (user.Inventory.HaveSpace(1))
                                {
                                    user.Inventory.Add(stream, prize.Attackment);
                                }
                                else
                                {
                                    user.Inventory.AddReturnedItem(stream, prize.Attackment);
                                }
                                user.SendSysMesage("You recieved Attachment from Inbox.");
                                prize.Attackment = 0;
                                if (user.MailBox.Count == 0)
                                {
                                    user.Send(stream.CreatMailNotify(2));
                                }
                            }
                            if (prize.JoyBeans > 0)
                            {
                                user.SendSysMesage("You recieved "+prize.JoyBeans+" Beans from Inbox.");
                                prize.JoyBeans = 0;
                                if (user.MailBox.Count == 0)
                                {
                                    user.Send(stream.CreatMailNotify(2));
                                }
                            }
                            if (prize.Item != null)
                            {
                                if (user.Inventory.HaveSpace(1))
                                {
                                    Database.ItemType.DBItem DbItem;
                                    if (Database.Server.ItemsBase.TryGetValue(prize.Item.ITEM_ID, out DbItem))
                                    {
                                        prize.Item.Mode = Role.Flags.ItemMode.AddItem;
                                        user.Inventory.Add(prize.Item, DbItem, stream);
                                        prize.Item.SendItemExtra(user, stream);
                                        user.SendSysMesage("You recieved Attachment from Inbox.");
                                        prize.Item = null;
                                        if (user.MailBox.Count == 0)
                                        {
                                            user.Send(stream.CreatMailNotify(2));
                                        }
                                    }
                                }
                                else
                                {
                                    user.SendSysMesage("Clear some stuff of your bag to claim item from inbox.");
                                }
                            }
                        }
                        break;
                    }
                case Action.ClaimCP:
                    {
                        if (user.MailBox.TryRemove(ID, out prize))
                        {
                            user.Player.ConquerPoints += prize.cpsprize;
                            user.SendSysMesage("You recieved " + prize.goldprize + " Cps from Inbox.");
                            if (user.MailBox.Count == 0)
                            {
                                user.Send(stream.CreatMailNotify(2));
                            }
                        }
                        break;
                    }
                case Action.ClaimItem:
                    {
                        if (user.MailBox.TryRemove(ID, out prize))
                        {
                            Database.ItemType.DBItem DbItem;
                            if (Database.Server.ItemsBase.TryGetValue(prize.Item.ITEM_ID, out DbItem))
                            {
                                prize.Item.Mode = Role.Flags.ItemMode.AddItem;
                                user.Inventory.Add(prize.Item, DbItem, stream);
                                prize.Item.SendItemExtra(user, stream);
                                prize.Item = null;
                                if (user.MailBox.Count == 0)
                                {
                                    user.Send(stream.CreatMailNotify(2));
                                }
                            }
                        }
                        break;
                    }
                case Action.ClaimAttach:
                    {
                        if (user.MailBox.TryRemove(ID, out prize))
                        {
                            user.Inventory.Add(stream, prize.Attackment);
                            user.SendSysMesage("You recieved item from Inbox.");
                            if (user.MailBox.Count == 0)
                            {
                                user.Send(stream.CreatMailNotify(2));
                            }
                        }
                        break;
                    }
                default: MyConsole.WriteLine("[Mailbox] Unknown Action:" + Type); break;
            }
        }
    }
}
