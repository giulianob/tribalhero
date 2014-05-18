using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Setup;
using Game.Util.Locking;

namespace Game.Data
{
    public class ThemeManager : IThemeManager
    {
        private readonly List<Theme> themes = new List<Theme>();

        private readonly ReaderWriterLockSlim updateLock = new ReaderWriterLockSlim();

        private readonly ILocker locker;

        public ThemeManager(ILocker locker)
        {
            this.locker = locker;
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
            if (id == "DEFAULT")
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
    }
}