using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public unsafe static class MsgTrade
    {
        public enum TradeID : uint
        {
            RequestNewTrade = 0x01,
            RequestCloseTrade = 0x02,
            RequestAddItemToTrade = 0x06,
            RequestAddMoneyToTrade = 0x07,
            RequestAddConquerPointsToTrade = 0x0D,
            RequestCompleteTrade = 0x0A,
            RemoveItem = 11,
            ShowTradeWindow = 0x03,
            CloseTradeWindow = 0x05,
            DisplayMoney = 0x08,
            DisplayConquerPoints = 0x0C
        }

        public static unsafe void GetTrade(this ServerSockets.Packet stream, out ulong dwParam, out TradeID ID)
        {
            dwParam = stream.ReadUInt64();
            ID = (TradeID)stream.ReadUInt16();
        }

        public static unsafe ServerSockets.Packet TradeCreate(this ServerSockets.Packet stream, ulong dwParam, TradeID ID)
        {
            stream.InitWriter();

            stream.Write(dwParam);//4
  
            stream.Write((ushort)ID);//8


            stream.Finalize(GamePackets.Trade);
            return stream;
        }
     
        [PacketAttribute(GamePackets.Trade)]
        private static void HandlerTrade(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (!user.Player.IsCheckedPass)
                return;
            ulong dwParam;
            TradeID ID;
            stream.GetTrade(out dwParam, out ID);

            switch (ID)
            {

                case TradeID.RequestNewTrade:
                    {
                        if (user.MyTrade == null)
                            user.MyTrade = new Role.Instance.Trade(user);


                        Role.IMapObj obj;
                        if (user.Player.View.TryGetValue((uint)dwParam, out obj, Role.MapObjectType.Player))
                        {
                            Client.GameClient Partner = user.MyTrade.Target = (obj as Role.Player).Owner;
                            if (user.MyTrade.Target != null)
                            {
                                if (!user.MyTrade.Target.InTrade)
                                {

                                    user.Player.targetTrade = (uint)dwParam;
                                    if (Partner.Player.targetTrade == user.Player.UID)
                                    {
                                        if (Partner.Player.UID == dwParam)
                                        {


                                            user.MyTrade.Target.MyTrade = new Role.Instance.Trade(user.MyTrade.Target);
                                            user.MyTrade.Target.MyTrade.Target = user;


                                            Partner.MyTrade.Target = user;
                                            Partner.MyTrade.WindowOpen = true;
                                            user.MyTrade.WindowOpen = true;
                                            Partner.MyTrade.Confirmed = false;
                                            user.MyTrade.Confirmed = false;


                                            user.Send(stream.TradeCreate(dwParam, TradeID.ShowTradeWindow));
                                            Partner.Send(stream.TradeCreate(user.Player.UID, TradeID.ShowTradeWindow));
                                        }
                                        else
                                        {
#if Arabic
                                                user.SendSysMesage("Player already in a trade.");
#else
                                            user.SendSysMesage("Player already in a trade.");
#endif
                                        
                                        }
                                    }
                                    else
                                    {

                                        Partner.Send(stream.PopupInfoCreate(user.Player.UID, Partner.Player.UID, user.Player.Level, user.Player.BattlePower));


                                        Partner.Send(stream.TradeCreate(user.Player.UID, TradeID.RequestNewTrade));

                                    }
                                }
                                else
                                {
#if Arabic
                                    user.SendSysMesage("Player already in a trade.");
#else
                                    user.SendSysMesage("Player already in a trade.");
#endif
                                    
                                }
                            }
                        }
                        break;
                    }
                case TradeID.RequestCloseTrade:
                    {
                        if (user.InTrade)
                        {
                            user.MyTrade.CloseTrade();
                        }
                        break;
                    }
                case TradeID.RequestAddItemToTrade:
                    {
                        if (user.InTrade)
                        {
                            Game.MsgServer.MsgGameItem DataItem;
                            if (user.Inventory.TryGetItem((uint)dwParam, out DataItem))
                                user.MyTrade.AddItem(stream, (uint)dwParam, DataItem);
                        }
                        break;
                    }
                case TradeID.RequestAddConquerPointsToTrade:
                    {
                        if (user.InTrade)
                            user.MyTrade.AddConquerPoints((uint)dwParam, stream);
                        break;
                    }
                case TradeID.RequestAddMoneyToTrade:
                    {
                        if (user.InTrade)
                            user.MyTrade.AddMoney((long)dwParam,stream);
                        break;
                    }
                case TradeID.RequestCompleteTrade:
                    {
                        if (user.InTrade)
                        {
                            if (user.MyTrade.Target.InTrade)
                            {
                                user.MyTrade.Confirmed = true;
                                if (!user.MyTrade.Target.MyTrade.Confirmed)
                                {

                                    user.MyTrade.Target.Send(stream.TradeCreate(dwParam, TradeID.RequestCompleteTrade));
                                }
                                else
                                {


                                    Client.GameClient Partner = user.MyTrade.Target;

                                    user.Player.targetTrade = 0;
                                    Partner.Player.targetTrade = 0;


                                    user.Send(stream.TradeCreate(dwParam, TradeID.CloseTradeWindow));
                                    Partner.Send(stream.TradeCreate(dwParam, TradeID.CloseTradeWindow));


                                    bool Acceped = false;
                                    if (user.Inventory.HaveSpace((byte)Partner.MyTrade.Items.Count))
                                    {
                                        if (Partner.MyTrade.ValidItems())
                                        {
                                            if (Partner.Inventory.HaveSpace((byte)user.MyTrade.Items.Count))
                                            {
                                                if (user.MyTrade.ValidItems())
                                                {
                                                    //add money and conquerpoints ------------------
                                                    user.Player.ConquerPoints += Partner.MyTrade.ConquerPoints;
                                                    user.Player.Money += Partner.MyTrade.Money;
   
                                                    user.Player.SendUpdate(stream,user.Player.Money, MsgUpdate.DataType.Money);

                                                    Partner.Player.ConquerPoints += user.MyTrade.ConquerPoints;
                                                    Partner.Player.Money += user.MyTrade.Money;
                                                 
                                                    Partner.Player.SendUpdate(stream, Partner.Player.Money, MsgUpdate.DataType.Money);
                                                    //-------------------------

                                                    //add items ------------------
                                                    foreach (var item in Partner.MyTrade.Items.Values)
                                                    {
                                                        user.Inventory.Update(item, Role.Instance.AddMode.MOVE, stream);
                                                        Partner.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);
                                                    }
                                                    foreach (var item in user.MyTrade.Items.Values)
                                                    {
                                                        Partner.Inventory.Update(item, Role.Instance.AddMode.MOVE, stream);
                                                        user.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);
                                                    }

                                                    user.MyTrade = null;
                                                    Partner.MyTrade = null;
                                                    Acceped = true;
                                                }
                                            }
                                        }
                                    }
                                    if (!Acceped)
                                    {
#if Arabic
                                          user.SendSysMesage("There was an error with the trade", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                        Partner.SendSysMesage("There was an error with the trade", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                        user.SendSysMesage("There was an error with the trade", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                        Partner.SendSysMesage("There was an error with the trade", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif
                                      
                                    }
                                }
                            }
                        }
                        break;
                    }

            }
        }
    }
}
