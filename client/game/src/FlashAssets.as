package src 
{
    import flash.display.Bitmap;
    import flash.events.Event;
    import flash.utils.Dictionary;

    import src.Util.Util;

    public class FlashAssets
	{
		public function FlashAssets()
		{
			//Minimap Icons
            MINIMAP_HIT_AREA;
			
			//Tiles
			DOT_SPRITE;
			DOT_SPRITE_ORANGE;
			DOT_SPRITE_BLACK;
			DOT_SPRITE_WHITE;
			TILE_MINI;
		}

        private static var sharedInstanceCache: Dictionary = new Dictionary(false);

        public static function doesSpriteExist(spriteName: String): Boolean {
            spriteName = spriteName.replace('-', '_').toUpperCase();
            var spriteClass: Class = FlashAssets[spriteName] as Class;

            return spriteClass != null;
        }

		public static function getInstance(spriteName: String): Bitmap
		{
			try {
				spriteName = spriteName.replace('-', '_').toUpperCase();				
				var spriteClass: Class = FlashAssets[spriteName] as Class;
				var sprite: Bitmap = (Bitmap)(new spriteClass());
				(sprite as Bitmap).smoothing = true;
				sprite.scaleY = 1.001;

				return sprite;
			}
			catch (e: Error) 
			{

			}
			
			Util.log("Could not find sprite: " + spriteName);
			return new Bitmap();
		}

        // Returns the same instance to everyone. This is good for things like Tilesets where
        // you wont add it to the stage. An error will be thrown if you try to add it to the stage.
        public static function getSharedInstance(spriteName: String): Bitmap
        {
            var sprite: Bitmap = sharedInstanceCache[spriteName];
            if (!sprite) {
                sprite = FlashAssets.getInstance(spriteName);

                sprite.addEventListener(Event.ADDED_TO_STAGE, function(e: Event): void {
                    throw new Error("Shared instance was added to the stage.")
                });

                sharedInstanceCache[spriteName] = sprite;
            }

            return sprite;
        }

        // Tileset
        [Embed(source = "../../../graphics/TILESET.png")]
        public static const TILESET: Class;

        [Embed(source = "../../../graphics/objects/MASK_TILE.png")]
        public static const MASK_TILE: Class;

        // Icons
        [Embed(source = "../../../graphics/icons/general/ICON_COIN.png")]
        public static const ICON_COIN: Class;

        [Embed(source = "../../../graphics/icons/general/MINIMAP_LARGE_CIRCLE_SPRITE.png")]
        public static const MINIMAP_LARGE_CIRCLE_SPRITE: Class;

        [Embed(source = "../../../graphics/icons/general/MINIMAP_SMALL_CIRCLE_SPRITE.png")]
        public static const MINIMAP_SMALL_CIRCLE_SPRITE: Class;

        [Embed(source = "../../../graphics/icons/general/DOT_SPRITE.png")]
        public static const DOT_SPRITE: Class;

        // Units
        [Embed(source = "../../../graphics/units/ARCHER_DARK_UNIT.png")]
        public static const ARCHER_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/BOWMAN_DARK_UNIT.png")]
        public static const BOWMAN_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/CAVALRY_DARK_UNIT.png")]
        public static const CAVALRY_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/CATAPULT_DARK_UNIT.png")]
        public static const CATAPULT_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/FIGHTER_DARK_UNIT.png")]
        public static const FIGHTER_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/HEAVY_CAVALRY_DARK_UNIT.png")]
        public static const HEAVY_CAVALRY_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/HEAVY_SWORDSMAN_DARK_UNIT.png")]
        public static const HEAVY_SWORDSMAN_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/HELEPOLIS_DARK_UNIT.png")]
        public static const HELEPOLIS_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/PIKEMAN_DARK_UNIT.png")]
        public static const PIKEMAN_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/SWORDSMAN_DARK_UNIT.png")]
        public static const SWORDSMAN_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/WAGON_DARK_UNIT.png")]
        public static const WAGON_DARK_UNIT: Class;

        [Embed(source = "../../../graphics/units/ARCHER_UNIT.png")]
        public static const ARCHER_UNIT: Class;

        [Embed(source = "../../../graphics/units/BOWMAN_UNIT.png")]
        public static const BOWMAN_UNIT: Class;

        [Embed(source = "../../../graphics/units/CAVALRY_UNIT.png")]
        public static const CAVALRY_UNIT: Class;

        [Embed(source = "../../../graphics/units/CATAPULT_UNIT.png")]
        public static const CATAPULT_UNIT: Class;

        [Embed(source = "../../../graphics/units/FIGHTER_UNIT.png")]
        public static const FIGHTER_UNIT: Class;

        [Embed(source = "../../../graphics/units/HEAVY_CAVALRY_UNIT.png")]
        public static const HEAVY_CAVALRY_UNIT: Class;

        [Embed(source = "../../../graphics/units/HEAVY_SWORDSMAN_UNIT.png")]
        public static const HEAVY_SWORDSMAN_UNIT: Class;

        [Embed(source = "../../../graphics/units/HELEPOLIS_UNIT.png")]
        public static const HELEPOLIS_UNIT: Class;

        [Embed(source = "../../../graphics/units/PIKEMAN_UNIT.png")]
        public static const PIKEMAN_UNIT: Class;

        [Embed(source = "../../../graphics/units/SWORDSMAN_UNIT.png")]
        public static const SWORDSMAN_UNIT: Class;

        [Embed(source = "../../../graphics/units/WAGON_UNIT.png")]
        public static const WAGON_UNIT: Class;

        // General Icons

        [Embed(source = "../../../graphics/icons/general/ICON_WOOD.png")]
        public static const ICON_WOOD: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_LABOR.png")]
        public static const ICON_LABOR: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_IRON.png")]
        public static const ICON_IRON: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_GOLD.png")]
        public static const ICON_GOLD: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_CROP.png")]
        public static const ICON_CROP: Class;

        [Embed(source = "../../../graphics/icons/general/ICON_GLOBE.png")]
        public static const ICON_WORLD: Class;

        [Embed(source = "../../../graphics/icons/general/ICON_UPGRADE.png")]
        public static const ICON_UPGRADE: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_UNFRIEND.png")]
        public static const ICON_UNFRIEND: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_BELL.png")]
        public static const ICON_TOGGLE_SOUND: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_SINGLE_SWORD.png")]
        public static const ICON_SINGLE_SWORD: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_STAR.png")]
        public static const ICON_STAR: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_SHIELD.png")]
        public static const ICON_SHIELD: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_RED_X.png")]
        public static const ICON_REDX: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_QUESTION.png")]
        public static const ICON_QUESTION: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_MINIMAP_ARROW_BLUE.png")]
        public static const ICON_MINIMAP_ARROW_BLUE: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_MINIMAP_ARROW_RED.png")]
        public static const ICON_MINIMAP_ARROW_RED: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_HAMMER.png")]
        public static const ICON_HAMMER: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_STAR_HALF.png")]
        public static const ICON_HALF_STAR: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_PLUS.png")]
        public static const ICON_EXPAND: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_STAR_EMPTY.png")]
        public static const ICON_EMPTY_STAR: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_MINUS.png")]
        public static const ICON_COLLAPSE: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_CHECKMARK.png")]
        public static const ICON_CHECKMARK: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_BOOK_SMALL.png")]
        public static const ICON_BOOK: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_SWORD.png")]
        public static const ICON_BATTLE: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_ARROW_LEFT.png")]
        public static const ICON_ARROW_LEFT: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_ARROW_RIGHT.png")]
        public static const ICON_ARROW_RIGHT: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_MESSAGING.png")]
        public static const ICON_MESSAGE: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_CLOCK.png")]
        public static const ICON_CLOCK: Class;
        [Embed(source = "../../../graphics/icons/general/ICON_ALERT.png")]
        public static const ICON_ALERT: Class;

        // Achievement Icons
		[Embed(source = "../../../graphics/icons/achievements/anvil-gold.png")]		
		public static const ICON_ACHIEVEMENT_ANVIL_GOLD: Class;

		[Embed(source = "../../../graphics/icons/achievements/arrowshield-gold.png")]
		public static const ICON_ACHIEVEMENT_ARROWSHIELD_GOLD: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/axe-gold.png")]
		public static const ICON_ACHIEVEMENT_AXE_GOLD: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/bar-gold.png")]
		public static const ICON_ACHIEVEMENT_BAR_GOLD: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/chalice-gold.png")]
		public static const ICON_ACHIEVEMENT_CHALICE_GOLD: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/chest-gold.png")]
		public static const ICON_ACHIEVEMENT_CHEST_GOLD: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/coins-gold.png")]
		public static const ICON_ACHIEVEMENT_COINS_GOLD: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/tower-gold.png")]
		public static const ICON_ACHIEVEMENT_TOWER_GOLD: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/wing-man-gold.png")]
		public static const ICON_ACHIEVEMENT_WING_MAN_GOLD: Class;	
		
		[Embed(source = "../../../graphics/icons/achievements/anvil-silver.png")]		
		public static const ICON_ACHIEVEMENT_ANVIL_SILVER: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/arrowshield-silver.png")]
		public static const ICON_ACHIEVEMENT_ARROWSHIELD_SILVER: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/axe-silver.png")]
		public static const ICON_ACHIEVEMENT_AXE_SILVER: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/bar-silver.png")]
		public static const ICON_ACHIEVEMENT_BAR_SILVER: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/chalice-silver.png")]
		public static const ICON_ACHIEVEMENT_CHALICE_SILVER: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/chest-silver.png")]
		public static const ICON_ACHIEVEMENT_CHEST_SILVER: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/coins-silver.png")]
		public static const ICON_ACHIEVEMENT_COINS_SILVER: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/tower-silver.png")]
		public static const ICON_ACHIEVEMENT_TOWER_SILVER: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/wing-man-silver.png")]
		public static const ICON_ACHIEVEMENT_WING_MAN_SILVER: Class;		
		
		[Embed(source = "../../../graphics/icons/achievements/anvil-bronze.png")]		
		public static const ICON_ACHIEVEMENT_ANVIL_BRONZE: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/arrowshield-bronze.png")]
		public static const ICON_ACHIEVEMENT_ARROWSHIELD_BRONZE: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/axe-bronze.png")]
		public static const ICON_ACHIEVEMENT_AXE_BRONZE: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/bar-bronze.png")]
		public static const ICON_ACHIEVEMENT_BAR_BRONZE: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/chalice-bronze.png")]
		public static const ICON_ACHIEVEMENT_CHALICE_BRONZE: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/chest-bronze.png")]
		public static const ICON_ACHIEVEMENT_CHEST_BRONZE: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/coins-bronze.png")]
		public static const ICON_ACHIEVEMENT_COINS_BRONZE: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/tower-bronze.png")]
		public static const ICON_ACHIEVEMENT_TOWER_BRONZE: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/wing-man-bronze.png")]
		public static const ICON_ACHIEVEMENT_WING_MAN_BRONZE: Class;	
		
		[Embed(source = "../../../graphics/icons/achievements/anvil-honorary.png")]		
		public static const ICON_ACHIEVEMENT_ANVIL_HONORARY: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/arrowshield-honorary.png")]
		public static const ICON_ACHIEVEMENT_ARROWSHIELD_HONORARY: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/axe-honorary.png")]
		public static const ICON_ACHIEVEMENT_AXE_HONORARY: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/bar-honorary.png")]
		public static const ICON_ACHIEVEMENT_BAR_HONORARY: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/chalice-honorary.png")]
		public static const ICON_ACHIEVEMENT_CHALICE_HONORARY: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/chest-honorary.png")]
		public static const ICON_ACHIEVEMENT_CHEST_HONORARY: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/coins-honorary.png")]
		public static const ICON_ACHIEVEMENT_COINS_HONORARY: Class;
		
		[Embed(source = "../../../graphics/icons/achievements/tower-honorary.png")]
		public static const ICON_ACHIEVEMENT_TOWER_HONORARY: Class;

		[Embed(source = "../../../graphics/icons/achievements/wing-man-honorary.png")]
		public static const ICON_ACHIEVEMENT_WING_MAN_HONORARY: Class;

        // Forests
        [Embed(source = "../../../graphics/objects/FOREST_LVL_1.png")]
        public static const FOREST_LVL_1: Class;

        [Embed(source = "../../../graphics/objects/FOREST_LVL_2.png")]
        public static const FOREST_LVL_2: Class;

        [Embed(source = "../../../graphics/objects/FOREST_LVL_3.png")]
        public static const FOREST_LVL_3: Class;

        [Embed(source = "../../../graphics/objects/FOREST_LVL_4.png")]
        public static const FOREST_LVL_4: Class;

        // Store Achievement Icons
        [Embed(source = "../../../graphics/icons/achievements/coins-gold.png")]
        public static const GOLD_ACHIEVEMENT_THUMBNAIL: Class;

        [Embed(source = "../../../graphics/icons/achievements/coins-silver.png")]
        public static const SILVER_ACHIEVEMENT_THUMBNAIL: Class;

        [Embed(source = "../../../graphics/icons/achievements/coins-bronze.png")]
        public static const BRONZE_ACHIEVEMENT_THUMBNAIL: Class;

        // Common structures
        [Embed(source = "../../../graphics/themes/default/DEFAULT_BARBARIAN_TRIBE_STRUCTURE.png")]
        public static const DEFAULT_BARBARIAN_TRIBE_STRUCTURE: Class;

        // Default Structure Pack
        [Embed(source = "../../../graphics/themes/default/DEFAULT_WALL.png")]
        public static const DEFAULT_WALL_TILESET: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_THUMBNAIL.png")]
        public static const DEFAULT_THEME_THUMBNAIL: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BANNER.png")]
        public static const DEFAULT_THEME_BANNER: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TROOP.png")]
        public static const DEFAULT_TROOP: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_ARMORY_STRUCTURE.png")]
        public static const DEFAULT_ARMORY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_FIELD_STRUCTURE.png")]
        public static const DEFAULT_FIELD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BARRACK_STRUCTURE.png")]
        public static const DEFAULT_BARRACK_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_FOUNDATION.png")]
        public static const DEFAULT_FOUNDATION: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BUILDING_1.png")]
        public static const DEFAULT_BUILDING_1: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BUILDING_2.png")]
        public static const DEFAULT_BUILDING_2: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BUILDING_3.png")]
        public static const DEFAULT_BUILDING_3: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TRIBAL_GATHERING_STRUCTURE.png")]
        public static const DEFAULT_EMBASSY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_FOUNDRY_STRUCTURE.png")]
        public static const DEFAULT_FOUNDRY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TRIBAL_CARNIVAL_STRUCTURE.png")]
        public static const DEFAULT_TRIBAL_CARNIVAL_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TRIBAL_FAIR_STRUCTURE.png")]
        public static const DEFAULT_TRIBAL_FAIR_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TRIBAL_FESTIVAL_STRUCTURE.png")]
        public static const DEFAULT_TRIBAL_FESTIVAL_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_STRONGHOLD_STRUCTURE.png")]
        public static const DEFAULT_STRONGHOLD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TRIBAL_GATHERING_STRUCTURE.png")]
        public static const DEFAULT_TRIBAL_GATHERING_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_FARM_STRUCTURE.png")]
        public static const DEFAULT_FARM_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_MARKET_STRUCTURE.png")]
        public static const DEFAULT_MARKET_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_REFINERY_STRUCTURE.png")]
        public static const DEFAULT_REFINERY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_STABLE_STRUCTURE.png")]
        public static const DEFAULT_STABLE_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TOWER_STRUCTURE.png")]
        public static const DEFAULT_TOWER_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TOWNCENTER_STRUCTURE.png")]
        public static const DEFAULT_TOWNCENTER_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TRADING_POST_STRUCTURE.png")]
        public static const DEFAULT_TRADING_POST_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_UNIVERSITY_STRUCTURE.png")]
        public static const DEFAULT_UNIVERSITY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_LUMBERMILL_STRUCTURE.png")]
        public static const DEFAULT_LUMBERMILL_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_WORKSHOP_STRUCTURE.png")]
        public static const DEFAULT_WORKSHOP_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_TRAINING_GROUND_STRUCTURE.png")]
        public static const DEFAULT_TRAINING_GROUND_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_CANNON_TOWER_STRUCTURE.png")]
        public static const DEFAULT_CANNON_TOWER_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BLACKSMITH_STRUCTURE.png")]
        public static const DEFAULT_BLACKSMITH_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BASEMENT_STRUCTURE.png")]
        public static const DEFAULT_BASEMENT_STRUCTURE: Class;
	
        [Embed(source = "../../../graphics/themes/default/DEFAULT_BASEMENT_TMP_STRUCTURE.png")]
        public static const DEFAULT_BASEMENT_TMP_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_FOREST_CAMP_STRUCTURE.png")]
        public static const DEFAULT_FOREST_CAMP_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_DEAD_FIELD_STRUCTURE.png")]
        public static const DEFAULT_DEAD_FIELD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_GRAPE_FIELD_STRUCTURE.png")]
        public static const DEFAULT_GRAPE_FIELD_STRUCTURE: Class;

        // Pirates structure pack
        [Embed(source = "../../../graphics/themes/pirates/PIRATES_WALL.png")]
        public static const PIRATES_WALL_TILESET: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_THUMBNAIL.png")]
        public static const PIRATES_THEME_THUMBNAIL: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_BANNER.png")]
        public static const PIRATES_THEME_BANNER: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BUILDING_1.png")]
        public static const PIRATES_BUILDING_1: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BUILDING_2.png")]
        public static const PIRATES_BUILDING_2: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BUILDING_3.png")]
        public static const PIRATES_BUILDING_3: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_ARMORY_STRUCTURE.png")]
        public static const PIRATES_ARMORY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_FIELD_STRUCTURE.png")]
        public static const PIRATES_FIELD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_BARRACK_STRUCTURE.png")]
        public static const PIRATES_BARRACK_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_FOUNDRY_STRUCTURE.png")]
        public static const PIRATES_FOUNDRY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_STRONGHOLD_STRUCTURE.png")]
        public static const PIRATES_STRONGHOLD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_FARM_STRUCTURE.png")]
        public static const PIRATES_FARM_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_MARKET_STRUCTURE.png")]
        public static const PIRATES_MARKET_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_REFINERY_STRUCTURE.png")]
        public static const PIRATES_REFINERY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_STABLE_STRUCTURE.png")]
        public static const PIRATES_STABLE_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_TOWER_STRUCTURE.png")]
        public static const PIRATES_TOWER_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_TOWNCENTER_STRUCTURE.png")]
        public static const PIRATES_TOWNCENTER_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_TRADING_POST_STRUCTURE.png")]
        public static const PIRATES_TRADING_POST_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_UNIVERSITY_STRUCTURE.png")]
        public static const PIRATES_UNIVERSITY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_LUMBERMILL_STRUCTURE.png")]
        public static const PIRATES_LUMBERMILL_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_WORKSHOP_STRUCTURE.png")]
        public static const PIRATES_WORKSHOP_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_TRAINING_GROUND_STRUCTURE.png")]
        public static const PIRATES_TRAINING_GROUND_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_CANNON_TOWER_STRUCTURE.png")]
        public static const PIRATES_CANNON_TOWER_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_BLACKSMITH_STRUCTURE.png")]
        public static const PIRATES_BLACKSMITH_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_BASEMENT_STRUCTURE.png")]
        public static const PIRATES_BASEMENT_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_FOREST_CAMP_STRUCTURE.png")]
        public static const PIRATES_FOREST_CAMP_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_DEAD_FIELD_STRUCTURE.png")]
        public static const PIRATES_DEAD_FIELD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_GRAPE_FIELD_STRUCTURE.png")]
        public static const PIRATES_GRAPE_FIELD_STRUCTURE: Class;
        // Feudal Japan structure pack
        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_WALL.png")]
        public static const FEUDALJAPAN_WALL_TILESET: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_THUMBNAIL.png")]
        public static const FEUDALJAPAN_THEME_THUMBNAIL: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_BANNER.png")]
        public static const FEUDALJAPAN_THEME_BANNER: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BUILDING_1.png")]
        public static const FEUDALJAPAN_BUILDING_1: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BUILDING_2.png")]
        public static const FEUDALJAPAN_BUILDING_2: Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_BUILDING_3.png")]
        public static const FEUDALJAPAN_BUILDING_3: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_ARMORY_STRUCTURE.png")]
        public static const FEUDALJAPAN_ARMORY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_FIELD_STRUCTURE.png")]
        public static const FEUDALJAPAN_FIELD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_BARRACK_STRUCTURE.png")]
        public static const FEUDALJAPAN_BARRACK_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_FOUNDRY_STRUCTURE.png")]
        public static const FEUDALJAPAN_FOUNDRY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_STRONGHOLD_STRUCTURE.png")]
        public static const FEUDALJAPAN_STRONGHOLD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_FARM_STRUCTURE.png")]
        public static const FEUDALJAPAN_FARM_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_MARKET_STRUCTURE.png")]
        public static const FEUDALJAPAN_MARKET_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_REFINERY_STRUCTURE.png")]
        public static const FEUDALJAPAN_REFINERY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_STABLE_STRUCTURE.png")]
        public static const FEUDALJAPAN_STABLE_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_TOWER_STRUCTURE.png")]
        public static const FEUDALJAPAN_TOWER_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_TOWNCENTER_STRUCTURE.png")]
        public static const FEUDALJAPAN_TOWNCENTER_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_TRADING_POST_STRUCTURE.png")]
        public static const FEUDALJAPAN_TRADING_POST_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_UNIVERSITY_STRUCTURE.png")]
        public static const FEUDALJAPAN_UNIVERSITY_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_LUMBERMILL_STRUCTURE.png")]
        public static const FEUDALJAPAN_LUMBERMILL_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_WORKSHOP_STRUCTURE.png")]
        public static const FEUDALJAPAN_WORKSHOP_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_TRAINING_GROUND_STRUCTURE.png")]
        public static const FEUDALJAPAN_TRAINING_GROUND_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_CANNON_TOWER_STRUCTURE.png")]
        public static const FEUDALJAPAN_CANNON_TOWER_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_BLACKSMITH_STRUCTURE.png")]
        public static const FEUDALJAPAN_BLACKSMITH_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_BASEMENT_STRUCTURE.png")]
        public static const FEUDALJAPAN_BASEMENT_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_BASEMENT_TMP_STRUCTURE.png")]
        public static const FEUDALJAPAN_BASEMENT_TMP_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_FOREST_CAMP_STRUCTURE.png")]
        public static const FEUDALJAPAN_FOREST_CAMP_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_DEAD_FIELD_STRUCTURE.png")]
        public static const FEUDALJAPAN_DEAD_FIELD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_GRAPE_FIELD_STRUCTURE.png")]
        public static const FEUDALJAPAN_GRAPE_FIELD_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_TRIBAL_CARNIVAL_STRUCTURE.png")]
        public static const FEUDALJAPAN_TRIBAL_CARNIVAL_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_TRIBAL_FAIR_STRUCTURE.png")]
        public static const FEUDALJAPAN_TRIBAL_FAIR_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_TRIBAL_FESTIVAL_STRUCTURE.png")]
        public static const FEUDALJAPAN_TRIBAL_FESTIVAL_STRUCTURE: Class;

        [Embed(source = "../../../graphics/themes/feudaljapan/FEUDALJAPAN_TRIBAL_GATHERING_STRUCTURE.png")]
        public static const FEUDALJAPAN_TRIBAL_GATHERING_STRUCTURE: Class;

        /* War Elephant Theme */
        [Embed(source = "../../../graphics/themes/warelephant/WARELEPHANT_TROOP.png")]
        public static const WARELEPHANT_TROOP: Class;

        [Embed(source = "../../../graphics/themes/warelephant/WARELEPHANT_TROOP.png")]
        public static const WARELEPHANT_THEME_THUMBNAIL: Class;

        [Embed(source = "../../../graphics/themes/warelephant/WARELEPHANT_BANNER.png")]
        public static const WARELEPHANT_THEME_BANNER: Class;

        // Cobblestone road pack
        [Embed(source = "../../../graphics/themes/cobblestone/COBBLESTONE_THUMBNAIL.png")]
        public static const COBBLESTONE_THEME_THUMBNAIL: Class;

        [Embed(source = "../../../graphics/themes/cobblestone/COBBLESTONE_BANNER.png")]
        public static const COBBLESTONE_THEME_BANNER: Class;
    }
}