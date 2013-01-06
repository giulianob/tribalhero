#region

using Game.Data;
using Game.Util;

#endregion

namespace Game.Comm
{
    public abstract class Session : IChannel
    {
        #region Delegates        

        public delegate void CloseCallback(Session sender);

        public event CloseCallback OnClose;

        #endregion

        protected Processor processor;

        protected Session(string name, Processor processor)
        {
            Name = name;
            this.processor = processor;
            PacketMaker = new PacketMaker();
        }

        public PacketMaker PacketMaker { get; private set; }

        public string Name { get; private set; }

        public bool IsLoggedIn
        {
            get
            {
                return Player != null;
            }
        }

        public IPlayer Player { get; set; }

        #region IChannel Members

        public void OnPost(Packet message)
        {
            Write(message);
        }

        #endregion

        public abstract bool Write(Packet packet);

        public void CloseSession()
        {
            Close();
        }

        protected void Close()
        {
            if (OnClose != null)
            {
                OnClose(this);
            }
        }

        public void Process(object obj)
        {
            var p = (Packet)obj;

            if (!IsLoggedIn && p.Cmd != Command.Login)
            {
                return;
            }

            if (IsLoggedIn)
            {
                if (p.Cmd == Command.Login)
                {
                    return;
                }

                if (Player.GetCityCount() == 0 && p.Cmd != Command.CityCreateInitial)
                {
                    return;
                }
            }

            if (processor != null)
            {
                processor.Execute(this, p);
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