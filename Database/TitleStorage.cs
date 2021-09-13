using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Database
{
    public class TitleStorage
    {

        public uint ID = 0;
        public uint SubID = 0;
        public string Name = "";
        public uint Score = 0;

        public static Dictionary<uint, TitleStorage> Titles = new Dictionary<uint, TitleStorage>();

        public static void LoadDBInformation()
        {

            string[] baseText = System.IO.File.ReadAllLines(Program.ServerConfig.DbLocation + "title_type.txt");
            foreach (var bas_line in baseText)
            {
                var line = bas_line.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
                TitleStorage title = new TitleStorage();
                title.ID = uint.Parse(line[0]);
                title.SubID = uint.Parse(line[1]);
                title.Name = line[2];
                title.Score = uint.Parse(line[7]);
                Titles.Add(title.ID, title);
            }
        }

        public static void CheckUpUser(Client.GameClient user, ServerSockets.Packet stream)
        {
            //#region Donate title & wing
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.PokerKing))
            //{
            //    if (user.Player.DonatePoints >= 100)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.PokerKing, stream);
            //    }                
            //}
            //else if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.PokerKing))
            //{
            //    if (user.Player.DonatePoints < 100)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.PokerKing, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SirenSong))
            //{
            //    if (user.Player.DonatePoints >= 100)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SirenSong, stream);
            //    }
            //}
            //else if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SirenSong))
            //{
            //    if (user.Player.DonatePoints < 100)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SirenSong, stream);
            //    }
            //}
            //#endregion
            //#region VIPTitles
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SkyKnight))
            //{
            //    if (user.Player.VipLevel >= 1 && user.Player.VipLevel <= 4)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SkyKnight, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SkyKnight))
            //{
            //    if (user.Player.VipLevel < 1 || user.Player.VipLevel > 4)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SkyKnight, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.GloryKnight))
            //{
            //    if (user.Player.VipLevel == 5 || user.Player.VipLevel == 6)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.GloryKnight, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.GloryKnight))
            //{
            //    if (user.Player.VipLevel < 5 || user.Player.VipLevel > 6)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.GloryKnight, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Paladin))
            //{
            //    if (user.Player.VipLevel == 7)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Paladin, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Paladin))
            //{
            //    if (user.Player.VipLevel != 7)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Paladin, stream);
            //    }
            //}
            //#endregion
            //#region SweetFairy
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SweetFairy))
            //{
            //    if (user.MyWardrobe.Contain(194875) && user.MyWardrobe.Contain(200613) && user.MyWardrobe.Contain(200614) && user.Player.Body / 1000 == 2)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SweetFairy, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SweetFairy))
            //{
            //    if (!user.MyWardrobe.Contain(194875) || !user.MyWardrobe.Contain(200613) || !user.MyWardrobe.Contain(200614) || user.Player.Body / 1000 != 2)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SweetFairy, stream);
            //    }
            //}
            //#endregion
            //#region LovesMadness
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.LovesMadness))
            //{
            //    if (user.MyWardrobe.Contain(194875) && user.MyWardrobe.Contain(200613) && user.MyWardrobe.Contain(200614) && user.Player.Body / 1000 == 1)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.LovesMadness, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.LovesMadness))
            //{
            //    if (!user.MyWardrobe.Contain(194875) || !user.MyWardrobe.Contain(200613) || !user.MyWardrobe.Contain(200614) || user.Player.Body / 1000 != 1)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.LovesMadness, stream);
            //    }
            //}
            //#endregion
            //#region Monkeys         
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.MonkeyRider))
            //{
            //    if (user.MyWardrobe.CollectedRandomMonkey())
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.MonkeyRider, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.MonkeyRider))
            //{
            //    if (!user.MyWardrobe.CollectedRandomMonkey())
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.MonkeyRider, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SaintRider))
            //{
            //    if (user.MyWardrobe.CollectedAllMonkeyTypes())
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SaintRider, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SaintRider))
            //{
            //    if (!user.MyWardrobe.CollectedAllMonkeyTypes())
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SaintRider, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SolarRider))
            //{
            //    if (user.MyWardrobe.CollectedSolarMonkey())
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SolarRider, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SolarRider))
            //{
            //    if (!user.MyWardrobe.CollectedSolarMonkey())
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SolarRider, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.LunarRider))
            //{
            //    if (user.MyWardrobe.CollectedLunarMonkey())
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.LunarRider, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.LunarRider))
            //{
            //    if (!user.MyWardrobe.CollectedLunarMonkey())
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.LunarRider, stream);
            //    }
            //}
            //#endregion
            //#region Prestige

            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.VioletCloudWing))
            //{
            //    if (user.PrestigeLevel >= 324)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.VioletCloudWing, stream);
            //    }
            //}
            //else if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.VioletCloudWing))
            //{
            //    if (user.PrestigeLevel < 324)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.VioletCloudWing, stream);
            //    }
            //}

            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.VioletLightning))
            //{
            //    if (user.PrestigeLevel >= 216)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.VioletLightning, stream);
            //    }
            //}
            //else if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.VioletLightning))
            //{
            //    if (user.PrestigeLevel < 216)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.VioletLightning, stream);
            //    }
            //}

            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Superman))
            //{
            //    if (user.MyPrestigePoints >= 600000)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Superman, stream);
            //    }
            //}
            //else if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Superman))
            //{
            //    if (user.MyPrestigePoints < 600000)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Superman, stream);
            //    }
            //}

            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Scholar))
            //{
            //    if (user.MyPrestigePoints >= 400000)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Scholar, stream);
            //    }
            //}
            //else if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Scholar))
            //{
            //    if (user.MyPrestigePoints < 400000)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Scholar, stream);
            //    }
            //}
            //#endregion
            //#region Achievement
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.RisingStar))
            //{
            //    if (user.Player.Achievement.Score() >= 300)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.RisingStar, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.RisingStar))
            //{
            //    if (user.Player.Achievement.Score() < 300)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.RisingStar, stream);
            //    }
            //}
            //#endregion
            //#region CTF
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Victor))
            //{
            //    if (user.Player.MyGuild != null && user.Player.MyGuild.CTF_Rank == 1 && user.Player.MyGuildMember != null && user.Player.MyGuildMember.Rank == Role.Flags.GuildMemberRank.GuildLeader)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.RadiantWings, stream);
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Victor, stream);
            //    }
            //}
            //else
            //{
            //    if (!(user.Player.MyGuild != null && user.Player.MyGuild.CTF_Rank == 1 && user.Player.MyGuildMember != null && user.Player.MyGuildMember.Rank == Role.Flags.GuildMemberRank.GuildLeader))
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.RadiantWings, stream);
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Victor, stream);
            //    }
            //}
            //#endregion
            //#region GuildWar
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Conqueror))
            //{
            //    if (user.Player.MyGuild != null && user.Player.MyGuildMember != null && user.Player.MyGuildMember.Rank == Role.Flags.GuildMemberRank.GuildLeader)
            //    {
            //        if (Game.MsgTournaments.MsgSchedules.GuildWar.Winner != null && Game.MsgTournaments.MsgSchedules.GuildWar.Winner.GuildID == user.Player.GuildID && Game.MsgTournaments.MsgSchedules.GuildWar.Proces == Game.MsgTournaments.ProcesType.Dead)
            //            user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Conqueror, stream);
            //    }
            //}
            //else
            //{
            //    if (!(user.Player.MyGuild != null && user.Player.MyGuildMember != null && user.Player.MyGuildMember.Rank == Role.Flags.GuildMemberRank.GuildLeader))
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Conqueror, stream);
            //    if (!(Game.MsgTournaments.MsgSchedules.GuildWar.Winner != null && Game.MsgTournaments.MsgSchedules.GuildWar.Winner.GuildID == user.Player.GuildID
            //        && Game.MsgTournaments.MsgSchedules.GuildWar.Proces == Game.MsgTournaments.ProcesType.Dead))
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Conqueror, stream);
            //}
            //#endregion
            //#region JiangHu
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Talent))
            //{
            //    if (user.Player.MyJiangHu != null && user.Player.MyJiangHu.Inner_Strength >= 80000)
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Talent, stream);
            //}
            //else
            //{
            //    if (!(user.Player.MyJiangHu != null && user.Player.MyJiangHu.Inner_Strength < 80000))
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Talent, stream);
            //}
            //#endregion
            //#region Chi
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Grandmaster))
            //{
            //    if (user.Player.MyChi != null && user.Player.MyChi.AllScore() >= 1500)
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Grandmaster, stream);
            //}
            //else
            //{
            //    if (!(user.Player.MyChi != null && user.Player.MyChi.AllScore() < 1500))
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Grandmaster, stream);
            //}
            //#endregion
            //#region EliteGroups
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.WingsofInfernal))
            //{
            //    if (Game.MsgTournaments.MsgEliteTournament.EliteGroups[3] != null)
            //    {
            //        if (Game.MsgTournaments.MsgEliteTournament.EliteGroups[3].Top8 != null
            //            && Game.MsgTournaments.MsgEliteTournament.EliteGroups[3].Top8.Length > 0
            //            && Game.MsgTournaments.MsgEliteTournament.EliteGroups[3].Top8[0].UID == user.Player.UID)
            //        {
            //            user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.WingsofInfernal, stream);
            //        }
            //    }
            //}
            //else
            //{
            //    if (Game.MsgTournaments.MsgEliteTournament.EliteGroups[3] != null)
            //    {
            //        if (!(Game.MsgTournaments.MsgEliteTournament.EliteGroups[3].Top8 != null
            //            && Game.MsgTournaments.MsgEliteTournament.EliteGroups[3].Top8.Length > 0
            //            && Game.MsgTournaments.MsgEliteTournament.EliteGroups[3].Top8[0].UID == user.Player.UID))
            //        {
            //            user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.WingsofInfernal, stream);
            //        }
            //    }
            //}
            //#endregion
            //#region CoatStorage
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Fashionist))
            //{
            //    if (CoatStorage.AmountStarGarments(user, 4) >= 5)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Fashionist, stream);
            //    }
            //}
            //else if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Fashionist))
            //{
            //    if (CoatStorage.AmountStarGarments(user, 4) < 5)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Fashionist, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SwiftChaser))
            //{
            //    if (CoatStorage.AmountStarMount(user, 4) >= 5)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SwiftChaser, stream);
            //    }
            //}
            //else if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.SwiftChaser))
            //{
            //    if (CoatStorage.AmountStarMount(user, 4) < 5)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.SwiftChaser, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.StarlightWings))
            //{
            //    if (CoatStorage.AmountStarGarments(user, 5) >= 1)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.StarlightWings, stream);
            //    }
            //}
            //else if(user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.StarlightWings))
            //{
            //    if (CoatStorage.AmountStarGarments(user, 5) < 1)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.StarlightWings, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.MoonlightWings))
            //{
            //    if (CoatStorage.AmountStarMount(user, 5) >= 1)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.MoonlightWings, stream);
            //    }
            //}
            //else if(user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.MoonlightWings))
            //{
            //    if (CoatStorage.AmountStarMount(user, 5) < 1)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.MoonlightWings, stream);
            //    }
            //}
            //#endregion            
            //#region Flower Ranks
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.FairyWings))
            //{
            //    if (user.Player.FlowerRank == 1)
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.FairyWings, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.FairyWings))
            //{
            //    if (user.Player.FlowerRank != 300)
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.FairyWings, stream);
            //    }
            //}
            //#endregion
            //#region ButterflyRose Gaerment
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.WingsofRomance))
            //{
            //    if (user.MyWardrobe.Contain(193625))
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.WingsofRomance, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.WingsofRomance))
            //{
            //    if (!user.MyWardrobe.Contain(193625))
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.WingsofRomance, stream);
            //    }
            //}
            //#endregion
            //#region GoldPrize Gaerment
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Supreme))
            //{
            //    if (user.MyWardrobe.Contain(2100075))
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Supreme, stream);
            //    }
            //}
            //if (user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Supreme))
            //{
            //    if (!user.MyWardrobe.Contain(2100075))
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Supreme, stream);
            //    }
            //}
            //#endregion
            //#region Overlord && WingsofSolarDra
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Overlord) && !user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.WingsofSolarDra))
            //{
            //    if (user.MyWardrobe.Contain(193275) && user.MyWardrobe.Contain(200531))
            //    {
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Overlord, stream);
            //        user.Player.AddSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.WingsofSolarDra, stream);
            //    }
            //}
            //if (!user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.Overlord) && !user.Player.SpecialTitles.Contains(Game.MsgServer.MsgTitleStorage.TitleType.WingsofSolarDra))
            //{
            //    if (user.MyWardrobe.Contain(193275) && user.MyWardrobe.Contain(200531))
            //    {
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.Overlord, stream);
            //        user.Player.RemoveSpecialTitle(Game.MsgServer.MsgTitleStorage.TitleType.WingsofSolarDra, stream);
            //    }
            //}
            //#endregion
        }
    }
}
