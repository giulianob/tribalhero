package src
{
import src.Graphics.Tileset;
import src.Map.CityRegionLegend;
import src.Objects.Tribe;

public class Constants
	{
		public static var version: int;
		public static var revision: int;
        
        // Comes from server
        public static var motd:String = "";
        
        // Hardcoded for now
        public static var motd_helpdesk:String = 'New player? Read the <a href="http://tribalhero.wikia.com/wiki/Frequently_Asked_Questions" target="_blank">wiki FAQ</a> for answers to common questions.';
		
		/* SESSION VARIABLES */
		public static var username: String = "1234";
		public static var hostname: String = "local.tribalhero.com";
		public static var sessionId: String;
		public static var playerName: String;
		public static var timeDelta: int;
		public static var admin: Boolean;
		public static var loginKey: String;
		public static var playerId: int;
		public static var playerHash: String;
		public static var tutorialStep: int;
		public static var tribeInviteId: int;
		public static var tribe: Tribe = new Tribe();
		public static var signupTime: Date;
		public static var newbieProtectionSeconds: int;
		public static var tribeAssignment: int;
		public static var tribeIncoming: int;
		
		/* APP CONSTANTS */
		public static var mainWebsite: String = "http://tribalhero.com/";
		public static const debug:int = 0;
        public static var debugStacktracer: Boolean = false;
		public static var defLang:String = "en";
		
		public static var alwaysEnableButtons: Boolean = false;

		public static var secondsPerUnit: Number = 1;

		/* COMM CONSTANTS */
		public static const headerSize: int = 8;

		/* ROAD CONSTANTS */
		public static const road_start_tile_id: int = 26;
		public static const road_end_tile_id: int = 41;

		/* MAP CONSTANTS */					
		public static const tileW:int = 160;
		public static const tileH:int = 80;

		public static const tileSetW:int = 2080;

		public static const tileSetTileW:int = tileSetW / tileW;

		public static const mapW:int = 3400 * tileW;
		public static const mapH:int = 6200 * tileH;

		public static const mapTileW:int = mapW / tileW;
		public static const mapTileH:int = mapH / tileH;

		public static const regionW:int = 34 * tileW;
		public static const regionH:int = 62 * tileH;

		public static const regionTileW:int = regionW / tileW;
		public static const regionTileH:int = regionH / tileH;

		public static const mapRegionW: int = mapW / regionW;
		public static const mapRegionH: int = mapH / regionH;

		public static var origScreenW:int = 976;
		public static var origScreenH:int = 640;
		
		public static var screenW:int = origScreenW;
		public static var screenH:int = origScreenH;
		
		public static const tileset: Tileset = new Tileset();
		
		public static const cityStartTile: int = 16;

		/* MINI MAP CONSTANTS */
		public static const miniMapTileW: int = 4;
		public static const miniMapTileH: int = 2;

		public static const cityRegionW: int = 100 * miniMapTileW;
		public static const cityRegionH: int = 100 * miniMapTileH;

		public static const cityRegionTileW: int = cityRegionW / miniMapTileW;
		public static const cityRegionTileH: int = cityRegionH / miniMapTileH;

		public static const miniMapRegionW: int = int(mapTileW / cityRegionTileW);
		public static const miniMapRegionH: int = int(mapTileH / cityRegionTileH);

		// Compact mini map constants
		public static const miniMapScreenW: int = 288;
		public static const miniMapScreenH: int = 138;

		public static const miniMapScreenX: Function = function(width: int) : int {
			return screenW - width - 6;
		};

		public static const miniMapScreenY: Function = function(height: int) : int {
			return screenH - height - 5;
		};

		// Expanded mini map constants
		public static const miniMapLargeScreenW: int = 800;
		public static const miniMapLargeScreenH: int = 550;

		public static const miniMapLargeScreenX: Function = function(width: int) : int {        
			var x : int = ((screenW - 0) / 2) - ((width +0) / 2);
			if (x < CityRegionLegend.LEGEND_WIDTH) {
				var delta : int = CityRegionLegend.LEGEND_WIDTH - x;
				return x - delta;
			}
			return x;
		};

		public static const miniMapLargeScreenY: Function = function(height: int) : int {
			return (screenH / 2) - (height / 2) + 30;
		};

		/* GAME DATA */
		public static const queryData: Boolean = true;

		public static const stealthRangeNames: Array = ["", "1st row", "2nd row", "3rd row", "4th row"];

		/* Ranking Types */
		public static const rankings: Array = [
			{desc: "City ranking by attack points", icon: ICON_BATTLE },
			{desc: "City ranking by defense points", icon: ICON_SHIELD },
			{desc: "City ranking by total loot stolen", icon: ICON_CROP },
			{desc: "City ranking by influence points", icon: ICON_UPGRADE },
			{desc: "Player ranking by attack points", icon: ICON_BATTLE },
			{desc: "Player ranking by defense points", icon: ICON_SHIELD },
			{desc: "Player ranking by total loot stolen", icon: ICON_CROP },
			{desc: "Player ranking by influence points", icon: ICON_UPGRADE }
		];
		
		/* Buffer for game XML data */
		public static var objData: XML = <Data></Data>;
	}
}

