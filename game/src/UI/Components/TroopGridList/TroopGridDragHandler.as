package src.UI.Components.TroopGridList 
{
	import org.aswing.Component;
	import org.aswing.Container;
	import org.aswing.dnd.DragListener;
	import org.aswing.dnd.DragManager;
	import org.aswing.dnd.RejectedMotion;
	import org.aswing.event.DragAndDropEvent;
	import org.aswing.VectorListModel;
	import src.UI.Dialog.NumberInputDialog;
	
	/**
	 * ...
	 * @author Giuliano
	 */
	public class TroopGridDragHandler implements DragListener
	{
		private var tilelists: Array = new Array();
		
		public function TroopGridDragHandler(tilelists: Array) {							
			for each (var tilelist: TroopGridList in tilelists)
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
		
		private function addToAllTileLists(parent: TroopGridList, tilelists: Array, c: Component): void
		{
			c.setDragEnabled(true);
			
			for each (var tilelist: TroopGridList in tilelists) {
				if (tilelist == parent) continue;
				tilelist.addDragAcceptableInitiator(c);
			}
		}
		
		private function removeFromAllTileLists(c: Component): void
		{			
			for each (var tilelist: TroopGridList in tilelists) {
				tilelist.removeDragAcceptableInitiator(c);
			}
		}		
		
		public function onDragStart(e: DragAndDropEvent): void {  }
		
		public function onDragOverring(e: DragAndDropEvent): void {  }
		
		public function onDragDrop(e: DragAndDropEvent): void  { 
			var targetComponent: Component = e.getTargetComponent();
			var dragInitiator: Component = e.getDragInitiator();
			
			if (targetComponent is TroopGridList && targetComponent.isDragAcceptableInitiator(dragInitiator)) {
				var sourceGrid: TroopGridList = (dragInitiator as TroopGridCell).getParent().getParent() as TroopGridList;
				var targetGrid: TroopGridList = TroopGridList(targetComponent);				
				var cellValue: * = (dragInitiator as TroopGridCell).getCellValue();
				
				if (cellValue.data.count > 1)
				{
					var numberInput: NumberInputDialog = new NumberInputDialog("Enter amount of troops to transfer", 1, cellValue.data.count, function():void {
						numberInput.getFrame().dispose();
						
						if (numberInput.getAmount().getValue() == cellValue.data.count) {
							(sourceGrid.getModel() as VectorListModel).remove(cellValue);
							removeFromAllTileLists(dragInitiator);
						}
						else
						{						
							cellValue.data.count -= numberInput.getAmount().getValue();
						}
						
						var newTroopCell: TroopGridCell = targetGrid.addUnit(cellValue.data.type, numberInput.getAmount().getValue());													
						
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
					(sourceGrid.getModel() as VectorListModel).remove(cellValue);
					removeFromAllTileLists(dragInitiator);	
					
					var newTroopCell: TroopGridCell = targetGrid.addUnit(cellValue.data.type, 1);
					
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