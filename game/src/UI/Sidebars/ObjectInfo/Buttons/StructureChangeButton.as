
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.*;
	import src.Util.Util;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Effects.Formula;
	import src.Objects.Prototypes.StructurePrototype;
	import src.UI.Cursors.*;
	import src.UI.Tooltips.StructureChangeTooltip;

	public class StructureChangeButton extends ActionButton
	{
		private var type: int;
		private var level: int;
		private var nextStructPrototype: StructurePrototype;

		private var changeToolTip: StructureChangeTooltip;

		public function StructureChangeButton(parentObj: SimpleGameObject, structPrototype: StructurePrototype, nextStructPrototype: StructurePrototype)
		{
			super(parentObj, "Convert to " + nextStructPrototype.getName());

			if (!structPrototype)
			return;

			this.nextStructPrototype = nextStructPrototype;

			changeToolTip = new StructureChangeTooltip(parentObj as StructureObject, nextStructPrototype);

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		public function onMouseOver(event: MouseEvent):void
		{
			changeToolTip.show(this);
		}

		public function onMouseOut(event: MouseEvent):void
		{
			changeToolTip.hide();
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				Global.mapComm.Objects.changeStructure(parentObj.groupId, parentObj.objectId, nextStructPrototype.type, nextStructPrototype.level);
			}
		}

		override public function validateButton(): Boolean
		{
			var city: City = Global.map.cities.get(parentObj.groupId);
			if (city == null) {
				Util.log("StructureChangeButton.validateButton: Unknown city");
				return true;
			}

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				Util.log("StructureChangeButton.validateButton: Unknown city object");
				return true;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.getMissingRequirements(parentObj, effects);

			changeToolTip.missingRequirements = missingReqs;
			changeToolTip.draw();

			if (nextStructPrototype == null)
			{
				changeToolTip.draw();
				return false;
			}

			city = Global.map.cities.get(parentObj.groupId);

			if (city == null)
			{
				return false;
			}

			if (missingReqs != null && missingReqs.length > 0)
			{
				return false;
			}

			return city.resources.GreaterThanOrEqual(Formula.buildCost(city, nextStructPrototype));
		}
	}

}

