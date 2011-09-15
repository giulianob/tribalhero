﻿namespace Game.Data.Troop
{
    public class Unit
    {
        public ushort Type { get; set; }
        public ushort Count { get; set; }

        public Unit(ushort type, ushort count)
        {
            Type = type;
            Count = count;
        }
    }
}
