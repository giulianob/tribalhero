package src.UI.Components.TimePicker 
{
    import org.aswing.JAdjuster;

    /**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class TimeAdjuster extends JAdjuster
	{
		
		public function TimeAdjuster() 
		{
			super();
			
			var ui: TimeAdjusterUI = new TimeAdjusterUI();			
			setUI(ui);			
		}
		
	}

}