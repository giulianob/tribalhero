package src.FeathersUI.MiniMap
{
    import src.Util.BinaryList.*;

    public class MiniMapRegionList extends BinaryList
	{			
		public function MiniMapRegionList()
		{
			super(MiniMapRegion.sortOnId, MiniMapRegion.compare);
		}	
		
	}
	
}