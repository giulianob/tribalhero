package src
{
	import flash.net.Socket;
	public class Constants
	{		
		/* APP CONSTANTS */
		public static const debug:int = 0;		
		public static const defLang:String = "en";
		public static const webVersion: Boolean = true;
		
		public static var secondsPerUnit: Number = 1;
		
		/* COMM CONSTANTS */
		public static const headerSize: int = 8;
		
		/* MAP CONSTANTS */
		public static const tileW:int = 108;
		public static const tileH:int = 54;
		
		public static const tileSetW:int = 1728;
		public static const tileSetH:int = 1944;
		
		public static const tileSetTileW:int = tileSetW / tileW;
		public static const tileSetTileH:int = tileSetH / tileH;		
		
		public static const mapW:int = 3808 * tileW;
		public static const mapH:int = 6944 * tileH;
		
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
		
		public static const screenW:int = 976;
		public static const screenH:int = 640;
		
		public static const movieW:int = 976;
		public static const movieH:int = 640;	
		
		/* MINI MAP CONSTANTS */
		public static const miniMapTileW: int = 6;
		public static const miniMapTileH: int = 3;
		
		public static const miniMapScreenTileW: int = miniMapScreenW / tileW;
		public static const miniMapScreenTileH: int = miniMapScreenH / tileH;
		
		public static const miniMapScreenW: int = 290;
		public static const miniMapScreenH: int = 140;
		
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
		
		/* ACTION LITERAL MAPPING */
		public static const structureBuild: String = "BUILD";
		
		/* GAME DATA */
		public static const queryData: Boolean = true;
		
		/* TROOP */
		public static const troopWorkerId: int = 100;
				
		public static var objData: XML = <Data></Data>;			
	}
}