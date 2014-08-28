package src
{
    import System.Linq.EnumerationExtender;

    import com.greensock.OverwriteManager;

    import com.greensock.plugins.TransformMatrixPlugin;
    import com.greensock.plugins.TweenPlugin;

    import fl.lang.*;

    import flash.display.*;
    import flash.events.*;
    import flash.net.*;
    import flash.system.Security;
    import flash.ui.*;

    import org.aswing.*;

    import src.Comm.*;
    import src.Map.*;
    import src.Map.MiniMap.MiniMap;
    import src.Objects.Factories.*;
    import src.UI.Dialog.*;
    import src.UI.Flows.LoginFlow;
    import src.UI.LookAndFeel.*;
    import src.UI.TweenPlugins.DynamicPropsPlugin;
    import src.UI.TweenPlugins.TransformAroundCenterPlugin;
    import src.UI.TweenPlugins.TransformAroundCenterStarlingPlugin;
    import src.UI.TweenPlugins.TransformAroundPointPlugin;
    import src.UI.TweenPlugins.TransformAroundPointStarlingPlugin;
    import src.Util.*;
	
    public class Main extends MovieClip
	{
		private var map:Map;

        private var miniMap: MiniMap;

        public var packetCounter:GeneralCounter;

		private var parms: Object;

		public function Main()
		{
            Security.loadPolicyFile(Constants.mainWebsite + "crossdomain.xml?m=" + Constants.version + "." + Constants.revision);

			name = "Main";
			trace("TribalHero v" + Constants.version + "." + Constants.revision);

			addEventListener(Event.ADDED_TO_STAGE, init);
		}
		
		public function init(e: Event = null) : void {
            var self:Main = this;

			removeEventListener(Event.ADDED_TO_STAGE, init);

            StarlingStage.init(stage).then(function(value: *): void {
                stage.showDefaultContextMenu = false;

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
                /*
                CONFIG::debug {
                    var fm_menu:ContextMenu = new ContextMenu();
                    var dump:ContextMenuItem = new ContextMenuItem("Dump stage");
                    dump.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, function(e:Event):void { Util.dumpDisplayObject(stage); } );
                    var dumpRegionQueryInfo:ContextMenuItem = new ContextMenuItem("Dump region query info");
                    dumpRegionQueryInfo.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, function(e:Event):void {
                        if (!Global.map) return;
                        Util.log("Pending regions:" + Util.implode(',', Global.map.pendingRegions));
                    } );
                    fm_menu.customItems.push(dump);
                    fm_menu.customItems.push(dumpRegionQueryInfo);
                    contextMenu = fm_menu;
                }
                */

                //Flash params
                parms = loaderInfo.parameters;

                //GameContainer
                Global.gameContainer = new GameContainer();
                addChild(Global.gameContainer);

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
			Global.map = map = new Map();
			miniMap = new MiniMap(Constants.miniMapScreenW, Constants.miniMapScreenH);
			
			map.usernames.players.add(new Username(Constants.session.playerId, Constants.session.playerName));
			map.setTimeDelta(Constants.session.timeDelta);
			
			EffectReqFactory.init(map, Constants.objData);
			PropertyFactory.init(map, Constants.objData);
			StructureFactory.init(map, Constants.objData);
			TechnologyFactory.init(map, Constants.objData);
			UnitFactory.init(map, Constants.objData);
			WorkerFactory.init(map, Constants.objData);
			ObjectFactory.init(map, Constants.objData);
			
			Constants.objData = <Data></Data>;

            Global.gameContainer.show();
			Global.mapComm.General.readLoginInfo(packet);
            Global.gameContainer.setMap(map, miniMap);
		}
    }
}
