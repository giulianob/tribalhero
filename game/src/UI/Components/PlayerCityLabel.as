package src.UI.Components 
{
    import mx.utils.StringUtil;

    import src.Global;
    import src.Map.Username;

    public class PlayerCityLabel extends RichLabel
	{		
		private var playerId:int;
		private var cityId:int;
		private var playerName:String;
		private var cityName:String;
		
		public function PlayerCityLabel(playerId: int, cityId: int, playerName: String = "", cityName: String = "") 
		{
			super("", 1);
			this.cityName = cityName;
			this.playerName = playerName;
			this.cityId = cityId;
			this.playerId = playerId;			
			
			updateText();
			
			if (playerName == "") {
				Global.map.usernames.players.getUsername(playerId, onPlayerUsername);
			}
			
			if (cityName == "") {
				Global.map.usernames.cities.getUsername(playerId, onCityUsername);
			}			
		}
		
		private function onPlayerUsername(u: Username):void 
		{
			playerName = u.name;
			updateText();
		}
		
		private function onCityUsername(u: Username):void 
		{
			cityName = u.name;
			updateText();
		}
	
		private function updateText(): void
		{
			var text: String = StringUtil.substitute("<a href='event:viewProfile:{0}'>{1}</a> (<a href='event:goToCity:{2}'>{3}</a>)", playerId, playerName, cityId, cityName);
			setHtmlText(text);
			setColumns(playerName.length + cityName.length + 5);
		}
	}

}