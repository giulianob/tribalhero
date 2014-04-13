package src.Objects.Battle 
{
	public class BattleOwner 
	{
		public var name:String;
		public var id:int;
		public var type:int;
		
		public function BattleOwner(type: int, id: int, name: String) 
		{
			this.name = name;
			this.id = id;
			this.type = type;			
		}
		
	}

}