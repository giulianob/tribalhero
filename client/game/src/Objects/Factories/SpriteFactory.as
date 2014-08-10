package src.Objects.Factories {
    import flash.display.DisplayObject;
    import flash.display.DisplayObjectContainer;
    import flash.display.Sprite;
    import flash.geom.Point;

    import src.FlashAssets;
    import src.StarlingStage;

    import starling.display.Image;
    import starling.textures.Texture;

    public class SpriteFactory {
        private static var mapPositions: Object = null;

        public static function getFlashSprite(typeName: String): Sprite
        {
            var mainImage: DisplayObject = FlashAssets.getInstance(typeName);

            var sprite: Sprite = new Sprite();
            sprite.addChild(mainImage);

            return sprite;
        }

        public static function getStarlingImage(typeName: String): Image
        {
            var mainImage: Texture = StarlingStage.assets.getTexture(typeName);
            if (mainImage === null) {
                throw new Error("Could not find texture named " + typeName);
            }
            return new Image(mainImage)
        }

        public static function getMapPosition(name: String):Point {
            if (mapPositions === null) {
                mapPositions = StarlingStage.assets.getObject("MAP_POSITIONS");
            }

            var position: Object = mapPositions[name];

            return new Point(position.x, position.y);
        }

    }
}
