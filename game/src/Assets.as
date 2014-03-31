package src 
{
    import flash.display.Bitmap;
    import flash.geom.Point;
    import flash.utils.Dictionary;
    import flash.utils.describeType;

    import src.Util.Util;

    public class Assets
	{
		public function Assets()
		{					
			//Icons
			ICON_UNFRIEND;
			ICON_COLLAPSE;
			ICON_EXPAND;
			ICON_WORLD;
			ICON_CROP;
			ICON_BATTLE;
			ICON_SHIELD;
			ICON_CLOCK;
			ICON_IRON;
			ICON_GOLD;
			ICON_WOOD;
			ICON_LABOR;
			ICON_ALERT;
			ICON_MESSAGE;
			ICON_UPGRADE;
			ICON_BOOK;
			ICON_SINGLE_SWORD;
			ICON_QUESTION;
			ICON_HAMMER;
            ICON_MINIMAP_ARROW_RED;
            ICON_MINIMAP_ARROW_BLUE;
            ICON_TOGGLE_SOUND;
			
			//Minimap Icons
            MINIMAP_HIT_AREA;
			MINIMAP_TROOP_ICON;
			MINIMAP_FOREST_ICON;
            MINIMAP_STRONGHOLD_ICON;
            MINIMAP_BARBARIAN_TRIBE_ICON;
			
			//Passive Action Icons
			PASSIVE_ATTACKING;
			PASSIVE_DEFENDING;
			PASSIVE_RETREATING;
			
			//Tiles
			DOT_SPRITE;
			DOT_SPRITE_ORANGE;
			DOT_SPRITE_BLACK;
			DOT_SPRITE_WHITE;
			TILE_MINI;

			//UNITS
			DEFAULT_UNIT;
			ARCHER_DARK_UNIT;			
			BOWMAN_DARK_UNIT;			
			CAVALRY_DARK_UNIT;			
			CATAPULT_DARK_UNIT;			
			FIGHTER_DARK_UNIT;			
			HEAVY_CAVALRY_DARK_UNIT;			
			HEAVY_SWORDSMAN_DARK_UNIT;			
			HELEPOLIS_DARK_UNIT;			
			PIKEMAN_DARK_UNIT;			
			SWORDSMAN_DARK_UNIT;			
			WAGON_DARK_UNIT;			
			ARCHER_UNIT;
			BOWMAN_UNIT;
			CAVALRY_UNIT;
			CATAPULT_UNIT;
			FIGHTER_UNIT;
			HEAVY_CAVALRY_UNIT;
			HEAVY_SWORDSMAN_UNIT;
			HELEPOLIS_UNIT;
			PIKEMAN_UNIT;
			SWORDSMAN_UNIT;
			WAGON_UNIT;
		}

        private static var embedPositionCache: Dictionary;

        public static function doesSpriteExist(spriteName: String): Boolean {
            spriteName = spriteName.replace('-', '_').toUpperCase();
            var spriteClass: Class = Assets[spriteName] as Class;

            return spriteClass != null;
        }

        private static function initializeEmbedPositionCache() : void {
            if (embedPositionCache == null) {
                embedPositionCache = new Dictionary(false);

                var constants: XMLList = describeType(Assets).constant;
                var constantsLength: int = constants.length();
                for (var i: int = 0; i < constantsLength; i++) {
                    if (!constants[i].metadata) {
                        continue;
                    }

                    var embedPosition: XMLList = constants[i].metadata.(@name=="EmbedPosition");
                    var embedPositionLength: int = embedPosition.length();
                    for (var embedPositionIdx: int = 0; embedPositionIdx < embedPositionLength; embedPositionIdx++) {
                        var positionName: String = embedPosition[embedPositionIdx].arg.(@key == "name").@value;
                        embedPositionCache[constants[i].@name.toString() + ":" + positionName] = new Point(
                                Number(embedPosition[embedPositionIdx].arg.(@key == "x").@value) || 0,
                                Number(embedPosition[embedPositionIdx].arg.(@key == "y").@value) || 0
                        );
                    }
                }

            }
        }

        public static function getPosition(spriteName: String, withPosition: String): Point {
            initializeEmbedPositionCache();
            return embedPositionCache[spriteName + ":" + withPosition];
        }

		public static function getInstance(spriteName: String, withPosition: String = ""): Bitmap
		{
			try {
				spriteName = spriteName.replace('-', '_').toUpperCase();				
				var spriteClass: Class = Assets[spriteName] as Class;
				var sprite: Bitmap = (Bitmap)(new spriteClass());
				(sprite as Bitmap).smoothing = true;
				sprite.scaleY = 1.001;

                if (withPosition) {
                    var point: Point = getPosition(spriteName, withPosition);
                    if (point) {
                        sprite.x = point.x;
                        sprite.y = point.y;
                    }
                }

				return sprite;
			}
			catch (e: Error) 
			{

			}
			
			Util.log("Could not find sprite: " + spriteName);
			return new Bitmap();
		}

        // Tileset
        [Embed(source = "../../graphics/MaskTile.png")]
        public static const MASK_TILE: Class;

        // Achievement Icons
		[Embed(source = "../../graphics/icons/achievements/anvil-gold.png")]		
		public static const ICON_ACHIEVEMENT_ANVIL_GOLD: Class;

		[Embed(source = "../../graphics/icons/achievements/arrowshield-gold.png")]
		public static const ICON_ACHIEVEMENT_ARROWSHIELD_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/axe-gold.png")]
		public static const ICON_ACHIEVEMENT_AXE_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/bar-gold.png")]
		public static const ICON_ACHIEVEMENT_BAR_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chalice-gold.png")]
		public static const ICON_ACHIEVEMENT_CHALICE_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chest-gold.png")]
		public static const ICON_ACHIEVEMENT_CHEST_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/coins-gold.png")]
		public static const ICON_ACHIEVEMENT_COINS_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/tower-gold.png")]
		public static const ICON_ACHIEVEMENT_TOWER_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/wing-man-gold.png")]
		public static const ICON_ACHIEVEMENT_WING_MAN_GOLD: Class;	
		
		[Embed(source = "../../graphics/icons/achievements/anvil-silver.png")]		
		public static const ICON_ACHIEVEMENT_ANVIL_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/arrowshield-silver.png")]
		public static const ICON_ACHIEVEMENT_ARROWSHIELD_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/axe-silver.png")]
		public static const ICON_ACHIEVEMENT_AXE_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/bar-silver.png")]
		public static const ICON_ACHIEVEMENT_BAR_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chalice-silver.png")]
		public static const ICON_ACHIEVEMENT_CHALICE_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chest-silver.png")]
		public static const ICON_ACHIEVEMENT_CHEST_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/coins-silver.png")]
		public static const ICON_ACHIEVEMENT_COINS_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/tower-silver.png")]
		public static const ICON_ACHIEVEMENT_TOWER_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/wing-man-silver.png")]
		public static const ICON_ACHIEVEMENT_WING_MAN_SILVER: Class;		
		
		[Embed(source = "../../graphics/icons/achievements/anvil-bronze.png")]		
		public static const ICON_ACHIEVEMENT_ANVIL_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/arrowshield-bronze.png")]
		public static const ICON_ACHIEVEMENT_ARROWSHIELD_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/axe-bronze.png")]
		public static const ICON_ACHIEVEMENT_AXE_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/bar-bronze.png")]
		public static const ICON_ACHIEVEMENT_BAR_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chalice-bronze.png")]
		public static const ICON_ACHIEVEMENT_CHALICE_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chest-bronze.png")]
		public static const ICON_ACHIEVEMENT_CHEST_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/coins-bronze.png")]
		public static const ICON_ACHIEVEMENT_COINS_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/tower-bronze.png")]
		public static const ICON_ACHIEVEMENT_TOWER_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/wing-man-bronze.png")]
		public static const ICON_ACHIEVEMENT_WING_MAN_BRONZE: Class;	
		
		[Embed(source = "../../graphics/icons/achievements/anvil-honorary.png")]		
		public static const ICON_ACHIEVEMENT_ANVIL_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/arrowshield-honorary.png")]
		public static const ICON_ACHIEVEMENT_ARROWSHIELD_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/axe-honorary.png")]
		public static const ICON_ACHIEVEMENT_AXE_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/bar-honorary.png")]
		public static const ICON_ACHIEVEMENT_BAR_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chalice-honorary.png")]
		public static const ICON_ACHIEVEMENT_CHALICE_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chest-honorary.png")]
		public static const ICON_ACHIEVEMENT_CHEST_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/coins-honorary.png")]
		public static const ICON_ACHIEVEMENT_COINS_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/tower-honorary.png")]
		public static const ICON_ACHIEVEMENT_TOWER_HONORARY: Class;

		[Embed(source = "../../graphics/icons/achievements/wing-man-honorary.png")]
		public static const ICON_ACHIEVEMENT_WING_MAN_HONORARY: Class;

        // Forests
        [Embed(source = "../../graphics/objects/FOREST_LVL_1.png")]
        [EmbedPosition(name = "map", x = "22", y = "-4")]
        public static const FOREST_LVL_1: Class;

        [Embed(source = "../../graphics/objects/FOREST_LVL_1_SHADOW.png")]
        [EmbedPosition(name = "map", x = "36", y = "8")]
        public static const FOREST_LVL_1_SHADOW: Class;

        [Embed(source = "../../graphics/objects/FOREST_LVL_2.png")]
        [EmbedPosition(name = "map", x = "23", y = "-7")]
        public static const FOREST_LVL_2: Class;

        [Embed(source = "../../graphics/objects/FOREST_LVL_2_SHADOW.png")]
        [EmbedPosition(name = "map", x = "33", y = "-3")]
        public static const FOREST_LVL_2_SHADOW: Class;

        [Embed(source = "../../graphics/objects/FOREST_LVL_3.png")]
        [EmbedPosition(name = "map", x = "12", y = "-3")]
        public static const FOREST_LVL_3: Class;

        [Embed(source = "../../graphics/objects/FOREST_LVL_3_SHADOW.png")]
        [EmbedPosition(name = "map", x = "27", y = "-2")]
        public static const FOREST_LVL_3_SHADOW: Class;

        [Embed(source = "../../graphics/objects/FOREST_LVL_4.png")]
        [EmbedPosition(name = "map", x = "4", y = "-19")]
        public static const FOREST_LVL_4: Class;

        [Embed(source = "../../graphics/objects/FOREST_LVL_4_SHADOW.png")]
        [EmbedPosition(name = "map", x = "2", y = "-8")]
        public static const FOREST_LVL_4_SHADOW: Class;

        // Common structures
        [Embed(source = "../../graphics/themes/default/BARBARIAN_TRIBE_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "66", y = "-41")]
        public static const BARBARIAN_TRIBE_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/BARBARIAN_TRIBE_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = "73", y = "19")]
        public static const BARBARIAN_TRIBE_STRUCTURE_SHADOW: Class;

        // Default Structure Pack
        [Embed(source = "../../graphics/themes/default/TROOP.png")]
        [EmbedPosition(name = "map", x = "7", y = "2")]
        public static const DEFAULT_TROOP: Class;

        [Embed(source = "../../graphics/themes/default/TROOP_SHADOW.png")]
        [EmbedPosition(name = "map", x = "0", y = "12")]
        public static const DEFAULT_TROOP_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/ARMORY_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "15", y = "-11")]
        public static const DEFAULT_ARMORY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/ARMORY_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = "11", y = "13")]
        public static const DEFAULT_ARMORY_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/FIELD_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "25", y = "4")]
        public static const DEFAULT_FIELD_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/BARRACK_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 93, y = "-16")]
        public static const DEFAULT_BARRACK_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/BARRACK_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 84, y = "10")]
        public static const DEFAULT_BARRACK_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/FOUNDATION.png")]
        [EmbedPosition(name = "map", x = 42, y = "-24")]
        public static const DEFAULT_FOUNDATION: Class;

        [Embed(source = "../../graphics/themes/default/CONSTRUCTION_SMALL.png")]
        [EmbedPosition(name = "map", x = 32, y = "1")]
        public static const DEFAULT_BUILDING_1: Class;

        [Embed(source = "../../graphics/themes/default/CONSTRUCTION.png")]
        [EmbedPosition(name = "map", x = 102, y = "-3")]
        public static const DEFAULT_BUILDING_2: Class;

        [Embed(source = "../../graphics/themes/default/CONSTRUCTION.png")]
        [EmbedPosition(name = "map", x = 177, y = "68")]
        public static const DEFAULT_BUILDING_3: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_GATHERING_STRUCTURE.png")]
        public static const DEFAULT_STRUCTURE_SIMPLE: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_GATHERING_STRUCTURE.png")]
        public static const DEFAULT_STRUCTURE_COMPLEX: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_GATHERING_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 24, y = "6")]
        public static const DEFAULT_EMBASSY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/FOUNDRY_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 29, y = "-3")]
        public static const DEFAULT_FOUNDRY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/FOUNDRY_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 28, y = "24")]
        public static const DEFAULT_FOUNDRY_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_CARNIVAL_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 39, y = "14")]
        public static const DEFAULT_TRIBAL_CARNIVAL_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_CARNIVAL_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 39, y = "21")]
        public static const DEFAULT_TRIBAL_CARNIVAL_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_FAIR_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 38, y = "13")]
        public static const DEFAULT_TRIBAL_FAIR_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_FAIR_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 36, y = "20")]
        public static const DEFAULT_TRIBAL_FAIR_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_FESTIVAL_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 35, y = "1")]
        public static const DEFAULT_TRIBAL_FESTIVAL_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_FESTIVAL_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 34, y = "11")]
        public static const DEFAULT_TRIBAL_FESTIVAL_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/STRONGHOLD_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 26, y = "-46")]
        public static const DEFAULT_STRONGHOLD_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/STRONGHOLD_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 20, y = "20")]
        public static const DEFAULT_STRONGHOLD_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/TRIBAL_GATHERING_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 24, y = "6")]
        public static const DEFAULT_TRIBAL_GATHERING_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/FARM_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 13, y = "1")]
        public static const DEFAULT_FARM_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/FARM_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 76, y = "15")]
        public static const DEFAULT_FARM_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/MARKET_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 31, y = "6")]
        public static const DEFAULT_MARKET_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/MARKET_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 33, y = "34")]
        public static const DEFAULT_MARKET_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/REFINERY_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 28, y = "0")]
        public static const DEFAULT_REFINERY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/REFINERY_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 21, y = "25")]
        public static const DEFAULT_REFINERY_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/STABLE_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 78, y = "-26")]
        public static const DEFAULT_STABLE_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/STABLE_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 74, y = "29")]
        public static const DEFAULT_STABLE_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/TOWER_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 46, y = "-18")]
        public static const DEFAULT_TOWER_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/TOWNCENTER_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 124, y = "-50")]
        public static const DEFAULT_TOWNCENTER_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/TOWNCENTER_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = "107", y = "5")]
        public static const DEFAULT_TOWNCENTER_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/TRADING_POST_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "66", y = "-23")]
        public static const DEFAULT_TRADING_POST_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/TRADING_POST_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = "67", y = "13")]
        public static const DEFAULT_TRADING_POST_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/UNIVERSITY_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 46, y = "-52")]
        public static const DEFAULT_UNIVERSITY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/UNIVERSITY_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 33, y = "-4")]
        public static const DEFAULT_UNIVERSITY_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/LUMBERMILL_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 17, y = "-10")]
        public static const DEFAULT_LUMBERMILL_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/LUMBERMILL_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 14, y = "26")]
        public static const DEFAULT_LUMBERMILL_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/WORKSHOP_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 76, y = "-46")]
        public static const DEFAULT_WORKSHOP_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/WORKSHOP_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 70, y = "13")]
        public static const DEFAULT_WORKSHOP_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/TRAINING_GROUND_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 53, y = "-24")]
        public static const DEFAULT_TRAINING_GROUND_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/TRAINING_GROUND_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 51, y = "4")]
        public static const DEFAULT_TRAINING_GROUND_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/CANNON_TOWER_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 46, y = "-18")]
        public static const DEFAULT_CANNON_TOWER_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/BLACKSMITH_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 14, y = "-19")]
        public static const DEFAULT_BLACKSMITH_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/BLACKSMITH_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 12, y = "21")]
        public static const DEFAULT_BLACKSMITH_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/BASEMENT_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 30, y = "6")]
        public static const DEFAULT_BASEMENT_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/FOREST_CAMP_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 36, y = "4")]
        public static const DEFAULT_FOREST_CAMP_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/FOREST_CAMP_STRUCTURE_SHADOW.png")]
        [EmbedPosition(name = "map", x = 34, y = "27")]
        public static const DEFAULT_FOREST_CAMP_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/default/DEAD_FIELD_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "25", y = "4")]
        public static const DEFAULT_DEAD_FIELD_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/default/GRAPE_FIELD_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "25", y = "4")]
        public static const DEFAULT_GRAPE_FIELD_STRUCTURE: Class;

        // Default Structure Pack
//        [Embed(source = "../../graphics/themes/pirates/TROOP.png")]
//        //****[EmbedPosition(name = "map", x = "7", y = "2")]
//        public static const PIRATES_TROOP: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/TROOP_SHADOW.png")]
//        //****[EmbedPosition(name = "map", x = "0", y = "12")]
//        public static const PIRATES_TROOP_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/ARMORY_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "25", y = "-18")]
        public static const PIRATES_ARMORY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/ARMORY_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = "11", y = "13")]
        public static const PIRATES_ARMORY_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/FIELD_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "24", y = "5")]
        public static const PIRATES_FIELD_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/BARRACK_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "75", y = "-52")]
        public static const PIRATES_BARRACK_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/BARRACK_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 84, y = "10")]
        public static const PIRATES_BARRACK_STRUCTURE_SHADOW: Class;

//        [Embed(source = "../../graphics/themes/pirates/FOUNDATION.png")]
//        //****[EmbedPosition(name = "map", x = 42, y = "-24")]
//        public static const PIRATES_FOUNDATION: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/CONSTRUCTION_SMALL.png")]
//        //****[EmbedPosition(name = "map", x = 32, y = "1")]
//        public static const PIRATES_BUILDING_1: Class;

//        [Embed(source = "../../graphics/themes/pirates/CONSTRUCTION.png")]
//        //****[EmbedPosition(name = "map", x = 102, y = "-3")]
//        public static const PIRATES_BUILDING_2: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/CONSTRUCTION.png")]
//        //****[EmbedPosition(name = "map", x = 177, y = "68")]
//        public static const PIRATES_BUILDING_3: Class;

//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_GATHERING_STRUCTURE.png")]
//        public static const PIRATES_STRUCTURE_SIMPLE: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_GATHERING_STRUCTURE.png")]
//        public static const PIRATES_STRUCTURE_COMPLEX: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_GATHERING_STRUCTURE.png")]
//        //****[EmbedPosition(name = "map", x = 24, y = "6")]
//        public static const PIRATES_EMBASSY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/FOUNDRY_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "27", y = "-7")]
        public static const PIRATES_FOUNDRY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/FOUNDRY_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 28, y = "24")]
        public static const PIRATES_FOUNDRY_STRUCTURE_SHADOW: Class;

//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_CARNIVAL_STRUCTURE.png")]
//        //****[EmbedPosition(name = "map", x = 39, y = "14")]
//        public static const PIRATES_TRIBAL_CARNIVAL_STRUCTURE: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_CARNIVAL_STRUCTURE_SHADOW.png")]
//        //****[EmbedPosition(name = "map", x = 39, y = "21")]
//        public static const PIRATES_TRIBAL_CARNIVAL_STRUCTURE_SHADOW: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_FAIR_STRUCTURE.png")]
//        //****[EmbedPosition(name = "map", x = 38, y = "13")]
//        public static const PIRATES_TRIBAL_FAIR_STRUCTURE: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_FAIR_STRUCTURE_SHADOW.png")]
//        //****[EmbedPosition(name = "map", x = 36, y = "20")]
//        public static const PIRATES_TRIBAL_FAIR_STRUCTURE_SHADOW: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_FESTIVAL_STRUCTURE.png")]
//        //****[EmbedPosition(name = "map", x = 35, y = "1")]
//        public static const PIRATES_TRIBAL_FESTIVAL_STRUCTURE: Class;
//
//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_FESTIVAL_STRUCTURE_SHADOW.png")]
//        //****[EmbedPosition(name = "map", x = 34, y = "11")]
//        public static const PIRATES_TRIBAL_FESTIVAL_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/STRONGHOLD_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "117", y = "-94")]
        public static const PIRATES_STRONGHOLD_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/STRONGHOLD_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 20, y = "20")]
        public static const PIRATES_STRONGHOLD_STRUCTURE_SHADOW: Class;

//        [Embed(source = "../../graphics/themes/pirates/TRIBAL_GATHERING_STRUCTURE.png")]
//        //****[EmbedPosition(name = "map", x = 24, y = "6")]
//        public static const PIRATES_TRIBAL_GATHERING_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/FARM_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "19", y = "-17")]
        public static const PIRATES_FARM_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/FARM_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 76, y = "15")]
        public static const PIRATES_FARM_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/MARKET_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "16", y = "-11")]
        public static const PIRATES_MARKET_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/MARKET_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 33, y = "34")]
        public static const PIRATES_MARKET_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/REFINERY_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "32", y = "-24")]
        public static const PIRATES_REFINERY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/REFINERY_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 21, y = "25")]
        public static const PIRATES_REFINERY_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/STABLE_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "99", y = "-18")]
        public static const PIRATES_STABLE_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/STABLE_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 74, y = "29")]
        public static const PIRATES_STABLE_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/TOWER_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "37", y = "-41")]
        public static const PIRATES_TOWER_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/TOWNCENTER_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "80", y = "-86")]
        public static const PIRATES_TOWNCENTER_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/TOWNCENTER_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = "107", y = "5")]
        public static const PIRATES_TOWNCENTER_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/TRADING_POST_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "56", y = "-14")]
        public static const PIRATES_TRADING_POST_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/TRADING_POST_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = "67", y = "13")]
        public static const PIRATES_TRADING_POST_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/UNIVERSITY_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "101", y = "-55")]
        public static const PIRATES_UNIVERSITY_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/UNIVERSITY_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 33, y = "-4")]
        public static const PIRATES_UNIVERSITY_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/LUMBERMILL_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "11", y = "-13")]
        public static const PIRATES_LUMBERMILL_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/LUMBERMILL_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 14, y = "26")]
        public static const PIRATES_LUMBERMILL_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/WORKSHOP_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "36", y = "-76")]
        public static const PIRATES_WORKSHOP_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/WORKSHOP_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 70, y = "13")]
        public static const PIRATES_WORKSHOP_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/TRAINING_GROUND_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "74", y = "-13")]
        public static const PIRATES_TRAINING_GROUND_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/TRAINING_GROUND_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 51, y = "4")]
        public static const PIRATES_TRAINING_GROUND_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/CANNON_TOWER_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 24, y = "-40")]
        public static const PIRATES_CANNON_TOWER_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/BLACKSMITH_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "19", y = "-47")]
        public static const PIRATES_BLACKSMITH_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/BLACKSMITH_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 12, y = "21")]
        public static const PIRATES_BLACKSMITH_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/BASEMENT_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "29", y = "4")]
        public static const PIRATES_BASEMENT_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/FOREST_CAMP_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "23", y = "2")]
        public static const PIRATES_FOREST_CAMP_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/FOREST_CAMP_STRUCTURE_SHADOW.png")]
        //****[EmbedPosition(name = "map", x = 34, y = "27")]
        public static const PIRATES_FOREST_CAMP_STRUCTURE_SHADOW: Class;

        [Embed(source = "../../graphics/themes/pirates/DEAD_FIELD_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "28", y = "10")]
        public static const PIRATES_DEAD_FIELD_STRUCTURE: Class;

        [Embed(source = "../../graphics/themes/pirates/GRAPE_FIELD_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = "25", y = "7")]
        public static const PIRATES_GRAPE_FIELD_STRUCTURE: Class;
    }
}