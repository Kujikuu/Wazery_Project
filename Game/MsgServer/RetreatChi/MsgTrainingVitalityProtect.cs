using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet CreateExpireNotify(this ServerSockets.Packet stream, List<LightConquer_Project.Game.MsgServer.MsgChiInfo.ChiPowerType> Ids)
        {
            stream.InitWriter();
            stream.Write((uint)Ids.Count);
            foreach (var gate in Ids)
            {
                stream.Write((byte)gate);
                stream.Write((byte)1);
                stream.Write((byte)0);
            }
            stream.Finalize(GamePackets.MsgTrainingVitalityExpiryNotify);
            return stream;
        }
        public static unsafe void GetChiRetreatHandler(this ServerSockets.Packet stream, out MsgTrainingVitalityProtect.RetreatType Type, out MsgChiInfo.ChiPowerType Mode)
        {
            Type = (MsgTrainingVitalityProtect.RetreatType)stream.ReadUInt16();
            Mode = (MsgChiInfo.ChiPowerType)stream.ReadUInt8();
        }
        public static unsafe ServerSockets.Packet CreateRetreatedChiPacket(this ServerSockets.Packet stream, LightConquer_Project.Game.MsgServer.MsgTrainingVitalityProtect.RetreatType Type, MsgChiInfo.ChiPowerType Mode)
        {
            stream.InitWriter();
            stream.Write((ushort)Type);
            stream.Write((byte)Mode);
            return stream;
        }
        public static unsafe ServerSockets.Packet ChiRetreatFinalize(this ServerSockets.Packet stream)
        {
            stream.Finalize(GamePackets.MsgTrainingVitalityProtect);
            return stream;
        }
    }
    public unsafe struct MsgTrainingVitalityProtect
    {
        [Flags]
        public enum RetreatType
        {
            #region client
            Info = 0,
            RequestRetreat = 1,
            RequestRestore = 3,
            RequestExtend = 5,
            RequestPayoff = 7,
            RequestAbondan = 9,
            RequestUpdate = 11,
            RequestExtend2 = 13,
            #endregion
            #region Server
            Retreat = 2,
            Restore = 4,
            Extend = 6,
            Payoff = 8,
            Abondan = 10,
            Update = 12,
            Extend2 = 14,
            #endregion Server
        }
        [PacketAttribute(GamePackets.MsgTrainingVitalityProtect)]
        private static void HandleRetreatChi(Client.GameClient _user, ServerSockets.Packet _stream)
        {
            RetreatType Action;
            MsgChiInfo.ChiPowerType Mode;
            _stream.GetChiRetreatHandler(out Action, out Mode);
                switch (Action)
                {
                    case RetreatType.Info:
                        {
                            MsgTrainingVitalityProtectInfo.SendInfo(_stream, _user);
                            break;
                        }
                    #region RequestRetreat
                    case RetreatType.RequestRetreat:
                        {
                            if (_user.Inventory.Contain(3005360, 100) || _user.Inventory.Contain(3005360, 100, 1))
                            {
                                _user.Inventory.Remove(3005360, 100, _stream);
                                goto Reatreat;
                            }
                            else if (_user.Player.ConquerPoints >= 100000)
                            {
                                _user.Player.ConquerPoints -= 100000;
                                goto Reatreat;
                            }
                            else
                            {
                                break;
                            }
                        Reatreat:
                            {
                                switch (Mode)
                                {
                                    case MsgChiInfo.ChiPowerType.Dragon:
                                        {
                                            if (_user.Player.MyChi.DragonTime != 0)
                                                break;
                                            _user.Player.MyChi.DragonPowers = _user.Player.MyChi.Dragon.Fields;
                                            _user.Player.MyChi.DragonTime = DateTime.Now.AddDays(5).ToBinary();
                                            _stream.CreateRetreatedChiPacket(RetreatType.Retreat, Mode);
                                            _user.Send(_stream.ChiRetreatFinalize());
                                            Database.ServerDatabase.SaveClient(_user);
                                            break;
                                        }
                                    case MsgChiInfo.ChiPowerType.Phoenix:
                                        {
                                            if (_user.Player.MyChi.PhoenixTime != 0)
                                                break;
                                            _user.Player.MyChi.PhoenixPowers = _user.Player.MyChi.Phoenix.Fields;
                                            _user.Player.MyChi.PhoenixTime = DateTime.Now.AddDays(5).ToBinary();
                                            _stream.CreateRetreatedChiPacket(RetreatType.Retreat, Mode);
                                            _user.Send(_stream.ChiRetreatFinalize());
                                            Database.ServerDatabase.SaveClient(_user);
                                            break;
                                        }
                                    case MsgChiInfo.ChiPowerType.Tiger:
                                        {
                                            if (_user.Player.MyChi.TigerTime != 0)
                                                break;
                                            _user.Player.MyChi.TigerPowers = _user.Player.MyChi.Tiger.Fields;
                                            _user.Player.MyChi.TigerTime = DateTime.Now.AddDays(5).ToBinary();
                                            _stream.CreateRetreatedChiPacket(RetreatType.Retreat, Mode);
                                            _user.Send(_stream.ChiRetreatFinalize());
                                            Database.ServerDatabase.SaveClient(_user);
                                            break;
                                        }
                                    case MsgChiInfo.ChiPowerType.Turtle:
                                        {
                                            if (_user.Player.MyChi.TurtleTime != 0)
                                                break;
                                            _user.Player.MyChi.TurtlePowers = _user.Player.MyChi.Turtle.Fields;
                                            _user.Player.MyChi.TurtleTime = DateTime.Now.AddDays(5).ToBinary();
                                            _stream.CreateRetreatedChiPacket(RetreatType.Retreat, Mode);
                                            _user.Send(_stream.ChiRetreatFinalize());
                                            Database.ServerDatabase.SaveClient(_user);
                                            break;
                                        }
                                }
                                break;
                            }
                        }
                    #endregion
                    #region RequestRestore
                    case RetreatType.RequestRestore:
                        {
                            switch (Mode)
                            {
                                case MsgChiInfo.ChiPowerType.Dragon:
                                    {
                                        if (_user.Player.MyChi.DragonTime == 0)
                                            break;
                                        if (DateTime.Now > DateTime.FromBinary(_user.Player.MyChi.DragonTime))
                                            break;
                                        _user.Player.MyChi.Dragon.Fields = _user.Player.MyChi.DragonPowers;
                                        Role.Instance.Chi.ComputeStatus(_user.Player.MyChi);
                                        Program.ChiRanking.Upadte(Program.ChiRanking.Dragon, _user.Player.MyChi.Dragon);
                                        _stream.CreateRetreatedChiPacket(RetreatType.Restore, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Phoenix:
                                    {
                                        if (_user.Player.MyChi.PhoenixTime == 0)
                                            break;
                                        if (DateTime.Now > DateTime.FromBinary(_user.Player.MyChi.PhoenixTime))
                                            break;
                                        _user.Player.MyChi.Phoenix.Fields = _user.Player.MyChi.PhoenixPowers;
                                        Role.Instance.Chi.ComputeStatus(_user.Player.MyChi);
                                        Program.ChiRanking.Upadte(Program.ChiRanking.Phoenix, _user.Player.MyChi.Phoenix);
                                        _stream.CreateRetreatedChiPacket(RetreatType.Restore, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Tiger:
                                    {
                                        if (_user.Player.MyChi.TigerTime == 0)
                                            break;
                                        if (DateTime.Now > DateTime.FromBinary(_user.Player.MyChi.TigerTime))
                                            break;
                                        _user.Player.MyChi.Tiger.Fields = _user.Player.MyChi.TigerPowers;
                                        Role.Instance.Chi.ComputeStatus(_user.Player.MyChi);
                                        Program.ChiRanking.Upadte(Program.ChiRanking.Tiger, _user.Player.MyChi.Tiger);
                                        _stream.CreateRetreatedChiPacket(RetreatType.Restore, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Turtle:
                                    {
                                        if (_user.Player.MyChi.TurtleTime == 0)
                                            break;
                                        if (DateTime.Now > DateTime.FromBinary(_user.Player.MyChi.TurtleTime))
                                            break;
                                        _user.Player.MyChi.Turtle.Fields = _user.Player.MyChi.TurtlePowers;
                                        Role.Instance.Chi.ComputeStatus(_user.Player.MyChi);
                                        Program.ChiRanking.Upadte(Program.ChiRanking.Turtle, _user.Player.MyChi.Turtle);
                                        _stream.CreateRetreatedChiPacket(RetreatType.Restore, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                default:
                                    break;
                            }
                            break;
                        }
                    #endregion
                    #region RequestExtend
                    case RetreatType.RequestExtend://Extend
                        {
                            switch (Mode)
                            {
                                case MsgChiInfo.ChiPowerType.Dragon:
                                    {
                                        if (_user.Player.MyChi.DragonTime == 0)
                                            break;
                                        if (DateTime.Now < DateTime.FromBinary(_user.Player.MyChi.DragonTime))
                                            break;
                                        _user.Player.MyChi.DragonTime = DateTime.Now.AddDays(5).ToBinary();
                                        _stream.CreateRetreatedChiPacket(RetreatType.Extend, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Phoenix:
                                    {
                                        if (_user.Player.MyChi.PhoenixTime == 0)
                                            break;
                                        if (DateTime.Now < DateTime.FromBinary(_user.Player.MyChi.PhoenixTime))
                                            break;
                                        _user.Player.MyChi.PhoenixTime = DateTime.Now.AddDays(5).ToBinary();
                                        _stream.CreateRetreatedChiPacket(RetreatType.Extend, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Tiger:
                                    {
                                        if (_user.Player.MyChi.TigerTime == 0)
                                            break;
                                        if (DateTime.Now < DateTime.FromBinary(_user.Player.MyChi.TigerTime))
                                            break;
                                        _user.Player.MyChi.TigerTime = DateTime.Now.AddDays(5).ToBinary();
                                        _stream.CreateRetreatedChiPacket(RetreatType.Extend, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Turtle:
                                    {
                                        if (_user.Player.MyChi.TurtleTime == 0)
                                            break;
                                        if (DateTime.Now < DateTime.FromBinary(_user.Player.MyChi.TurtleTime))
                                            break;
                                        _user.Player.MyChi.TurtleTime = DateTime.Now.AddDays(5).ToBinary();
                                        _stream.CreateRetreatedChiPacket(RetreatType.Extend, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                default:
                                    break;
                            }
                            break;
                        }
                    #endregion
                    #region RequestUpdate
                    case RetreatType.RequestUpdate://Update
                        {
                            switch (Mode)
                            {
                                case MsgChiInfo.ChiPowerType.Dragon:
                                    {
                                        if (_user.Player.MyChi.DragonTime == 0)
                                            break;
                                        if (DateTime.Now > DateTime.FromBinary(_user.Player.MyChi.DragonTime))
                                            break;
                                        _user.Player.MyChi.DragonPowers = _user.Player.MyChi.Dragon.Fields;
                                        _stream.CreateRetreatedChiPacket(RetreatType.Update, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Phoenix:
                                    {
                                        if (_user.Player.MyChi.PhoenixTime == 0)
                                            break;
                                        if (DateTime.Now > DateTime.FromBinary(_user.Player.MyChi.PhoenixTime))
                                            break;
                                        _user.Player.MyChi.PhoenixPowers = _user.Player.MyChi.Phoenix.Fields;
                                        _stream.CreateRetreatedChiPacket(RetreatType.Update, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Tiger:
                                    {
                                        if (_user.Player.MyChi.TigerTime == 0)
                                            break;
                                        if (DateTime.Now > DateTime.FromBinary(_user.Player.MyChi.TigerTime))
                                            break;
                                        _user.Player.MyChi.TigerPowers = _user.Player.MyChi.Tiger.Fields;
                                        _stream.CreateRetreatedChiPacket(RetreatType.Update, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Turtle:
                                    {
                                        if (_user.Player.MyChi.TurtleTime == 0)
                                            break;
                                        if (DateTime.Now > DateTime.FromBinary(_user.Player.MyChi.TurtleTime))
                                            break;
                                        _user.Player.MyChi.TurtlePowers = _user.Player.MyChi.Turtle.Fields;
                                        _stream.CreateRetreatedChiPacket(RetreatType.Update, Mode);
                                        _user.Send(_stream.ChiRetreatFinalize());
                                        break;
                                    }
                                default:
                                    break;
                            }
                            break;
                        }
                    #endregion
                    #region RequestPayoff
                    case RetreatType.RequestPayoff://Restore pay off
                        {
                            /*int Points = 4000;
                            int IDentifier = (int)(Mode - 1);
                            if (IDentifier > 3 || IDentifier < 0)
                                break;
                            TimeSpan Time = DateTime.Now - DateTime.FromBinary(_user.Player.MyChi.Reatreat[IDentifier].Item2);
                            Points += (int)Time.TotalMinutes;
                            if (_user.Player.MyChi.ChiPoints < Points)
                            {
                                break;
                            }
                            _user.Player.MyChi.ChiPoints -= Points;
                            switch (Mode)
                            {
                                case MsgChiInfo.ChiPowerType.Dragon:
                                    {
                                        _user.Player.MyChi.Dragon = _user.Player.MyChi.Reatreat[IDentifier].Item1;
                                        Program.ChiRanking.Upadte(Program.ChiRanking.Dragon, _user.Player.MyChi.Dragon);
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Phoenix:
                                    {
                                        _user.Player.MyChi.Phoenix = _user.Player.MyChi.Reatreat[IDentifier].Item1;
                                        Program.ChiRanking.Upadte(Program.ChiRanking.Phoenix, _user.Player.MyChi.Phoenix);
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Tiger:
                                    {
                                        _user.Player.MyChi.Tiger = _user.Player.MyChi.Reatreat[IDentifier].Item1;
                                        Program.ChiRanking.Upadte(Program.ChiRanking.Tiger, _user.Player.MyChi.Tiger);
                                        break;
                                    }
                                case MsgChiInfo.ChiPowerType.Turtle:
                                    {
                                        _user.Player.MyChi.Turtle = _user.Player.MyChi.Reatreat[IDentifier].Item1;
                                        Program.ChiRanking.Upadte(Program.ChiRanking.Turtle, _user.Player.MyChi.Turtle);
                                        break;
                                    }
                                default:
                                    break;
                            }
                            Role.Instance.Chi.ComputeStatus(_user.Player.MyChi);
                            _stream.CreateRetreatedChiPacket(RetreatType.Payoff, Mode);
                            _user.Send(_stream.ChiRetreatFinalize());*/
                            break;
                        }
                    #endregion
                    #region RequestAbondan
                    case RetreatType.RequestAbondan://Abondan 
                        {
                           /* int IDentifier = (int)(Mode - 1);
                            if (IDentifier > 3 || IDentifier < 0)
                                break;
                            if (_user.Player.MyChi.Reatreat[IDentifier] == null)
                                break;
                            _user.Player.MyChi.Reatreat[IDentifier] = null;
                            _stream.CreateRetreatedChiPacket(RetreatType.Abondan, Mode);
                            _user.Send(_stream.ChiRetreatFinalize());*/
                            break;
                        }
                    #endregion
                    #region RequestExtend2
                    case RetreatType.RequestExtend2:
                        {
                            /*int IDentifier = (int)(Mode - 1);
                            if (IDentifier > 3 || IDentifier < 0)
                                break;
                            if (_user.Player.MyChi.Reatreat[IDentifier] == null)
                                break;
                            var power = _user.Player.MyChi.Reatreat[IDentifier].Item1;
                            _user.Player.MyChi.Reatreat[IDentifier] = Tuple.Create(power, DateTime.Now.AddDays(5).ToBinary());
                            _stream.CreateRetreatedChiPacket(RetreatType.Extend, Mode);
                            _user.Send(_stream.ChiRetreatFinalize());*/
                            break;
                        }
                    #endregion
                    default: MyConsole.WriteLine("Unknown Chi Retreat Action Type [" + Action + "]."); break;
                }
            
        }
        public static void Login(Client.GameClient client)
        {
            if (client.Player.VipLevel < 4)
                return;
            List<LightConquer_Project.Game.MsgServer.MsgChiInfo.ChiPowerType> Removed = new List<MsgChiInfo.ChiPowerType>(4);
            if (client.Player.MyChi.DragonTime != 0)
            {
                if (DateTime.Now > DateTime.FromBinary(client.Player.MyChi.DragonTime))
                {
                    Removed.Add(LightConquer_Project.Game.MsgServer.MsgChiInfo.ChiPowerType.Dragon);
                }
            }
            if (client.Player.MyChi.PhoenixTime != 0)
            {
                if (DateTime.Now > DateTime.FromBinary(client.Player.MyChi.PhoenixTime))
                {
                    Removed.Add(LightConquer_Project.Game.MsgServer.MsgChiInfo.ChiPowerType.Phoenix);
                }
            }
            if (client.Player.MyChi.TigerTime != 0)
            {
                if (DateTime.Now > DateTime.FromBinary(client.Player.MyChi.TigerTime))
                {
                    Removed.Add(LightConquer_Project.Game.MsgServer.MsgChiInfo.ChiPowerType.Tiger);
                }
            }
            if (client.Player.MyChi.TurtleTime != 0)
            {
                if (DateTime.Now > DateTime.FromBinary(client.Player.MyChi.TurtleTime))
                {
                    Removed.Add(LightConquer_Project.Game.MsgServer.MsgChiInfo.ChiPowerType.Turtle);
                }
            }
            if(Removed.Count > 0)
            {
                using (var recycle = new ServerSockets.RecycledPacket())
                {
                    var stream = recycle.GetStream();
                    client.Send(stream.CreateExpireNotify(Removed));
                }
            }
        }
    }
}
