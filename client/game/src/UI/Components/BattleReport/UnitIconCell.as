package src.UI.Components.BattleReport
{
    import flash.display.DisplayObject;

    import org.aswing.*;
    import org.aswing.table.*;

    import src.Objects.Factories.ObjectFactory;
    import src.UI.Components.SimpleTooltip;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.Util.Util;

    public class UnitIconCell extends AbstractTableCell
	{
		protected var panel:JPanel;
		protected var tooltip:SimpleTooltip;
		
		public function UnitIconCell()
		{
			panel = new JPanel();
			panel.setOpaque(true);
		}
		
		override public function setCellValue(data:*):void
		{
			// Get Icon
			var prototype:* = ObjectFactory.getPrototype(data.type, data.level);
			var icon:DisplayObject = ObjectFactory.getSpriteEx(data.theme, prototype.type, prototype.level);
            Util.resizeSprite(icon, 55, 35);
			new SimpleTooltip(icon, prototype.getName());
			
			// Lay it out
			panel.removeAll();
			
			var pnlTextHolder:JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0, AsWingConstants.CENTER));
			
			if (ObjectFactory.getClassType(data.type) == ObjectFactory.TYPE_UNIT)
			{
				pnlTextHolder.append(new JLabel(data.count, null, AsWingConstants.RIGHT));
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
			
			panel.append(pnlTextHolder);
			panel.append(new AssetPane(icon));
			panel.pack();
		}
		
		override public function getCellComponent():Component
		{
			return panel;
		}
	}

}

