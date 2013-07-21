package src.Map 
{
    import src.Util.BinaryList.*;

    /**
	* ...
	* @author Giuliano Barberi

	*/
	public class CityRegionList extends BinaryList
	{			
		public function CityRegionList() 
		{
			super(CityRegion.sortOnId, CityRegion.compare);			
		}	
		
	}
	
}