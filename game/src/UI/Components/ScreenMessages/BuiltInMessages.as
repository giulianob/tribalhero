package src.UI.Components.ScreenMessages
{
	import org.aswing.AssetIcon;
	import src.Global;
	import src.Map.City;
	import src.Objects.Actions.Notification;
	import src.Objects.Actions.PassiveAction;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class BuiltInMessages
	{

		public static function showInBattle(city: City) : void {
			if (city.inBattle) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/BATTLECOST/" + city.id, city.name + ": All costs raised by 50% while city is in battle", new AssetIcon(new ICON_GOLD)));
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/BATTLE/" + city.id, city.name + " is under attack", new AssetIcon(new ICON_BATTLE)));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/BATTLE/" + city.id);
				Global.gameContainer.screenMessage.removeMessage("/BATTLECOST/" + city.id);
			}
		}

		public static function showTroopsStarving(city: City): void {
			if (city.resources.crop.getRate() < city.resources.crop.getUpkeep()) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/STARVE", city.name + "'s troops may be starving to death", new AssetIcon(new ICON_CROP)));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/STARVE");
			}
		}
		
		public static function showIncomingAttack(city: City): void {
			if (!city.inBattle) {
				for each (var notification: Notification in city.notifications.each()) {
					if (notification.cityId != city.id && notification.type == PassiveAction.ATTACK) {
						Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/INATK", city.name + ": Incoming attack", new AssetIcon(new ICON_BATTLE)));
						return;
					}
				}
			}
			
			Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/INATK");
		}

	}

}

