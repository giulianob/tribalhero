﻿
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
	import src.Objects.Effects.Formula;
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.Cursors.*;
	import src.UI.Dialog.NumberInputDialog;
	import src.UI.Dialog.UnitTrainDialog;
	import src.UI.Tooltips.TrainTooltip;
	
	public class TrainButton extends ActionButton
	{				
		private var unitPrototype: UnitPrototype;
		
		private var trainToolTip: TrainTooltip;
		
		public function TrainButton(parentObj: GameObject, unitPrototype: UnitPrototype)
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
				
				var inputDialog: UnitTrainDialog = new UnitTrainDialog(unitPrototype, onAcceptDialog, Formula.trainTime(parentObj, unitPrototype.trainTime, null));
				
				inputDialog.show();
			}
		}
		
		public function onAcceptDialog(sender: UnitTrainDialog):void
		{
			sender.getFrame().dispose();
			Global.mapComm.Troop.trainUnit(this.parentObj.cityId, this.parentObj.objectId, unitPrototype.type, sender.getAmount().getValue());
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
