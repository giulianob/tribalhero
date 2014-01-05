package src.UI.Components.CityActionGridList
{
    import flash.display.DisplayObjectContainer;
    import flash.events.Event;
    import flash.events.TimerEvent;
    import flash.utils.Timer;

    import org.aswing.VectorListModel;
    import org.aswing.ext.GeneralGridListCellFactory;
    import org.aswing.ext.GridList;

    import src.Map.City;
    import src.Map.CityObject;
    import src.Objects.Actions.CurrentAction;
    import src.Objects.Actions.CurrentActionReference;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Util.BinaryList.*;
    import src.Util.Util;

    /**
	 * ...
	 * @author Giuliano
	 */
	public class CityActionGridList extends GridList
	{
		private var city: City;
		private var timer: Timer;

		public function CityActionGridList(city: City)
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(CityActionGridCell), 2, 0);

			this.city = city;

			setTileWidth(235);
			setTileHeight(60);
			setColsRows(2, 0);
			setTracksWidth(true);

			timer = new Timer(1000);
			timer.addEventListener(TimerEvent.TIMER, function(e: TimerEvent):void { updateTimes(); } );
			timer.start();

			setCity(city);
		}

		public function dispose(): void {
			city.currentActions.removeEventListener(BinaryListEvent.CHANGED, onUpdateActions);
			timer.stop();
		}

		private function updateTimes(): void {
			for (var i: int = 0; i < cells.size(); i++) {
				var cell: CityActionGridCell = cells.get(i);
				cell.updateTime();
			}
		}
		
		public function setCity(city: City): void {
			if (city) {
				city.currentActions.removeEventListener(BinaryListEvent.CHANGED, onUpdateActions);
			}
			
			this.city = city;			
			onUpdateActions();
			city.currentActions.addEventListener(BinaryListEvent.CHANGED, onUpdateActions);
		}

		public function onUpdateActions(e: Event = null): void {
			(getModel() as VectorListModel).clear();

			if (city.currentActions.size() == 0) {
				(getModel() as VectorListModel).append( { 'message': 'There is nothing going on...' } );
			}

			var actions: BinaryList = new BinaryList(function(a:CurrentAction, b:CurrentAction):Number {
				return a.endTime - b.endTime;
			}, function(a: CurrentAction, value: int):int {
				return a.endTime - value;
			});

			for each(var currentAction: CurrentAction in city.currentActions) {
				if (currentAction is CurrentActionReference) continue; //Skip action references as it should only show actions directly related to objects
				if (currentAction.workerId == 0) continue; //Skip city actions

				actions.add(currentAction, false);
			}

			actions.sort();

			for each(currentAction in actions) {
				var cityObj: CityObject = city.objects.get(currentAction.workerId);

				var prototype: * = ObjectFactory.getPrototype(cityObj.type, cityObj.level);
				var icon: DisplayObjectContainer = ObjectFactory.getSpriteEx(cityObj.type, cityObj.level);

				(getModel() as VectorListModel).append( { 'cityObj': cityObj, 'source': icon, 'cityId': city.id , 'objPrototype': prototype, 'currentAction': currentAction } );
			}
		}
	}

}

