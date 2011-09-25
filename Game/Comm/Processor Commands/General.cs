#region

using System;
using System.Reflection;
using Game.Setup;
using Ninject;
using Ninject.Extensions.Logging;

#endregion

namespace Game.Comm
{
    public partial class Processor
    {
        private CmdLineProcessor cmdLineProcessor;

        public void CmdLineCommand(Session session, Packet packet)
        {
            if (!session.Player.Admin)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            var reply = new Packet(packet);

            string cmd;

            try
            {
                cmd = packet.GetString();

                if (string.IsNullOrEmpty(cmd))
                    throw new Exception();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            string[] cmdParts = cmd.Split(new[] {' '}, 2);

            if (cmdParts.Length < 1)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            string parms = cmdParts.Length == 2 ? cmdParts[1] : string.Empty;

            string output = cmdLineProcessor.Execute(session,cmdParts[0].Trim(), parms);

            reply.AddString(output);
            session.Write(reply);
        }
    }
}