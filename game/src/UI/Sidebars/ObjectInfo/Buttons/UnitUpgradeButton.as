﻿
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Util.Util;
	import src.Objects.Actions.UnitUpgradeAction;
	import src.Objects.Effects.Formula;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.*;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.Cursors.*;
	import src.UI.Tooltips.UnitUpgradeTooltip;

	public class UnitUpgradeButton extends ActionButton
	{
		private var type: int;
		private var level: int;
		private var nextUnitPrototype: UnitPrototype;

		private var upgradeToolTip: UnitUpgradeTooltip;

		public function UnitUpgradeButton(parentObj: SimpleGameObject, unitPrototype: UnitPrototype, maxLevel: int)
		{
			super(parentObj, unitPrototype.getName());

			if (!unitPrototype)
			return;

			nextUnitPrototype = unitPrototype.level >= maxLevel?null:UnitFactory.getPrototype(unitPrototype.type, (unitPrototype.level + 1));

			upgradeToolTip = new UnitUpgradeTooltip(parentObj as StructureObject, unitPrototype, nextUnitPrototype);

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		public function onMouseOver(event: MouseEvent):void
		{
			upgradeToolTip.show(this);
		}

		public function onMouseOut(event: MouseEvent):void
		{
			upgradeToolTip.hide();
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				Global.mapComm.Troop.upgradeUnit(parentObj.groupId, parentObj.objectId, nextUnitPrototype.type);
			}
		}

		override public function validateButton(): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.groupId);
			if (city == null) {
				Util.log("UnitUpgradeButton.validateButton: Unknown city");
				return true;
			}

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				Util.log("UnitUpgradeButton.validateButton: Unknown city object");
				return true;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.validate(parentObj, effects);

			upgradeToolTip.missingRequirements = missingReqs;
			upgradeToolTip.draw(currentCount, parentAction.maxCount);

			var unitUpgradeAction: UnitUpgradeAction = parentAction as UnitUpgradeAction;

			if (nextUnitPrototype == null || nextUnitPrototype.level > unitUpgradeAction.maxlevel)
			{
				upgradeToolTip.draw(currentCount, parentAction.maxCount);
				disable();
				return false;
			}

			city = Global.map.cities.get(parentObj.groupId);

			if (city == null)
			{
				disable();
				return false;
			}

			if (missingReqs != null && missingReqs.length > 0)
			{
				disable();
				return false;
			}

			if (city.resources.GreaterThanOrEqual(Formula.unitUpgradeCost(city, nextUnitPrototype)))
			{
				enable();
				return true;
			}
			else
			{
				disable();
				return false;
			}
		}
	}

}

