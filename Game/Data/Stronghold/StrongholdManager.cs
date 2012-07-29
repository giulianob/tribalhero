using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Game.Data.Tribe;
using Game.Map;
using Game.Module;
using Game.Util;

namespace Game.Data.Stronghold
{
    class StrongholdManager : IStrongholdManager
    {
        private readonly ConcurrentDictionary<uint, IStronghold> strongholds = new ConcurrentDictionary<uint, IStronghold>();

        private readonly IStrongholdFactory strongholdFactory;
        private readonly IStrongholdConfigurator strongholdConfigurator;
        private readonly IdGenerator idGenerator;
        private readonly IWorld world;
        private Chat chat;

        public StrongholdManager(IdGenerator idGenerator,
                                    IStrongholdConfigurator strongholdConfigurator,
                                    IStrongholdFactory strongholdFactory,
                                    IWorld world,
                                    Chat chat)
        {
            this.idGenerator = idGenerator;
            this.strongholdConfigurator = strongholdConfigurator;
            this.strongholdFactory = strongholdFactory;
            this.world = world;
            this.chat = chat;
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
            chat.SendSystemChat(stronghold.Name + "is now activated and can be captured!!");
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
