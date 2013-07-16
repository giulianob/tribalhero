package src.UI.Dialog
{
    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.ext.*;

    import src.Global;
    import src.UI.Components.TimePicker.TimeAdjuster;
    import src.UI.GameJPanel;

    public class AssignmentCreateDialog extends GameJPanel
	{
		private var troopTravelTime: int;
		
		private var chooserArrivalTime: TimeAdjuster;
		
		private var onAccept: Function;
		private var btnOk: JButton;
		private var txtDescription: JTextArea;
		
		public function AssignmentCreateDialog(troopTravelTime: int, onAccept: Function):void
		{
			this.onAccept = onAccept;
			this.troopTravelTime = troopTravelTime;
			
			createUI();

			title = "Create Assignment";
			
			chooserArrivalTime.setMinimum(troopTravelTime + 900);
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

		public function getDescription() : String
		{
			return txtDescription.getText();
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);

			Global.gameContainer.showFrame(frame);

			return frame;
		}

		public function createUI(): void {		
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));

			var lblDescription: MultilineLabel = new MultilineLabel("Specify in how long you would like the troops to reach the target.\nAll troops will be dispatched automatically in order to reach the target at the selected time. If you make the assignment arrive too early then other members may arrive late to the battle.", 5);
			lblDescription.setPreferredWidth(300);	
			
			chooserArrivalTime = new TimeAdjuster();
			chooserArrivalTime.setColumns(10);

			btnOk = new JButton();
			btnOk.setText("Create Assignment");

			txtDescription = new JTextArea("",5,40);
			txtDescription.setWordWrap(true);
			txtDescription.setMaxChars(250);
			
			//component layoution						
			append(lblDescription);
			append(AsWingUtils.createPaneToHold(chooserArrivalTime, new FlowLayout(AsWingConstants.LEFT)));
			append(AsWingUtils.createPaneToHold(new JLabel("Enter a description:"), new FlowLayout(AsWingConstants.LEFT)));
			append(AsWingUtils.createPaneToHold(txtDescription, new FlowLayout(AsWingConstants.LEFT)));
			append(AsWingUtils.createPaneToHold(btnOk, new FlowLayout(AsWingConstants.CENTER)));
		}
	}
}