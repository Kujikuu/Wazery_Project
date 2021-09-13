using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe void GetInnerPower(this ServerSockets.Packet stream, out MsgInnerPower.ActionID mode, out uint dwparam, out uint dwparam2)
        {
            mode = (MsgInnerPower.ActionID)stream.ReadUInt8();
            dwparam = stream.ReadUInt32();
            dwparam2 = stream.ReadUInt32();
        }
    }
    public class MsgInnerPower
    {
        public enum ActionID : byte
        {
            InfoStage = 0,
            UpdateGong = 3,
            UnlockStage = 4,
            OpenInner = 5,
            TransferInnerPowers = 6,
        }
        [PacketAttribute(GamePackets.InnerPowerHandler)]
        public unsafe static void InnerPowerHandler(Client.GameClient client, ServerSockets.Packet stream)
        {
            ActionID Action;
            uint dwparam;
            uint dwparam2;

            stream.GetInnerPower(out Action, out dwparam, out dwparam2);

            switch (Action)
            {
                case ActionID.TransferInnerPowers:
                    {

                        break;
                    }
                case ActionID.OpenInner:
                    {
                        //Client.GameClient user;
                        //if (Database.Server.GamePoll.TryGetValue(dwparam, out user))
                        //{
                        //    client.Send(stream.InnerPowerGui(user.Player.InnerPower.GetNeiGongs()));
                        //    var stages = client.Player.InnerPower.Stages.Where(p => p.Complete);
                        //    if (stages.Count() > 0)
                        //        client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateStage, user.Player.UID, stages.Last()));
                        //}
                        break;
                    }
                case ActionID.InfoStage:
                    {

                        //Client.GameClient user;
                        //if (Database.Server.GamePoll.TryGetValue(dwparam, out user))
                        //{
                        //    client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateStage, user.Player.UID, user.Player.InnerPower.Stages[dwparam2 - 1]));
                        //    client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateScore, user.Player.UID, user.Player.InnerPower.Stages[dwparam2 - 1]));
                        //}
                        break;
                    }
                case ActionID.UnlockStage:
                    {
                        //Database.InnerPowerTable.Stage DBStage = null;
                        //Database.InnerPowerTable.Stage.NeiGong DBGong = null;
                        //if (Database.InnerPowerTable.GetDBInfo(dwparam, out DBStage, out DBGong))
                        //{
                        //    if (DBGong.CheckAccount(client.Player.Reborn, client.Player.Level) && client.Inventory.Contain(DBGong.ItemID, 1)
                        //        && client.Player.InnerPower.isUnlockedNeiGong((byte)dwparam))
                        //    {
                        //        Role.Instance.InnerPower.Stage stage = null;
                        //        Role.Instance.InnerPower.Stage.NeiGong gong = null;
                        //        if (client.Player.InnerPower.TryGetStageAndGong((byte)dwparam, out stage, out gong))
                        //        {
                        //            stage.UnLocked = gong.Unlocked = true;

                        //            stream.InnerPowerGui(client.Player.InnerPower.GetNeiGongs());
                        //            client.Send(stream);
                        //            client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateStage, client.Player.UID, stage));
                        //            client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateScore, client.Player.UID, stage));

                        //            client.Inventory.Remove(DBGong.ItemID, 1, stream);
                        //        }
                        //    }
                        //}
                        break;
                    }
                case ActionID.UpdateGong:
                    {
                        //Database.InnerPowerTable.Stage DBStage = null;
                        //Database.InnerPowerTable.Stage.NeiGong DBGong = null;
                        //if (Database.InnerPowerTable.GetDBInfo(dwparam, out DBStage, out DBGong))
                        //{
                        //    Role.Instance.InnerPower.Stage stage = null;
                        //    Role.Instance.InnerPower.Stage.NeiGong gong = null;
                        //    if (client.Player.InnerPower.TryGetStageAndGong((byte)dwparam, out stage, out gong))
                        //    {
                        //        if (stage.UnLocked && gong.Unlocked && gong.level < DBGong.MaxLevel)
                        //        {
                        //            int potency_cost = (int)DBGong.ProgressNeiGongValue[Math.Min(gong.level, (int)(DBGong.ProgressNeiGongValue.Length - 1))];
                        //            if (client.Player.InnerPower.Potency >= potency_cost)
                        //            {
                        //                client.Player.InnerPower.AddPotency(stream, client, -potency_cost);

                        //                gong.level += 1;
                        //                gong.Score = (byte)Math.Ceiling(((float)((float)100 / (float)DBGong.MaxLevel) * (float)gong.level));
                        //                gong.Complete = gong.level == DBGong.MaxLevel;

                        //                client.Send(stream.InnerPowerGui(client.Player.InnerPower.GetNeiGongs()));
                        //                client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateStage, client.Player.UID, stage));
                        //                client.Send(stream.InnerPowerStageInfo(InnerPowerStage.ActionID.UpdateScore, client.Player.UID, stage));
                        //                client.Player.InnerPower.UpdateStatus();
                        //                client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                        //                Role.Instance.InnerPower.InnerPowerRank.UpdateRank(client.Player.InnerPower);
                        //            }
                        //        }
                        //    }
                        //}
                        break;
                    }
            }
        }
    }
}
