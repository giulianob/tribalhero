
package src.UI.Sidebars.ForestInfo.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Objects.Actions.ForestCampBuildAction;
	import src.Objects.Factories.*;
	import src.Objects.Forest;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.SimpleGameObject;
	import src.Objects.Troop.*;
	import src.UI.Cursors.*;
	import src.UI.Dialog.ForestLaborDialog;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Tooltips.TextTooltip;

	public class ForestCampBuildButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		public function ForestCampBuildButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Gather Lumber");

			tooltip = new TextTooltip("Gather Lumber");

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		public function onMouseOver(event: MouseEvent):void
		{
			tooltip.show(this);
		}

		public function onMouseOut(event: MouseEvent):void
		{
			tooltip.hide();
		}

		public function onMouseClick(event: Event):void
		{
			if (isEnabled())
			{
				// Check to see if this is being called from the forest or from the lumbermill. If this is from the forest, then the parent action will be null
				var forestCampBuildAction: ForestCampBuildAction = parentAction as ForestCampBuildAction;

				var campTypes: Array = ObjectFactory.getList("ForestCamp");
				if (forestCampBuildAction == null) {
					var laborDialog: ForestLaborDialog = new ForestLaborDialog(Global.gameContainer.selectedCity.id, parentObj as Forest, onSetLabor);
					laborDialog.show();
				} else {									
					var cursor: GroundForestCursor = new GroundForestCursor(function(forest: Forest) : void {
						var laborDialog: ForestLaborDialog = new ForestLaborDialog(parentObj.groupId, forest, onSetLabor);
						laborDialog.show();
					});

				}
			}

			event.stopImmediatePropagation();
		}

		private function onSetLabor(dlg: ForestLaborDialog) : void {
			// This is kind of a hack since we need to know the campType.
			var campTypes: Array = ObjectFactory.getList("ForestCamp");
			
			Global.mapComm.Objects.createForestCamp(dlg.getForest().objectId , dlg.city.id, campTypes[0], dlg.getCount());

			dlg.getFrame().dispose();
		}
	}

}

