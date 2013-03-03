package src.UI.Components.Tribe 
{
	import src.Util.StringHelper;
	import org.aswing.table.PropertyTranslator;
	
	public class TribeRankTranslator implements PropertyTranslator
	{
		
		public function TribeRankTranslator() 
		{
			
		}
		
		public function translate(info:*, key:String):*
		{
			var rank:int = info[key];
			return StringHelper.localize("TRIBE_RANK_" + rank);
		}
	}		

}