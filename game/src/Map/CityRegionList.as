package src.Map 
{
	import src.Util.BinaryList;
	
	/**
	* ...
	* @author Giuliano Barberi

	*/
	public class CityRegionList extends BinaryList
	{
		private var map: Map;
		
		public function CityRegionList(map: Map) 
		{
			super(CityRegion.sortOnId, CityRegion.compare);
			this.map = map;
		}	
		
	}
	
}