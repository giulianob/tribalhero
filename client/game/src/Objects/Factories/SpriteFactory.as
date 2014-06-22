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

        protected static function getFlashSprite(typeName: String): DisplayObjectContainer
        {
            var mainImage: DisplayObject = FlashAssets.getInstance(typeName);

            var sprite: Sprite = new Sprite();
            sprite.addChild(mainImage);

            return sprite;
        }

        protected static function getStarlingSprite(typeName: String): starling.display.DisplayObjectContainer
        {
            var mainImage: Texture = StarlingStage.assets.getTexture(typeName);

            var sprite: starling.display.Sprite = new starling.display.Sprite();
            sprite.addChild(new Image(mainImage));

            return sprite;
        }

        protected static function getMapPosition(name: String):Point {
            if (mapPositions === null) {
                mapPositions = StarlingStage.assets.getObject("MAP_POSITIONS");
            }

            var position: Object = mapPositions[name];

            return new Point(position.x, position.y);
        }

    }
}
