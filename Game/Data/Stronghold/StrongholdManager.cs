using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Game.Data.Tribe;
using Game.Map;
using Game.Module;
using Game.Util;
using Persistance;

namespace Game.Data.Stronghold
{
    class StrongholdManager : IStrongholdManager
    {
        private readonly ConcurrentDictionary<uint, IStronghold> strongholds = new ConcurrentDictionary<uint, IStronghold>();

        private readonly IStrongholdFactory strongholdFactory;
        
        private readonly IStrongholdConfigurator strongholdConfigurator;
        
        private readonly LargeIdGenerator idGenerator = new LargeIdGenerator(10000, 5000);
        
        private readonly IRegionManager regionManager;
        
        private readonly Chat chat;

        private readonly IDbManager dbManager;

        public StrongholdManager(IStrongholdConfigurator strongholdConfigurator,
                                    IStrongholdFactory strongholdFactory,
                                    IRegionManager regionManager,
                                    Chat chat,
                                    IDbManager dbManager)
        {
            this.strongholdConfigurator = strongholdConfigurator;
            this.strongholdFactory = strongholdFactory;
            this.regionManager = regionManager;
            this.chat = chat;
            this.dbManager = dbManager;
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
            dbManager.Save(stronghold);
        }

        public void DbLoaderAdd(IStronghold stronghold)
        {
            strongholds.AddOrUpdate(stronghold.Id, stronghold, (id, old) => stronghold);
        }

        public bool TryGetStronghold(uint id, out IStronghold stronghold)
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
                using (dbManager.GetThreadTransaction())
                {
                    Add(stronghold);
                }
            }
        }

        public void Activate(IStronghold stronghold)
        {
            stronghold.StrongholdState = StrongholdState.Neutral;
            stronghold.BeginUpdate();
            regionManager.Add(stronghold);
            stronghold.EndUpdate();
            dbManager.Save(stronghold);
            chat.SendSystemChat(string.Format("{0} is now activated and can be captured!!", stronghold.Name));
        }

        public void TransferTo(IStronghold stronghold, ITribe tribe)
        {
            if (tribe == null)
                return;
            ITribe oldTribe = stronghold.Tribe;
            stronghold.BeginUpdate();
            stronghold.StrongholdState = StrongholdState.Occupied;
            stronghold.Tribe = tribe;
            stronghold.EndUpdate();
            dbManager.Save(stronghold);
            chat.SendSystemChat(oldTribe != null
                                        ? string.Format("{0} is taken over by {1} from the hands of {2}!!", stronghold.Name, tribe.Name, oldTribe.Name)
                                        : string.Format("{0} is now under the command of {1} ", stronghold.Name, tribe.Name));
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
