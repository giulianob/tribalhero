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
		public var strongholds: UsernameList;
		
		public function UsernameManager() 
		{			
			cities = new UsernameList(Global.mapComm.Objects.getCityUsername);
			players = new UsernameList(Global.mapComm.Objects.getPlayerUsername);
			tribes = new UsernameList(Global.mapComm.Objects.getTribeUsername);
			strongholds = new UsernameList(Global.mapComm.Objects.getStrongholdUsername);
		}		
	}	
}