﻿
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Actions.Action;
	import src.Objects.Actions.BuildAction;
	import src.Objects.Actions.CurrentActiveAction;
	import src.Objects.Actions.StructureUpgradeAction;
	import src.Objects.Effects.Formula;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.SimpleGameObject;
	import src.Util.Util;
	import src.Objects.Factories.StructureFactory;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Prototypes.EffectReqPrototype;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.StructureObject;
	import src.UI.Cursors.*;
	import src.UI.Tooltips.StructureUpgradeTooltip;

	public class StructureUpgradeButton extends ActionButton
	{
		private var nextStructPrototype: StructurePrototype;

		private var upgradeToolTip: StructureUpgradeTooltip;

		public function StructureUpgradeButton(parentObj: SimpleGameObject, structPrototype: StructurePrototype)
		{
			super(parentObj, structPrototype.getName(), new AssetIcon(new ICON_UPGRADE()));

			if (!structPrototype)
				return;

			nextStructPrototype = StructureFactory.getPrototype(structPrototype.type, (structPrototype.level + 1));

			upgradeToolTip = new StructureUpgradeTooltip(parentObj as StructureObject, structPrototype, nextStructPrototype);

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
				Global.mapComm.Object.upgradeStructure(parentObj.groupId, parentObj.objectId);
			}
		}

		override public function validateButton(): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.groupId);
			if (city == null) {
				Util.log("StructureUpgradeButton.validateButton: Unknown city");
				disable();
				return false;
			}

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				Util.log("StructureUpgradeButton.validateButton: Unknown city object");
				return true;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.validate(parentObj, effects);

			if (nextStructPrototype == null)
			{
				upgradeToolTip.draw(currentCount, parentAction.maxCount);
				disable();
				return false;
			}

			// Enforce only 2 build/upgrades at a time
			if (!ObjectFactory.isType("UnlimitedBuilding", nextStructPrototype.type)) {
				var currentBuildActions: Array = city.currentActions.getActions();
				var currentCount: int = 0;
				for each (var currentAction: * in currentBuildActions) {			
					if (!(currentAction is CurrentActiveAction))
						continue;
					else if (currentAction.getAction() is BuildAction) {
						var buildAction: BuildAction = currentAction.getAction();
						if (!ObjectFactory.isType("UnlimitedBuilding", buildAction.type))
							currentCount++;
					}
					else if (currentAction.getAction() is StructureUpgradeAction)
						currentCount++;
				}
				
				if (currentCount >= 2)
					missingReqs.push(EffectReqPrototype.asMessage("You can only build/upgrade two structures at a time"));
			}			
			
			upgradeToolTip.missingRequirements = missingReqs;
			upgradeToolTip.draw(currentCount, parentAction.maxCount);
			
			if (missingReqs != null && missingReqs.length > 0)
			{
				disable();
				return false;
			}

			if (city.resources.GreaterThanOrEqual(Formula.buildCost(city, nextStructPrototype)))
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

