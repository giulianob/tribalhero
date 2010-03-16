package src.UI.Components.NotificationGridList 
{
	import flash.display.DisplayObjectContainer;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import flash.utils.Timer;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.MapUtil;
	import src.Objects.Actions.Notification;
	import src.Objects.Actions.PassiveAction;
	import src.Objects.Factories.TroopFactory;
	import src.Objects.Factories.WorkerFactory;
	import src.Objects.Prototypes.Worker;
	import src.Objects.Troop.TroopStub;
	import src.UI.Components.SimpleTooltip;
	import src.Util.Util;

public class NotificationGridCell extends JLabel implements GridListCell{	
	
	private var value: * ;	
	
	public function NotificationGridCell() 
	{				
		buttonMode = true;
	}
	
	public function setCellValue(value:*):void{
		this.value = value;
		
		setIcon(new AssetIcon(TroopFactory.getStateSprite(TroopStub.IDLE)));
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
