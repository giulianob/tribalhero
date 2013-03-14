using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

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

        #region Implementation of ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Name", Name);
            info.AddValue("Permission", Permission, typeof(TribePermission));
        }

        #endregion
    }
}
