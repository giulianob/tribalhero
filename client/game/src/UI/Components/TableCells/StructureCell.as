package src.UI.Components.TableCells
{
    import flash.display.*;

    import org.aswing.*;
    import org.aswing.geom.*;
    import org.aswing.table.AbstractTableCell;

    import src.Objects.Factories.ObjectFactory;
    import src.Util.Util;

    public class StructureCell extends AbstractTableCell {

		private var panel: JPanel;
		
		public function StructureCell() 
		{
			panel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 5));
			panel.setPreferredSize(new IntDimension(80, 40));
			panel.setOpaque(true);
		}

		override public function setCellValue(data:*):void {			
			this.value = data;

			// Get Icon
			var icon:DisplayObjectContainer = ObjectFactory.getSpriteEx(data.theme, data.type, 1);
            Util.resizeSprite(icon, 50, 50);

			// Lay it out
			panel.removeAll();
			
			var pnlTextHolder:JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0, AsWingConstants.CENTER));			
			
			if (ObjectFactory.getClassType(data.type) != ObjectFactory.TYPE_STRUCTURE)
			{
				throw new Error("Must be a structure to use StructureCell");
			}
			
			var lblName: JLabel = new JLabel(data.getStructurePrototype().getName(), null, AsWingConstants.RIGHT);		
			pnlTextHolder.append(lblName);
			
			var iconAssetPane: AssetPane = new AssetPane(icon);
			iconAssetPane.setVerticalAlignment();
			
			panel.append(iconAssetPane);
			panel.append(pnlTextHolder);
			panel.pack();
		}

		override public function getCellComponent():Component{
			return panel;
		}
	}

}
