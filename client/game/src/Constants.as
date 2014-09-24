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

        // Returns the current scale compared to the default gfx.
        // Generally for desktop this would be 1 since we use full res gfx
        // and for mobile it would be 0.5 since the baseline is low res
        public static var scale: Number;

        // The content scale factor we should use if we load something manually like
        // the tileset. For desktop this is 1 and for mobile it will be 2.
        public static var contentScaleFactorBaseline: int;

		public static var tileW:int;
		public static var tileH:int;

		public static var tileSetTileW:int = 12;
		public static var tileSetTileH:int = 12;

		public static var mapW:int;
		public static var mapH:int;

		public static var mapTileW:int = 3400;
		public static var mapTileH:int = 6200;

        public static var regionCount:int = (mapTileW/regionTileW)*(mapTileH/regionTileH);

		public static var regionW:int;
		public static var regionH:int;

		public static var regionTileW:int = 34;
		public static var regionTileH:int = 62;

		public static var mapRegionW: int;
		public static var mapRegionH: int;

		public static var origScreenW:int = 976;
		public static var origScreenH:int = 640;
		
		public static var screenW:int = origScreenW;
		public static var screenH:int = origScreenH;
		
		public static var cityStartTile: int = 16;

		/* MINI MAP CONSTANTS */
		public static var miniMapTileW: int;
		public static var miniMapTileH: int;

		public static var miniMapRegionW: int;
		public static var miniMapRegionH: int;

		public static var miniMapRegionTileW: int = 100;
		public static var miniMapRegionTileH: int = 100;

        public static var miniMapRegionCount:int = (mapTileW/miniMapRegionTileW)*(mapTileH/miniMapRegionTileH);

        public static var miniMapRegionRatioW: int;
		public static var miniMapRegionRatioH: int;

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

        public static function initMapSize(scaleBaseline: Number): void {
            trace("Setting scale basline to " + scaleBaseline);

            Constants.scale = scaleBaseline;
            Constants.contentScaleFactorBaseline = int(1.0 / Constants.scale + 0.5);

            var scaleOverOne: int = (int)(1.0/scaleBaseline + 0.5);

            tileW = 160 / scaleOverOne;
            tileH = 80 / scaleOverOne;

            regionW = regionTileW * tileW;
            regionH = regionTileH * tileH/2;
            mapW = mapTileW * tileW;
            mapH = mapTileH * tileH;
            mapRegionW = mapW / (regionTileW * tileW);
            mapRegionH = mapH / (regionTileH * tileH);

            miniMapTileW = 4 / scaleOverOne;
            miniMapTileH = 2 / scaleOverOne;

            miniMapRegionW = miniMapRegionTileW * miniMapTileW;
            miniMapRegionH = miniMapRegionTileH * miniMapTileH/2.0;

            miniMapRegionRatioW = mapTileW / miniMapRegionTileW;
            miniMapRegionRatioH = mapTileH / miniMapRegionTileH;
        }
	}
}

