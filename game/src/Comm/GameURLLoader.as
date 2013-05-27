package src.Comm 
{
	import com.adobe.serialization.json.*;
	import flash.events.Event;
	import flash.events.IEventDispatcher;
	import flash.net.*;
import flash.system.Capabilities;

import org.aswing.JPanel;
	import src.Constants;	
	import src.UI.Dialog.InfoDialog;
	import src.Util.Util;
	
	public class GameURLLoader implements IEventDispatcher
	{
		private var loader: URLLoader = new URLLoader();
		public var lastURL: String;
		private var pnlLoading: InfoDialog;
		
		public function GameURLLoader() 
		{
			addEventListener(Event.COMPLETE, onComplete);
		}
		
		public function getData() : String {
			return loader.data;
		}
		
		public function getDataAsXML() : XML {			
			return XML(loader.data);
		}
		
		public function getDataAsObject() : Object {
			try {
				return new JSONDecoder(loader.data).getValue();
			}
			catch (e: Error) {				
                Util.log("Unable to convert data to object");
                Util.log(loader.data.toString());
				
				throw e;
			}
			
			return null;
		}
		
		private function addParameter(request: Object, key: String, value: *): Object {
			if (value is Array) {
				for (var i: int = 0; i < (value as Array).length; i++) {
					request += escape(key) + "[]=" + escape(value[i]) + "&";
				}
			}
			else {
				request += escape(key) + "=" + escape(value) + "&";
			}
			
			return request;
		}
		
		public function load(path: String, params: Array, includeLoginInfo: Boolean = true, showLoadingMessage: Boolean = true) : void {			
			var request: URLRequest = new URLRequest("http://" + Constants.hostname + path);			
			var variables :URLVariables = new URLVariables();
			
			request.data = "";
			
			if (includeLoginInfo) {
				request.data = addParameter(request.data, "sessionId", Constants.sessionId);
				request.data = addParameter(request.data, "playerId", Constants.playerId);
			}

			for each (var obj: * in params) {
				request.data = addParameter(request.data, obj.key, obj.value);
			}
			
			request.method = URLRequestMethod.POST;
			
			if (showLoadingMessage && !pnlLoading) {
				pnlLoading = InfoDialog.showMessageDialog("Tribal Hero", "Loading...", null, null, true, false, 0);			
            }
			
			try {
				lastURL = request.url + "?" + request.data;
				
				if (Capabilities.isDebugger) {
					Util.log("Loading url: " + lastURL);
				}
				
				loader.load(request);
			}
			catch (e: Error) {
                Util.log("URLLoader error");
                Util.log(e.message);
                
				loader.dispatchEvent(new Event(Event.COMPLETE));
			}
		}
		
		private function onComplete(e: Event = null): void {
			if (pnlLoading) {
				pnlLoading.getFrame().dispose();				
				pnlLoading = null;
			}
		}
		
		public function addEventListener(type:String, listener:Function, useCapture:Boolean = false, priority:int = 0, useWeakReference:Boolean = false):void{
			loader.addEventListener(type, listener, useCapture, priority);
		}
		
		public function dispatchEvent(evt: Event):Boolean{
			return loader.dispatchEvent(evt);
		}
    
		public function hasEventListener(type:String):Boolean{
			return loader.hasEventListener(type);
		}
		
		public function removeEventListener(type:String, listener:Function, useCapture:Boolean = false):void{
			loader.removeEventListener(type, listener, useCapture);
		}
		
		public function willTrigger(type:String):Boolean {
			return loader.willTrigger(type);
		}	
		
	}

}