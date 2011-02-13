package src.UI.Components
{
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import src.Constants;
	import src.Global;
	import src.UI.Components.SimpleTooltip;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class GoToCityIcon extends AssetIcon
	{
		private var cityId: int;
		private var icon: MovieClip = new ICON_WORLD();

		public function GoToCityIcon(cityId: int)
		{
			super(icon);

			icon.buttonMode = true;
			icon.mouseEnabled = true;
			new SimpleTooltip(icon, "Go to city");
			icon.addEventListener(MouseEvent.CLICK, function(e: MouseEvent) : void {				
				Global.mapComm.City.gotoCityLocation(cityId);
			});
		}

	}

}
