using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe void GetVipHandler(this ServerSockets.Packet stream, out MsgVipHandler.VIPTeleportTypes TeleportType, out MsgVipHandler.VIPTeleportLocations Locations,out uint Countdown
            , out string name)
        {
            TeleportType = (MsgVipHandler.VIPTeleportTypes)stream.ReadUInt32();
            Locations = (MsgVipHandler.VIPTeleportLocations)stream.ReadUInt32();
            Countdown = stream.ReadUInt32();

            byte size = stream.ReadUInt8();
            name = stream.ReadCString(size);
        }
        public static unsafe ServerSockets.Packet VipHandlerCreate(this ServerSockets.Packet stream,  MsgVipHandler.VIPTeleportTypes TeleportType,  MsgVipHandler.VIPTeleportLocations Locations,  uint Countdown
         ,  string name)
        {

            stream.InitWriter();

            stream.Write((uint)TeleportType);
            stream.Write((uint)Locations);
            stream.Write(Countdown);

            stream.Write(name.Length);
            stream.Write(name, name.Length);

            stream.Finalize(Game.GamePackets.MsgVipHandler);

            return stream;
        }
    }
    public unsafe struct  MsgVipHandler
    {
        public enum VIPTeleportTypes : uint
        {
            SelfTeleport = 0,
            TeamTeleport = 1,
            TeammateConfirmation = 2,
            TeammateTeleport = 3
        }
        public enum VIPTeleportLocations : uint
        {
            TwinCity = 1,
            PhoenixCastle = 2,
            ApeCity = 3,
            DesertCity = 4,
            BirdIland = 5,
            TCSquare = 6,
            WPFarm = 7,
            WPBridge = 8,
            WPAltar = 9,
            WPApparation = 10,
            WPPoltergiest = 11,
            WPTurtledove = 12,
            PCSqaure = 13,
            MFWaterCave = 14,
            MFVillage = 15,
            MFLake = 16,
            MFMineCave = 17,
            MFBridge = 18,
            MFToApeCity = 19,
            ACSquare = 20,
            ACSouth = 21,
            ACEast = 22,
            ACNorth = 23,
            ACWest = 24,
            BISquare = 25,
            BICenter = 26,
            BISouthWest = 27,
            BINorthWest = 28,
            BINorthEast = 29,
            DCSquare = 30,
            DCSouth = 31,
            DCVillage = 32,
            DCMoonSpring = 33,
            DCAncientMaze = 34
        }

        [PacketAttribute(GamePackets.MsgVipHandler)]
        public unsafe static void Handler(Client.GameClient user, ServerSockets.Packet packet)
        {

                VIPTeleportTypes TeleportType;
         VIPTeleportLocations Locations;
         uint Countdown;
         string name;


         packet.GetVipHandler(out TeleportType, out Locations, out Countdown, out name);
            if (user.PokerInfo != null)
                return;
            switch (TeleportType)
            {
                case VIPTeleportTypes.SelfTeleport:
                    {
                        if (Extensions.Time32.Now > user.LastVIPTeleport.AddMinutes(3))
                        {
                            Teleport(user, Locations);
                            user.LastVIPTeleport = Extensions.Time32.Now;
                        }
                        else
                        {
#if Arabic
                                    user.SendSysMesage("You have to wait " + (user.LastVIPTeleport.AddMinutes(3) - Extensions.Time32.Now).AllSeconds.ToString() + " more seconds to use the VIP Teleport.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                    
#else
                            user.SendSysMesage("You have to wait " + (user.LastVIPTeleport.AddMinutes(3) - Extensions.Time32.Now).AllSeconds.ToString() + " more seconds to use the VIP Teleport.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                    
#endif
                        }
                        break;
                    }
                case VIPTeleportTypes.TeamTeleport:
                    {
                        if (Extensions.Time32.Now > user.LastVIPTeamTeleport.AddMinutes(3))
                        {
                            if (user.Team != null)
                            {
                                packet = packet.VipHandlerCreate(VIPTeleportTypes.TeammateConfirmation, Locations, 15, user.Player.Name);

                                foreach (var member in user.Team.Temates)
                                {
                                    if (member.client.Player.UID != user.Player.UID)
                                    {
                                        member.client.Send(packet);
                                    }
                                }
                            }
                            Teleport(user, Locations);
                            user.LastVIPTeamTeleport = Extensions.Time32.Now;
                        }
                        else
                        {
#if Arabic
                                    user.SendSysMesage("You have to wait " + (user.LastVIPTeamTeleport.AddMinutes(3) - Extensions.Time32.Now).AllSeconds.ToString() + " more seconds to use the VIP Teleport.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                     
#else
                            user.SendSysMesage("You have to wait " + (user.LastVIPTeamTeleport.AddMinutes(3) - Extensions.Time32.Now).AllSeconds.ToString() + " more seconds to use the VIP Teleport.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                     
#endif
                       }
                        break;
                    }
                case VIPTeleportTypes.TeammateTeleport:
                    {
                        Teleport(user, Locations);
                        break;
                    }
            }
        }

        public static void Teleport(Client.GameClient user, VIPTeleportLocations Location)
        {
            switch (Location)
            {
                case VIPTeleportLocations.TwinCity:
                case VIPTeleportLocations.TCSquare: user.Teleport(1002, 428, 378); break;
                case VIPTeleportLocations.WPFarm: user.Teleport(1002, 378, 162); break;
                case VIPTeleportLocations.WPBridge: user.Teleport(1002, 598, 678); break;
                case VIPTeleportLocations.WPAltar: user.Teleport(1002, 564, 793); break;
                case VIPTeleportLocations.WPApparation: user.Teleport(1002, 340, 650); break;
                case VIPTeleportLocations.WPPoltergiest: user.Teleport(1002, 230, 388); break;
                case VIPTeleportLocations.WPTurtledove: user.Teleport(1002, 627, 474); break;

                case VIPTeleportLocations.PhoenixCastle:
                case VIPTeleportLocations.PCSqaure: user.Teleport(1011, 188, 264); break;
                case VIPTeleportLocations.MFWaterCave: user.Teleport(1011, 380, 31); break;
                case VIPTeleportLocations.MFVillage: user.Teleport(1011, 785, 472); break;
                case VIPTeleportLocations.MFLake: user.Teleport(1011, 369, 568); break;
                case VIPTeleportLocations.MFMineCave: user.Teleport(1011, 924, 560); break;
                case VIPTeleportLocations.MFBridge: user.Teleport(1011, 648, 567); break;
                case VIPTeleportLocations.MFToApeCity: user.Teleport(1011, 475, 841); break;

                case VIPTeleportLocations.ApeCity:
                case VIPTeleportLocations.ACSquare: user.Teleport(1020, 565, 562); break;
                case VIPTeleportLocations.ACSouth: user.Teleport(1020, 699, 640); break;
                case VIPTeleportLocations.ACEast: user.Teleport(1020, 624, 337); break;
                case VIPTeleportLocations.ACNorth: user.Teleport(1020, 200, 224); break;
                case VIPTeleportLocations.ACWest: user.Teleport(1020, 322, 621); break;

                case VIPTeleportLocations.DesertCity:
                case VIPTeleportLocations.DCSquare: user.Teleport(1000, 500, 650); break;
                case VIPTeleportLocations.DCSouth: user.Teleport(1000, 758, 750); break;
                case VIPTeleportLocations.DCVillage: user.Teleport(1000, 480, 271); break;
                case VIPTeleportLocations.DCMoonSpring: user.Teleport(1000, 291, 450); break;
                case VIPTeleportLocations.DCAncientMaze: user.Teleport(1000, 87, 321); break;

                case VIPTeleportLocations.BirdIland:
                case VIPTeleportLocations.BISquare: user.Teleport(1015, 717, 571); break;
                case VIPTeleportLocations.BICenter: user.Teleport(1015, 585, 593); break;
                case VIPTeleportLocations.BISouthWest: user.Teleport(1015, 562, 786); break;
                case VIPTeleportLocations.BINorthWest: user.Teleport(1015, 125, 323); break;
                case VIPTeleportLocations.BINorthEast: user.Teleport(1015, 125, 323); break;
            }
        }


    }
}
