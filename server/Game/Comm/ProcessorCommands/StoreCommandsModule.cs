using Game.Data.Store;
using Game.Util;

namespace Game.Comm.ProcessorCommands
{
    class StoreCommandsModule : CommandModule
    {
        private readonly IStoreManager storeManager;

        public StoreCommandsModule(IStoreManager storeManager)
        {
            this.storeManager = storeManager;
        }

        public override void RegisterCommands(IProcessor processor)
        {
            processor.RegisterCommand(Command.StoreGetItems, GetItems);
        }

        private void GetItems(Session session, Packet packet)
        {
            var reply = new Packet(packet);

            reply.AddInt32(storeManager.ItemsCount);
            foreach (var item in storeManager.Items)
            {
                reply.AddInt32(item.Type);
                reply.AddString(item.Id);
                reply.AddInt32(item.Cost);
                reply.AddDate(item.Created);
            }

            session.Write(reply);
        }
    }
}
