package src {
    import com.greensock.TweenMax;

    import fl.motion.AdjustColor;

    import flash.events.Event;
    import flash.events.MouseEvent;
    import flash.filters.ColorMatrixFilter;
    import flash.net.URLRequest;
    import flash.net.navigateToURL;

    import src.FeathersUI.Map.MapView;

    import src.Map.ScreenPosition;
    import src.UI.Components.SimpleTooltip;
    import src.UI.Dialog.GoToDialog;

    public class MinimapToolsContainer extends MinimapToolsContainer_base {
        public var minimapZoomed: Boolean = false;
        private var minimapZoomTooltip: SimpleTooltip;
        private var musicPlayer: MusicPlayer;
        private var gameContainer: GameContainer;
        private var map: MapView;

        public function MinimapToolsContainer(musicPlayer: MusicPlayer, gameContainer: GameContainer) {
            this.musicPlayer = musicPlayer;
            this.gameContainer = gameContainer;

            minimapZoomTooltip = new SimpleTooltip(btnMinimapZoom);
            btnMinimapZoom.addEventListener(MouseEvent.CLICK, onZoomIntoMinimap);

            new SimpleTooltip(btnGoToCoords, "Find...");
            btnGoToCoords.addEventListener(MouseEvent.CLICK, onGoToCoords);

            new SimpleTooltip(btnFeedback, "Send Feedback");
            btnFeedback.addEventListener(MouseEvent.CLICK, onSendFeedback);

            new SimpleTooltip(btnZoomIn, "Zoom In");
            btnZoomIn.addEventListener(MouseEvent.CLICK, onZoomIn);

            new SimpleTooltip(btnZoomOut, "Zoom Out");
            btnZoomOut.addEventListener(MouseEvent.CLICK, onZoomOut);

            new SimpleTooltip(btnMute, "Play/Pause Music");
            btnMute.addEventListener(MouseEvent.CLICK, onMuteSound);

            musicPlayer.events.addEventListener(MusicPlayer.STATE_CHANGE, onMusicPlayerStateChanged, false, 0, true);
            setMusicButtonLook();
        }

        private function onMusicPlayerStateChanged(event: Event): void {
            setMusicButtonLook();
        }

        private function setMusicButtonLook(): void {
            if (musicPlayer.isMuted) {
                var color: AdjustColor = new AdjustColor();
                color.brightness = 5;
                color.contrast = 0;
                color.hue = 0;
                color.saturation = -100;
                btnMute.filters = [new ColorMatrixFilter(color.CalculateFinalFlatArray())];
            }
            else {
                btnMute.filters = [];
            }
        }

        private function onMuteSound(event: MouseEvent): void {
            musicPlayer.toggle();
        }

        public function onGoToCoords(e: Event) : void {
            var goToDialog: GoToDialog = new GoToDialog();
            goToDialog.show();
        }

        public function onSendFeedback(e: Event) : void {
            navigateToURL(new URLRequest(Constants.mainWebsite + "feedback"), "_blank");
        }

        public function onZoomIn(e: Event) : void {
            if (minimapZoomed || gameContainer.camera.zoomFactor == 100) {
                return;
            }

            var step: int = 10;
            var newValue: int = gameContainer.camera.zoomFactor + step >= 100 ? 100 : gameContainer.camera.zoomFactor + step;
            var duration: Number = 0.2 * ((newValue - gameContainer.camera.zoomFactor) / step);

            TweenMax.to(gameContainer.camera, duration, {
                zoomFactor: newValue,
                onUpdateParams: [gameContainer.camera.mapCenter()],
                onUpdate: onZoomUpdate
            });
        }

        public function onZoomOut(e: Event) : void {
            if (minimapZoomed || gameContainer.camera.zoomFactor == 50) {
                return;
            }

            var step: int = 10;
            var newValue: int = gameContainer.camera.zoomFactor - step <= 50 ? 50 : gameContainer.camera.zoomFactor - step;
            var duration: Number = 0.2 * ((gameContainer.camera.zoomFactor - newValue) / step);

            TweenMax.to(gameContainer.camera, duration, {
                zoomFactor: newValue,
                onUpdateParams: [gameContainer.camera.mapCenter()],
                onUpdate: onZoomUpdate
            });
        }

        private function onZoomUpdate(center: ScreenPosition): void {
            gameContainer.camera.scrollRate = gameContainer.camera.getZoomFactorOverOne();
            gameContainer.camera.scrollToCenter(center);
        }

        public function onZoomIntoMinimap(e: Event):void {
            zoomIntoMinimap(!minimapZoomed);
        }

        public function zoomIntoMinimap(zoom: Boolean, query: Boolean = true) : void {
            return;

            if (minimapZoomed == false) {
                gameContainer.camera.cue();
            }
            else {
                gameContainer.camera.goToCue();
            }

            gameContainer.clearAllSelections();

            if (zoom) {
                gameContainer.screenMessage.setVisible(false);
                // We leave a bit of border incase the screen is smaller than the map size
                var width: int = Math.min(Constants.screenW - 60, Constants.miniMapLargeScreenW);
                var height: int = Math.min(Constants.screenH - 75, Constants.miniMapLargeScreenH);
//                gameContainer.miniMap.resize(width, height);
//                gameContainer.miniMap.x = Constants.miniMapLargeScreenX(width)-30;
//                gameContainer.miniMap.y = Constants.miniMapLargeScreenY(height);
                minimapZoomTooltip.setText("Minimize map");
//                gameContainer.miniMap.setScreenRectHidden(true);
                map.disableMapQueries(true);
                gameContainer.camera.scrollRate = 25;
                btnZoomIn.visible = false;
                btnZoomOut.visible = false;
                gameContainer.message.showMessage("Double click to go anywhere\nPress Escape to close this map");
//                gameContainer.miniMap.showLegend();
//                gameContainer.miniMap.showPointers();
            }
            else {
                gameContainer.screenMessage.setVisible(true);
//                gameContainer.miniMap.resize(Constants.miniMapScreenW, Constants.miniMapScreenH);
//                gameContainer.miniMap.x = Constants.miniMapScreenX(Constants.miniMapScreenW);
//                gameContainer.miniMap.y = Constants.miniMapScreenY(Constants.miniMapScreenH);
                minimapZoomTooltip.setText("World view");
//                gameContainer.miniMap.setScreenRectHidden(false);
                map.disableMapQueries(false);
                gameContainer.camera.scrollRate = gameContainer.camera.getZoomFactorOverOne();
                btnZoomIn.visible = true;
                btnZoomOut.visible = true;
                gameContainer.message.hide();
//                gameContainer.miniMap.hideLegend();
//                gameContainer.miniMap.hidePointers();
            }

            minimapZoomed = zoom;
            if (query) {
                map.update();
            }

            alignMinimapTools();
        }

        private function alignMinimapTools() : void {
//            x = gameContainer.miniMap.x;
//            y = gameContainer.miniMap.y - 3;
        }


    }
}
