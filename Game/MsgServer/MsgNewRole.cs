using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public static class MsgNewRole
    {

        public static object SynName = new object();


        public static void GetNewRoleInfo(this ServerSockets.Packet msg, out string name, out ushort Body, out byte Class)
        {
            msg.ReadBytes(20);
            name = msg.ReadCString(16);
            msg.ReadBytes(32);

            Body = msg.ReadUInt16();
            Class = msg.ReadUInt8();

        }

        [PacketAttribute(Game.GamePackets.NewClient)]
        public unsafe static void CreateCharacter(Client.GameClient client, ServerSockets.Packet stream)
        {
            if ((client.ClientFlag & Client.ServerFlag.CreateCharacter) == Client.ServerFlag.CreateCharacter)
            {
                client.ClientFlag &= ~Client.ServerFlag.AcceptLogin;


                string CharacterName; ushort Body; byte Class;

                stream.GetNewRoleInfo(out CharacterName, out Body, out Class);

                //last update
                byte attackType = 0;
                switch (Class)
                {
                    case 0:
                    case 1: Class = 100; break;
                    case 2:
                    case 3: Class = 10; break;
                    case 4:
                    case 5: Class = 40; break;
                    case 6:
                    case 7: Class = 20; break;
                        //case 8:
                        //case 9: Class = 50; break;
                        //case 10:
                        //case 11: Class = 60; break;
                        //case 12:
                        //case 13: Class = 70; break;
                        //case 14:
                        //case 15: Class = 80; break;

                        //case 16:
                        //case 17:
                        //    {
                        //        attackType = 0;
                        //        Class = 160;
                        //        break;
                        //    }
                        //case 18:
                        //case 19:
                        //    {
                        //        attackType = 1;
                        //        Class = 160;
                        //        break;
                        //    }
                }


                if (!ExitBody(Body))
                {
#if Arabic
                     client.Send(new MsgServer.MsgMessage("AHAHAH! WRONG Body, NICE TRY", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
#else
                    client.Send(new MsgServer.MsgMessage("AHAHAH! WRONG Body, NICE TRY", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));

#endif
                    return;
                }
                if (!ExitClass(Class))
                {
#if Arabic
                       client.Send(new MsgServer.MsgMessage("AHAHAH! WRONG Class, NICE TRY", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
#else
                    client.Send(new MsgServer.MsgMessage("AHAHAH! WRONG Class, NICE TRY", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
#endif

                    return;
                }

                CharacterName = CharacterName.Replace("\0", "");
                if (Program.NameStrCheck(CharacterName))
                {
                    if (!Database.Server.NameUsed.Contains(CharacterName.GetHashCode()))
                    {
                        client.ClientFlag &= ~Client.ServerFlag.CreateCharacter;

                        lock (Database.Server.NameUsed)
                            Database.Server.NameUsed.Add(CharacterName.GetHashCode());

                        client.Player.Name = CharacterName;
                        client.Player.Class = Class;
                        client.Player.Body = Body;

                        client.Player.Level = 1;
                        client.Player.Map = 1100;
                        client.Player.X = 139;//300,279
                        client.Player.Y = 108;


                        Database.DataCore.LoadClient(client.Player);

                        client.Player.UID = client.ConnectionUID;
                        if (attackType == 1)
                            client.Player.MainFlag |= Role.Player.MainFlagType.OnMeleeAttack;

                        Database.DataCore.AtributeStatus.GetStatus(client.Player);

                        if (Body == 1003 || Body == 1004)
                            client.Player.Face = (ushort)Program.GetRandom.Next(1, 50);
                        else
                            client.Player.Face = (ushort)Program.GetRandom.Next(201, 250);

                        byte Color = (byte)Program.GetRandom.Next(4, 8);
                        client.Player.Hair = (ushort)(Color * 100 + 10 + (byte)Program.GetRandom.Next(4, 9));



                        if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                        {
                            client.Equipment.Add(stream, 152005, Role.Flags.ConquerItem.Ring);
                            client.Equipment.Add(stream, 421301, Role.Flags.ConquerItem.RightWeapon);
                        }

                        else if (Database.AtributesStatus.IsArcher(client.Player.Class))
                        {
                            client.Equipment.Add(stream, 150003, Role.Flags.ConquerItem.Ring);
                            client.Equipment.Add(stream, 500006, Role.Flags.ConquerItem.RightWeapon);
                        }
                        else
                        {
                            client.Equipment.Add(stream, 150003, Role.Flags.ConquerItem.Ring);
                            if (Database.AtributesStatus.IsPirate(client.Player.Class))
                            {
                                client.Equipment.Add(stream, 611301, Role.Flags.ConquerItem.RightWeapon);
                                client.Equipment.Add(stream, 612301, Role.Flags.ConquerItem.LeftWeapon);
                            }
                            else if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                            {
                                client.Equipment.Add(stream, 420301, Role.Flags.ConquerItem.RightWeapon);
                                client.Equipment.Add(stream, 420301, Role.Flags.ConquerItem.LeftWeapon);
                            }

                            else if (Database.AtributesStatus.IsLee(client.Player.Class))
                            {
                                client.Equipment.Add(stream, 617301, Role.Flags.ConquerItem.RightWeapon);
                                client.Equipment.Add(stream, 617301, Role.Flags.ConquerItem.LeftWeapon);
                                client.Equipment.Add(stream, 138313, Role.Flags.ConquerItem.Armor);
                            }
                            else if (Database.AtributesStatus.IsMonk(client.Player.Class))
                            {
                                client.Equipment.Add(stream, 610301, Role.Flags.ConquerItem.RightWeapon);
                                client.Equipment.Add(stream, 610301, Role.Flags.ConquerItem.LeftWeapon);
                            }
                            else if (Database.AtributesStatus.IsNinja(client.Player.Class))
                            {
                                client.Equipment.Add(stream, 601301, Role.Flags.ConquerItem.RightWeapon);
                                client.Equipment.Add(stream, 601301, Role.Flags.ConquerItem.LeftWeapon);
                            }
                            else if (Database.AtributesStatus.IsWindWalker(client.Player.Class))
                            {

                                client.Equipment.Add(stream, 101319, Role.Flags.ConquerItem.Armor);
                                client.Equipment.Add(stream, 626301, Role.Flags.ConquerItem.RightWeapon);
                                client.Equipment.Add(stream, 626301, Role.Flags.ConquerItem.LeftWeapon);
                            }
                            else if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                            {
                                client.Equipment.Add(stream, 561301, Role.Flags.ConquerItem.RightWeapon);
                            }
                            else
                                client.Equipment.Add(stream, 410301, Role.Flags.ConquerItem.RightWeapon);
                        }
                        if (!Database.AtributesStatus.IsLee(client.Player.Class) && !Database.AtributesStatus.IsWindWalker(client.Player.Class))
                            client.Equipment.Add(stream, 132009, Role.Flags.ConquerItem.Armor);

                        if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                        {
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FastBlader))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FastBlader);

                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ScrenSword))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ScrenSword);

                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Cyclone))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Cyclone);
                        }
                        else if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                        {
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Superman))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Superman);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FastBlader))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FastBlader);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ScrenSword))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ScrenSword);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Shield))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Shield);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Accuracy))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Accuracy);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Roar))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Roar);
                        }
                        else if (Database.AtributesStatus.IsArcher(client.Player.Class))
                        {
                            client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.XpFly);
                        }
                        else if (Database.AtributesStatus.IsNinja(client.Player.Class))
                        {
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FatalStrike))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FatalStrike);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ToxicFog))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ToxicFog);
                        }
                        else if (Database.AtributesStatus.IsMonk(client.Player.Class))
                        {
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.WhirlwindKick))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.WhirlwindKick);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TripleAttack))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.TripleAttack);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Oblivion))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Oblivion);
                        }

                        else if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                        {
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ChainBolt))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ChainBolt);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Lightning))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Lightning);

                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Thunder))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Thunder);

                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Cure))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Cure);
                        }

#if Encore
                        client.Player.Money += 10000;
#else

                        client.Player.Money += 10000;
#endif


                        client.Player.SendUpdate(stream, client.Player.Money, MsgServer.MsgUpdate.DataType.Money);
                        client.Inventory.Add(stream, 727266, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true); //EquipmentPack
                        client.Inventory.Add(stream, 722122);//NewbieToken
                        client.Send(new MsgServer.MsgMessage("ANSWER_OK", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));

                        client.Player.SendGlobalMesage($"{client.Player.Name} has created a new character, Please welcome him!", MsgMessage.ChatMode.TopLeft);
                        //Program.SendGlobalPackets.Enqueue(new MsgMessage(, MsgMessage.MsgColor.white, MsgMessage.ChatMode.TopLeft).GetArray(stream));

                        client.Status.MaxHitpoints = client.CalculateHitPoint();
                        client.Player.HitPoints = (int)client.Status.MaxHitpoints;


                        client.ClientFlag |= Client.ServerFlag.CreateCharacterSucces;
                    }
                    else
                    {
#if Arabic
                          client.Send(new MsgServer.MsgMessage("The name is in use! try other name", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
#else
                        client.Send(new MsgServer.MsgMessage("The name is in use! try other name", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
#endif

                    }
                }
                else
                {
#if Arabic
                     client.Send(new MsgServer.MsgMessage("Invalid characters name!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
#else
                    client.Send(new MsgServer.MsgMessage("Invalid characters name!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
#endif

                }
            }
        }

        public static bool ExitBody(ushort _body)
        {
            return (_body == 1003 || _body == 1004 || _body == 2001 || _body == 2002);
        }

        public static bool ExitClass(byte cls)
        {
            return (cls == 10
                || cls == 20
                || cls == 40
                //|| cls == 50
                //|| cls == 60
                //|| cls == 70
                || cls == 100);
            //|| cls == 80 
            //|| cls == 160);
        }
    }
}
