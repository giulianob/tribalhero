package src.UI.Dialog
{
	import flash.events.Event;
	import flash.events.TimerEvent;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.InteractiveEvent;
	import org.aswing.event.PopupEvent;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.Formula;
	import src.Objects.Resources;
	import src.Objects.SimpleGameObject;
	import src.Objects.StructureObject;
	import src.UI.Components.TimePicker.TimeAdjuster;
	import src.UI.GameJPanel;

	public class AssignmentCreateDialog extends GameJPanel
	{
		private var troopTravelTime: int;
		
		private var chooserArrivalTime: TimeAdjuster;
		
		private var onAccept: Function;
		private var btnOk: JButton;

		public function AssignmentCreateDialog(troopTravelTime: int, onAccept: Function):void
		{
			this.onAccept = onAccept;
			this.troopTravelTime = troopTravelTime;
			
			createUI();

			title = "Create Assignment";		
			
			chooserArrivalTime.setMinimum(Math.max(1, troopTravelTime));
			chooserArrivalTime.setMaximum(162000);
			chooserArrivalTime.setValue(Math.floor(troopTravelTime / 60) * 60);
			
			var self: AssignmentCreateDialog = this;
			btnOk.addActionListener(function(e: Event = null): void {
				if (onAccept != null)
					onAccept(self);
			});
		}
		
		public function getTime(): int
		{
			return chooserArrivalTime.getValue();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);

			Global.gameContainer.showFrame(frame);

			return frame;
		}

		public function createUI(): void {		
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));

			var lblDescription: MultilineLabel = new MultilineLabel("Specify in how long you would like the troops in this assignment to begin attacking.", 3);			
			lblDescription.setPreferredWidth(300);			
			
			chooserArrivalTime = new TimeAdjuster();
			chooserArrivalTime.setColumns(10);

			btnOk = new JButton();
			btnOk.setText("Create Assignment");

			//component layoution						
			append(lblDescription);
			append(AsWingUtils.createPaneToHold(chooserArrivalTime, new FlowLayout(AsWingConstants.CENTER)));
			append(AsWingUtils.createPaneToHold(btnOk, new FlowLayout(AsWingConstants.CENTER)));
		}
	}
}