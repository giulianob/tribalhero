package src.Comm {
	import flash.events.Event;
	public interface Session {	
		function login(useLoginKey: Boolean, username:String, passwdOrLoginKey:String): void;
		
		function connect(hostname: String): void;
		
		function logout(): void;
		
		function write(packet:Packet, callback: Function = null, custom: * = null): Boolean;
		
		function setReceive(callback: Function): void;
		
		function setConnect(callback: Function): void;
		
		function setDisconnect(callback: Function): void;
		
		function setLogin(callback: Function): void;
		
		function addEventListener(type:String, listener:Function, useCapture:Boolean = false, priority:int = 0, useWeakReference:Boolean = false): void;
		
		function dispatchEvent(evt:Event): Boolean;
		
		function hasEventListener(type:String): Boolean;
		
		function removeEventListener(type:String, listener:Function, useCapture:Boolean = false): void;
		
		function willTrigger(type:String): Boolean;
		
		function hasLoginSuccess() : Boolean;
		
		function setLoginSuccess(bool: Boolean): void;
	}
}