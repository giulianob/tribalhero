package src.Objects.Effects {
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Factories.*;
	import src.Objects.GameObject;
	import src.Objects.Prototypes.*
	import src.Objects.TechnologyStats;
	import src.Util.Util;

	/**
	 * ...
	 * @author Default
	 */
	public class RequirementFormula {

		private static var methodLookup: Array = new Array(
		{name: "Message", method: custom, message: customMsg},
		{name: "CanBuild", method: canBuild, message: canBuildMsg},
		{name: "HaveTechnology", method: haveTechnology, message: haveTechnologyMsg },
		{name: "HaveStructure", method: haveStructure, message: haveStructureMsg },
		{name: "HaveNoStructure", method: haveNoStructure, message: haveNoStructureMsg },
		{name: "CountLessThan", method: countLessThan, message: countLessThanMsg },
		{name: "DefensePoint", method: defensePoint, message: defensePointMsg },
		{name: "AttackPoint", method: attackPoint, message: attackPointMsg },
		{name: "HaveUnit", method: haveUnit, message: haveUnitMsg },
		{name: "UniqueTechnology", method: uniqueTechnology, message: uniqueTechnologyMsg }
		);

		private static var methodsSorted: Boolean = false;

		public static function methodCompare(data: Object, value: String): int
		{
			if (data.name > value) return 1;
			else if (data.name < value) return -1;
			else return 0;
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

			if (idx <= -1) {
				trace("Missing effect requirement formula for effect " + effectReq.method);
				return true;
			}

			return methodLookup[idx].method(parentObj, effects, effectReq.param1, effectReq.param2, effectReq.param3, effectReq.param4, effectReq.param5);
		}

		public static function getMessage(parentObj: GameObject, effectReq: EffectReqPrototype): String
		{
			if (effectReq.description != "" && effectReq.description != null) {
				return effectReq.description;
			}

			var idx: int = getMethodIndex(effectReq);

			if (idx <= -1) {
				return effectReq.method;
			}

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

		/*CUSTOM*/
		private static function custom(parentObj: GameObject, effects: Array, message: String, param2: String, param3: String, param4: String, param5: String): Boolean
		{
			return false;
		}

		private static function customMsg(parentObj: GameObject, message: String, param2: String, param3: String, param4: String, param5: String): String
		{		
			return message;
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

			return "Upgraded " + techName + " to level " + techLevel.toString();
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

			return "Built a " + structPrototype.getName() + " (Lvl " + minlevel.toString() + (minlevel != maxlevel || minlevel > 1 ? "-" + maxlevel.toString() : "") + ")";
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

			return "You can only have one " + structPrototype.getName() + (minlevel > 0 ? " (Lvl " + minlevel.toString() + "-" + maxlevel.toString() + ")" : "");
		}

		/*COUNT LESS THAN*/
		private static function countLessThan(parentObj: GameObject, effects: Array, effectCode: int, maxCount: int, param3: int, param4: int, param5:int): Boolean
		{
			var count: int = 0;
			for each (var effect: EffectPrototype in effects) {
				if (effect.effectCode == EffectPrototype.EFFECT_COUNT_LESS_THAN && effect.param1 == effectCode)
				count += effect.param2;
			}

			return count < maxCount;
		}

		private static function countLessThanMsg(parentObj: GameObject, effectCode: int, maxCount: int, param3: int, param4: int, param5:int): String
		{
			return "Count less than";
		}

		/*DEFENSE POINT*/
		private static function defensePoint(parentObj: GameObject, effects: Array, comparison: String, value: int, param3: int, param4: int, param5:int): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.cityId);

			if (city == null) return false;

			if (comparison == "lt")
			return city.defensePoint < value;

			if (comparison == "gt")
			return city.defensePoint > value;

			return false;
		}

		private static function defensePointMsg(parentObj: GameObject, comparison: String, value: int, param3: int, param4: int, param5:int): String
		{
			var city: City = Global.map.cities.get(parentObj.cityId);

			if (city == null) return "";

			if (comparison == "lt")
			return "Less than " + value + " defense points" + (city.defensePoint >= value ? ". You currently have " + city.defensePoint + " defense points." : "");

			if (comparison == "gt")
			return "More than " + value + " defense points" + (city.defensePoint <= value ? ". You currently have " + city.defensePoint + " defense points." : "");

			return "";
		}

		/*ATTACK POINT*/
		private static function attackPoint(parentObj: GameObject, effects: Array, comparison: String, value: int, param3: int, param4: int, param5:int): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.cityId);

			if (city == null) return false;

			if (comparison == "lt")
			return city.attackPoint < value;

			if (comparison == "gt")
			return city.attackPoint > value;

			return false;
		}

		private static function attackPointMsg(parentObj: GameObject, comparison: String, value: int, param3: int, param4: int, param5:int): String
		{
			var city: City = Global.map.cities.get(parentObj.cityId);

			if (city == null) return "";

			if (comparison == "lt")
			return "Less than " + value + " attack points" + (city.attackPoint >= value ? ". You currently have " + city.attackPoint + " attack points." : "");

			if (comparison == "gt")
			return "More than " + value + " attack points" + (city.attackPoint <= value ? ". You currently have " + city.attackPoint + " attack points." : "");

			return "";
		}

		/*HAVE UNIT*/
		private static function haveUnit(parentObj: GameObject, effects: Array, type: int, comparison: String, count: int, param4: int, param5:int): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.cityId);

			if (city == null) return false;

			var total: int = city.troops.getIndividualUnitCount(type);

			if (comparison == "lt")
			return total < count;

			if (comparison == "gt")
			return total > count;

			return false;
		}

		private static function haveUnitMsg(parentObj: GameObject, type: int, comparison: String, count: int, param4: int, param5:int): String
		{
			var unit: UnitPrototype = UnitFactory.getPrototype(type, 1);

			if (!unit) return "";

			var city: City = Global.map.cities.get(parentObj.cityId);

			if (city == null) return "";

			var total: int = city.troops.getIndividualUnitCount(type);

			if (comparison == "lt")
			return "Trained less than " + count + " " + unit.getName(count).toLowerCase() + (total >= count ? ". You currently have " + total + " " + unit.getName(total).toLowerCase() + ".": "");

			if (comparison == "gt")
			return "Trained at least " + (count+1) + " " + unit.getName(count+1).toLowerCase() + (total <= count ? ". You currently have " + total + " " + unit.getName(total).toLowerCase() + ".": "");

			return "";
		}

		/*HAVE UNIT*/
		private static function uniqueTechnology(parentObj: GameObject, effects: Array, type: int, param2: int, param3: int, param4: int, param5:int): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.cityId);

			if (city == null) return false;

			for each (var obj: CityObject in city.objects.each()) {
				if (ObjectFactory.getClassType(obj.type) != ObjectFactory.TYPE_STRUCTURE) continue;

				// Skip the current obj
				if (obj.objectId == parentObj.objectId) continue;

				for each (var tech: TechnologyStats in obj.techManager.technologies) {
					if (tech.prototype.level > 0 && tech.prototype.techtype == type)
					return false;
				}
			}

			return true;
		}

		private static function uniqueTechnologyMsg(parentObj: GameObject, type: int, param2: int, param3: int, param4: int, param5:int): String
		{
			return "Does not already have this technology elsewhere";
		}
	}
}

