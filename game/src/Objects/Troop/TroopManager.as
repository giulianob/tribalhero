package src.Objects.Troop {
	import src.Map.City;
	import src.Util.BinaryList.*;
	import System.Linq.Enumerable;

	/**
	 * ...
	 * @author Default
	 */
	public class TroopManager extends BinaryList
	{
		private var city: City;

		public function TroopManager(city: City) {
			super(TroopStub.sortOnCityIdAndTroopId, TroopStub.compareCityIdAndTroopId);

			this.city = city;
		}

		public function getDefaultTroop(): TroopStub
		{
			return get([city.id, 1]);
		}
		
		public function getUpkeep(): int
		{
			return Enumerable.from(this)
				      .where(function(p: TroopStub): Boolean { 
							return p.cityId == city.id;
					  })
					  .sum(function(p: TroopStub): int {
							return p.getUpkeep(true);
					  });
		}
		
		public function getUnitTotalsByStatus(): * {
			var onTheMove: int = getDefaultTroop().get(Formation.InBattle).getIndividualUnitCount();
			var idle: int = getDefaultTroop().get(Formation.Normal).getIndividualUnitCount() + getDefaultTroop().get(Formation.Garrison).getIndividualUnitCount();
			
			for each(var stub: TroopStub in this) {
				// Skip other peoples stubs or the default one since its handled above
				if (stub.cityId != city.id || stub.id == 1) continue;
				
				var unitCount: int = stub.getIndividualUnitCount();
				
				switch (stub.state) {
					case TroopStub.IDLE:
					case TroopStub.STATIONED:
						idle += unitCount;
						break;
					default:
						onTheMove += unitCount;
						break;
				}
			}	
			
			return { idle: idle, onTheMove: onTheMove };
		}

		public function getIndividualUnitCount(type: int = -1, minLevel: int = 1): int {

			if (type > -1) {
				var unitTemplate: UnitTemplate = city.template.get(type);
				if (!unitTemplate || unitTemplate.level < minLevel) return 0;
			}

			var total: int = 0;
			for each(var stub: TroopStub in this) {
				if (stub.cityId != city.id) continue;

				total += stub.getIndividualUnitCount(type);
			}
			
			return total;
		}
	}
}
