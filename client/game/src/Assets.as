package src {
    public class Assets {
        [Embed(source="../../../graphics/map_positions.json", mimeType="application/octet-stream")]
        public static const MAP_POSITIONS:Class;

        [Embed(source = "../../../graphics/TILESET.png")]
        public static const TILESET: Class;
    }
}
