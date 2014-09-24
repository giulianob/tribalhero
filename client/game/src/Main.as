package src
{
    import System.Linq.EnumerationExtender;

    import com.greensock.OverwriteManager;
    import com.greensock.plugins.TransformMatrixPlugin;
    import com.greensock.plugins.TweenPlugin;

    import feathers.core.PopUpManager;

    import flash.display.*;
    import flash.events.*;
    import flash.system.Security;

    import org.aswing.*;

    import src.Comm.*;
    import src.FeathersUI.GameScreen.IGameScreenFlow;
    import src.FeathersUI.Map.MapVM;
    import src.FeathersUI.MiniMap.MiniMapVM;
    import src.Map.*;
    import src.Objects.Factories.*;
    import src.Objects.ObjectContainer;
    import src.UI.Flows.LoginFlow;
    import src.UI.LookAndFeel.*;
    import src.UI.TweenPlugins.DynamicPropsPlugin;
    import src.UI.TweenPlugins.TransformAroundCenterPlugin;
    import src.UI.TweenPlugins.TransformAroundCenterStarlingPlugin;
    import src.UI.TweenPlugins.TransformAroundPointPlugin;
    import src.UI.TweenPlugins.TransformAroundPointStarlingPlugin;
    import src.Util.*;

    import starling.display.Sprite;

    public class Main extends flash.display.Sprite
	{
        public var packetCounter:GeneralCounter;

		private var parms: Object;

        private var assetInitializer: Function;
        private var bootstrapper: IBootstrapper;

		public function Main(bootstrapper: IBootstrapper)
		{
            this.bootstrapper = bootstrapper;
            this.assetInitializer = assetInitializer;

            Security.loadPolicyFile(Constants.mainWebsite + "crossdomain.xml?m=" + Constants.version + "." + Constants.revision);

			name = "Main";
			trace("TribalHero v" + Constants.version + "." + Constants.revision);

			addEventListener(Event.ADDED_TO_STAGE, init);
		}
		
		public function init(e: Event = null) : void {
            var self:Main = this;

			removeEventListener(Event.ADDED_TO_STAGE, init);

                StarlingStage.init(stage, bootstrapper).then(function(value: *): void {
                stage.showDefaultContextMenu = false;

                var popupRoot: starling.display.Sprite = new starling.display.Sprite();
                PopUpManager.root = popupRoot;
                Global.starlingStage.addChild(popupRoot);

                Global.stage = stage;
                Global.musicPlayer = new MusicPlayer();

                //Init actionLinq
                EnumerationExtender.Initialize();

                //Init ASWING
                AsWingManager.initAsStandard(self);
                UIManager.setLookAndFeel(new GameLookAndFeel());

                //Init TweenLite
                TweenPlugin.activate([DynamicPropsPlugin, TransformMatrixPlugin, TransformAroundCenterPlugin, TransformAroundPointPlugin, TransformAroundPointStarlingPlugin, TransformAroundCenterStarlingPlugin]);
                OverwriteManager.init(OverwriteManager.AUTO);

                //Init stage options
                stage.stageFocusRect = false;
                stage.scaleMode = StageScaleMode.NO_SCALE;
                stage.align = StageAlign.TOP_LEFT;

                Global.main = self;

                //Init right context menu for debugging

                CONFIG::debug {
                    /*
                    var fm_menu:ContextMenu = new ContextMenu();
                    var dump:ContextMenuItem = new ContextMenuItem("Dump stage");
                    dump.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, function(e:Event):void {
                        Util.dumpDisplayObject(stage);
                        Util.dumpStarlingStage();
                    } );
                    fm_menu.customItems = fm_menu.customItems !== null ? fm_menu.customItems : [];
                    fm_menu.customItems.push(dump);
                    self.contextMenu = fm_menu;
                    */
                }

                //Flash params
                parms = loaderInfo.parameters;

                //GameContainer
                Global.gameContainer = new GameContainer();

                var tmp: flash.display.Sprite = new flash.display.Sprite();
                tmp.visible = false;
                tmp.addChild(Global.gameContainer);
                addChild(tmp);

                //Packet Counter
                if (Constants.debug > 0) {
                    packetCounter = new GeneralCounter("pkts");
                    packetCounter.y = Constants.screenH - 64;
                    addChild(packetCounter);
                }

                Constants.mainWebsite = parms.mainWebsite || Constants.mainWebsite;

                if (parms.playerName) {
                    Constants.session.playerName = parms.playerName;
                }

                if (parms.hostname) {
                    Constants.session.hostname = parms.hostname;
                }

                // Need to give feathers time to init for some reason
                Util.callLater(function(): void {
                    var loginFlow: LoginFlow = new LoginFlow(parms, Global.gameContainer);
                    loginFlow.on(LoginFlow.LOGIN_COMPLETE, completeLogin);
                    loginFlow.showLogin();
                });
            }).done();
		}

        private function completeLogin(packet: Packet):void
		{
            var camera: Camera = new Camera(0, 0);

            var objContainer: ObjectContainer = new ObjectContainer();
            var mapVM: MapVM = new MapVM(objContainer, camera);

            Global.map = mapVM;
			var miniMapVM: MiniMapVM = new MiniMapVM(camera);

			mapVM.usernames.players.add(new Username(Constants.session.playerId, Constants.session.playerName));
			mapVM.setTimeDelta(Constants.session.timeDelta);
			
			EffectReqFactory.init(Constants.objData);
			PropertyFactory.init(Constants.objData);
			StructureFactory.init(mapVM, Constants.objData);
			TechnologyFactory.init(Constants.objData);
			UnitFactory.init(Constants.objData);
			WorkerFactory.init(Constants.objData);
			ObjectFactory.init(Constants.objData);
			
			Constants.objData = <Data></Data>;

            Global.gameContainer.show();
			Global.mapComm.General.readLoginInfo(packet);
            Global.gameContainer.setMap(mapVM, miniMapVM);

            var gameScreen: IGameScreenFlow = bootstrapper.getFlowFactory(mapVM, miniMapVM).createGameScreenFlow();;
            gameScreen.show();
		}
    }
}
