
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.Event;
    import flash.events.MouseEvent;

    import src.Global;
    import src.Map.City;
    import src.Map.CityObject;
    import src.Objects.*;
    import src.FeathersUI.Controls.ActionButton;
    import src.UI.Dialog.InfoDialog;
    import src.UI.Dialog.TribeContributeDialog;
    import src.UI.Tooltips.SimpleRequirementTooltip;

    public class TribeContributeButton extends ActionButton
	{
		private var toolTip: SimpleRequirementTooltip;
		private var pnlGetPrices: InfoDialog;

		public function TribeContributeButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Contribute Resources");			
			
			toolTip = new SimpleRequirementTooltip(this, "Send resources to your tribe");			

			addEventListener(MouseEvent.CLICK, onMouseClick);
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled)
			{						
				var dlg: TribeContributeDialog = new TribeContributeDialog(parentObj as StructureObject, function(dlg: TribeContributeDialog) : void {
					dlg.getFrame().dispose();
				});
				
				dlg.show();
			}
		}

		override public function validateButton():Boolean 
		{
			var city: City = Global.map.cities.get(parentObj.groupId);
			var parentCityObj: CityObject = city.objects.get(parentObj.objectId);
			var effects: Array = parentCityObj.techManager.getAllEffects(parentAction.effectReqInherit);
			var missingRequirements: Array = parentAction.getMissingRequirements(parentObj, effects);
			
			toolTip.setRequirements(missingRequirements);
			toolTip.draw();
			
			return !missingRequirements || missingRequirements.length == 0;
		}
	}

}

