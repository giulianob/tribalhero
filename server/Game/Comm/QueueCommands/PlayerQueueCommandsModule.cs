using System;
using Common;
using Game.Data;
using Game.Util;
using Game.Util.Locking;

namespace Game.Comm.QueueCommands
{
    public class PlayerQueueCommandsModule : IQueueCommandModule
    {
        private readonly ILocker locker;

        private readonly ILogger logger = LoggerFactory.Current.GetLogger<PlayerQueueCommandsModule>();

        public PlayerQueueCommandsModule(ILocker locker, IChannel channel)
        {
            this.locker = locker;
        }

        public void RegisterCommands(IQueueCommandProcessor processor)
        {
            processor.RegisterCommand("player.add_coins", AddCoins);
            processor.RegisterCommand("store.theme_purchased", StoreThemePurchased);
        }

        private void StoreThemePurchased(dynamic payload)
        {
            uint playerId;
            string themeId;

            try
            {
                playerId = (uint)payload.player_id;
                themeId = (string)payload.theme_id;
            }
            catch(Exception e)
            {
                logger.Error(e, "Failed to process AddCoins");
                return;
            }

            IPlayer player;
            locker.Lock(playerId, out player).Do(() =>
            {
                if (player == null)
                {                    
                    return;
                }

                player.AddTheme(themeId);
            });            
        }

        private void AddCoins(dynamic payload)
        {
            uint playerId;
            int coins;

            try
            {
                playerId = (uint)payload.player_id;
                coins = (int)payload.coins;
            }
            catch(Exception e)
            {
                logger.Error(e, "Failed to process AddCoins");
                return;
            }

            IPlayer player;
            locker.Lock(playerId, out player).Do(() =>
            {
                if (player == null)
                {                    
                    return;
                }

                player.UpdateCoins(coins);
            });
        }
    }
}