#region

using System;
using System.Collections.Generic;
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
        private uint barbarianTribeId;

        private readonly IDbManager dbManager;

        private readonly Formula formula;

        private readonly BarbarianTribeBattleProcedure barbarianTribeBattleProcedure;

        private readonly IGameObjectLocator gameObjectLocator;

        private readonly ILocker locker;

        private readonly IWorld world;

        private readonly SimpleStubGenerator simpleStubGenerator;

        private uint localGroupId;

        public BarbarianTribeBattlePassiveAction(ILocker locker,
                                                 IGameObjectLocator gameObjectLocator,
                                                 IDbManager dbManager,
                                                 Formula formula,
                                                 BarbarianTribeBattleProcedure barbarianTribeBattleProcedure,
                                                 IWorld world,
                                                 ISimpleStubGeneratorFactory simpleStubGeneratorFactory)
        {
            this.locker = locker;
            this.gameObjectLocator = gameObjectLocator;
            this.dbManager = dbManager;
            this.formula = formula;
            this.barbarianTribeBattleProcedure = barbarianTribeBattleProcedure;
            this.world = world;

            simpleStubGenerator = simpleStubGeneratorFactory.CreateSimpleStubGenerator(formula.BarbarianTribeUnitRatios(), formula.BarbarianTribeUnitTypes());
        }

        public BarbarianTribeBattlePassiveAction(uint barbarianTribeId,
                                                 ILocker locker,
                                                 IGameObjectLocator gameObjectLocator,
                                                 IDbManager dbManager,
                                                 Formula formula,
                                                 BarbarianTribeBattleProcedure barbarianTribeBattleProcedure,
                                                 IWorld world,
                                                 ISimpleStubGeneratorFactory simpleStubGeneratorFactory) 
            : this(locker, gameObjectLocator, dbManager, formula, barbarianTribeBattleProcedure, world, simpleStubGeneratorFactory)
        {
            this.barbarianTribeId = barbarianTribeId;
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

            simpleStubGenerator = simpleStubGeneratorFactory.CreateSimpleStubGenerator(formula.BarbarianTribeUnitRatios(), formula.BarbarianTribeUnitTypes());
        }

        public override void LoadProperties(IDictionary<string, string> properties)
        {
            barbarianTribeId = uint.Parse(properties["barbarian_tribe_id"]);
            localGroupId = uint.Parse(properties["local_group_id"]);
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
                return XmlSerializer.Serialize(new[]
                {
                        new XmlKvPair("barbarian_tribe_id", barbarianTribeId),
                        new XmlKvPair("local_group_id", localGroupId)
                });
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

            locker.Lock(lockHandler, null, barbarianTribe).Do(() =>
            {
                if (barbarianTribe.Battle.ExecuteTurn())
                {
                    // Battle continues, just save it and reschedule
                    dbManager.Save(barbarianTribe.Battle);
                    endTime = SystemClock.Now.AddSeconds(formula.GetBattleInterval(barbarianTribe.Battle.Defenders, barbarianTribe.Battle.Attackers));
                    StateChange(ActionState.Fired);
                    return;
                }

                // Battle has ended
                // Delete the battle
                world.Remove(barbarianTribe.Battle);
                dbManager.Delete(barbarianTribe.Battle);

                barbarianTribe.BeginUpdate();
                barbarianTribe.Battle = null;
                barbarianTribe.State = GameObjectStateFactory.NormalState();

                var initialBarbResources = formula.BarbarianTribeResources(barbarianTribe);
                if (!initialBarbResources.Equals(barbarianTribe.Resource))
                {
                    // Lower camps remaining
                    barbarianTribe.CampRemains--;

                    // Reset resources
                    barbarianTribe.Resource.Clear();
                    barbarianTribe.Resource.Add(formula.BarbarianTribeResources(barbarianTribe));
                }

                barbarianTribe.EndUpdate();

                StateChange(ActionState.Completed);
            });
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
            ISimpleStub simpleStub;
            int upkeep;
            byte unitLevel;
            formula.BarbarianTribeUpkeep(barbarianTribe.Lvl, out upkeep, out unitLevel);
            simpleStubGenerator.Generate(barbarianTribe.Lvl, upkeep, unitLevel, Config.barbarian_tribes_npc_randomness, (int)barbarianTribe.ObjectId + barbarianTribe.CampRemains, out simpleStub);

            var combatGroup = barbarianTribeBattleProcedure.AddBarbarianTribeUnitsToBattle(barbarianTribe.Battle,
                                                                                           barbarianTribe,
                                                                                           simpleStub.ToUnitList(FormationType.Normal));
            localGroupId = combatGroup.Id;
            
            beginTime = SystemClock.Now;
            endTime = SystemClock.Now.Add(formula.GetBattleDelayStartInterval());

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