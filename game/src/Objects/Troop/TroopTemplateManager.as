package src.Objects.Troop {

    import src.Util.BinaryList.*;

    /**
	 * ...
	 * @author Default
	 */
	public class TroopTemplateManager extends BinaryList {

		public function TroopTemplateManager() {
			super(TroopTemplate.sortOnType, TroopTemplate.compareUnitType);
		}

	}

}
