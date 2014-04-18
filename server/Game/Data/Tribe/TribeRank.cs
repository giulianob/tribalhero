using System.Text.RegularExpressions;

namespace Game.Data.Tribe
{
    public class TribeRank : ITribeRank
    {
        public byte Id { get; set; }

        public string Name { get; set; }

        public TribePermission Permission { get; set; }

        public TribeRank(byte id)
        {
            Id = id;
        }

        public static bool IsNameValid(string name)
        {
            return !string.IsNullOrEmpty(name) && name.Length <= 16 && Regex.IsMatch(name, Global.ALPHANUMERIC_NAME, RegexOptions.IgnoreCase);
        }
    }
}
