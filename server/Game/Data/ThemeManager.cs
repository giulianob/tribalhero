using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Data.Stronghold;
using Game.Data.Troop;
using Game.Map;
using Game.Setup;

namespace Game.Data
{
    public class ThemeManager : IThemeManager
    {
        private readonly IRoadManager roadManager;

        private readonly IRegionManager regionManager;
        
        private readonly ITileLocator tileLocator;

        private readonly List<Theme> themes = new List<Theme>();

        private readonly ReaderWriterLockSlim updateLock = new ReaderWriterLockSlim();

        public ThemeManager(IRoadManager roadManager, IRegionManager regionManager, ITileLocator tileLocator)
        {
            this.roadManager = roadManager;
            this.regionManager = regionManager;
            this.tileLocator = tileLocator;
        }

        public List<Theme> Themes
        {
            get
            {
                updateLock.EnterReadLock();
                var themeCopy = themes.ToList();
                updateLock.ExitReadLock();

                return themeCopy;
            }
        }

        public void UpdateThemes(IEnumerable<Theme> newThemes)
        {
            updateLock.EnterWriteLock();
            themes.Clear();
            themes.AddRange(newThemes.Where(theme => Config.client_min_version == 0 || Config.client_min_version >= theme.MinimumVersion));
            updateLock.ExitWriteLock();
        }

        public bool HasTheme(IPlayer player, string id)
        {
            if (id == Theme.DEFAULT_THEME_ID)
            {
                return true;
            }

            updateLock.EnterReadLock();
            var result = themes.Any(theme => theme.Id == id);
            updateLock.ExitReadLock();

            if (!result)
            {
                return false;
            }

            return player.HasPurchasedTheme(id);
        }

        public Error SetDefaultTheme(ICity city, string id)
        {
            if (!HasTheme(city.Owner, id))
            {
                return Error.ThemeNotPurchased;
            }

            city.BeginUpdate();
            city.DefaultTheme = id;
            city.EndUpdate();

            return Error.Ok;
        }

        public Error ApplyToAll(ICity city, string id)
        {
            if (!HasTheme(city.Owner, id))
            {
                return Error.ThemeNotPurchased;
            }

            city.BeginUpdate();
            city.WallTheme = id;
            city.EndUpdate();

            foreach (var structure in city)
            {
                structure.BeginUpdate();
                structure.Theme = id;
                structure.EndUpdate();
            }

            return Error.Ok;
        }

        public Error SetStructureTheme(IStructure structure, string id)
        {
            if (!HasTheme(structure.City.Owner, id))
            {
                return Error.ThemeNotPurchased;
            }

            structure.BeginUpdate();
            structure.Theme = id;
            structure.EndUpdate();

            return Error.Ok;
        }

        public Error SetStrongholdTheme(IStronghold stronghold, IPlayer player, string id)
        {
            if (!HasTheme(player, id))
            {
                return Error.ThemeNotPurchased;
            }

            stronghold.BeginUpdate();
            stronghold.Theme = id;
            stronghold.EndUpdate();

            return Error.Ok;
        }

        public Error SetTroopTheme(ITroopObject troop, string id)
        {
            if (!HasTheme(troop.City.Owner, id))
            {
                return Error.ThemeNotPurchased;
            }

            troop.BeginUpdate();
            troop.Theme = id;
            troop.EndUpdate();

            return Error.Ok;
        }

        public Error SetDefaultTroopTheme(ICity city, string id)
        {
            if (!HasTheme(city.Owner, id))
            {
                return Error.ThemeNotPurchased;
            }

            city.BeginUpdate();
            city.TroopTheme = id;
            city.EndUpdate();

            return Error.Ok;
        }

        public Error SetWallTheme(ICity city, string id)
        {
            if (!HasTheme(city.Owner, id))
            {
                return Error.ThemeNotPurchased;
            }
            
            city.BeginUpdate();
            city.WallTheme = id;
            city.EndUpdate();

            // Due to how the client gets wall updates we need to do force the main structure to update
            city.MainBuilding.BeginUpdate();
            city.MainBuilding.EndUpdate();

            return Error.Ok;
        }

        public Error SetRoadTheme(ICity city, string id)
        {
            if (!HasTheme(city.Owner, id))
            {
                return Error.ThemeNotPurchased;
            }

            var previousTheme = city.RoadTheme;

            city.BeginUpdate();
            city.RoadTheme = id;
            city.EndUpdate();

            var lockedRegions = regionManager.LockRegions(tileLocator.ForeachTile(city.PrimaryPosition.X, city.PrimaryPosition.Y, city.Radius));
            
            roadManager.ChangeRoadTheme(city, previousTheme, id);
            
            regionManager.UnlockRegions(lockedRegions);

            return Error.Ok;
        }
    }
}