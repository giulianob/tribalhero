package src.Objects.Effects {
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Prototypes.*
	import src.Util.Util;

	/**
	 * ...
	 * @author Default
	 */
	public class RequirementFormula {

		private static var methodLookup: Array = new Array(
		{name: "CanBuild", method: canBuild, message: canBuildMsg},
		{name: "HaveTechnology", method: haveTechnology, message: haveTechnologyMsg },
		{name: "CanTrain", method: canTrain, message: canTrainMsg },
		{name: "HaveStructure", method: haveStructure, message: haveStructureMsg },
		{name: "HaveNoStructure", method: haveNoStructure, message: haveNoStructureMsg }
		);

		private static var methodsSorted: Boolean = false;

		public static function methodCompare(data: Object, value: String): int
		{
			if (data.name > value)
			return 1;
			else if (data.name < value)
			return -1;
			else
			return 0;
		}

		private static function getMethodIndex(effectReq: EffectReqPrototype): int
		{
			if (!methodsSorted)
			{
				methodLookup.sortOn("name");
				methodsSorted = true;
			}

			return Util.binarySearch(methodLookup, methodCompare, effectReq.method);
		}

		public static function validate(parentObj: GameObject, effectReq: EffectReqPrototype, effects: Array): Boolean
		{
			var idx: int = getMethodIndex(effectReq);

			if (idx == -1) {
				trace("Missing effect requirement formula for effect " + effectReq.method);
				return true;
			}

			return methodLookup[idx].method(parentObj, effects, effectReq.param1, effectReq.param2, effectReq.param3, effectReq.param4, effectReq.param5);
		}

		public static function getMessage(parentObj: GameObject, effectReq: EffectReqPrototype): String
		{
			var idx: int = getMethodIndex(effectReq);

			if (idx == -1)
			return effectReq.method;

			if (methodLookup[idx].message)
			{
				return methodLookup[idx].message(parentObj, effectReq.param1, effectReq.param2, effectReq.param3, effectReq.param4, effectReq.param5);
			}
			else
			{
				return methodLookup[idx].name;
			}
		}

		public static function getMessages(parentObj: GameObject, effectReqs: Array): Array
		{
			var ret: Array = new Array();

			for each (var effectReq: EffectReqPrototype in effectReqs)
			ret.push(getMessage(parentObj, effectReq));

			return ret;
		}

		/*CAN BUILD*/
		private static function canBuild(parentObj: GameObject, effects: Array, structId: int, param2: int, param3: int, param4: int, param5: int): Boolean
		{
			for each(var effect: EffectPrototype in effects)
			{
				if (effect.effectCode != EffectPrototype.EFFECT_CAN_BUILD || effect.param1 != structId)
				continue;

				return true;
			}

			return false;
		}

		private static function canBuildMsg(parentObj: GameObject,structId: int, param2: int, param3: int, param4: int, param5: int): String
		{
			var structPrototype: StructurePrototype  = StructureFactory.getPrototype(structId, 1);

			if (structPrototype == null)
			{
				trace("CanBuild requirement formula referencing missing struct id: " + structId);
				return "[Missing Struct]";
			}

			return structPrototype.getName();
		}

		/*CAN TRAIN*/
		private static function canTrain(parentObj: GameObject,effects: Array, unitId: int, level: int, param3: int, param4: int, param5: int): Boolean
		{
			for each(var effect: EffectPrototype in effects)
			{
				if (effect.effectCode != EffectPrototype.EFFECT_CAN_TRAIN || effect.param1 != unitId)
				continue;

				//how can we access the template here?

				return true;
			}

			return false;
		}

		private static function canTrainMsg(parentObj: GameObject,unitId: int, param2: int, param3: int, param4: int, param5: int): String
		{
			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unitId, 1);

			if (unitPrototype == null)
			{
				trace("CanTrain requirement formula referencing missing unit id: " + unitId);
				return "[Missing Unit]";
			}

			return unitPrototype.getName();
		}

		/*HAVE TECHNOLOGY*/
		private static function haveTechnology(parentObj: GameObject,effects: Array, techType: int, techLevel: int, count: int, param4: int, param5:int): Boolean
		{
			var total: int = 0;
			for each(var effect: EffectPrototype in effects)
			{
				if (effect.effectCode != EffectPrototype.EFFECT_HAVE_TECHNOLOGY || effect.param1 != techType || techLevel < effect.param2)
				continue;

				total++;

				if (total == count)
				return true;
			}

			return false;
		}

		private static function haveTechnologyMsg(parentObj: GameObject,techType: int, techLevel: int, count: int, param4: int, param5:int): String
		{
			var tech: TechnologyPrototype = TechnologyFactory.getPrototype(techType, techLevel);
			//temporary keying NLS name from description
			var techName: String = "";

			if (count > 1)
			techName += count + "x";

			techName += tech.getName();

			if (techName == '' || techName == null)
			techName = "[" + tech.spriteClass + "]";

			return techName + " (Lvl " + techLevel.toString() + ")";
		}

		/*HAVE STRUCTURE*/
		private static function haveStructure(parentObj: GameObject,effects: Array, type: int, minlevel: int, maxlevel: int, param4: int, param5:int): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.cityId);

			for each (var obj: CityObject in city.objects.each())
			{
				if (obj.type == type && obj.level >= minlevel && obj.level <= maxlevel)
				return true;
			}

			return false;
		}

		private static function haveStructureMsg(parentObj: GameObject, type: int, minlevel: int, maxlevel: int, param4: int, param5:int): String
		{
			var structPrototype: StructurePrototype = StructureFactory.getPrototype(type, minlevel);

			return structPrototype.getName() + " (Lvl " + minlevel.toString() + "-" + maxlevel.toString() + ")";
		}

		/*HAVE NO STRUCTURE*/
		private static function haveNoStructure(parentObj: GameObject,effects: Array, type: int, minlevel: int, maxlevel: int, param4: int, param5:int): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.cityId);

			for each (var obj: CityObject in city.objects.each())
			{
				if (obj.type == type && obj.level >= minlevel && obj.level <= maxlevel)
				return false;
			}

			return true;
		}

		private static function haveNoStructureMsg(parentObj: GameObject, type: int, minlevel: int, maxlevel: int, param4: int, param5:int): String
		{
			var structPrototype: StructurePrototype = StructureFactory.getPrototype(type, minlevel);

			return "Has not built " + structPrototype.getName() + " (Lvl " + minlevel.toString() + "-" + maxlevel.toString() + ")";
		}
	}

}

