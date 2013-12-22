/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.display.MovieClip;
    import flash.events.MouseEvent;

    import org.aswing.JOptionPane;

    import src.Global;
    import src.Map.City;
    import src.Objects.Actions.Action;
    import src.Objects.Actions.CurrentAction;
    import src.Objects.GameObject;
    import src.UI.Components.SimpleTooltip;
    import src.UI.Dialog.InfoDialog;

    public class CancelActionButton extends MovieClip  {

		private var ui: CancelActionButton_base = new CancelActionButton_base();
		private var id: int;
		private var parentObj: GameObject;

		public function CancelActionButton(parentObj: GameObject, id: int)
		{
			addChild(ui);

			new SimpleTooltip(ui, "Cancel");

			ui.addEventListener(MouseEvent.CLICK, onClickEvent);

			this.parentObj = parentObj;
			this.id = id;
		}

		public function onClickEvent(e: MouseEvent):void
		{
			var city: City = Global.map.cities.get(parentObj.cityId);
			
			if (city == null) return;
			
			var currentAction: CurrentAction = city.currentActions.get(id);
			
			if (currentAction == null) return;
			
			var actionType: int = currentAction.getType();		

			var diff: int = Global.map.getServerTime() - currentAction.startTime;
			
			if (diff > 60) {
				InfoDialog.showMessageDialog("Cancel Action", "Are you sure?\n" + (Action.costsToCancelActions.indexOf(actionType) >= 0? "You will only receive half of the action cost if cancelling after 60 seconds." : ""), function(result: int) : void {
					if (result == JOptionPane.YES) {
						Global.mapComm.Objects.cancelAction(parentObj.cityId, parentObj.objectId, id);
					}
				}, null, true, false, JOptionPane.YES | JOptionPane.NO);
			} else {
				Global.mapComm.Objects.cancelAction(parentObj.cityId, parentObj.objectId, id);
			}
		}

	}

}

