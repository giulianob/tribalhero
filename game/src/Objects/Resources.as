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
		
		public function multiplyByUnit(count: Number) : Resources {
			return new Resources(crop * count, gold * count, iron * count, wood * count, labor * count);
		}
		
		public static function sum(r1: Resources, r2: Resources) : Resources {
			return new Resources(r1.crop + r2.crop, r1.gold + r2.gold, r1.iron + r2.iron, r1.wood + r2.wood, r1.labor + r2.labor);
		}
		
		public function toString():String 
		{
			return "Gold: " + gold.toString() + " Wood:" + wood.toString() + " Iron:" + iron.toString() + " Crop:" + crop.toString() + " Labor:" + labor.toString();
		}
		
		public function total():int 
		{
			return crop + wood + iron + gold + labor;
		}
		
		public function toNiceString() : String {
            var parts: Array = new Array();
            if (wood > 0) parts.push(wood + " wood");
            if (crop > 0) parts.push(crop + " crop");
            if (iron > 0) parts.push(iron + " iron");
            if (gold > 0) parts.push(gold + " gold");
            if (labor > 0) parts.push(labor + " labor");

            return parts.join(", ");
        }		
	}

}