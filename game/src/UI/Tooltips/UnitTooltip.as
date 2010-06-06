package src.UI.Tooltips {
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.GameLookAndFeel;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;		
	
	public class UnitTooltip extends Tooltip{
		
				
		private var pnlHeader: JPanel;
		private var lblName: JLabel;
		private var lblLevel: JLabel;
		private var lblCount: JLabel;
		
		public function UnitTooltip(unitPrototype: UnitPrototype, count: int = -1) {		
			
			createUI();
			
			lblName.setText(unitPrototype.getName());
			lblLevel.setText("Level " + unitPrototype.level.toString());
			if (count > -1)
				lblCount.setText(count.toString());
		}
		
		private function createUI() : void {
			ui.setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 5));
			pnlHeader = new JPanel(new BorderLayout(20));
			
			lblName = new JLabel();							
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");
			
			lblLevel = new JLabel();				
			lblLevel.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLevel, "Tooltip.text");			
			
			lblCount = new JLabel();
			lblCount.setConstraints("East");
			lblCount.setHorizontalAlignment(AsWingConstants.RIGHT);
			GameLookAndFeel.changeClass(lblCount, "header");			
			
			pnlHeader.append(lblLevel);
			pnlHeader.append(lblCount);					
			
			ui.append(lblName);
			ui.append(pnlHeader);
		}		
		
	}	
}