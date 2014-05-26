using System;
using Common;
using Game.Data;
using Game.Util;
using Game.Util.Locking;
using Newtonsoft.Json;

namespace Game.Comm.QueueCommands
{
    public class PlayerQueueCommandsModule : IQueueCommandModule
    {
        private readonly ILocker locker;

        private readonly ILogger logger = LoggerFactory.Current.GetLogger<PlayerQueueCommandsModule>();

        public PlayerQueueCommandsModule(ILocker locker)
        {
            this.locker = locker;
        }

        public void RegisterCommands(IQueueCommandProcessor processor)
        {
            processor.RegisterCommand("player.add_coins", AddCoins);
            processor.RegisterCommand("store.theme_purchased", StoreThemePurchased);
            processor.RegisterCommand("store.achievement_purchased", StoreAchievementPurchased);
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
        
        private void StoreAchievementPurchased(dynamic payload)
        {
            Achievement achievement;

            try
            {
                achievement = new Achievement
                {
                    Id = payload.id,
                    Icon = payload.icon,
                    Description = payload.description,
                    PlayerId = payload.player_id,
                    Tier = (AchievementTier)payload.tier,
                    Title = payload.title,
                    Type = payload.type
                };
            }
            catch(Exception e)
            {
                logger.Error(e, "Failed to process AddCoins");
                return;
            }

            IPlayer player;
            locker.Lock(achievement.PlayerId, out player).Do(() =>
            {
                if (player == null)
                {                    
                    return;
                }

                player.AddAchievement(achievement);
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