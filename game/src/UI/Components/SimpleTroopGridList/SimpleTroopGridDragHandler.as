package src.UI.Components.SimpleTroopGridList
{
	import flash.events.Event;
	import flash.ui.Keyboard;
	import flash.ui.KeyLocation;
	import org.aswing.Component;
	import org.aswing.dnd.DragListener;
	import org.aswing.dnd.DragManager;
	import org.aswing.dnd.RejectedMotion;
	import org.aswing.event.DragAndDropEvent;
	import org.aswing.VectorListModel;
	import src.Global;
	import src.UI.Dialog.NumberInputDialog;

	/**
	 * ...
	 * @author Giuliano
	 */
	public class SimpleTroopGridDragHandler implements DragListener
	{
		private var tilelists: Array = [];

		public function SimpleTroopGridDragHandler(tilelists: Array) {
			for each (var tilelist: SimpleTroopGridList in tilelists)
			{
				tilelist.setDragEnabled(true);
				tilelist.setDropTrigger(true);

				tilelist.addEventListener(DragAndDropEvent.DRAG_OVERRING, onDragOverring);
				tilelist.addEventListener(DragAndDropEvent.DRAG_ENTER, onDragEnter);
				tilelist.addEventListener(DragAndDropEvent.DRAG_EXIT, onDragExit);
				tilelist.addEventListener(DragAndDropEvent.DRAG_START, onDragStart);
				tilelist.addEventListener(DragAndDropEvent.DRAG_DROP, onDragDrop);

				this.tilelists.push(tilelist);
				for (var i: int = 0; i < tilelist.getModel().getSize(); i++)
					addToAllTileLists(tilelist, tilelists, tilelist.getCellByIndex(i) as Component);
			}
		}

		private function addToAllTileLists(parent: SimpleTroopGridList, tilelists: Array, c: Component): void
		{
			c.setDragEnabled(true);

			for each (var tilelist: SimpleTroopGridList in tilelists) {
				if (tilelist == parent) continue;
				tilelist.addDragAcceptableInitiator(c);
			}
		}

		private function removeFromAllTileLists(c: Component): void
		{
			for each (var tilelist: SimpleTroopGridList in tilelists) {
				tilelist.removeDragAcceptableInitiator(c);
			}
		}

		public function onDragStart(e: DragAndDropEvent): void {  }

		public function onDragOverring(e: DragAndDropEvent): void {  }

		public function onDragDrop(e: DragAndDropEvent): void  {
			var targetComponent: Component = e.getTargetComponent();
			var dragInitiator: Component = e.getDragInitiator();

			if (targetComponent is SimpleTroopGridList && targetComponent.isDragAcceptableInitiator(dragInitiator)) {
				var cell: SimpleTroopGridCell = dragInitiator as SimpleTroopGridCell;
				var sourceGrid: SimpleTroopGridList = (dragInitiator as SimpleTroopGridCell).getParent().getParent() as SimpleTroopGridList;
				var targetGrid: SimpleTroopGridList = SimpleTroopGridList(targetComponent);
				var cellValue: * = cell.getCellValue();

				if (!Global.gameContainer.isKeyDown(Keyboard.SHIFT) && cellValue.data.count > 1)
				{
					var numberInput: NumberInputDialog = new NumberInputDialog("Enter amount of troops to transfer", 1, cellValue.data.count, function():void {
						numberInput.getFrame().dispose();

						// Remove from source
						if (numberInput.getAmount().getValue() >= cellValue.data.count) {
							(sourceGrid.getModel() as VectorListModel).remove(cellValue);
							removeFromAllTileLists(dragInitiator);
						}
						else
						{
							cellValue.data.count -= numberInput.getAmount().getValue();
							cell.setCellValue(cellValue);							
						}
						
						sourceGrid.dispatchEvent(new Event(SimpleTroopGridList.UNIT_CHANGED));

						// Add to target
						var newTroopCell: SimpleTroopGridCell = targetGrid.addUnit(cellValue.data.type, numberInput.getAmount().getValue());

						// Bind drag handlre to new cell item
						if (newTroopCell != null)
							addToAllTileLists(targetGrid, tilelists, newTroopCell); //new guy, add to all
					},
					cellValue.data.count);

					numberInput.show(null, true, function():void {
						DragManager.setDropMotion(new RejectedMotion());
					});
				}
				else
				{
					// Remove from source
					(sourceGrid.getModel() as VectorListModel).remove(cellValue);
					removeFromAllTileLists(dragInitiator);

					sourceGrid.dispatchEvent(new Event(SimpleTroopGridList.UNIT_CHANGED));
					
					// Add to target
					var newTroopCell: SimpleTroopGridCell = targetGrid.addUnit(cellValue.data.type, cellValue.data.count);

					// Bind drag handler to new cell item
					if (newTroopCell != null)
						addToAllTileLists(targetGrid, tilelists, newTroopCell); //new guy, add to all
				}
			}else{
				DragManager.setDropMotion(new RejectedMotion());
			}
		}

		public function onDragEnter(e: DragAndDropEvent): void  {
		}

		public function onDragExit(e: DragAndDropEvent): void  {
		}
	}

}
