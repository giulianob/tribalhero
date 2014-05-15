package src {
    import flash.events.Event;
    import flash.events.EventDispatcher;

    import src.Objects.Tribe;
    import src.Util.Util;

    public class SessionVariables extends EventDispatcher {
        public static const COINS_UPDATE: String = "COINS_UPDATE";

        public var username: String = "1234";
        public var hostname: String = "local.tribalhero.com";
        public var serverPort: int = 443;
        public var sessionId: String;
        public var playerName: String;
        public var timeDelta: int;
        public var admin: Boolean;
        public var loginKey: String;
        public var playerId: int;
        public var playerHash: String;
        public var tutorialStep: int;
        public var tribeInviteId: int;
        public var tribe: Tribe = new Tribe();
        public var signupTime: Date;
        public var newbieProtectionSeconds: int;
        public var tribeAssignment: int;
        public var tribeIncoming: int;
        public var soundMuted: Boolean;
        public var themesPurchased: Array = [];

        private var _coins: int = 0;

        public function get coins(): int {
            return _coins;
        }

        public function set coins(value: int): void {
            _coins = value;

            Util.log("Received coin update for " + value);
            dispatchEvent(new Event(COINS_UPDATE));
        }
    }
}
