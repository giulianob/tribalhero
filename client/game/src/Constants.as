package src
{
    import src.Graphics.Tileset;
    import src.Map.MiniMap.MiniMapLegend;
    import src.Objects.Store.CoinPrice;

    public class Constants
	{
        public static var storeEnabled: Boolean = false;

		public static var version: int;
		public static var revision: int;
        
        // Comes from server
        public static var motd:String = "";
        
        // Hardcoded for now
        public static var motd_helpdesk:String = 'New player? Read the <a href="http://tribalhero.wikia.com/wiki/Frequently_Asked_Questions" target="_blank">wiki FAQ</a> for answers to common questions.';
		
		public static var session: SessionVariables = new SessionVariables();

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
		public static const road_start_tile_id: int = 60;
		public static const road_end_tile_id: int = 90;

		/* MAP CONSTANTS */					
		public static const tileW:int = 160;
		public static const tileH:int = 80;

		public static const tileSetTileW:int = 12;

		public static const mapW:int = mapTileW * tileW;
		public static const mapH:int = mapTileH * tileH;

		public static const mapTileW:int = 3400;
		public static const mapTileH:int = 6200;

		public static const regionW:int = regionTileW * tileW;
		public static const regionH:int = regionTileH * tileH;

		public static const regionTileW:int = 34;
		public static const regionTileH:int = 62;

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

		public static const miniMapRegionW: int = miniMapRegionTileW * miniMapTileW;
		public static const miniMapRegionH: int = miniMapRegionTileW * miniMapTileH;

		public static const miniMapRegionTileW: int = 100;
		public static const miniMapRegionTileH: int = 100;

		public static const miniMapRegionRatioW: int = int(mapTileW / miniMapRegionTileW);
		public static const miniMapRegionRatioH: int = int(mapTileH / miniMapRegionTileH);

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
			if (x < MiniMapLegend.LEGEND_WIDTH) {
				var delta : int = MiniMapLegend.LEGEND_WIDTH - x;
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

        // Object Priority on the map when there are multiple objects on the same tile
        public static const mapObjectPriority: * = {
            stronghold: 1,
            structureObject: 2,
            barbarianTribe: 3,
            forest: 4,
            troopObject: 5,
            wallPriority: 6,
            gameObject: 7,
            simpleGameObject: 8,
            simpleObject: 9,
            displayObject: 10
        };

        public static const coinPrices: Array = [
            new CoinPrice("REFILL3", 3, 115, 0),
            new CoinPrice("REFILL6", 6, 240, 4),
            new CoinPrice("REFILL10", 10, 450, 15),
            new CoinPrice("REFILL15", 15, 700, 18),
            new CoinPrice("REFILL20", 20, 1000, 23)
        ];
	}
}

