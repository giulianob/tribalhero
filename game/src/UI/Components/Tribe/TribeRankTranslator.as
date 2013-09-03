package src.UI.Components.Tribe 
{
	import src.Util.StringHelper;
	import org.aswing.table.PropertyTranslator;
	
	public class TribeRankTranslator implements PropertyTranslator
	{
		private var ranks : * ;
		public function TribeRankTranslator(ranks : *) 
		{
			this.ranks = ranks;
		}
		
		public function translate(info:*, key:String):*
		{
			var rank:int = info[key];
			return ranks[rank].name;
		}
	}		

}