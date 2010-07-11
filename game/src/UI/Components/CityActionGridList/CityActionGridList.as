package src.UI.Components.CityActionGridList 
{
	import flash.display.DisplayObject;
	import flash.events.Event;
	import flash.events.TimerEvent;
	import flash.utils.Timer;
	import org.aswing.event.AWEvent;
	import org.aswing.ext.DefaultGridCell;
	import org.aswing.ext.GeneralGridListCellFactory;
	import org.aswing.ext.GridList;
	import org.aswing.VectorListModel;
	import src.Map.City;
	import src.Map.CityObject;
	import src.Objects.Actions.CurrentAction;
	import src.Objects.Actions.CurrentActionReference;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Util.BinaryList.*;
	
	/**
	 * ...
	 * @author Giuliano
	 */
	public class CityActionGridList extends GridList
	{
		private var city: City;
		private var timer: Timer;
		
		public function CityActionGridList(city: City, tileWidth: int) 
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(CityActionGridCell), 0, 2);
			
			this.city = city;
			
			setTileWidth(tileWidth/2 - 30);
			setTileHeight(60);
			setColsRows(2, 0);
			setTracksWidth(true);
			
			onUpdateActions(null);
			
			timer = new Timer(1000);
			timer.addEventListener(TimerEvent.TIMER, function(e: TimerEvent):void { updateTimes(); } );
			timer.start();
			
			city.currentActions.addEventListener(BinaryListEvent.CHANGED, onUpdateActions);
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
		
		public function onUpdateActions(e: Event): void {
			(getModel() as VectorListModel).clear();
			
			if (city.currentActions.size() == 0) {
				(getModel() as VectorListModel).append( { 'message': 'There is nothing going on...' } );
			}
			
			for each(var currentAction: CurrentAction in city.currentActions.each()) {		
				if (currentAction is CurrentActionReference) continue; //Skip action references as it should only show actions directly related to objects
				if (currentAction.workerId == 0) continue; //Skip city actions
				
				var cityObj: CityObject = city.objects.get(currentAction.workerId);
				
				var prototype: * = ObjectFactory.getPrototype(cityObj.type, cityObj.level);
				var icon: DisplayObject = ObjectFactory.getSpriteEx(cityObj.type, cityObj.level, true);
				if (prototype is StructurePrototype)
				{
					icon.scaleX = 0.50;
					icon.scaleY = 0.50;
				}
				
				(getModel() as VectorListModel).append( { 'cityObj': cityObj, 'source': icon, 'cityId': city.id , 'prototype': prototype, 'currentAction': currentAction } );
			}
		}
	}
	
}