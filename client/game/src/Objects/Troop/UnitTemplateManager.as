package src.Objects.Troop {

    import src.Util.BinaryList.*;

    /**
	 * ...
	 * @author Default
	 */
	public class UnitTemplateManager extends BinaryList {

		public function UnitTemplateManager() {
			super(UnitTemplate.sortOnType, UnitTemplate.compareUnitType);
		}

		override public function get(val: *): *
		{
			var template: UnitTemplate = super.get(val);

			if (template == null)
			template = new UnitTemplate((val as int), 1);

			return template;
		}
	}

}
