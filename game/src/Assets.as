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
			
			//Walls
			WALL_NE_0;
			WALL_NW_0;
			WALL_SE_0;
			WALL_SW_0;
			WALL_N_0;
			WALL_S_0;
			WALL_W_0;
			WALL_E_0;
			WALL_NE_1;
			WALL_NW_1;
			WALL_SE_1;
			WALL_SW_1;
			WALL_N_1;
			WALL_S_1;
			WALL_W_1;
			WALL_E_1;		
			WALL_O1;
			WALL_O2;
			WALL_O3;
			WALL_O4;
			
			//Forest
			FOREST_LVL_1;
			FOREST_LVL_2;
			FOREST_LVL_3;
			FOREST_LVL_4;
			
			//Troops
			DEFAULT_TROOP;			
			
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
			
			//Dialog
			MOVEMENT_DIALOG_BG;
			
			//MOVEMENT TROOP ICONS
			TROOP_ATTACK_0;
			TROOP_ATTACK_1;
			TROOP_ATTACK_2;
			TROOP_ATTACK_3;
			TROOP_ATTACK_4;
			TROOP_IDLE_0;
			TROOP_IDLE_1;
			TROOP_IDLE_2;
			TROOP_IDLE_3;
			TROOP_IDLE_4;
			TROOP_DEFENSE_0;
			TROOP_DEFENSE_1;
			TROOP_DEFENSE_2;
			TROOP_DEFENSE_3;
			TROOP_DEFENSE_4;
		}

        private static var embedPositionCache: Dictionary;

		public static function getInstance(spriteName: String, withPosition: String = ""): Bitmap
		{
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

			try {
				spriteName = spriteName.replace('-', '_').toUpperCase();				
				var spriteClass: Class = Assets[spriteName] as Class;
				var sprite: Bitmap = (Bitmap)(new spriteClass());
				(sprite as Bitmap).smoothing = true;
				sprite.scaleY = 1.001;

                if (withPosition) {
                    var point: Point = embedPositionCache[spriteName + ":" + withPosition];
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

        // Structures
        [Embed(source = "../../graphics/buildings/TOWN_CENTER_STRUCTURE.png")]
        public static const BARBARIAN_TRIBE_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/ARMORY_STRUCTURE.png")]
        public static const ARMORY_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/FIELD_STRUCTURE.png")]
        public static const FIELD_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/BARRACK_STRUCTURE.png")]
        public static const BARRACK_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/DEFAULT_BUILDING_ANIM.png")]
        public static const DEFAULT_BUILDING: Class;

        [Embed(source = "../../graphics/buildings/DEFAULT_BUILDING_ANIM.png")]
        public static const DEFAULT_BUILDING_ANIM: Class;

        [Embed(source = "../../graphics/buildings/TRIBAL_GATHERING_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 124, y = 34)]
        public static const DEFAULT_STRUCTURE_SIMPLE: Class;

        [Embed(source = "../../graphics/buildings/TRIBAL_GATHERING_STRUCTURE.png")]
        public static const DEFAULT_STRUCTURE_COMPLEX: Class;

        [Embed(source = "../../graphics/buildings/TRIBAL_GATHERING_STRUCTURE.png")]
        public static const EMBASSY_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/FOUNDRY_STRUCTURE.png")]
        public static const FOUNDRY_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/TRIBAL_CARNIVAL_STRUCTURE.png")]
        public static const TRIBAL_CARNIVAL_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/TRIBAL_FAIR_STRUCTURE.png")]
        public static const TRIBAL_FAIR_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/TRIBAL_FESTIVAL_STRUCTURE.png")]
        public static const TRIBAL_FESTIVAL_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/TRIBAL_GATHERING_STRUCTURE.png")]
        public static const STRONGHOLD_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/TRIBAL_GATHERING_STRUCTURE.png")]
        public static const TRIBAL_GATHERING_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/FARM_STRUCTURE.png")]
        public static const FARM_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/MARKET_STRUCTURE.png")]
        public static const MARKET_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/REFINERY_STRUCTURE.png")]
        public static const REFINERY_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/STABLE_STRUCTURE.png")]
        public static const STABLE_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/TOWER_STRUCTURE.png")]
        public static const TOWER_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/TOWN_CENTER_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 124, y = 34)]
        public static const TOWNCENTER_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/TRADING_POST_STRUCTURE.png")]
        public static const TRADING_POST_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/UNIVERSITY_STRUCTURE.png")]
        [EmbedPosition(name = "map", x = 46, y = "-52")]
        public static const UNIVERSITY_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/LUMBERMILL_STRUCTURE.png")]
        public static const LUMBERMILL_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/WORKSHOP_STRUCTURE.png")]
        public static const WORKSHOP_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/TRAINING_GROUND_STRUCTURE.png")]
        public static const TRAINING_GROUND_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/CANNON_TOWER_STRUCTURE.png")]
        public static const CANNON_TOWER_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/BLACKSMITH_STRUCTURE.png")]
        public static const BLACKSMITH_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/BASEMENT_STRUCTURE.png")]
        public static const BASEMENT_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/FOREST_CAMP_STRUCTURE.png")]
        public static const FOREST_CAMP_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/DEAD_FIELD_STRUCTURE.png")]
        public static const DEAD_FIELD_STRUCTURE: Class;

        [Embed(source = "../../graphics/buildings/GRAPE_FIELD_STRUCTURE.png")]
        public static const GRAPE_FIELD_STRUCTURE: Class;
	}
}