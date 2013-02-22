using System.Collections.Generic;
using Game.Battle;
using Game.Data.Troop;
using Game.Logic;
using Game.Logic.Notifications;
using Game.Util;
using Persistance;

namespace Game.Data
{
    public interface ICity : IEnumerable<IStructure>,
                             ICanDo,
                             IPersistableObject,
                             ICityRegionObject,
                             IStation,
                             INotificationOwner
    {
        /// <summary>
        ///     Enumerates only through structures in this city
        /// </summary>
        Dictionary<uint, IStructure>.Enumerator Structures { get; }

        /// <summary>
        ///     Radius of city. This affects city wall and where user can build.
        /// </summary>
        byte Radius { get; set; }

        byte Lvl { get; }

        NotificationManager Notifications { get; }

        ReferenceManager References { get; }

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

        decimal AlignmentPoint { get; set; }

        City.DeletedState Deleted { get; set; }

        IActionWorker Worker { get; }

        /// <summary>
        ///     Enumerates through all structures and troops in this city
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        IGameObject this[uint objectId] { get; }

        ITroopObject GetTroop(uint objectId);

        bool TryGetObject(uint objectId, out IGameObject obj);

        bool TryGetStructure(uint objectId, out IStructure structure);

        bool TryGetTroop(uint objectId, out ITroopObject troop);

        bool Add(uint objId, ITroopObject troop, bool save);

        bool Add(ITroopObject troop);

        bool Add(uint objId, IStructure structure, bool save);

        bool Add(uint objId, IStructure structure);

        bool Add(IStructure structure);

        bool ScheduleRemove(ITroopObject obj, bool wasKilled);

        bool ScheduleRemove(IStructure obj, bool wasKilled, bool cancelReferences = false);

        /// <summary>
        ///     Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="obj"></param>
        void DoRemove(IStructure obj);

        /// <summary>
        ///     Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name="obj"></param>
        void DoRemove(ITroopObject obj);

        List<IGameObject> GetInRange(uint x, uint y, uint inRadius);

        void BeginUpdate();

        void EndUpdate();

        void Subscribe(IChannel s);

        void Unsubscribe(IChannel s);
        
        void NewCityUpdate();

        void ObjUpdateEvent(IGameObject sender, uint origX, uint origY);
    }
}