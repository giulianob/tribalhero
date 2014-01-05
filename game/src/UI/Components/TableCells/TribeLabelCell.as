package src.UI.Components.TableCells
{
    import org.aswing.*;
    import org.aswing.table.*;

    import src.UI.Components.*;
    import src.Util.StringHelper;

    public class TribeLabelCell extends AbstractTableCell
	{
		protected var label: TribeLabel;
		protected var wrapper: JPanel;

		public function TribeLabelCell()
		{
			super();
			
			wrapper = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			wrapper.setOpaque(true);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			wrapper.removeAll();
			
			if (value is int) {
				label = new TribeLabel(value);
				wrapper.append(label);
			} else if( value.tribeId!=null) {
				label = new TribeLabel(value.tribeId, value.tribeName);
				wrapper.append(label);
			} else {
				wrapper.append(new JLabel(StringHelper.localize("STR_NEUTRAL")));
			}
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

