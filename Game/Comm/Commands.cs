namespace Game.Comm
{
    public enum Command : ushort
    {
        Invalid = 1,

        SystemChat = 6,

        CmdLine = 7,

        MessageBox = 8,

        Chat = 9,

        #region Account Information

        Login = 10,

        QueryXml = 11,

        PlayerUsernameGet = 12,

        CityUsernameGet = 13,

        PlayerNameFromCityName = 14,

        TribeNameGet = 15,

        StrongholdNameGet = 16,

        PlayerProfile = 20,

        PlayerDescriptionSet = 21,

        ProfileByType = 22,

        SaveTutorialStep = 23,

        SaveMuteSound = 24,

        #endregion

        #region Action

        ActionCancel = 51,

        ActionCompleted = 52,

        ActionStarted = 53,

        ActionRescheduled = 54,

        #endregion

        #region Notification

        NotificationAdd = 61,

        NotificationRemove = 62,

        NotificationUpdate = 63,

        NotificationLocate = 64,

        MessageUnread = 65,

        BattleReportUnread = 66,

        ForumUnread = 67,

        RefreshUnread = 68,

        #endregion

        #region Reference

        ReferenceAdd = 71,

        ReferenceRemove = 72,

        #endregion

        #region Region

        RegionRoadDestroy = 102,

        RegionRoadBuild = 103,

        RegionSetTile = 104,

        RegionGet = 105,

        MiniMapRegionGet = 106,

        #endregion

        #region Road

        RoadAdd = 150,

        RoadRemove = 151,

        #endregion

        #region Object

        ObjectAdd = 201,

        ObjectUpdate = 202,

        ObjectRemove = 203,

        ObjectMove = 204,

        ObjectLocate = 210,

        #endregion

        #region Structure

        StructureInfo = 300,

        StructureBuild = 301,

        StructureUpgrade = 302,

        StructureChange = 303,

        StructureLaborMove = 304,

        StructureDowngrade = 305,

        StructureSelfDestroy = 306,

        TechAdded = 311,

        TechUpgrade = 312,

        TechRemoved = 313,

        TechUpgraded = 314,

        TechCleared = 315,

        #endregion

        #region Forest

        ForestInfo = 350,

        ForestCampCreate = 351,

        ForestCampRemove = 352,

        #endregion

        #region City

        CityObjectAdd = 451,

        CityObjectUpdate = 452,

        CityObjectRemove = 453,

        CityResourceSend = 460,

        CityResourcesUpdate = 462,

        CityUnitList = 463,

        CityLocateByName = 464,

        CityRadiusUpdate = 465,

        CityLocate = 466,

        CityPointUpdate = 467,

        CityHideNewUnitsUpdate = 468,

        CityHasApBonus = 469,

        CityBattleStarted = 490,

        CityBattleEnded = 491,

        CityNewUpdate = 497,

        CityCreate = 498,

        CityCreateInitial = 499,

        #endregion

        #region Troop

        UnitTrain = 501,

        UnitUpgrade = 502,

        UnitTemplateUpgraded = 503,

        TroopInfo = 600,

        TroopRetreat = 603,

        TroopAdded = 611,

        TroopUpdated = 612,

        TroopRemoved = 613,

        TroopAttackCity = 614,

        TroopAttackStronghold = 615,

        TroopDefendCity = 616,

        TroopDefendStronghold = 617,

        TroopAttackBarbarianTribe = 618,
		
        TroopModeSwitch = 619,

        TroopTransfer = 620,

        TroopLocalSet = 621,

        #endregion

        #region Battle

        BattleSubscribe = 700,

        BattleUnsubscribe = 701,

        BattleAttack = 702,

        BattleReinforceAttacker = 703,

        BattleReinforceDefender = 704,

        BattleEnded = 705,

        BattleSkipped = 706,

        BattleNewRound = 707,

        BattleWithdrawAttacker = 708,

        BattleWithdrawDefender = 709,

        BattleGroupUnitAdded = 710,

        BattleGroupUnitRemoved = 711,

        BattlePropertyUpdate = 712,

        #endregion

        #region Misc

        ResourceGather = 801,

        #endregion

        #region Market

        MarketBuy = 901,

        MarketSell = 902,

        MarketPrices = 903,

        #endregion

        #region Tribe

        TribeInfo = 1001,

        TribeCreate = 1002,

        TribeDelete = 1003,

        TribeUpdate = 1004,

        TribeUpgrade = 1005,

        TribeSetDescription = 1006,

        TribePublicInfo = 1007,

        TribeTransfer = 1008,

        TribeInfoByName = 1009,
        
        TribeUpdateRank = 1010,

        TribesmanRemove = 1012,

        TribesmanUpdate = 1013,

        TribesmanRequest = 1014,

        TribesmanConfirm = 1015,

        TribesmanSetRank = 1016,

        TribesmanLeave = 1017,

        TribesmanContribute = 1018,

        TribesmanKicked = 1019,

        TribeCityAssignmentCreate = 1022,

        TribeAssignmentJoin = 1023,

        TribeStrongholdAssignmentCreate = 1024,

        TribeChannelNotification = 1031,

        TribeChannelUpdate = 1051,

        TribeChannelRanksUpdate = 1052,

        #endregion

        #region Stronghold

        StrongholdInfo = 1101,

        StrongholdInfoByName = 1102,

        StrongholdLocate = 1103,

        StrongholdGateRepair = 1104,

        StrongholdLocateByName = 1105,

        StrongholdList = 1106,

        #endregion

        #region Internal Messages

        OnConnect = 10001,

        OnDisconnect = 10002,

        #endregion

        #region Testing

        PlaceObjects = 1001,

        MoveObject = 1002,

        FooRegionMoveRight = 1003,

        #endregion
    }
}