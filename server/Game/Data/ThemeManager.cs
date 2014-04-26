using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.Setup;

namespace Game.Data
{
    public class ThemeManager : IThemeManager
    {
        private readonly List<Theme> themes = new List<Theme>();

        private readonly ReaderWriterLockSlim updateLock = new ReaderWriterLockSlim();

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

        public bool HasTheme(string id)
        {
            if (id == "DEFAULT")
            {
                return true;
            }

            updateLock.EnterReadLock();
            var result = themes.Any(theme => theme.Id == id);
            updateLock.ExitReadLock();

            return result;
        }
    }
}