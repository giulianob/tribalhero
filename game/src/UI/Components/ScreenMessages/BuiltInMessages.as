package src.UI.Components.ScreenMessages
{
	import src.Util.StringHelper;
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import mx.utils.StringUtil;
	import org.aswing.AssetIcon;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Objects.Actions.Notification;
	import src.Objects.Actions.PassiveAction;
	import src.Util.StringHelper;
	import src.Util.Util;

	public class BuiltInMessages
	{
		private var timer: Timer;
		
		public function BuiltInMessages() {
			timer = new Timer(5 * 60 * 1000);
			timer.addEventListener(TimerEvent.TIMER, periodicMessages);
		}
		
		public function start(): void {
			timer.start();
			periodicMessages();
			showTribeAssignmentIncoming(Constants.tribeAssignment, Constants.tribeIncoming, true);
		}
		
		public function stop(): void {
			timer.stop();
		}
		
		public static function processAll(city: City) : void {
			showInBattle(city);
			showTroopsStarving(city);
			showIncomingAttack(city);
			showApStatus(city);
		}
		
		private function periodicMessages(e: Event = null): void {
			showNewbieProtection();
		}
		
		public static function showApStatus(city: City) : void {
			Global.gameContainer.screenMessage.removeMessage("/AP_STATUS/RESOURCE_CAP");
			if (city.ap >= 75) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/AP_STATUS/RESOURCE_CAP", StringHelper.localize("MSG_AP_RESOURCE_BONUS", city.name), new AssetIcon(new ICON_STAR)));
			}
			
			Global.gameContainer.screenMessage.removeMessage("/AP_STATUS/STRUCTURE_DEFENSE");
			if (city.ap >= 90) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/AP_STATUS/STRUCTURE_DEFENSE", StringHelper.localize("MSG_AP_STRUCTURE_DEFENSE_BONUS", city.name), new AssetIcon(new ICON_STAR)));
			}
		}		
		
		public static function showNewbieProtection() : void {			
			if (Constants.signupTime.time / 1000 + Constants.newbieProtectionSeconds > Global.map.getServerTime()) {
				var timediff :Number = Constants.newbieProtectionSeconds + Constants.signupTime.time / 1000 - Global.map.getServerTime();
				Global.gameContainer.screenMessage.removeMessage("/NEWBIE_PROTECTION/");
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/NEWBIE_PROTECTION/", StringHelper.localize("MSG_NEWBIE_PROTECTION", Util.niceTime(timediff)), new AssetIcon(new ICON_STAR)));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/NEWBIE_PROTECTION/");
			}			
		}
		
		public static function showInBattle(city: City) : void {
			if (city.inBattle) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/BATTLE/" + city.id, StringHelper.localize("MSG_UNDER_ATTACK", city.name), new AssetIcon(new ICON_BATTLE)));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/BATTLE/" + city.id);
			}
		}

		public static function showTroopsStarving(city: City): void {
			if (city.resources.crop.getRate() < city.resources.crop.getUpkeep()) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/STARVE", StringHelper.localize("MSG_TROOPS_STARVING", city.name), new AssetIcon(new ICON_CROP)));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/STARVE");
			}
		}

		public static function showIncomingAttack(city: City): void {

			var inAtk: Boolean = false;
			var inDef: Boolean = false;

			if (!city.inBattle) {
				for each (var notification: Notification in city.notifications) {
					if (notification.cityId == city.id) continue;

					if (notification.type == PassiveAction.CITY_ATTACK) {
						Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/INATK", StringHelper.localize("MSG_INCOMING_ATTACK", city.name), new AssetIcon(new ICON_BATTLE)));
						inAtk = true;
					}

					if (notification.type == PassiveAction.CITY_DEFENSE) {
						Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/INDEF", StringHelper.localize("MSG_INCOMING_DEFENSE", city.name), new AssetIcon(new ICON_SHIELD)));
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
		
		public static function showTribeAssignmentIncoming(assignment:int, incoming:int, firstTime:Boolean = false):void {
			if (incoming > 0 || assignment > 0) {
				Global.gameContainer.screenMessage.removeMessage("TRIBE");
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("TRIBE", StringHelper.localize("MSG_TRIBE_ALERTS", incoming, assignment), new AssetIcon(new ICON_ALERT)));
			} else if(!firstTime) {
				Global.gameContainer.screenMessage.removeMessage("TRIBE");
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("TRIBE", StringHelper.localize("MSG_TRIBE_NO_ALERTS"), new AssetIcon(new ICON_ALERT), 60000));
			}
		}
		
		public static function hideTribeAssignmentIncoming() : void {
				Global.gameContainer.screenMessage.removeMessage("TRIBE");		
		}
	}

}

