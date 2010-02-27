namespace Game.Comm {
    public enum Command : ushort {
        INVALID = 1,

        #region Internal Messages

        ON_CONNECT = 10001,
        ON_DISCONNECT = 10002,

        #endregion

        #region Testing

        PLACE_OBJECTS = 1001,
        MOVE_OBJECT = 1002,
        FOO_REGION_MOVE_RIGHT = 1003,

        #endregion

        #region Account Information

        LOGIN = 10,
        QUERY_XML = 11,
        PLAYER_USERNAME_GET = 12,
        CITY_USERNAME_GET = 13,

        #endregion

        #region Action

        ACTION_CANCEL = 51,
        ACTION_COMPLETED = 52,
        ACTION_STARTED = 53,
        ACTION_RESCHEDULED = 54,

        #endregion

        #region Notification

        NOTIFICATION_ADD = 61,
        NOTIFICATION_REMOVE = 62,
        NOTIFICATION_UPDATE = 63,
        NOTIFICATION_LOCATE = 64,

        #endregion

        #region Region

        REGION_GET = 105,
        CITY_REGION_GET = 106,

        #endregion

        #region Object

        OBJECT_ADD = 201,
        OBJECT_UPDATE = 202,
        OBJECT_REMOVE = 203,
        OBJECT_MOVE = 204,

        #endregion

        #region Structure

        STRUCTURE_INFO = 300,
        STRUCTURE_BUILD = 301,
        STRUCTURE_UPGRADE = 302,
        STRUCTURE_CHANGE = 303,
        STRUCTURE_LABOR_MOVE = 304,
        TECH_ADDED = 311,
        TECH_UPGRADE = 312,
        TECH_REMOVED = 313,
        TECH_UPGRADED = 314,

        #endregion

        #region City

        CITY_OBJECT_ADD = 451,
        CITY_OBJECT_UPDATE = 452,
        CITY_OBJECT_REMOVE = 453,

        CITY_RESOURCES_UPDATE = 462,
        CITY_UNIT_LIST = 463,
        CITY_RADIUS_UPDATE = 465,

        #endregion

        #region Troop

        UNIT_TRAIN = 501,
        UNIT_UPGRADE = 502,
        UNIT_TEMPLATE_UPGRADED = 503,

        TROOP_INFO = 600,
        TROOP_ATTACK = 601,
        TROOP_DEFEND = 602,
        TROOP_RETREAT = 603,
        TROOP_ADDED = 611,
        TROOP_UPDATED = 612,
        TROOP_REMOVED = 613,

        TROOP_LOCAL_SET = 621,

        #endregion

        #region Market

        MARKET_BUY = 901,
        MARKET_SELL = 902,
        MARKET_PRICES = 903,

        #endregion

        #region Battle

        BATTLE_SUBSCRIBE = 700,
        BATTLE_UNSUBSCRIBE = 701,
        BATTLE_ATTACK = 702,
        BATTLE_REINFORCE_ATTACKER = 703,
        BATTLE_REINFORCE_DEFENDER = 704,
        BATTLE_ENDED = 705,
        BATTLE_SKIPPED = 706,

        #endregion
    }
}