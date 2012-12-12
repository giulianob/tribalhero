package src.UI.Components.Stronghold 
{
	import fl.lang.Locale;
	import org.aswing.table.PropertyTranslator;
	import src.Global;
	import src.Util.Util;
	
	public class DaysOccupiedRankTranslator implements PropertyTranslator
	{
		
		public function DaysOccupiedRankTranslator() 
		{
			
		}
		
		public function translate(info:*, key:String):*
		{
			var occupiedSince:int = info[key];
			if (occupiedSince == 0) return "";
			var timediff :int = Global.map.getServerTime() - occupiedSince;
			return Util.niceDays(timediff);
		}
	}		

}