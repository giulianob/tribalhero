using System;
using System.Collections.Generic;
using Game.Logic.Actions.ResourceActions;
using Persistance;

namespace Game.Data.Forest
{
    public interface IForest : ISimpleGameObject, IPersistableObject, IEnumerable<IStructure>, IMiniMapRegionObject
    {
        /// <summary>
        ///     The action which will deplete this forest
        /// </summary>
        ForestDepleteAction DepleteAction { get; set; }

        /// <summary>
        ///     The lumber availabel at this forest.
        ///     Notice: The rate is not used, only the upkeep. The rate of the forest is kept in a separate variable.
        /// </summary>
        AggressiveLazyValue Wood { get; set; }

        /// <summary>
        /// Time until forest is depleted. Only the db loader should be setting this.
        /// </summary>
        DateTime DepleteTime { get; set; }

        void AddLumberjack(IStructure structure);

        /// <summary>
        ///     Removes structure from forest
        /// </summary>
        /// <param name="structure">Structure to remove</param>
        void RemoveLumberjack(IStructure structure);

        /// <summary>
        ///     Recalculates the rate of the forest and all lumberjack structures around it.
        ///     Must lock all players using the forest and the forest manager.
        /// </summary>
        void RecalculateForest();
    }
}