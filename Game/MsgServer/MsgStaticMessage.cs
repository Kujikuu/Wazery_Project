using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe void GetStaticMessage(this ServerSockets.Packet stream, out uint Accept)
        {
            Accept = stream.ReadUInt32();
        }

        public static unsafe ServerSockets.Packet StaticMessageCreate(this ServerSockets.Packet stream,MsgStaticMessage.Messages Message
            , MsgStaticMessage.Action Mode, uint Seconds)
        {
            stream.InitWriter();

            stream.Write((uint)0);
            stream.Write((uint)Message);
            stream.Write((uint)Mode);
            stream.Write(Seconds);


            stream.Finalize(GamePackets.MsgStaticMessage);

            return stream;
        }


    }

    public unsafe struct MsgStaticMessage
    {
        public enum Messages : uint
        {
            None = 0,
            GuildWar = 10515,//Guild War is about to begin! Will you join it?
            ClassPk = 10519,//Class PK War is about to begin! Will you join it?
            WeeklyPKWar = 10521,//Weekly PK War is about to begin! Will you join it?
            MonthlyPk = 10523,//Monthly PK War is about to begin! Will you join it?
            HorseRace = 10525,//The Horse Racing has started! Do you want to join in? Click OK and enter the Horse Racing map now!
            MonthlyPK = 10527,//The Monthly PK War will start soon. Would you like to sign up now?
            WeeklyPK = 10529,//The Weekly PK War will start soon. Would you like to sign up now?
            PowerArena = 10531,//The Power Arena Challenge will start soon. Would you like to sign up now?
            ElitePKTournament = 10533,//Today`s Elite PK Tournament begins at 20:00. Get yourself prepared for it!
            CapturetheFlag = 10535,//Capture the Flag is about to start! Do you want to participate?
            GuildContest = 10537,//The Guild Contest is about to begin! Do you want to sign up for it?
            SkillTeamPKTournament = 10541,//The Skill Team PK Tournament will start at 20:00. Prepare yourself and sign up for it as a team!
            TeamPKTournament = 10543,//The Skill Team PK Tournament will start at 20:00. Prepare yourself and sign up for it as a team!
            CashRain = 10545,//The Cash Rain will start falling in 5 minutes. Do you want to grab some Cash Chests full of CPs? Click ok to enter the Blessed Land.

            PassTheBomb = 10554, // Pass The Bomb is about to begin! Do you want to sign up for it?
            KOTH = 10555,//The FootBall Tournament is about to kick-off. Join now?
            TreasureThief = 10556,//The TreasureThief Tournament is about to kick-off. Join now?
            FreezeWar = 10557,//The RushTime Tournament is about to kick-off. Join now?
            TDM = 10558,
            LastManStand = 10559,//The LastManStand PK Tournament is about to kick-off. Join now?
            skillmaster = 10560,
            SSFBTournament = 10561,//The SoulShackle Tournament is about to kick-off. Join now?
            Infection = 10562,//The ExtremePkTournament is about to kick-off. Join now?
            KillTheCaptain = 10563,//The TeamDeathMatch Tournament is about to kick-off. Join now?
            FiveNOut = 10564,//The FiveNOut Tournament is about to kick-off. Join now?
            FFa = 10565,//The Confused Tournament is about to kick-off. Join now?
            DragonWar = 10566,//The DragonWar Tournament is about to kick-off. Join now?
            TDMFour = 10567,
            LadderTournament = 10568,
            SkillChampionship = 10569,
            Spacelnvasion = 10570,
            VampireWar = 10571,
            WhackTheThief = 10572,
            Couple = 10573,//The Couple Tournament is about to kick-off. Join now?

        }
        public enum Action : uint
        {
            Append = 6
        }
        [PacketAttribute(GamePackets.MsgStaticMessage)]
        public static void Handler(Client.GameClient user, ServerSockets.Packet stream)
        {
            uint Accept;
            stream.GetStaticMessage(out Accept);

            if (Accept == 1)
            {
                if (Program.BlockTeleportMap.Contains(user.Player.Map))
                    return;
                if (user.Player.StartMessageBox > Extensions.Time32.Now)
                {
                    if (user.Player.MessageOK != null)
                        user.Player.MessageOK.Invoke(user);
                    else if (user.Player.MessageCancel != null)
                        user.Player.MessageCancel.Invoke(user);
                }
                user.Player.MessageOK = null;
                user.Player.MessageCancel = null;
            }
        }
    }
}
