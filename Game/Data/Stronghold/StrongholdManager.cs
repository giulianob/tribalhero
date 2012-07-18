using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Map;
using Game.Util;

namespace Game.Data.Stronghold
{
    class StrongholdManager : IStrongholdManager
    {
        private List<IStronghold> strongholds = new List<IStronghold>();

        private IStrongholdFactory strongholdFactory;
        private IStrongholdConfigurator strongholdConfigurator;
        private IdGenerator idGenerator;
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
            strongholds.Add(stronghold);
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
                strongholds.Add(strongholdFactory.CreateStronghold((uint)idGenerator.GetNext(), name, level, x, y));
            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<IStronghold> GetEnumerator()
        {
            return strongholds.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
