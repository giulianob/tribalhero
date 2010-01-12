#region

using Game.Comm;

#endregion

namespace Game {
    class Engine {
        private int state;
        private TcpServer server;

        public const int STATE_STARTED = 0;
        public const int STATE_STARTING = 1;
        public const int STATE_STOPPED = 2;
        public const int STATE_STOPPING = 3;

        public Engine() {
            state = STATE_STOPPED;
            server = new TcpServer();
        }

        public bool start() {
            if (state != STATE_STOPPED)
                return false;
            state = STATE_STOPPING;
            // load map

            // load users with all the belonging

            // start server
            server.start();
            state = STATE_STOPPED;
            return true;
        }

        public bool stop() {
            if (state != STATE_STARTED)
                return false;
            state = STATE_STOPPING;
            server.stop();
            state = STATE_STOPPED;
            return true;
        }
    }
}