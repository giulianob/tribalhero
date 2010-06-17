package src.Objects.Troop {
	import src.Map.City;
	import src.Util.BinaryList.*;

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

		public function getIndividualUnitCount(type: int = -1, minLevel: int = 1): int {

			if (type > -1) {
				var unitTemplate: UnitTemplate = city.template.get(type);
				if (!unitTemplate || unitTemplate.level < minLevel) return 0;
			}

			var total: int = 0;
			for each(var stub: TroopStub in each()) {
				if (stub.cityId != city.id) continue;

				total += stub.getIndividualUnitCount(type);
			}
			
			return total;
		}
	}
}
