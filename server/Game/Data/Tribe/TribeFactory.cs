using System;
using Game.Data.Stronghold;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup.DependencyInjection;
using Persistance;

namespace Game.Data.Tribe
{
    public class TribeFactory : ITribeFactory
    {
        private readonly IKernel kernel;

        public TribeFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ITribe CreateTribe(IPlayer owner, string name, string desc, byte level, decimal victoryPoints, int attackPoints, int defensePoints, Resource resource, DateTime created)
        {
            return new Tribe(owner,
                             name,
                             desc,
                             level,
                             victoryPoints,
                             attackPoints,
                             defensePoints,
                             resource,
                             created,
                             kernel.Get<Procedure>(),
                             kernel.Get<IDbManager>(),
                             kernel.Get<Formula>(),
                             kernel.Get<IAssignmentFactory>(),
                             kernel.Get<ICityManager>(),
                             kernel.Get<IStrongholdManager>(),
                             kernel.Get<ITileLocator>());
        }

        public ITribe CreateTribe(IPlayer owner, string name)
        {
            return new Tribe(owner,
                             name,
                             kernel.Get<Procedure>(),
                             kernel.Get<IDbManager>(),
                             kernel.Get<Formula>(),
                             kernel.Get<IAssignmentFactory>(),
                             kernel.Get<ICityManager>(),
                             kernel.Get<IStrongholdManager>(),
                             kernel.Get<ITileLocator>());
        }
    }
}