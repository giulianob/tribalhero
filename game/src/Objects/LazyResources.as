/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects {

	public class LazyResources {
				
		public var gold: LazyValue;
		public var wood: LazyValue;
		public var iron: LazyValue;
		public var crop: LazyValue;
		public var labor: LazyValue;
		
		public function LazyResources(crop: LazyValue, iron: LazyValue, gold: LazyValue, wood: LazyValue, labor: LazyValue) {
			this.crop = crop;
			this.gold = gold;
			this.iron = iron;
			this.wood = wood;
			this.labor = labor;
		}
		
		public function toResources(): Resources
		{
			return new Resources(crop.getValue(), gold.getValue(), iron.getValue(), wood.getValue(), labor.getValue());
		}
		
		public function Div(resource: Resources): int
		{
			var cropDelta: int;
			if (resource.crop == 0) 
				cropDelta = 999999;
			else
				cropDelta = int(crop.getValue() / resource.crop);
				
			var goldDelta: int;
			if (resource.gold == 0) 
				goldDelta = 999999;
			else
				goldDelta = int(gold.getValue() / resource.gold);
			
			var ironDelta: int;
			if (resource.iron == 0) 
				ironDelta = 999999;
			else
				ironDelta = int(iron.getValue() / resource.iron);
				
			var woodDelta: int;
			if (resource.wood == 0) 
				woodDelta = 999999;
			else
				woodDelta = int(wood.getValue() / resource.wood);
				
			var laborDelta: int;
			if (resource.labor == 0) 
				laborDelta = 999999;
			else
				laborDelta = int(labor.getValue() / resource.labor);
			
			return Math.min(cropDelta, goldDelta, woodDelta, ironDelta);
		}
		
		public function GreaterThanOrEqual(resource: Resources): Boolean
		{
			if (crop.getValue() < resource.crop) return false;
			if (gold.getValue() < resource.gold) return false;
			if (iron.getValue() < resource.iron) return false;
			if (wood.getValue() < resource.wood) return false;
			if (labor.getValue() < resource.labor) return false;
			
			return true;
		}
		public function toString(): String
		{
            return "Gold " + gold.getValue() + "/" + gold.getRawValue() + "/" + gold.getRate() +
                "Wood " + wood.getValue() + "/" + wood.getRawValue() + "/" + wood.getRate() +
                "Iron " + iron.getValue() + "/" + iron.getRawValue() + "/" + iron.getRate() +
                "Crop " + crop.getValue() + "/" + crop.getRawValue() + "/" + crop.getRate() +
                "Gold " + labor.getValue() + "/" + labor.getRawValue() + "/" + labor.getRate();        
		}
	}
	
}
