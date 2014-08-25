package src.UI.Components.ScreenMessages
{
    import System.Linq.Enumerable;

    import flash.events.Event;
    import flash.events.TimerEvent;
    import flash.utils.Timer;

    import org.aswing.AssetIcon;

    import src.Constants;
    import src.Global;
    import src.Map.City;
    import src.Objects.Actions.Notification;
    import src.Objects.Actions.PassiveAction;
    import src.Objects.Factories.SpriteFactory;
    import src.Util.DateUtil;
    import src.Util.StringHelper;

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
			showTribeAssignmentIncoming(Constants.session.tribeAssignment, Constants.session.tribeIncoming, true);
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
			Global.gameContainer.screenMessage.removeMessage("/CITY/"+city.id+"/AP_STATUS/RESOURCE_CAP/");
			if (city.ap >= 75) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/"+city.id+"/AP_STATUS/RESOURCE_CAP/", StringHelper.localize("MSG_AP_RESOURCE_BONUS", city.name), new AssetIcon(SpriteFactory.getFlashSprite("ICON_STAR"))));
			}
			
			Global.gameContainer.screenMessage.removeMessage("/CITY/"+city.id+"/AP_STATUS/STRUCTURE_DEFENSE");
			if (city.ap >= 90) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/"+city.id+"/AP_STATUS/STRUCTURE_DEFENSE", StringHelper.localize("MSG_AP_STRUCTURE_DEFENSE_BONUS", city.name), new AssetIcon(SpriteFactory.getFlashSprite("ICON_STAR"))));
            }
		}		
		
		public static function showNewbieProtection() : void {			
			if (Constants.session.signupTime.time / 1000 + Constants.session.newbieProtectionSeconds > Global.map.getServerTime()) {
				var timediff :Number = Constants.session.newbieProtectionSeconds + Constants.session.signupTime.time / 1000 - Global.map.getServerTime();
				Global.gameContainer.screenMessage.removeMessage("/NEWBIE_PROTECTION/");
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/NEWBIE_PROTECTION/", StringHelper.localize("MSG_NEWBIE_PROTECTION", DateUtil.niceTime(timediff)), new AssetIcon(SpriteFactory.getFlashSprite("ICON_STAR"))));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/NEWBIE_PROTECTION/");
			}			
		}
		
		public static function showInBattle(city: City) : void {
			if (city.inBattle) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/BATTLE/" + city.id, StringHelper.localize("MSG_UNDER_ATTACK", city.name), new AssetIcon(SpriteFactory.getFlashSprite("ICON_BATTLE"))));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/BATTLE/" + city.id);
			}
		}

		public static function showTroopsStarving(city: City): void {
			if (city.resources.crop.getRate() < city.resources.crop.getUpkeep()) {
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/STARVE", StringHelper.localize("MSG_TROOPS_STARVING", city.name), new AssetIcon(SpriteFactory.getFlashSprite("ICON_CROP"))));
			}
			else {
				Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/STARVE");
			}
		}

		public static function showIncomingAttack(city: City): void {              
            var attacks: int = Enumerable.from(city.notifications).where(function (notification: Notification): Boolean {
                return notification.cityId != city.id && notification.type == PassiveAction.CITY_ATTACK;
            }).count();
            
            if (attacks > 0) {
                Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/INATK");
                Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/INATK", StringHelper.localize("MSG_INCOMING_ATTACK", city.name, attacks), new AssetIcon(SpriteFactory.getFlashSprite("ICON_BATTLE"))));
            }
            else {
                Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/INATK");
            }
            
            var defenses: int = Enumerable.from(city.notifications).where(function (notification: Notification): Boolean {
                return notification.cityId != city.id && notification.type == PassiveAction.CITY_DEFENSE;
            }).count();
            
            if (defenses > 0) {
                Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/INDEF");
                Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("/CITY/" + city.id + "/INDEF", StringHelper.localize("MSG_INCOMING_DEFENSE", city.name, defenses), new AssetIcon(SpriteFactory.getFlashSprite("ICON_SHIELD"))));
            }
            else {
                Global.gameContainer.screenMessage.removeMessage("/CITY/" + city.id + "/INDEF");
            }                                            
		}
		
		public static function showTribeAssignmentIncoming(assignment:int, incoming:int, firstTime:Boolean = false):void {
			if (incoming > 0 || assignment > 0) {
				Global.gameContainer.screenMessage.removeMessage("TRIBE");
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("TRIBE", StringHelper.localize("MSG_TRIBE_ALERTS", incoming, assignment), new AssetIcon(SpriteFactory.getFlashSprite("ICON_ALERT"))));
			} else if(!firstTime) {
				Global.gameContainer.screenMessage.removeMessage("TRIBE");
				Global.gameContainer.screenMessage.addMessage(new ScreenMessageItem("TRIBE", StringHelper.localize("MSG_TRIBE_NO_ALERTS"), new AssetIcon(SpriteFactory.getFlashSprite("ICON_ALERT")), 60000));
			}
		}
		
		public static function hideTribeAssignmentIncoming() : void {
				Global.gameContainer.screenMessage.removeMessage("TRIBE");		
		}
	}

}

