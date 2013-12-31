package src.Objects.Effects {

    import System.Linq.Enumerable;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Actions.BuildAction;
    import src.Objects.Actions.CurrentAction;
    import src.Objects.Actions.CurrentActiveAction;
    import src.Objects.Actions.StructureUpgradeAction;
    import src.Objects.Factories.*;
    import src.Objects.Prototypes.*;
    import src.Objects.Troop.*;
    import src.Util.StringHelper;
    import src.Util.Util;

    public class Formula {
		
		public static const RESOURCE_CHUNK: int = 100;		
		public static const TRIBE_MEMBER_PER_LEVEL: int = 5;
		
		public static function resourcesBuyable(marketLevel: int): Array {
			if (marketLevel >= 8)
				return ["Wood", "Iron", "Crop"];
			if (marketLevel >= 3)
				return ["Wood", "Crop"];
			return [];
		}
		
		public static function resourcesSellable(marketLevel: int): Array {
			if (marketLevel >= 8)
				return ["Wood", "Iron", "Crop"];
			return ["Wood", "Crop"];
		}		
		
		public static function resourceMaxTrade(marketLevel: int): int {
			var rate: Array = [0, 200, 400, 400, 400, 700, 700, 1100, 1100, 1800, 1800];
			return rate[marketLevel];
		}

		public static function concurrentBuildUpgrades(mainStructureLevel: int) : int
		{
			return mainStructureLevel >= 11 ? 3 : 2;
		}
		
		public static function sendCapacity(level: int) : int {
			var rate: Array = [0, 200, 200, 400, 400, 600, 600, 800, 1000, 1200, 1200, 1400, 1600, 1800, 1800, 2000];
			return rate[level];
		}
		
		public static function contributeCapacity(level: int) : int {
			return sendCapacity(level) * 2;
		}
		
		public static function troopRadius(troop: TroopStub) : int {
			var city: City = Global.map.cities.get(troop.cityId);
			return Math.min(4, city.value / 40);
		}
			
		public static function laborMoveTime(parentObj: GameObject, count: int, cityToStructure:Boolean, city: City, techManager: TechnologyManager): int
		{			
			const secondsPerLaborer: int = 180;
			
            var totalLaborers: int = city.getBusyLaborCount() + city.resources.labor.getValue();
			var moveTime: int;
			// Assign faster during beginning of the game
            if (cityToStructure && totalLaborers < 160)
            {
                moveTime = Math.ceil(0.95 * Math.exp(0.033 * totalLaborers)) * count;
            }
			else {
				var overtime: int = 0;
				
				for each (var tech: EffectPrototype in techManager.getEffects(EffectPrototype.EFFECT_LABOR_MOVE_TIME_MOD, EffectPrototype.INHERIT_ALL)) {
					overtime = Math.max(overtime, (int)(tech.param1));
				}
				
				overtime = Math.min(100, overtime);
				
				moveTime = (int)((100 - overtime * 10) * count * secondsPerLaborer * Constants.secondsPerUnit / 100);
			}
			
			if (!cityToStructure) {
				moveTime = moveTime / 20;
			}
			
			return moveTime;
		}
		
		public static function trainTime(structureLvl: int, unitCount: int, unitPrototype: UnitPrototype, city: City, techManager: TechnologyManager, ignoreUnitCountDiscounts: Boolean): int
		{			
			var structureDiscountByLevel: Array = [0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 10, 15, 15, 20, 30, 40];
			
            var currentCityUpkeep: Number = city.troops.getUpkeep();
            var structureLevelDiscount: Number = (100.0 - structureDiscountByLevel[Math.min(structureLvl, structureDiscountByLevel.length - 1)]) / 100.0;
            var trainTimePerUnit: Number = unitPrototype.trainTime * structureLevelDiscount;

            if (currentCityUpkeep < 15)
            {						
				var trainFirst15Discount: Number = (100.0 - Enumerable.from(techManager.getEffects(EffectPrototype.EFFECT_UNIT_TRAIN_FIRST_15_REDUCTION, EffectPrototype.INHERIT_ALL))
															  .sum(function(p: EffectPrototype): int {
																	return int(p.param1);
															  })) / 100.0;

                var discountedUnits: int = Math.min(15 - currentCityUpkeep, unitCount);
                return (int)(((trainTimePerUnit * trainFirst15Discount * discountedUnits) + (trainTimePerUnit * (unitCount - discountedUnits))) * Constants.secondsPerUnit);
            }
            
            return (int)(trainTimePerUnit * unitCount) * Constants.secondsPerUnit;
		}
		
		public static function buildTime(parentObjOrCity: *, baseValue: int, techManager:TechnologyManager): int
		{	
			var city: City;
			
			if (parentObjOrCity is City)
				city = parentObjOrCity;
			else
				city = Global.map.cities.get(parentObjOrCity.cityId);
			
			if (!city) return 0;

			var university: CityObject;
			for each (var structure: CityObject in city.objects) {
				if (ObjectFactory.isType("University", structure.type)) {
					university = structure;
					break;
				}
			}			
			
			var buildTime: int = (baseValue * (100 - (university ? university.labor : 0) * 0.25) / 100);

			return buildTime * Constants.secondsPerUnit;
		}

		public static function moveTimeTotal(city: City, speed: Number, distance: int, isAttacking: Boolean) : int {
			var moveTime: Number = moveTimePerTile(speed);
			var doubleTimeBonus: Number = 0;
			var rushBonus: Number = 0;
			var doubleTimeDistance: int = 500;

			for each (var tech: EffectPrototype in city.techManager.getEffects(EffectPrototype.EFFECT_TROOP_SPEED_MOD, EffectPrototype.INHERIT_ALL)) {
				if ( (tech.param2.toUpperCase() == "ATTACK" && isAttacking) || (tech.param2.toUpperCase() == "DEFENSE" && !isAttacking)) {
					rushBonus += (int) (tech.param1);				
				} else if (tech.param2.toUpperCase() == "DISTANCE") {
					doubleTimeBonus += (int) (tech.param1);
				}
			}

            var rushBonusPercentage: Number = rushBonus / 100;
            var doubleTimeBonusPercentage: Number = doubleTimeBonus / 100;
			
			if (distance <= doubleTimeDistance) 
			{
				return moveTime * distance / (1 + rushBonusPercentage) * Constants.secondsPerUnit;
			}
			
            var shortDistance: Number = moveTime * doubleTimeDistance / (1 + rushBonusPercentage);
            var longDistance: Number = (distance - doubleTimeDistance) * moveTime / (1 + rushBonusPercentage + doubleTimeBonusPercentage);
            return (shortDistance + longDistance) * Constants.secondsPerUnit;
		}
		
		public static function moveTimePerTile(speed: Number): Number {
			return Util.roundNumber(3600.0 / (45 + speed * 5),1);
		}
		
		public static function moveTimeStringSimple(speed: Number) : String {
			if (speed <= 5) {
				return StringHelper.localize("TROOP_SPEED_VERY_SLOW");
			} else if (speed <= 10) {
				return StringHelper.localize("TROOP_SPEED_SLOW");
			} else if (speed <= 15) {
				return StringHelper.localize("TROOP_SPEED_NORMAL");
			} else if (speed <= 20) {
				return StringHelper.localize("TROOP_SPEED_FAST");
			} else {
				return StringHelper.localize("TROOP_SPEED_VERY_FAST");
			}			
		}
		public static function moveTimeStringFull(speed: Number): String {
			var secondPerTile: Number = moveTimePerTile(speed);
			if (Constants.debug) {
				return speed.toFixed(1) + " speed (" + secondPerTile.toFixed(1) + " seconds per tile)";
			}
			if (speed <= 5) {
				return StringHelper.localize("TROOP_SPEED_VERY_SLOW_FULL", secondPerTile.toFixed(1));
			} else if (speed <= 10) {
				return StringHelper.localize("TROOP_SPEED_SLOW_FULL", secondPerTile.toFixed(1));
			} else if (speed <= 15) {
				return StringHelper.localize("TROOP_SPEED_NORMAL_FULL", secondPerTile.toFixed(1));
			} else if (speed <= 20) {
				return StringHelper.localize("TROOP_SPEED_FAST_FULL", secondPerTile.toFixed(1));
			} else {
				return StringHelper.localize("TROOP_SPEED_VERY_FAST_FULL", secondPerTile.toFixed(1));
			}
		}

		public static function getTribeUpgradeCost(level: int) : Resources {
			return new Resources(1000, 400, 40, 2000, 0).multiplyByUnit(TRIBE_MEMBER_PER_LEVEL * level * (1 + ( level * level / 20)));
		}
		
		public static function marketBuyCost(price: int, amount: int): int
		{
			return Math.round(((amount / RESOURCE_CHUNK) * price));
		}	

		public static function marketSellCost(price: int, amount: int): int
		{
			return Math.round(((amount / RESOURCE_CHUNK) * price));
		}

		public static function buildCost(city: City, prototype: StructurePrototype) : Resources
		{
			return prototype.buildResources;
		}

		public static function unitTrainCost(city: City, prototype: UnitPrototype) : Resources
		{
			return prototype.trainResources;
		}

		public static function unitUpgradeCost(city: City, prototype: UnitPrototype) : Resources
		{
			return prototype.upgradeResources;
		}

		public static function maxLumbermillLabor(level: int) : int {
            var maxLabor : Array = [ 0, 40, 40, 80, 160, 160, 160, 240, 240, 360, 360, 360, 480, 480, 480, 640 ];
            return maxLabor[level];

		}

        public static function maxLaborPerForestCamp(level: int) : int {
            var maxLabor : Array = [ 0, 40, 40, 40, 80, 80, 80, 80, 80, 120, 120, 120, 120, 120, 240, 320 ];
            return maxLabor[level];
        }

        public static function getUpkeepWithReductions(baseUpkeep: int, unitType: int, city: City): Number {
            var reduceUpkeep: int = 0;
			for each (var tech: EffectPrototype in city.techManager.getEffects(EffectPrototype.EFFECT_REDUCE_UPKEEP, EffectPrototype.INHERIT_ALL)) {
				if (tech.param2.indexOf(unitType.toString()) != -1) {
					reduceUpkeep += (int) (tech.param1);				
				} 
			}
            
            return baseUpkeep * (100.0 - Math.min(reduceUpkeep, 30)) / 100.0;
        }

		public static function laborRate(city: City) : int {
			var laborTotal: int = city.getBusyLaborCount() + city.resources.labor.getValue();
			if (laborTotal < 140) laborTotal = 140;
			
			var effects: Array = city.techManager.getEffects(EffectPrototype.EFFECT_LABOR_TRAIN_TIME_MOD, EffectPrototype.INHERIT_SELF_ALL);
			var rateBonus: Number = 1;
			for each (var effect: EffectPrototype in effects) {
				rateBonus = Math.min(rateBonus, (int)(effect.param1) / 10);
			}
            
			if (effects.length > 1)
				rateBonus *= Math.pow(0.92, effects.length - 1); // for every extra tribal gathering, you gain 10 % each
			
			var newMultiplier:Number = Math.min(2, 3 - laborTotal / 400);
            newMultiplier = Math.max(1,newMultiplier);
			return (43200 / (( -6.845 * Math.log(laborTotal / 1.3 - 100) + 55) * newMultiplier)) * rateBonus * Constants.secondsPerUnit;
		}
		
		// Returns the new resource rate and accounts for any resource bonus
		public static function getResourceRateWithBonus(resource: LazyValue, buildingLevel: int, resourceType: int, laborDelta: int) : int
		{
			var bonus: Array;
			
			switch (resourceType) 
			{
				case Resources.TYPE_CROP:
					bonus = [1, 1, 1, 1, 1, 1, 1, 1.1, 1.1, 1.2, 1.2, 1.3, 1.3, 1.4, 1.4, 1.5];
					break;
				default:
					bonus = [1, 1, 1, 1, 1, 1, 1, 1.1, 1.1, 1, 1, 1, 1, 1, 1, 1];
			}
			
			// The rate includes the bonus so dividing it by the bonus and taking the ceil gives
			// the actual number of laborers then we add the new number and remultiple by bonus to get the true value
			return (Math.ceil(resource.getRate() / Number(bonus[buildingLevel])) + (laborDelta)) * bonus[buildingLevel];
		}
		
		public static function getResourceNewCity() : *
		{
			var numberOfCities:Number = Global.map.cities.size();
			var wagonRequired:Number = 50 * numberOfCities;
			var wagonCurrent:Number = Global.gameContainer.selectedCity.troops.getDefaultTroop().getIndividualUnitCount(ObjectFactory.getFirstType("Wagon"));
			var influenceRequired:Number = (60 + 40 * numberOfCities) * numberOfCities;
			var influenceCurrent:Number = 0;
			for each(var city: City in Global.map.cities)
			{
				influenceCurrent += city.value;
			}
			return { wagonRequired:wagonRequired, wagonCurrent:wagonCurrent, influenceRequired:influenceRequired, influenceCurrent:influenceCurrent };
		}

        public static function getGateRepairCost(gateMax: int, currentHp: Number) : Resources
        {
			var hp: int = gateMax - currentHp;
            return new Resources(0, hp / 8, hp / 16, hp / 4, 0);
        }

        public static function cityMaxConcurrentBuildActions(structureType: int, city: City): Boolean
        {
            if (ObjectFactory.isType("UnlimitedBuilding", structureType)) {
                return true;
            }

            var maxConcurrentUpgrades: int = concurrentBuildUpgrades(city.MainBuilding.level);

            var currentBuildActions: int = Enumerable.from(city.currentActions.getActions())
                    .ofType(CurrentActiveAction)
                    .count(function (currentActiveAction: CurrentActiveAction): Boolean {
                        if (currentActiveAction.getAction() is BuildAction) {
                            var buildAction: BuildAction = currentActiveAction.getAction();
                            if (!ObjectFactory.isType("UnlimitedBuilding", buildAction.type)) {
                                return true;
                            }
                        }

                        return currentActiveAction.getAction() is StructureUpgradeAction;
                    });

            return currentBuildActions < maxConcurrentUpgrades;
        }
    }
}

