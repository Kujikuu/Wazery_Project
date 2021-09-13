using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LightConquer_Project.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static void GetJiangHuUpdates(this ServerSockets.Packet stream, out  MsgJiangHuUpdates.Action Action,out byte Star,out byte Stage)
        {
            uint freecourse = stream.ReadUInt32();
            byte unKnow = stream.ReadUInt8();
            Action = (MsgJiangHuUpdates.Action)stream.ReadUInt8();
            Star = stream.ReadUInt8();
            Stage = stream.ReadUInt8();
        }

        public static unsafe ServerSockets.Packet JiangHuUpdatesCreate(this ServerSockets.Packet stream, uint FreeCourse
            , MsgJiangHuUpdates.Action Action, byte Star, byte Stage, ushort Atribut
            , byte FreeTimeTodeyUsed, uint RoundBuyPoints)
        {
            stream.InitWriter();

            stream.Write(FreeCourse);

            stream.Write((byte)0);
            stream.Write((byte)Action);
            stream.Write(Star);
            stream.Write(Stage);
            stream.Write(Atribut);
            stream.Write(FreeTimeTodeyUsed);
            stream.Write(RoundBuyPoints);

            stream.Finalize(GamePackets.JiangHuUpdates);

            return stream;
        }

    }
    public unsafe struct MsgJiangHuUpdates
    {
        public enum Action : byte
        {
            CreateStar = 0,
            BuyStrength = 1,
            TrainingPill = 2
        }


        [PacketAttribute(GamePackets.JiangHuUpdates)]
        private static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
#if Jiang
             MsgJiangHuUpdates.Action Mode; 
            byte Star;
            byte Stage;

            stream.GetJiangHuUpdates(out Mode, out Star, out Stage);


            switch (Mode)
            {
                case Action.CreateStar:
                    {
                        if (user.Player.MyJiangHu != null)
                        {
                            if (Star > 9 || Stage > 9)
                                break;
                            if (Star > 1 && !user.Player.MyJiangHu.ArrayStages[Stage - 1].ArrayStars[Star - 2].Activate)
                                break;
                            if (user.Player.SubClass.StudyPoints >= 20 && user.Player.MyJiangHu.Talent >= 1 && user.Player.MyJiangHu.FreeCourse >= 10000
                                && user.Player.MyJiangHu.FreeTimeToday < 10)
                            {

                                user.Activeness.IncreaseTask(38);
                                user.Player.MyJiangHu.FreeTimeToday += 1;
                                user.Player.MyJiangHu.Level = (byte)user.Player.Level;
                                user.Player.MyJiangHu.Talent -= 1;
                                user.Player.MyJiangHu.FreeCourse -= 10000;
                                user.Player.SubClass.RemoveStudy(user, 20,stream);
                                user.Player.MyJiangHu.CreateRollValue(stream, user, Star, Stage);
                                user.Player.MyJiangHu.SendInfo(user, MsgJiangHuInfo.JiangMode.UpdateTime, false, user.Player.MyJiangHu.FreeCourse.ToString(), user.Player.MyJiangHu.Time.ToString());
                                user.Player.MyJiangHu.SendInfo(user, MsgJiangHuInfo.JiangMode.UpdateStar, false, Stage.ToString(), Star.ToString());
                                user.Player.MyJiangHu.SendInfo(user, MsgJiangHuInfo.JiangMode.UpdateTalent, true, user.Player.UID.ToString(), user.Player.MyJiangHu.Talent.ToString());

                            }
                        }
                        break;
                    }
                case Action.TrainingPill:
                    {
                        if (user.Player.MyJiangHu != null)
                        {
                            if (Star > 9 || Stage > 9)
                                break;
                            if (Star > 1 && !user.Player.MyJiangHu.ArrayStages[Stage - 1].ArrayStars[Star - 2].Activate)
                                break;
                            ushort priceCps = (ushort)((user.Player.MyJiangHu.RoundBuyPoints * 10) + 10); // oficial conquer cps
                            uint NeedPills = (uint)priceCps / 10;
                            
                            if (user.Inventory.Contain(3003124,NeedPills))
                            {
                                user.Player.AddChampionPoints(10);
                                user.SendSysMesage("You received 10 ChampionPoints.");
                                user.Activeness.IncreaseTask(39);
                                user.Activeness.IncreaseTask(38);
                                user.Inventory.Remove(3003124, NeedPills, stream);
                                user.Player.MyJiangHu.RoundBuyPoints = (byte)Math.Min(49, user.Player.MyJiangHu.RoundBuyPoints + 1);
                                user.Player.MyJiangHu.CreateRollValue(stream, user, Star, Stage);
                                user.Player.MyJiangHu.SendInfo(user, MsgJiangHuInfo.JiangMode.UpdateStar, false, Stage.ToString(), Star.ToString());
                            }
                            else
                            {
#if Arabic
                                user.SendSysMesage("You do not have " + NeedPills + " FavoredTrainingPill with you.");
#else
                                user.SendSysMesage("You do not have " + NeedPills + " FavoredTrainingPill with you.");
#endif

                            }
                        }
                        break;
                    }
                case Action.BuyStrength:
                    {
                        if (user.Player.MyJiangHu != null)
                        {
                            if (Star > 9 || Stage > 9)
                                break;
                            if (Star > 1 && !user.Player.MyJiangHu.ArrayStages[Stage - 1].ArrayStars[Star - 2].Activate)
                                break;
                            ushort priceCps = (ushort)((user.Player.MyJiangHu.RoundBuyPoints * 10) + 10); // oficial conquer cps
                            if (user.Player.ConquerPoints >= priceCps)
                            {
                                user.Activeness.IncreaseTask(38);
                                user.Player.ConquerPoints -= priceCps;


                                user.Player.AddChampionPoints(10);
                                user.SendSysMesage("You received 10 ChampionPoints.");
                               
                                user.Player.MyJiangHu.RoundBuyPoints = (byte)Math.Min(49, user.Player.MyJiangHu.RoundBuyPoints + 1);
                                user.Player.MyJiangHu.ActiveJiangMode(user);

                                user.Player.MyJiangHu.CreateRollValue(stream,user, Star, Stage);
                                user.Player.MyJiangHu.SendInfo(user, MsgJiangHuInfo.JiangMode.UpdateStar, false, Stage.ToString(),Star.ToString());
                            }
                            else
                            {
#if Arabic
                                  user.SendSysMesage("You do not have " + priceCps + " ConquerPoints with you.");
#else
                                user.SendSysMesage("You do not have " + priceCps + " ConquerPoints with you.");
#endif    
                              
                            }
                        }
                        break;
                    }
            }
#endif
        }
    }
}
