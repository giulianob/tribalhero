using System.Collections.Generic;

namespace Game.Data
{
    public interface IThemeManager
    {
        List<Theme> Themes { get; }

        void UpdateThemes(IEnumerable<Theme> newThemes);

        bool HasTheme(string id);
    }
}