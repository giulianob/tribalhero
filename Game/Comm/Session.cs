#region

using Game.Data;
using Game.Util;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Comm
{
    public abstract class Session : IChannelListener
    {
        protected readonly ILogger Logger = LoggerFactory.Current.GetCurrentClassLogger();

        #region Delegates

        public delegate void CloseCallback(Session sender);

        public event CloseCallback OnClose;

        #endregion

        private readonly Processor processor;

        protected Session(string name, Processor processor)
        {
            Name = name;
            Processor = processor;
            PacketMaker = new PacketMaker();
        }

        public PacketMaker PacketMaker { get; private set; }

        public string Name { get; private set; }

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

        public abstract bool Write(Packet packet);

        public void CloseSession()
        {
            if (OnClose != null)
            {
                OnClose(this);
            }
        }

        public void Process(object obj)
        {
            var packet = (Packet)obj;

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

            if (Processor != null)
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Processing IP[{0}] {1}", Name, packet.ToString());
                }

                Processor.Execute(this, packet);
            }
        }

        public void ProcessEvent(object obj)
        {
            if (Processor != null)
            {
                Processor.ExecuteEvent(this, (Packet)obj);
            }
        }
    }
}