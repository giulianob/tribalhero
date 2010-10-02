#region

using System;
using Game.Data;
using Game.Database;
using Game.Util;

#endregion

namespace Game.Comm {
    public partial class Processor {

        public CmdLineProcessor cmdLineProcessor = new CmdLineProcessor();

        public void CmdLineCommand(Session session, Packet packet) {

            if (!session.Player.Admin) {
                ReplyError(session, packet, Setup.Error.UNEXPECTED);
                return;
            }

            Packet reply = new Packet(packet);

            string cmd;

            try {                
                cmd = packet.GetString();

                if (string.IsNullOrEmpty(cmd)) throw new Exception();
            }
            catch (Exception) {
                ReplyError(session, packet, Setup.Error.UNEXPECTED);
                return;
            }

            string[] cmdParts = cmd.Split(new[] { ' ' }, 2);

            if (cmdParts.Length < 1) {
                ReplyError(session, packet, Setup.Error.UNEXPECTED);
                return;
            }

            string parms = cmdParts.Length == 2 ? cmdParts[1] : string.Empty;

            string output = cmdLineProcessor.Execute(cmdParts[0], parms);

            reply.AddString(output);
            session.Write(reply);
        }
    }
}