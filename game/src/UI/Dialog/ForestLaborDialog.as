package src.UI.Dialog{

	import flash.events.Event;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.InteractiveEvent;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.Formula;
	import src.Objects.Forest;
	import src.UI.GameJPanel;

	public class ForestLaborDialog extends GameJPanel{

		//members define
		private var label81:JLabel;
		private var panel82:JPanel;
		private var lblCount:JLabel;
		private var lblRate:JLabel;
		private var sldCount:JSlider;
		private var panel86:JPanel;
		private var btnOk:JButton;

		private var forest: Forest;
		public var city: City;

		public function ForestLaborDialog(cityId: int, forest: Forest, onAccept: Function) {
			this.forest = forest;

			this.city = Global.gameContainer.map.cities.get(cityId);

			city.addEventListener(City.RESOURCES_UPDATE, updateSlider);

			createUI();

			sldCount.setMaximum(Formula.maxForestLaborPerUser(forest.level));
            sldCount.setValue(sldCount.getMaximum());

			if (sldCount.getMaximum() > 20) {
				sldCount.setPaintTicks(false);
			}

			sldCount.addEventListener(InteractiveEvent.STATE_CHANGED, onSlideChange);

			var self: ForestLaborDialog = this;
			btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );

			updateSlider();
			onSlideChange();
		}

		private function updateSlider(e: Event = null): void {
			var extent: int = sldCount.getMaximum() - city.resources.labor.getValue();

			if (sldCount.getValue() > sldCount.getMaximum() - extent) {
				sldCount.setValue(sldCount.getMaximum() - extent);
			}

			if (extent <= 0) {
				sldCount.setExtent(0);
			}
			else if (sldCount.getExtent() != extent) {
				sldCount.setExtent(extent);
			}

			onSlideChange();
		}

		private function onSlideChange(e: InteractiveEvent = null):void {
			lblCount.setText(sldCount.getValue().toString() + " out of " + sldCount.getMaximum().toString());

			if (getFrame() != null)
			getFrame().pack();
		}

		public function getForest():Forest {
			return forest;
		}
		
		public function getCount():int{
			return sldCount.getValue();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose, dispose);
			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function dispose():void {
			this.city.removeEventListener(City.RESOURCES_UPDATE, updateSlider);
		}

		private function createUI():void {
			//component creation
			title = "Labor Assignment";
			setSize(new IntDimension(220, 111));
			var layout0:BorderLayout = new BorderLayout();
			setLayout(layout0);

			label81 = new JLabel();
			label81.setLocation(new IntPoint(5, 5));
			label81.setSize(new IntDimension(250, 30));
			label81.setPreferredSize(new IntDimension(260, 30));
			label81.setConstraints("North");
			label81.setText("How many laborers to place in this forest?");
			label81.setHorizontalAlignment(AsWingConstants.LEFT);

			panel82 = new JPanel(new BorderLayout());
			panel82.setLocation(new IntPoint(5, 37));
			panel82.setSize(new IntDimension(230, 164));
			panel82.setConstraints("Center");

			lblCount = new JLabel();
			lblCount.setConstraints("West");

			lblRate = new JLabel();
			lblRate.setConstraints("East");

			sldCount = new JSlider();
			sldCount.setValue(0);
			sldCount.setConstraints("South");
			sldCount.setMinimum(0);
			sldCount.setMajorTickSpacing(1);
			sldCount.setPaintTicks(true);
			sldCount.setSnapToTicks(true);

			panel86 = new JPanel();
			panel86.setConstraints("South");
			var layout2:FlowLayout = new FlowLayout();
			layout2.setAlignment(AsWingConstants.CENTER);
			panel86.setLayout(layout2);

			btnOk = new JButton();
			btnOk.setLocation(new IntPoint(113, 5));
			btnOk.setSize(new IntDimension(22, 22));
			btnOk.setText("Ok");

			//component layoution
			append(label81);
			append(panel82);
			append(panel86);

			panel82.append(lblCount);
			panel82.append(lblRate);
			panel82.append(sldCount);

			panel86.append(btnOk);
		}
	}
}

