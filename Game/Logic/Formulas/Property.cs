using System;
using System.Collections.Generic;
using System.Text;
using Game.Module;
using Game.Data;

namespace Game.Logic {
    public partial class Formula {
        public static ushort CropPrice(Structure structure) {
            return (ushort)Market.Crop.Price;
        }
        public static ushort WoodPrice(Structure structure) {
            return (ushort)Market.Wood.Price;
        }
        public static ushort IronPrice(Structure structure) {
            return (ushort)Market.Iron.Price;
        }
        public static byte TradeAvailable(Structure structure) {
            return (byte)((ushort)structure["Efficiency"]/100);
        }
    }
}
