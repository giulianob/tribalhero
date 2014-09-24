package src.UI.Components.TroopsDialogTable 
{
    import org.aswing.*;

    import src.Objects.Actions.*;
    import src.Objects.Factories.SpriteFactory;
    import src.UI.Components.*;
    import src.UI.Components.TableCells.*;
    import src.Util.StringHelper;

    public class TroopNotificationStatusCell extends AbstractPanelTableCell
	{			
		private var lblState:JLabel;
		private var lblCountdown: CountDownLabel;
		
		public function TroopNotificationStatusCell() 
		{
			super();
			
			lblState = new JLabel("", null, AsWingConstants.LEFT);
			lblCountdown = new CountDownLabel( -1);
			
			lblState.setConstraints("Center");
			lblCountdown.setConstraints("East");
			
			getCellPanel().appendAll(lblState, lblCountdown);
		}
		
		override public function setCellValue(value:*):void 
		{
			super.setCellValue(value);
			
			var notification: Notification = value as Notification;
            
            var isAttack: Boolean = Action.actionCategory[notification.type] == Action.CATEGORY_ATTACK;
            
            lblCountdown.setTime(notification.endTime);
			
			lblState.setText(StringHelper.localize(isAttack ? "STR_ATTACKING" : "STR_DEFENDING"));
			lblState.setIcon(isAttack ? new AssetIcon(SpriteFactory.getFlashSprite("ICON_SINGLE_SWORD")) : new AssetIcon(SpriteFactory.getFlashSprite("ICON_SHIELD")));
		}
		
		override protected function getCellLayout():LayoutManager 
		{
			return new BorderLayout(5);
		}
	}

}