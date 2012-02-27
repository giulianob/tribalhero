﻿
package src.UI.Sidebars.ObjectInfo.Buttons {
	import fl.lang.Locale;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Actions.Action;
	import src.Objects.Actions.BuildAction;
	import src.Objects.Actions.CurrentActiveAction;
	import src.Objects.Actions.StructureUpgradeAction;
	import src.Objects.Effects.Formula;
	import src.Objects.Factories.*;
	import src.Objects.*;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Prototypes.*
	import src.Util.Util;
	import src.UI.Cursors.*;
	import src.UI.Tooltips.StructureBuildTooltip;

	public class BuildButton extends ActionButton
	{
		private var structPrototype: StructurePrototype;
		private var buildToolTip: StructureBuildTooltip;

		public function BuildButton(parentObj: SimpleGameObject, structPrototype: StructurePrototype)
		{
			super(parentObj, structPrototype.getName());

			this.structPrototype = structPrototype;

			buildToolTip = new StructureBuildTooltip(parentObj as StructureObject, structPrototype);

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		public function onMouseOver(event: MouseEvent):void
		{
			buildToolTip.show(this);
		}

		public function onMouseOut(event: MouseEvent):void
		{
			buildToolTip.hide();
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				var cursor: BuildStructureCursor = new BuildStructureCursor();
				cursor.init((parentAction as BuildAction).type, (parentAction as BuildAction).level, (parentAction as BuildAction).tilerequirement, parentObj);//hardcoded here to always create level 1
			}
		}

		override public function validateButton(): Boolean
		{
			if (structPrototype == null)
			{
				enable();
				return true;
			}

			var city: City = Global.map.cities.get(parentObj.groupId);

			if (city == null)
			{
				return false;
			}

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				Util.log("StructureChangeButton.validateButton: Unknown city object");
				return true;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.getMissingRequirements(parentObj, effects);
			
			// Enforce only two building/upgrade at a time for structures that arent marked as UnlimitedBuilding			
			if (!ObjectFactory.isType("UnlimitedBuilding", structPrototype.type)) {
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
				
				var concurrentUpgrades: int = Formula.concurrentBuildUpgrades(city.MainBuilding.level);				
				if (currentCount >= concurrentUpgrades)
					missingReqs.push(EffectReqPrototype.asMessage(Locale.loadString("CONCURRENT_UPGRADE_" + concurrentUpgrades)));					
			}

			buildToolTip.missingRequirements = missingReqs;
			buildToolTip.draw();

			if (missingReqs != null && missingReqs.length > 0)
			{
				return false;
			}

			return city.resources.GreaterThanOrEqual(Formula.buildCost(city, structPrototype));
		}
	}

}

