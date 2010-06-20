package src.Objects.Factories {
	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.display.SimpleButton;
	import flash.filters.BlurFilter;
	import flash.geom.ColorTransform;
	import flash.geom.Matrix;
	import flash.geom.Rectangle;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Map.Map;
	import src.Objects.Actions.TechUpgradeAction;
	import src.Objects.Prototypes.AwayFromLayout;
	import src.Objects.Prototypes.ILayout;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.Prototypes.SimpleLayout;
	import src.Objects.Prototypes.TechnologyPrototype;
	import src.Objects.SimpleObject;
	import src.Objects.StructureObject;
	import src.UI.Sidebars.ObjectInfo.Buttons.TechnologyButton;
	import src.Util.BinaryList.*;
	import src.Objects.Resources;
	import src.Objects.Prototypes.Worker;
	import src.Objects.TechnologyStats;

	import flash.utils.getDefinitionByName;

	/**
	 * ...
	 * @author Default
	 */
	public class StructureFactory {

		private static var map: Map;
		private static var structurePrototypes: BinaryList;

		public static function init(_map: Map, data: XML):void
		{
			map = _map;

			structurePrototypes = new BinaryList(StructurePrototype.sortOnTypeAndLevel, StructurePrototype.compareTypeAndLevel);

			for each (var structNode: XML in data.Structures.*)
			{
				var strctObj: StructurePrototype = new StructurePrototype();
				strctObj.name = structNode.@name.toUpperCase();
				strctObj.type = structNode.@type;
				strctObj.level = structNode.@level;
				strctObj.baseClass = structNode.@baseclass;
				strctObj.spriteClass = structNode.@spriteclass;
				strctObj.hp = structNode.@hp;
				strctObj.maxlabor = structNode.@maxlabor;
				strctObj.attack = structNode.@attack;
				strctObj.defense = structNode.@defense;
				strctObj.radius = structNode.@radius;
				strctObj.stealth = structNode.@stealth;
				strctObj.range = structNode.@range;
				strctObj.speed = structNode.@speed;
				strctObj.buildResources = new Resources(structNode.@crop, structNode.@gold, structNode.@iron, structNode.@wood, 0);
				strctObj.buildTime = structNode.@time;
				strctObj.weapon = structNode.@weapon;
				strctObj.workerid = structNode.@workerid;

				for each (var layoutNode: XML in structNode.Layout.*)
				{
					var layout: * ;
					var name: String = layoutNode.name().localName;
					switch (name) {
						case "Simple":
							layout = new SimpleLayout();
						break;
						case "AwayFrom":
							layout = new AwayFromLayout();
						break;
						default:
							continue;
					}

					layout.type = layoutNode.@type;
					layout.minlevel = layoutNode.@minlevel;
					layout.maxlevel = layoutNode.@maxlevel;
					layout.mindist = layoutNode.@mindist;
					layout.maxdist = layoutNode.@maxdist;
					layout.compare = layoutNode.@compare;

					strctObj.addLayout(layout);
				}

				structurePrototypes.add(strctObj, false);
			}

			structurePrototypes.sort();
		}

		public static function getPrototype(type: int, level: int): StructurePrototype
		{
			return structurePrototypes.get([type, level]);
		}

		public static function getSprite(type: int, level: int, centered: Boolean = false): DisplayObjectContainer
		{
			var strPrototype: StructurePrototype = getPrototype(type, level);
			var objRef: Class;

			if (strPrototype == null)
			{
				trace("Missing obj prototype. type: " + type.toString() + " lvl: " + level.toString() + " Loading generic simple object");
				objRef = getDefinitionByName("DEFAULT_STRUCTURE_SIMPLE") as Class;
			}
			else
			{
				var worker: Worker = WorkerFactory.getPrototype(strPrototype.workerid);

				if (worker == null)
				{
					trace("Missing obj worker. type: " + type.toString() + " lvl: " + level.toString() + " Loading generic simple object");
					objRef = getDefinitionByName("DEFAULT_STRUCTURE_SIMPLE") as Class;
				}
				else
				{
					try
					{
						objRef = getDefinitionByName(strPrototype.spriteClass) as Class;
					}
					catch (error: Error)
					{
						trace("Missing sprite " + strPrototype.spriteClass + ". Loading generic complex object");
						objRef = getDefinitionByName("DEFAULT_STRUCTURE_COMPLEX") as Class;
					}
				}
			}

			var sprite: DisplayObjectContainer = new objRef();

			if (centered)
			{
				var item: DisplayObject;
				for (var i: int = 0; i < sprite.numChildren; i++)
				{
					item = sprite.getChildAt(i);
					var rect: Rectangle = item.getRect(item);
					item.x -= rect.x;
					item.y -= rect.y;
				}
			}

			return sprite;
		}

		public static function getSimpleObject(type: int, level:int): SimpleObject
		{
			var sprite: DisplayObjectContainer = getSprite(type, level) as DisplayObjectContainer;
			var simpleObject: SimpleObject = new SimpleObject();
			if (sprite != null)
			simpleObject.addChild(sprite);

			return simpleObject;
		}

		public static function getInstance(type: int, level: int): Object
		{
			var structureObj: StructureObject = new StructureObject();

			var shadow: DisplayObjectContainer = StructureFactory.getSprite(type, level);
			shadow.transform.colorTransform = new ColorTransform(0, 0, 0);
			shadow.transform.matrix = new Matrix(1, 0, -0.7, 0.5, 20, 15);
			shadow.alpha = 0.4;
			shadow.filters = [new BlurFilter(5, 5)];
			shadow.mouseEnabled = false;
			structureObj.addChild(shadow);

			var img: DisplayObject = getSprite(type, level);
			structureObj.addChild(img);

			return structureObj;
		}

		public static function getButtons(parentObj: StructureObject): Array
		{
			var structPrototype: StructurePrototype = getPrototype(parentObj.type, parentObj.level);

			if (structPrototype == null)
			return null;

			var worker: Worker = WorkerFactory.getPrototype(structPrototype.workerid);

			if (worker == null)
			{
				trace("Missing worker type: " + structPrototype.workerid);
				return null;
			}
			else
			return worker.getButtons(parentObj, structPrototype);
		}

		private static function getTechButtonInstance(techPrototype: TechnologyPrototype) : SimpleButton {
			var objRef: Class;

			if (techPrototype != null)
			{
				try
				{
					objRef = getDefinitionByName(techPrototype.spriteClass + "_TECHNOLOGY_BUTTON") as Class;
				}
				catch (error: Error)
				{
					trace("Missing technology button sprite class: " + techPrototype.spriteClass + "_TECHNOLOGY_BUTTON. Loading default");
					objRef = getDefinitionByName("DEFAULT_TECHNOLOGY_BUTTON") as Class;
				}
			}
			else
			{
				trace("Missing technology prototype for object type tech type " + techPrototype.techtype + " tech level " + techPrototype.level);
				objRef = getDefinitionByName("DEFAULT_TECHNOLOGY_BUTTON") as Class;
			}

			return new objRef();
		}

		public static function getTechButtons(gameObj: StructureObject): Array
		{
			var structPrototype: StructurePrototype = getPrototype(gameObj.type, gameObj.level);

			if (structPrototype == null)
			return new Array();

			var city: City = map.cities.get(gameObj.cityId);
			if (city == null) {
				trace("StructureFactory.getTechButtons: Unknown city");
				return new Array();
			}

			var cityObj: CityObject = city.objects.get(gameObj.objectId);
			if (cityObj == null) {
				trace("StructureFactory.getTechButtons: Unknown city object");
				return new Array();
			}

			var worker: Worker = WorkerFactory.getPrototype(structPrototype.workerid);

			var upgradeActions: Array = worker.getTechUpgradeActions();

			var buttons: Array = new Array();

			// Add all of the tech buttons that have technologies attached to them
			for each (var techStats: TechnologyStats in cityObj.techManager.technologies)
			{
				var techBtn: TechnologyButton = new TechnologyButton(gameObj, structPrototype, techStats.prototype);

				for (var i: int = 0; i < upgradeActions.length; ++i) {
					if (upgradeActions[i].techtype != techStats.prototype.techtype)
					continue;

					techBtn.parentAction = upgradeActions[i];
					upgradeActions.splice(i, 1);
					break;
				}

				buttons.push(techBtn);
			}

			// Add all of the upgrade technology actions that don't currently have technologies attached to them
			for each (var upgradeAction: TechUpgradeAction in upgradeActions) {
				var techPrototype: TechnologyPrototype = TechnologyFactory.getPrototype(upgradeAction.techtype, 0);
				techBtn = new TechnologyButton(gameObj, structPrototype, techPrototype);
				techBtn.parentAction = upgradeAction;
				buttons.push(techBtn);
			}

			return buttons;
		}

		public static function getCursorInstance(action: String, type: int, level: int): Object
		{
			var strPrototype: StructurePrototype = getPrototype(type, level);

			var obj: Object = null;
			var objRef:Class = null;

			if (strPrototype != null)
			{
				try
				{
					objRef = getDefinitionByName(strPrototype.spriteClass + "_" + action + "_CURSOR") as Class;
					obj = new objRef();
				}
				catch (error: Error)
				{
					trace("Missing cursor '" + strPrototype.spriteClass + "_" + action + "_CURSOR' falling back to standard cursor");
					objRef = getDefinitionByName("STANDARD_STRUCTURE_" + action + "_CURSOR") as Class;
				}
			}
			else
			{
				objRef = getDefinitionByName("STANDARD_STRUCTURE_" + action + "_CURSOR") as Class;
			}

			obj = new objRef();
			return obj;
		}
	}

}

