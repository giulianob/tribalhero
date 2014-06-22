package src {
    public class StarlingAssets {

        [Embed(source = "../../../graphics/map_positions.json", mimeType="application/octet-stream")]
        public static const MAP_POSITIONS:Class;

        [Embed(source = "../../../graphics/Tilemap.png")]
        public static const TILESET: Class;

        [Embed(source = "../../../graphics/objects/OBJECTS_ATLAS.png")]
        public static const OBJECTS_ATLAS: Class;
        [Embed(source = "../../../graphics/objects/OBJECTS_ATLAS.xml", mimeType="application/octet-stream")]
        public static const OBJECTS_ATLAS_XML:Class;

        // Default Theme
        [Embed(source = "../../../graphics/themes/default/DEFAULT_ATLAS.png")]
        public static const DEFAULT_ATLAS: Class;
        [Embed(source = "../../../graphics/themes/default/DEFAULT_ATLAS.xml", mimeType="application/octet-stream")]
        public static const DEFAULT_ATLAS_XML:Class;

        [Embed(source = "../../../graphics/themes/default/DEFAULT_WALL.png")]
        public static const DEFAULT_WALL: Class;

        // Pirate Theme
        [Embed(source = "../../../graphics/themes/pirates/PIRATES_ATLAS.png")]
        public static const PIRATES_ATLAS: Class;
        [Embed(source = "../../../graphics/themes/pirates/PIRATES_ATLAS.xml", mimeType="application/octet-stream")]
        public static const PIRATES_ATLAS_XML:Class;

        [Embed(source = "../../../graphics/themes/pirates/PIRATES_WALL.png")]
        public static const PIRATES_WALL: Class;


    }
}
