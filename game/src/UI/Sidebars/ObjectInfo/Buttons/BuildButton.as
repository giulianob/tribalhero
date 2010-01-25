
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Factories.*;
	import src.Objects.*;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Prototypes.StructurePrototype;
	import src.UI.Cursors.*;
	import src.UI.Tooltips.StructureBuildTooltip;

	public class BuildButton extends ActionButton
	{
		private var structPrototype: StructurePrototype;
		private var buildToolTip: StructureBuildTooltip;

		public function BuildButton(button: SimpleButton, parentObj: GameObject, structPrototype: StructurePrototype)
		{
			super(button, parentObj);

			this.structPrototype = structPrototype;

			buildToolTip = new StructureBuildTooltip(parentObj as StructureObject, structPrototype);

			ui.addEventListener(MouseEvent.CLICK, onMouseClick);
			ui.addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
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
			if (enabled)
			{
				var cursor: BuildStructureCursor = new BuildStructureCursor();
				cursor.init(Global.map, structPrototype.type, 1, parentObj);//hardcoded here to always create level 1
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
				trace("StructureChangeButton.validateButton: Unknown city object");
				return true;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.validate(parentObj, effects);

			buildToolTip.missingRequirements = missingReqs;
			buildToolTip.draw(currentCount, parentAction.maxCount);

			if (missingReqs != null && missingReqs.length > 0)
			{
				disable();
				return false;
			}

			if (city.resources.GreaterThanOrEqual(structPrototype.buildResources))
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

