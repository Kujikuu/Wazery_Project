using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace LightConquer_Project.Game.MsgServer
{
    public static class MsgExchangeShopBuy
    {
        [ProtoContract]
        public class ExchangeShopBuy
        {
            [ProtoMember(1, IsRequired = true)]
            public uint DwParam;
            [ProtoMember(2, IsRequired = true)]
            public uint Type;//1 and 2 for others ranks?
            [ProtoMember(3, IsRequired = true)]
            public uint CountItems;
            [ProtoMember(4, IsRequired = true)]
            public uint Index;

            [ProtoMember(5, IsRequired = true)]
            public Item[] Items;

            [ProtoContract]
            public class Item
            {
                [ProtoMember(1, IsRequired = true)]
                public uint ItemUID;
                [ProtoMember(2, IsRequired = true)]
                public uint Cost;

            }
        }

        public static unsafe ServerSockets.Packet CreateBuyExchangeShop(this ServerSockets.Packet stream, ExchangeShopBuy obj)
        {
            stream.InitWriter();
            stream.ProtoBufferSerialize(obj);
            stream.Finalize(GamePackets.BuyFromExchangeShop);//2443

            return stream;
        }
        public static unsafe void GetBuyExchangeShop(this ServerSockets.Packet stream, out ExchangeShopBuy pQuery)
        {
            pQuery = new ExchangeShopBuy();
            pQuery = stream.ProtoBufferDeserialize<ExchangeShopBuy>(pQuery);
        }
        [PacketAttribute(GamePackets.BuyFromExchangeShop)]
        private unsafe static void Process(Client.GameClient client, ServerSockets.Packet stream)
        {
            ExchangeShopBuy pQuery;
            stream.GetBuyExchangeShop(out pQuery);

            bool accept = true;

            uint _allcost = 0;
            if (pQuery.Items != null)
            {
                foreach (var _items in pQuery.Items)
                {
                    _allcost += _items.Cost;
                    if (!client.Inventory.ContainItemWithStack(_items.ItemUID, (ushort)_items.Cost))
                    {
                        accept = false;
                        break;
                    }
                }
            }
            else
                accept = false;


            if (accept)
            {
                Database.ExchangeShop.Item Item;
                if (Database.ExchangeShop.Items.TryGetValue((int)pQuery.Index, out Item))
                {
                    uint dbcost = Item.RequestItemCount * pQuery.CountItems;
                    if (_allcost == dbcost && client.MyExchangeShop.CanBuyItem(Item, (ushort)pQuery.CountItems))
                
                    {
                        if (!client.Inventory.HaveSpace((byte)pQuery.CountItems))
                        {
                            client.CreateBoxDialog("Please make " + pQuery.CountItems + " more space in your inventory.");
                            return;
                        }

                        client.MyExchangeShop.AddItem(Item, (ushort)pQuery.CountItems);

                        foreach (var _items in pQuery.Items)
                        {
                            client.Inventory.RemoveStackItem(_items.ItemUID, (ushort)_items.Cost, stream);
                        }

                        if ((Item.ItemID % 730000) <= 9)
                            client.Inventory.AddItemWitchStack(Item.ItemID, (byte)(Item.ItemID % 730000), (byte)pQuery.CountItems, stream);
                        else
                            client.Inventory.AddItemWitchStack(Item.ItemID, 0, (byte)pQuery.CountItems, stream);

                        client.Send(stream.CreateBuyExchangeShop(pQuery));
                    }
                    //else cheater
                }
            }
          
        }
    }
}
