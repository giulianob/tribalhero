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

        RoadNotAround = 420,
        RoadAlreadyExists = 421,
        RoadDestroyUniquePath = 422,

        #endregion

        #region Layout/Effect Requirement Errors

        LayoutNotFullfilled = 250,
        EffectRequirementNotMet = 251,

        #endregion

        #region Market Errors

        MarketPriceChanged = 300,
        MarketInvalidQuantity = 301,

        #endregion

        #region Troop Errors

        AttackSelf = 600,
        CityInBattle = 601,
        DefendSelf = 602,
        BattleNotViewable = 603,
        TroopChanged = 604,
        TooManyTroops = 605,

        #endregion

        #region Tribe Errors
        TribeFull = 701,
        TribeIsNull = 702,
        TribeAlreadyExists =703,
        TribeNotFound = 704,
        TribeNameInvalid = 705,
        TribesmanNotFound = 721,
        TribesmanAlreadyExists = 722,
        TribesmanIsOwner = 723,
        TribesmanAlreadyInTribe = 724,
        TribesmanNotAuthorized = 725,
        TribesmanPendingRequest = 726,
        TribesmanNoRequest = 727,

        AssignmentDone = 741,
        AssignmentNotFound = 742,
        #endregion

    }
}