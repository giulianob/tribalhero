package src.Objects.Effects {

	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Actions.StructureChangeAction;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.GameObject;
	import src.Objects.LazyResources;
	import src.Objects.LazyValue;
	import src.Objects.Prototypes.EffectPrototype;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.Resources;
	import src.Objects.StructureObject;
	import src.Objects.TechnologyManager;
	import src.Objects.Troop.TroopStub;

	public class Formula {
		
		public static const RESOURCE_CHUNK: int = 100;
		public static const RESOURCE_MAX_TRADE: int = 1500;
		public static const TRIBE_MEMBER_PER_LEVEL: int = 5;
		
		public static function sendCapacity(level: int) : int {
			var rate: Array = [0, 200, 200, 400, 400, 600, 600, 800, 1000, 1200, 1200, 1400, 1600, 1800, 1800, 2000];
			return rate[level];
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
			
            return (int)((100 - overtime*10) * count * 300 * Constants.secondsPerUnit / 100);
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
			for each (var structure: CityObject in city.objects.each()) {
				if (ObjectFactory.isType("University", structure.type)) {
					university = structure;
					break;
				}
			}			
			
			var buildTime: int = (baseValue * (100 - (university ? university.labor : 0) * 0.25) / 100);

			return buildTime * Constants.secondsPerUnit;
		}
		
		public static function moveTime(city: City, speed: int, distance: int, isAttacking: Boolean) : int {
			
			// MoveTimeMod
			var mod: Number = 0;
			for each (var tech: EffectPrototype in city.techManager.getEffects(EffectPrototype.EFFECT_TROOP_SPEED_MOD, EffectPrototype.INHERIT_ALL)) {
				if ( (tech.param2.toUpperCase() == "ATTACK" && isAttacking) || (tech.param2.toUpperCase() == "DEFENSE" && !isAttacking)) {
					mod += (int) (tech.param1);				
				} else if (tech.param2.toUpperCase() == "DISTANCE" && distance > (int)(tech.param3)) {
					mod += (int) (tech.param1);
				}
			}

			mod = 100 / Math.max(1, (mod + 100));

			// MoveTime
			var moveTime: int = 60 * (100 - ((speed - 12) * 5)) / 100;

			// Calculate total
			return Math.max(1, moveTime * mod) * distance * Constants.secondsPerUnit;
		}	

		public static function getTribeUpgradeCost(level: int) : Resources {
			return new Resources(5000, 5000, 200, 10000, 0).multiplyByUnit(TRIBE_MEMBER_PER_LEVEL * level);
		}
		
		public static function marketBuyCost(price: int, amount: int, tax: Number): int
		{
			return Math.round(((amount / RESOURCE_CHUNK) * price) * (1.0 + tax));
		}	

		public static function marketSellCost(price: int, amount: int, tax: Number): int
		{
			return Math.round(((amount / RESOURCE_CHUNK) * price) * (1.0 - tax));
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

		public static function marketTax(structure: StructureObject): Number
		{
			var rate: Array = [ 0.15, 0.15, 0.12, 0.09, 0.06, 0.03, 0, -0.03, -0.06, -0.09, -0.12 ];			
			return rate[structure.level];
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
			
			return (43200 / ( -6.845 * Math.log(laborTotal / 1.3 - 100) + 55)) * rateBonus;
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
	}
}

