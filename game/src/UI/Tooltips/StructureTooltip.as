package src.UI.Tooltips {
	import org.aswing.BorderLayout;
	import org.aswing.JLabel;
	import org.aswing.JPanel;
	import org.aswing.SoftBoxLayout;
	import src.Objects.Prototypes.StructurePrototype;
	import src.UI.GameLookAndFeel;
	
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;	
	
	public class StructureTooltip extends Tooltip{
		
		private var pnlHeader: JPanel;
		private var lblName: JLabel;
		private var lblLevel: JLabel;
		
		public function StructureTooltip(structurePrototype: StructurePrototype) {			
			createUI();
			
			lblName.setText(structurePrototype.getName());
			lblLevel.setText("Level " + structurePrototype.level);
		}
		
		private function createUI() : void {
			pnlHeader = new JPanel(new SoftBoxLayout(AsWingConstants.VERTICAL, 5, 0));
			
			lblName = new JLabel();						
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblName, "header");
			
			lblLevel = new JLabel();						
			lblLevel.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLevel, "Tooltip.text");			
			
			pnlHeader.append(lblName);
			pnlHeader.append(lblLevel);					
			
			ui.append(pnlHeader);
		}
		
	}	
}