namespace Game.Data
{
    public class Achievement
    {
        public int Id { get; set; }
        
        public uint PlayerId { get; set; }

        public string Type { get; set; }

        public AchievementTier Tier { get; set; }

        public string Icon { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
