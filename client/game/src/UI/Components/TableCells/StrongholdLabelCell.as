package src.UI.Components.TableCells
{
    import org.aswing.*;
    import org.aswing.table.*;

    import src.UI.Components.*;

    public class StrongholdLabelCell extends AbstractTableCell
	{
		protected var label: StrongholdLabel;
		protected var wrapper: JPanel;

		public function StrongholdLabelCell()
		{
			super();
			
			wrapper = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			wrapper.setOpaque(true);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			wrapper.removeAll();
			
			if (value is int)
				label = new StrongholdLabel(value, false);
			else
				label = new StrongholdLabel(value.strongholdId, false, value.strongholdName);
				
			wrapper.append(label);
		}

		override public function getCellValue():*
		{
			return value;
		}

		override public function getCellComponent():Component
		{
			return wrapper;
		}
	}

}

