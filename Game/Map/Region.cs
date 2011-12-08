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
        private readonly ReaderWriterLockSlim objLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly ObjectList objlist = new ObjectList();
        private bool isDirty;
        private byte[] objects;

        public ReaderWriterLockSlim Lock
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
            objLock.EnterWriteLock();
            objlist.AddGameObject(obj);
            isDirty = true;
            objLock.ExitWriteLock();

            return true;
        }

        public void Remove(SimpleGameObject obj)
        {
            objLock.EnterWriteLock();
            objlist.Remove(obj);
            isDirty = true;
            objLock.ExitWriteLock();
        }

        public void Remove(SimpleGameObject obj, uint origX, uint origY)
        {
            objLock.EnterWriteLock();
            objlist.Remove(obj, origX, origY);
            isDirty = true;
            objLock.ExitWriteLock();
        }

        public void Update(SimpleGameObject obj, uint origX, uint origY)
        {
            objLock.EnterWriteLock();
            if (obj.X != origX || obj.Y != origY)
            {
                if (!objlist.Remove(obj, origX, origY))
                    throw new Exception("WTF");

                objlist.AddGameObject(obj);
            }
            isDirty = true;
            objLock.ExitWriteLock();
        }

        public List<SimpleGameObject> GetObjects(uint x, uint y)
        {
            objLock.EnterReadLock();
            var gameObjects = objlist.Get(x, y);
            objLock.ExitReadLock();
            return gameObjects;
        }

        public IEnumerable<SimpleGameObject> GetObjects()
        {
            objLock.EnterReadLock();
            foreach (var obj in objlist)
            {
                yield return obj;
            }
            objLock.ExitReadLock();
        }

        public byte[] GetObjectBytes()
        {
            if (!isDirty && objects != null)
                return objects;

            objLock.EnterWriteLock();
            if (isDirty || objects == null)
            {
                using (var ms = new MemoryStream())
                {
                    var bw = new BinaryWriter(ms);

                    // Write map tiles
                    bw.Write(map);

                    // Write objects
                    bw.Write(objlist.Count);
                    foreach (SimpleGameObject obj in objlist)
                    {
                        bw.Write(obj.Type);
                        bw.Write((ushort)(obj.RelX));
                        bw.Write((ushort)(obj.RelY));

                        bw.Write(obj.GroupId);
                        bw.Write(obj.ObjectId);

                        if (obj is GameObject)
                            bw.Write(((GameObject)obj).City.Owner.PlayerId);

                        if (obj is IHasLevel)
                            bw.Write(((IHasLevel)obj).Lvl);

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
            objLock.ExitWriteLock();

            return objects;
        }

        public ushort GetTileType(uint x, uint y)
        {
            objLock.EnterReadLock();
            var tileType = BitConverter.ToUInt16(map, GetTileIndex(x, y)*2);
            objLock.ExitReadLock();
            return tileType;
        }

        public void SetTileType(uint x, uint y, ushort tileType)
        {
            objLock.EnterWriteLock();
            int idx = GetTileIndex(x, y)*TILE_SIZE;
            byte[] ushortArr = BitConverter.GetBytes(tileType);
            map[idx] = ushortArr[0];
            map[idx + 1] = ushortArr[1];

            isDirty = true;
            objLock.ExitWriteLock();
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