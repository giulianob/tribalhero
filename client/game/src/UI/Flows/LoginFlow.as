package src.UI.Flows {
    import feathers.core.PopUpManager;

    import fl.lang.Locale;

    import flash.events.Event;
    import flash.events.IOErrorEvent;
    import flash.events.SecurityErrorEvent;
    import flash.net.URLLoader;
    import flash.net.URLRequest;

    import src.Comm.Packet;

    import src.Comm.TcpSession;

    import src.Constants;
    import src.GameContainer;
    import src.Global;
    import src.Map.MapComm;
    import src.UI.Dialog.InfoDialog;
    import src.UI.Dialog.InitialCityDialog;
    import src.FeathersUI.Login.LoginDialog;
    import src.FeathersUI.Flow;
    import src.FeathersUI.Login.LoginVM;
    import src.Util.Util;

    public class LoginFlow extends Flow {
        public static const LOGIN_COMPLETE: String = "LOGIN_COMPLETE";

        private var gameContainer: GameContainer;
        private var parms: *;

        public function LoginFlow(parms: *, gameContainer: GameContainer) {
            this.parms = parms;
            this.gameContainer = gameContainer;
        }

        private var session: TcpSession;

        private var password: String;

        private var loginDialog: LoginDialog;

        private var pnlLoading: InfoDialog;

        public var errorAlreadyTriggered: Boolean;

        private var siteVersion: String;

        public function showLogin(): void {
            //Define login type and perform login action
            if (parms.lsessid)
            {
                siteVersion = parms.siteVersion;
                Constants.session.loginKey = parms.lsessid;
                loadData();
            }
            else
            {
                siteVersion = new Date().getTime().toString();
                showLoginDialog();
            }
        }

        private function loadData(): void
        {
            pnlLoading = InfoDialog.showMessageDialog("TribalHero", "Launching the game...", null, null, true, false, 0);

            var loader: URLLoader = new URLLoader();
            loader.addEventListener(Event.COMPLETE, function(e: Event) : void {
                Constants.objData = XML(e.target.data);
                loadLanguages();
            });
            loader.addEventListener(IOErrorEvent.IO_ERROR, function(e: Event): void {
                onDisconnected();
                showConnectionError(true);
            });
            loader.load(new URLRequest("http://" + Constants.session.hostname + "/data.xml?m=" + new Date().getTime().toString() + "&v=" + siteVersion));
        }

        private function loadLanguages():void
        {
            Locale.setLoadCallback(function(success: Boolean) : void {
                if (!success) {
                    onDisconnected();
                    showConnectionError(true);
                }
                else {
                    doConnect();
                }
            });
            Locale.addXMLPath(Constants.defLang, "http://" + Constants.session.hostname + "/Game_" + Constants.defLang + ".xml?m=" + new Date().getTime().toString() + "&v=" + siteVersion);
            Locale.setDefaultLang(Constants.defLang);
            Locale.loadLanguageXML(Constants.defLang);
        }

        public function doConnect():void
        {
            errorAlreadyTriggered = false;

            session = new TcpSession();
            session.setConnect(onConnected);
            session.setLogin(onLogin);
            session.setDisconnect(onDisconnected);
            session.setSecurityErrorCallback(onSecurityError);
            session.connect(Constants.session.hostname);
        }

        public function showLoginDialog():void
        {
            gameContainer.closeAllFrames();

            var loginVM: LoginVM = new LoginVM();            
            loginDialog = new LoginDialog(loginVM);
			loginVM.addEventListener(LoginVM.LOGIN, onConnect);

            PopUpManager.addPopUp(loginDialog);
        }

        public function onConnect(username: String, password: String, hostname: String):void
        {
            Constants.session.username = username;
            this.password = password;
            Constants.session.hostname = hostname;

            loadData();
        }

        public function onSecurityError(event: SecurityErrorEvent):void
        {
            Util.log("Security error " + event.toString());

            if (session && session.hasLoginSuccess())
                return;

            onDisconnected();
        }

        public function onDisconnected(event: Event = null):void
        {
            Util.triggerJavascriptEvent("clientDisconnect");

            var wasStillLoading: Boolean = session == null || !session.hasLoginSuccess();

            if (pnlLoading) {
                pnlLoading.getFrame().dispose();
            }

            gameContainer.dispose();
            if (Global.mapComm) {
                Global.mapComm.dispose();
            }

            if (Global.musicPlayer) {
                Global.musicPlayer.stop();
            }

            Global.mapComm = null;
            Global.map = null;
            session = null;

            if (!errorAlreadyTriggered) {
                showConnectionError(wasStillLoading);
            }
        }

        public function showConnectionError(wasStillLoading: Boolean) : void {
            var unableToConnectMsg: String = "Unable to connect to server.\nIf you continue to have problems, try the following:\n\n1. Clear your browser cache.\n2. Make sure your firewall and/or antivirus programs allow access to ports 843 and 443. You may be unable to connect if you are behind a shared connection such as an office or school.\n3. Update to the latest version of Flash player.\n4. Check our main page to see if a server maintenance is in progress.\n\nIf none of these solved your problem, contact us at feedback@tribalhero.com for individual help.";

            if (parms.hostname) {
                InfoDialog.showMessageDialog("Connection Lost", (wasStillLoading ? unableToConnectMsg : "Connection to Server Lost") + ". Refresh the page to rejoin the battle.", null, null, true, false, 1, true);
            }
            else {
                InfoDialog.showMessageDialog("Connection Lost", (wasStillLoading ? unableToConnectMsg : "Connection to Server Lost."), function(result: int):void { showLoginDialog(); }, null, true, false, 1, true);
            }
        }

        public function onConnected(event: Event, connected: Boolean):void
        {
            if (pnlLoading) {
                pnlLoading.getFrame().dispose();
            }

            if (!connected) {
                showConnectionError(true);
            }
            else
            {
                Util.triggerJavascriptEvent("clientConnect");

                Global.mapComm = new MapComm(session);

                if (Constants.session.loginKey) {
                    session.login(true, Constants.session.playerName, Constants.session.loginKey);
                }
                else {
                    session.login(false, Constants.session.username, password);
                }
            }

            password = '';
        }

        public function onLogin(packet: Packet):void
        {
            if (MapComm.tryShowError(packet, function(result: int) : void { showLoginDialog(); } , true)) {
                errorAlreadyTriggered = true;
                return;
            }

            session.setLoginSuccess(true);

            if (loginDialog != null) {
                PopUpManager.removePopUp(loginDialog, true);
            }

            var newPlayer: Boolean = Global.mapComm.General.onLogin(packet);

            if (!newPlayer) {
                dispatch(LOGIN_COMPLETE, packet);
            }
            else {
                var createCityDialog: InitialCityDialog = new InitialCityDialog(function(sender: InitialCityDialog): void {
                    Global.mapComm.General.createInitialCity(sender.getCityName(),
                        sender.getLocationParameter(),
                        function(packet: Packet):void {
                            dispatch(LOGIN_COMPLETE, packet);
                        });
                });

                createCityDialog.show();
            }

            Global.musicPlayer.setMuted(Constants.session.soundMuted, true);
            if (!Constants.session.soundMuted) {
                Global.musicPlayer.play(newPlayer);
            }
        }
    }
}
