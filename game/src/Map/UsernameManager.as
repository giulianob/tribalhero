package src.Map 
{
	
	/**
	* ...
	* @author DefaultUser (Tools -> Custom Arguments...)
	*/
	public class UsernameManager 
	{				
		private var map: Map;
		public var cities: UsernameList;
		public var players: UsernameList;
		
		public function UsernameManager(map: Map) 
		{
			this.map = map;
			cities = new UsernameList(map.mapComm.Object.getCityUsername);
			players = new UsernameList(map.mapComm.Object.getPlayerUsername);
		}		
	}	
}