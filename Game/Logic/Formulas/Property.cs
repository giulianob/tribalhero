#region

using Game.Data;
using Game.Module;

#endregion

namespace Game.Logic {
    public partial class Formula {
        public static ushort CropPrice(Structure structure) {
            return (ushort) Market.Crop.Price;
        }

        public static ushort WoodPrice(Structure structure) {
            return (ushort) Market.Wood.Price;
        }

        public static ushort IronPrice(Structure structure) {
            return (ushort) Market.Iron.Price;
        }
    }
}