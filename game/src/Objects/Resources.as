package src.Objects 
{

	public class Resources
	{
		
		public static const TYPE_GOLD: int = 0;
		public static const TYPE_CROP: int = 1;
		public static const TYPE_WOOD: int = 2;
		public static const TYPE_IRON: int = 3;
		
		public var gold: int;
		public var wood: int;
		public var iron: int;
		public var crop: int;
		public var labor: int;
		
		public function Resources(crop: int, gold: int, iron: int, wood:int, labor: int) {
			this.crop = crop;
			this.gold = gold;
			this.iron = iron;
			this.wood = wood;
			this.labor = labor;
		}
		
		public function multiplyByUnit(count: int) : Resources {
			return new Resources(crop * count, gold * count, iron * count, wood * count, labor * count);
		}
		
		public function toString():String 
		{
			return "Gold: " + gold.toString() + " Wood:" + wood.toString() + " Iron:" + iron.toString() + " Crop:" + crop.toString() + " Labor:" + labor.toString();
		}
		
	}

}