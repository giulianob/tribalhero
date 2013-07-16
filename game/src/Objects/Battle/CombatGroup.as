package src.Objects.Battle 
{
    import src.Util.BinaryList.*;

    public class CombatGroup extends BinaryList
	{
		public var troopId:int;
		public var owner:BattleOwner;
		public var id: int;
		
		public function CombatGroup(id: int, troopId: int, owner: BattleOwner) 
		{
			super(CombatObject.sortOnId, CombatObject.compareObjId);
			this.troopId = troopId;
			this.owner = owner;
			this.id = id;
		}
				
		public static function sortOnId(a:CombatGroup, b:CombatGroup):Number 
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
		
		public static function compareObjId(a: CombatGroup, value: int):int
		{
			return a.id - value;
		}			
	}

}