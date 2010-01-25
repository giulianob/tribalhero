
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Actions.UnitUpgradeAction;
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

		public function UnitUpgradeButton(button: SimpleButton, parentObj: GameObject, unitPrototype: UnitPrototype)
		{
			super(button, parentObj);

			if (!unitPrototype)
			return;

			nextUnitPrototype = UnitFactory.getPrototype(unitPrototype.type, (unitPrototype.level + 1));

			upgradeToolTip = new UnitUpgradeTooltip(parentObj as StructureObject, unitPrototype, nextUnitPrototype);

			ui.addEventListener(MouseEvent.CLICK, onMouseClick);
			ui.addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
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
			if (enabled)
			{
				Global.map.mapComm.Troop.upgradeUnit(parentObj.cityId, parentObj.objectId, nextUnitPrototype.type);
			}
		}

		override public function validateButton(): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.cityId);
			if (city == null) {
				trace("UnitUpgradeButton.validateButton: Unknown city");
				return true;
			}

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				trace("UnitUpgradeButton.validateButton: Unknown city object");
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

			city = Global.map.cities.get(parentObj.cityId);

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

			if (city.resources.GreaterThanOrEqual(nextUnitPrototype.upgradeResources))
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

