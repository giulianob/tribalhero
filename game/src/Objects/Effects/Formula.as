package src.Objects.Effects {

	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Factories.*;
	import src.Objects.Prototypes.*;
	import src.Objects.Troop.*;

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
		
		private static function timeDiscount(level: int) : int {
			var discount: Array = [0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 10, 15, 15, 20, 30, 40];
			return discount[level];
		}
		
		public static function laborMoveTime(parentObj: GameObject, count: int, techManager: TechnologyManager): int
		{
			var overtime: int = 0;
			for each (var tech: EffectPrototype in techManager.getEffects(EffectPrototype.EFFECT_LABOR_MOVE_TIME_MOD, EffectPrototype.INHERIT_ALL)) {
				overtime = Math.max(overtime, (int)(tech.param1));
			}
			overtime = Math.min(100, overtime);
			
            return (int)((100 - overtime*10) * count * 180 * Constants.secondsPerUnit / 100);
		}
		
		public static function trainTime(parentObj: StructureObject, baseValue: int, techManager: TechnologyManager): int
		{			
			return (baseValue * Constants.secondsPerUnit) * (100 - timeDiscount(parentObj.level)) / 100;
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

		public static function doubleTimeTotal(moveTime : int, distance :int, bonusPercentage: Number, forEveryDistance: int ):int {
			var total: int = 0;
            var index: int = 0;
            var tiles: int = 0;
            while (distance > 0) {
                tiles = distance > forEveryDistance ? forEveryDistance : distance;
                total += (int)(moveTime * tiles / (1 + bonusPercentage * index++));
                distance -= tiles;
            }
            return total;
		}
		
		public static function moveTimeTotal(city: City, speed: int, distance: int, isAttacking: Boolean) : int {
			var moveTime: int = 60 * (100 - ((speed - 12) * 5)) / 100;
			var bonus: Number = 0;
			var rushMod: Number = 0;

			for each (var tech: EffectPrototype in city.techManager.getEffects(EffectPrototype.EFFECT_TROOP_SPEED_MOD, EffectPrototype.INHERIT_ALL)) {
				if ( (tech.param2.toUpperCase() == "ATTACK" && isAttacking) || (tech.param2.toUpperCase() == "DEFENSE" && !isAttacking)) {
					rushMod += (int) (tech.param1);				
				} else if (tech.param2.toUpperCase() == "DISTANCE") {
					bonus = (int) (tech.param1) > bonus? (int) (tech.param1): bonus;
				}
			}

            var bonusPercentage: Number = bonus / 100;
            rushMod = 100 / (Math.max(1, rushMod + 100));
			return (int)(doubleTimeTotal(moveTime, distance, bonusPercentage, 500) * rushMod) * Constants.secondsPerUnit;
		}
		
		public static function moveTimeString(speed: int): String{
			if (speed <= 5) {
				return "Very slow";
			} else if (speed <= 10) {
				return "Slow";
			} else if (speed <= 15) {
				return "Normal";
			} else if (speed <= 20) {
				return "Fast";
			} else {
				return "Very Fast";
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

		public static function maxForestLabor(level: int) : int {
			return level * 240;
		}

		public static function maxForestLaborPerUser(level: int) : int {
			return Formula.maxForestLabor(level) / 6;
		}

		public static function movementIconTroopSize(troopStub: TroopStub) : int {
			var upkeep: int = troopStub.getUpkeep();
			return Math.min(4, upkeep / 60);
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
			var size:Number = Global.map.cities.size();
			var wagonRequired:Number = 50 * size;
			var wagonCurrent:Number = Global.gameContainer.selectedCity.troops.getDefaultTroop().getIndividualUnitCount(ObjectFactory.getFirstType("Wagon"));
			var influenceRequired:Number = size * (100 + 20 * (size-1));
			var influenceCurrent:Number = 0;
			for each(var city: City in Global.map.cities)
			{
				influenceCurrent += city.value;
			}
			return { wagonRequired:wagonRequired, wagonCurrent:wagonCurrent, influenceRequired:influenceRequired, influenceCurrent:influenceCurrent };
		}
		 
		public static function getGateLimit(level: int) : int
        {
            var limit:Array = [0, 10000, 13500, 17300, 21500, 26200, 31300, 37100, 43400, 50300, 58000,
                            66500, 75800, 86300, 97800, 110500, 124600, 140100, 157200, 176200, 200000];
            return limit[level];
        }

        public static function getGateRepairCost(level: int, currentHp: Number) : Resources
        {
			var hp: int = getGateLimit(level) - currentHp;
            return new Resources(0, level * hp / 8, level * hp / 16, level * hp / 4, 0);
        }
	}
}

