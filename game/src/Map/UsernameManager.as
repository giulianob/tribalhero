package src.Map 
{
	import src.Global;
	
	/**
	* ...
	* @author DefaultUser (Tools -> Custom Arguments...)
	*/
	public class UsernameManager 
	{						
		public var cities: UsernameList;
		public var players: UsernameList;
		
		public function UsernameManager() 
		{			
			cities = new UsernameList(Global.mapComm.Object.getCityUsername);
			players = new UsernameList(Global.mapComm.Object.getPlayerUsername);
		}		
	}	
}