using System;
using Game.Data;
using Game.Data.Store;
using Game.Setup;
using Game.Util.Locking;

namespace Game.Comm.ProcessorCommands
{
    class StoreCommandsModule : CommandModule
    {
        private readonly IStoreManager storeManager;

        private readonly IThemeManager themeManager;

        private readonly ILocker locker;

        public StoreCommandsModule(IStoreManager storeManager, IThemeManager themeManager, ILocker locker)
        {
            this.storeManager = storeManager;
            this.themeManager = themeManager;
            this.locker = locker;
        }

        public override void RegisterCommands(IProcessor processor)
        {
            processor.RegisterCommand(Command.StoreGetItems, GetItems);
            processor.RegisterCommand(Command.StorePurchaseItem, PurchaseItem);
            processor.RegisterCommand(Command.StoreSetDefaultTheme, SetDefaultTheme);
            processor.RegisterCommand(Command.StoreSetTroopTheme, SetTroopTheme);
            processor.RegisterCommand(Command.StoreApplyThemeToAll, ApplyThemeToAll);
        }

        private void ApplyThemeToAll(Session session, Packet packet)
        {
            uint cityId;
            string itemId;

            try
            {
                cityId = packet.GetUInt32();
                itemId = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            locker.Lock(session.Player).Do(() =>
            {
                var city = session.Player.GetCity(cityId);
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                var result = themeManager.ApplyToAll(city, itemId);

                ReplyWithResult(session, packet, result);
            });
        }

        private void SetDefaultTheme(Session session, Packet packet)
        {
            uint cityId;
            string itemId;

            try
            {
                cityId = packet.GetUInt32();
                itemId = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            locker.Lock(session.Player).Do(() =>
            {
                var city = session.Player.GetCity(cityId);
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                var result = themeManager.SetDefaultTheme(city, itemId);

                ReplyWithResult(session, packet, result);
            });
        }

        private void SetTroopTheme(Session session, Packet packet)
        {
            uint cityId;
            string itemId;

            try
            {
                cityId = packet.GetUInt32();
                itemId = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            locker.Lock(session.Player).Do(() =>
            {
                var city = session.Player.GetCity(cityId);
                if (city == null)
                {
                    ReplyError(session, packet, Error.CityNotFound);
                    return;
                }

                var result = themeManager.SetDefaultTroopTheme(city, itemId);

                ReplyWithResult(session, packet, result);
            });
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
        
        private void PurchaseItem(Session session, Packet packet)
        {
            string itemId;

            try
            {
                itemId = packet.GetString();
            }
            catch(Exception)
            {
                ReplyError(session, packet, Error.Unexpected);
                return;
            }

            var response = ApiCaller.PurchaseItem(session.Player.PlayerId, itemId);

            ReplyWithResult(session, packet, response.AsErrorEnumerable());
        }
    }
}
