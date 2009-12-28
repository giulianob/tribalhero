
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.MovieClip;
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Map.Map;
	import src.Objects.*;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.Cursors.*;
	import src.UI.Dialog.NumberInputDialog;
	import src.UI.Tooltips.TrainTooltip;
	
	public class TrainButton extends ActionButton
	{				
		private var unitPrototype: UnitPrototype;
		
		private var trainToolTip: TrainTooltip;
		
		public function TrainButton(button: SimpleButton, parentObj: GameObject, unitPrototype: UnitPrototype)
		{				
			super(button, parentObj);
			
			this.unitPrototype = unitPrototype;
			
			trainToolTip = new TrainTooltip(parentObj as StructureObject, unitPrototype);			
			
			ui.addEventListener(MouseEvent.CLICK, onMouseClick);
			ui.addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);						
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
			if (enabled)
			{				
				var city: City = Global.map.cities.get(parentObj.cityId);
				
				if (!city)
				{
					disable();
					return;
				}
					
				var maxValue: int = city.resources.Div(unitPrototype.trainResources);
				
				if (maxValue == 0)
				{
					disable();
					return;
				}
				
				var inputDialog: NumberInputDialog = new NumberInputDialog("How many " + unitPrototype.getName() + " would you like to train?", 1, maxValue, onAcceptDialog, 1, unitPrototype.trainResources);
				
				inputDialog.show();
			}
		}
		
		public function onAcceptDialog(sender: NumberInputDialog):void
		{
			sender.getFrame().dispose();
			Global.map.mapComm.Troop.trainUnit(this.parentObj.cityId, this.parentObj.objectId, unitPrototype.type, sender.getAmount().getValue());
		}		
		
		override public function validateButton(): Boolean
		{						
			if (unitPrototype == null)
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
				trace("TrainButton.validateButton: Unknown city object");
				return true;
			}
			
			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingReqs: Array = parentAction.validate(parentObj, effects);
			
			trainToolTip.missingRequirements = missingReqs;
			trainToolTip.draw(currentCount, parentAction.maxCount);
			
			if (missingReqs != null && missingReqs.length > 0)
			{
				disable();
				return false;
			}
			
			if (city.resources.GreaterThanOrEqual(unitPrototype.trainResources))
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
