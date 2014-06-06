using System;

namespace Game.Data
{
    public class Theme
    {
        public const string DEFAULT_THEME_ID = "DEFAULT";

        public string Id { get; set; }
        
        public int MinimumVersion { get; set; }

        public Theme(string id, int minimumVersion)
        {
            Id = id;
            MinimumVersion = minimumVersion;
        }
    }
}