package src.UI.Components.SimpleTroopGridList 
{
import flash.events.Event;
import flash.filters.DropShadowFilter;
import org.aswing.*;
import org.aswing.border.*;
import org.aswing.dnd.DragManager;
import org.aswing.dnd.SourceData;
import org.aswing.event.DragAndDropEvent;
import org.aswing.geom.*;
import org.aswing.colorchooser.*;
import org.aswing.ext.*;

public class SimpleTroopGridCell extends JLabel implements GridListCell{
		
	protected var value: *;
	
	public function SimpleTroopGridCell(){
		super();
				
        addEventListener(DragAndDropEvent.DRAG_RECOGNIZED, __dragRecognized);
			  
		setBorder(new LineBorder(null, ASColor.BLACK, 1));
		
		setPreferredSize(new IntDimension(20, 20));
				
		setOpaque(true);
		setBackground(ASColor.WHITE);
	}
	
    private function __dragRecognized(e: DragAndDropEvent) : void{
        DragManager.startDrag(this, new SourceData("dragData", {"data": value.data}), new SimpleTroopDraggingImage(e.getDragInitiator()));
    }	
	
	public function setCellValue(value:*):void{
		this.value = value;
		
		setIcon(new AssetIcon(value.source));
	}
	
	public function getCellValue():*{
		return value;
	}
				
	public function getCellComponent():Component{
		return this;
	}
	
	public function setGridListCellStatus(gridList:GridList, selected:Boolean, index:int):void {		
	}
		
}
	
}