using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Game.Data.Tribe
{
    public interface ITribeRank 
    {
        byte Id { get; set; }
        string Name { get; set; }
        TribePermission Permission { get; set; }
    }
}
