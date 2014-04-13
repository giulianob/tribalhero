using System;
using System.CodeDom;
using System.Diagnostics;
using Game.Data;
using Game.Util;
using NDesk.Options;

namespace Game.Comm
{
    public class SystemCommandLineModule : CommandLineModule
    {
        private readonly INetworkServer networkServer;

        public SystemCommandLineModule(INetworkServer networkServer)
        {
            this.networkServer = networkServer;
        }

        public override void RegisterCommands(CommandLineProcessor processor)
        {
            processor.RegisterCommand("sessionstatus", SessionStatus, PlayerRights.Bureaucrat);            
            processor.RegisterCommand("disconnectall", DisconnectAll, PlayerRights.Bureaucrat);
            processor.RegisterCommand("gccollect", GcCollect, PlayerRights.Bureaucrat);
            processor.RegisterCommand("gccreategarbage", GcCreateGarbage, PlayerRights.Bureaucrat);
        }

        private string GcCreateGarbage(Session session, string[] parms)
        {
            bool help = false;
            int? blockSize = null;
            int? numberOfBlocks = null;

            try
            {
                var p = new OptionSet
                {
                        {"?|help|h", v => help = true},
                        {"blocksize=", v => blockSize = int.Parse(v.TrimMatchingQuotes())},
                        {"blocks=", v => numberOfBlocks = int.Parse(v.TrimMatchingQuotes())},
                };
                p.Parse(parms);
            }
            catch(Exception)
            {
                help = true;
            }

            if (help || blockSize == null || numberOfBlocks == null)
            {
                return "gccreategarbage --blocksize=bytearraysize --blocks=numberofarrays";
            }

            var beforeAlloc = GetMemoryUsage();

            var random = new Random();
            var randomArr = new byte[blockSize.Value];
            random.NextBytes(randomArr);

            for (var i = 1; i < numberOfBlocks; i++)
            {
                var arr = new byte[blockSize.Value];
                Array.Copy(randomArr, arr, arr.Length);
            }

            var afterAlloc = GetMemoryUsage();

            return string.Format("OK. Memory Before: {0}MB After: {1}MB", beforeAlloc, afterAlloc);
        }

        private string GcCollect(Session session, string[] parms)
        {
            var beforeCleanUp = GetMemoryUsage();

            GC.Collect(GC.MaxGeneration);

            var afterCleanUp = GetMemoryUsage();

            return string.Format("OK. Memory Before: {0}MB After: {1}MB", beforeCleanUp, afterCleanUp);
        }

        private string SessionStatus(Session session, string[] parms)
        {
            return networkServer.GetAllSessionStatus();
        }

        private string DisconnectAll(Session session, string[] parms)
        {
            return networkServer.DisconnectAll();
        }

        private int GetMemoryUsage()
        {
            return (int)(Process.GetCurrentProcess().WorkingSet64 / 1048576);
        }
    }
}