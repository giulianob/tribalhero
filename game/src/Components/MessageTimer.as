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

		public function MessageTimer()
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
			loader.addEventListener(Event.COMPLETE, onReceiveUnread);
		}

		public function start() : void {
			timer.start();

			// Call initial unread
			onTimer();
		}

		public function stop() : void {
			timer.stop();
		}
		
		public function check() : void {
			timer.stop();
			onTimer();
			timer.start();
		}
		
		private function onTimer(e: TimerEvent = null) : void {
			try {
				loader.load("/actions/unread", [ ], true, false);
			}
			catch (e: Error) { }
		}

		private function onReceiveUnread(e: Event = null) : void {
			try {
				var data: Object = loader.getDataAsObject();
				
				var unreadMessages: int = data.unreadMessages;								
				Global.gameContainer.txtUnreadMessages.visible = unreadMessages > 0;				
				if (unreadMessages > 0) Global.gameContainer.txtUnreadMessages.txtCount.text = unreadMessages > 9 ? "!" : unreadMessages.toString();				
				
				var unreadReports: int = data.unreadReports;
				Global.gameContainer.txtUnreadReports.visible = unreadReports > 0;				
				if (unreadReports > 0) Global.gameContainer.txtUnreadReports.txtCount.text = unreadReports > 9 ? "!" : unreadReports.toString();								
			}
			catch (e: Error) { }
		}
	}

}

