using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightConquer_Project.Game.MsgNpc;

namespace LightConquer_Project.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet LoginHandlerCreate(this ServerSockets.Packet stream, uint Type, uint Map)
        {
            stream.InitWriter();

            stream.Write(0);
            stream.Write(Type);
            stream.Write(Map);

            stream.Finalize(GamePackets.MapLoading);

            return stream;
        }

    }
    public class top_typ
    {
        public const byte
        VIP = 22;
    }
    public unsafe struct MsgLoginHandler
    {

        [PacketAttribute(GamePackets.MapLoading)]
        public unsafe static void LoadMap(Client.GameClient client, ServerSockets.Packet packet)
        {
            if ((client.ClientFlag & Client.ServerFlag.AcceptLogin) == Client.ServerFlag.AcceptLogin)
            {
                try
                {
                    client.Player.ServerID = (ushort)Database.GroupServerList.MyServerInfo.ID;

                    client.Send(packet.HeroInfo(client.Player));


                    MsgChiInfo.MsgHandleChi.SendInfo(client, MsgChiInfo.Action.Upgrade, client, 142);


                    client.Send(packet.FlowerCreate(Role.Core.IsBoy(client.Player.Body) ? MsgFlower.FlowerAction.Flower : MsgFlower.FlowerAction.FlowerSender
                        , 0, 0, client.Player.Flowers.RedRoses, client.Player.Flowers.RedRoses.Amount2day
                        , client.Player.Flowers.Lilies, client.Player.Flowers.Lilies.Amount2day
                        , client.Player.Flowers.Orchids, client.Player.Flowers.Orchids.Amount2day
                        , client.Player.Flowers.Tulips, client.Player.Flowers.Tulips.Amount2day));


                    if (client.Player.Flowers.FreeFlowers > 0)
                    {
                        client.Send(packet.FlowerCreate(Role.Core.IsBoy(client.Player.Body)
                            ? MsgFlower.FlowerAction.FlowerSender : MsgFlower.FlowerAction.Flower
                            , 0, 0, client.Player.Flowers.FreeFlowers));
                    }
                    client.Send(packet.NobilityIconCreate(client.Player.Nobility));
                    if (client.Player.Achievement != null)
                        client.Player.Achievement.Send(client, packet);

                    if (client.Player.BlessTime > 0)
                        client.Player.SendUpdate(packet, client.Player.BlessTime, MsgUpdate.DataType.LuckyTimeTimer);

                    client.Player.ProtectAttack(1000 * 10);
                    client.Player.CreateHeavenBlessPacket(packet, true);


                    if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.QuizShow
                        && MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                        MsgTournaments.MsgSchedules.CurrentTournament.Join(client, packet);


                    if (client.Player.DExpTime > 0)
                        client.Player.CreateExtraExpPacket(packet);


                    if (client.Player.MyClan != null)
                    {
                        client.Player.MyClan.SendThat(packet, client);

                        foreach (var ally in client.Player.MyClan.Ally.Values)
                            client.Send(packet.ClanRelationCreate(client.Player.MyClan.ID, ally.Name, ally.LeaderName, MsgClan.Info.AddAlly));
                        foreach (var enemy in client.Player.MyClan.Enemy.Values)
                            client.Send(packet.ClanRelationCreate(client.Player.MyClan.ID, enemy.Name, enemy.LeaderName, MsgClan.Info.AddEnemy));
                    }

                    client.Equipment.Show(packet);
                    client.Inventory.ShowALL(packet);

#if Jiang
                    if (client.Player.MyJiangHu != null)
                        client.Player.MyJiangHu.LoginClient(packet, client);
                    else if (client.Player.Reborn == 20)//bahaa
                    {
                        client.Send(packet.JiangHuInfoCreate(MsgJiangHuInfo.JiangMode.IconBar, "0"));
                    }
#endif
                    //send chi------------- query
                    foreach (var chipower in client.Player.MyChi)
                        client.Player.MyChi.SendQueryUpdate(client, chipower, packet);

                    //send confiscator items
                    foreach (var item in client.Confiscator.RedeemContainer.Values)
                    {
                        var Dataitem = item;
                        Dataitem.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(item.Date).Ticks).Days);
                        if (Dataitem.DaysLeft > 7)
                        {
                            Dataitem.Action = MsgDetainedItem.ContainerType.RewardCps;
                        }
                        if (Dataitem.Action != MsgDetainedItem.ContainerType.RewardCps)
                        {
                            Dataitem.Action = MsgDetainedItem.ContainerType.DetainPage;
                            Dataitem.Send(client, packet);
                        }
                        if (Dataitem.Action == MsgDetainedItem.ContainerType.RewardCps)
                            client.Confiscator.RedeemContainer.TryRemove(item.UID, out Dataitem);
                    }
                    foreach (var item in client.Confiscator.ClaimContainer.Values)
                    {
                        var Dataitem = item;
                        Dataitem.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(item.Date).Ticks).Days);
                        if (Dataitem.RewardConquerPoints != 0)
                        {
                            Dataitem.Action = MsgDetainedItem.ContainerType.RewardCps;
                        }
                        Dataitem.Send(client, packet);
                        client.Confiscator.ClaimContainer[item.UID] = Dataitem;
                    }
                    //-------------

                    if (MsgTournaments.MsgSchedules.GuildWar.RewardDeputiLeader.Contains(client.Player.UID))
                        client.Player.AddFlag(MsgUpdate.Flags.TopDeputyLeader, Role.StatusFlagsBigVector32.PermanentFlag, false);
                    if (MsgTournaments.MsgSchedules.GuildWar.RewardLeader.Contains(client.Player.UID))
                        client.Player.AddFlag(MsgUpdate.Flags.TopGuildLeader, Role.StatusFlagsBigVector32.PermanentFlag, false);


                    client.Player.PKPoints = client.Player.PKPoints;
                    //if (client.Player.CursedTimer > 0)
                    //{
                    //    client.Player.AddCursed(client.Player.CursedTimer);
                    //}
                    if (client.Player.VipLevel == 6)
                    {
                        client.Player.AddTitle(top_typ.VIP, true);
                    }
                    client.Send(packet.ServerTimerCreate());


                    MsgTournaments.MsgSchedules.ClassPkWar.LoginClient(client);
                    MsgTournaments.MsgSchedules.WeeklyPKChampion.LoginClient(client);
                    MsgTournaments.MsgSchedules.MonthlyPKChampion.LoginClient(client);
                    MsgTournaments.MsgSchedules.ElitePkTournament.GetTitle(client, packet);
                    MsgTournaments.MsgSchedules.TeamPkTournament.GetTitle(client, packet);
                    MsgTournaments.MsgSchedules.SkillTeamPkTournament.GetTitle(client, packet);

                    if (MsgTournaments.MsgBroadcast.CurrentBroadcast.EntityID != 1)
                    {
                        client.Send(new MsgServer.MsgMessage(MsgTournaments.MsgBroadcast.CurrentBroadcast.Message
                            , "ALLUSERS"
                            , MsgTournaments.MsgBroadcast.CurrentBroadcast.EntityName
                            , MsgServer.MsgMessage.MsgColor.red
                            , MsgServer.MsgMessage.ChatMode.BroadcastMessage
                            ).GetArray(packet));
                    }

                    if (client.Player.RacePoints > 0)
                        client.Player.SendUpdate(packet, client.Player.RacePoints, MsgUpdate.DataType.RaceShopPoints);
                    client.Player.SendUpdate(packet, client.Player.RacePoints, MsgUpdate.DataType.BoundConquerPoints);

                    client.Activeness.CheckTasks(packet);

                    client.Activeness.IncreaseTask(1);
                    client.Activeness.IncreaseTask(13);
                    client.Activeness.IncreaseTask(25);

                    if (client.Player.VipLevel > 1)
                    {
                        client.Activeness.IncreaseTask(2);
                        client.Activeness.IncreaseTask(14);
                        client.Activeness.IncreaseTask(26);
                    }
                    client.Activeness.UpdateActivityPoints(packet);
                    client.Activeness.UpdateClaimRewards(packet);
                    //client.Player.InnerPower.AddPotency(packet, client, 0);
                    client.Player.UpdateVip(packet);
                    client.Player.SendUpdate(packet, 255, MsgUpdate.DataType.Merchant);



                    ActionQuery action = new ActionQuery()
                    {
                        ObjId = client.Player.UID,
                        Type = (ActionType)157,
                        dwParam = 2
                    };

                    client.Send(packet.ActionCreate(&action));

                    client.Send(packet.ServerConfig());



                    if (client.Player.InUnion)
                    {
                        client.Player.MyUnion.SendMyInfo(packet, client);
                    }



                    if (Database.RechargeShop.RechargeAccounts.ContainsKey(client.Player.UID))
                    {
                        if (Database.RechargeShop.RechargeAccounts[client.Player.UID] >= client.Player.RechargeProgress)
                        {
                            client.Player.RechargeProgress = Database.RechargeShop.RechargeAccounts[client.Player.UID];
                        }
                        else
                        {
                            Database.RechargeShop.RechargeAccounts[client.Player.UID] = client.Player.RechargeProgress;
                        }
                    }
                    else if (client.Player.RechargeProgress > 0)
                    {
                        Database.RechargeShop.RechargeAccounts.TryAdd(client.Player.UID, client.Player.RechargeProgress);
                    }


                    Database.TitleStorage.CheckUpUser(client, packet);


                    foreach (var title in client.Player.SpecialTitles)
                        client.Player.AddSpecialTitle(title, packet);
                    client.Player.SendSpecialTitle(packet);
                    client.Player.ActiveSpecialTitles(packet);



                    if (client.Player.Level >= 1)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Send(stream.AutoHuntCreate(0, 341));
                        }
                    }
                    if (client.Player.Level >= 1)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.ActiveNpc = (uint)Game.MsgNpc.NpcID.VotePointsReward;
                            Game.MsgNpc.NpcHandler.VotePointsReward(client, stream, 0, "", 0);
                        }
                    }

                    if (client.Player.SecurityPassword != 0)
                    {
                        client.Send(packet.SecondaryPasswordCreate(MsgSecondaryPassword.ActionID.PasswordCorrect, 1, 0));
                    }
                    else
                        client.Player.IsCheckedPass = true;


                    // Welcome Messages.
                    client.SendSysMesage("Welcome to " + Program.ServerConfig.ServerName + ", visit the Guide Npc for help!", MsgMessage.ChatMode.Talk);
                    client.SendSysMesage("Official Site: ", MsgMessage.ChatMode.Talk);
                    client.SendSysMesage("Enjoy " + Program.ServerConfig.ServerName + ".", MsgMessage.ChatMode.Talk);
                    client.SendSysMesage($"Total Online: {Database.Server.GamePoll.Count} - Max Online: {KernelThread.MaxOnline} ");

                    // Update Message.
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Header, "                    #StayHome & Be Safe #"));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Body, "# Official Site: RiviaConquer.com"));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Body, "# Check our website for the updates made."));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Body, "# Official facebook: facebook.com/riviaconquer/"));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Body, " "));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Body, " "));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Body, " "));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Body, " "));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Body, " "));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Body, "# Enjoy " + Program.ServerConfig.ServerName + "`s staff. "));
                    //client.Send(packet.StaticGUI(MsgNpc.MsgBuilder.StaticGUIType.Footer, "                    # Total Online: " + Database.Server.GamePoll.Count + " - Max Online:" + KernelThread.MaxOnline + " "));

                    //Check if the player level above 100 and still have Newbietoken
                    if (client.Player.Level >= 100 && client.Inventory.Contain(722122, 1))
                        client.Inventory.Remove(722122, 1, packet);

                    if (client.Player.Map == 1616)
                        client.Teleport(1002, 430, 380);


                    if(client.Player.Reborn < 2)
                        if(!client.Player.ContainFlag(MsgUpdate.Flags.NewbeeAura))
                            client.Player.AddFlag(MsgUpdate.Flags.NewbeeAura, Role.StatusFlagsBigVector32.PermanentFlag, false);

                    if (client.Player.ContainFlag(MsgUpdate.Flags.NewbeeAura))
                        if (client.Player.Reborn >= 2)
                            client.Player.RemoveFlag(MsgUpdate.Flags.NewbeeAura);

                    client.Player.UpdateInventorySash(packet);
                    if (client.Player.ExpProtection > 0)
                        client.Player.CreateExpProtection(packet, 0, false);
                    if (client.Player.VipLevel >= 6)
                    {
                        TimeSpan timer1 = new TimeSpan(client.Player.ExpireVip.Ticks);
                        TimeSpan Now2 = new TimeSpan(DateTime.Now.Ticks);
                        int days_left = (int)(timer1.TotalDays - Now2.TotalDays);
                        int hour_left = (int)(timer1.TotalHours - Now2.TotalHours);
                        int left_minutes = (int)(timer1.TotalMinutes - Now2.TotalMinutes);
#if Arabic
                        if (days_left > 0)
                            client.SendSysMesage("Your VIP 6 will expire in : " + days_left + "(Days) .", MsgMessage.ChatMode.System);
                        else if(hour_left > 0)
                            client.SendSysMesage("Your VIP 6 will expire in : " + hour_left + "(Hours) .", MsgMessage.ChatMode.System);
                        else if (left_minutes > 0)
                            client.SendSysMesage("Your VIP 6 will expire in : " + left_minutes + "(Minutes) .", MsgMessage.ChatMode.System);
#else
                        if (days_left > 0)
                            client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + days_left + " days.", MsgMessage.ChatMode.System);
                        else if (hour_left > 0)
                            client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + hour_left + " hours.", MsgMessage.ChatMode.System);
                        else if (left_minutes > 0)
                            client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + left_minutes + " minutes.", MsgMessage.ChatMode.System);
#endif



                    }

                    if (Database.AtributesStatus.IsTrojan(client.Player.Class)
                        || Database.AtributesStatus.IsTrojan(client.Player.FirstClass)
                        || Database.AtributesStatus.IsTrojan(client.Player.SecoundeClass))
                    {
                        if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Cyclone))
                            client.MySpells.Add(packet, (ushort)Role.Flags.SpellID.Cyclone);
                    }


                    if (client.Inventory.HaveSpace(1))
                    {
                        foreach (var item in client.Equipment.ClientItems.Values)
                        {
                            if (item.Position >= (uint)Role.Flags.ConquerItem.Head && item.Position <= (uint)Role.Flags.ConquerItem.Wing)
                            {
                                if (client.Inventory.HaveSpace(1) && item.Position == (uint)Role.Flags.ConquerItem.RightWeapon
                                    && item.Position == (uint)Role.Flags.ConquerItem.LeftWeapon)
                                {
                                    if (!Database.ItemType.IsShield(item.ITEM_ID))
                                    {
                                        if (!Database.ItemType.Equipable(item.ITEM_ID, client))
                                        {
                                            client.Equipment.Remove((Role.Flags.ConquerItem)item.Position, packet);
                                        }
                                    }
                                }
                            }
                            else if (item.Position >= (uint)Role.Flags.ConquerItem.AleternanteHead && item.Position <= (uint)Role.Flags.ConquerItem.AleternanteGarment)
                            {
                                if (client.Inventory.HaveSpace(1) && item.Position == (uint)Role.Flags.ConquerItem.AleternanteRightWeapon
                                    && item.Position == (uint)Role.Flags.ConquerItem.AleternanteLeftWeapon)
                                {
                                    if (!Database.ItemType.IsShield(item.ITEM_ID))
                                    {
                                        if (!Database.ItemType.Equipable(item.ITEM_ID, client))
                                        {
                                            client.Equipment.RemoveAlternante((Role.Flags.ConquerItem)item.Position, packet);
                                        }
                                    }
                                }
                            }
                            else if (item.Position >= (uint)Role.Flags.ConquerItem.Head && item.Position <= (uint)Role.Flags.ConquerItem.Wing)
                            {
                                if (client.Inventory.HaveSpace(1) && item.Position == (uint)Role.Flags.ConquerItem.RightWeapon
                                    && item.Position == (uint)Role.Flags.ConquerItem.LeftWeapon)
                                {
                                    if (!Database.ItemType.IsShield(item.ITEM_ID))
                                    {
                                        if (!Database.ItemType.Equipable(item.ITEM_ID, client))
                                        {
                                            client.Equipment.Remove((Role.Flags.ConquerItem)item.Position, packet);
                                        }
                                    }
                                }
                            }
                        }
                    }



                    MsgServer.MsgSameGroupServerList.GroupServer group = new MsgSameGroupServerList.GroupServer();
                    var GroupServers = Database.GroupServerList.GroupServers.Values.ToArray();
                    group.Servers = new MsgSameGroupServerList.Server[Database.GroupServerList.GroupServers.Count];
                    for (int x = 0; x < GroupServers.Length; x++)
                    {
                        var DBServer = GroupServers[x];
                        group.Servers[x] = new MsgSameGroupServerList.Server();
                        group.Servers[x].GroupID = DBServer.Group;
                        group.Servers[x].MapID = DBServer.MapID;
                        group.Servers[x].Name = DBServer.Name;
                        group.Servers[x].X = DBServer.X;
                        group.Servers[x].Y = DBServer.Y;
                        group.Servers[x].ServerID = DBServer.ID;
                    }
                    client.Send(packet.CreateGroupServerList(group));


                    client.Warehouse.SendReturnedItems(packet);


                    client.ClientFlag &= ~Client.ServerFlag.AcceptLogin;
                    client.ClientFlag |= Client.ServerFlag.LoginFull;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }
        }

    }
}
