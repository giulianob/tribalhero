package src
{
    import src.Map.MiniMap.MiniMapLegend;
    import src.Objects.Store.CoinPrice;

    public class Constants
	{
		public static var version: int = CONFIG::version;
		public static var revision: int = CONFIG::revision;
        
        // Comes from server
        public static var motd:String = "";
        
        // Hardcoded for now
        public static var motd_helpdesk:String = 'New player? Read the <a href="http://tribalhero.wikia.com/wiki/Frequently_Asked_Questions" target="_blank">wiki FAQ</a> for answers to common questions.';
		
		public static var session: SessionVariables = new SessionVariables();

        /* APP CONSTANTS */
		public static var mainWebsite: String = "http://tribalhero.com/";
		public static var debug:int = 1;
        public static var debugStacktracer: Boolean = false;
		public static var defLang:String = "en";    
		
		public static var alwaysEnableButtons: Boolean = false;

		public static var secondsPerUnit: Number = 1;

		/* COMM CONSTANTS */
		public static var headerSize: int = 8;

		/* ROAD CONSTANTS */
		public static var road_start_tile_id: int = 60;
		public static var road_end_tile_id: int = 126;
        public static var roadThemes: Array = ["DEFAULT", "COBBLESTONE"];

		/* MAP CONSTANTS */					
		public static var tileW:int = 160;
		public static var tileH:int = 80;

		public static var tileSetTileW:int = 12;
		public static var tileSetTileH:int = 12;

		public static var mapW:int = mapTileW * tileW;
		public static var mapH:int = mapTileH * tileH;

		public static var mapTileW:int = 3400;
		public static var mapTileH:int = 6200;

		public static var regionW:int = regionTileW * tileW;
		public static var regionH:int = regionTileH * tileH;

		public static var regionTileW:int = 34;
		public static var regionTileH:int = 62;

		public static var mapRegionW: int = mapW / regionW;
		public static var mapRegionH: int = mapH / regionH;

		public static var origScreenW:int = 976;
		public static var origScreenH:int = 640;
		
		public static var screenW:int = origScreenW;
		public static var screenH:int = origScreenH;
		
		public static var cityStartTile: int = 16;

		/* MINI MAP CONSTANTS */
		public static var miniMapTileW: int = 4;
		public static var miniMapTileH: int = 2;

		public static var miniMapRegionW: int = miniMapRegionTileW * miniMapTileW;
		public static var miniMapRegionH: int = miniMapRegionTileW * miniMapTileH;

		public static var miniMapRegionTileW: int = 100;
		public static var miniMapRegionTileH: int = 100;

		public static var miniMapRegionRatioW: int = int(mapTileW / miniMapRegionTileW);
		public static var miniMapRegionRatioH: int = int(mapTileH / miniMapRegionTileH);

		// Compact mini map constants
		public static var miniMapScreenW: int = 288;
		public static var miniMapScreenH: int = 138;

		public static var miniMapScreenX: Function = function(width: int) : int {
			return screenW - width - 6;
		};

		public static var miniMapScreenY: Function = function(height: int) : int {
			return screenH - height - 5;
		};

		// Expanded mini map constants
		public static var miniMapLargeScreenW: int = 800;
		public static var miniMapLargeScreenH: int = 550;

		public static var miniMapLargeScreenX: Function = function(width: int) : int {        
			var x : int = ((screenW - 0) / 2) - ((width +0) / 2);
			if (x < MiniMapLegend.LEGEND_WIDTH) {
				var delta : int = MiniMapLegend.LEGEND_WIDTH - x;
				return x - delta;
			}
			return x;
		};

		public static var miniMapLargeScreenY: Function = function(height: int) : int {
			return (screenH / 2) - (height / 2) + 30;
		};

		public static var stealthRangeNames: Array = ["", "1st row", "2nd row", "3rd row", "4th row"];

		/* Ranking Types */
		public static var rankings: Array = [
			{desc: "City ranking by attack points", icon: "ICON_BATTLE" },
			{desc: "City ranking by defense points", icon: "ICON_SHIELD" },
			{desc: "City ranking by total loot stolen", icon: "ICON_CROP" },
			{desc: "City ranking by influence points", icon: "ICON_UPGRADE" },
			{desc: "City ranking by most expensive", icon: "ICON_GOLD" },
			{desc: "Player ranking by attack points", icon: "ICON_BATTLE" },
			{desc: "Player ranking by defense points", icon: "ICON_SHIELD" },
			{desc: "Player ranking by total loot stolen", icon: "ICON_CROP" },
			{desc: "Player ranking by influence points", icon: "ICON_UPGRADE" }
		];
		
		/* Buffer for game XML data */
		public static var objData: XML = <Data></Data>;

        // Object Priority on the map when there are multiple objects on the same tile
        public static var mapObjectPriority: * = {
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

        public static var coinPrices: Array = [
            new CoinPrice("REFILL3", 3, 115, 0),
            new CoinPrice("REFILL6", 6, 240, 4),
            new CoinPrice("REFILL10", 10, 450, 15),
            new CoinPrice("REFILL15", 15, 700, 18),
            new CoinPrice("REFILL20", 20, 1000, 23)
        ];

        public static function updateContentScale(scale: Number): void {
            tileW *= scale;
            tileH *= scale;
            mapW *= scale;
            mapH *= scale;

            mapRegionW *= scale;
            mapRegionH *= scale;            
            regionW *= scale;
            regionH *= scale;
            regionW *= scale;
            regionH *= scale;

            miniMapTileW *= scale;
            miniMapTileH *= scale;
            miniMapRegionW *= scale;
            miniMapRegionH *= scale;
            miniMapRegionRatioW *= scale;
            miniMapRegionRatioH *= scale;            
        }
	}
}

