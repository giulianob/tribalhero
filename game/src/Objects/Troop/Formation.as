/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Troop {
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Util.BinaryList.*;

	public class Formation extends BinaryList {

		public static const Normal: int = 1;
		public static const Attack: int = 2;
		public static const Defense: int = 3;
		public static const Scout: int = 4;
		public static const Garrison: int = 5;
		public static const Structure: int = 6;
		public static const InBattle: int = 7;
		public static const Captured: int = 11;
		public static const Wounded: int = 12;
		public static const Killed: int = 13;

		public static const TypeStrings: Array = ["", "Normal", "Attack", "Defense", "Scout", "Hiding", "Structure", "In Battle", "", "", "", "", "Captured", "Wounded", "Killed"];

		public var type: int;

		public function Formation(type: int)
		{
			super(Unit.sortOnType, Unit.compareUnitType);
			this.type = type;
		}

		public function getIndividualUnitCount(type: int = -1): int
		{
			var total: int = 0;
			for each(var unit: Unit in this)
			{
				if (type > -1 && unit.type != type) continue;

				total += unit.count;
			}

			return total;
		}

		public function getUpkeep(template: *): int
		{
			var total: int = 0;
			for each(var unit: Unit in this)
			{
				var unitPrototype: UnitPrototype;
				
				// Template is from a city
				if (template is UnitTemplateManager) {
					var unitTemplate: UnitTemplate = template.get(unit.type);
					unitPrototype = UnitFactory.getPrototype(unitTemplate.type, unitTemplate.level);
				} 
				// Use troop template
				else {
					var troopTemplate: TroopTemplate = template.get(unit.type);
					unitPrototype = UnitFactory.getPrototype(troopTemplate.type, troopTemplate.level);
				}

				total += (unitPrototype.upkeep * unit.count);
			}

			return total;
		}

		override public function add(obj: *, resort: Boolean = true):void
		{
			var unit: Unit = get((obj as Unit).type);

			if (unit == null) {
				super.add(obj, resort);
				unit = obj as Unit;
			} else {
				unit.count += ((obj as Unit).count);
			}
		}

		override public function remove(val: *): *
		{
			var type: int = val[0];
			var count: int = val[1];

			var unit: Unit = get(type);

			if (unit == null)
			return;

			unit.count -= count;

			if (unit.count <= 0)
			return super.remove(type);

			return unit;
		}

		public static function sortOnType(a:Formation, b:Formation):Number
		{
			var aType:Number = a.type;
			var bType:Number = b.type;

			if(aType > bType) {
				return 1;
			} else if(aType < bType) {
				return -1;
			} else  {
				return 0;
			}
		}

		public static function compareType(a: Formation, value: int):int
		{
			return a.type - value;
		}

	}

}

