using System.Collections.Generic;
using Game.Data.Stronghold;
using Game.Data.Troop;
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

        Error SetWallTheme(ICity city, string theme);
        
        Error SetRoadTheme(ICity city, string theme);

        Error SetStrongholdTheme(IStronghold stronghold, IPlayer player, string id);

        Error SetTroopTheme(ITroopObject troop, string id);

        Error SetDefaultTroopTheme(ICity city, string id);
    }
}