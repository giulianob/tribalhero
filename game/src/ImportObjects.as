package src 
{
	import com.greensock.loading.core.DisplayObjectLoader;
	import flash.display.Bitmap;
	import flash.display.BitmapData;
	import flash.display.DisplayObject;
	import flash.display.Sprite;
	import flash.utils.getDefinitionByName;
	import src.Util.Util;
	
	/**
	 * Required to have the compiler include these items in the final build
	 * @author Giuliano
	 */
	public class ImportObjects 
	{
		
		public function ImportObjects() 
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
			MASK_TILE;
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
			
			//Structures
            BARBARIAN_TRIBE_STRUCTURE;
			ARMORY_STRUCTURE;
			BARRACK_STRUCTURE;
			DEFAULT_BUILDING;
			DEFAULT_BUILDING_ANIM;
			DEFAULT_STRUCTURE_SIMPLE;
			DEFAULT_STRUCTURE_COMPLEX;
			EMBASSY_STRUCTURE;
			FOUNDRY_STRUCTURE;
			TRIBAL_CARNIVAL_STRUCTURE;
			TRIBAL_FAIR_STRUCTURE;
			TRIBAL_FESTIVAL_STRUCTURE;
            STRONGHOLD_STRUCTURE;
			TRIBAL_GATHERING_STRUCTURE;
			STABLE_STRUCTURE;
			FARM_STRUCTURE;
			BLACKSMITH_STRUCTURE
			MARKET_STRUCTURE;
			REFINERY_STRUCTURE;
			STABLE_STRUCTURE;
			TOWER_STRUCTURE;
			TOWNCENTER_STRUCTURE;
			TRADING_POST_STRUCTURE;
			UNIVERSITY_STRUCTURE;
			LUMBERMILL_STRUCTURE
			WORKSHOP_STRUCTURE;			
			TRAINING_GROUND_STRUCTURE;
			CANNON_TOWER_STRUCTURE;
			BLACKSMITH_STRUCTURE;
			BASEMENT_STRUCTURE;
			FOREST_CAMP_STRUCTURE;
			DEAD_FIELD_STRUCTURE;
			GRAPE_FIELD_STRUCTURE;
			FIELD_STRUCTURE;
		}
		
		public static function getInstance(spriteName: String): DisplayObject
		{
			try {
				spriteName = spriteName.replace('-', '_').toUpperCase();				
				var spriteClass: Class = ImportObjects[spriteName] as Class;
				var sprite: DisplayObject = (DisplayObject)(new spriteClass());
				(sprite as Bitmap).smoothing = true;
				sprite.scaleY = 1.001;
				return sprite;
			}
			catch (e: Error) 
			{				
			}
			
			Util.log("Could not find sprite: " + spriteName);
			return new Sprite();			
		}
		
		// Achievement Icons
		[Embed(source = "../../graphics/icons/achievements/anvil-gold.png")]		
		public static var ICON_ACHIEVEMENT_ANVIL_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/arrowshield-gold.png")]
		public static var ICON_ACHIEVEMENT_ARROWSHIELD_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/axe-gold.png")]
		public static var ICON_ACHIEVEMENT_AXE_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/bar-gold.png")]
		public static var ICON_ACHIEVEMENT_BAR_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chalice-gold.png")]
		public static var ICON_ACHIEVEMENT_CHALICE_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chest-gold.png")]
		public static var ICON_ACHIEVEMENT_CHEST_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/coins-gold.png")]
		public static var ICON_ACHIEVEMENT_COINS_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/tower-gold.png")]
		public static var ICON_ACHIEVEMENT_TOWER_GOLD: Class;
		
		[Embed(source = "../../graphics/icons/achievements/wing-man-gold.png")]
		public static var ICON_ACHIEVEMENT_WING_MAN_GOLD: Class;	
		
		[Embed(source = "../../graphics/icons/achievements/anvil-silver.png")]		
		public static var ICON_ACHIEVEMENT_ANVIL_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/arrowshield-silver.png")]
		public static var ICON_ACHIEVEMENT_ARROWSHIELD_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/axe-silver.png")]
		public static var ICON_ACHIEVEMENT_AXE_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/bar-silver.png")]
		public static var ICON_ACHIEVEMENT_BAR_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chalice-silver.png")]
		public static var ICON_ACHIEVEMENT_CHALICE_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chest-silver.png")]
		public static var ICON_ACHIEVEMENT_CHEST_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/coins-silver.png")]
		public static var ICON_ACHIEVEMENT_COINS_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/tower-silver.png")]
		public static var ICON_ACHIEVEMENT_TOWER_SILVER: Class;
		
		[Embed(source = "../../graphics/icons/achievements/wing-man-silver.png")]
		public static var ICON_ACHIEVEMENT_WING_MAN_SILVER: Class;		
		
		[Embed(source = "../../graphics/icons/achievements/anvil-bronze.png")]		
		public static var ICON_ACHIEVEMENT_ANVIL_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/arrowshield-bronze.png")]
		public static var ICON_ACHIEVEMENT_ARROWSHIELD_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/axe-bronze.png")]
		public static var ICON_ACHIEVEMENT_AXE_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/bar-bronze.png")]
		public static var ICON_ACHIEVEMENT_BAR_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chalice-bronze.png")]
		public static var ICON_ACHIEVEMENT_CHALICE_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chest-bronze.png")]
		public static var ICON_ACHIEVEMENT_CHEST_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/coins-bronze.png")]
		public static var ICON_ACHIEVEMENT_COINS_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/tower-bronze.png")]
		public static var ICON_ACHIEVEMENT_TOWER_BRONZE: Class;
		
		[Embed(source = "../../graphics/icons/achievements/wing-man-bronze.png")]
		public static var ICON_ACHIEVEMENT_WING_MAN_BRONZE: Class;	
		
		[Embed(source = "../../graphics/icons/achievements/anvil-honorary.png")]		
		public static var ICON_ACHIEVEMENT_ANVIL_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/arrowshield-honorary.png")]
		public static var ICON_ACHIEVEMENT_ARROWSHIELD_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/axe-honorary.png")]
		public static var ICON_ACHIEVEMENT_AXE_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/bar-honorary.png")]
		public static var ICON_ACHIEVEMENT_BAR_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chalice-honorary.png")]
		public static var ICON_ACHIEVEMENT_CHALICE_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/chest-honorary.png")]
		public static var ICON_ACHIEVEMENT_CHEST_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/coins-honorary.png")]
		public static var ICON_ACHIEVEMENT_COINS_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/tower-honorary.png")]
		public static var ICON_ACHIEVEMENT_TOWER_HONORARY: Class;
		
		[Embed(source = "../../graphics/icons/achievements/wing-man-honorary.png")]
		public static var ICON_ACHIEVEMENT_WING_MAN_HONORARY: Class;			
	}
	
}