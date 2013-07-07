package src.UI.Dialog {

	import flash.events.Event;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.City;
	import src.Map.TileLocator;
	import src.Objects.Effects.Formula;
	import src.Objects.GameError;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Components.SimpleTroopGridList.*;
	import src.UI.Components.TroopStubGridList.TroopStubGridCell;
	import src.UI.GameJPanel;
	import src.Objects.Troop.*;
	import src.Util.StringHelper;
	import src.Util.Util;

	public class AssignmentJoinDefDialog extends ReinforceTroopDialog {
		private var sourceCity:City;

		protected var assignment: *;
		protected var distance: int;
	
		public function AssignmentJoinDefDialog(city: City, onAccept: Function, assignment: *):void
		{
			super(city, onAccept, false);
			
			title = "Join Assignment";
			
			this.assignment = assignment;
			this.distance = TileLocator.distance(city.MainBuilding.x, city.MainBuilding.y, assignment.x, assignment.y);
		}

		override protected function updateSpeedInfo(e:Event = null):void 
		{
			var stub: TroopStub = getTroop();			
			if (stub.getIndividualUnitCount() == 0) {
				lblTroopSpeed.setText(StringHelper.localize("TROOP_CREATE_DRAG_HINT")) 
			}
			else {				
				var moveTime: int = Formula.moveTimeTotal(city, stub.getSpeed(city), distance, false);
				if (Global.map.getServerTime() + moveTime > assignment.endTime) {
					var diff: int = Global.map.getServerTime() + moveTime - assignment.endTime;
					lblTroopSpeed.setText("Your units will be "+ Util.niceTime(diff)+" late. Choose faster units to arrive on time.");
				}
				else {
					lblTroopSpeed.setText(StringHelper.localize("TROOP_CREATE_DRAG_HINT"));
				}
			}
		}
	}

}

