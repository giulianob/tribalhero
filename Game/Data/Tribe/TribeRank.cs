namespace Game.Data.Tribe
{
    public class TribeRank : ITribeRank 
    {
        #region Implementation of ITribeRank

        public byte Id { get; private set; }
        public string Name { get; set; }
        public TribePermission Permission { get; set; }

        #endregion

        public TribeRank(byte id)
        {
            Id = id;
        }
    }
}
