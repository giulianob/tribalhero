package src.UI.Tooltips 
{
	import org.aswing.AsWingConstants;
	import org.aswing.ext.MultilineLabel;
	import org.aswing.FlowLayout;
	import org.aswing.JLabel;
	import org.aswing.JPanel;
	import src.Objects.Resources;
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class GateRepairTooltip extends Tooltip 
	{
		private var level: int;
		private var value: int;
		private var tribeResource: Resources;
		
		private var pnlHeader: JPanel;
		private var pnlFooter: JPanel;
		private var lblTitle: JLabel;
		private var lblDescription: MultilineLabel;
		
		public function GateRepairTooltip(level: int, value: int, tribeResource: Resources) 
		{
			this.level = level;
			this.value = value;
			this.tribeResource = tribeResource;
		}
		/*
		override public function draw(): void
		{
			super.draw();
			
			if (!drawTooltip) return;
			else if (pnlHeader == null) createUI();
			
			lblTime.setText(Util.formatTime(Formula.buildTime(city, structPrototype.buildTime, city.techManager)));

			pnlResources.removeAll();
			
			pnlResources.append(new NewCityResourcesPanel());
		}*/
/*
		private function createUI(): void {
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(3);
			ui.setLayout(layout0);

			pnlHeader = new JPanel();
			pnlHeader.setLocation(new IntPoint(5, 5));
			pnlHeader.setSize(new IntDimension(200, 17));
			pnlHeader.setLayout(new BorderLayout(10, 0));

			lblTitle = new JLabel();
			lblTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			lblTitle.setConstraints("Center");
			GameLookAndFeel.changeClass(lblTitle, "header");

			lblDescription = new MultilineLabel();
			lblDescription.setConstraints("South");
			GameLookAndFeel.changeClass(lblDescription, "Tooltip.text");

			pnlFooter = new JPanel();
			pnlFooter.setLayout(new BorderLayout(10, 0));

			pnlResources = new JPanel();
			pnlResources.setConstraints("Center");
			var layout4:FlowLayout = new FlowLayout();
			layout4.setAlignment(AsWingConstants.RIGHT);
			pnlResources.setLayout(layout4);

			//component layoution
			ui.append(pnlHeader);
			ui.append(pnlFooter);

			pnlHeader.append(lblTitle);
			pnlHeader.append(lblDescription);
			
			pnlFooter.append(pnlResources);
			
			// text values
			lblTitle.setText("Repair stronghold's gate");
			lblDescription.setText(Locale.loadString("STRONGHOLD_GATE_REPAIR_DESC"));
		}*/
	}

}