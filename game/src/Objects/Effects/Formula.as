package src.Objects.Effects {
	
	/**
	* ...
	* @author Default
	*/
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Objects.GameObject;
	import src.Objects.Resources;
	import src.Objects.StructureObject;
	import src.Objects.TechnologyManager;
	
	public class Formula {
		
		public static const RESOURCE_CHUNK: int = 100;
		public static const RESOURCE_MAX_TRADE: int = 1500;
		
		public static function trainTime(parentObj: GameObject, baseValue: int, techManager: TechnologyManager): int
		{
			var discount: Array = [ 0, 0, 0, 0, 0, 5, 10, 15, 20, 30, 40 ];
			return (baseValue * Constants.secondsPerUnit) * (100 - discount[parentObj.level]) / 100;
		}
		
		public static function buildTime(baseValue: int, techManager:TechnologyManager): int
		{
			return (baseValue * Constants.secondsPerUnit);
		}				
		
		public static function moveTime(speed: int) : int {
			return 20 - speed;
		}
		
		public static function marketBuyCost(price: int, amount: int, tax: Number): int
		{
			return Math.round(((amount / RESOURCE_CHUNK) * price) * (1.0 + tax));
		}

		public static function marketSellCost(price: int, amount: int, tax: Number): int
		{
			return Math.round(((amount / RESOURCE_CHUNK) * price) * (1.0 - tax));
		}
		
		public static function marketTax(structure: StructureObject): Number
		{
			if (structure.level == 1)
				return 0.25;
			else if (structure.level == 2)
				return 0.20;
			else if (structure.level == 3)
				return 0.15;
			else if (structure.level == 4)
				return 0.10;			
			else 
				return 0.05;
		}		
		
		public static function maxForestLabor(level: int) : int {
			return level * 240;
		}
	}
}
