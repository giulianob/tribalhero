package src.Objects.Battle 
{
	public class BattleLocation 
	{		
		public var name:String;
		public var id:int;
		public var type:int;
		
		public function BattleLocation(type: int, id: int, name: String) 
		{
			this.name = name;
			this.id = id;
			this.type = type;
			
		}
		
	}

}