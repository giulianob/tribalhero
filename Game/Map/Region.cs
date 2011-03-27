#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Game.Data;
using Game.Setup;

#endregion

namespace Game.Map
{
    public class Region
    {
        #region Constants

        public const int TILE_SIZE = 2;

        #endregion

        #region Members

        private readonly byte[] map;
        private readonly object objLock = new object();
        private readonly ObjectList objlist = new ObjectList();
        private bool isDirty;
        private byte[] objects;

        public ushort Count
        {
            get
            {
                ushort count;

                lock (objlist)
                {
                    count = objlist.Count;
                }

                return count;
            }
        }

        public object Lock
        {
            get
            {
                return objLock;
            }
        }

        #endregion

        #region Constructors

        public Region(byte[] map)
        {
            this.map = map;
        }

        #endregion

        #region Methods

        public bool Add(SimpleGameObject obj)
        {
            lock (objlist)
            {
                objlist.AddGameObject(obj);
                isDirty = true;
            }

            return true;
        }

        public void Remove(SimpleGameObject obj)
        {
            lock (objlist)
            {
                objlist.Remove(obj);
                isDirty = true;
            }
        }

        public void Remove(SimpleGameObject obj, uint origX, uint origY)
        {
            lock (objlist)
            {
                objlist.Remove(obj, origX, origY);
                isDirty = true;
            }
        }

        public void Update(SimpleGameObject obj, uint origX, uint origY)
        {
            lock (objlist)
            {
                if (obj.X != origX || obj.Y != origY)
                {
                    if (!objlist.Remove(obj, origX, origY))
                        throw new Exception("WTF");
                    objlist.AddGameObject(obj);
                }
                isDirty = true;
            }
        }

        public List<SimpleGameObject> GetObjects(uint x, uint y)
        {
            lock (objlist)
            {
                return objlist.Get(x, y);
            }
        }

        public IEnumerable<SimpleGameObject> GetObjects()
        {
            return objlist;
        }

        public byte[] GetObjectBytes()
        {
            if (isDirty || objects == null)
            {
                lock (objlist)
                {
                    if (isDirty || objects == null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            var bw = new BinaryWriter(ms);
                            bw.Write(Count);
                            foreach (SimpleGameObject obj in objlist)
                            {
                                bw.Write(obj.Lvl);
                                bw.Write(obj.Type);

                                if (obj is GameObject)
                                {
                                    bw.Write(((GameObject)obj).City.Owner.PlayerId);
                                    bw.Write(((GameObject)obj).City.Id);
                                }
                                else
                                {
                                    bw.Write((uint)0);
                                    bw.Write((uint)0);
                                }

                                bw.Write(obj.ObjectId);
                                bw.Write((ushort)(obj.RelX));
                                bw.Write((ushort)(obj.RelY));
                                bw.Write((byte)obj.State.Type);
                                foreach (var parameter in obj.State.Parameters)
                                {
                                    if (parameter is byte)
                                        bw.Write((byte)parameter);
                                    else if (parameter is short)
                                        bw.Write((short)parameter);
                                    else if (parameter is int)
                                        bw.Write((int)parameter);
                                    else if (parameter is ushort)
                                        bw.Write((ushort)parameter);
                                    else if (parameter is uint)
                                        bw.Write((uint)parameter);
                                    else if (parameter is string)
                                        bw.Write((string)parameter);
                                }

                                //if this is the main building then include radius
                                if (obj is Structure)
                                {
                                    var structure = obj as Structure;

                                    if (structure.IsMainBuilding)
                                        bw.Write(structure.City.Radius);
                                }
                            }

                            isDirty = false;

                            ms.Position = 0;
                            objects = ms.ToArray();
                        }
                    }

                }                
            }

            return objects;
        }

        public byte[] GetBytes()
        {
            lock (objlist)
            {
                return map;
            }
        }

        public ushort GetTileType(uint x, uint y)
        {
            lock (objlist)
            {
                return BitConverter.ToUInt16(map, GetTileIndex(x, y)*2);
            }
        }

        public void SetTileType(uint x, uint y, ushort tileType)
        {
            lock (objlist)
            {
                int idx = GetTileIndex(x, y)*TILE_SIZE;
                byte[] ushortArr = BitConverter.GetBytes(tileType);
                map[idx] = ushortArr[0];
                map[idx + 1] = ushortArr[1];
            }
        }

        #endregion

        #region Static Util Methods

        public static ushort GetRegionIndex(SimpleGameObject obj)
        {
            return GetRegionIndex(obj.X, obj.Y);
        }

        public static ushort GetRegionIndex(uint x, uint y)
        {
            return (ushort)(x/Config.region_width + (y/Config.region_height)*Config.column);
        }

        public static int GetTileIndex(SimpleGameObject obj)
        {
            return GetTileIndex(obj.X, obj.Y);
        }

        public static int GetTileIndex(uint x, uint y)
        {
            return (int)(x%Config.region_width + (y%Config.region_height)*Config.region_width);
        }

        #endregion
    }
}