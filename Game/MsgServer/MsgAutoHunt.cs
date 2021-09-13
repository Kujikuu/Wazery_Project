using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public static class MsgAutoHunt
    {
        [Flags]
        public enum Mode : byte
        {
            Icon = 0,
            Start = 1,
            EndAuto = 2,
            EXPGained = 3,
            SuddenlyGain = 4,
            FirstCreditKilledBy = 5,
            KilledBy = 6,
            ChangedMap = 7
        }
        public static unsafe ServerSockets.Packet AutoHuntCreate(this ServerSockets.Packet stream, ushort type, ulong Icon, ulong Exp = 0, string KillerName = null)
        {

            stream.InitWriter();
            stream.Write(type);
            stream.Write(Icon);//341
            stream.Write(Exp);//ulong.MaxValue);
            stream.Write(KillerName);//ulong.MaxValue);
            stream.Finalize(GamePackets.AutoHunt);
            return stream;
        }
        public static unsafe void GetAutoHuntOperation(this ServerSockets.Packet stream, out ushort Act)
        {
            Act = stream.ReadUInt16();
        }
        [PacketAttribute(GamePackets.AutoHunt)]
        private static unsafe void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.Player.Map != 5000)
            {
                user.SendSysMesage("Auto hunt is not available.");
                return;
            }
            ushort Action;
            stream.GetAutoHuntOperation(out Action);
            switch ((Mode)Action)
            {
                case Mode.Start:
                    {
                        {
                            if (user.Player.VipLevel >= 7)
                            {
                                if (user.Player.OnXPSkill() != MsgUpdate.Flags.Normal)
                                    user.Player.RemoveFlag(user.Player.OnXPSkill());
                                user.Send(stream.AutoHuntCreate(0, 341));
                                user.Send(stream.AutoHuntCreate(1, 341));
                                user.Player.OnAutoHunt = true;
                                user.Player.AutoHuntExp = 0;
                            }
                        }

                        break;
                    }
                case Mode.EndAuto:
                    {
                        if (user.Player.AutoHuntExp > 0)
                        {
                            user.Send(stream.AutoHuntCreate(3, 0, user.Player.AutoHuntExp));
                            user.IncreaseAutoExperience(stream, user.Player.AutoHuntExp);
                        }
                        user.Send(stream.AutoHuntCreate(2, 0, user.Player.AutoHuntExp));
                        user.Player.OnAutoHunt = false;
                        user.Player.AutoHuntExp = 0;
                        break;
                    }
                case Mode.EXPGained:
                    {
                        if (user.Player.AutoHuntExp > 0)
                        {
                            user.IncreaseAutoExperience(stream, user.Player.AutoHuntExp);
                        }
                        user.Send(stream.AutoHuntCreate(3, 0, user.Player.AutoHuntExp));
                        user.Player.AutoHuntExp = 0;
                        break;
                    }
                default: Console.WriteLine("[AutoHunt] Unknown Action: " + Action + ""); break;
            }

        }
    }
}
