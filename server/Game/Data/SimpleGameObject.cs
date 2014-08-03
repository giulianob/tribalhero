#region

using System;
using System.ComponentModel;
using Game.Data.Events;
using Game.Map;
using Game.Util;

#endregion

namespace Game.Data
{
    public abstract class SimpleGameObject : ISimpleGameObject
    {
        public event EventHandler<SimpleGameObjectArgs> ObjectUpdated;

        public enum SystemGroupIds : uint
        {
            Forest = 10000002,

            Stronghold = 10000003,

            BarbarianTribe = 10000004
        }

        public enum Types : ushort
        {
            Troop = 100,

            Forest = 200,

            Stronghold = 300,

            BarbarianTribe = 400,
        }

        private readonly uint objectId;

        #region Properties

        private bool inWorld;

        private GameObjectState state = GameObjectStateFactory.NormalState();

        public bool InWorld
        {
            get
            {
                return inWorld;
            }
            set
            {
                CheckUpdateMode();
                inWorld = value;
                SaveOrigPos();
            }
        }

        public GameObjectState State
        {
            get
            {
                return state;
            }
            set
            {
                CheckUpdateMode();
                state = value;
            }
        }

        public Position PrimaryPosition { get; private set; }

        public abstract byte Size { get; }

        public abstract ushort Type { get; }

        public abstract uint GroupId { get; }

        public uint ObjectId
        {
            get
            {
                return objectId;
            }          
        }

        #endregion

        #region Update Events

        private Position originalPosition = new Position();

        #endregion

        #region Constructors

        protected SimpleGameObject(uint objectId, uint x, uint y)
        {
            this.objectId = objectId;
            this.PrimaryPosition = new Position(x, y);
            this.state = GameObjectStateFactory.NormalState();
        }

        protected bool Updating;

        public virtual void BeginUpdate()
        {
            if (Updating)
            {
                throw new Exception("Nesting beginupdate");
            }

            Updating = true;

            SaveOrigPos();
        }

        protected abstract void CheckUpdateMode();

        public void EndUpdate()
        {
            if (!Updating)
            {
                throw new Exception("Called endupdate without first calling begin update");
            }

            Updating = false;
            Update();
        }
        protected virtual bool Update()
        {
            if (!Global.Current.FireEvents)
            {
                return false;
            }

            if (Updating)
            {
                return false;
            }

            ObjectUpdated.Raise(this, new SimpleGameObjectArgs(this) {OriginalX = originalPosition.X, OriginalY = originalPosition.Y});

            return true;
        }

        #endregion

        public void Move(Position newPosition)
        {
            CheckUpdateMode();
         
            PrimaryPosition = newPosition;
            
            if (!inWorld)
            {
                originalPosition = newPosition;
            }
        }

        private void SaveOrigPos()
        {
            if (InWorld)
            {
                originalPosition = PrimaryPosition;
            }
        }
        
        public override string ToString()
        {
            return string.Format("{0} x[{1}] y[{2}] origX[{7}] origY[{8}] type[{3}] groupId[{4}] objId[{5}] inWorld[{6}]", base.ToString(), PrimaryPosition.X, PrimaryPosition.Y, Type, GroupId, ObjectId, inWorld, originalPosition.X, originalPosition.Y);
        }

        public abstract int Hash { get; }

        public abstract object Lock { get; }
    }
}