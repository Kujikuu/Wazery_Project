using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using LightConquer_Project.Game.MsgServer;
using LightConquer_Project.Game.MsgNpc;

namespace LightConquer_Project.Role
{
    public unsafe class Player : IMapObj
    {
        public Extensions.Time32 LoginStamp = new Extensions.Time32();
        public ulong Lastthread;
        public ulong thread_time;
        public DateTime LastNewbieUsed = new DateTime();
        public byte CPBoundPack = 0;
        public bool ClaimedBCPToday = false;
        public bool Mining = false;
        public Extensions.Time32 NextMine;
        public uint MiningAttempts = 140;
        public void StopMining()
        {
            Mining = false;
        }
        public DateTime LeftDoorStamp, RightDoorStamp;
        public byte AukKilled = 0;
        public byte FateFruitUsed = 0;
        public uint ExchangeNormalAvaliability = 0, ExchangehighAvaliability = 0, AnswerdToday = 0;
        public int[] WrongAnswers = new int[5] { 0, 0, 0, 0, 0 };
        public byte nextquizop = 0, currentquizop = 0;
        public bool JustCreated = false;
        public uint DragonIslandDamage = 0;
        public uint GiantGhasomKill = 0;
        public bool GiantGhasomKillClaimed = false;

        public enum RechargeType : ulong
        {
            None = 0,
            OneGarment = 1u << 0,
            TwoVipTokens = 1u << 1,
            OneMount = 1u << 2,
            InnerPower1000 = 1u << 3,
            FourVipTokens = 1u << 4,
            ConquerPoints225 = 1u << 5,
            InnerPower1500 = 1u<< 6,
            InnerPower5000 = 1u << 7,
            OneMount2 = 1u << 8,
            OneRareAccesory = 1u << 9,
            OneGerment2 = 1u << 10,
            InnerPower5000_2 = 1u << 11,
            ConquerPoints500 = 1u << 12,
            OneSoulP7 = 1u << 13,
            OneRareMaterial = 1u << 14,
            TenVip6Tokens = 1u << 15,
            OneRareAccesory_2 = 1u << 16,
            InnerPower5000_3 = 1u << 17,
            OneMount3 = 1u << 18,
            GoldPrize = 1u << 19,
            InnerPower10000 = 1u << 20
        }
        
        public LightConquer_Project.Role.Instance.Chi.ChiAttributeType selectedAttribute;
        public string SelectedGate = "";
        public int SelectedPhase = 0;
        public Client.GameClient AttackerScarofEarthl;
        public Database.MagicType.Magic ScarofEarthl;

        public Extensions.Time32 UseLayTrap = new Extensions.Time32();

        public Extensions.Time32 EarthStamp = new Extensions.Time32();
        public DateTime CanChangeWindWalkerFree = DateTime.Now;
        public Extensions.Time32 FanRecoverStamin = new Extensions.Time32();
        public Extensions.Time32 HuntingBounsStamp = new Extensions.Time32();

        public int RevengeTailChange = 0;

        public byte UseChiToken = 0;
        public uint HuntingBounsLayer;
        public DateTime MerchantApplicationEnd;
        private uint _merchant = 0;
        public uint Merchant
        {
            get
            {
                return _merchant;
            }
            set
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    if (value == byte.MaxValue)
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                        stream = packet.Append(stream, MsgUpdate.DataType.Merchant, value);
                        stream = packet.GetArray(stream);
                        Owner.Send(stream);
                    }
                }
                _merchant = value;
            }
        }
        public bool OnRemoveLukyAmulet = false;
        public bool OnBluedBird = false;
        public DateTime BlueBirdPlumeStamp = new DateTime();

        public bool OnFerentPill { get { return ContainFlag(MsgUpdate.Flags.Poisoned); } }
        public DateTime FerventPill = new DateTime();

        public byte LuiseQuestions = 0;
        public DateTime RealPortraitStamp = new DateTime();
        public DateTime RevealVialStamp = new DateTime();
        public DateTime LastDragonPill;
        public DateTime EventQuests;
        public DateTime vote;
        public DateTime LastSwordSoul;

        public bool IsBoy()
        {
            return Role.Core.IsBoy(Body);
        }
        public bool IsGirl()
        {
            return Role.Core.IsGirl(Body);
        }
        public int GiveFlowersToPerformer = 0;

        public DateTime GallbladerrStamp = new DateTime();

        public byte MonkMiseryTransforming = 0;

        public uint[] EpicTrojanItemChange = new uint[2];
        public byte StageEpicTrojanQuest = 0;
        public uint ChangeEpicTrojan = 0;
        public byte CanChangeEpicMaterial = 1;

        public uint EpicTrojanEvilArrayPoints = 0;
        public uint EpicTrojanAbysalStage = 0;

        public uint ChangeArrayEpicTrojan = 0;
        public byte CanChangeArrayEpicMaterial = 1;
        public uint[] EpicTrojanArrayItemChange = new uint[2];

        public byte EpicTrijanKillGhostReaver = 0;
        public uint[] EpicTrojanMr_MirrorItemChange = new uint[2];
        public uint ChangeMr_MirrorEpicTrojan = 0;
        public byte CanChangeMr_MirrorEpicMaterial = 1;


        public uint[] EpicTrojanGeneralPakItemChange = new uint[2];
        public uint ChangeGeneralPakEpicTrojan = 0;
        public byte CanChangGeneralPakMaterial = 1;

        public byte EpicTrojanMrMirrorPrograss = 0;

        public uint FiveNOutLifes = 0;
        internal int SoulShackle = 0;

        public void ResetEpicTrojan()
        {
            EpicTrojanMrMirrorPrograss = 0;
            StageEpicTrojanQuest = 0;
            EpicTrojanEvilArrayPoints = 0;
            EpicTrojanAbysalStage = 0;
            EpicTrijanKillGhostReaver = 0;
          ChangeEpicTrojan = ChangeArrayEpicTrojan = ChangeMr_MirrorEpicTrojan =ChangeGeneralPakEpicTrojan = 0;
           CanChangeEpicMaterial = CanChangeArrayEpicMaterial =CanChangeMr_MirrorEpicMaterial = CanChangGeneralPakMaterial = 1;
        }
        public string SaveEpicTrojan()
        {
            Database.DBActions.WriteLine writer = new Database.DBActions.WriteLine('/');
            writer.Add(EpicTrojanItemChange[0]).Add(EpicTrojanItemChange[1]).Add(StageEpicTrojanQuest)
                .Add(ChangeEpicTrojan).Add(CanChangeEpicMaterial).Add(CanChangeArrayEpicMaterial)
                .Add(ChangeArrayEpicTrojan).Add(EpicTrojanArrayItemChange[0]).Add(EpicTrojanArrayItemChange[1])
                .Add(EpicTrojanEvilArrayPoints).Add(EpicTrojanAbysalStage).Add(EpicTrojanMr_MirrorItemChange[0])
                .Add(EpicTrojanMr_MirrorItemChange[1]).Add(ChangeMr_MirrorEpicTrojan)
                .Add(CanChangeMr_MirrorEpicMaterial)

                .Add(EpicTrojanGeneralPakItemChange[0]).Add(EpicTrojanGeneralPakItemChange[1])
                .Add(ChangeGeneralPakEpicTrojan).Add(CanChangGeneralPakMaterial)
                .Add(EpicTrojanMrMirrorPrograss);
            return writer.Close();
        }
        public void LoadEpicTrojan(string line)
        {
            if (line == null)
                return;
            Database.DBActions.ReadLine reader = new Database.DBActions.ReadLine(line, '/');
            EpicTrojanItemChange[0] = reader.Read((uint)0);
            EpicTrojanItemChange[1] = reader.Read((uint)0);
            StageEpicTrojanQuest = reader.Read((byte)0);
            ChangeEpicTrojan = reader.Read((uint)0);
            CanChangeEpicMaterial = reader.Read((byte)0);
            CanChangeArrayEpicMaterial = reader.Read((byte)0);
            ChangeArrayEpicTrojan = reader.Read((uint)0);
            EpicTrojanArrayItemChange[0] = reader.Read((uint)0);
            EpicTrojanArrayItemChange[1] = reader.Read((uint)0);
            EpicTrojanEvilArrayPoints = reader.Read((uint)0);
            EpicTrojanAbysalStage = reader.Read((uint)0);
            EpicTrojanMr_MirrorItemChange[0] = reader.Read((uint)0);
            EpicTrojanMr_MirrorItemChange[1] = reader.Read((uint)0);
            ChangeMr_MirrorEpicTrojan = reader.Read((uint)0);
            CanChangeMr_MirrorEpicMaterial = reader.Read((byte)0);

            EpicTrojanGeneralPakItemChange[0] = reader.Read((uint)0);
            EpicTrojanGeneralPakItemChange[1] = reader.Read((uint)0);
            ChangeGeneralPakEpicTrojan = reader.Read((uint)0);
            CanChangGeneralPakMaterial = reader.Read((byte)0);
            EpicTrojanMrMirrorPrograss = reader.Read((byte)0);
        }

        public int DefeatedArenaGuardians = 0;

        public DateTime JoinPowerArenaStamp = new DateTime();
       

        public DateTime JoinPrizeNpcOctopus = new DateTime();

        public bool TOM_StartChallenge = false;
        public bool TOM_FinishChallenge = false;

        public byte ClaimTowerAmulets = 0;//reset
        public DateTime TowerAmuletStamp = new DateTime();


        public byte TOMClaimTeamReward = 0;//reset
        public DateTime TowerOfMysteryFrezeeStamp = new DateTime();
        public byte JoinTowerOfMysteryLayer = 0;
        public byte MyTowerOfMysteryLayer = 0;//save
        public byte MyTowerOfMysteryLayerElite = 0;//save
        public byte TowerOfMysterychallenge = 3;//reset to 3
        public bool CanSweapTowerOfMystery = false;

        public uint TowerOfMysteryChallengeFlag = 0;//reset
        public byte TOMSelectChallengeToday = 0;//reset
        public byte TOMChallengeToday = 0;//reset
        public uint TOMRefreshReward = 0;//reset
        public Game.MsgTournaments.MsgTowerOfMystery.RewardTypes TOM_Reward = Game.MsgTournaments.MsgTowerOfMystery.RewardTypes.Normal;

        public byte TOM_SellectOptionC = 0;

        public byte OpenHousePack = 0;

        public void InitializeTransfer(uint ServerID)
        {
            if (ConquerPoints > 200000)
            {
                var server = Database.GroupServerList.GetServer(ServerID);
                TransferToServer = server.Name;
                CheckTransfer = true;
                MsgInterServer.PipeClient.Connect(Owner, server.IPAddress, server.Port);
                Owner.CreateBoxDialog("We're preparing your transfer , please stand by ...");
            }
            else
                Owner.CreateBoxDialog("You need to have 200k CPs for transfer ! ");
        }
        public string TransferToServer = "";
        
        public uint InitTransfer = 0;
        public bool CheckTransfer = false;
        public bool OnTransfer = false;

        public ushort ExtraAtributes = 0;
        public void AddExtraAtributes(ServerSockets.Packet stream, ushort value)
        {

                ExtraAtributes += value;
                Atributes += value;
                SendUpdate(stream, Atributes, MsgUpdate.DataType.Atributes);
        }

        public uint BuyItemS = 0;

        public RechargeType RechargeProgress = RechargeType.None;
        public bool ContainRechargeType(RechargeType flag)
        {
            return (RechargeProgress & flag) == flag;
        }
        public bool AddRechargeFlag(RechargeType flag)
        {
            if (!ContainRechargeType(flag))
            {
                RechargeProgress |= flag;

                Database.RechargeShop.UpdateRecharge(Owner);

                return true;
            }
            return false;
        }
        public uint RechargePoints = 0;

        public byte BuyKingdomDeeds = 0;
        public uint KingDomDeeds = 0;

        
        public void UpdateKingdomTreasury(uint points)
        {
            if (InUnion)
            {
                MyUnion.Treasury += points;
                UnionMemeber.MyTreasury += points;
            }
        }

        public bool OnMyOwnServer
        {
            get { return ServerID == Database.GroupServerList.MyServerInfo.ID; }
        }
     
        public ushort ServerID = 0;

        public ushort SetLocationType = 0;
        public bool InUnion
        {
            get { return MyUnion != null && UnionMemeber != null; }
        }
        public bool IsUnionEmperror
        {
            get { return MyUnion != null && UnionMemeber != null && UnionMemeber.Rank == Instance.Union.Member.MilitaryRanks.Emperor; }
        }
        public Role.Instance.Union MyUnion = null;
        public Role.Instance.Union.Member UnionMemeber = null;

        public DateTime SickleStamp2 = new DateTime();

        public DateTime StampJump = new DateTime();
        public int StampJumpMiliSeconds = 0;
        public Extensions.Time32 ManiacDanceStamp = new Extensions.Time32();

        public DateTime KingOfTheHillStamp = new DateTime();
        public uint KingOfTheHillScore = 0;
        public uint SkillTournamentLifes = 0;

        public const ushort MaxInventorySashCount = 300;
        public ushort InventorySashCount = 0;
        public bool Invisible = false;
        public void UpdateInventorySash(ServerSockets.Packet stream)
        {
            SendUpdate(stream, MaxInventorySashCount, MsgUpdate.DataType.InventorySashMax);
            SendUpdate(stream, InventorySashCount , MsgUpdate.DataType.InventorySash);
        }
        public DateTime StampSecorSpells = new DateTime();
        public DateTime StampBloodyScytle = new DateTime();
        public DateTime MedicineStamp = new DateTime();
        public uint ReceiveTest = 0;
        public DateTime LastSuspect = DateTime.Now;
        public DateTime ReceivePing = new DateTime();

        public DateTime LastMove;
        public DateTime LastSuccessCaptcha = DateTime.Now;
        public bool LootStones = true, LootPlusItems = true, LootDragonBall = true, LootMetero = true, LootSmallLotteryTicketPack = true, LootRefenieryPacks = true;
        private ushort _stamina;
        public int NextCaptcha = 5;
        public uint OnlinePoints;
        public uint VotePoints;
        public uint PVPPoints;
        public uint PVEPoints;
        public DateTime BossTeleCooldown;
        public DateTime SummonGuild;
        public int TotalHits, Hits, Chains, MaxChains;
        public int BossPoints = 0;
        public int stickpoints = 0;
        //
        public int DragonWar = 0;
        public int EliteLadderTournament = 0;
        public int FFa = 0;
        public int FreezeWar = 0;
        public int Get3Out = 0;
        public int Get5Out = 0;
        public int SkillLadder = 0;
        public int LastManStand = 0;
        public int PassTheBomb = 0;
        public int SkillChampionship = 0;
        public int skillmaster = 0;
        public int ZombieWar = 0;
        //
        public bool ShowGemEffects = true;
        internal bool SendAllies = true;

        internal bool DeadState = true;
        //internal Extensions.Time32 JumpingStamp, PreviousJump;
        //internal Extensions.Time32 WalkingStamp, PreviousWalk;
        internal bool BlockMovementCo = false;
        //internal DateTime BlockMovement;
        internal uint ArenaCPS;
        internal int DragonPills = 0;

        internal void SolveCaptcha()
        {
            Owner.MobsKilled = 0;
            WaitingKillCaptcha = false;
            KillCountCaptcha = "";
            LastSuccessCaptcha = DateTime.Now;
            NextCaptcha = Role.Core.Random.Next(15, 30);
        }

        public uint AtiveQuestApe = 0;

        public uint QuestCaptureType = 0;
        public Random MyRandom = new Random(Program.GetRandom.Next());
        public  bool Rate(int value)
        {
            return value > MyRandom.Next() % 100;
        }
        public DateTime SickleStamp = new DateTime();
        public int FootballTeamID = 0;
        public bool OnAutoHunt = false;
        public ulong AutoHuntExp = 0;
        public uint FootBallMatchPoints = 0;
        public uint MyFootBallPoints = 0;

        public ulong DailySignUpDays = 0;
        public byte DailySignUpRewards = 0;
        public byte DailyMonth = 0;
        public uint DailyDays
        {
            get
            {
                uint days = 0;
                for (byte x = 0; x < 31; x++)
                    if ((DailySignUpDays & (1ul << x)) == (1ul << x))
                        days += 1;
                return days;
            }
        }

        public uint TaskReward = 0;
        public uint TaskRewardIndex = 0;
        //public uint CountSpeedHack = 0;

        public ushort CountryID = 0;

        public Extensions.Time32 WindWalkerEffect = new Extensions.Time32();

        public void SwitchWingWalkerAttack(ServerSockets.Packet stream)
        {
            if ((MainFlag & MainFlagType.OnMeleeAttack) == MainFlagType.OnMeleeAttack)
            {
                MainFlag &= ~MainFlagType.OnMeleeAttack;


                SendUpdate(stream, (uint)MainFlag, MsgUpdate.DataType.MainFlag);
                Owner.CreateBoxDialog("You have successfully changed your branch to Chaser(Ranged).");
          
              
            }
            else
            {
                MainFlag |= MainFlagType.OnMeleeAttack;
                SendUpdate(stream, (uint)MainFlag, MsgUpdate.DataType.MainFlag);
                Owner.CreateBoxDialog("You have successfully changed your branch to Stomper(Melee).");
            }
        }
        public enum MainFlagType : uint
        {
            None = 0,
            CanClaim = 1 << 0,
            ShowSpecialItems = 1 << 1,
            ClaimGift = 1 << 2,
            OnMeleeAttack = 1 << 3,
        }
        public MainFlagType MainFlag = 0;
        public ushort NameEditCount = 0;
        public uint CurrentTreasureBoxes = 0;
        public uint eveSpeedHunterGamePoints = 0;
        public DateTime TaskQuestTimer = new DateTime();
        public uint QuestMultiple = 0;

        public DateTime AzurePillStamp = new DateTime();
        public List<MsgTitleStorage.TitleType> SpecialTitles = new List<MsgTitleStorage.TitleType>();

        public uint GetTitlesScore()
        {
            uint score = 0;
            foreach (var title in SpecialTitles)
            {
                score += Database.TitleStorage.Titles[(uint)title].Score;
            }
            return score;
        }
        public void AddSpecialTitle(MsgTitleStorage.TitleType type, ServerSockets.Packet stream)
        {
            Database.TitleStorage dbtitle;
            if (Database.TitleStorage.Titles.TryGetValue((uint)type, out dbtitle))
            {
                if (!SpecialTitles.Contains(type))
                {
                    MsgTitleStorage.TitleStorage title = new MsgTitleStorage.TitleStorage();
                    title.ActionID = MsgTitleStorage.Action.UpdateScore;
                    title.dwparam1 = GetTitlesScore();
                    title.dwparam2 = dbtitle.ID;
                    title.dwparam3 = dbtitle.SubID;
                    title.Title = new MsgTitleStorage.Title();
                    title.Title.ID = dbtitle.ID;
                    title.Title.SubId = dbtitle.SubID;
                    Owner.Send(stream.CreateTitleStorage(title));
                    title.dwparam1 = 100;
                    title.ActionID = MsgTitleStorage.Action.FullLoad;
                    Owner.Send(stream.CreateTitleStorage(title));
                    SpecialTitles.Add(type);
                }
            }
        }
        public void SendSpecialTitle(ServerSockets.Packet stream)
        {
            foreach (var _title in SpecialTitles)
            {
                Database.TitleStorage dbtitle;
                if (Database.TitleStorage.Titles.TryGetValue((uint)_title, out dbtitle))
                {
                    MsgTitleStorage.TitleStorage title = new MsgTitleStorage.TitleStorage();
                    title.ActionID = MsgTitleStorage.Action.UpdateScore;
                    title.dwparam1 = GetTitlesScore();
                    title.dwparam2 = dbtitle.ID;
                    title.dwparam3 = dbtitle.SubID;
                    title.Title = new MsgTitleStorage.Title();
                    title.Title.ID = dbtitle.ID;
                    title.Title.SubId = dbtitle.SubID;
                    Owner.Send(stream.CreateTitleStorage(title));
                    title.dwparam1 = 100;
                    title.ActionID = MsgTitleStorage.Action.FullLoad;
                    Owner.Send(stream.CreateTitleStorage(title));
                   // SpecialTitles.Add(_title);
                }
            }
        }
        public void ActiveSpecialTitles(ServerSockets.Packet stream)
        {
            Database.TitleStorage dbtitle;
            if (Database.TitleStorage.Titles.TryGetValue((uint)(SpecialTitleID / 10000), out dbtitle))
            {
                MsgTitleStorage.TitleStorage title = new MsgTitleStorage.TitleStorage();
                title.ActionID = MsgTitleStorage.Action.UpdateScore;
                title.dwparam1 = GetTitlesScore();
                title.dwparam2 = dbtitle.ID;
                title.dwparam3 = dbtitle.SubID;
                title.Title = new MsgTitleStorage.Title();
                title.Title.ID = dbtitle.ID;
                title.Title.SubId = dbtitle.SubID;
                title.Title.dwparam1 = 100;
                Owner.Send(stream.CreateTitleStorage(title));
            }
            if (Database.TitleStorage.Titles.TryGetValue((uint)(SpecialWingID / 10000), out dbtitle))
            {
                MsgTitleStorage.TitleStorage title = new MsgTitleStorage.TitleStorage();
                title.ActionID = MsgTitleStorage.Action.UpdateScore;
                title.dwparam1 = GetTitlesScore();
                title.dwparam2 = dbtitle.ID;
                title.dwparam3 = dbtitle.SubID;
                title.Title = new MsgTitleStorage.Title();
                title.Title.ID = dbtitle.ID;
                title.Title.SubId = dbtitle.SubID;
                title.Title.dwparam1 = 100;
                Owner.Send(stream.CreateTitleStorage(title));
            }
        }
        public uint SpecialTitleID = 0;
        public uint SpecialWingID = 0;
        public uint SpecialTitleScore = 0;

        public void RemoveSpecialTitle(MsgTitleStorage.TitleType type, ServerSockets.Packet stream)
        {
            Database.TitleStorage dbtitle;
            if (Database.TitleStorage.Titles.TryGetValue((uint)type, out dbtitle))
            {
                if (SpecialTitles.Contains(type))
                    SpecialTitles.Remove(type);
                MsgTitleStorage.TitleStorage title = new MsgTitleStorage.TitleStorage();
                title.ActionID = MsgTitleStorage.Action.UnEquip;
                title.dwparam2 = dbtitle.ID;
                title.dwparam3 = dbtitle.SubID;
                Owner.Send(stream.CreateTitleStorage(title));
                title.dwparam1 = GetTitlesScore();
                title.Title = new MsgTitleStorage.Title();
                title.Title.ID = dbtitle.ID;
                title.Title.SubId = dbtitle.SubID;
                title.Title.dwparam1 = 0;
                Owner.Send(stream.CreateTitleStorage(title));
                title.ActionID = MsgTitleStorage.Action.UpdateScore;
                Owner.Send(stream.CreateTitleStorage(title));
                title.ActionID = MsgTitleStorage.Action.RemoveTitle;
                Owner.Send(stream.CreateTitleStorage(title));

                if (SpecialTitleID / 10000 == (uint)type)
                    SpecialTitleID = SpecialTitleScore = 0;
                else if (SpecialWingID / 10000 == (uint)type)
                    SpecialWingID = 0;
             
            }

        }




        public bool StartVote = false;
        public bool VIPsystem = false;
        public bool TaskRewardClaim = false;
        public Extensions.Time32 StartVoteStamp = new Extensions.Time32();
        public Extensions.Time32 VIPsystemStamp = new Extensions.Time32();
        public Extensions.Time32 TaskRewardClaimStamp = new Extensions.Time32();

        public bool OnAttackPotion = false;
        public Extensions.Time32 OnAttackPotionStamp = new Extensions.Time32();
        public void ActiveAttackPotion(int Timer)
        {
            OnAttackPotion = true;
            OnAttackPotionStamp = Extensions.Time32.Now.AddMinutes(Timer);
            AddFlag(MsgUpdate.Flags.Stigma, 60, true);
#if Arabic
              Owner.SendSysMesage("Your attack will increase during the next 30 minutes.", MsgMessage.ChatMode.System);
#else
            Owner.SendSysMesage("Your attack will increase during the next 30 minutes.", MsgMessage.ChatMode.System);
#endif
          
        }

        public bool OnDefensePotion = false;
        public Extensions.Time32 OnDefensePotionStamp = new Extensions.Time32();
        public void ActiveDefensePotion(int Timer)
        {
            OnDefensePotion = true;
            OnDefensePotionStamp = Extensions.Time32.Now.AddMinutes(Timer);
            AddFlag(MsgUpdate.Flags.Shield, 60, true);
#if Arabic
               Owner.SendSysMesage("Your defense will increase during the next 30 minutes.", MsgMessage.ChatMode.System);
#else
            Owner.SendSysMesage("Your defense will increase during the next 30 minutes.", MsgMessage.ChatMode.System);
#endif
         
        }


        public bool WaveofBlood = false;
        public DateTime WaveofBloodStamp = new DateTime();

        public bool AllowDynamic { get; set; } 
        public uint DailyMagnoliaChance = 0;
        public uint DailyMagnoliaItemId = 0;

        public uint DailySpiritBeadCount = 0;

        public uint DailySpiritBeadItem = 0;
        public uint DailyRareChance = 0;
        public uint DailyHeavenChance = 0;

        public uint TodayChampionPoints = 0;
        public uint HistoryChampionPoints = 0;

        uint _ChampionPoints;
        public uint ChampionPoints
        {
         
            get
            {
                return _ChampionPoints;
            }
            set
            {
                
                _ChampionPoints = value;
                if (_ChampionPoints > HistoryChampionPoints)
                    HistoryChampionPoints = value;
            }
        }
      
        public void AddChampionPoints(uint value, bool settodayvalue = true)
        {
            if (settodayvalue)
            {
                if (TodayChampionPoints > 650)
                {
                    Owner.SendSysMesage("Your Champion Points have reached the maximum amount of 650 points. You can collect more points tomorrow.");
                    return;
                }
            }
            ChampionPoints += value;
            TodayChampionPoints += value;
        }

        public Extensions.Time32 PickStamp = Extensions.Time32.Now;
        public bool ActivePick = false;
        public void AddPick(ServerSockets.Packet stream, string Name, ushort timer)
        {
            PickStamp = Extensions.Time32.Now.AddSeconds(timer);
            Owner.Send(stream.ActionPick(UID, 1, timer, Name));
            ActivePick = true;
            ActionQuery action = new ActionQuery()
            {
                ObjId = UID,
                Type = (ActionType)1165,
                wParam1 = 277,
                wParam2 = 2050
            };
            Owner.Send(stream.ActionCreate(&action));
        }
        public void RemovePick(ServerSockets.Packet stream)
        {
            ActivePick = false;
            Owner.Send(stream.ActionPick(UID, 3, 0, Name));
        }

        public uint EpicQuestChance = 0;
        public uint EpicQuestTwo_ndChance = 0;
        public uint EpicQuestThree_rdChance = 0;

        public uint IndexInScreen { get; set; }
        public bool IsTrap() { return false; }

        public byte Away = 0;
        public uint OnlineMinutes = 0;
        public Extensions.Time32 OnlineStamp = Extensions.Time32.Now;
        public uint OnlineHours
        {
            get { return OnlineMinutes / 60; }
        }
        public Extensions.Time32 LastAttack;
        public Extensions.Time32 KillCountCaptchaStamp;
        public bool WaitingKillCaptcha;
        public string KillCountCaptcha;

        public Role.Instance.InnerPower InnerPower;

        //public Game.MsgTournaments.MsgSteedRace.UsableRacePotion[] RacePotions = null;

        public uint KillerPkPoints = 0;
        public uint XtremePkPoints = 0;

        public uint DragonWarHits = 0;
        public uint DragonWarScore = 0;
        public uint SpeedHunterGameScore = 0;
        
      

        public uint TeamDeathMacthKills = 0;
        public uint TournamentKills = 0;
        public Extensions.Time32 StampGuildWarScore = new Extensions.Time32();

        public uint SpecialGarment = 0;
        public void RemoveSpecialGarment(ServerSockets.Packet stream)
        {
            SpecialGarment = 0;
            GarmentId = 0;
         //   Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.Unequip, uint.MaxValue - 1, (ushort)Flags.ConquerItem.Garment, 0, 0, 0, 0));

            MsgGameItem item;
            if (Owner.Equipment.TryGetEquip(Flags.ConquerItem.Garment, out item))
            {
                Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.Equip, item.UID, (ushort)Flags.ConquerItem.Garment, 0, 0, 0, 0));
                item.Mode = Flags.ItemMode.AddItem;
                item.Send(Owner, stream);
            }
            Owner.Equipment.QueryEquipment(Owner.Equipment.Alternante);
        }
        public void AddSpecialGarment(ServerSockets.Packet stream, uint ID)
        {

            SpecialGarment = ID;
            GarmentId = SpecialGarment;

        
            Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.Equip,uint.MaxValue -1,(ushort)Flags.ConquerItem.Garment, 0, 0, 0, 0));

            Game.MsgServer.MsgGameItem item = new MsgGameItem();
            item.ITEM_ID = ID;
            item.Mode = Flags.ItemMode.AddItem;
            item.UID = uint.MaxValue - 1;
            item.Color = Flags.Color.Red;
            item.Position = (ushort)Flags.ConquerItem.Garment;
            item.Durability = Database.Server.ItemsBase[ID].Durability;
            item.Send(Owner, stream);
            Owner.Equipment.AppendItems(true, Owner.Equipment.CurentEquip, stream);
            View.SendView(GetArray(stream, false), false);
        }

        public Extensions.Time32 StampArenaScore = new Extensions.Time32();

        public uint HitShoot = 0;
        public uint MisShoot = 0;
        public uint ArenaDeads = 0;
        public uint ArenaKills = 0;


        public uint KillersDisCity = 0;


        public byte TaoistPower = 0;
        public Extensions.Time32 TaoistStampPower = Extensions.Time32.Now;

        public void UpdateTaoPower(ServerSockets.Packet stream)
        {
            if (Database.AtributesStatus.IsWater(Class) && TaoistPower == 10)
            {
                if (!ContainFlag(MsgUpdate.Flags.FullPowerWater))
                    AddFlag(MsgUpdate.Flags.FullPowerWater, StatusFlagsBigVector32.PermanentFlag, false);
            }
            else if (Database.AtributesStatus.IsFire(Class) && TaoistPower == 10)
            {
                if (!ContainFlag(MsgUpdate.Flags.FullPowerFire))
                    AddFlag(MsgUpdate.Flags.FullPowerFire, StatusFlagsBigVector32.PermanentFlag, false);
            }

            uint icon = Database.AtributesStatus.IsWater(Class) ? (uint)172 : (uint)173;

            Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
            stream = packet.Append(stream, MsgUpdate.DataType.AppendIcon, icon, 3, (uint)(TaoistPower * 30), 0);
            stream = packet.GetArray(stream);

            View.SendView(stream, true);
        }
        public void SendPowerTaoist(Client.GameClient user , ServerSockets.Packet stream)
        {
            if (TaoistPower > 0)
            {
                uint icon = Database.AtributesStatus.IsWater(Class) ? (uint)172 : (uint)173;

                Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                stream = packet.Append(stream, MsgUpdate.DataType.AppendIcon, icon, 3, (uint)(TaoistPower * 30), 0);
                stream = packet.GetArray(stream);

                user.Send(stream);
            }
        }

        public void UpdateTaoistPower(Extensions.Time32 now)
        {
            if (TaoistPower < 10 && Class > 100)
            {
                if (now > TaoistStampPower.AddSeconds(9))
                {
                    Game.MsgServer.MsgGameItem GameItem;
                    if (Owner.Equipment.TryGetEquip(Flags.ConquerItem.RightWeapon, out GameItem))
                    {
                        if (Database.ItemType.IsTaoistEpicWeapon(GameItem.ITEM_ID))
                        {
                            TaoistPower += 1;
                            using (var rec = new ServerSockets.RecycledPacket())
                                UpdateTaoPower(rec.GetStream());
                        }
                    }
                    TaoistStampPower = Extensions.Time32.Now;
                }
            }
        }

        public uint AparenceType = 0;
       // public ServerSockets.Packet MyStream = ServerSockets.PacketRecycle.Take();
       // public ServerSockets.Packet MyStream2 =  ServerSockets.PacketRecycle.Take();

        public unsafe void memcpy(void* dest, void* src, Int32 size)
        {
            Int32 count = size / sizeof(long);
            for (Int32 i = 0; i < count; i++)
                *(((long*)dest) + i) = *(((long*)src) + i);

            Int32 pos = size - (size % sizeof(long));
            for (Int32 i = 0; i < size % sizeof(long); i++)
                *(((Byte*)dest) + pos + i) = *(((Byte*)src) + pos + i);
        }
        public unsafe byte[] GetBytes(byte* packet)
        {
            int size = *(ushort*)(packet);
            size += 8;
            byte[] buff = new byte[size];
            fixed (byte* ptr = buff)
                memcpy(ptr, packet, size);
            return buff;
        }
        public uint TCCaptainTimes = 0;
        public bool IsCheckedPass = false;
        public uint SecurityPassword = 0;
        public uint OnReset = 0;
        public DateTime ResetSecurityPassowrd = new DateTime();

        public Extensions.Time32 LoginTimer = Extensions.Time32.Now;

        public bool InElitePk = false;
        public bool InTeamPk = false;

        public uint RacePoints = 0;
        public uint DonatePoints = 0;
        public uint Remove = 0;
        public uint SpeedHunterGamePoints = 0;


        public List<byte> Titles = new List<byte>();

        public unsafe void AddTitle(byte _title, bool aSwitch)
        {
            if (!Titles.Contains(_title))
            {
                Titles.Add(_title);

                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    Owner.Send(stream.TitleCreate(UID, _title, MsgTitle.QueueTitle.Enqueue));

                }

                if (aSwitch)
                    SwitchTitle(_title);
            }
        }
        public unsafe void SwitchTitle(byte ntitle)
        {
            if (Titles.Contains(ntitle) || ntitle == 0)
            {
                MyTitle = ntitle;

                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    Owner.Send(stream.TitleCreate(UID, ntitle, MsgTitle.QueueTitle.Change));

                }
            }
        }
        public unsafe byte MyTitle;

        public Flags.PKMode PreviousPkMode = Flags.PKMode.Capture;
        public unsafe void SetPkMode(Flags.PKMode pkmode)
        {
            PreviousPkMode = PkMode;
            PkMode = pkmode;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                ActionQuery action = new ActionQuery()
                {
                    ObjId = UID,
                    dwParam = (uint)PkMode,
                    Type = ActionType.SetPkMode
                };
                Owner.Send(stream.ActionCreate(&action));
            }

        }
        public unsafe void RestorePkMode()
        {
            PkMode = PreviousPkMode;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                ActionQuery action = new ActionQuery()
                {
                    ObjId = UID,
                    dwParam = (uint)PkMode,
                    Type = ActionType.SetPkMode
                };
                Owner.Send(stream.ActionCreate(&action));
            }
        }
        public DateTime EnlightenTime = new DateTime();

        //public int CursedTimer = 0;
        //public void AddCursed(int time)
        //{
        //    if (time != 0)
        //    {
        //        if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Cursed))
        //            RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Cursed);

        //        CursedTimer += time;
        //        using (var rec = new ServerSockets.RecycledPacket())
        //        {
        //            var stream = rec.GetStream();
        //            SendUpdate(stream,CursedTimer, Game.MsgServer.MsgUpdate.DataType.CursedTimer);
        //        }
        //        AddFlag(Game.MsgServer.MsgUpdate.Flags.Cursed, CursedTimer, false, 1);
        //    }
        //}


        

        public uint MyKillerUID;
        public string MyKillerName;

        public bool Delete = false;


        public uint testtttttttttt = 0;


        public Extensions.MyList<Clone> MyClones = new Extensions.MyList<Clone>();

        public ConcurrentDictionary<ushort, FloorSpell.ClientFloorSpells> FloorSpells = new ConcurrentDictionary<ushort, FloorSpell.ClientFloorSpells>();

        public ushort RandomSpell = 0;

        public bool DbTry = false;

        public bool TCTry = false;
        public bool BITry = false;
        public bool PCTry = false;
        public bool DCTry = false;
        public bool ACTry = false;

        public bool QuestTry = false;

        public byte AddJade = 0;

        public byte LotteryEntries;
        public Database.LotteryTable.LotteryItem LotteryItem;


        public bool Reincarnation = false;

        public byte JingTryngUltra = 0;
        public Instance.JiangHu MyJiangHu;
        public unsafe byte JiangHuTalent;
        public unsafe byte JiangHuActive;

        Instance.Clan.Member clanmemb;
        public Instance.Clan.Member MyClanMember
        {
            get
            {
                if (clanmemb == null)
                {
                    if (MyClan != null)
                    {
                        MyClan.Members.TryGetValue(UID, out clanmemb);
                    }
                }
                return clanmemb;
            }
            set
            {
                clanmemb = value;
            }
        }

        public Instance.Clan MyClan;
        public unsafe uint ClanUID;
        public unsafe ushort ClanRank;



        public Role.Instance.Guild.Member MyGuildMember;
        public Role.Instance.Guild MyGuild;
        public uint TargetGuild = 0;

        uint _extbattle;
        public unsafe uint ExtraBattlePower
        {
            get { return _extbattle; }
            set
            {
                _extbattle = value;
            }
        }

        public unsafe Flags.GuildMemberRank GuildRank = Flags.GuildMemberRank.None;
        public unsafe uint GuildID;

        uint guildBP;
        public uint GuildBattlePower
        {
            get
            {
                return guildBP;
            }
            set
            {
                ExtraBattlePower -= guildBP;
                guildBP = value;
                ExtraBattlePower += guildBP;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    SendUpdate(stream,guildBP, Game.MsgServer.MsgUpdate.DataType.GuildBattlePower);
                }
            }
        }

        uint _clanbp;
        public uint ClanBp
        {
            get { return _clanbp; }
            set
            {
                ExtraBattlePower -= _clanbp;
                _clanbp = value;
                ExtraBattlePower += _clanbp;
            }
        }

        uint _mentorBp;
        private uint MentorBp
        {
            get { return _mentorBp; }
            set
            {
                ExtraBattlePower -= _mentorBp;
                ExtraBattlePower += value;
                _mentorBp = value;
            }
        }
        public unsafe void SetMentorBattlePowers(uint val, uint mentorPotency)
        {

            MentorBp = val;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Game.MsgServer.MsgUpdate upd = new Game.MsgServer.MsgUpdate(stream, UID, 2);
                stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.ExtraBattlePower, val);
                stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.ExtraBattlePower, mentorPotency);
                stream = upd.GetArray(stream);
                Owner.Send(stream);
            }
        }

        public uint targetTrade = 0;

        public Role.Instance.Associate.MyAsociats MyMentor = null;
        public Role.Instance.Associate.MyAsociats Associate;


        public uint TradePartner = 0;
        public uint TargetFriend = 0;

        public Role.Instance.Nobility Nobility;
        Role.Instance.Nobility.NobilityRank _NobilityRank;
        public  Role.Instance.Nobility.NobilityRank NobilityRank
        {
            get{return _NobilityRank;}
            set
            {
                _NobilityRank = value;
                if (MyGuild != null && MyGuildMember != null)
                    MyGuildMember.NobilityRank = (uint)_NobilityRank;
                if (UnionMemeber != null)
                    UnionMemeber.NobilityRank = _NobilityRank;
            }
        }

        public Instance.Flowers Flowers;
        public unsafe uint FlowerRank;
        public bool OnFairy = false;
        public unsafe Game.MsgServer.MsgTransformFairy FairySpawn;


        public Instance.Chi MyChi;
        public ushort ActiveDance = 0;
        public Client.GameClient ObjInteraction;

        uint _KingDomExploits;
        public uint KingDomExploits
        {
            get { return _KingDomExploits; }
            set
            {
                _KingDomExploits = value;
                if (InUnion)
                    UnionMemeber.Exploits = value;
                UpdateExploitsRank();

            }
        }
        public void UpdateExploitsRank()
        {
            if (KingDomExploits < 200)
                return;

            if (KingDomExploits >= 23000)
                SetExploitsRank(Role.Flags.ExploitsRank.GeneralinChief);
            else if(KingDomExploits >= 15000)
                SetExploitsRank(Role.Flags.ExploitsRank.FlyingCavalryGeneral);
            else if (KingDomExploits >= 10000)
                SetExploitsRank(Role.Flags.ExploitsRank.ChariotsandCavalryGeneral);
            else if (KingDomExploits >= 7500)
                SetExploitsRank(Role.Flags.ExploitsRank.ChiefofStaff);
            else if (KingDomExploits >= 6000)
                SetExploitsRank(Role.Flags.ExploitsRank.General);
            else if (KingDomExploits >= 4700)
                SetExploitsRank(Role.Flags.ExploitsRank.AssistantGeneral);
            else if (KingDomExploits >= 3700)
                SetExploitsRank(Role.Flags.ExploitsRank.DeputyGeneral);
            else if (KingDomExploits >= 2800)
                SetExploitsRank(Role.Flags.ExploitsRank.MasterSergeant);
            else if (KingDomExploits >= 2100)
                SetExploitsRank(Role.Flags.ExploitsRank.StaffSergeant);
            else if (KingDomExploits >= 1500)
                SetExploitsRank(Role.Flags.ExploitsRank.Sergeant);
            else if (KingDomExploits >= 1000)
                SetExploitsRank(Role.Flags.ExploitsRank.Centurion);
            else if (KingDomExploits >= 500)
                SetExploitsRank(Role.Flags.ExploitsRank.Decurion);
            else if (KingDomExploits >= 200)
                SetExploitsRank(Role.Flags.ExploitsRank.Corporal);
        }
        public Role.Flags.ExploitsRank ExploitsRank = 0;
        public void SetExploitsRank(Role.Flags.ExploitsRank rank)
        {
            ExploitsRank = rank;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                SendUpdate(stream, (long)rank, MsgUpdate.DataType.ExploitsRank,true);
            }
        }

        public unsafe Game.MsgServer.InteractQuery InteractionEffect = default(Game.MsgServer.InteractQuery);
        public bool OnInteractionEffect = false;

        public Instance.SubClass SubClass;
        public unsafe uint SubClassHasPoints;
        public unsafe Database.DBLevExp.Sort ActiveSublass;

        public uint ToxicLevel = 0;
        public bool ContainReflect { get { return Database.AtributesStatus.IsWarrior(SecoundeClass); } }
        public bool BlackSpot = false;
        public Extensions.Time32 Stamp_BlackSpot = new Extensions.Time32();

        public byte UseStamina = 0;
        public Extensions.Time32 Protect = new Extensions.Time32();
        private Extensions.Time32 ProtectedJumpAttack = new Extensions.Time32();
        internal void ProtectAttack(int StampMiliSeconds)
        {
            Protect = Extensions.Time32.Now.AddMilliseconds(StampMiliSeconds);
        }
        internal void ProtectJumpAttack(int Seconds)
        {
            ProtectedJumpAttack = Extensions.Time32.Now.AddSeconds(Seconds);
        }
        internal bool AllowAttack()
        {
            return Extensions.Time32.Now > Protect && Extensions.Time32.Now > ProtectedJumpAttack;
        }
        public uint ShieldBlockDamage = 0;

        internal void CheckAura()
        {
            if (UseAura != Game.MsgServer.MsgUpdate.Flags.Normal)
            {
                IncreaseStatusAura(UseAura, Aura);
            }
        }
       
        public Game.MsgServer.MsgUpdate.Flags UseAura = Game.MsgServer.MsgUpdate.Flags.Normal;
        public Database.MagicType.Magic Aura;
        private int AuraTimer = 0;

        internal unsafe bool AddAura(Game.MsgServer.MsgUpdate.Flags flag, Database.MagicType.Magic new_aura, int Timer)
        {
            if (flag == UseAura)
            {
                RemoveFlag(UseAura);
                DecreaseStatusAura(UseAura);
                UseAura = Game.MsgServer.MsgUpdate.Flags.Normal;
                return false;
            }
            AuraTimer = Timer;
            if (UseAura != Game.MsgServer.MsgUpdate.Flags.Normal)
            {
                RemoveFlag(UseAura);
                DecreaseStatusAura(UseAura);
                UseAura = Game.MsgServer.MsgUpdate.Flags.Normal;
            }
            UseAura = flag;
            Aura = new_aura;
            IncreaseStatusAura(flag, new_aura);
            AddFlag(flag, Timer, true, 0);

            Game.MsgServer.MsgFlagIcon.ShowIcon icon = MsgFlagIcon.ShowIcon.EarthAura;

            if (flag == Game.MsgServer.MsgUpdate.Flags.FeandAura) icon = Game.MsgServer.MsgFlagIcon.ShowIcon.FeandAura;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.TyrantAura) icon = Game.MsgServer.MsgFlagIcon.ShowIcon.TyrantAura;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.MetalAura) icon = Game.MsgServer.MsgFlagIcon.ShowIcon.MetalAura;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.WoodAura) icon = Game.MsgServer.MsgFlagIcon.ShowIcon.WoodAura;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.WaterAura) icon = Game.MsgServer.MsgFlagIcon.ShowIcon.WaterAura;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.FireAura) icon = Game.MsgServer.MsgFlagIcon.ShowIcon.FireAura;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.EartAura) icon = Game.MsgServer.MsgFlagIcon.ShowIcon.EarthAura;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Owner.Send(stream.FlagIconCreate(UID, icon, new_aura.Level, (uint)new_aura.Damage));
            }

            return true;
        }

        private void DecreaseStatusAura(Game.MsgServer.MsgUpdate.Flags flag)
        {
            if (flag == Game.MsgServer.MsgUpdate.Flags.FeandAura)
                Owner.Status.Immunity -= (uint)(Aura.Damage * 100);
            else if (flag == Game.MsgServer.MsgUpdate.Flags.TyrantAura)
                Owner.Status.CriticalStrike -= (uint)(Aura.Damage * 100);
            else if (flag == Game.MsgServer.MsgUpdate.Flags.MetalAura)
                Owner.Status.MetalResistance -= (uint)Aura.Damage;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.WoodAura)
                Owner.Status.WoodResistance -= (uint)Aura.Damage;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.WaterAura)
                Owner.Status.WaterResistance -= (uint)Aura.Damage;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.FireAura)
                Owner.Status.FireResistance -= (uint)Aura.Damage;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.EartAura)
                Owner.Status.EarthResistance -= (uint)Aura.Damage;
        }
        private void IncreaseStatusAura(Game.MsgServer.MsgUpdate.Flags flag, Database.MagicType.Magic new_aura)
        {
            if (flag == Game.MsgServer.MsgUpdate.Flags.FeandAura)
                Owner.Status.Immunity += (uint)(new_aura.Damage * 100);
            else if (flag == Game.MsgServer.MsgUpdate.Flags.TyrantAura)
                Owner.Status.CriticalStrike += (uint)(new_aura.Damage * 100);
            else if (flag == Game.MsgServer.MsgUpdate.Flags.MetalAura)
                Owner.Status.MetalResistance += (uint)new_aura.Damage;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.WoodAura)
                Owner.Status.WoodResistance += (uint)new_aura.Damage;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.WaterAura)
                Owner.Status.WaterResistance += (uint)new_aura.Damage;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.FireAura)
                Owner.Status.FireResistance += (uint)new_aura.Damage;
            else if (flag == Game.MsgServer.MsgUpdate.Flags.EartAura)
                Owner.Status.EarthResistance += (uint)new_aura.Damage;
        }
        public uint SpouseUID = 0;

        public Extensions.Time32 AttackStamp = new Extensions.Time32();
        public Extensions.Time32 SpellAttackStamp = new Extensions.Time32();

        public bool OnTransform { get { return TransformationID != 0; } }
        public ClientTransform TransformInfo = null;
        public byte PoisonLevel = 0;
        public byte PoisonLevehHu = 0;

        public bool ActivateCounterKill = false;
   
        public Action<Client.GameClient> MessageOK;
        public Action<Client.GameClient> MessageCancel;
        public Extensions.Time32 StartMessageBox = new Extensions.Time32();
        public unsafe void MessageBox(string text, Action<Client.GameClient> msg_ok, Action<Client.GameClient> msg_cancel, int Seconds = 0, Game.MsgServer.MsgStaticMessage.Messages messaj = Game.MsgServer.MsgStaticMessage.Messages.None)
        {
            if (!OnMyOwnServer)
                return;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                MessageOK = msg_ok;
                MessageCancel = msg_cancel;
                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(Owner, stream);
                dialog.CreateMessageBox(text).FinalizeDialog(true);
                StartMessageBox = Extensions.Time32.Now.AddHours(24);
                if (Seconds != 0)
                {
                    StartMessageBox = Extensions.Time32.Now.AddSeconds(Seconds);
                    if (messaj != Game.MsgServer.MsgStaticMessage.Messages.None)
                    {
                        Owner.Send(stream.StaticMessageCreate(messaj, MsgStaticMessage.Action.Append, (uint)Seconds));
                    }
                    /*Game.MsgServer.MsgDataPacket datapacket = Game.MsgServer.MsgDataPacket.Create();
                    datapacket.UID = UID;
                    datapacket.ID = Game.MsgServer.ActionType.CountDown;
                    unsafe
                    {
                        Send((byte*)&datapacket);
                    }*/
                }
            }
        }

        public void RemoveBuffersMovements(ServerSockets.Packet stream)
        {
            InUseIntensify = false;
            InUseSpiritFocus = false;

            //Intensify = false;

            RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Praying);
            RemoveFlag(Game.MsgServer.MsgUpdate.Flags.CastPray);
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.MagicDefender))
            {
                RemoveFlag(Game.MsgServer.MsgUpdate.Flags.MagicDefender);
                SendUpdate(stream,Game.MsgServer.MsgUpdate.Flags.MagicDefender, 0
   , 0, 0, Game.MsgServer.MsgUpdate.DataType.AzureShield, true);
            }
        }

        public bool InUseIntensify = false;
        public bool InUseSpiritFocus = false;

        public Extensions.Time32 IntensifyStamp = new Extensions.Time32();
        public Extensions.Time32 SpiritFocusStamp = new Extensions.Time32();

        public bool Intensify = false;
        public bool SpiritFocus = false;
        public int SpiritFocusDamage = 0;

        public int IntensifyDamage = 0;

        public int BattlePower
        {
            get
            {

                int val = (int)(Level + Reborn * 5 + Owner.Equipment.BattlePower /*+ (byte)NobilityRank*/ + ExtraBattlePower);

                //                int val = (int)(Level + Reborn * 5 + Owner.Equipment.BattlePower + (byte)NobilityRank + ExtraBattlePower);

                return Math.Min(405, val);
            }
        }
        public int RealBattlePower
        {
            get
            {
                int val = (int)(Level + Reborn * 5 + Owner.Equipment.BattlePower/* + (byte)NobilityRank*/);


                return val;
            }
        }
        ushort azuredef;
        public byte AzureShieldLevel;
        public ushort AzureShieldDefence
        {
            get { return azuredef; }
            set
            {
                azuredef = value;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    SendUpdate(stream,Game.MsgServer.MsgUpdate.Flags.AzureShield, 60
                        , value, AzureShieldLevel, Game.MsgServer.MsgUpdate.DataType.AzureShield, true);
                }
            }
        }
        public Extensions.Time32 XPListStamp = new Extensions.Time32();
        //public ushort Stamina = 0;
        public ushort Stamina
        {
            get
            {
                if (UnlimitedArenaRooms.Maps.ContainsValue(DynamicID))
                    return 150;
                return _stamina;
            }
            set { _stamina = value; }
        }
        public ushort GetAddStamina()
        {
            //Role.Flags.ConquerAction act = Action;
            //if (VipLevel == 6)
            //    switch (act)
            //    {
            //        case Role.Flags.ConquerAction.Sit:
            //            return 15;
            //        default: return 5;
            //    }
            //else
            switch (Action)
            {
                case Role.Flags.ConquerAction.Sit:
                    return 12;
                default: return 5;
            }
            //return 100;
        }
        public StatusFlagsBigVector32 BitVector;
        public void AddSpellFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool RemoveOnDead, int StampSeconds = 0)
        {
            if (BitVector.ContainFlag((int)Flag))
                BitVector.TryRemove((int)Flag);
            AddFlag(Flag, Seconds, RemoveOnDead, StampSeconds);
        }
        public bool AddFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool RemoveOnDead,int StampSeconds =0, uint showamount = 0, uint amount =0)
        {
            if (!BitVector.ContainFlag((int)Flag))
            {
                
                BitVector.TryAdd((int)Flag, Seconds, RemoveOnDead, StampSeconds);
              
                UpdateFlagOffset();
                if ((int)Flag >= 52 && (int)Flag <= 60)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        View.SendView(stream.GameUpdateCreate(UID, (Game.MsgServer.MsgGameUpdate.DataType)Flag, true, showamount, (uint)Seconds, amount),true);

                    }
                }
                return true;
            }
            return false;
        }
        public bool RemoveFlag(Game.MsgServer.MsgUpdate.Flags Flag)
        {
            if (BitVector.ContainFlag((int)Flag))
            {
                BitVector.TryRemove((int)Flag);
                UpdateFlagOffset();
                if (Flag == MsgUpdate.Flags.Oblivion)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Owner.IncreaseExperience(stream,Owner.ExpOblivion);
                    }
                    Owner.ExpOblivion = 0;
                }
                if ((int)Flag >= 52 && (int)Flag <= 60)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        Owner.Send(stream.GameUpdateCreate(UID, (Game.MsgServer.MsgGameUpdate.DataType)Flag, false, 0, 0, 0));

                    }
                }
                return true;
            }
            return false;
        }
        public bool UpdateFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool SetNewTimer, int MaxTime)
        {
            return BitVector.UpdateFlag((int)Flag, Seconds, SetNewTimer, MaxTime);
        }
        public void ClearFlags()
        {
            BitVector.GetClear();
            UpdateFlagOffset();
        }
        public bool ContainFlag(Game.MsgServer.MsgUpdate.Flags Flag)
        {
            return BitVector.ContainFlag((int)Flag);
        }
        public bool CheckInvokeFlag(Game.MsgServer.MsgUpdate.Flags Flag, Extensions.Time32 timer32)
        {
            return BitVector.CheckInvoke((int)Flag, timer32);
        }
        public unsafe void UpdateFlagOffset()
        {
            SendUpdate(BitVector.bits, Game.MsgServer.MsgUpdate.DataType.StatusFlag, true);
        }
        public unsafe void SendUpdateHP()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var MyStream = rec.GetStream();
                Game.MsgServer.MsgUpdate Upd = new Game.MsgServer.MsgUpdate(MyStream, UID, 2);
                MyStream = Upd.Append(MyStream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, Owner.Status.MaxHitpoints);
                MyStream = Upd.Append(MyStream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, HitPoints);
                MyStream = Upd.GetArray(MyStream);
                Owner.Send(MyStream);
            }
        }
        public ushort Dead_X;
        public ushort Dead_Y;

        public bool GetPkPkPoints = false;
        public bool CompleteLogin = false;

        public DateTime GhostStamp = new DateTime();

        public unsafe void Dead(Role.Player killer, ushort DeadX, ushort DeadY, uint KillerUID)
        {
            if (UnlimitedArenaRooms.Maps.ContainsValue(DynamicID))
                return;
            if (OnTransform && TransformInfo != null)
            {
                TransformInfo.FinishTransform();
            }
            else if (OnTransform)
                TransformationID = 0;
            if (killer != null)
            {
                if (Map == 10166 && killer.Map == 10166 || Map == 10137 && killer.Map == 10137)
                {
                    if (killer.HuntingBounsLayer < 9)
                    {
                        killer.HuntingBounsLayer += 1;
                        killer.HuntingBounsStamp = new Extensions.Time32().AddMinutes(5);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, killer.UID, 1);
                            stream = packet.Append(stream, MsgUpdate.DataType.HuntingBouns, (byte)0, (uint)(killer.HuntingBounsStamp - Program.CurrentTime).AllSeconds, 0, killer.HuntingBounsLayer);
                            stream = packet.GetArray(stream);
                            killer.Owner.Send(stream);
                        }
                    }
                    if (HuntingBounsLayer > 0)
                    {
                        HuntingBounsLayer -= 1;
                        if (HuntingBounsLayer == 0)
                            HuntingBounsStamp = new Extensions.Time32().AddMinutes(-4).AddSeconds(-50);
                        else
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                                stream = packet.Append(stream, MsgUpdate.DataType.HuntingBouns, (byte)0, (uint)(HuntingBounsStamp - Program.CurrentTime).AllSeconds, 0, HuntingBounsLayer);
                                stream = packet.GetArray(stream);
                                Owner.Send(stream);
                            }
                        }
                    }
                }
            }
            GhostStamp = DateTime.Now.AddMilliseconds(1000);
            Owner.OnAutoAttack = false;

            //Owner.SendSysMesage("You are dead.", MsgMessage.ChatMode.System);

            GetPkPkPoints = true;
            if (this.ContainFlag(MsgUpdate.Flags.RedName)
                || this.ContainFlag(MsgUpdate.Flags.BlackName)
                || this.ContainFlag(MsgUpdate.Flags.FlashingName))
                GetPkPkPoints = false ;
         
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                
                foreach (var clone in MyClones.GetValues())
                {
                    clone.RemoveThat(this.Owner);
                }
                MyClones.Clear();

              

                if (!Program.FreePkMap.Contains(Map))
                {
                    if (Associate != null && killer != null)
                    {
                        killer.Associate.AddPKExplorer(killer.Owner, this);
                        Associate.AddEnemy(Owner, killer);
                    }
                }
                if (killer != null)
                {
                    if (killer.Owner.OnSoulSpell != 0)
                    {
                        killer.Send(stream.CreateJiangMessage(killer.Owner.OnSoulSpell));
                    }
                    if (killer.InElitePk || killer.InTeamPk)
                    {
                        killer.TournamentKills += 1;
                    }
                    

               
                
                    
                 
                    if (Program.MapCounterHits.Contains(Map))
                    {
                        killer.ArenaKills += 1;
                        ArenaDeads += 1;
                    }

                    if (killer.PkMode == Flags.PKMode.Jiang && JiangHuActive != 0)
                    {
                        if (Map == 1002 || Map == 1000 || Map == 1015 || Map == 1020 || Map == 1011)
                        {
                            if (killer.MyJiangHu != null && MyJiangHu != null)
                                killer.MyJiangHu.Kill(killer.Owner, this.Owner);
                        }
                    }
                  
                }
              
                if (BlackSpot)
                {
                    BlackSpot = false;

                    View.SendView(stream.BlackspotCreate(false, UID), true);

                }
                Dead_X = DeadX;
                Dead_Y = DeadY;
                DeadStamp = Extensions.Time32.Now;
                HitPoints = 0;
                ClearFlags();
                AddFlag(Game.MsgServer.MsgUpdate.Flags.Dead, StatusFlagsBigVector32.PermanentFlag, true);
              


                if (Map == 700)
                {
                    Owner.EndQualifier();
                }

                if (killer != null)
                {
                    killer.XPCount++;



                    InteractQuery action = new InteractQuery()
                    {
                        UID = killer.UID,
                        X = DeadX,
                        Y = DeadY,
                        AtkType = MsgAttackPacket.AttackID.Death,
                        KillCounter = killer.KillCounter,
                        SpellID = (ushort)(Database.ItemType.IsBow(killer.Owner.Equipment.RightWeapon) ? 5 : 1),
                        OpponentUID = UID,
                    };
                    View.SendView(stream.InteractionCreate(&action), true);


                    if (!Program.NoDropItems.Contains(Map) && !Program.FreePkMap.Contains(Map))
                    {
                        if (killer.PkMode != Flags.PKMode.Jiang)
                            CheckDropItems(killer, stream);

                        CheckPkPoints(killer);
                    }

                }
                else
                {


                    InteractQuery action = new InteractQuery()
                    {
                        UID = KillerUID,
                        X = DeadX,
                        Y = DeadY,
                        AtkType = MsgAttackPacket.AttackID.Death,
                        OpponentUID = UID
                    };
                    View.SendView(stream.InteractionCreate(&action), true);


                    if (!Program.NoDropItems.Contains(Map) && !Program.FreePkMap.Contains(Map))
                    {
                        CheckDropItems(killer, stream);
                    }

                }
               if (Owner.PerfectionStatus.StraightLife > 0)
                {
                    if (Role.Core.Rate(Owner.PerfectionStatus.StraightLife))
                    {
                        View.SendView(stream.MsgRefineEffectCreate(new MsgRefineEffect.RefineEffectProto()
                        {
                            Effect = MsgRefineEffect.RefineEffects.InvisbleArrow,
                            Id = UID,
                            dwParam = UID
                        }), true);
                        this.Revive(stream);
                    }
                }

            }
        }
        public void CheckDropItems(Role.Player killer, ServerSockets.Packet stream)
        {
            if (OnMyOwnServer == false)
                return;
            if (Map == 3935)
                return;
            try
            {
                ushort x = X;
                ushort y = Y;

                /* uint DropMoney = (uint)(Money + 1 / 12);
                 if (DropMoney > 1)
                 {
                     DropMoney = (uint)Program.GetRandom.Next(1, (int)DropMoney);
                     if (Owner.Map.AddGroundItem(ref x, ref y))
                     {


                         Game.MsgServer.MsgGameItem DataItem = new Game.MsgServer.MsgGameItem();
                         DataItem.ITEM_ID = Database.ItemType.MoneyItemID((uint)DropMoney);
                         DataItem.Durability = (ushort)Program.GetRandom.Next(1000, 3000);
                         DataItem.MaximDurability = (ushort)Program.GetRandom.Next(DataItem.Durability, 6000);
                         DataItem.Color = Role.Flags.Color.Red;

                         Game.MsgFloorItem.MsgItem DropItem = new Game.MsgFloorItem.MsgItem(DataItem, x, y, Game.MsgFloorItem.MsgItem.ItemType.Money, DropMoney, DynamicID, Map, UID, false, Owner.Map);
                         if (Owner.Map.EnqueueItem(DropItem))
                         {
                             DropItem.SendAll(stream,Game.MsgFloorItem.MsgDropID.Visible);

                             Money -= DropMoney;
                             SendUpdate(stream,Money, Game.MsgServer.MsgUpdate.DataType.Money);
                         }
                     }
                 }*/
                if (x > 5 && y > 5)
                {
                    var inventoryItems = Owner.Inventory.ClientItems.Values.ToArray();
                    if (inventoryItems.Length / 4 > 1)
                    {
                        uint count = (uint)Program.GetRandom.Next(1, (int)(inventoryItems.Length / 4));

                        for (int index = 0; index < count; index++)
                        {
                            try
                            {
                                if (inventoryItems.Length > index && inventoryItems[index] != null)
                                {
                                    var item = inventoryItems[index];
                                    if (item.Position == (ushort)Role.Flags.ConquerItem.AleternanteBottle || item.Position == (ushort)Role.Flags.ConquerItem.Bottle)
                                        continue;
                                    if (item.Locked == 0 && item.Inscribed == 0 && item.Bound == 0
                                        && !Database.ItemType.unabletradeitem.Contains(item.ITEM_ID) && !Database.ItemType.IsSash(item.ITEM_ID))
                                    {

                                        ushort New_X = (ushort)Program.GetRandom.Next((ushort)(x - 5), (ushort)(x + 5));
                                        ushort New_Y = (ushort)Program.GetRandom.Next((ushort)(y - 5), (ushort)(y + 5));
                                        if (Owner.Map.AddGroundItem(ref New_X, ref New_Y))
                                        {
                                            DropItem(item, New_X, New_Y, stream);
                                        }
                                    }
                                }
                            }
                            catch (Exception e) { Console.WriteLine(e.ToString()); }
                        }

                    }
                }
                if (PKPoints >= 30 && killer != null && !Program.FreePkMap.Contains(Map))
                {
                    int Count_DropItem = (PKPoints >= 30 && PKPoints <= 99) ? 1 : 2;
                    var EquipmentArray = Owner.Equipment.CurentEquip.Where(p => p != null &&
                         p.Position != (ushort)Role.Flags.ConquerItem.Bottle && p.Position != (ushort)Role.Flags.ConquerItem.AleternanteBottle
                         && p.Position != (ushort)Role.Flags.ConquerItem.Garment && p.Position != (ushort)Role.Flags.ConquerItem.AleternanteGarment
                         && p.Position != (ushort)Role.Flags.ConquerItem.Steed && p.Position != (ushort)Role.Flags.ConquerItem.SteedMount
                         && p.Position != (ushort)Role.Flags.ConquerItem.RightWeaponAccessory && p.Position != (ushort)Role.Flags.ConquerItem.LeftWeaponAccessory).ToArray();

                    if (EquipmentArray.Length > 0)
                    {
                        int trying = 0;
                        int Dropable = 0;
                        Dictionary<uint, Game.MsgServer.MsgGameItem> ItemsDrop = new Dictionary<uint, Game.MsgServer.MsgGameItem>();
                        do
                        {
                            if (trying == 14)
                                break;
                            byte ArrayPosition = (byte)Program.GetRandom.Next(0, EquipmentArray.Length);
                            var Element = EquipmentArray[ArrayPosition];
                            if (!ItemsDrop.ContainsKey(Element.UID))
                            {
                                ItemsDrop.Add(Element.UID, Element);
                                Dropable++;
                            }
                            trying++;
                        }
                        while (Dropable < Count_DropItem);

                        //remove equip item--------------


                        foreach (var item in ItemsDrop.Values)
                        {

                            Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.RemoveEquipment, item.UID, item.Position, 0, 0, 0, 0));

                            Game.MsgServer.MsgGameItem Remover;
                            Owner.Equipment.ClientItems.TryRemove(item.UID, out Remover);
                            if (item.Inscribed == 1)
                            {
                                if (MyGuild != null && MyGuild.MyArsenal != null)
                                {
                                    MyGuild.MyArsenal.Remove(Role.Instance.Guild.Arsenal.GetArsenalPosition(item.ITEM_ID), item.UID);
                                }
                                item.Inscribed = 0;
                            }
                        }
                        //checkGuildBattlePower;
                        if (MyGuild != null)
                            GuildBattlePower = MyGuild.ShareMemberPotency(GuildRank);

                        //compute status
                        Owner.Equipment.QueryEquipment(Owner.Equipment.Alternante);

                        //--------------------------------

                        //add container Item
                        foreach (var item in ItemsDrop.Values)
                            Owner.Confiscator.AddItem(Owner, killer.Owner, item, stream);
                        //-----------
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void DropItem(Game.MsgServer.MsgGameItem item, ushort x, ushort y, ServerSockets.Packet stream)
        {
            Game.MsgFloorItem.MsgItem DropItem = new Game.MsgFloorItem.MsgItem(item, x, y, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, UID, false,Owner.Map);

            if (Owner.Map.EnqueueItem(DropItem))
            {
                DropItem.SendAll(stream, Game.MsgFloorItem.MsgDropID.Visible);
                Owner.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);
            }
        }
        private void CheckPkPoints(Role.Player killer)
        {
            if (killer.OnMyOwnServer == true && OnMyOwnServer == false)
                return;
            if (Map == 3935)
                return;
            if (killer.PkMode != Flags.PKMode.Jiang)
            {
                if (!Program.FreePkMap.Contains(Map))
                {
                    if (!this.ContainFlag(Game.MsgServer.MsgUpdate.Flags.RedName) && !this.ContainFlag(Game.MsgServer.MsgUpdate.Flags.BlackName))
                    {
                        if (HeavenBlessing > 0)
                        {
                            if (killer.HeavenBlessing > 0)
                            {
                                Owner.LoseDeadExperience(killer.Owner);
                            }
                            else
                            {
                                Owner.SendSysMesage("Your Heaven Blessing takes effect! You lose no EXP!", MsgMessage.ChatMode.System);
                                //killer.AddCursed(5 * 60);
                            }
                        }
                        else
                            Owner.LoseDeadExperience(killer.Owner);
                       
                        //ckeck blessing
                       
                        //check guild lose experience 
                        /*if (MyGuild != null && MyGuildMember != null)
                        {
                            if (MyGuildMember.MoneyDonate > 100)
                            {
                                uint lose_found = (uint)Program.GetRandom.Next((int)(MyGuildMember.MoneyDonate / 10), (int)(MyGuildMember.MoneyDonate / 9));
                                MyGuildMember.MoneyDonate -= lose_found;
                                MyGuild.Info.SilverFund -= lose_found;
                                Owner.SendSysMesage("" + lose_found.ToString()+ " of experience was compensated by guild fund.", Game.MsgServer.MsgMessage.ChatMode.System, Game.MsgServer.MsgMessage.MsgColor.white);
                            }
                            else
                            {
                                uint lose_found = (uint)Program.GetRandom.Next(10000, 38000);
                                MyGuildMember.MoneyDonate -= lose_found;
                                MyGuild.Info.SilverFund -= lose_found;
                                Owner.SendSysMesage("" + lose_found.ToString() + " of experience was compensated by guild fund.", Game.MsgServer.MsgMessage.ChatMode.System, Game.MsgServer.MsgMessage.MsgColor.white);
                            }
                        }*/
                      
                     if(GetPkPkPoints)
                        {
                            if (killer.MyGuild != null)
                            {
                                if (killer.MyGuild.Enemy.ContainsKey(GuildID))
                                {
                                    killer.PKPoints += 3;
                                    if (Database.Server.MapName.ContainsKey(Map))
                                    {
                                        if (GuildRank >= Flags.GuildMemberRank.Manager)
                                            killer.MyGuild.SendMessajGuild("The (" + killer.GuildRank.ToString() + ")" + killer.Name + " at killed on (" + GuildRank.ToString() + ")" + Name + " from guild " + MyGuild.GuildName + " in " + Database.Server.MapName[Map] + "", Game.MsgServer.MsgMessage.ChatMode.Guild, Game.MsgServer.MsgMessage.MsgColor.yellow);
                                    }
                                    return;
                                }
                            }
                            if (killer.MyClan != null)
                            {
                                if (killer.MyClan.Enemy.ContainsKey(ClanUID))
                                {
                                    killer.PKPoints += 3;
                                    return;
                                }
                            }
                            if (killer.Associate.Contain(Role.Instance.Associate.Enemy, UID))
                            {
                                killer.PKPoints += 5;
                                return;
                            }
                            killer.PKPoints += 10;
                        }
                    }
                    else
                    {
                        if (PKPoints > 99)//just for black name's
                        {
                            MyKillerName = killer.Name;
                            MyKillerUID = killer.UID;
                            Owner.Teleport(39, 79, 6000,0, true);

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(Name + " has been captured by " + MyKillerName + " and sent in jail! The world is now safer!", MsgMessage.MsgColor.white, MsgMessage.ChatMode.System).GetArray(stream));
                            }
                        }
                    }
                  
                }
                
            }
        }
        public unsafe void Revive(ServerSockets.Packet stream)
        {
            ProtectAttack(5 * 1000);//5 Seconds

            HitPoints = (int)Owner.Status.MaxHitpoints;
            ClearFlags();
            TransformationID = 0;


            if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TwistofWar))
                XPCount = 0;
            SendUpdate(stream,XPCount, MsgUpdate.DataType.XPCircle);

            Stamina = 100;
            SendUpdate(stream,Stamina, MsgUpdate.DataType.Stamina);


            Send(stream.MapStatusCreate(Map, Map, (uint)Owner.Map.TypeStatus));
            View.SendView(GetArray(stream, false), false);
        }
        public Extensions.Time32 PkPointsStamp = new Extensions.Time32();
        public uint BlessTime = 0;
        public Extensions.Time32 CastPrayStamp = new Extensions.Time32();
        public Extensions.Time32 CastPrayActionsStamp = new Extensions.Time32();

        public Game.MsgServer.MsgUpdate.Flags UseXPSpell;
        public void OpenXpSkill(Game.MsgServer.MsgUpdate.Flags flag, int Timer,int StampExec= 0)
        {
            if (OnAutoHunt)
                return;
            XPCount = 0;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                SendUpdate(stream,XPCount, Game.MsgServer.MsgUpdate.DataType.XPCircle);
            }
            Game.MsgServer.MsgUpdate.Flags UseSpell = OnXPSkill();
            if (UseSpell == Game.MsgServer.MsgUpdate.Flags.Normal)
            {
                KillCounter = 0;
                UseXPSpell = flag;
                AddFlag(flag, Timer, true, StampExec);
            }
            else
            {
                if (UseSpell != flag)
                {
                    RemoveFlag(UseSpell);
                    UseXPSpell = flag;
                    AddFlag(flag, Timer, true, StampExec);
                }
                else
                {
                    if(flag == MsgUpdate.Flags.Cyclone || flag == MsgUpdate.Flags.Superman || flag == MsgUpdate.Flags.SuperCyclone)
                        UpdateFlag(flag, Timer, true, 20);
                    else
                        UpdateFlag(flag, Timer, true, 60);
                }
            }
        }
        public Game.MsgServer.MsgUpdate.Flags OnXPSkill()
        {
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Cyclone))
                return Game.MsgServer.MsgUpdate.Flags.Cyclone;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Superman))
                return Game.MsgServer.MsgUpdate.Flags.Superman;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Oblivion))
                return Game.MsgServer.MsgUpdate.Flags.Oblivion;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.FatalStrike))
                return Game.MsgServer.MsgUpdate.Flags.FatalStrike;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.ShurikenVortex))
                return Game.MsgServer.MsgUpdate.Flags.ShurikenVortex;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.ChaintBolt))
                return Game.MsgServer.MsgUpdate.Flags.ChaintBolt;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.BlackbeardsRage))
                return Game.MsgServer.MsgUpdate.Flags.BlackbeardsRage;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.CannonBarrage))
                return Game.MsgServer.MsgUpdate.Flags.CannonBarrage;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.BladeFlurry))
                return Game.MsgServer.MsgUpdate.Flags.BladeFlurry;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.SuperCyclone))
                return Game.MsgServer.MsgUpdate.Flags.SuperCyclone;
            //else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.DragonCyclone))
            //    return Game.MsgServer.MsgUpdate.Flags.DragonCyclone;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Omnipotence))
                return Game.MsgServer.MsgUpdate.Flags.Omnipotence;
            else
                return Game.MsgServer.MsgUpdate.Flags.Normal;
        }
        public void UpdateXpSkill()
        {
            if (UseXPSpell == Game.MsgServer.MsgUpdate.Flags.Cyclone
                || UseXPSpell == Game.MsgServer.MsgUpdate.Flags.SuperCyclone
                || UseXPSpell == Game.MsgServer.MsgUpdate.Flags.Superman)
            {
                if (ContainFlag(UseXPSpell))
                    UpdateFlag(UseXPSpell, 1, false, 20);
            }
        }
        public unsafe void SendScrennXPSkill(IMapObj obj)
        {
            if (OnXPSkill() != Game.MsgServer.MsgUpdate.Flags.Normal)
            {


                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    InteractQuery action = new InteractQuery()
                    {
                        UID = UID,
                        KilledMonster = true,
                        X = X,
                        Y = Y,
                        AtkType = MsgAttackPacket.AttackID.Death,
                        KillCounter = KillCounter
                    };
                    obj.Send(stream.InteractionCreate(&action));

                }
            }
        }
        public ushort KillCounter;
        ushort _xpc;
        public ushort XPCount
        {
            get { return _xpc; }
            set
            {
                _xpc = value;

                if (value == 20)
                {
                    Owner.HeroRewards.AddGoal(103);
                }
            }
        }

        public Extensions.Time32 DeadStamp = new Extensions.Time32();
        public Extensions.Time32 eventStamp = new Extensions.Time32();

        public ushort Avatar;
        public long WHMoney;
        public bool IsAsasin = false;
        public Game.MsgServer.ClientAchievement Achievement;
        public Extensions.Time32 LastWorldMessaj = new Extensions.Time32();
        public Flags.PKMode PkMode = Flags.PKMode.Capture;
        public Instance.JiangHu.AttackFlag JiangPkFlag = Instance.JiangHu.AttackFlag.None;
        public Client.GameClient Owner;
        public MapObjectType ObjType { get; set; }
        public RoleView View;
        public Instance.Quests QuestGUI;
        public unsafe void Send(ServerSockets.Packet msg)
        {
            Owner.Send(msg);
        }
        public Player(Client.GameClient _own)
        {
            AllowDynamic = false;
            this.Owner = _own;
            ObjType = MapObjectType.Player;
            View = new RoleView(Owner);
            BitVector = new StatusFlagsBigVector32(32 * 7);//6
            QuestGUI = new Instance.Quests(this);
        }
        public int Day = 0;

        public unsafe uint UID { get; set; }
        public unsafe string Name = "";
        public unsafe string ClanName = "";

        public string Spouse = "None";
        public ushort Agility;
        public ushort Vitality;
        public ushort Spirit;
        public ushort Strength;
        public ushort Atributes;
        public uint DragonIslandCollectedPoints = 0;

        byte _class;
        public unsafe byte Class
        {
            get { return _class; }
            set
            {
                _class = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream,value, Game.MsgServer.MsgUpdate.DataType.Class);
                    }
                    if (MyGuildMember != null)
                        MyGuildMember.Class = value;
                }
            }
        }
        public byte FirstRebornLevel;
        public byte SecoundeRebornLevel;

        public unsafe byte FirstClass;
        public unsafe byte SecoundeClass;

        ushort _level;
        public unsafe ushort Level
        {
            get { return _level; }
            set
            {
                if (Owner.FullLoading)
                {
                    //if (_level != 0 && Reborn == 0)
                    //{
                    //    if (_level < 15 && value >= 15)
                    //    {
                    //        using (var rec = new ServerSockets.RecycledPacket())
                    //        {
                    //            var stream = rec.GetStream();
                    //            if (Class == 160)
                    //            {
                    //                Class = 161;
                    //                SendUpdate(stream, Class, MsgUpdate.DataType.Class);

                    //            }
                    //            Owner.Inventory.Add(stream, 3005160);
                    //        }
                    //    }
                    //    if (_level < 40 && value >= 40)
                    //    {
                    //        using (var rec = new ServerSockets.RecycledPacket())
                    //        {
                    //            var stream = rec.GetStream();
                    //            Owner.Inventory.Add(stream, 3005161);
                    //            if (Database.AtributesStatus.IsWindWalker(Class))
                    //            {
                    //                if (!QuestGUI.CheckQuest(3797, MsgQuestList.QuestListItem.QuestStatus.Finished))
                    //                {
                    //                    var ActiveQuest = Database.QuestInfo.GetFinishQuest((uint)NpcID.WindwalkerGuard, Class, 3797);
                    //                    QuestGUI.Accept(ActiveQuest, 0);
                    //                }
                    //            }
                    //        }
                    //    }
                    //    if (_level < 70 && value >= 70)
                    //    {
                    //        using (var rec = new ServerSockets.RecycledPacket())
                    //        {
                    //            var stream = rec.GetStream();
                    //            Owner.Inventory.Add(stream, 3005164);
                    //        }
                    //    }
                    //    if (_level < 100 && value >= 100)
                    //    {
                    //        using (var rec = new ServerSockets.RecycledPacket())
                    //        {
                    //            var stream = rec.GetStream();
                    //            Owner.Inventory.Add(stream, 3005167);
                    //        }
                    //    }
                    //}
                    if (Database.AtributesStatus.IsTrojan(Class))
                    {
                        if (_level < 15 && value >= 15)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ScrenSword))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ScrenSword);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FastBlader))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FastBlader);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Cyclone))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Cyclone);
                            }
                        }
                        if (_level < 40 && value >= 40)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Accuracy))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Accuracy);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Golem))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Golem);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Hercules))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Hercules);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.SpiritHealing))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.SpiritHealing);

                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ScrenSword))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ScrenSword);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FastBlader))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FastBlader);
                                if (Reborn == 2 && FirstClass == 15 && SecoundeClass == 15)
                                {
                                    if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.DragonWhirl))
                                        Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.DragonWhirl);
                                }
                            }
                        }
                    }
                    else if (Database.AtributesStatus.IsWarrior(Class))
                    {
                        if (_level < 40 && value >= 40)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Dash))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Dash);
                                //if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ShieldBlock))
                                //    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ShieldBlock);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FlyingMoon))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FlyingMoon);
                                //if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.MagicDefender))
                                //    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.MagicDefender);
                                //if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.WaveofBlood))
                                //    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.WaveofBlood);
                                //if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ScarofEarth))
                                //    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ScarofEarth);
                            }
                        }
                        //if (_level < 70 && value >= 70)
                        //{
                        //    using (var rec = new ServerSockets.RecycledPacket())
                        //    {
                        //        var stream = rec.GetStream();
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.MagicDefender))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.MagicDefender);
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.DefensiveStance))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.DefensiveStance);
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TwistofWar))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.TwistofWar);
                        //    }
                        //}
                        //if (_level < 100 && value >= 100)
                        //{
                        //    using (var rec = new ServerSockets.RecycledPacket())
                        //    {
                        //        var stream = rec.GetStream();
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Backfire))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Backfire);

                        //    }
                        //}
                    }
                    else if (Database.AtributesStatus.IsArcher(Class))
                    {
                        if (_level < 23 && value >= 23)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ScatterFire))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ScatterFire);
                            }
                        }
                        //if (_level < 40 && value >= 40)
                        //{
                        //    using (var rec = new ServerSockets.RecycledPacket())
                        //    {
                        //        var stream = rec.GetStream();
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.PathOfShadow))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.PathOfShadow);
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.BladeFlurry))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.BladeFlurry);
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.MortalWound))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.MortalWound);
                        //    }
                        //}
                        if (_level < 46 && value >= 46)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.RapidFire))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.RapidFire);
                            }
                        }
                        //if (_level < 50 && value >= 50)
                        //{
                        //    using (var rec = new ServerSockets.RecycledPacket())
                        //    {
                        //        var stream = rec.GetStream();
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.KineticSpark))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.KineticSpark);
                        //    }
                        //}
                        if (_level < 70 && value >= 70)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Fly))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Fly);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ArrowRain))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ArrowRain);
                                //if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.BlisteringWave))
                                //    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.BlisteringWave);
                            }
                        }
                        if (_level < 71 && value >= 71)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Intensify))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Intensify);
                            }
                        }
                        //if (_level < 90 && value >= 90)
                        //{
                        //    using (var rec = new ServerSockets.RecycledPacket())
                        //    {
                        //        var stream = rec.GetStream();
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.SpiritFocus))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.SpiritFocus);
                        //    }
                        //}
                        //if (_level < 100 && value >= 100)
                        //{
                        //    using (var rec = new ServerSockets.RecycledPacket())
                        //    {
                        //        var stream = rec.GetStream();
                        //        if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.DaggerStorm))
                        //            Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.DaggerStorm);
                        //    }
                        //}
                    }
                    else if (Database.AtributesStatus.IsNinja(Class))
                    {
                        if (_level < 20 && value >= 20)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.MortalDrag))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.MortalDrag);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ToxicFog))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ToxicFog);
                            }
                        }
                        if (_level < 40 && value >= 40)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TwofoldBlades))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.TwofoldBlades);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.BloodyScythe))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.BloodyScythe);
                            }
                        }
                        if (_level < 70 && value >= 70)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ShurikenVortex))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ShurikenVortex);
                            }
                        }
                        if (_level < 100 && value >= 100)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ArcherBane))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ArcherBane);
                            }
                        }
                    }
                    else if (Database.AtributesStatus.IsMonk(Class))
                    {
                        if (_level < 20 && value >= 20)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TyrantAura))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.TyrantAura);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FendAura))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FendAura);
                            }
                        }
                        if (_level < 40 && value >= 40)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.RadiantPalm))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.RadiantPalm);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Serenity))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Serenity);
                            }
                        }
                        if (_level < 70 && value >= 70)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Tranquility))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Tranquility);
                            }
                        }
                        if (_level < 100 && value >= 100)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Compassion))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Compassion);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.EarthAura))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.EarthAura);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FireAura))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FireAura);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.MetalAura))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.MetalAura);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.WatherAura))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.WatherAura);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.WoodAura))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.WoodAura);
                            }
                        }
                    }
                    else if (Database.AtributesStatus.IsPirate(Class))
                    {
                        if (_level < 15 && value >= 15)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Golem))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Golem);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Windstorm))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Windstorm);
                            }
                        }
                        if (_level < 20 && value >= 20)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.GaleBomb))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.GaleBomb);
                            }
                        }
                        if (_level < 40 && value >= 40)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.AdrenalineRush))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.AdrenalineRush);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.EagleEye))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.EagleEye);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.BlackbeardsRage))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.BlackbeardsRage);
                            }
                        }
                        if (_level < 70 && value >= 70)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.KrakensRevenge))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.KrakensRevenge);
                            }
                        }
                    }
                    else if (Database.AtributesStatus.IsWindWalker(Class))
                    {
                        if (_level < 3 && value >= 3)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Omnipotence))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Omnipotence);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.JusticeChant))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.JusticeChant);
                            }
                        }
                        if (_level < 15 && value >= 15)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.SwirlingStorm))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.SwirlingStorm);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ShadowofChaser))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ShadowofChaser);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.BurntFrost))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.BurntFrost);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.HealingSnow))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.HealingSnow);
                            }
                        }
                        if (_level < 40 && value >= 40)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.RageofWar))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.RageofWar);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.HorrorofStomper))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.HorrorofStomper);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TripleBlasts))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.TripleBlasts);
                            }
                        }
                        if (_level < 70 && value >= 70)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Thundercloud))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Thundercloud);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.PeaceofStomper))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.PeaceofStomper);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ChillingSnow))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ChillingSnow);
                                
                            }
                        }
                        if (_level < 100 && value >= 100)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.RevengeTail))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.RevengeTail);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FreezingPelter))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FreezingPelter);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Thunderbolt))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Thunderbolt);
                            }
                        }

                    }
                    else if (Database.AtributesStatus.IsLee(Class))
                    {
                        if (_level < 40 && value >= 40)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.SpeedKick))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.SpeedKick);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ViolentKick))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ViolentKick);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.StormKick))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.StormKick);
                            }
                        }
                        if (_level < 70 && value >= 70)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.CrackingSwipe))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.CrackingSwipe);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.DragonRoar))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.DragonRoar);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.DragonSwing))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.DragonSwing);
                                if (Reborn == 2 && FirstClass == 85 && SecoundeClass == 85)
                                {
                                    if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.DragonFury))
                                        Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.DragonFury);
                                }
                            }
                        }
                        if (_level < 100 && value >= 100)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.DragonSlash))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.DragonSlash);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.SplittingSwipe))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.SplittingSwipe);
                            }
                        }
                    }
                    else if (Database.AtributesStatus.IsWater(Class))
                    {
                        if (_level < 40 && value >= 40)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.HealingRain))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.HealingRain);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.StarofAccuracy))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.StarofAccuracy);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Revive))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Revive);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.SpeedLightning))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.SpeedLightning);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Vulcano))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Vulcano);

                                if (Reborn == 2 && FirstClass == 135 && SecoundeClass == 135)
                                {
                                    if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.AzureShield))
                                        Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.AzureShield);
                                }

                            }
                        }
                        if (_level < 44 && value >= 44)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Meditation))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Meditation);
                            }
                        }
                        if (_level < 50 && value >= 50)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.MagicShield))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.MagicShield);
                            }
                        }
                        if (_level < 55 && value >= 55)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Stigma))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Stigma);
                            }
                        }
                        if (_level < 60 && value >= 60)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Invisibility))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Invisibility);
                            }
                        }
                        if (_level < 70 && value >= 70)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Pray))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Pray);
                            }
                        }
                        if (_level < 81 && value >= 81)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.AdvancedCure))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.AdvancedCure);
                            }
                        }
                        if (_level < 94 && value >= 94)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Nectar))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Nectar);
                            }
                        }
                    }
                    else if (Database.AtributesStatus.IsFire(Class))
                    {
                        if (_level < 40 && value >= 40)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.SpeedLightning))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.SpeedLightning);
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Vulcano))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Vulcano);

                                if (Reborn == 2 && FirstClass == 145 && SecoundeClass == 145)
                                {
                                    if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.HeavenBlade))
                                        Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.HeavenBlade);
                                }

                            }
                        }
                        if (_level < 44 && value >= 44)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Meditation))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Meditation);
                            }
                        }
                        if (_level < 52 && value >= 52)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FireMeteor))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FireMeteor);
                            }
                        }
                        if (_level < 55 && value >= 55)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FireRing))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FireRing);
                            }
                        }
                        if (_level < 65 && value >= 65)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FireCircle))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FireCircle);
                            }
                        }
                        if (_level < 82 && value >= 82)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (!Owner.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Bomb))
                                    Owner.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Bomb);
                            }
                        }
                    }
                }
                _level = value;
                if (_level >= 135)
                {
                    _level = 135;
                    Experience = 0;
                }
            }
        }
        public unsafe byte Reborn;
        public long Money;
        public uint ConquerPointsLimitaion = 0;
        public byte ConquerPointDropLimitLayer = 1;
        public DateTime ConquerPointsDropStamp = DateTime.FromBinary(0);
        public void AddDropLayer(byte layerlevel)
        {
            if (layerlevel > byte.MaxValue)
            {
                MyConsole.WriteLine("Invalid Layer Level");
                return;
            }
            ConquerPointDropLimitLayer = layerlevel;
            ConquerPointsDropStamp = DateTime.Now.AddDays(7);
            MessageBox("Congratulation, you charged " + ConquerPointDropLimitLayer.ToString() + " drop layer level will last for 7 days from now.", null, null);
        }
        uint _cps;
        public uint ConquerPoints
        {
            get { return _cps; }
            set
            {
                if (value > 1094967295)
                {
                    //banned
                    string logs = "[CallStack2]" + Name + " get " + value + " he have ConquerPoints : " + _cps + "";
                    logs += Environment.StackTrace;
                    Database.ServerDatabase.LoginQueue.Enqueue(logs);
                    Database.SystemBannedAccount.AddBan(UID, Name, 999999);
                    Owner.SendSysMesage("You Account was Banned by [PM]/[GM].");
                    Owner.Socket.Disconnect();
                }
                if (Owner.FullLoading)
                {            
                    if (value > _cps)
                    {
                        uint get_cps = value - _cps;
                        if (get_cps > 59)
                        {
                            string logs = "[CallStack]" + Name + " get " + get_cps + " he have ConquerPoints : " + _cps + "";
                          //  logs += Environment.StackTrace;
                            Database.ServerDatabase.LoginQueue.Enqueue(logs);
                        }
                    }
                    else
                    {
                        uint lost_cps = _cps - value;
                        if (lost_cps > 59)
                        {
                            string logs = "[CallStack]" + Name + " lost " + lost_cps + " he have ConquerPoints :" + _cps + "";
                           // logs += Environment.StackTrace;
                            Database.ServerDatabase.LoginQueue.Enqueue(logs);
                        }
                    }
                }
                _cps = value;
                if (Owner.FullLoading)
                {
                   
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                        stream = packet.Append(stream, MsgUpdate.DataType.ConquerPoints, value);
                        stream = packet.GetArray(stream);
                        Owner.Send(stream);
                    }
                }
            }
        }
        int _bountCps;
        public int BoundConquerPoints
        {
            get { return _bountCps; }
            set
            {
                _bountCps = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                        stream = packet.Append(stream, MsgUpdate.DataType.BoundConquerPoints, value);
                        stream = packet.GetArray(stream);
                        Owner.Send(stream);
                    }
                }
            }
        }
        public ulong Experience;
        public uint VirtutePoints;

        int _minhitpoints;
        public unsafe int HitPoints
        {
            get
            {
                if (UnlimitedArenaRooms.Maps.ContainsValue(DynamicID))
                    return (int)Owner.Status.MaxHitpoints;
                return _minhitpoints;
            }
            set
            {
                if (value > 0) DeadState = false;
                else DeadState = true;
                _minhitpoints = value;
                if (Owner.Team != null)
                {
                    var TeamMember = Owner.Team.GetMember(UID);
                    if (TeamMember != null)
                    {
                        TeamMember.Info.MaxHitpoints = (ushort)Owner.Status.MaxHitpoints;
                        TeamMember.Info.MinMHitpoints = (ushort)value;
                        Owner.Team.SendTeamInfo(TeamMember);
                    }
                }
                if (Owner.FullLoading)
                {
                    SendUpdateHP();
                }
            }
        }
        ushort _mana;
        public unsafe ushort Mana
        {
            get { return _mana; }
            set
            {
                _mana = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream,value, Game.MsgServer.MsgUpdate.DataType.Mana);
                    }
                }
            }
        }
        ushort _pkpoints;
        public ushort PKPoints
        {
            get { return _pkpoints; }
            set
            {
                _pkpoints = value;
                if (PKPoints > 99)
                {
                    RemoveFlag(Game.MsgServer.MsgUpdate.Flags.RedName);
                    AddFlag(Game.MsgServer.MsgUpdate.Flags.BlackName, StatusFlagsBigVector32.PermanentFlag, false, 6 * 60);
                }
                else if (PKPoints > 29)
                {
                    AddFlag(Game.MsgServer.MsgUpdate.Flags.RedName, StatusFlagsBigVector32.PermanentFlag, false, 6 * 60);
                    RemoveFlag(Game.MsgServer.MsgUpdate.Flags.BlackName);
                }
                else if (PKPoints < 30)
                {
                    RemoveFlag(Game.MsgServer.MsgUpdate.Flags.RedName);
                    RemoveFlag(Game.MsgServer.MsgUpdate.Flags.BlackName);
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    SendUpdate(stream,PKPoints, Game.MsgServer.MsgUpdate.DataType.PKPoints);
                }
            }
        }
        public unsafe uint QuizPoints;
        public unsafe ushort Enilghten;

        //public DateTime ExpireVip = new DateTime();
        //DateTime _ExpireVip;
        public DateTime ExpireVip = new DateTime();


        byte _viplevel;
        public byte VipLevel
        {
            get
            {
                //return 6;  
                return _viplevel;
            }
            set
            {

                _viplevel = value;
            }
        }

        public ushort EnlightenReceive;
        #region events & quests
        public bool canhelplady = false;
        public byte helpladytime = 0;
        public Extensions.Time32 StartHelpLady = new Extensions.Time32();
        #endregion
        ushort face;
        public unsafe ushort Face
        {
            get { return face; }
            set
            {
                face = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream,Mesh, Game.MsgServer.MsgUpdate.DataType.Mesh);
                    }
                }
            }
        }

        public byte HairColor
        {
            get
            {
                return (byte)(Hair / 100);
            }
            set
            {
                Hair = (ushort)((value * 100) + (Hair % 100));
            }
        }

        public unsafe ushort Hair;

        public uint PDinamycID { get; set; }
        public uint DynamicID { get; set; }

        uint _mmmap;
        public uint Map
        {
            get { return _mmmap; }
            set { _mmmap = value; }
        }


        ushort xx, yy;
        public unsafe ushort X
        {
            get { return xx; }
            set { Px = X; xx = value;  }
        }
        public unsafe ushort Y
        {
            get { return yy; }
            set { Py = Y; yy = value; }
        }

        public void ClearPreviouseCoord()
        {
            Px = 0;
            Py = 0;
        }

        public ushort Px;
        public ushort Py;
        public ushort PMapX;
        public ushort PMapY;
        public uint PMap;
        public short GetMyDistance(ushort X2, ushort Y2)
        {
            return Core.GetDistance(X, Y, X2, Y2);
        }
        public short OldGetDistance(ushort X2, ushort Y2)
        {
            return Core.GetDistance(Px, Py, X2, Y2);
        }
        public bool InView(ushort X2, ushort Y2, byte distance)
        {
      //      Console.WriteLine(Name + " " + OldGetDistance(X2, Y2) + " " + GetMyDistance(X2, Y2));
            return ((OldGetDistance(X2, Y2) > distance) && GetMyDistance(X2, Y2) <= distance);
        }

        public unsafe Flags.ConquerAngle Angle = Flags.ConquerAngle.East;
        public unsafe Flags.ConquerAction Action = Flags.ConquerAction.None;

        public byte ExpBallUsed = 0;
        public byte BDExp = 0;
        public DateTime JoinOnflineTG = new DateTime();
        public Extensions.Time32 OnlineTrainingTime = new Extensions.Time32();
        public Extensions.Time32 ReceivePointsOnlineTraining = new Extensions.Time32();
        public Extensions.Time32 HeavenBlessTime = new Extensions.Time32();
        public int HeavenBlessing = 0;
        public uint OnlineTrainingPoints = 0;
        public uint HuntingBlessing = 0;

        public uint DExpTime = 0;
        public uint RateExp = 2;

        public uint ExpProtection = 0;
        public void CreateExpProtection(ServerSockets.Packet stream, uint Time, bool uppdate =true)
        {
            if (uppdate)
                ExpProtection = Time;
            Game.MsgServer.MsgUpdate update = new Game.MsgServer.MsgUpdate(stream, UID, 1);
            stream = update.Append(stream, Game.MsgServer.MsgUpdate.DataType.ExpProtection, new uint[5] { 0, ExpProtection, 0, 0, 0 });
            stream = update.GetArray(stream);
            Owner.Send(stream);
        }
        public unsafe void CreateExtraExpPacket(ServerSockets.Packet stream)
        {
            Game.MsgServer.MsgUpdate update = new Game.MsgServer.MsgUpdate(stream,UID,1);
            stream = update.Append(stream,Game.MsgServer.MsgUpdate.DataType.DoubleExpTimer, new uint[5] { 0, DExpTime, 0, (uint)(RateExp * 100),0});
            stream = update.GetArray(stream);
            Owner.Send(stream);
        }
        public void AddHeavenBlessing(ServerSockets.Packet stream, int Time)
        {
            if (!ContainFlag(Game.MsgServer.MsgUpdate.Flags.HeavenBlessing))
                HeavenBlessTime = Extensions.Time32.Now;
#if Arabic
               if (Time > 60 * 60 * 24)
                Owner.SendSysMesage("You`ve received " + Time / (60 * 60 * 24) + " days` blessing time.", Game.MsgServer.MsgMessage.ChatMode.System);
            else
            {
                Owner.SendSysMesage("You`ve received " + (Time / 60 ) / 60 + " hours` blessing time.", Game.MsgServer.MsgMessage.ChatMode.System);
            }
#else
            if (Time > 60 * 60 * 24)
                Owner.SendSysMesage("You`ve received " + Time / (60 * 60 * 24) + " days` blessing time.", Game.MsgServer.MsgMessage.ChatMode.System);
            else
            {
                Owner.SendSysMesage("You`ve received " + (Time / 60) / 60 + " hours` blessing time.", Game.MsgServer.MsgMessage.ChatMode.System);
            }
#endif

         
            bool None = HeavenBlessing == 0;
            HeavenBlessTime = HeavenBlessTime.AddSeconds(Time);

            HeavenBlessing += Time;
            CreateHeavenBlessPacket(stream,None);

            if (MyMentor != null)
            {
                MyMentor.Mentor_Blessing += (uint)(Time / 10000);
                Role.Instance.Associate.Member mee;
                if (MyMentor.Associat.ContainsKey(Role.Instance.Associate.Apprentice))
                {
                    if (MyMentor.Associat[Role.Instance.Associate.Apprentice].TryGetValue(UID, out mee))
                    {
                        mee.Blessing += (uint)(Time / 10000);
                    }
                }
            }
           
        }
        public void CreateHeavenBlessPacket(ServerSockets.Packet stream, bool ResetOnlineTraining)
        {
            if (HeavenBlessing > 0)
            {
                if (ResetOnlineTraining)
                {
                    ReceivePointsOnlineTraining = Extensions.Time32.Now.AddMinutes(1);
                    OnlineTrainingTime = Extensions.Time32.Now.AddMinutes(10);
                }
                AddFlag(Game.MsgServer.MsgUpdate.Flags.HeavenBlessing, Role.StatusFlagsBigVector32.PermanentFlag, false);
                SendUpdate(stream, HeavenBlessing, Game.MsgServer.MsgUpdate.DataType.HeavensBlessing, false);

                SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.Show, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                if (Map == 601 || Map == 1039)
                    SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.InTraining, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { "bless" });
            }
        }
        public byte GetGender
        {
            get
            {
                if (Body % 10 >= 3)
                    return 0;
                else
                    return 1;
            }
        }
        ushort body;
        public unsafe ushort Body
        {
            get { return body; }
            set
            {
                body = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream,Mesh, Game.MsgServer.MsgUpdate.DataType.Mesh, true);
                    }
                }
            }
        }
        private ushort _transformationid;
        public unsafe ushort TransformationID
        {
            get
            {
                return _transformationid;
            }
            set
            {
                _transformationid = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream,Mesh, Game.MsgServer.MsgUpdate.DataType.Mesh, true);
                    }
                }
            }
        }
        public bool Alive { get { return HitPoints > 0; } }
        public DateTime DeathHit = DateTime.Now;

        //  uint _mesh = 0;
        public unsafe uint Mesh
        {
            get
            {
              //  if (_mesh != 0)
                //    return _mesh;
                //2471671003                     10000000
                return (uint)(TransformationID * 10000000 + Face * 10000 + Body);
            }
            //set { _mesh = value; }
        }

        public unsafe void SendUpdate(ServerSockets.Packet stream, long Value, Game.MsgServer.MsgUpdate.DataType datatype, bool scren = false)
        {
            Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
            stream = packet.Append(stream, datatype, Value);
            stream = packet.GetArray(stream);
            Owner.Send(stream);
            if (scren)
            {
                View.SendView(stream, false);
            }
        }
        
        public unsafe void SendUpdate(ServerSockets.Packet stream, Game.MsgServer.MsgUpdate.Flags Flag, uint Time, uint Dmg, uint Level, Game.MsgServer.MsgUpdate.DataType datatype, bool scren = false)
        {
            Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
            stream = packet.Append(stream, datatype, (byte)Flag, Time, Dmg, Level);
            stream = packet.GetArray(stream);
            Owner.Send(stream);
            if (scren)
                View.SendView(stream, false);
        }
        public unsafe void SendUpdate(ServerSockets.Packet stream, Game.MsgServer.MsgUpdate.DataType datatype, uint Time, uint Dmg, uint Level, bool scren = false)
        {
            Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
            stream = packet.Append(stream, datatype, (byte)0, Time, Dmg, Level);
            stream = packet.GetArray(stream);

            Owner.Send(stream);
            if (scren)
                View.SendView(stream, false);
        }
        public unsafe void SendUpdate(uint[] Value, Game.MsgServer.MsgUpdate.DataType datatype, bool scren = false)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                stream = packet.Append(stream, datatype, Value);
                stream = packet.GetArray(stream);
                Owner.Send(stream);
                if (scren)
                    View.SendView(stream, false);
            }
        }
        public unsafe void UpdateVip(ServerSockets.Packet stream )
        {
            SendUpdate(stream, VipLevel, MsgUpdate.DataType.VIPLevel, false);
            if (VipLevel == 6)
            {             
                Owner.Send(stream.VipStatusCreate(MsgVipStatus.VipFlags.FullVip));
            }
            else
                Owner.Send(stream.VipStatusCreate(MsgVipStatus.VipFlags.None));
        }
        public unsafe void SendGemString(ServerSockets.Packet stream, Game.MsgServer.MsgStringPacket.StringID id, bool SendScreen, params string[] args)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = id;
            packet.UID = UID;
            packet.Strings = args;

            if (SendScreen)
                View.SendView(stream.StringPacketCreate(packet), true, true, ShowGemEffects);
            else
                if (ShowGemEffects)
                Owner.Send(stream.StringPacketCreate(packet));
        }
        public unsafe void SendString(ServerSockets.Packet stream, Game.MsgServer.MsgStringPacket.StringID id, bool SendScreen, params string[] args)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = id;
            packet.UID = UID;
            packet.Strings = args;

            if (SendScreen)
                View.SendView(stream.StringPacketCreate(packet), true);
            else
                Owner.Send(stream.StringPacketCreate(packet));
        }
        public unsafe void SendString(ServerSockets.Packet stream, Game.MsgServer.MsgStringPacket.StringID id, uint _uid, bool SendScreen, params string[] args)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = id;
            packet.UID = _uid;
            packet.Strings = args;

            if (SendScreen)
                View.SendView(stream.StringPacketCreate(packet), true);
            else
                Owner.Send(stream.StringPacketCreate(packet));
        }

        public uint HeadId = 0;
        public uint GarmentId = 0;
        public uint ArmorId = 0;
        public uint LeftWeaponId = 0;
        public uint RightWeaponId = 0;
        public uint LeftWeaponAccessoryId = 0;
        public uint RightWeaponAccessoryId = 0;
        public uint SteedId = 0;
        public uint MountArmorId = 0;

        public ushort ColorArmor = 0;
        public ushort ColorShield = 0;
        public ushort ColorHelment = 0;

        public uint SteedPlus = 0;
        public uint SteedColor = 0;

        public uint HeadSoul = 0;
        public uint ArmorSoul = 0;
        public uint LeftWeapsonSoul = 0;
        public uint RightWeapsonSoul = 0;

        public uint WingId = 0;
        public byte WingPlus = 0;
        public uint WingProgress = 0;


        public uint RealUID = 0;

        public string UnionName = "";

        public int bigphoenix;
        public int bigape;
        public int bigbird;
        public int bigdesert;
        public int bigmystic;
        public int bigwinged;
        public int biggiant;
        public int bighill;
        public int bighawk;
        public int bigfire;
        public int bigbloody;

        public void AddMapEffect(ServerSockets.Packet stream, ushort x, ushort y, params string[] effect)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = MsgStringPacket.StringID.LocationEffect;
            packet.X = x;
            packet.Y = y;
            packet.Strings = effect;
           View.SendView(stream.StringPacketCreate(packet),true);

            /*stream.InitWriter();

            uint _uid = (uint)Program.GetRandom.Next(0, 10000);
            stream.Write(_uid);
            stream.Write(0);
            stream.Write(0);
            stream.Write(0);
            stream.Write(x);
            stream.Write(y);
            stream.Write((ushort)385);
            stream.Write((ushort)26);
            stream.Write((uint)0);
            stream.Write((uint)0);
            stream.Write((uint)0);
            stream.Write(" ");
            stream.Finalize(Game.GamePackets.SobNpcs);

            View.SendView(stream,true);
            SendString(stream, MsgStringPacket.StringID.Effect, _uid, true, effect);*/
        }

        public void ClearItemsSpawn()
        {
            HeadId = GarmentId = WingId = WingProgress = ArmorId = LeftWeaponId = RightWeaponId = LeftWeaponAccessoryId = RightWeaponAccessoryId = SteedId = MountArmorId = 0;
            ColorArmor = ColorShield = ColorHelment = 0;
            SteedPlus = SteedColor = WingPlus = 0;
            HeadSoul = ArmorSoul = LeftWeapsonSoul = RightWeapsonSoul = 0;
        }
        public unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, bool WindowsView)
        {
            //  Console.WriteLine(this.GetHashCode());
            stream.InitWriter();

            stream.Write(Extensions.Time32.Now.Value);

            stream.Write(Mesh);//(uint)(TransformationID * 10000000 + Face * 10000 + Body));

            stream.Write(UID);
            stream.Write(GuildID);
            if (Program.ServerConfig.IsInterServer == false && Owner.OnInterServer == false)
                stream.Write((ushort)GuildRank);
            else
                stream.ZeroFill(2);
      
            stream.Write((uint)0);//unknow


            for (int x = 0; x < BitVector.bits.Length; x++)
                stream.Write(BitVector.bits[x]);


            if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == Game.MsgTournaments.TournamentType.FreezeWar
                || Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == Game.MsgTournaments.TournamentType.FootBall
                || Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == Game.MsgTournaments.TournamentType.TeamDeathMatch)
            {
                if (Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(Owner))
                    stream.Write((ushort)0);
                else
                    stream.Write((ushort)AparenceType);
            }
            else
                stream.Write((ushort)AparenceType);//apparence type
            stream.Write(HeadId);
            stream.Write(GarmentId);
            stream.Write(ArmorId);
            stream.Write(LeftWeaponId);
            stream.Write(RightWeaponId);
            stream.Write(LeftWeaponAccessoryId);
            stream.Write(RightWeaponAccessoryId);
            stream.Write(SteedId);
            stream.Write(MountArmorId);
            stream.Write(WingId);
            stream.Write((byte)WingPlus);//talisman plus
            stream.Write(WingProgress);

            stream.Write((uint)0);//?? unknow

            stream.ZeroFill(6);//unknow

            stream.Write(HitPoints);
            stream.Write((ushort)0);//unknow
            stream.Write((ushort)0);//monster level

            stream.Write(X);
            stream.Write(Y);
            stream.Write(Hair);
            stream.Write((byte)Angle);
            stream.Write((uint)Action);
            stream.Write((ushort)0);//unknow
            stream.Write((byte)0);//padding?
            stream.Write(Reborn);
            stream.Write(Level);


            stream.Write((byte)(WindowsView ? 1 : 0));
            stream.Write((byte)Away);//away
            stream.Write(ExtraBattlePower);
            stream.Write((uint)0);//unknow position = 125
            stream.Write((uint)0);//unknow position = 129
            stream.Write((uint)0);//unknow p = 133;
            stream.Write((uint)(FlowerRank + 10000));
            stream.Write((uint)NobilityRank);

            stream.Write(ColorArmor);
            stream.Write(ColorShield);
            stream.Write(ColorHelment);
            stream.Write((uint)0);//quiz points
            stream.Write(SteedPlus);
            stream.Write((ushort)0);//unknow
            stream.Write(SteedColor);
            stream.Write((ushort)Enilghten);
            stream.Write((ushort)0);//merit points
            stream.Write((uint)0);//unknow
            stream.Write((uint)0);//unknow

            stream.Write(ClanUID);
            stream.Write((uint)ClanRank);


  
          

            stream.Write((uint)0);//unknow
           stream.Write((ushort)MyTitle);

 
            stream.Write((byte)0);
            stream.Write((byte)0);
            stream.Write((byte)0);
            stream.Write((byte)0);
            stream.Write((byte)0);
            stream.Write((byte)0);//15
            stream.Write((byte)0);
            stream.Write((byte)0);
            stream.Write((byte)0);

            stream.Write((byte)(Owner.IsWatching() ? 1 : 0));//1 == invisible player watcher
            stream.Write((byte)0);
            stream.Write((byte)0);
            stream.Write((byte)0);

            // stream.ZeroFill(14);
            stream.Write(HeadSoul);
            stream.Write(ArmorSoul);
            stream.Write(LeftWeapsonSoul);
            stream.Write(RightWeapsonSoul);
            stream.Write((byte)ActiveSublass);
            stream.Write(SubClassHasPoints);
            stream.Write((uint)0);//unknow
            stream.Write((ushort)FirstClass);
            stream.Write((ushort)SecoundeClass);
            stream.Write((ushort)Class);

            stream.Write((ushort)CountryID);//country
            if (Owner.Team != null)
            {
                stream.Write((uint)Owner.Team.UID);
            }
            else
                stream.Write(0);



            stream.Write(BattlePower);
            stream.Write(JiangHuTalent);
            stream.Write(JiangHuActive);

      //      stream.Write((ushort)2);
            stream.Write((byte)0);
            if (OnMyOwnServer == false)
                stream.Write(ServerID);
            else
                stream.ZeroFill(2);
            stream.Write((uint)RealUID);
            stream.Write((byte)2);//clone count 
            stream.Write((ushort)0); // clone ID
            stream.Write(0); //clone owner

            if (InUnion)
            {
                stream.Write((uint)MyUnion.UID);
                stream.Write((uint)ExploitsRank);//??
                stream.Write((uint)Role.Instance.Union.Member.GetRank(UnionMemeber.Rank));//UnionMemeber.Rank);        
                stream.Write((byte)(UnionMemeber.Rank == Instance.Union.Member.MilitaryRanks.Emperor ? 1 : 0));
                stream.Write((byte)MyUnion.IsKingdom);
            }
            else
            {
                stream.ZeroFill(4);
                stream.Write((uint)ExploitsRank);
                stream.ZeroFill(6);
            }
            stream.Write(SpecialTitleID);
            stream.Write(SpecialTitleScore);
            stream.Write(SpecialWingID);

            stream.Write((uint)MainFlag);
            stream.Write(0);

            if (OnMyOwnServer == false)
            {
                if (InUnion)
                    stream.Write(Name, string.Empty, ClanName, string.Empty, string.Empty, MyGuild != null ? MyGuild.GuildName : string.Empty, MyUnion.Name);
                else
                    stream.Write(Name, string.Empty, ClanName, string.Empty, string.Empty, MyGuild != null ? MyGuild.GuildName : string.Empty, string.Empty);
            }
            else
            {
                if (InUnion)
                    stream.Write(Name,string.Empty, ClanName, string.Empty, string.Empty, string.Empty, MyUnion.Name);
                else
                    stream.Write(Name, string.Empty, ClanName, string.Empty, string.Empty, string.Empty, string.Empty);
            }
            stream.Finalize(Game.GamePackets.SpawnPlayer);

            return stream;

        }
        public uint GetShareBattlePowers(uint target_battlepower)
        {
            return (uint)Database.TutorInfo.ShareBattle(this.Owner, (int)target_battlepower);
        }
        public bool RedTeam = false;
        public bool BlueTeam = false;
    }
  
}
