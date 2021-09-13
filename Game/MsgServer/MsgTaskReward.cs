using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public static class MsgTaskReward
    {
        public static unsafe void GetMsgTaskReward(this ServerSockets.Packet stream, out ActionID Action, out uint dwPram)
        {
            Action = (ActionID)stream.ReadUInt8();
            dwPram = stream.ReadUInt32();
        }

        public static unsafe ServerSockets.Packet MsgTaskRewardCreate(this ServerSockets.Packet stream, ActionID Action, uint dwParam)
        {
            stream.InitWriter();

            stream.Write((byte)Action);
            stream.Write(dwParam);
            stream.Finalize(GamePackets.MsgTaskReward);
            return stream;
        }
        public enum ActionID : byte
        {
            Open = 0,
            SetReward = 1,
            Draw = 2,
            Claim = 3,
            Redraw = 4,
            Continue = 5,
            Times10Spinning = 7
        }
        [PacketAttribute(GamePackets.MsgTaskReward)]
        private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
          //  MyConsole.PrintPacketAdvanced(stream.Memory, stream.Size);

            ActionID Action;
            uint dwParam;
            stream.GetMsgTaskReward(out Action, out dwParam);
            switch (Action)
            {
                case ActionID.Times10Spinning:
                    {
                        if (!user.Inventory.HaveSpace(100))
                        {
#if Arabic
                                   user.SendSysMesage("Please make 1 more space in your inventory.");
#else
                            user.SendSysMesage("You can redraw this quest`s 1 time only please select Draw..");
#endif

                            break;
                        }
                        //for (int x = 0; x < 10; x++)
                        //{
                        //    List<uint> Rewards = new List<uint>();
                        //    if (Database.TaskRewards.Rewards.TryGetValue(dwParam, out Rewards))
                        //    {
                        //        if (user.Inventory.Contain(3600023, 1, 0) || user.Inventory.Contain(3600023, 1, 1) )
                        //        {
                        //            user.Player.TaskRewardIndex = (uint)Program.GetRandom.Next(0, Rewards.Count);
                        //            user.Player.TaskReward = Rewards.ToArray()[user.Player.TaskRewardIndex];
                        //            user.Inventory.Remove(3600023, 1, stream);
                        //            //user.Inventory.Add(stream, user.Player.TaskReward, 1);
                        //            user.Send(stream.MsgTaskRewardCreate(ActionID.SetReward, user.Player.TaskRewardIndex));
                        //        }
                        //        else if (user.Inventory.Contain(3008733, 1, 0) || user.Inventory.Contain(3008733, 1, 1))
                        //        {
                        //            user.Player.TaskRewardIndex = (uint)Program.GetRandom.Next(0, Rewards.Count);
                        //            user.Player.TaskReward = Rewards.ToArray()[user.Player.TaskRewardIndex];
                        //            user.Inventory.Remove(3008733, 1, stream);
                        //            //user.Inventory.Add(stream, user.Player.TaskReward, 1);
                        //            user.Send(stream.MsgTaskRewardCreate(ActionID.SetReward, user.Player.TaskRewardIndex));

                        //        }
                        //        else if (user.Inventory.Contain(3301169, 1, 0) || user.Inventory.Contain(3301169, 1, 1))
                        //        {
                        //            user.Player.TaskRewardIndex = (uint)Program.GetRandom.Next(0, Rewards.Count);
                        //            user.Player.TaskReward = Rewards.ToArray()[user.Player.TaskRewardIndex];
                        //            user.Inventory.Remove(3301169, 1, stream);
                        //            //user.Inventory.Add(stream, user.Player.TaskReward, 1);
                        //            user.Send(stream.MsgTaskRewardCreate(ActionID.SetReward, user.Player.TaskRewardIndex));

                        //        }
                        //    }
                        //}
                        break;
                    }
                case ActionID.Continue:
                case ActionID.Draw:
                    {
                        if (!user.Inventory.HaveSpace(1))
                        {
#if Arabic
                                   user.SendSysMesage("Please make 1 more space in your inventory.");
#else
                            user.SendSysMesage("Please make 1 more space in your inventory.");
#endif

                            break;
                        }
                        List<uint> Rewards = new List<uint>();
                        if (Database.TaskRewards.Rewards.TryGetValue(dwParam, out Rewards))
                        {
                            if (user.Inventory.Contain(3600023, 1, 0) || user.Inventory.Contain(3600023, 1, 1))
                            {
                                user.Player.TaskRewardIndex = (uint)Program.GetRandom.Next(0, Rewards.Count);
                                user.Player.TaskReward = Rewards.ToArray()[user.Player.TaskRewardIndex];
                                user.Inventory.Remove(3600023, 1, stream);
                                user.Send(stream.MsgTaskRewardCreate(ActionID.SetReward, user.Player.TaskRewardIndex));
                            }
                            else if (user.Inventory.Contain(3008733, 1, 0) || user.Inventory.Contain(3008733, 1, 1))
                            {
                                user.Player.TaskRewardIndex = (uint)Program.GetRandom.Next(0, Rewards.Count);
                                user.Player.TaskReward = Rewards.ToArray()[user.Player.TaskRewardIndex];
                                user.Inventory.Remove(3008733, 1, stream);
                                user.Send(stream.MsgTaskRewardCreate(ActionID.SetReward, user.Player.TaskRewardIndex));

                            }
                            else if (user.Inventory.Contain(3301169, 1, 0) || user.Inventory.Contain(3301169, 1, 1))
                            {
                                user.Player.TaskRewardIndex = (uint)Program.GetRandom.Next(0, Rewards.Count);
                                user.Player.TaskReward = Rewards.ToArray()[user.Player.TaskRewardIndex];
                                user.Inventory.Remove(3301169, 1, stream);
                                //user.Inventory.Add(stream, user.Player.TaskReward, 1);
                                user.Send(stream.MsgTaskRewardCreate(ActionID.SetReward, user.Player.TaskRewardIndex));

                            }
                        }
                        break;
                    }
                case ActionID.Claim:
                    {
                        if (user.Player.TaskReward != 0)
                        {
#if Arabic
                              user.SendSysMesage("You received a new item from the roulette prize.", MsgMessage.ChatMode.System);
#else
                            user.SendSysMesage("You received a new item from the roulette prize.", MsgMessage.ChatMode.System);
#endif
                          
                            user.Inventory.Add(stream, user.Player.TaskReward, 1, 0,0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false);
                            user.Player.TaskReward = 0;
                        }
                        break;
                    }
                case ActionID.Redraw:
                    {
                        if (!user.Inventory.HaveSpace(100))
                        {
#if Arabic
                                   user.SendSysMesage("Please make 1 more space in your inventory.");
#else
                            user.SendSysMesage("You can redraw this quest`s 1 time only please select Draw..");
#endif

                            break;
                        }
                        if (user.Player.ConquerPoints > 10000)
                        {
                            
                            if (!user.Inventory.HaveSpace(1))
                            {
#if Arabic
                                    user.SendSysMesage("Please make 1 more space in your inventory.");
#else
                                user.SendSysMesage("Please make 1 more space in your inventory.");
#endif
                            
                                break;
                            }
                            List<uint> Rewards = new List<uint>();
                            if (Database.TaskRewards.Rewards.TryGetValue(dwParam, out Rewards))
                            {
                                user.Player.ConquerPoints -= 10000;
                                user.Player.TaskRewardIndex = (uint)Program.GetRandom.Next(0, Rewards.Count);
                                user.Player.TaskReward = Rewards.ToArray()[user.Player.TaskRewardIndex];
                                user.Send(stream.MsgTaskRewardCreate(ActionID.SetReward, user.Player.TaskRewardIndex));
                            }
                        }
                        break;
                    }
            }

        }
    }
}
