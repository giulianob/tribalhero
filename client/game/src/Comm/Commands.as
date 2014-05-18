package src.Comm {
	
	public class Commands {
		public static const CHANNEL_NOTIFICATION: String = "CHANNEL_NOTIFICATION";
		
		public static const INVALID: int = 1;
		
		public static const SYSTEM_CHAT: int = 6;
		
		public static const CMD_LINE: int = 7;
		
		public static const MESSAGE_BOX: int = 8;
		public static const CHAT: int = 9;
		
		public static const LOGIN: int = 10;
		public static const QUERY_XML: int = 11;
		public static const PLAYER_USERNAME_GET: int = 12;
		public static const CITY_USERNAME_GET: int = 13;
		public static const PLAYER_NAME_FROM_CITY_NAME: int = 14;
		public static const TRIBE_USERNAME_GET: int = 15;
		public static const STRONGHOLD_USERNAME_GET: int = 16;
		
		public static const PLAYER_PROFILE: int = 20;
		public static const PLAYER_DESCRIPTION_SET: int = 21;
		public static const PROFILE_BY_TYPE: int = 22;
		public static const SAVE_TUTORIAL_STEP: int = 23;
		public static const SAVE_MUTE_SOUND: int = 24;
		public static const PLAYER_COINS_UPDATE: int = 25;
		public static const PLAYER_THEME_PURCHASED: int = 26;

		public static const ACTION_CANCEL: int = 51;
		public static const ACTION_COMPLETE: int = 52;
		public static const ACTION_START: int = 53;
		public static const ACTION_RESCHEDULE: int = 54;
		
        public static const NOTIFICATION_ADD: int = 61;
        public static const NOTIFICATION_REMOVE: int = 62;
        public static const NOTIFICATION_UPDATE: int = 63;		
		public static const NOTIFICATION_LOCATE: int = 64;
		
        public static const MESSAGE_UNREAD: int = 65;
        public static const BATTLE_REPORT_UNREAD: int = 66;
        public static const FORUM_UNREAD: int = 67;
		public static const REFRESH_UNREAD: int = 68;
		
		public static const REFERENCE_ADD: int = 71;
		public static const REFERENCE_REMOVE: int = 72;
		
		public static const REGION_ROAD_DESTROY: int = 102;
		public static const REGION_ROAD_BUILD: int = 103;
		public static const REGION_SET_TILE: int = 104;
		public static const REGION_GET: int = 105;
		public static const MINIMAP_REGION_GET: int = 106;
		
		public static const OBJECT_ADD: int = 201;
		public static const OBJECT_UPDATE: int = 202;
		public static const OBJECT_REMOVE: int = 203;
		public static const OBJECT_MOVE: int = 204;
		
		public static const STRUCTURE_INFO: int = 300;
		public static const STRUCTURE_BUILD: int = 301;
		public static const STRUCTURE_UPGRADE: int = 302;
		public static const STRUCTURE_CHANGE: int = 303;
		public static const STRUCTURE_LABOR_MOVE: int = 304;
		public static const STRUCTURE_DOWNGRADE: int = 305;
		public static const STRUCTURE_SELF_DESTROY: int = 306;
        public static const STRUCTURE_SET_THEME: int = 307;
		
		public static const TECHNOLOGY_ADDED: int = 311;
		public static const TECHNOLOGY_UPGRADE: int = 312;
		public static const TECHNOLOGY_REMOVED: int = 313;
		public static const TECHNOLOGY_UPGRADED: int = 314;
		public static const TECHNOLOGY_CLEARED: int = 315;
		
        public static const CITY_OBJECT_ADD: int  = 451;
        public static const CITY_OBJECT_UPDATE: int  = 452;
        public static const CITY_OBJECT_REMOVE: int  = 453;
		public static const CITY_RESOURCES_SEND: int = 460;
        public static const CITY_RESOURCES_UPDATE: int  = 462;
		public static const CITY_UNIT_LIST: int = 463;		
		public static const CITY_LOCATE_BY_NAME: int = 464;
		public static const CITY_RADIUS_UPDATE: int = 465;
		public static const CITY_LOCATE: int = 466;
		public static const CITY_POINTS_UPDATE: int = 467;
		public static const CITY_HIDE_NEW_UNITS_UPDATE: int = 468;
		public static const CITY_HAS_AP_BONUS: int = 469;
		public static const CITY_DEFAULT_THEME_UPDATE: int = 470;
		public static const CITY_BATTLE_STARTED: int = 490;
		public static const CITY_BATTLE_ENDED: int = 491;
		public static const CITY_NEW_UPDATE: int = 497;
		public static const CITY_CREATE: int = 498;
		public static const CITY_CREATE_INITIAL: int = 499;
		
		public static const FOREST_INFO: int = 350;
        public static const FOREST_CAMP_CREATE: int = 351;
		public static const FOREST_CAMP_REMOVE: int = 352;
		
		public static const UNIT_TRAIN: int = 501;
        public static const UNIT_UPGRADE: int = 502;
        public static const UNIT_TEMPLATE_UPGRADED: int = 503;
		
		public static const TROOP_INFO: int = 600;		
		public static const TROOP_RETREAT: int = 603;		
		public static const TROOP_ADDED: int = 611;
		public static const TROOP_UPDATED: int = 612;
		public static const TROOP_REMOVED: int = 613;
		public static const TROOP_ATTACK_CITY: int = 614;		
		public static const TROOP_ATTACK_STRONGHOLD: int = 615;
		public static const TROOP_REINFORCE_CITY: int = 616;
		public static const TROOP_REINFORCE_STRONGHOLD: int = 617;
	    public static const TROOP_ATTACK_BARBARIAN_TRIBE: int = 618;
		public static const TROOP_SWITCH_MODE: int = 619;
		public static const TROOP_TRANSFER: int = 620;
		public static const LOCAL_TROOP_MOVE: int = 621;	
		
        public static const BATTLE_SUBSCRIBE: int = 700;
        public static const BATTLE_UNSUBSCRIBE: int = 701;
        public static const BATTLE_ATTACK: int = 702;
        public static const BATTLE_REINFORCE_ATTACKER: int = 703;
        public static const BATTLE_REINFORCE_DEFENDER: int = 704;
        public static const BATTLE_ENDED: int = 705;
		public static const BATTLE_SKIPPED: int = 706;
		public static const BATTLE_NEW_ROUND: int = 707;
		public static const BATTLE_WITHDRAW_ATTACKER: int = 708;
		public static const BATTLE_WITHDRAW_DEFENDER: int = 709;
		public static const BATTLE_GROUP_UNIT_ADDED: int = 710;				
		public static const BATTLE_GROUP_UNIT_REMOVED: int = 711;		
		public static const BATTLE_PROPERTIES_UPDATED: int = 712;		
		
		public static const RESOURCE_GATHER: int = 801;
		
		public static const	MARKET_BUY: int = 901;
        public static const	MARKET_SELL: int = 902;
        public static const	MARKET_PRICES: int = 903;		
		
        public static const TRIBE_INFO: int = 1001;
        public static const TRIBE_CREATE: int = 1002;
        public static const TRIBE_DELETE: int = 1003;
        public static const TRIBE_UPDATE: int = 1004;
        public static const TRIBE_UPGRADE: int = 1005;
		public static const TRIBE_DESCRIPTION_SET: int = 1006;		
		public static const TRIBE_TRANSFER: int = 1008;
		public static const TRIBE_INFO_BY_NAME: int = 1009;
		public static const TRIBE_UPDATE_RANK: int = 1010;
        public static const TRIBESMAN_REMOVE: int = 1012;
        public static const TRIBESMAN_UPDATE: int = 1013;
        public static const TRIBESMAN_REQUEST: int = 1014;
        public static const TRIBESMAN_CONFIRM: int = 1015;
		public static const TRIBESMAN_SET_RANK: int = 1016;
		public static const TRIBESMAN_LEAVE: int = 1017;
		public static const TRIBESMAN_CONTRIBUTE: int = 1018;
		public static const TRIBESMAN_GOT_KICKED: int = 1019;
		public static const TRIBE_CITY_ASSIGNMENT_CREATE: int = 1022;
		public static const TRIBE_ASSIGNMENT_JOIN: int = 1023;
		public static const TRIBE_STRONGHOLD_ASSIGNMENT_CREATE: int = 1024;
		public static const TRIBE_UPDATE_NOTIFICATIONS: int = 1031;
		public static const TRIBE_UPDATE_CHANNEL: int = 1051;
		public static const TRIBE_RANKS_UPDATE_CHANNEL: int = 1052;
		
		public static const STRONGHOLD_INFO: int = 1101;
		public static const STRONGHOLD_PUBLIC_INFO_BY_NAME: int = 1102;
		public static const STRONGHOLD_LOCATE: int = 1103;
		public static const STRONGHOLD_GATE_REPAIR: int = 1104;
		public static const STRONGHOLD_LOCATE_BY_NAME: int = 1105;
		public static const STRONGHOLD_LIST: int = 1106;

        public static const STORE_GET_ITEMS: int = 1201;
        public static const STORE_PURCHASE_ITEM: int = 1202;
        public static const STORE_SET_DEFAULT_THEME: int = 1203;
        public static const STORE_THEME_APPLY_TO_ALL: int = 1204;
	}
}
