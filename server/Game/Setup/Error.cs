namespace Game.Setup
{
    public enum Error
    {
        #region General Errors

        Unexpected = -1,

        Ok = 0,

        ClientOldVersion = 5,

        InvalidLogin = 6,

        Banned = 7,

        UnderMaintenance = 8,

        NewFeature = 9,

        #endregion

        #region Chat

        ChatFloodWarning = 20,

        ChatMessageTooLong = 21,

        ChatMuted = 22,

        ChatDisabled = 23,

        #endregion

        #region Action Errors

        ActionTotalMaxReached = 100,

        ActionWorkerMaxReached = 102,

        ActionIndexMaxReached = 103,

        ActionNotFound = 104,

        ActionInvalid = 105,

        ActionAlreadyInProgress = 106,

        ActionCountInvalid = 107,

        ActionUncancelable = 108,

        ActionSelf = 109,

        

        #endregion

        #region Resource Errors

        ResourceNotEnough = 200,

        ResourceNotTradable = 201,

        LaborNotEnough = 202,

        LaborOverflow = 203,

        ForestFull = 204,

        LumbermillUnavailable = 205,

        AlreadyInForest = 206,

        ForestInaccessible = 207,

        ForestCampMaxReached = 208,

        ResourceExceedTradeLimit = 209,

        LumbermillBusy = 210,

        ForestCampMaxLaborReached = 211,
        
        #endregion

        #region Layout/Effect Requirement Errors

        LayoutNotFullfilled = 250,

        EffectRequirementNotMet = 251,

        #endregion

        #region Market Errors

        MarketPriceChanged = 300,

        MarketInvalidQuantity = 301,

        #endregion

        #region Map Errors

        ObjectNotFound = 400,

        ObjectStructureNotFound = 401,

        PlayerNotFound = 402,

        CityNotFound = 403,

        StructureNotFound = 404,

        TechnologyNotFound = 405,

        TechnologyMaxLevelReached = 406,

        MapFull = 410,

        TileMismatch = 411,

        StructureExists = 412,

        CityNameTaken = 413,

        CityNameInvalid = 414,

        ObjectNotAttackable = 415,

        NotWithinWalls = 416,

        PlayerNewbieProtection = 417,

        StructureUndestroyable = 418,

        StructureUndowngradable = 419,

        RoadNotAround = 420,

        RoadAlreadyExists = 421,

        RoadDestroyUniquePath = 422,

        FriendMapFull = 423,

        PlayerHashNotFound = 424,

        #endregion

        #region Troop Errors

        AttackSelf = 600,

        CityInBattle = 601,

        DefendSelf = 602,

        TroopChanged = 604,

        TooManyTroops = 605,

        TroopEmpty = 606,

        TroopInBattle = 607,

        TroopNotStationed = 608,

        TroopNotStationedStronghold = 609,

        BattleNotViewable = 610,

        BattleViewableNoTroopsInBattle = 611,

        BattleViewableInRounds = 612,

        BattleViewableGateHp = 613,

        BattleViewableGateAttackingUnits = 614,

        TroopOutstanding = 615,

        TroopStationedInCity = 616,

        CityIncomingAttack = 617,

        #endregion

        #region Tribe Errors

        TribeFull = 701,

        TribeIsNull = 702,

        TribeAlreadyExists = 703,

        TribeNotFound = 704,

        TribeNameInvalid = 705,

        TribeMaxLevel = 706,

        TribeHasAssignment = 707,

        TribeDescriptionTooLong = 708,

        TribeRankNotFound = 709,

        TribeRankInvalidName = 710,

        TribesmanNotFound = 721,

        TribesmanAlreadyExists = 722,

        TribesmanIsOwner = 723,

        TribesmanAlreadyInTribe = 724,

        TribesmanNotAuthorized = 725,

        TribesmanPendingRequest = 726,

        TribesmanNoRequest = 727,

        TribesmanNotPartOfTribe = 728,

        TribeCannotRejoinYet = 729,

        TribeCannotCreateYet = 730,

        AssignmentDone = 741,

        AssignmentNotFound = 742,

        AssignmentBadTime = 743,

        AssignmentCantAttackFriend = 744,

        AssignmentUnitsTooSlow = 745,

        AssignmentNotEligible = 746,

        AssignmentTooManyInProgress = 747,

        AssignmentTooFewTroops = 748,

        #endregion

        #region Stronghold

        StrongholdNotFound = 801,

        StrongholdNotOccupied = 802,

        StrongholdCantAttackSelf = 810,

        StrongholdGateNotOpenToTribe = 811,

        StrongholdBelongsToOther = 812,

        StrongholdStillInactive = 813,

        StrongholdGateFull = 814,

        StrongholdNotRepairableInBattle = 815,

        StrongholdNotUpdatableInBattle = 816,

        #endregion

        #region Barbarian Tribes

        BarbarianTribeNoCampsRemaining = 901,
    
        #endregion

        #region Store And Themes

        ThemeNotPurchased = 10000,

        StoreItemNotFound = 10001,
        
        StoreItemAlreadyPurchased = 10002,

        StoreItemPurchaseProblem = 10003,

        PlayerBalanceNotEnough = 10004,

        #endregion

        #region
        UncancelableBarbarianTribeAttack = 11000,

        UncancelableBarbarianTribeBattle = 11001,

        UncancelableBarbarianTribeEngageAttack = 11002,

        UncancelableCityAttackChain = 11003,

        UncancelableCityBattle = 11004,

        UncancelableCityCreate = 11005,

        UncancelableCityDefenseChain = 11006,

        UncancelableCityEngageAttack = 11007,

        UncancelableCityEngageDefense = 11008,

        UncancelableCityRadiusChange = 11009,

        UncancelableCityMove = 11010,

        UncancelableCityResoureCapUpdate = 11011,

        UncancelableDistributedPointsUpdate = 11012,

        UncancelableForestCampLaborUpdate = 11013,

        UncancelableForestCampUpdate = 11014,

        UncancelablePropertyCreate = 11015,

        UncancelableResourceRateUpdate = 11016,

        UncancelableRetreatChain = 11017,

        UncancelableStrongholdAttack = 11018,

        UncancelableStrongholdDefense = 11019,

        UncancelableStarve = 11020,

        UncancelableStrongholdGateGateAttack = 11021,

        UncancelableStrongholdEngageMainAttack = 11022,

        UncancelableStrongholdGateBattle = 11023,

        UncancelableStrongholdMainBattle = 11024,

        UncancelableTroopMove = 11025,

        UncancelableTechnologyCreate = 11026,

        UncancelableForestCampBuild = 11027,

        #endregion
    }
}