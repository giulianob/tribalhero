package src {
    import com.greensock.loading.MP3Loader;

    import flash.events.Event;
    import flash.events.EventDispatcher;
    import flash.events.TimerEvent;
    import flash.media.SoundLoaderContext;
    import flash.net.URLRequest;
    import flash.utils.Timer;

    public class MusicPlayer {
        private const SONGS: Array = [
                "http://cdn.tribalhero.com/music/TribalHero1.mp3",
                "http://cdn.tribalhero.com/music/TribalHero2.mp3",
                "http://cdn.tribalhero.com/music/TribalHero3.mp3",
                "http://cdn.tribalhero.com/music/TribalHero4.mp3"
        ];

        public var events: EventDispatcher = new EventDispatcher();
        public static const STATE_CHANGE: String = "STATE_CHANGE";

        private var player: MP3Loader;
        private var _isMuted: Boolean;
        private var lastSong: int = -1;
        private var waitBetweenSongsTimer: Timer = new Timer(60000);
        private var _volume: Number = 0.35;

        public function MusicPlayer() {
            waitBetweenSongsTimer.addEventListener(TimerEvent.TIMER, onWaitedBetweenSongsTimer);
        }

        private function onWaitedBetweenSongsTimer(event: TimerEvent): void {
            play(false);
        }

        private function nextSong(): String {
            var nextSong: int;
            do {
                nextSong = Math.floor(Math.random() * SONGS.length);
            } while (lastSong == nextSong);

            lastSong = nextSong;
            return SONGS[nextSong];
        }

        public function play(startWithThemeSong: Boolean): void {
            if (player && player.soundPaused) {
                player.soundPaused = false;
            }
            else {
                stop();

                player = new MP3Loader(new URLRequest(startWithThemeSong ? SONGS[0] : nextSong()), {
                    autoPlay: true,
                    context: new SoundLoaderContext(1000, false),
                    volume: _volume,
                    repeat: 0
                });

                player.addEventListener(MP3Loader.SOUND_COMPLETE, soundCompleteHandler);

                player.load();
            }
        }
        private function soundCompleteHandler(event: Event): void {
            stop();

            waitBetweenSongsTimer.delay = Math.random() * 120000 + 60000;
            waitBetweenSongsTimer.start();
        }

        public function stop(): void {
            waitBetweenSongsTimer.stop();

            if (player) {
                player.dispose(true);
                player = null;
            }
        }

        public function toggle(playMusic: * = null): void {
            if (playMusic === null) {
                this.setMuted(!this.isMuted);
            }
            else {
                this.setMuted(!playMusic);
            }

            if (!this._isMuted) {
                play(false);
            } else if (this._isMuted && player) {
                player.soundPaused = true;
            }
        }

        public function setMuted(isMuted: Boolean, isProgrammatic: Boolean = false): void {
            _isMuted = isMuted;

            if (!isProgrammatic) {
                Global.mapComm.General.saveMuteSound(isMuted);
            }

            events.dispatchEvent(new Event(STATE_CHANGE));
        }

        public function get isMuted(): Boolean {
            return _isMuted;
        }

        public function set volume(value: Number): void {
            _volume = value;

            if (player) {
                player.volume = value;
            }
        }
    }
}
