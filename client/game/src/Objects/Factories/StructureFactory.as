package src.Objects.Factories {

    import System.Linq.Enumerable;

    import src.FeathersUI.Map.MapVM;

    import src.FlashAssets;
    import src.Global;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Actions.TechUpgradeAction;
    import src.Objects.Prototypes.*;
    import src.Objects.States.GameObjectState;
    import src.UI.Sidebars.ObjectInfo.Buttons.TechnologyButton;
    import src.Util.BinaryList.*;
    import src.Util.Util;

    public class StructureFactory {

		private static var map: MapVM;
		private static var structurePrototypes: BinaryList;

		public static function init(_map: MapVM, data: XML):void
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
				strctObj.splash = structNode.@splash;
				strctObj.radius = structNode.@radius;
				strctObj.stealth = structNode.@stealth;
				strctObj.range = structNode.@range;
				strctObj.speed = structNode.@speed;
				strctObj.buildResources = new Resources(structNode.@crop, structNode.@gold, structNode.@iron, structNode.@wood, structNode.@labor);
				strctObj.buildTime = structNode.@time;
				strctObj.weapon = structNode.@weapon;
				strctObj.workerid = structNode.@workerid;
                strctObj.size = structNode.@size;

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

        public static function getAllStructureTypes(): Array
        {
            return Enumerable.from(structurePrototypes.toArray()).where(function(strPrototype: StructurePrototype): Boolean {
                return strPrototype.level == 1
            }).toArray();
        }

		public static function getPrototype(type: int, level: int): StructurePrototype
		{
			var structPrototype: StructurePrototype = structurePrototypes.get([type, level]);
			if (!structPrototype)
				Util.log("Did not find structure prototype for type " + type + " level " + level);
				
			return structPrototype;
		}

        public static function getSpriteName(theme: String, type: int, level: int, fallbackToDefaultTheme: Boolean = false): String {
            var strPrototype: StructurePrototype = getPrototype(type, level);

            if (strPrototype == null)
            {
                throw new Error("Missing obj prototype. type: " + type.toString() + " lvl: " + level.toString() + " Loading generic simple object");
            }

            var worker: Worker = WorkerFactory.getPrototype(strPrototype.workerid);

            if (worker == null)
            {
                throw new Error("Missing structure worker " + strPrototype.workerid);
            }

            var typeName: String = strPrototype.getSpriteName(theme);

            if (fallbackToDefaultTheme && theme != Theme.DEFAULT_THEME_ID && !FlashAssets.doesSpriteExist(typeName)) {
                return getSpriteName("DEFAULT", type, level);
            }

            return typeName;
        }

		public static function getSimpleObject(theme: String, type: int, level:int, x: int, y: int, size: int): SimpleObject
		{
            var typeName: String = getSpriteName(theme, type, level, true);

			var simpleObject: SimpleObject = new SimpleObject(x, y, size);
            simpleObject.setSprite(SpriteFactory.getStarlingImage(typeName), SpriteFactory.getMapPosition(typeName));

			return simpleObject;
		}

		public static function getInstance(theme: String, type: int, state: GameObjectState, objX: int, objY: int, size: int, playerId: int, cityId: int, objectId: int, level: int, wallRadius: int, wallTheme: String): StructureObject
		{
            var typeName: String = getSpriteName(theme, type, level);

			var structureObj: StructureObject = new StructureObject(theme, type, state, objX, objY, size, playerId, cityId, objectId, level, wallRadius, wallTheme);
			structureObj.setSprite(SpriteFactory.getStarlingImage(typeName), SpriteFactory.getMapPosition(typeName));
			structureObj.setOnSelect(Global.map.selectObject);

			return structureObj;
		}

		public static function getButtons(parentObj: StructureObject): Array
		{
			var structPrototype: StructurePrototype = getPrototype(parentObj.type, parentObj.level);

			if (structPrototype == null) return null;

			var worker: Worker = WorkerFactory.getPrototype(structPrototype.workerid);

			if (worker == null)
			{
				Util.log("Missing worker type: " + structPrototype.workerid);
				return null;
			}
			else return worker.getButtons(parentObj, structPrototype);
		}

        public static function getTechButtons(gameObj: StructureObject): Array
		{
			var structPrototype: StructurePrototype = getPrototype(gameObj.type, gameObj.level);

			if (structPrototype == null)
			return [];

			var city: City = map.cities.get(gameObj.cityId);
			if (city == null) {
				Util.log("StructureFactory.getTechButtons: Unknown city");
				return [];
			}

			var cityObj: CityObject = city.objects.get(gameObj.objectId);
			if (cityObj == null) {
				Util.log("StructureFactory.getTechButtons: Unknown city object");
				return [];
			}

			var worker: Worker = WorkerFactory.getPrototype(structPrototype.workerid);

			var upgradeActions: Array = worker.getTechUpgradeActions();

			var buttons: Array = [];

			// Add all of the tech buttons that have technologies attached to them
            var technologyStats: TechnologyStats;
			for each (technologyStats in cityObj.techManager.technologies)
			{
                var techBtn: TechnologyButton = new TechnologyButton(gameObj, technologyStats.techPrototype);

				for (var i: int = 0; i < upgradeActions.length; ++i) {
					if (upgradeActions[i].techtype != technologyStats.techPrototype.techtype) {
					    continue;
                    }

					techBtn.parentAction = upgradeActions[i];
					upgradeActions.splice(i, 1);
					break;
				}

				buttons.push(techBtn);
			}

			// Add all of the upgrade technology actions that don't currently have technologies attached to them
			for each (var upgradeAction: TechUpgradeAction in upgradeActions) {
				var techPrototype: TechnologyPrototype = TechnologyFactory.getPrototype(upgradeAction.techtype, 0);
				techBtn = new TechnologyButton(gameObj, techPrototype);
				techBtn.parentAction = upgradeAction;
				buttons.push(techBtn);
			}

			return buttons;
		}

}

}

