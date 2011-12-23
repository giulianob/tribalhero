using System.Collections.Generic;
using Game.Battle;
using Game.Data.Troop;
using Game.Logic;
using Game.Map;
using Game.Util;
using Game.Util.Locking;
using Persistance;

namespace Game.Data
{
    public interface ICity : IEnumerable<Structure>, ICanDo, ILockable, IPersistableObject, ICityRegionObject
    {
        /// <summary>
        ///   Enumerates only through structures in this city
        /// </summary>
        Dictionary<uint, Structure>.Enumerator Structures { get; }

        /// <summary>
        ///   Radius of city. This affects city wall and where user can build.
        /// </summary>
        byte Radius { get; set; }

        byte Lvl { get; }

        /// <summary>
        ///   Returns the city's center point which is the town centers position
        /// </summary>
        uint X { get; }

        /// <summary>
        ///   Returns the city's center point which is the town centers position
        /// </summary>
        uint Y { get; }

        /// <summary>
        ///   City's battle manager. Maybe null if city is not in battle.
        /// </summary>
        IBattleManager Battle { get; set; }

        /// <summary>
        ///   Enumerates through all troop objects in this city
        /// </summary>
        IEnumerable<TroopObject> TroopObjects { get; }

        /// <summary>
        ///   Troop manager which manages all troop stubs in city
        /// </summary>
        TroopManager Troops { get; }

        /// <summary>
        ///   Technology manager for city
        /// </summary>
        TechnologyManager Technologies { get; }

        /// <summary>
        ///   Returns the local troop
        /// </summary>
        TroopStub DefaultTroop { get; set; }

        /// <summary>
        ///   Returns unit template. Unit template holds levels for all units in the city.
        /// </summary>
        UnitTemplate Template { get; }

        /// <summary>
        ///   Resource available in the city
        /// </summary>
        LazyResource Resource { get; }

        /// <summary>
        ///   Amount of loot this city has stolen from other players
        /// </summary>
        uint LootStolen { get; set; }

        /// <summary>
        ///   Unique city id
        /// </summary>
        uint Id { get; set; }

        /// <summary>
        ///   City name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///   Player that owns this city
        /// </summary>
        Player Owner { get; }

        /// <summary>
        ///   Whether to send new units to hiding or not
        /// </summary>
        bool HideNewUnits { get; set; }

        /// <summary>
        ///   Attack points earned by this city
        /// </summary>
        int AttackPoint { get; set; }

        /// <summary>
        ///   Defense points earned by this city
        /// </summary>
        int DefensePoint { get; set; }

        ushort Value { get; set; }

        bool IsUpdating { get; }

        City.DeletedState Deleted { get; set; }

        ActionWorker Worker { get; }

        /// <summary>
        ///   Enumerates through all structures and troops in this city
        /// </summary>
        /// <param name = "objectId"></param>
        /// <returns></returns>
        GameObject this[uint objectId] { get; }

        TroopObject GetTroop(uint objectId);

        bool TryGetObject(uint objectId, out GameObject obj);

        bool TryGetStructure(uint objectId, out Structure structure);

        bool TryGetTroop(uint objectId, out TroopObject troop);

        bool Add(uint objId, TroopObject troop, bool save);

        bool Add(TroopObject troop);

        bool Add(uint objId, Structure structure, bool save);

        bool Add(uint objId, Structure structure);

        bool Add(Structure structure);

        bool ScheduleRemove(TroopObject obj, bool wasKilled);

        bool ScheduleRemove(Structure obj, bool wasKilled);

        bool ScheduleRemove(Structure obj, bool wasKilled, bool cancelReferences);

        /// <summary>
        ///   Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name = "obj"></param>
        void DoRemove(Structure obj);

        /// <summary>
        ///   Removes the object from the city. This function should NOT be called directly. Use ScheduleRemove instead!
        /// </summary>
        /// <param name = "obj"></param>
        void DoRemove(TroopObject obj);

        List<GameObject> GetInRange(uint x, uint y, uint inRadius);

        void BeginUpdate();

        void EndUpdate();

        void Subscribe(IChannel s);

        void Unsubscribe(IChannel s);

        void ResourceUpdateEvent();

        void RadiusUpdateEvent();

        void DefenseAttackPointUpdate();

        void HideNewUnitsUpdate();

        void NewCityUpdate();

        void ObjAddEvent(GameObject obj);

        void ObjRemoveEvent(GameObject obj);

        void ObjUpdateEvent(GameObject sender, uint origX, uint origY);

        void UnitTemplateUnitUpdated(UnitTemplate sender);

        void BattleStarted();

        void BattleEnded();
    }
}