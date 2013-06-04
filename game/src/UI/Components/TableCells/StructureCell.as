package src.UI.Components.TableCells
{
	import flash.display.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import org.aswing.table.AbstractTableCell;
	import org.aswing.table.TableCell;
	import src.Map.CityObject;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.Troop.*;
	import src.UI.LookAndFeel.*;

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
			var icon:DisplayObject = ObjectFactory.getSpriteEx(data.type, 1, true);
			var scale:Number = 40 / icon.height;
			if (scale < 1)
			{
				scale = Math.min(0.5, Number(scale.toFixed(1)));
				icon.scaleX = scale;
				icon.scaleY = scale;
			}
			
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
			iconAssetPane.setPreferredSize(new IntDimension(40, panel.getPreferredHeight()));
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
