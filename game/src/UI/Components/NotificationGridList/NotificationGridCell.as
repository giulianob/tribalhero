package src.UI.Components.NotificationGridList
{
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Objects.Actions.Notification;
	import src.Objects.Actions.PassiveAction;
	import src.Objects.Factories.TroopFactory;
	import src.Objects.Troop.TroopStub;

	public class NotificationGridCell extends JLabel implements GridListCell{

		private var value: * ;

		public function NotificationGridCell()
		{
			buttonMode = true;
		}

		public function setCellValue(value:*):void{
			this.value = value;

			// A bit hard coded here to match notification to icon
			var notification: Notification = value.notification as Notification;

			var state: int = TroopStub.IDLE;
			switch(notification.type) {
				case PassiveAction.CITY_ATTACK:
					state = TroopStub.BATTLE;
				break;
				case PassiveAction.CITY_DEFENSE:
					state = TroopStub.BATTLE_STATIONED;
				break;
			}

			setIcon(new AssetIcon(TroopFactory.getStateSprite(state)));
		}

		public function getCellValue():*{
			return value;
		}

		public function getCellComponent():Component{
			return this;
		}

		public function setGridListCellStatus(gridList:GridList, selected:Boolean, index:int):void {
		}
	}

}

