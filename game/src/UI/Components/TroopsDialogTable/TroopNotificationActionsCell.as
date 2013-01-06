package src.UI.Components.TroopsDialogTable 
{
	import flash.events.*;
	import org.aswing.*;
	import src.*;
	import src.Objects.Process.*;
	import src.Objects.Troop.*;
	import src.UI.Components.TableCells.*;
	import src.Util.*;
	
	public class TroopNotificationActionsCell extends AbstractPanelTableCell 
	{				
		override protected function getCellLayout():LayoutManager 
		{
			return new FlowLayout(AsWingConstants.RIGHT, 10, 0, false);
		}
		
		override public function setCellValue(value:*):void 
		{
			super.setCellValue(value);
			getCellPanel().removeAll();
            
            var btnLocate: JLabelButton = new JLabelButton(StringHelper.localize("STR_LOCATE"));
            btnLocate.addActionListener(function(e: Event): void {
                new LocateNotificationProcess(value).execute();
            });
            getCellPanel().append(btnLocate);			
		}		
	}

}