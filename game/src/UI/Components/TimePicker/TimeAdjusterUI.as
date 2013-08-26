package src.UI.Components.TimePicker 
{
    import org.aswing.skinbuilder.SkinAdjusterUI;

    /**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class TimeAdjusterUI extends SkinAdjusterUI
	{
		
		public function TimeAdjusterUI() 
		{
			super();
			inputText = new TimeTextField();
			inputText.setFocusable(false);
		}
		
	}

}