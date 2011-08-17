package src.UI.Dialog {
	import flash.display.*
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import org.aswing.AssetPane;
	import org.aswing.AsWingConstants;
	import org.aswing.BorderLayout;
	import org.aswing.FlowLayout;
	import org.aswing.geom.IntDimension;
	import org.aswing.JFrame;
	import org.aswing.JLabel;
	import org.aswing.JLabelButton;
	import org.aswing.JPanel;
	import org.aswing.JScrollPane;
	import org.aswing.JViewport;
	import org.aswing.SoftBoxLayout;
	import src.Global;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Troop.*;
	import src.UI.Components.CityLabel;
	import src.UI.Components.CoordLabel;
	import src.UI.Components.CountDownLabel;
	import src.UI.Components.PlayerCityLabel;
	import src.UI.Components.PlayerLabel;
	import src.UI.Components.SimpleTroopGridList.SimpleTroopGridList;
	import src.UI.GameJPanel;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.UI.Tooltips.SimpleTroopStubTooltip;
	import src.UI.Tooltips.TroopStubTooltip;

	public class AssignmentInfoDialog extends GameJPanel {

		private var assignment: * ;
		private var tooltip: SimpleTroopStubTooltip;

		public function AssignmentInfoDialog(assignment: *):void
		{
			title = "Assignment Information";

			this.assignment = assignment;
			
			createUI();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
		
		private function createRow(...coms): JPanel {
			var holder: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0));
			for each(var i:* in coms) {
				holder.append(i);
			}			
			return holder;
		}
			
		private function createUI(): void {
			setPreferredSize(new IntDimension(350, 500));
			setLayout(new BorderLayout(5));
			
			var pnlHeader: JPanel = new JPanel(new BorderLayout(5, 5));
			pnlHeader.setConstraints("North");
			
			var lblCountdown: CountDownLabel = new CountDownLabel(assignment.endTime);
			lblCountdown.setConstraints("East");
			var pnlDetails: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
			pnlDetails.setConstraints("Center");					
			
			pnlDetails.append(createRow(new JLabel("Organized by"), new PlayerLabel(assignment.troops[0].playerId, assignment.troops[0].playerName)));
			pnlDetails.append(createRow(new JLabel("Attack"), new PlayerCityLabel(assignment.targetPlayerId, assignment.targetCityId, assignment.targetPlayerName, assignment.targetCityName)));
			pnlDetails.append(createRow(new JLabel("Target"), new CoordLabel(assignment.x, assignment.y)));
			
			var lblTroops: JLabel = new JLabel("Troops", null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblTroops, "darkHeader");
			pnlDetails.appendAll(new JLabel(" "), lblTroops);
			
			var pnlTroops: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			for each (var troop:* in assignment.troops) {
				var troopHolder: JPanel = new JPanel(new BorderLayout(5));
				
				var lblPlayerCity: PlayerCityLabel = new PlayerCityLabel(troop.playerId, troop.cityId, troop.playerName, troop.cityName);
				lblPlayerCity.setConstraints("Center");
				
				var lblUnits: JLabelButton = new JLabelButton("View Units");
				lblUnits.setConstraints("East");
				
				lblUnits.addEventListener(MouseEvent.MOUSE_OVER, function(e: Event): void {
					if (tooltip) {
						tooltip.hide();
					}
					
					tooltip = new SimpleTroopStubTooltip(troop.stub);
					tooltip.show(lblUnits);
				});
				
				lblUnits.addEventListener(MouseEvent.MOUSE_OUT, function(e: Event): void {
					if (tooltip) {
						tooltip.hide();
						tooltip = null;
					}
				});
				
				troopHolder.appendAll(lblPlayerCity, lblUnits);
				pnlTroops.append(troopHolder);
			}
			
			var scrollTroops: JScrollPane = new JScrollPane(new JViewport(pnlTroops, true), JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			(scrollTroops.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
			scrollTroops.setConstraints("Center");			
			
			pnlHeader.appendAll(pnlDetails, lblCountdown);
			appendAll(pnlHeader, scrollTroops);
		}
	}

}

