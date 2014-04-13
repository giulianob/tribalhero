package src.UI.Components.BattleReport 
{
    import src.Util.DateUtil;

    public class DateUnreadTextCell extends UnreadTextCell
	{
        override public function setCellValue(value:*):void
        {
            super.setCellValue(value);

            lbl.setText(DateUtil.niceShort(value[getCellProperty()]));
        }

		override protected function getCellProperty():String 
		{
			return "date";
		}
		
	}

}