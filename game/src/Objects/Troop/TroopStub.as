package src.Objects.Troop {
	import adobe.utils.CustomActions;
	import src.Global;
	import src.Map.City;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Util.BinaryList.*;
	import fl.lang.Locale;

	public class TroopStub extends BinaryList {

		public static const IDLE: int = 0;
		public static const BATTLE: int = 1;
		public static const STATIONED: int = 2;
		public static const BATTLE_STATIONED: int = 3;
		public static const MOVING: int = 4;
		public static const RETURNING_HOME: int = 5;

		public static const STATE_NAMES: Array = ["IDLE", "BATTLE", "STATIONED", "BATTLE_STATIONED", "MOVING", "RETURNING_HOME"];

		public var id: int;
		public var state: int = 0;

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

		public function getSpeed(city: City) : int
		{			
			var count: int = 0;
			var totalSpeed: int = 0;

			for each (var formation: Formation in each())
			{
				for each (var unit: Unit in formation.each()) {
					var template: UnitTemplate = city.template.get(unit.type);
					var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, template.level);
                    count += (unit.count * unitPrototype.upkeep);
                    totalSpeed += (unit.count * unitPrototype.upkeep * unitPrototype.speed);									
				}
			}

			return totalSpeed / count;
		}

		public function getIndividualUnitCount(type: int = -1): int
		{
			var total: int = 0;
			for each (var formation: Formation in each())
			{
				total += formation.getIndividualUnitCount(type);
			}

			return total;
		}

		public function getUpkeep(forceUsingCityTemplate: Boolean = false): int
		{
			var total: int = 0;
			var useTemplate: * ;
			
			// If this is local troop, then use city's template, otherwise use troop stub template
			if (id == 1 || forceUsingCityTemplate) {
				var city: City = Global.map.cities.get(cityId);
				useTemplate = city.template;
			} else {		
				useTemplate = template;
			}
			
			for each (var formation: Formation in each())
			{
				// InBattle formation always uses the troop's template
				total += formation.getUpkeep(formation.type == Formation.InBattle && !forceUsingCityTemplate ? template : useTemplate);
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

