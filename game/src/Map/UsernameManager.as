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
		public var tribes: UsernameList;
		
		public function UsernameManager() 
		{			
			cities = new UsernameList(Global.mapComm.Object.getCityUsername);
			players = new UsernameList(Global.mapComm.Object.getPlayerUsername);
			tribes = new UsernameList(Global.mapComm.Object.getTribeUsername);
		}		
	}	
}