package src.UI.Components.BattleReport 
{
	import org.aswing.*;
	import org.aswing.event.*;
	import org.aswing.table.*;

	import src.Util.Util;
	/**
	 * ...
	 * @author ...
	 */
	public class OneDecimalValueCell extends AbstractTableCell
	{
		private var wrapper: JLabel;

		public function OneDecimalValueCell() 
		{
			super();
			wrapper = new JLabel("",null,AsWingConstants.LEFT);
			wrapper.setOpaque(true);
		}

		override public function setCellValue(value:*):void
		{
			this.value = value;
			super.setCellValue(value);
			wrapper.setText(Util.roundNumber(Number(value)).toString());
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