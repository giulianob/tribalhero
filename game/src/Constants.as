package src
{
	public class Constants
	{
		public static var version: int = 0;
		public static var revision: int = 5;
		
		/* SESSION VARIABLES */
		public static var username: String = "1234";
		public static var hostname: String = "local.tribalhero.com";
		public static var sessionId: String;
		public static var loginKey: String;
		public static var playerId: int;

		/* APP CONSTANTS */
		public static const debug:int = 0;
		public static const defLang:String = "en";
		public static const webVersion: Boolean = true;
		
		public static const alwaysEnableButtons: Boolean = false;

		public static var secondsPerUnit: Number = 1;

		/* COMM CONSTANTS */
		public static const headerSize: int = 8;

		/* ROAD CONSTANTS */
		public static const road_start_tile_id: int = 224;
		public static const road_end_tile_id: int = 255;

		/* MAP CONSTANTS */
		public static const tileW:int = 108;
		public static const tileH:int = 54;

		public static const tileSetW:int = 1728;
		public static const tileSetH:int = 1944;

		public static const tileSetTileW:int = tileSetW / tileW;
		public static const tileSetTileH:int = tileSetH / tileH;

		public static const mapW:int = 1904 * tileW;
		public static const mapH:int = 3472 * tileH;

		public static const mapTileW:int = mapW / tileW;
		public static const mapTileH:int = mapH / tileH;

		public static const regionW:int = 34 * tileW;
		public static const regionH:int = 62 * tileH;

		public static const regionTileW:int = regionW / tileW;
		public static const regionTileH:int = regionH / tileH;

		public static const regionBitmapW: int = regionW;
		public static const regionBitmapH: int = regionH;

		public static const regionBitmapTileW: int = regionBitmapW / tileW;
		public static const regionBitmapTileH: int = regionBitmapH / tileH;

		public static const mapRegionW: int = mapW / regionW;
		public static const mapRegionH: int = mapH / regionH;

		public static var origScreenW:int = 976;
		public static var origScreenH:int = 640;
		
		public static var screenW:int = origScreenW;
		public static var screenH:int = origScreenH;

		/* MINI MAP CONSTANTS */
		public static const miniMapTileW: int = 6;
		public static const miniMapTileH: int = 3;

		public static const cityRegionW: int = 56 * miniMapTileW;
		public static const cityRegionH: int = 56 * miniMapTileH;

		public static const cityRegionTileW: int = cityRegionW / miniMapTileW;
		public static const cityRegionTileH: int = cityRegionH / miniMapTileH;

		public static const miniMapRegionW: int = int(mapTileW / cityRegionTileW);
		public static const miniMapRegionH: int = int(mapTileH / cityRegionTileH);

		public static const cityRegionBitmapW: int = cityRegionW;
		public static const cityRegionBitmapH: int = cityRegionH;

		public static const cityRegionBitmapTileW: int = cityRegionBitmapW / miniMapTileW;
		public static const cityRegionBitmapTileH: int = cityRegionBitmapH / miniMapTileH;

		// Compact mini map constants
		public static const miniMapScreenW: int = 288;
		public static const miniMapScreenH: int = 138;

		public static const miniMapScreenX: Function = function(width: int) : int {
			return 6;
		}
		public static const miniMapScreenY: Function = function(height: int) : int {
			return screenH - height - 6;
		}

		// Expanded mini map constants
		public static const miniMapLargeScreenW: int = 800;
		public static const miniMapLargeScreenH: int = 550;

		public static const miniMapLargeScreenX: Function = function(width: int) : int {
			return (screenW / 2) - (width / 2);
		}
		public static const miniMapLargeScreenY: Function = function(height: int) : int {
			return (screenH / 2) - (height / 2) + 30;
		}

		/* GAME DATA */
		public static const queryData: Boolean = true;

		/* TROOP */
		public static const troopWorkerId: int = 100;

		/* STAT RANGES */
		public static const unitStatRanges: * = {
			"attack": { min: 5, max: 100 },
			"defense": { min: 5, max: 84 },
			"stealth": { min: 0, max: 14 },
			"range": { min: 0, max: 19 },
			"speed": { min: 2, max: 22 },
			"carry": { min: 0, max: 352 }
		};

		public static const structureStatRanges: * = {
			"defense": { min: 0, max: 5520 },
			"stealth": { min: 0, max: 17 },
			"range": { min: 0, max: 11 }
		};

		public static var objData: XML = <Data></Data>;
	}
}

