using System.Collections.Generic;
using Game.Setup;

namespace Game.Data
{
    public interface IThemeManager
    {
        List<Theme> Themes { get; }

        void UpdateThemes(IEnumerable<Theme> newThemes);

        bool HasTheme(IPlayer player, string id);

        Error SetDefaultTheme(ICity city, string id);

        Error ApplyToAll(ICity city, string id);

        Error SetStructureTheme(IStructure structure, string id);
    }
}