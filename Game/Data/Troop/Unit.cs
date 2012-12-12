namespace Game.Data.Troop
{
    public class Unit
    {
        public Unit(ushort type, ushort count)
        {
            Type = type;
            Count = count;
        }

        public ushort Type { get; set; }

        public ushort Count { get; set; }
    }
}