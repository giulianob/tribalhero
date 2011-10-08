package src.UI.Components.Tribe 
{
	import fl.lang.Locale;
	import org.aswing.table.PropertyTranslator;
	
	public class TribeRankTranslator implements PropertyTranslator
	{
		
		public function TribeRankTranslator() 
		{
			
		}
		
		public function translate(info:*, key:String):*
		{
			var rank:int = info[key];
			return Locale.loadString("TRIBE_RANK_" + rank);
		}
	}		

}