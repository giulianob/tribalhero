package src.UI.Components.TroopsDialogTable 
{
	import flash.events.*;
	import org.aswing.*;
	import src.*;
	import src.Objects.Process.*;
	import src.Objects.Troop.*;
	import src.UI.Components.TableCells.*;
	import src.UI.Dialog.TroopAttackModeDialog;
	import src.UI.Dialog.TroopTransferDialog;
	import src.Util.*;
	import src.Comm.Commands.*;
	
	public class TroopActionsCell extends AbstractPanelTableCell 
	{				
		override protected function getCellLayout():LayoutManager 
		{
			return new FlowLayout(AsWingConstants.RIGHT, 2, 0, false);
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
			else if (value.isMoving()) {
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
                
				if(value.isStationedNotInBattle() && value.playerId == Constants.playerId) {
					var btnSwitch: JLabelButton = new JLabelButton(StringHelper.localize("STR_MANAGE"));
					btnSwitch.addActionListener(function(e: Event): void {
						var dialog : TroopAttackModeDialog = new TroopAttackModeDialog(value);
						dialog.show();
					});				
					getCellPanel().append(btnSwitch);
					
					if((value as TroopStub).stationedLocation.type==2) {
						var btnTransfer: JLabelButton = new JLabelButton("Transfer");
						btnTransfer.addActionListener(function(e: Event): void {
							Global.mapComm.Stronghold.listStrongholds(function(strongholds:*):void {
								var dialog : TroopTransferDialog = new TroopTransferDialog(value,strongholds);
								dialog.show();
							});
						});				
						getCellPanel().append(btnTransfer);
					}
				}
			}
		}		
	}

}