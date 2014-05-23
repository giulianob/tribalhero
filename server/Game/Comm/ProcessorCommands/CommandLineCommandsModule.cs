#region

using System;
using Game.Setup;

#endregion

namespace Game.Comm.ProcessorCommands
{
    class CommandLineCommandsModule : CommandModule
    {
        private readonly CommandLineProcessor commandLineProcessor;

        public CommandLineCommandsModule(CommandLineProcessor commandLineProcessor)
        {
            this.commandLineProcessor = commandLineProcessor;
        }

        public override void RegisterCommands(IProcessor processor)
        {
            processor.RegisterCommand(Command.CmdLine, CommandLine);
        }

        private void CommandLine(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            string cmd;

            try
            {
                cmd = packet.GetString();

                if (string.IsNullOrEmpty(cmd))
                {
                    throw new Exception();
                }
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

            string output = commandLineProcessor.Execute(session, cmdParts[0].Trim(), parms);

            reply.AddString(output);
            session.Write(reply);
        }
    }
}