package src.UI.Components 
{
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.AssetIcon;
	import org.aswing.AsWingConstants;
	import org.aswing.Component;
	import org.aswing.JLabel;
	import src.Global;
    import src.Util.DateUtil;
    import src.Util.Util;
		
	public class CountDownLabel extends JLabel
	{
		private var time: int;
		private var timer: Timer;
		private var negativeText: String;
		
		public function CountDownLabel(time: int, negativeText: String = "--:--:--")
		{
			this.time = time;
			this.negativeText = negativeText;
			
			setHorizontalAlignment(AsWingConstants.RIGHT);
			setIcon(new AssetIcon(new ICON_CLOCK()));						
			
			timer = new Timer(1000);
			timer.addEventListener(TimerEvent.TIMER, function(e: Event = null): void {
				update();
			});			
			
			addEventListener(Event.REMOVED_FROM_STAGE, function(e: Event = null): void {
				timer.stop();
			});
			
			addEventListener(Event.ADDED_TO_STAGE, function(e: Event = null): void {
				update();
				timer.start();
			});						
		}
		
		public function setTime(time: int): void 
		{
			this.time = time;						
			update();
			
			if (stage) {
				timer.start();
			}
		}
		
		public function update(): void {
			var timeLeft: int = Math.max(0, time - Global.map.getServerTime());
			if (timeLeft <= 0) {
				setText(this.negativeText);
				timer.stop();
			}
			else
				setText(DateUtil.formatTime(timeLeft));
		}
		
	}

}