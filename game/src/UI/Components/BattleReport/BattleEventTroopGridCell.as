package src.UI.Components.BattleReport
{
	import flash.display.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.Troop.*;
	import src.UI.LookAndFeel.*;

	public class BattleEventTroopGridCell extends JPanel implements GridListCell{

		protected var value: * ;
		
		public function BattleEventTroopGridCell() 
		{
			setLayout(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 5));
			setPreferredSize(new IntDimension(80, 40));
		}

		public function setCellValue(data:*):void {			
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
			removeAll();
			
			var pnlTextHolder:JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0, AsWingConstants.CENTER));			
			
			if (ObjectFactory.getClassType(data.type) == ObjectFactory.TYPE_UNIT)
			{
				var lblCount: JLabel = new JLabel(data.count, null, AsWingConstants.RIGHT);				
				GameLookAndFeel.changeClass(lblCount, "Tooltip.text");
				pnlTextHolder.append(lblCount);
			}
			
			if (data.hasOwnProperty("joinCount"))
			{
				var delta:int = -(data.joinCount - data.count);
				if (delta < 0)
				{
					var lblDelta:JLabel = new JLabel(delta.toString(), null, AsWingConstants.RIGHT);
					GameLookAndFeel.changeClass(lblDelta, "Label.error Label.very_small");
					pnlTextHolder.append(lblDelta);
				}
			}
			
			append(pnlTextHolder);
			append(new AssetPane(icon));
			pack();
		}

		public function getCellValue():*{
			return value;
		}

		public function getCellComponent():Component{
			return this;
		}

		public function setGridListCellStatus(gridList:GridList, selected:Boolean, index:int):void {
		}

	}

}
