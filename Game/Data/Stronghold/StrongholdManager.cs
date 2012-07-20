using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Game.Data.Tribe;
using Game.Map;
using Game.Util;

namespace Game.Data.Stronghold
{
    class StrongholdManager : IStrongholdManager
    {
        private readonly ConcurrentDictionary<uint, IStronghold> strongholds = new ConcurrentDictionary<uint, IStronghold>();

        private readonly IStrongholdFactory strongholdFactory;
        private readonly IStrongholdConfigurator strongholdConfigurator;
        private readonly IdGenerator idGenerator;
        private IWorld world;

        public StrongholdManager(IdGenerator idGenerator,
                                    IStrongholdConfigurator strongholdConfigurator,
                                    IStrongholdFactory strongholdFactory,
                                    IWorld world)
        {
            this.idGenerator = idGenerator;
            this.strongholdConfigurator = strongholdConfigurator;
            this.strongholdFactory = strongholdFactory;
            this.world = world;
        }

        public int Count
        {
            get
            {
                return strongholds.Count;
            }
        }

        public void Add(IStronghold stronghold)
        {
            strongholds.AddOrUpdate(stronghold.Id, stronghold, (id, old) => stronghold);
        }

        public bool TryGetValue(uint id, out IStronghold stronghold)
        {
            return strongholds.TryGetValue(id, out stronghold);
        }

        public void Generate(int count)
        {
            while (count-- > 0)
            {
                string name;
                byte level;
                uint x, y;

                if (!strongholdConfigurator.Next(out name, out level, out x, out y))
                    break;
                IStronghold stronghold = strongholdFactory.CreateStronghold((uint)idGenerator.GetNext(), name, level, x, y);
                Add(stronghold);
            }
        }

        public void Activate(IStronghold stronghold)
        {
            stronghold.StrongholdState = StrongholdState.Neutral;
            stronghold.BeginUpdate();
            world.Add(stronghold);
            stronghold.EndUpdate();
        }

        public void TransferTo(IStronghold stronghold, ITribe tribe)
        {
            stronghold.BeginUpdate();
            stronghold.StrongholdState = tribe == null ? StrongholdState.Neutral : StrongholdState.Occupied;
            ++stronghold.Lvl;
            stronghold.Tribe = tribe;
            stronghold.EndUpdate();
        }

        #region Implementation of IEnumerable

        public IEnumerator<IStronghold> GetEnumerator()
        {
            return strongholds.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
