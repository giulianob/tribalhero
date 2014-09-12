package src.Objects.Factories {
    import src.Objects.Prototypes.UnitPrototype;
    import src.Objects.Resources;
    import src.Util.BinaryList.*;
    import src.Util.Util;

    public class UnitFactory {

		private static var unitPrototypes: BinaryList;

        public static function init(data: XML): void {
			unitPrototypes = new BinaryList(UnitPrototype.sortOnTypeAndLevel, UnitPrototype.compareTypeAndLevel);

			for each (var unitNode: XML in data.Units.*)
			{
				var unitObj: UnitPrototype = new UnitPrototype();
				unitObj.nlsname = unitNode.@name;
				unitObj.type = unitNode.@type;
				unitObj.level = unitNode.@level;
				unitObj.baseClass = unitNode.@baseclass;
				unitObj.spriteClass = unitNode.@spriteclass;
				unitObj.hp = Util.roundNumber(unitNode.@hp);
				unitObj.attack = Util.roundNumber(unitNode.@attack);
				unitObj.defense = unitNode.@defense;
				unitObj.splash = unitNode.@splash;
				unitObj.vision = unitNode.@vision;
				unitObj.stealth = unitNode.@stealth;
				unitObj.range = unitNode.@range;
				unitObj.speed = unitNode.@speed;
				unitObj.trainResources = new Resources(unitNode.@crop, unitNode.@gold, unitNode.@iron, unitNode.@wood, unitNode.@labor);
				unitObj.trainTime = unitNode.@time;
				unitObj.upgradeResources = new Resources(unitNode.@upgradecrop, unitNode.@upgradegold, unitNode.@upgradeiron, unitNode.@upgradewood, unitNode.@upgradelabor);
				unitObj.upgradeTime = unitNode.@upgradetime;
				unitObj.weapon = unitNode.@weapon;
				unitObj.unitClass = unitNode.@unitclass;
				unitObj.weaponClass = unitNode.@weaponclass;
				unitObj.armor = unitNode.@armor;
				unitObj.upkeep = unitNode.@upkeep;
				unitObj.carry = unitNode.@carry;

				unitPrototypes.add(unitObj, false);
			}

			unitPrototypes.sort();
		}

		public static function getPrototype(type: int, level: int): UnitPrototype
		{
			return unitPrototypes.get([type, level]);
		}

        public static function getSpriteName(type: int, level: int, forDarkBackground: Boolean = false): String {
            var unitPrototype: UnitPrototype = getPrototype(type, level);
            var typeName: String;

            if (unitPrototype == null)
            {
                throw new Error("Missing unit prototype. type: " + type.toString() + " lvl: " + level.toString());
            }

            typeName = unitPrototype.spriteClass;

            if (forDarkBackground) {
                typeName = typeName.replace("_UNIT", "_DARK_UNIT");
            }

            return typeName;
        }
	}

}

