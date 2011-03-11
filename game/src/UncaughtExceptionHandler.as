package src 
{
	import flash.display.LoaderInfo;
	import flash.display.Stage;
	import flash.external.ExternalInterface;
	import flash.net.*;
	import flash.system.Capabilities;
	import src.Util.Util;
	import flash.events.*;
	
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class UncaughtExceptionHandler
	{
		public function UncaughtExceptionHandler(loaderInfo: LoaderInfo)
		{						
			if (loaderInfo.hasOwnProperty("uncaughtErrorEvents")) {
				trace("Watching for errors");
				IEventDispatcher(loaderInfo["uncaughtErrorEvents"]).addEventListener("uncaughtError", uncaughtErrorHandler);
			}
		}
		
		private function uncaughtErrorHandler(event:*):void
		{			
			var error: Error = event.error;
			
			Util.log("Got error");
			Util.log(error.getStackTrace());			
			
			// Only send error if we are in web mode
			if (Constants.loginKey == "") return;
			
			var url:String = "http://" + Constants.hostname + "/stacktraces/game_submit";
			var request:URLRequest = new URLRequest(url);
			var requestVars:URLVariables = new URLVariables();
			var a: UncaughtErrorEvent;
			
			requestVars.stacktrace = error.message + "\n" + error.getStackTrace();
			requestVars.playerId = Constants.playerId;
			requestVars.playerName = Constants.playerName;
			requestVars.flashVersion = Capabilities.version;
			requestVars.gameVersion = Constants.version.toString() + "." + Constants.revision.toString();
			
			try {
				requestVars.browserVersion = ExternalInterface.call("function(){return navigator.appVersion+'-'+navigator.appName;}");
			} catch (e: Error) {
				requestVars.browserVersion = "N/A";
			}		
			
			request.data = requestVars;
			request.method = URLRequestMethod.POST;
			
			var urlLoader:URLLoader = new URLLoader();			
			urlLoader.dataFormat = URLLoaderDataFormat.TEXT;
			
			urlLoader.addEventListener(Event.COMPLETE, function(ev: Event) : void {
				Util.log(ev.target.data);
			});

			for (var prop:String in requestVars)
				Util.log(prop + " = " + requestVars[prop]);			
			
			try {
				urlLoader.load(request);
			} catch (e:Error) {				
			}					
		}		
	}
}