namespace Game.Setup {
    public enum Error {
        #region General Errors

        UNEXPECTED = -1,
        OK = 0,

        #endregion

        #region Action Errors

        ACTION_TOTAL_MAX_REACHED = 100,
        ACTION_WORKER_MAX_REACHED = 102,
        ACTION_INDEX_MAX_REACHED = 103,
        ACTION_NOT_FOUND = 104,
        ACTION_INVALID = 105,
        ACTION_ALREADY_IN_PROGRESS = 106,
        ACTION_COUNT_INVALID = 107,

        #endregion

        #region Resource Errors

        RESOURCE_NOT_ENOUGH = 200,
        RESOURCE_NOT_TRADABLE = 201,
        LABOR_NOT_ENOUGH = 202,
        LABOR_OVERFLOW = 203,

        #endregion

        #region Map Errors

        OBJECT_NOT_FOUND = 400,
        OBJECT_STRUCTURE_NOT_FOUND = 401,
        PLAYER_NOT_FOUND = 402,
        CITY_NOT_FOUND = 403,
        STRUCTURE_NOT_FOUND = 404,
        TECHNOLOGY_NOT_FOUND = 405,
        TECHNOLOGY_MAX_LEVEL_REACHED = 406,
        MAP_FULL = 410,
        TILE_MISMATCH = 411,
        STRUCTURE_EXISTS = 412,
        CITY_NAME_TAKEN = 413,
        CITY_NAME_INVALID = 414,

        #endregion

        #region Layout/Effect Requirement Errors

        LAYOUT_NOT_FULLFILLED = 250,
        EFFECT_REQUIREMENT_NOT_MET = 251,

        #endregion

        #region Market Errors            

        MARKET_PRICE_CHANGED = 300,
        MARKET_INVALID_QUANTITY = 301,

        #endregion

        #region Troop Errors

        ATTACK_SELF = 600,
        CITY_IN_BATTLE = 601,        
        DEFEND_SELF = 602,
        BATTLE_NOT_VIEWABLE = 603

        #endregion        
    }
}