package src.UI.Dialog {

	import flash.events.Event;
	import src.Global;
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.Components.SimpleResourcesPanel;
	import src.UI.Components.SimpleTooltip;
	import src.UI.GameJPanel;
	import src.Util.StringHelper;
	import src.Util.Util;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class UnitTrainDialog extends GameJPanel {

		private var txtTitle:JLabel;
		private var panel4:JPanel;
		private var sldAmount:JAdjuster;
		private var panel8:JPanel;
		private var pnlResources: JPanel;
		private var lblTime: JLabel;
		private var lblUpkeep: JLabel;
		private var btnOk:JButton;
		private var unitPrototype: UnitPrototype;
		private var trainTime: int;

		public function UnitTrainDialog(unitPrototype: UnitPrototype, onAccept: Function, trainTime: int):void {
			this.unitPrototype = unitPrototype;
			this.trainTime = trainTime;

			createUI();

			title = "Train " + unitPrototype.getName();

			txtTitle.setText("How many units would you like to train?");

			sldAmount.setMinimum(1);
			sldAmount.setValues(1, 0, 1, Global.gameContainer.selectedCity.resources.Div(unitPrototype.trainResources));

			sldAmount.addStateListener(updateResources);
			sldAmount.addStateListener(updateTime);

			var self: UnitTrainDialog = this;
			btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );

			updateResources();
			updateTime();
		}

		private function updateTime(e: Event = null) : void {
			if (trainTime <= 0) return;

			lblTime.setText(Util.formatTime(trainTime * sldAmount.getValue()));
		}

		private function updateResources(e: Event = null) : void {
			lblUpkeep.setText("-" + (unitPrototype.upkeep * sldAmount.getValue()) + " per hour");
			
			pnlResources.removeAll();
			pnlResources.append(new SimpleResourcesPanel(unitPrototype.trainResources.multiplyByUnit(sldAmount.getValue())));
		}

		public function getAmount(): JAdjuster
		{
			return sldAmount;
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI(): void
		{
			setPreferredSize(new IntDimension(250, 150));
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 2);
			setLayout(layout0);

			txtTitle = new JLabel();
			txtTitle.setMinimumWidth(220);
			txtTitle.setHorizontalAlignment(AsWingConstants.LEFT);

			panel4 = new JPanel();
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.CENTER);
			panel4.setLayout(layout1);

			sldAmount = new JAdjuster();
			sldAmount.setColumns(5);

			panel8 = new JPanel();
			var layout2:FlowLayout = new FlowLayout();
			layout2.setAlignment(AsWingConstants.CENTER);
			panel8.setLayout(layout2);

			btnOk = new JButton();
			btnOk.setSize(new IntDimension(22, 22));
			btnOk.setText("Ok");

			var pnlCost: JPanel = new JPanel(new BorderLayout(5, 5));
			
			pnlResources = new JPanel();
			pnlResources.setConstraints("North");

			lblTime = new JLabel("", new AssetIcon(new ICON_CLOCK()));
			lblTime.setConstraints("West");
			new SimpleTooltip(lblTime, "Time to train units");
			
			lblUpkeep = new JLabel("", new AssetIcon(new ICON_CROP()));
			lblUpkeep.setConstraints("East");
			new SimpleTooltip(lblUpkeep, "Upkeep");
			
			//component layoution
			append(txtTitle);
			append(panel4);

			pnlCost.append(pnlResources);
			pnlCost.append(lblTime);
			pnlCost.append(lblUpkeep);			
			append(pnlCost);

			append(panel8);

			panel4.append(sldAmount);

			panel8.append(btnOk);
		}
	}

}

