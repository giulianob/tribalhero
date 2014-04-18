#region

using Common;
using Game.Data;
using Game.Util;

#endregion

namespace Game.Comm
{
    public abstract class Session : IChannelListener
    {
        protected readonly ILogger Logger = LoggerFactory.Current.GetLogger<Session>();

        #region Delegates

        public delegate void CloseCallback(Session sender);

        public event CloseCallback OnClose;

        #endregion

        private readonly IProcessor processor;

        protected Session(string remoteIp, IProcessor processor)
        {
            RemoteIP = remoteIp;
            this.processor = processor;
            PacketMaker = new PacketMaker();
        }

        public PacketMaker PacketMaker { get; private set; }

        public string RemoteIP { get; private set; }

        private bool IsLoggedIn
        {
            get
            {
                return Player != null;
            }
        }

        public IPlayer Player { get; set; }

        #region IChannelListener Members

        public void OnPost(Packet message)
        {
            Write(message);
        }

        #endregion

        public abstract void Write(Packet packet);

        public void CloseSession()
        {
            if (OnClose != null)
            {
                OnClose(this);
            }
        }

        public void Process(Packet packet)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Processing IP[{0}] {1}", RemoteIP, packet.ToString());
            }

            if (!IsLoggedIn && packet.Cmd != Command.Login)
            {
                return;
            }

            if (IsLoggedIn)
            {
                if (packet.Cmd == Command.Login)
                {
                    return;
                }

                if (Player.GetCityCount() == 0 && packet.Cmd != Command.CityCreateInitial)
                {
                    return;
                }
            }

            if (processor != null)
            {
                processor.Execute(this, packet);
            }
        }

        public void ProcessEvent(object obj)
        {
            if (processor != null)
            {
                processor.ExecuteEvent(this, (Packet)obj);
            }
        }
    }
}