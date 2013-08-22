using System;
using System.Collections.Generic;
using System.ComponentModel;
using Game.Battle;
using Game.Data.Events;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Notifications;
using Persistance;

namespace Game.Data
{
    public interface ICity : IEnumerable<IStructure>,
                             ICanDo,
                             IPersistableObject,
                             IMiniMapRegionObject,
                             IStation,
                             INotificationOwner
    {
        #region Events

        event City.CityEventHandler<PropertyChangedEventArgs> PropertyChanged;

        event City.CityEventHandler<TroopStubEventArgs> TroopUnitUpdated;

        event City.CityEventHandler<TroopStubEventArgs> TroopUpdated;

        event City.CityEventHandler<TroopStubEventArgs> TroopRemoved;

        event City.CityEventHandler<TroopStubEventArgs> TroopAdded;

        event City.CityEventHandler<ActionWorkerEventArgs> ActionRemoved;
 
        event City.CityEventHandler<ActionWorkerEventArgs> ActionStarted;

        event City.CityEventHandler<ActionWorkerEventArgs> ActionRescheduled;

        event City.CityEventHandler<EventArgs> ResourcesUpdated;
 
        event City.CityEventHandler<EventArgs> UnitTemplateUpdated;

        event City.CityEventHandler<TechnologyEventArgs> TechnologyCleared;

        event City.CityEventHandler<TechnologyEventArgs> TechnologyAdded;

        event City.CityEventHandler<TechnologyEventArgs> TechnologyRemoved;

        event City.CityEventHandler<TechnologyEventArgs> TechnologyUpgraded;        

        event City.CityEventHandler<GameObjectArgs> ObjectAdded;

        event City.CityEventHandler<GameObjectArgs> ObjectRemoved;

        event City.CityEventHandler<GameObjectArgs> ObjectUpdated;

        event City.CityEventHandler<ActionReferenceArgs> ReferenceAdded;

        event City.CityEventHandler<ActionReferenceArgs> ReferenceRemoved;

        #endregion

        /// <summary>
        ///     Enumerates only through structures in this city
        /// </summary>
        Dictionary<uint, IStructure>.Enumerator Structures { get; }

        /// <summary>
        ///     Radius of city. This affects city wall and where user can build.
        /// </summary>
        byte Radius { get; set; }

        byte Lvl { get; }

        IReferenceManager References { get; }

        /// <summary>
        ///     City's battle manager. Maybe null if city is not in battle.
        /// </summary>
        IBattleManager Battle { get; set; }

        /// <summary>
        ///     Enumerates through all troop objects in this city
        /// </summary>
        IEnumerable<ITroopObject> TroopObjects { get; }

        /// <summary>
        ///     Technology manager for city
        /// </summary>
        ITechnologyManager Technologies { get; }

        /// <summary>
        ///     Returns the local troop
        /// </summary>
        ITroopStub DefaultTroop { get; set; }

        /// <summary>
        ///     Returns unit template. Unit template holds levels for all units in the city.
        /// </summary>
        IUnitTemplate Template { get; }

        /// <summary>
        ///     Resource available in the city
        /// </summary>
        ILazyResource Resource { get; }

        /// <summary>
        ///     Amount of loot this city has stolen from other players
        /// </summary>
        uint LootStolen { get; set; }

        /// <summary>
        ///     Unique city id
        /// </summary>
        uint Id { get; }

        /// <summary>
        ///     City name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Player that owns this city
        /// </summary>
        IPlayer Owner { get; }

        /// <summary>
        ///     Whether to send new units to hiding or not
        /// </summary>
        bool HideNewUnits { get; set; }

        /// <summary>
        ///     Attack points earned by this city
        /// </summary>
        int AttackPoint { get; set; }

        /// <summary>
        ///     Defense points earned by this city
        /// </summary>
        int DefensePoint { get; set; }

        ushort Value { get; set; }

        IStructure MainBuilding { get; }

        decimal AlignmentPoint { get; set; }

        City.DeletedState Deleted { get; set; }

        IActionWorker Worker { get; }

        /// <summary>
        ///     Enumerates through all structures and troops in this city
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        IGameObject this[uint objectId] { get; }

        /// <summary>
        /// Gets total number of laborers in the city (includes ones in structures).
        /// </summary>
        /// <returns></returns>
        int GetTotalLaborers();

        bool TryGetObject(uint objectId, out IGameObject obj);

        bool TryGetStructure(uint objectId, out IStructure structure);

        bool TryGetTroop(uint objectId, out ITroopObject troop);

        bool Add(uint objId, ITroopObject troop, bool save);

        bool Add(uint objId, IStructure structure, bool save);

        bool ScheduleRemove(ITroopObject obj, bool wasKilled);

        bool ScheduleRemove(IStructure obj, bool wasKilled, bool cancelReferences = false);

        /// <summary>
        ///     Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="structure"></param>
        void DoRemove(IStructure structure);

        /// <summary>
        ///     Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="troop"></param>
        void DoRemove(ITroopObject troop);

        bool IsUpdating { get; }

        void BeginUpdate();

        void EndUpdate();

        ITroopStub CreateTroopStub();

        ITroopObject CreateTroopObject(ITroopStub stub, uint x, uint y);

        IStructure CreateStructure(ushort type, byte level, uint x, uint y);
    }
}