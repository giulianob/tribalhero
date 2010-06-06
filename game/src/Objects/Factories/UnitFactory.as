package src.Objects.Factories {
	import flash.display.DisplayObjectContainer;
	import src.Map.Map;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Util.BinaryList.*;
	import src.Objects.Resources;

	import flash.utils.getDefinitionByName;

	public class UnitFactory {

		private static var map: Map;
		private static var unitPrototypes: BinaryList;

		public static function init(_map: Map, data: XML):void
		{
			map = _map;

			unitPrototypes = new BinaryList(UnitPrototype.sortOnTypeAndLevel, UnitPrototype.compareTypeAndLevel);

			for each (var unitNode: XML in data.Units.*)
			{
				var unitObj: UnitPrototype = new UnitPrototype();
				unitObj.nlsname = unitNode.@name;
				unitObj.type = unitNode.@type;
				unitObj.level = unitNode.@level;
				unitObj.baseClass = unitNode.@baseclass;
				unitObj.spriteClass = unitNode.@spriteclass;
				unitObj.hp = unitNode.@hp;
				unitObj.attack = unitNode.@attack;
				unitObj.defense = unitNode.@defense;
				unitObj.vision = unitNode.@vision;
				unitObj.stealth = unitNode.@stealth;
				unitObj.range = unitNode.@range;
				unitObj.speed = unitNode.@speed;
				unitObj.trainResources = new Resources(unitNode.@crop, unitNode.@gold, unitNode.@iron, unitNode.@wood, unitNode.@labor);
				unitObj.trainTime = unitNode.@time;
				unitObj.upgradeResources = new Resources(unitNode.@upgradecrop, unitNode.@upgradegold, unitNode.@upgradeiron, unitNode.@upgradewood, unitNode.@upgradelabor);
				unitObj.upgradeTime = unitNode.@time;
				unitObj.weapon = unitNode.@weapon;
				unitObj.armor = unitNode.@armor;
				unitObj.upkeep = unitNode.@upkeep;

				unitPrototypes.add(unitObj, false);
			}

			unitPrototypes.sort();
		}

		public static function getPrototype(type: int, level: int): UnitPrototype
		{
			return unitPrototypes.get([type, level]);
		}

		public static function getSprite(type: int, level: int): DisplayObjectContainer
		{
			var unitPrototype: UnitPrototype = getPrototype(type, level);
			var objRef: Class;

			if (unitPrototype == null)
			{
				trace("Missing unit prototype. type: " + type.toString() + " lvl: " + level.toString() + " Loading generic unit");
				objRef = getDefinitionByName("DEFAULT_UNIT") as Class;
			}
			else
			{
				try
				{
					objRef = getDefinitionByName(unitPrototype.spriteClass) as Class;
				}
				catch (error: Error)
				{
					trace("Missing sprite " + unitPrototype.spriteClass + ". Loading generic unit");
					objRef = getDefinitionByName("DEFAULT_UNIT") as Class;
				}
			}

			return new objRef() as DisplayObjectContainer;
		}

	}

}
