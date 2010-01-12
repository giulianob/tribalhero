#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Map;
using Game.Setup;

#endregion

namespace Game {
    public class Region {
        #region Constants

        public const int TILE_SIZE = 2;

        #endregion

        #region Members

        private ObjectList objlist = new ObjectList();
        private object objLock = new object();
        private byte[] map;
        private byte[] objects;
        private bool isDirty;

        public ushort Count {
            get {
                ushort count;

                lock (objlist) {
                    count = objlist.Count;
                }

                return count;
            }
        }

        public object Lock {
            get { return objLock; }
        }

        #endregion

        #region Constructors

        public Region() {}

        public Region(Byte[] bytes) {
            map = (byte[]) bytes.Clone();
        }

        #endregion

        #region Methods

        public bool add(GameObject obj) {
            lock (objlist) {
                objlist.addGameObject(obj);
                isDirty = true;
            }

            return true;
        }

        public void remove(GameObject obj) {
            lock (objlist) {
                objlist.remove(obj);
                isDirty = true;
            }
        }

        public void remove(GameObject obj, uint origX, uint origY) {
            lock (objlist) {
                objlist.remove(obj, origX, origY);
                isDirty = true;
            }
        }

        public void update(GameObject obj, uint origX, uint origY) {
            lock (objlist) {
                if (obj.X != origX || obj.Y != origY) {
                    if (!objlist.remove(obj, origX, origY))
                        throw new Exception("WTF");
                    objlist.addGameObject(obj);
                }
                isDirty = true;
            }
        }

        public List<GameObject> getObjects(uint x, uint y) {
            return objlist.get(x, y);
        }

        public byte[] getObjectBytes() {
            if (isDirty || objects == null) {
                lock (objlist) {
                    using (MemoryStream ms = new MemoryStream()) {
                        BinaryWriter bw = new BinaryWriter(ms);
                        bw.Write(Count);
                        foreach (GameObject obj in objlist) {
                            bw.Write(obj.Lvl);
                            bw.Write(obj.Type);
                            bw.Write(obj.City.Owner.PlayerId);
                            bw.Write(obj.City.CityId);
                            bw.Write(obj.ObjectId);
                            bw.Write((ushort) (obj.RelX));
                            bw.Write((ushort) (obj.RelY));
                            bw.Write((byte) obj.State.Type);
                            foreach (object parameter in obj.State.Parameters) {
                                if (parameter is byte)
                                    bw.Write((byte) parameter);
                                else if (parameter is short)
                                    bw.Write((short) parameter);
                                else if (parameter is int)
                                    bw.Write((int) parameter);
                                else if (parameter is ushort)
                                    bw.Write((ushort) parameter);
                                else if (parameter is uint)
                                    bw.Write((uint) parameter);
                                else if (parameter is string)
                                    bw.Write((string) parameter);
                            }

                            //if this is the main building then include radius
                            if (obj.ObjectId == obj.City.MainBuilding.ObjectId)
                                bw.Write(obj.City.Radius);
                        }

                        isDirty = false;

                        ms.Position = 0;
                        objects = ms.ToArray();
                    }
                }
            }

            return objects;
        }

        public byte[] getBytes() {
            return map;
        }

        public ushort getTileType(uint x, uint y) {
            return BitConverter.ToUInt16(map, getTileIndex(x, y)*2);
        }

        #endregion

        #region Static Util Methods

        public static ushort getRegionIndex(GameObject obj) {
            return getRegionIndex(obj.X, obj.Y);
        }

        public static ushort getRegionIndex(uint x, uint y) {
            return (ushort) (x/Config.region_width + (y/Config.region_height)*Config.column);
        }

        public static int getTileIndex(GameObject obj) {
            return getTileIndex(obj.X, obj.Y);
        }

        public static int getTileIndex(uint x, uint y) {
            return (int) (x%Config.region_width + (y%Config.region_height)*Config.region_width);
        }

        #endregion
    }
}