package src.UI.Components
{
	import flash.display.DisplayObjectContainer;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.JPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Objects.Actions.Notification;
	import src.Objects.Actions.PassiveAction;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Util.Util;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class NotificationBox extends JPanel
	{

		private var pnlText:JPanel;
		private var lblAction:JLabel;
		private var lblTime:JLabel;

		private var notification: Notification;
		private var tooltipMode: Boolean;

		private var timer: Timer = new Timer(1000, 0);

		public function NotificationBox(notification: Notification, tooltipMode: Boolean = false)
		{
			this.notification = notification;
			this.tooltipMode = tooltipMode;

			createUI();

			if (tooltipMode) {
				GameLookAndFeel.changeClass(lblAction, "Tooltip.text");
				GameLookAndFeel.changeClass(lblTime, "Tooltip.text");
			}

			var actionDescription: String = PassiveAction.toString(notification.type);

			lblAction.setText(actionDescription);
			lblAction.setToolTipText(lblAction.getText());
			updateTime();

			timer.addEventListener(TimerEvent.TIMER, updateTime);

			addEventListener(Event.ADDED_TO_STAGE, function(e: Event) : void {
				timer.start();
			});

			addEventListener(Event.REMOVED_FROM_STAGE, function(e: Event) : void {
				timer.stop();
			});

			pack();
		}

		public function updateTime(e: Event = null) : void {
			var time: Number = Math.max(0, notification.endTime - Global.map.getServerTime());

			lblTime.setText(Util.formatTime(time));
		}

		private function createUI() : void
		{
			mouseEnabled = false;
			mouseChildren = false;

			setLayout(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));

			pnlText = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));

			lblAction = new JLabel();
			lblAction.setHorizontalAlignment(AsWingConstants.LEFT);

			lblTime = new JLabel();
			lblTime.setHorizontalAlignment(AsWingConstants.LEFT);

			//component layout
			pnlText.append(lblAction);
			pnlText.append(lblTime);

			append(pnlText);
		}
	}

}

