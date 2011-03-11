
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Actions.Action;
	import src.Objects.Actions.BuildAction;
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

		public function BuildButton(parentObj: GameObject, structPrototype: StructurePrototype)
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
				cursor.init(Global.map, (parentAction as BuildAction).type, (parentAction as BuildAction).level, (parentAction as BuildAction).tilerequirement, parentObj);//hardcoded here to always create level 1
			}
		}

		override public function validateButton(): Boolean
		{
			if (structPrototype == null)
			{
				enable();
				return true;
			}

			var city: City = Global.map.cities.get(parentObj.cityId);

			if (city == null)
			{
				disable();
				return false;
			}

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				Util.log("StructureChangeButton.validateButton: Unknown city object");
				return true;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.validate(parentObj, effects);
			
			// Enforce only one building at a time
			if (city.currentActions.hasAction(Action.STRUCTURE_BUILD)) {
				missingReqs.push(EffectReqPrototype.asMessage("You can only build one structure at a time"));
			}

			buildToolTip.missingRequirements = missingReqs;
			buildToolTip.draw(currentCount, parentAction.maxCount);

			if (missingReqs != null && missingReqs.length > 0)
			{
				disable();
				return false;
			}

			if (city.resources.GreaterThanOrEqual(Formula.buildCost(city, structPrototype)))
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

