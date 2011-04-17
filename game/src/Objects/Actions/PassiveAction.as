﻿package src.Objects.Actions 
{
	import flash.display.DisplayObject;
	import flash.display.DisplayObjectContainer;
	import flash.display.Sprite;
	import flash.utils.getDefinitionByName;
	import src.Global;
	import src.Objects.Factories.ObjectFactory;
	import src.Util.Util;
	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class PassiveAction 
	{
		public static const STRUCTURE_SELF_DESTROY: int = 108;
		public static const ATTACK: int = 250;
		public static const DEFENSE: int = 251;		
		public static const RETREAT: int = 252;			
		public static const FOREST_CAMP_HARVEST: int = 310;		
		public static const STRUCTURE_CHANGE: int = 5103;
		public static const CREATE_CITY: int = 505;
		
		private static var actionLookup: Array = new Array(
			{type: ATTACK, description: "Attacking", notificationDescription: attackNotification, icon: "PASSIVE_ATTACKING", cancellable: false },
			{type: DEFENSE, description: "Defending", notificationDescription: defenseNotification, icon: "PASSIVE_DEFENDING", cancellable: false },
			{type: RETREAT, description: "Retreating", notificationDescription: retreatNotification, icon: "PASSIVE_RETREATING", cancellable: false },
			{type: FOREST_CAMP_HARVEST, description: "Gathering Wood", notificationDescription: retreatNotification, icon: "PASSIVE_DEFENDING", cancellable: true },
			{type: STRUCTURE_SELF_DESTROY, description: "Time Left", notificationDescription: selfDestroyNotification, icon: "PASSIVE_DEFENDING", cancellable: false },
			{type: STRUCTURE_CHANGE, description: "Converting", notificationDescription: noNotification, icon: "PASSIVE_DEFENDING", cancellable: false },
			{type: CREATE_CITY, description: "Building City", notificationDescription: noNotification, icon: "PASSIVE_DEFENDING", cancellable: false }
		);		
		
		private static var actionsSorted: Boolean = false;
		
		public static function actionCompare(data: Object, value: int): int
		{
			if (data.type > value)
				return 1;
			else if (data.type < value)
				return -1;
			else
				return 0;
		}		
		
		public static function isCancellable(actionType: int) : Boolean
		{
			var idx: int = getActionIndex(actionType);
			
			if (idx <= -1)
				return true;
			
			return actionLookup[idx].cancellable;
		}
		
		private static function getActionIndex(actionType: int): int
		{
			if (!actionsSorted)
			{
				actionLookup.sortOn("type");
				actionsSorted = true;
			}
			
			return Util.binarySearch(actionLookup, actionCompare, actionType);			
		}
		
		public static function toString(actionType: int): String
		{
			var idx: int = getActionIndex(actionType);
			
			if (idx <= -1)
				return "Unknown Action";
			
			return actionLookup[idx].description;
		}
		
		public static function getIcon(actionType: int): DisplayObject
		{
			var idx: int = getActionIndex(actionType);
			
			if (idx <= -1)
				return new Sprite();
			
			return ObjectFactory.getIcon(actionLookup[idx].icon);
		}		
		
		/* NOTIFICATION DESCRIPTIONS */
		private static function noNotification(notification: Notification, local: Boolean): String {		
			return "";
		}
		
		private static function defenseNotification(notification: Notification, local: Boolean): String {		
			return "";
		}
		
		private static function retreatNotification(notification: Notification, local: Boolean): String {				
			return "";
		}
		
		private static function selfDestroyNotification(notification: Notification, local: Boolean) : String {
			return "";
		}
		
		private static function attackNotification(notification: Notification, local: Boolean): String {				
			if (local) {
				return "You are attacking";
			}
			else
			{
				var city: String = Global.map.usernames.cities.getUsername(notification.cityId).name;
				return city + " is attacking you";
			}
		}
	}
	
}