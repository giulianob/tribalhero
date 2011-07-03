
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.*;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Effects.Formula;
	import src.Util.Util;
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.Cursors.*;
	import src.UI.Dialog.UnitTrainDialog;
	import src.UI.Tooltips.TrainTooltip;

	public class TrainButton extends ActionButton
	{
		private var unitPrototype: UnitPrototype;

		private var trainToolTip: TrainTooltip;

		public function TrainButton(parentObj: SimpleGameObject, unitPrototype: UnitPrototype)
		{
			super(parentObj, unitPrototype.getName());

			this.unitPrototype = unitPrototype;

			trainToolTip = new TrainTooltip(parentObj as StructureObject, unitPrototype);

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		public function onMouseOver(event: MouseEvent):void
		{
			trainToolTip.show(this);
		}

		public function onMouseOut(event: MouseEvent):void
		{
			trainToolTip.hide();
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				var city: City = Global.map.cities.get(parentObj.groupId);

				if (!city)
				{
					disable();
					return;
				}

				var maxValue: int = city.resources.Div(Formula.unitTrainCost(city, unitPrototype));

				if (maxValue == 0)
				{
					disable();
					return;
				}

				var inputDialog: UnitTrainDialog = new UnitTrainDialog(parentObj as StructureObject, unitPrototype, onAcceptDialog, Formula.trainTime(parentObj as StructureObject, unitPrototype.trainTime, (parentObj as StructureObject).getCorrespondingCityObj().techManager));

				inputDialog.show();
			}
		}

		public function onAcceptDialog(sender: UnitTrainDialog):void
		{
			sender.getFrame().dispose();
			Global.mapComm.Troop.trainUnit(parentObj.groupId, parentObj.objectId, unitPrototype.type, sender.getAmount().getValue());
		}

		override public function validateButton(): Boolean
		{
			if (unitPrototype == null)
			{
				return true;
			}

			var city: City = Global.map.cities.get(parentObj.groupId);

			if (city == null)
			{
				return false;
			}

			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			if (parentCityObj == null) {
				Util.log("TrainButton.validateButton: Unknown city object");
				return true;
			}

			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.getMissingRequirements(parentObj, effects);

			trainToolTip.missingRequirements = missingReqs;
			trainToolTip.draw();

			if (missingReqs != null && missingReqs.length > 0)
			{
				return false;
			}

			return city.resources.GreaterThanOrEqual(Formula.unitTrainCost(city, unitPrototype));
		}
	}

}

