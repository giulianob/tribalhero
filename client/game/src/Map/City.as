/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Map {
    import flash.events.Event;
    import flash.events.EventDispatcher;

    import src.Objects.*;
    import src.Objects.Actions.*;
    import src.Objects.Battle.BattleManager;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Prototypes.EffectPrototype;
    import src.Objects.Troop.*;
    import src.Util.BinaryList.*;
    import src.Util.Util;

    public class City extends EventDispatcher
	{
		public static var RESOURCES_UPDATE: String = "RESOURCES_UPDATE";
		public static var RADIUS_UPDATE: String = "RADIUS_UPDATE";

		public var id: int;
		public var resources: LazyResources;
		public var name: String;
		public var radius: int = 0;
		public var attackPoint: int = 0;
		public var defensePoint: int = 0;
		public var value: int = 0;
		public var ap: Number = 0;
        public var defaultTheme: String;
        public var troopTheme: String;

		public var hideNewUnits: Boolean;

		public var inBattle: Boolean;

		public var battle: BattleManager;

		public var techManager: TechnologyManager = new TechnologyManager(EffectPrototype.LOCATION_CITY);

		public var currentActions: CurrentActionManager = new CurrentActionManager();
		public var notifications: NotificationManager = new NotificationManager();
		public var references: ReferenceManager = new ReferenceManager();

		public var troops: TroopManager;
		public var objects: BinaryList = new BinaryList(CityObject.sortOnId, CityObject.compareObjId);
		public var template: UnitTemplateManager = new UnitTemplateManager();
        public var primaryPosition: Position;

		public function get MainBuilding() : CityObject {
			return objects.get(1);
		}

		public function City(id: int,
                             name: String,
                             position: Position,
                             radius: int,
                             resources: LazyResources,
                             attackPoint: int,
                             defensePoint: int,
                             value: int,
                             inBattle: Boolean,
                             hideNewUnits : Boolean,
                             ap: Number,
                             defaultTheme: String,
                             troopTheme: String) {
			this.id = id;
			this.resources = resources;
			this.radius = radius;
			this.ap = ap;
			this.name = name;
            this.primaryPosition = position;
			this.attackPoint = attackPoint;
			this.defensePoint = defensePoint;
			this.inBattle = inBattle;
			this.hideNewUnits = hideNewUnits;
			this.value = value;
            this.defaultTheme = defaultTheme;
            this.troopTheme = troopTheme;

            troops = new TroopManager(this);

			dispatchEvent(new Event(RESOURCES_UPDATE));
		}

		public function nearObjectsByRadius(mindist: int, maxdist: int, position: Position, size: int, type: int): Array
		{
			var ret: Array = [];

			for each(var obj: CityObject in objects)
			{
				if (type != -1 && obj.type != type) {
				    continue;
                }

				var dist: Number = TileLocator.radiusDistance(obj.x, obj.y, obj.size, position.x, position.y, size);

				if (mindist <= dist && (maxdist == -1 || maxdist >= dist)) {
				    ret.push(obj);
                }
			}

			return ret;
		}

		public function getStructureOfType(type: int): CityObject
		{
			for each(var obj: CityObject in objects)
			{
				if (ObjectFactory.getClassType(obj.type) != ObjectFactory.TYPE_STRUCTURE)
					continue;
				
				if (obj.type == type)
					return obj;				
			}
			
			return null;
		}

        public function structures(): Array {
            var structures: Array = [];

            for each(var obj: CityObject in objects)
            {
                if (ObjectFactory.getClassType(obj.type) != ObjectFactory.TYPE_STRUCTURE) {
                    continue;
                }

                structures.push(obj);
            }

            return structures;
        }
		
		public function hasStructureAt(mapPos: Position): Boolean
		{
			return getStructureAt(mapPos) != null;
		}

		public function getStructureAt(mapPos: Position): CityObject
		{
			for each(var obj: CityObject in objects)
			{
				if (ObjectFactory.getClassType(obj.type) != ObjectFactory.TYPE_STRUCTURE) {
				    continue;
                }

                for each (var position: Position in TileLocator.foreachMultitile(obj.x, obj.y, obj.size)) {
                    if (position.equals(mapPos)) {
                        return obj;
                    }
                }
			}

			return null;
		}

        public function getBusyLaborCount() : int {
			var labors: int = 0;

			for each(var obj: CityObject in objects)
			{
				labors += obj.labor;
			}

			return labors;
		}
		
		public function validateAction(action: Action, parentObj: GameObject): Boolean {
			// Button doesnt have a real action, dont validate it
			if (!action)
				return true;
			
			var concurrency: int = Action.actionConcurrency[action.actionType];
			
			if (!concurrency) {
				Util.log("Missing concurrency for action " + action.actionType);
				return true;
			}
				
			for each (var currentAction: CurrentActiveAction in currentActions.getObjectActions(parentObj.objectId, true)) {
				var otherConcurrency: int = Action.actionConcurrency[currentAction.getType()];
				
				switch (concurrency) {
					case Action.CONCURRENCY_STANDALONE:
						return false;
					case Action.CONCURRENCY_NORMAL:
						if (otherConcurrency == Action.CONCURRENCY_STANDALONE || otherConcurrency == Action.CONCURRENCY_NORMAL)
							return false;
						break;
					case Action.CONCURRENCY_CONCURRENT:
						if (otherConcurrency == Action.CONCURRENCY_STANDALONE || action.actionType == currentAction.getType())
							return false;
						break;
				}
			}
				
			return true;
		}

		public static function sortOnId(a:City, b:City):Number
		{
			var aId:Number = a.id;
			var bId:Number = b.id;

			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}

		public static function compare(a: City, value: int):int
		{
			return a.id - value;
		}
	}

}

