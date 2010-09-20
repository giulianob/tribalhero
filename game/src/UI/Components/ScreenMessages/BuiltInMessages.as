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

		public static function processAll(city: City) : void {
			showInBattle(city);
			showTroopsStarving(city);
			showIncomingAttack(city);			
		}
		
		public static function showInBattle(city: City) : void {
			if (city.inBattle) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/BATTLE/" + city.id, city.name + " is under attack", new AssetIcon(new ICON_BATTLE)));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/BATTLE/" + city.id);
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

			var inAtk: Boolean = false;
			var inDef: Boolean = false;

			if (!city.inBattle) {
				for each (var notification: Notification in city.notifications.each()) {
					if (notification.cityId == city.id) continue;

					if (notification.type == PassiveAction.ATTACK) {
						Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/INATK", city.name + ": Incoming attack", new AssetIcon(new ICON_BATTLE)));
						inAtk = true;
					}

					if (notification.type == PassiveAction.DEFENSE) {
						Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/INDEF", city.name + ": Incoming reinforcement", new AssetIcon(new ICON_SHIELD)));
						inDef = true;
					}
				}
			}

			if (!inAtk) {
				Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/INATK");
			}

			if (!inDef) {
				Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/INDEF");
			}
		}

	}

}

