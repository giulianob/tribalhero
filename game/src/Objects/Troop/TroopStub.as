package src.Objects.Troop {
	import adobe.utils.CustomActions;
	import flash.utils.ByteArray;
	import flash.utils.Dictionary;
	import mx.events.ResourceEvent;
	import src.Global;
	import src.Map.City;
	import src.Objects.Location;
	import src.Objects.Resources;
	import src.Util.Util;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Util.BinaryList.*;    
	import src.Util.StringHelper;
    import System.Collection.Generic.IEqualityComparer;

	public class TroopStub extends BinaryList {

		public static const IDLE: int = 0;
		public static const BATTLE: int = 1;
		public static const STATIONED: int = 2;
		public static const BATTLE_STATIONED: int = 3;
		public static const MOVING: int = 4;
		public static const RETURNING_HOME: int = 5;
        public static const WAITING_IN_DEFENSIVE_ASSIGNMENT:int = 6;
        public static const WAITING_IN_OFFENSIVE_ASSIGNMENT: int = 7;
		
		public static const ATTACK_MODE_WEAK: int = 0;
		public static const ATTACK_MODE_NORMAL: int = 1;
		public static const ATTACK_MODE_STRONG: int = 2;
		
		public static const REPORT_STATE_ENTERING: int = 0;
		public static const REPORT_STATE_STAYING: int = 1;
		public static const REPORT_STATE_EXITING: int = 2;
		public static const REPORT_STATE_DYING: int = 3;
		public static const REPORT_STATE_RETREATING: int = 4;
		public static const REPORT_STATE_REINFORCED: int = 5;
		public static const REPORT_STATE_OUT_OF_STAMINA: int = 6;

		public static const STATE_NAMES: Array = ["IDLE", "BATTLE", "STATIONED", "BATTLE_STATIONED", "MOVING", "RETURNING_HOME", "WAITING_IN_DEFENSIVE_ASSIGNMENT", "WAITING_IN_OFFENSIVE_ASSIGNMENT"];

		public var id: int;
		public var state: int = 0;
		public var attackMode: int;

		public var x: int;
		public var y: int;

		public var cityId: int;
		public var objectId: int;
		public var playerId: int;

		public var template: TroopTemplateManager = new TroopTemplateManager();
		
		public var stationedLocation: * ;
		
		public var resources: Resources;
		
		public function TroopStub(id: int = 0, playerId: int = 0, cityId: int = 0)
		{
			super(Formation.sortOnType, Formation.compareType);
			this.id = id;
			this.playerId = playerId;
			this.cityId = cityId;
		}

		public function isStationed() : Boolean
		{
			return state == BATTLE_STATIONED || state == STATIONED;
		}
		
		public function isStationedNotInBattle() : Boolean
		{
			return state == STATIONED;
		}

		public function isLocal() : Boolean {
			return id == 1;
		}
        
        public function isMoving() : Boolean {
            return state == RETURNING_HOME || state == BATTLE || state == MOVING;
        }
		
		public function getNiceId(includeParenthesis: Boolean = false) : String {			
			return (includeParenthesis?"(":"") + (id == 1 ? StringHelper.localize("TROOP_LOCAL") : id.toString()) + (includeParenthesis?")":"");
		}

		public function getStateName(): String {
			var str: String = StringHelper.localize("TROOP_STATE_" + STATE_NAMES[state]);
			if (str && str != "")
			return str;

			return "[" + STATE_NAMES[state] + "]";
		}

		public function getSpeed(city: City) : Number
		{			
			var count: int = 0;
			var totalSpeed: Number = 0;
			var machineSpeed: int = int.MAX_VALUE;
			
			for each (var formation: Formation in this)
			{
				for each (var unit: Unit in formation) {
					var template: UnitTemplate = city.template.get(unit.type);
					var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, template.level);
					
					if (unitPrototype.unitClass == "Machine") {
						  machineSpeed = Math.min(unitPrototype.speed, machineSpeed);
					} else {
						count += (unit.count * unitPrototype.upkeep);
						totalSpeed += (unit.count * unitPrototype.upkeep * unitPrototype.speed);		
					}
				}
			}

			return Util.roundNumber(machineSpeed == int.MAX_VALUE ? totalSpeed / count: machineSpeed, 1);
		}

		public function getIndividualUnitCount(type: int = -1): int
		{
			var total: int = 0;
			for each (var formation: Formation in this)
			{
				total += formation.getIndividualUnitCount(type);
			}

			return total;
		}

        public function addTroop(troop: TroopStub) : void {
            for each (var formation: Formation in troop)
            {
                var local: Formation = this.get(formation.type);
                if(!local)      {
                    local = new Formation(formation.type)
                    this.add(local);
                }
                for each (var unit: Unit in formation) {
                    local.add(new Unit(unit.type,unit.count));
                }
            }
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
			
			for each (var formation: Formation in this)
			{
				// InBattle formation always uses the troop's template
				total += formation.getUpkeep(formation.type == Formation.InBattle && !forceUsingCityTemplate ? template : useTemplate);
			}

			return total;
		}
		
		public function toUnitsArray(): Dictionary {
			var units: Dictionary = new Dictionary();
			
			for each(var formation: Formation in this)
			{
				for each(var unit: Unit in formation)
				{
					if (!units.hasOwnProperty(unit.type)) {
						units[unit.type] = 0;
					}
					
					units[unit.type] += unit.count;
				}
			}
			
			return units;
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

