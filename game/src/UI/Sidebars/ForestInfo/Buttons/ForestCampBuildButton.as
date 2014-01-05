
package src.UI.Sidebars.ForestInfo.Buttons {
import System.Linq.Enumerable;

	import flash.events.Event;
	import flash.events.MouseEvent;

	import src.Global;
import src.Map.CityObject;
    import src.Objects.Actions.ActionButton;
	import src.Objects.Actions.ForestCampBuildAction;
import src.Objects.Effects.Formula;
	import src.Objects.Factories.*;
	import src.Objects.Forest;
	import src.Objects.SimpleGameObject;
	import src.UI.Cursors.*;
	import src.UI.Dialog.ForestLaborDialog;
import src.UI.Dialog.InfoDialog;
import src.UI.Tooltips.TextTooltip;
import src.Util.StringHelper;

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
                var obj: * = Enumerable.from(Global.gameContainer.selectedCity.objects).firstOrNone(function(obj: CityObject): Boolean {
                    return ObjectFactory.isType("Lumbermill", obj.type);
                });

                var lumbermill: CityObject = obj.isNone?null:obj.value;
                if(lumbermill==null) {
                    InfoDialog.showMessageDialog("Error",StringHelper.localize("FOREST_REQUIRE_LUMBERMILL"));
                    return;
                }
                var max: int = Formula.maxLumbermillLabor(lumbermill.level);
                var limit: int = max-Enumerable.from(Global.gameContainer.selectedCity.objects).where(function(obj: CityObject): Boolean {
                    return ObjectFactory.isType("ForestCamp", obj.type);
                }).sum(function(obj: CityObject): int {
                    return obj.labor;
                });

                if(limit<=0) {
                    InfoDialog.showMessageDialog("Error",StringHelper.localize("FOREST_LABOR_LIMIT",max));
                    return;
                }
                // Check to see if this is being called from the forest or from the lumbermill. If this is from the forest, then the parent action will be null
				var forestCampBuildAction: ForestCampBuildAction = parentAction as ForestCampBuildAction;

                if (forestCampBuildAction == null) {
					var laborDialog: ForestLaborDialog = new ForestLaborDialog(Global.gameContainer.selectedCity.id, parentObj as Forest, lumbermill.level, limit, onSetLabor);
					laborDialog.show();
				} else {									
					new GroundForestCursor(function(forest: Forest) : void {
						var laborDialog: ForestLaborDialog = new ForestLaborDialog(parentObj.groupId, forest, lumbermill.level, limit, onSetLabor);
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

