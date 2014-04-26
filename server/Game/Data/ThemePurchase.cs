using System;

namespace Game.Data
{
    public class ThemePurchase
    {
        public int Id { get; set; }

        public string ThemeId { get; set; }

        public int Price { get; set; }

        public DateTime Created { get; set; }
    }
}