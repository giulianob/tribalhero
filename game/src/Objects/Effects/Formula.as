package src.Objects.Effects {

	/**
	 * ...
	 * @author Default
	 */
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Objects.GameObject;
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

		public static function troopRadius(troop: TroopStub) : int {
			return Math.min(Math.ceil(troop.getUpkeep(true) / 100.0), 5);
		}

		public static function trainTime(parentObj: GameObject, baseValue: int, techManager: TechnologyManager): int
		{
			var discount: Array = [ 0, 0, 0, 0, 0, 5, 10, 15, 20, 30, 40 ];
			return (baseValue * Constants.secondsPerUnit) * (100 - discount[parentObj.level]) / 100;
		}

		public static function buildTime(parentObj: GameObject, baseValue: int, techManager:TechnologyManager): int
		{
			var discount: Array = [ 0, 0, 0, 0, 0, 5, 10, 15, 20, 30, 40 ];
			var mainBuildingLvl: int = 0;

			var city: City = Global.map.cities.get(parentObj.cityId);
			if (city) mainBuildingLvl = city.MainBuilding.level;

			var buildTime: int = (baseValue * (100 - discount[mainBuildingLvl]) / 100) - techManager.sum(EffectPrototype.EFFECT_BUILD_TIME_MULTIPLIER, EffectPrototype.INHERIT_SELF);

			return (buildTime * Constants.secondsPerUnit);
		}

		public static function moveTime(city: City, speed: int, distance: int) : int {
			var mod: int = 100;
			for each (var tech: EffectPrototype in city.techManager.getEffects(EffectPrototype.EFFECT_TROOP_SPEED_MOD, EffectPrototype.INHERIT_ALL)) {
				mod -= tech.param1;
			}
			mod = Math.max(50, mod);

			var moveTime: int = 3600 / (speed * 10);

			return Math.max(1, moveTime * Constants.secondsPerUnit * mod / 100) * distance;
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
			if (city.inBattle) return prototype.buildResources.multiplyByUnit(1.5);

			return prototype.buildResources;
		}

		public static function unitTrainCost(city: City, prototype: UnitPrototype) : Resources
		{
			if (city.inBattle) return prototype.trainResources.multiplyByUnit(1.5);

			return prototype.trainResources;
		}

		public static function unitUpgradeCost(city: City, prototype: UnitPrototype) : Resources
		{
			if (city.inBattle) return prototype.upgradeResources.multiplyByUnit(1.5);

			return prototype.upgradeResources;
		}

		public static function marketTax(structure: StructureObject): Number
		{
			switch (structure.level) {
				case 1:
					return 0.10;
				case 2:
					return 0.05;
				case 3:
					return 0;
				case 4:
					return -0.05;
				case 5:
					return -0.10;
				default:
					return 0;
			}
		}

		public static function maxForestLabor(level: int) : int {
			return level * 240;
		}

		public static function movementIconTroopSize(troopStub: TroopStub) : int {
			var upkeep: int = troopStub.getUpkeep();
			return Math.min(4, upkeep / 100);
		}
	}
}

