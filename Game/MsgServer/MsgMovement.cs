using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using LightConquer_Project.Game.MsgFloorItem;
using System.IO;
using LightConquer_Project.ServerSockets;
using ProtoBuf;
using Extensions;

namespace LightConquer_Project.Game.MsgServer
{
    [ProtoContract]
    public class WalkQuery
    {
        [ProtoMember(1, IsRequired = true)]
        public uint Direction;
        [ProtoMember(2, IsRequired = true)]
        public uint UID;
        [ProtoMember(3, IsRequired = true)]
        public uint Running;
        [ProtoMember(4, IsRequired = true)]
        public uint TimeStamp;
    }
    public static unsafe class MsgMovement
    {
        public const uint Walk = 0, Run = 1, Steed = 9;


        public static sbyte[] DeltaMountX = new sbyte[24] { 0, -2, -2, -2, 0, 2, 2, 2, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1 };
        public static sbyte[] DeltaMountY = new sbyte[24] { 2, 2, 0, -2, -2, -2, 0, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2 };

        
        public static unsafe void GetWalk(this ServerSockets.Packet stream, out WalkQuery pQuery)
        {
          pQuery = new WalkQuery();
          pQuery=  stream.ProtoBufferDeserialize<WalkQuery>(pQuery);
        }

        public static unsafe ServerSockets.Packet MovementCreate(this ServerSockets.Packet stream, WalkQuery pQuery)
        {
            stream.InitWriter();
            pQuery.TimeStamp = (uint)Extensions.Time32.Now.Value;
            stream.ProtoBufferSerialize(pQuery);
        
            stream.Finalize(GamePackets.Movement);

            return stream;
        }

        public static uint Bodyyyy = 0;
        public static uint UIDDDD = 1000000;
        public static int eeffect = 1;
        public static int LastClientStamp = 0;
        [PacketAttribute(GamePackets.Movement)]
        public unsafe static void Movement(Client.GameClient client, ServerSockets.Packet packet)
        {
            if (client.Player.Away == 1)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var apacket = rec.GetStream();
                    client.Player.Away = 0;
                    client.Player.View.SendView(client.Player.GetArray(apacket, false), false);
                }
            }
            Role.Flags.ConquerAngle dir;

            WalkQuery walkPacket;

            packet.GetWalk(out walkPacket);
            walkPacket.UID = client.Player.UID;


            client.Player.Action = Role.Flags.ConquerAction.None;

            client.OnAutoAttack = false;
            client.Player.RemoveBuffersMovements(packet);

            client.Player.Protect = Extensions.Time32.Now;


            if (walkPacket.Running == MsgMovement.Steed)
            {

                dir = (Role.Flags.ConquerAngle)(walkPacket.Direction % 24);

                client.Player.View.SendView(packet.MovementCreate(walkPacket), true);


                int newX = client.Player.X + DeltaMountX[(byte)dir];
                int newY = client.Player.Y + DeltaMountY[(byte)dir];
#if TEST
                MyConsole.WriteLine("Steed walk direction -> " + dir.ToString() + " " + (byte)dir + ", X " + newX + " Y " + newY + "");
#endif
                if (client.Map == null)
                {
                    client.Teleport(310, 288, 1002);
                    return;
                }
                if (client.Player.Map == 1038)
                {
                    if (!Game.MsgTournaments.MsgSchedules.GuildWar.ValidWalk(client.TerainMask, out client.TerainMask, client.Player.X, client.Player.Y))
                    {
#if Arabic
                          client.SendSysMesage("Illegal jumping over the gates detected.");
#else
                        client.SendSysMesage("Illegal jumping over the gates detected.");
#endif

                        client.Pullback();
                        return;
                    }
                }

                client.Map.View.MoveTo<Role.IMapObj>(client.Player, newX, newY);
                client.Player.X = (ushort)newX;
                client.Player.Y = (ushort)newY;

                client.Player.Action = Role.Flags.ConquerAction.None;
                client.Player.View.Role(false, packet.MovementCreate(walkPacket));

                if (client.Vigor >= 2)
                    client.Vigor -= 2;
                else
                    client.Vigor = 0;

                client.Send(packet.ServerInfoCreate(MsgServerInfo.Action.Vigor, client.Vigor));


            }
            else
            {
                ushort walkX = client.Player.X, walkY = client.Player.Y;
                dir = (Role.Flags.ConquerAngle)(walkPacket.Direction % 8);
                Role.Core.IncXY(dir, ref walkX, ref walkY);


#if TEST
                MyConsole.WriteLine("walk direction -> " + dir.ToString() + " " + (byte)dir + ", X " + walkX + " Y " + walkY + "");
#endif
                if (client.Map == null)
                {
                    client.Teleport(310, 288, 1002);
                    return;
                }
                if (client.Player.Map == 1038)
                {
                    if (!Game.MsgTournaments.MsgSchedules.GuildWar.ValidWalk(client.TerainMask, out client.TerainMask, walkX, walkY))
                    {
                        client.SendSysMesage("Illegal jumping over the gates detected.");
                        client.Pullback();
                        return;
                    }
                }

                if (client.Player.ObjInteraction != null)
                {
                    if (client.Player.ObjInteraction.Player.X == client.Player.X && client.Player.ObjInteraction.Player.Y == client.Player.Y)
                    {

                        InterActionWalk query = new InterActionWalk()
                        {
                            Mode = MsgInterAction.Action.Walk,
                            UID = client.Player.UID,
                            OponentUID = client.Player.ObjInteraction.Player.UID,
                            DirectionOne = (byte)dir
                        };

                        client.Player.View.SendView(packet.InterActionWalk(&query), true);

                        client.Map.View.MoveTo<Role.IMapObj>(client.Player, walkX, walkY);
                        client.Player.X = walkX;
                        client.Player.Y = walkY;
                        client.Player.Angle = dir;

                        client.Player.View.Role(false, packet.InterActionWalk(&query));

                        client.Map.View.MoveTo<Role.IMapObj>(client.Player.ObjInteraction.Player, walkX, walkY);
                        client.Player.ObjInteraction.Player.X = walkX;
                        client.Player.ObjInteraction.Player.Y = walkY;
                        client.Player.ObjInteraction.Player.Angle = dir;

                        client.Player.ObjInteraction.Player.View.Role();
                        return;
                    }
                }
                client.Player.View.SendView(packet.MovementCreate(walkPacket), true);
                client.Map.View.MoveTo<Role.IMapObj>(client.Player, walkX, walkY);
                client.Player.X = walkX;
                client.Player.Y = walkY;
                client.Player.Angle = dir;

                client.Player.View.Role(false, packet.MovementCreate(walkPacket));
            }
            if (MsgTournaments.MsgSchedules.CaptureTheFlag != null)
                MsgTournaments.MsgSchedules.CaptureTheFlag.ChechMoveFlag(client);
           
            if (client.Player.ActivePick)
                client.Player.RemovePick(packet);







        }
    }
}
