package src.UI.Components.BattleReport
{
	import org.aswing.*;
	import org.aswing.table.*;
	import src.Objects.Factories.ObjectFactory;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class UnitIconCell extends AbstractTableCell
	{		
		protected var panel: JPanel;
		
		public function UnitIconCell()
		{
			panel = new JPanel();
			panel.setOpaque(true);
		}

		override public function setCellValue(param1: *) : void
		{
			panel.removeAll();
			
			var pnlTextHolder: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0, AsWingConstants.CENTER));
						
			if (ObjectFactory.getClassType(param1.type) == ObjectFactory.TYPE_UNIT) {
				pnlTextHolder.append(new JLabel(param1.count, null, AsWingConstants.RIGHT));				
			}
			
			if (param1.delta < 0) {
				var lblDelta: JLabel = new JLabel(param1.delta, null, AsWingConstants.RIGHT);
				GameLookAndFeel.changeClass(lblDelta, "Label.error Label.very_small");
				pnlTextHolder.append(lblDelta);
			}

			panel.append(pnlTextHolder);
			panel.append(new AssetPane(param1.icon));			
			panel.pack();
		}

		override public function getCellComponent():Component 
		{
			return panel;
		}
	}

}

