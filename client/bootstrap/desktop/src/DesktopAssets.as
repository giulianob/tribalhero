package src {
    public class DesktopAssets {
        // Any asset that is not in the map (e.g. UI elements, icons) should be loaded at 0.5x
        // Map items should be full and loaded at 1x since the map supports zoom

        [Embed(source="../../../../graphics/atlas/0.5x/ICONS_ATLAS.png")]
        public static const ICONS_ATLAS: Class;
        [Embed(source="../../../../graphics/atlas/0.5x/ICONS_ATLAS.xml", mimeType="application/octet-stream")]
        public static const ICONS_ATLAS_XML:Class;

        [Embed(source="../../../../graphics/atlas/1x/OBJECTS_ATLAS.png")]
        public static const OBJECTS_ATLAS: Class;
        [Embed(source="../../../../graphics/atlas/1x/OBJECTS_ATLAS.xml", mimeType="application/octet-stream")]
        public static const OBJECTS_ATLAS_XML:Class;

        // Default Theme
        [Embed(source="../../../../graphics/atlas/1x/DEFAULT_THEME_ATLAS.png")]
        public static const DEFAULT_THEME_ATLAS: Class;
        [Embed(source="../../../../graphics/atlas/1x/DEFAULT_THEME_ATLAS.xml", mimeType="application/octet-stream")]
        public static const DEFAULT_THEME_ATLAS_XML:Class;

        [Embed(source="../../../../graphics/atlas/1x/DEFAULT_THEME_WALL_ATLAS.png")]
        public static const DEFAULT_THEME_WALL_ATLAS: Class;
        [Embed(source="../../../../graphics/atlas/1x/DEFAULT_THEME_WALL_ATLAS.xml", mimeType="application/octet-stream")]
        public static const DEFAULT_THEME_WALL_ATLAS_XML:Class;
        
        // Pirate Theme
        [Embed(source="../../../../graphics/atlas/1x/PIRATES_THEME_ATLAS.png")]
        public static const PIRATES_THEME_ATLAS: Class;
        [Embed(source="../../../../graphics/atlas/1x/PIRATES_THEME_ATLAS.xml", mimeType="application/octet-stream")]
        public static const PIRATES_THEME_ATLAS_XML:Class;

        [Embed(source="../../../../graphics/atlas/1x/PIRATES_THEME_WALL_ATLAS.png")]
        public static const PIRATES_THEME_WALL_ATLAS: Class;
        [Embed(source="../../../../graphics/atlas/1x/PIRATES_THEME_WALL_ATLAS.xml", mimeType="application/octet-stream")]
        public static const PIRATES_THEME_WALL_ATLAS_XML:Class;

        // Feudal Japan Theme
        [Embed(source="../../../../graphics/atlas/1x/FEUDALJAPAN_THEME_ATLAS.png")]
        public static const FEUDALJAPAN_THEME_ATLAS: Class;
        [Embed(source="../../../../graphics/atlas/1x/FEUDALJAPAN_THEME_ATLAS.xml", mimeType="application/octet-stream")]
        public static const FEUDALJAPAN_THEME_ATLAS_XML:Class;

        [Embed(source="../../../../graphics/atlas/1x/FEUDALJAPAN_THEME_WALL_ATLAS.png")]
        public static const FEUDALJAPAN_THEME_WALL_ATLAS: Class;
        [Embed(source="../../../../graphics/atlas/1x/FEUDALJAPAN_THEME_WALL_ATLAS.xml", mimeType="application/octet-stream")]
        public static const FEUDALJAPAN_THEME_WALL_ATLAS_XML:Class;
    }
}
