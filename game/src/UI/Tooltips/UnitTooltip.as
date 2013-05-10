package src.UI.Tooltips {
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.LookAndFeel.GameLookAndFeel;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;		
	
	public class UnitTooltip extends Tooltip{
		
		private var pnlHeader: JPanel;
		private var lblName: JLabel;
		private var lblLevel: JLabel;
		
		public function UnitTooltip(unitPrototype: UnitPrototype, count: int = -1) {		
			
			createUI();
			
			if (count > -1)
				lblName.setText(count + " " + unitPrototype.getName(0));
			else	
				lblName.setText(unitPrototype.getName(0));
				
			lblLevel.setText("Level " + unitPrototype.level.toString());
		}
		
		private function createUI() : void {
			ui.setLayout(new BorderLayout(20));
			
			lblName = new JLabel();							
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "Tooltip.text");
			lblName.setConstraints("Center");
			
			lblLevel = new JLabel();				
			lblLevel.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLevel, "Tooltip.text");
			lblLevel.setConstraints("East");
					
			ui.append(lblName);
			ui.append(lblLevel);
		}		
		
	}	
}