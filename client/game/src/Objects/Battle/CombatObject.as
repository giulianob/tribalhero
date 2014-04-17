package src.Objects.Battle {
    import flash.display.DisplayObjectContainer;

    import src.Objects.Factories.ObjectFactory;

    public class CombatObject {
		public var combatObjectId: int;
		public var type: int;
		public var level: int;
		public var hp: Number;
		public var maxHp: Number;
		
		public function CombatObject(combatObjectId: int, type: int, level: int, hp: Number, maxHp: Number)
		{
			this.combatObjectId = combatObjectId;
            this.type = type;
			this.level = level;
			this.hp = hp;
			this.maxHp = maxHp;
		}
		
		public function get name(): String {
			return "Unimplemented";
		}
		
		public function getIcon(): DisplayObjectContainer {
			throw new Error("Unimplemented method in inheritor");
		}		
		
		public static function sortOnId(a:CombatObject, b:CombatObject):Number 
		{
			var aId:Number = a.combatObjectId;
			var bId:Number = b.combatObjectId;

			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}
		
		public static function compareObjId(a: CombatObject, value: int):int
		{
			return a.combatObjectId - value;
		}	

	}
	
}