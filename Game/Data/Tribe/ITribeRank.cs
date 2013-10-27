namespace Game.Data.Tribe
{
    public interface ITribeRank 
    {
        byte Id { get; set; }
        string Name { get; set; }
        TribePermission Permission { get; set; }
    }
}
