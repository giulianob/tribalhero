package src {
    import flash.display.Sprite;
    import flash.events.Event;

    public class Loader extends Sprite {
        public function Loader() {
            if (stage) {
                start();
            }
            else {
                addEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
            }
        }

        private function onAddedToStage(e:Object):void
        {
            removeEventListener(Event.ADDED_TO_STAGE, onAddedToStage);
            start();
        }

        private function start():void
        {
            var main: Main = new Main(new MobileBootstrapper());
            addChild(main);
        }
    }
}
