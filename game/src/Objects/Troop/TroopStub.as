package src.Objects.Troop {
	import src.Util.BinaryList.*;
	import fl.lang.Locale;

	public class TroopStub extends BinaryList {

		public static const IDLE: int = 0;
		public static const BATTLE: int = 1;
		public static const STATIONED: int = 2;
		public static const BATTLE_STATIONED: int = 3;
		public static const MOVING: int = 4;

		public static const STATE_NAMES: Array = ["IDLE", "BATTLE", "STATIONED", "BATTLE_STATIONED", "MOVING"];

		public var id: int;
		public var state: int = 0;
		public var upkeep: int = 0;

		public var x: int;
		public var y: int;

		public var cityId: int;
		public var objectId: int;
		public var playerId: int;

		public var template: TroopTemplateManager = new TroopTemplateManager();

		public function TroopStub(id: int = 0)
		{
			super(Formation.sortOnType, Formation.compareType);
			this.id = id;
		}

		public function isStationed() : Boolean
		{
			return state == BATTLE_STATIONED || state == STATIONED;
		}

		public function getNiceId(includeParenthesis: Boolean = false) : String {
			if (id == 1) {
				if (includeParenthesis) return "(Local Troop)";
				else return "Local Troop";
			}
			else return (includeParenthesis?"(":"") + id.toString()  + (includeParenthesis?")":"");
		}

		public function getStateName(): String {
			var str: String = Locale.loadString("TROOP_STATE_" + STATE_NAMES[state]);
			if (str && str != "")
			return str;

			return "[" + STATE_NAMES[state] + "]";
		}

		public function getSpeed() : int
		{
			var count: int = 0;

			for each (var formation: Formation in each())
			{
				if (formation.type == Formation.Scout) continue;
				for each (var unit: Unit in formation.each()) {
					count += unit.count;
				}
			}

			count /= 100;

			return Math.min(15, 10 - Math.max(count, 5));
		}

		public function getIndividualUnitCount(): int
		{
			var total: int = 0;
			for each (var formation: Formation in each())
			{
				total += formation.getIndividualUnitCount();
			}

			return total;
		}

		public function ToString():void
		{
			trace("=========");
			trace("Troop " + id );
			trace("Formation count: " + size());
			trace("Total unit count: " + getIndividualUnitCount());
			for each (var formation: Formation in each())
			{
				trace("\tFormation: " + formation.type);
				trace("\tSize: " + formation.size());
				for each (var unit: Unit in formation.each())
				{
					trace("\t\tUnit: " + unit.type + " (" + unit.count + ")");
				}
			}
			trace("=========");
		}

		public static function compareCityIdAndTroopId(a: TroopStub, value: Array):int
		{
			var cityDelta: int = a.cityId - value[0];
			var idDelta: int = a.id - value[1];

			if (cityDelta != 0)
			return cityDelta;

			if (idDelta != 0)
			return idDelta;
			else
			return 0;
		}

		public static function sortOnCityIdAndTroopId(a:TroopStub, b:TroopStub):Number {
			var aCityId:Number = a.cityId;
			var bCityId:Number = b.cityId;

			var aObjId:Number = a.id;
			var bObjId:Number = b.id;

			if (aCityId > bCityId)
			return 1;
			else if (aCityId < bCityId)
			return -1;
			else if (aObjId > bObjId)
			return 1;
			else if (aObjId < bObjId)
			return -1;
			else
			return 0;
		}
	}

}

