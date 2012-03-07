using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Comm;
using Game.Data;

namespace Game.Map
{
    public class TileManager
    {
        protected void SendUpdate(Dictionary<ushort, List<TileUpdate>> updates)
        {
            foreach (var list in updates)
            {
                var packet = new Packet(Command.RegionSetTile);
                packet.AddUInt16((ushort)list.Value.Count);
                foreach (var update in list.Value)
                {
                    packet.AddUInt32(update.X);
                    packet.AddUInt32(update.Y);
                    packet.AddUInt16(update.TileType);
                }

                Global.Channel.Post("/WORLD/" + list.Key, packet);
            }
        }
        #region Nested type: TileUpdate

        /// <summary>
        ///   Simple wrapper to keep track of tiles that were updated so we can send it in one shot to the client.
        /// </summary>
        protected class TileUpdate
        {
            public TileUpdate(uint x, uint y, ushort tileType)
            {
                X = x;
                Y = y;
                TileType = tileType;
            }

            public uint X { get; set; }
            public uint Y { get; set; }
            public ushort TileType { get; set; }
        }

        #endregion
    }
}
