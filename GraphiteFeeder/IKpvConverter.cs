using System.Collections.Generic;

namespace GraphiteFeeder
{
    interface IKpvConverter : IEnumerable<KeyValuePair<string, long>> 
    {
    }
}
