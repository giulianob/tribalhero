#region

using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Map {
    public class Region {
        #region Constants

        public const int TILE_SIZE = 2;

        #endregion

        #region Members

        private readonly ObjectList objlist = new ObjectList();
        private readonly object objLock = new object();
        private readonly byte[] map;
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

        public bool Add(SimpleGameObject obj) {
            lock (objlist) {
                objlist.AddGameObject(obj);
                isDirty = true;
            }

            return true;
        }

        public void Remove(SimpleGameObject obj) {
            lock (objlist) {
                objlist.Remove(obj);
                isDirty = true;
            }
        }

        public void Remove(SimpleGameObject obj, uint origX, uint origY) {
            lock (objlist) {
                objlist.Remove(obj, origX, origY);
                isDirty = true;
            }
        }

        public void Update(SimpleGameObject obj, uint origX, uint origY) {
            lock (objlist) {
                if (obj.X != origX || obj.Y != origY) {
                    if (!objlist.Remove(obj, origX, origY))
                        throw new Exception("WTF");
                    objlist.AddGameObject(obj);
                }
                isDirty = true;
            }
        }

        public List<SimpleGameObject> GetObjects(uint x, uint y) {
            return objlist.Get(x, y);
        }

        public byte[] GetObjectBytes() {
            if (isDirty || objects == null) {
                lock (objlist) {
                    using (MemoryStream ms = new MemoryStream()) {
                        BinaryWriter bw = new BinaryWriter(ms);
                        bw.Write(Count);
                        foreach (SimpleGameObject obj in objlist) {
                            bw.Write(obj.Lvl);
                            bw.Write(obj.Type);

                            if (obj is GameObject) {
                                bw.Write(((GameObject)obj).City.Owner.PlayerId);
                                bw.Write(((GameObject)obj).City.Id);
                            }
                            else {
                                bw.Write((uint) 0);
                                bw.Write((uint) 0);
                            }

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
                            if (obj is GameObject && obj == ((GameObject)obj).City.MainBuilding)
                                bw.Write(((GameObject)obj).City.Radius);
                        }

                        isDirty = false;

                        ms.Position = 0;
                        objects = ms.ToArray();
                    }
                }
            }

            return objects;
        }

        public byte[] GetBytes() {
            return map;
        }

        public ushort GetTileType(uint x, uint y) {
            return BitConverter.ToUInt16(map, GetTileIndex(x, y)*2);
        }

        #endregion

        #region Static Util Methods

        public static ushort GetRegionIndex(SimpleGameObject obj) {
            return GetRegionIndex(obj.X, obj.Y);
        }

        public static ushort GetRegionIndex(uint x, uint y) {
            return (ushort) (x/Config.region_width + (y/Config.region_height)*Config.column);
        }

        public static int GetTileIndex(SimpleGameObject obj) {
            return GetTileIndex(obj.X, obj.Y);
        }

        public static int GetTileIndex(uint x, uint y) {
            return (int) (x%Config.region_width + (y%Config.region_height)*Config.region_width);
        }

        #endregion
    }
}