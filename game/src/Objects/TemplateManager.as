package src.Objects {

	import src.Util.BinaryList;
	import src.Util.Util;

	/**
	* ...
	* @author Default
	*/
	public class TemplateManager extends BinaryList {
			
		public function TemplateManager() {
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