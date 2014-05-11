using System;

namespace Game.Data
{
    public class Theme
    {
        public string Id { get; set; }
        
        public int MinimumVersion { get; set; }

        public Theme(string id, int minimumVersion)
        {
            Id = id;
            MinimumVersion = minimumVersion;
        }
    }
}