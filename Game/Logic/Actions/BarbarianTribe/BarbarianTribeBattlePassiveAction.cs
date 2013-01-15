#region

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Battle;
using Game.Battle.CombatGroups;
using Game.Battle.CombatObjects;
using Game.Battle.Reporting;
using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Troop;
using Game.Logic.Formulas;
using Game.Logic.Procedures;
using Game.Map;
using Game.Setup;
using Game.Util;
using Game.Util.Locking;
using Persistance;

#endregion

namespace Game.Logic.Actions
{
    public class BarbarianTribeBattlePassiveAction : ScheduledPassiveAction
    {
        private readonly BattleProcedure battleProcedure;

        private readonly uint barbarianTribeId;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly BarbarianTribeBattleProcedure barbarianTribeBattleProcedure;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly IWorld world;
        
        public BarbarianTribeBattlePassiveAction(uint barbarianTribeId,
                                       BattleProcedure battleProcedure,
                                       ILocker locker,
                                       IGameObjectLocator gameObjectLocator,
                                       IDbManager dbManager,
                                       Formula formula,
                                       BarbarianTribeBattleProcedure barbarianTribeBattleProcedure,
                                       IWorld world)
        {
            this.barbarianTribeId = barbarianTribeId;
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.barbarianTribeBattleProcedure = barbarianTribeBattleProcedure;
            this.world = world;

            IBarbarianTribe barbarianTribe;
            if (!gameObjectLocator.TryGetObjects(barbarianTribeId, out barbarianTribe))
            {
                throw new Exception("Did not find barb tribe that was supposed to be having a battle");
            }
        }

        public BarbarianTribeBattlePassiveAction(uint barbarianTribeId,
                                       DateTime beginTime,
                                       DateTime nextTime,
                                       DateTime endTime,
                                       bool isVisible,
                                       string nlsDescription,
                                       IDictionary<string, string> properties,
                                       IActionFactory actionFactory,
                                       BattleProcedure battleProcedure,
                                       ILocker locker,
                                       IGameObjectLocator gameObjectLocator,
                                       IDbManager dbManager,
                                       Formula formula,
                                       BarbarianTribeBattleProcedure barbarianTribeBattleProcedure,
                                       IWorld world)
                : base(barbarianTribeId, beginTime, nextTime, endTime, isVisible, nlsDescription)
        {
            this.battleProcedure = battleProcedure;
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.barbarianTribeBattleProcedure = barbarianTribeBattleProcedure;
            this.world = world;

            barbarianTribeId = uint.Parse(properties["barbarian_tribe_id"]);

            IBarbarianTribe barbarianTribe;
            if (!gameObjectLocator.TryGetObjects(barbarianTribeId, out barbarianTribe))
            {
                throw new Exception();
            }
        }

        public override ActionType Type
        {
            get
            {
                return ActionType.BarbarianTribeBattlePassive;
            }
        }

        public override string Properties
        {
            get
            {
                return XmlSerializer.Serialize(new[] {new XmlKvPair("barbarian_tribe_id", barbarianTribeId)});
            }
        }
        
        public override void Callback(object custom)
        {
            IBarbarianTribe barbarianTribe;
            if (!gameObjectLocator.TryGetObjects(barbarianTribeId, out barbarianTribe))
            {
                throw new Exception("Barb tribe is missing");
            }

            CallbackLock.CallbackLockHandler lockHandler = delegate
                {
                    var toBeLocked = new List<ILockable>();
                    toBeLocked.AddRange(barbarianTribe.Battle.LockList);
                    toBeLocked.Add(barbarianTribe);                    
                    return toBeLocked.ToArray();
                };

            using (locker.Lock(lockHandler, null, barbarianTribe))
            {
                if (barbarianTribe.Battle.ExecuteTurn())
                {
                    // Battle continues, just save it and reschedule
                    dbManager.Save(barbarianTribe.Battle);
                    endTime =
                            SystemClock.Now.AddSeconds(
                                                       formula.GetBattleInterval(barbarianTribe.Battle.Defenders.Count +
                                                                                 barbarianTribe.Battle.Attackers.Count));
                    StateChange(ActionState.Fired);
                    return;
                }

                // Battle has ended
                // Delete the battle
                world.Remove(barbarianTribe.Battle);
                dbManager.Delete(barbarianTribe.Battle);                
                barbarianTribe.BeginUpdate();
                barbarianTribe.Battle = null;
                // TODO: Add camps
                // barbarianTribe.Camps--;
                barbarianTribe.EndUpdate();

                StateChange(ActionState.Completed);
            }
        }

        public override Error Validate(string[] parms)
        {
            return Error.Ok;
        }

        public override Error Execute()
        {
            IBarbarianTribe barbarianTribe;
            if (!gameObjectLocator.TryGetObjects(barbarianTribeId, out barbarianTribe))
            {
                return Error.ObjectNotFound;
            }

            world.Add(barbarianTribe.Battle);
            dbManager.Save(barbarianTribe.Battle);

            //Add local troop
            // TODO: Generate 
            barbarianTribe.AddLocalUnitsToBattle(barbarianTribe.Battle, barbarianTribe);
            
            beginTime = SystemClock.Now;
            endTime = SystemClock.Now;

            return Error.Ok;
        }
        
        public override void UserCancelled()
        {
        }

        public override void WorkerRemoved(bool wasKilled)
        {
            throw new Exception("Barbarian tribe removed during battle?");
        }
    }
}