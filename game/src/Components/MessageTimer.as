package src.Components
{
	import flash.display.DisplayObject;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import src.Comm.GameURLLoader;
	import src.Global;
	import src.Util.Util;
	/*
	 * Handles setting the message icon to gray and updating message unread count
	 * 
	 * @author Giuliano Barberi
	 */
	public class MessageTimer
	{
		private var timer: Timer = new Timer(60000);
		private var loader: GameURLLoader = new GameURLLoader();
		private var messageButton: DisplayObject;
		private var unread: int = 0;

		public function MessageTimer()
		{
			messageButton = Global.gameContainer.btnMessages;
			timer.addEventListener(TimerEvent.TIMER, onTimer);
			loader.addEventListener(Event.COMPLETE, onReceiveUnread);
		}

		public function start() : void {
			messageButton.addEventListener(MouseEvent.MOUSE_OVER, onMessageMouseOver);
			messageButton.addEventListener(MouseEvent.MOUSE_OUT, onMessageMouseOut);

			timer.start();

			// Call initial unread
			onTimer();
		}

		public function stop() : void {
			messageButton.removeEventListener(MouseEvent.MOUSE_OVER, onMessageMouseOver);
			messageButton.removeEventListener(MouseEvent.MOUSE_OUT, onMessageMouseOut);

			timer.stop();
		}

		private function onMessageMouseOver(e: Event = null) : void {			
		}

		private function onMessageMouseOut(e: Event = null) : void {			
		}

		private function onTimer(e: TimerEvent = null) : void {
			try {
				loader.load("/messages/unread", [ ], true);
			}
			catch (e: Error) { }
		}

		private function onReceiveUnread(e: Event = null) : void {
			try {
				var data: * = loader.getDataAsObject();
				
				unread = data.unread;
				
				Global.gameContainer.txtUnread.visible = unread > 0;
				
				if (unread > 0) {
					Global.gameContainer.txtUnread.txtUnreadCount.text = unread > 9 ? "!" : unread.toString();
				}
			}
			catch (e: Error) { }
		}
	}

}

