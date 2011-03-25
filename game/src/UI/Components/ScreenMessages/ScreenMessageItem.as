package src.UI.Components.ScreenMessages
{
	import org.aswing.AsWingConstants;
	import org.aswing.Icon;
	import org.aswing.JLabel;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class ScreenMessageItem extends JLabel
	{
		public var key: String;
		public var duration: int;

		public function ScreenMessageItem(key: String, message: String, icon: Icon = null, duration: int = 0)
		{
			mouseEnabled = false;
			mouseChildren = false;
			
			setText(message);
			if (icon != null) setIcon(icon);
			
			setHorizontalAlignment(AsWingConstants.LEFT);			

			this.key = key;
			this.duration = duration;
		}

	}

}

