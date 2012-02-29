package src.UI.Components.ScreenMessages
{
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.AssetIcon;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Objects.Actions.Notification;
	import src.Objects.Actions.PassiveAction;
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
		}
		
		private function periodicMessages(e: Event = null): void {
			showNewbieProtection();
		}
		
		public static function showNewbieProtection() : void {
			if (Constants.signupTime.time / 1000 + Constants.newbieProtectionSeconds > Global.map.getServerTime()) {
				var timediff :Number = Constants.newbieProtectionSeconds + Constants.signupTime.time / 1000 - Global.map.getServerTime();
				Global.gameContainer.screenMessage.removeMessage("/NEWBIE_PROTECTION/");
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/NEWBIE_PROTECTION/", " Under newbie protection: Expires in " + Util.niceTime(timediff), new AssetIcon(new ICON_STAR)));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/NEWBIE_PROTECTION/");
			}			
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
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/STARVE", city.name + ": Troops may be starving to death", new AssetIcon(new ICON_CROP)));
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
		
		public static function showTribeAssignmentIncoming(assignment:int, incoming:int, firstTime:Boolean = false):void {
			if (incoming > 0 || assignment > 0) {
				Global.gameContainer.screenMessage.removeMessage("Tribe");
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("Tribe", "Tribe: " + incoming + " invasion alert(s) and " + assignment + " pending assignment(s)", new AssetIcon(new ICON_ALERT), 0));
			} else if(!firstTime) {
				Global.gameContainer.screenMessage.removeMessage("Tribe");
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("Tribe", "Tribe: no more invasion alert or pending assignment", new AssetIcon(new ICON_ALERT), 10000));
			}
		}
		
		public static function hideTribeAssignmentIncoming() : void {
				Global.gameContainer.screenMessage.removeMessage("Tribe");		
		}
	}

}

