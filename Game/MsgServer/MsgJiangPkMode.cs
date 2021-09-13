using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static void GetJiangPkMode(this ServerSockets.Packet stream, out  Role.Instance.JiangHu.AttackFlag Flag)
        {
            Flag = (Role.Instance.JiangHu.AttackFlag)stream.ReadUInt32();
        }

    }
    public class MsgJiangPkMode
    {
        [PacketAttribute(GamePackets.JiangHuPkMode)]
        private static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
#if Jiang
            Role.Instance.JiangHu.AttackFlag Flag;

            stream.GetJiangPkMode(out Flag);

            user.Player.JiangPkFlag = Flag;
#endif
        }
    }
}
