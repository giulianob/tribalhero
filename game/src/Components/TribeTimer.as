package src.Components
{
	import flash.display.DisplayObject;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import src.Global;
	import src.Util.Util;
	/*
	 * Handles setting the message icon to gray and updating message unread count
	 * 
	 * @author Giuliano Barberi
	 */
	public class TribeTimer
	{
		private var timer: Timer = new Timer(60000);

		public function TribeTimer()
		{
			timer.addEventListener(TimerEvent.TIMER, onTimer);
		}

		public function start() : void {
			timer.start();

			// Call initial unread
			onTimer();
		}

		public function stop() : void {
			timer.stop();
			Global.gameContainer.txtIncoming.visible = false;
		}
			
		private function onTimer(e: TimerEvent = null) : void {
			Global.mapComm.Tribe.incomingCount(onReceiveIncomingCount);
		}

		private function onReceiveIncomingCount(count:int) : void {
			Global.gameContainer.txtIncoming.visible = count > 0;
			if (count > 0) Global.gameContainer.txtIncoming.txtUnreadCount.text = count > 9? "!":count.toString();
		}
	}

}

