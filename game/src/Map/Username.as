package src.Map {
	
	/**
	* ...
	* @author Default
	*/
	public class Username {
		
		public var name: String = null;
		public var id: int;
		
		public function Username(id: int, name: String) {		
			this.name = name;
			this.id = id;
		}
		
		public static function sortOnId(a:Username, b:Username):Number 
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
		
		public static function compare(a: Username, value: int):int
		{
			return a.id - value;
		}	
	}
	
}