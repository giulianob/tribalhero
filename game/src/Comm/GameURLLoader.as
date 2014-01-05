package src.Comm 
{
    import com.adobe.serialization.json.*;

    import flash.events.Event;
    import flash.events.IEventDispatcher;
    import flash.net.*;
	import org.aswing.JPanel;

    import src.Constants;
    import src.UI.Dialog.InfoDialog;
    import src.Util.Util;

    public class GameURLLoader implements IEventDispatcher
	{
		private var loader: URLLoader = new URLLoader();
		private var pnlLoading: InfoDialog;
		
		public function GameURLLoader() 
		{
			addEventListener(Event.COMPLETE, onComplete);
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

		public function load(path: String, params: Array, includeLoginInfo: Boolean = true, showLoadingMessage: Boolean = true) : void {			
			var request: URLRequest = new URLRequest("http://" + Constants.hostname + path);			
			var variables :URLVariables = new URLVariables();
			
			request.data = variables;

            for each (var obj: * in params) {
                if (obj.value is Array) {
                    // URLVariables understands that an array value means it needs to repeat the parameter
                    // but we need to add the [] for cake
                    variables[obj.key + "[]"] = obj.value;
                }
                else {
                    variables[obj.key] = obj.value;
                }
            }

			if (includeLoginInfo) {
                variables.sessionId = Constants.sessionId;
                variables.playerId = Constants.playerId;
			}
			
			request.method = URLRequestMethod.POST;
			
			if (showLoadingMessage && !pnlLoading) {
				pnlLoading = InfoDialog.showMessageDialog("Tribal Hero", "Loading...", null, null, true, false, 0);			
            }
			
			try {
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