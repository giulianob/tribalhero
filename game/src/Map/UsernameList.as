package src.Map {
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.events.IEventDispatcher;
	import flash.text.TextField;
	import org.aswing.JLabel;
	import src.Objects.SimpleGameObject;
	import src.Objects.TroopObject;
	import src.Util.BinaryList;
	import src.Util.Util;
	
	
	/**
	* ...
	* @author Default
	*/
	public class UsernameList extends BinaryList {
			 	
		private var commGetUsername: Function;
		
		private var pending: Array = new Array();
		
		public function UsernameList(callback: Function) {
			super(Username.sortOnId, Username.compare);
			this.commGetUsername = callback;
		}
		
		public function getUsername(id: int, callback: Function = null, custom: * = null): Username
		{
			var username: Username = get(id);
			
			if (!username) 
			{
				var pass: Object = new Object();
				pass.callback = callback;
				pass.custom = custom;
				
				var found: Boolean = false;
				for (var i: int = 0; i < pending.length; i++) {
					if (pending[i].id == id) {		
						(pending[i].callbacks as Array).push(pass);
						found = true;
						break;
					}
				}
				
				if (!found) {
					var pendingObj: Object = new Object();
					pendingObj.id = id;
					pendingObj.callbacks = new Array();
					(pendingObj.callbacks as Array).push(pass);
					
					pending.push(pendingObj);
					
					commGetUsername(id, setUsername);
				}
				
				return null;
			}
			else
			{							
				if (callback != null)
					callback(username, custom);
					
				return username;			
			}
		}
		
		private function setUsername(id: int, name: String, custom: *):void
		{
			var username: Username = get(id);
			
			if (!username)
			{
				username = new Username(id, name);
				add(username);
			}
			else			
				username.name = name;
			
			for (var i: int = 0; i < pending.length; i++) {
				if (pending[i].id != id) continue;
				
				for each(var obj: Object in pending[i].callbacks)
					obj.callback(username, obj.custom);
				
				pending.splice(i, 1);
				
				break;
			}						
			
			dispatchEvent(new Event(BinaryList.CHANGED));
		}
		

		//*********************
		// Helper Functions
		
		public function setObjectUsername(id: int, obj: SimpleGameObject):void {
			getUsername(id, onGetObjectUsername, obj);
		}
		
		public function onGetObjectUsername(username: Username, custom: *):void
		{
			(custom as SimpleGameObject).showUsername(username.name);
		}

		public function setTextfieldUsername(id: int, obj: TextField):void {
			getUsername(id, onGetTextfieldUsername, obj);
		}
		
		public function onGetTextfieldUsername(username: Username, custom: *):void
		{
			(custom as TextField).text = username.name;
		}

		public function setLabelUsername(id: int, obj: JLabel):void {
			getUsername(id, onGetLabelUsername, obj);
		}
		
		public function onGetLabelUsername(username: Username, custom: *):void
		{
			(custom as JLabel).setText(username.name);
		}
		
	}
	
}