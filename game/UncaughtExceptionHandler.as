package 
{
    import flash.display.LoaderInfo;
    import flash.events.*;
    import flash.external.ExternalInterface;
    import flash.net.*;
    import flash.system.Capabilities;

    import src.*;
    import src.Util.Util;

    /**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class UncaughtExceptionHandler
	{
		public function UncaughtExceptionHandler(loaderInfo: LoaderInfo)
		{						           
			if (loaderInfo.hasOwnProperty("uncaughtErrorEvents")) {
				loaderInfo.uncaughtErrorEvents.addEventListener(UncaughtErrorEvent.UNCAUGHT_ERROR, uncaughtErrorHandler);
			}
		}
		
		private static var stack: Array = [];
		
		private static var lastSubmission: Number = 0;

        //noinspection JSUnusedGlobalSymbols
        public static function enterFunction(functionId: int) : void {
			stack.push(functionId);
		}

        //noinspection JSUnusedGlobalSymbols
		public static function exitFunction(functionId: int) : void {
			stack.pop();
		}
		
		public static function getStacktrace() : String {
			return stack.join(",");
		}
		
		private function uncaughtErrorHandler(event:*):void
		{			
			var error: Error = event.error;
			
			Util.log("Got error");
			Util.log(error.getStackTrace());
			
			// Only send error if we are in web mode
			if (Constants.loginKey == "") return;
			
			var url:String = Constants.mainWebsite + "stacktraces/game_submit";
			var request:URLRequest = new URLRequest(url);
			var requestVars:URLVariables = new URLVariables();
			
			requestVars.stacktrace = error.message + "\n" + getStacktrace();
			requestVars.playerId = Constants.playerId;
			requestVars.playerName = Constants.playerName;
			requestVars.flashVersion = Capabilities.version;
			requestVars.gameVersion = Constants.version.toString() + "." + Constants.revision.toString();
			
			// Clear stacktrace since unhandled exceptions kill current execution
			stack = [];
			
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

			for (var prop:String in requestVars) {
                if (!requestVars.hasOwnProperty(prop)) {
                    continue;
                }

				Util.log(prop + " = " + requestVars[prop]);
            }
			
			var now: Number = new Date().time;
			
			try {
				if (now - lastSubmission > 60000)
					urlLoader.load(request);
			} catch (e:Error) {
			}
			
			lastSubmission = now;
		}
	}
}