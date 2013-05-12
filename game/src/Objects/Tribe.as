package src.Objects 
{
	import src.Constants;
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class Tribe 
	{
		public static var NONE: int = 0;
		public static var ALL : int = 1;
		public static var INVITE: int = 2;
		public static var KICK: int = 4;
		public static var SET_RANK: int = 8;	
		public static var REPAIR: int = 16;	
		public static var UPGRADE: int = 32;	
		public static var ASSIGNMENT: int = 64;
		public static var DELETE_POST: int = 128;
		public static var ANNOUNCEMENT: int = 256;;
		
		public var id: int = 0;
		public var rank: int = 0;
		public var ranks: * = null;

		public function hasRight(right: int) :Boolean
		{
			if (id == 0 || ranks == null) return false;
			if ((ranks[rank].rights & ALL) == ALL) return true;
            
			return (ranks[rank].rights & right) == right;
		}
		
		public function isInTribe(id: int = -1): Boolean
		{
            // If id is 0 then just return true if the user is in a tribe at all
			if (id == -1) return this.id != 0;
			
			if (this.id <= 0) return false;

			return this.id == id;
		}
	}
}