package src.UI.Components.TroopsDialogTable 
{
	import flash.events.*;
	import org.aswing.*;
	import src.*;
	import src.Objects.Process.*;
	import src.Objects.Troop.*;
	import src.UI.Components.TableCells.*;
	import src.Util.*;
	
	public class TroopActionsCell extends AbstractPanelTableCell 
	{				
		override protected function getCellLayout():LayoutManager 
		{
			return new FlowLayout(AsWingConstants.RIGHT, 10, 0, false);
		}
		
		override public function setCellValue(value:*):void 
		{
			super.setCellValue(value);
			getCellPanel().removeAll();
			
			if (value.isLocal()) {
				var btnManage: JLabelButton = new JLabelButton(StringHelper.localize("STR_MANAGE"));
				btnManage.setOpaque(true);
				btnManage.setBackground(ASColor.getASColor(0, 0, 0, 0));
				btnManage.addActionListener(function(e: Event): void {
					new ManageLocalTroopsProcess(Global.map.cities.get(value.cityId)).execute();
				});
				getCellPanel().append(btnManage);
			}
			
			if (value.state == TroopStub.MOVING || value.state == TroopStub.BATTLE) {
				var btnLocate: JLabelButton = new JLabelButton(StringHelper.localize("STR_LOCATE"));
				btnLocate.addActionListener(function(e: Event): void {
					new LocateTroopProcess(value).execute();
				});
				getCellPanel().append(btnLocate);
			}
			
			if (value.isStationed()) {
				var btnRetreat: JLabelButton = new JLabelButton(StringHelper.localize("STR_RETREAT"));
				btnRetreat.addActionListener(function(e: Event): void {
					new RetreatTroopProcess(value).execute();
				});				
				getCellPanel().append(btnRetreat);
			}
		}		
	}

}