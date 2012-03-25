package src.UI.Components 
{
	import org.aswing.AsWingConstants;
	import org.aswing.border.EmptyBorder;
	import org.aswing.Container;
	import org.aswing.FlowLayout;
	import org.aswing.Insets;
	import org.aswing.JLabel;
	
	public class PlayerCityLabel extends Container
	{
		
		public function PlayerCityLabel(playerId: int, cityId: int, playerName: String = null, cityName: String = null) 
		{
			setLayout(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));	
			
			appendAll(new PlayerLabel(playerId, playerName), getLabel("("), new CityLabel(cityId, cityName), getLabel(")"));
		}
	
		private function getLabel(text: String): JLabel
		{
			var lbl: JLabel = new JLabel(text, null, AsWingConstants.LEFT);
			lbl.setBorder(new EmptyBorder(null, new Insets()));
			lbl.mouseEnabled = false;
			
			return lbl;
		}
	}

}