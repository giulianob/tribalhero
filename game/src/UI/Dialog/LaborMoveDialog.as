package src.UI.Dialog{

	import flash.events.Event;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.InteractiveEvent;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.plaf.basic.background.PanelBackground;
	import src.Global;
	import src.Map.City;
	import src.Objects.Effects.Formula;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.Factories.StructureFactory;
	import src.Objects.LazyResources;
	import src.Objects.LazyValue;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.Resources;
	import src.Objects.StructureObject;
	import src.UI.GameJPanel;
    import src.Util.DateUtil;
    import src.Util.Util;

	public class LaborMoveDialog extends GameJPanel{

		//members define
		private var label81:JLabel;
		private var panel82:JPanel;
		private var panelTime: JPanel;
		private var lblCount:JLabel;
		private var lblRate:JLabel;
		private var lblTime:JLabel;
		private var sldCount:JSlider;
		private var panel86:JPanel;
		private var btnOk:JButton;
		private var msg: JLabel;

		private var structure: StructureObject;
		private var structPrototype: StructurePrototype;
		private var city: City;
		private var resource: LazyValue;
		private var resourceType: int;

		public function LaborMoveDialog(structure: StructureObject, onAccept: Function) {
			this.structure = structure;

			this.city = Global.gameContainer.map.cities.get(structure.cityId);

			city.addEventListener(City.RESOURCES_UPDATE, updateSlider);

			if (ObjectFactory.isType("Crop", structure.type)) {
				resourceType = Resources.TYPE_CROP;
				resource = city.resources.crop;
			}
			else if (ObjectFactory.isType("Wood", structure.type)) {
				resourceType = Resources.TYPE_CROP;
				resource = city.resources.wood;
			}

			createUI();

			this.structPrototype = StructureFactory.getPrototype(structure.type, structure.level);

			sldCount.setMaximum(structPrototype.maxlabor);

			if (structPrototype.maxlabor > 20) {
				sldCount.setPaintTicks(false);
			}

			sldCount.setValue(structure.labor);

			sldCount.addEventListener(InteractiveEvent.STATE_CHANGED, onSlideChange);

			var self: LaborMoveDialog = this;
			btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );

			updateSlider();
			onSlideChange();
		}

		private function updateSlider(e: Event = null): void {
			var maxAffordable: int = (city.resources.labor.getValue() + structure.labor);
			var maxAllowed: int = Math.max(5 + structure.labor, Math.ceil(structPrototype.maxlabor / 2.0) + structure.labor);

			msg.setText("You can only add " + (maxAllowed - structure.labor) + " labors at a time.");
			var extent: int = structPrototype.maxlabor - Math.min(maxAffordable, maxAllowed);

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

			var laborDelta: int = (sldCount.getValue() - structure.labor);
			
			if (resource)
			{				
				var newRate: int = LazyResources.getHourlyRate(Formula.getResourceRateWithBonus(resource, structure.level, resourceType, laborDelta));

				lblRate.setText((newRate == 0 ? "0" : "+" + newRate) + " per hour");
			}
			
			var moveTime: int = Formula.laborMoveTime(structure, Math.abs(laborDelta), laborDelta >= 0, city, city.techManager);			
			
			lblTime.setText(DateUtil.formatTime(moveTime));
			lblCount.setText(sldCount.getValue().toString() + " out of " + sldCount.getMaximum().toString());

			if (getFrame() != null) {
				getFrame().pack();
			}
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
			setSize(new IntDimension(220, 160));
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

			var topPanel: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			topPanel.setConstraints("North");

			msg = new JLabel("");
			msg.setHorizontalAlignment(AsWingConstants.LEFT);

			label81 = new JLabel("How many laborers to place in this structure?");
			label81.setHorizontalAlignment(AsWingConstants.LEFT);

			panel82 = new JPanel(new BorderLayout());
			panel82.setBorder(new EmptyBorder(null, new Insets(15, 0, 5, 0)));
			panel82.setLocation(new IntPoint(5, 37));
			panel82.setSize(new IntDimension(230, 164));
			panel82.setConstraints("Center");

			lblCount = new JLabel();
			lblCount.setConstraints("West");

			lblRate = new JLabel();
			lblRate.setConstraints("East");
			switch (resourceType) {
				case Resources.TYPE_CROP:
					lblRate.setIcon(new AssetIcon(new ICON_CROP()));
				break;
				case Resources.TYPE_WOOD:
					lblRate.setIcon(new AssetIcon(new ICON_WOOD()));
				break;
			}

			sldCount = new JSlider();			
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
			
			panelTime = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 0, AsWingConstants.RIGHT));
			
			lblTime = new JLabel("00:00:00", new AssetIcon(new ICON_CLOCK()), AsWingConstants.RIGHT);

			//component layoution
			topPanel.append(label81);
			topPanel.append(msg);

			var pnlGrouping: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			pnlGrouping.append(panel82);
			pnlGrouping.append(panelTime);			
			
			append(topPanel);
			append(pnlGrouping);			
			append(panel86);

			panelTime.append(lblTime);
			
			panel82.append(lblCount);
			panel82.append(lblRate);
			panel82.append(sldCount);						

			panel86.append(btnOk);
		}
	}
}

