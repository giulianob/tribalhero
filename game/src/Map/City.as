/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Map {
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.geom.Point;
	import src.Objects.Actions.*;
	import src.Objects.Battle.BattleManager;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.Prototypes.EffectPrototype;
	import src.Objects.*;
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

		public function get MainBuilding() : CityObject {
			return objects.get(1);
		}

		public function City(id: int, name: String, radius: int, resources: LazyResources, attackPoint: int, defensePoint: int, value: int, inBattle: Boolean, hideNewUnits : Boolean, ap: Number) {
			this.id = id;
			this.resources = resources;
			this.radius = radius;
			this.ap = ap;
			this.name = name;
			this.attackPoint = attackPoint;
			this.defensePoint = defensePoint;
			this.inBattle = inBattle;
			this.hideNewUnits = hideNewUnits;
			this.value = value;

			troops = new TroopManager(this);

			dispatchEvent(new Event(RESOURCES_UPDATE));
		}

		public function nearObjectsByRadius(mindist: int, maxdist: int, x: int, y: int, type: int = -1): Array
		{
			var ret: Array = new Array();

			var pos: Point = MapUtil.getMapCoord(x, y);

			for each(var obj: CityObject in objects)
			{
				if (type != -1 && obj.type != type)
				continue;

				var dist: Number = obj.radiusDistance(pos.x, pos.y);

				if (mindist <= dist && (maxdist == -1 || maxdist >= dist))
				ret.push(obj);
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
		
		public function hasStructureAt(mapPos: Point): Boolean
		{
			var ret: Array = new Array();

			for each(var obj: CityObject in objects)
			{
				if (ObjectFactory.getClassType(obj.type) != ObjectFactory.TYPE_STRUCTURE)
				continue;

				if (obj.x != mapPos.x || obj.y != mapPos.y)
				continue;

				return true;
			}

			return false;
		}

		public function getStructureAt(mapPos: Point): CityObject
		{
			var ret: Array = new Array();

			for each(var obj: CityObject in objects)
			{
				if (ObjectFactory.getClassType(obj.type) != ObjectFactory.TYPE_STRUCTURE)
				continue;

				if (obj.x != mapPos.x || obj.y != mapPos.y)
				continue;

				return obj;
			}

			return null;
		}

		public function nearObjectsByRadius2(mindist: int, maxdist: int, mapPos: Point, classType: int): Array
		{
			var ret: Array = new Array();

			for each(var obj: CityObject in objects)
			{
				if (ObjectFactory.getClassType(obj.type) != classType)
				continue;

				var dist: Number = obj.radiusDistance(mapPos.x, mapPos.y);

				if (mindist <= dist && maxdist >= dist)
				ret.push(obj);
			}

			return ret;
		}

		public function nearObjects(mindist: int, maxdist: int, x: int, y: int, type: int = -1): Array
		{
			var ret: Array = new Array();

			var pos: Point = MapUtil.getMapCoord(x, y);

			for each(var obj: CityObject in objects)
			{
				if (type != -1 && obj.type != type)
				continue;

				var dist: Number = obj.distance(pos.x, pos.y);

				if (mindist <= dist && maxdist >= dist)
				ret.push(obj);
			}

			return ret;
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

