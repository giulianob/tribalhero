package src.UI.Components.TroopsDialogTable 
{
    import org.aswing.*;

    import src.Global;
    import src.Map.City;
    import src.Objects.Actions.Action;
    import src.Objects.Actions.Notification;
    import src.Objects.Factories.SpriteFactory;
    import src.Objects.Troop.TroopStub;
    import src.UI.Components.CountDownLabel;
    import src.UI.Components.TableCells.AbstractPanelTableCell;

    public class TroopStatusCell extends AbstractPanelTableCell
	{			
		private var lblState:JLabel;
		private var lblCountdown: CountDownLabel;
		
		public function TroopStatusCell() 
		{
			super();
			
			lblState = new JLabel("", null, AsWingConstants.LEFT);
			lblCountdown = new CountDownLabel( -1);
			
			lblState.setConstraints("Center");
			lblCountdown.setConstraints("East");
			
			getCellPanel().appendAll(lblState, lblCountdown);
			lblCountdown.setVisible(false);
		}
		
		override public function setCellValue(value:*):void 
		{
			super.setCellValue(value);
			
			var troopStub: TroopStub = value as TroopStub;		
			
			var icon: Icon;
			lblCountdown.setVisible(false);
			
			var city: City = Global.map.cities.get(troopStub.cityId);
			if (city) {
				var notifications: * = city.notifications.getByObject(troopStub.cityId, troopStub.objectId);
				for each (var notification: Notification in notifications) {
					if (Action.actionCategory[notification.type] == Action.CATEGORY_ATTACK) {
						icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_SINGLE_SWORD"));
						lblCountdown.setTime(notification.endTime);
						lblCountdown.setVisible(true);
						break;
					}
                    else if (Action.actionCategory[notification.type] == Action.CATEGORY_DEFENSE) {
						icon = new AssetIcon(SpriteFactory.getFlashSprite("ICON_SHIELD"));
						lblCountdown.setTime(notification.endTime);
						lblCountdown.setVisible(true);
						break;
                    }
				}
			}
			
			if (!icon) {
				icon = troopStub.state == TroopStub.WAITING_IN_OFFENSIVE_ASSIGNMENT ? new AssetIcon(SpriteFactory.getFlashSprite("ICON_SINGLE_SWORD")) : new AssetIcon(SpriteFactory.getFlashSprite("ICON_SHIELD"));
			}
			
			lblState.setText(troopStub.getStateName());
			lblState.setIcon(icon);
		}
		
		override protected function getCellLayout():LayoutManager 
		{
			return new BorderLayout(5);
		}
	}

}