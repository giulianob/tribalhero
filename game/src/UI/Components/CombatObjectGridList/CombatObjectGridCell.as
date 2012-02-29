﻿package src.UI.Components.CombatObjectGridList
{
import flash.events.Event;
import flash.filters.DropShadowFilter;
import flash.text.TextField;
import flash.text.TextFieldAutoSize;
import flash.text.TextFormat;
import flash.text.TextFormatAlign;
import org.aswing.*;
import org.aswing.border.*;
import org.aswing.dnd.DragManager;
import org.aswing.dnd.SourceData;
import org.aswing.event.DragAndDropEvent;
import org.aswing.geom.*;
import org.aswing.colorchooser.*;
import org.aswing.ext.*;
import src.Objects.Prototypes.StructurePrototype;
import src.Objects.Prototypes.UnitPrototype;
import src.Util.Util;

public class CombatObjectGridCell extends JLabel implements GridListCell{
		
	protected var value: * ;
	
	private var uiTextField: TextField = new TextField();
	
	public function CombatObjectGridCell(){
		super();
							  
		setBorder(new LineBorder(null, ASColor.BLACK, 1));
		
		setPreferredSize(new IntDimension(60, 60));
		setBackground(ASColor.WHITE);
				
		uiTextField.text = "N/A";
 		uiTextField.autoSize = TextFieldAutoSize.LEFT;		
 		uiTextField.selectable = false;
 		uiTextField.mouseEnabled = false;
 		uiTextField.mouseWheelEnabled = false;
		uiTextField.y = getPreferredHeight() - uiTextField.textHeight - 2;
		
		addChild(uiTextField);
		
		setOpaque(true);		
	}
	
	public function setCellValue(value:*):void{
		this.value = value;		
		
		setIcon(new AssetIcon(value.source));		
		
		if (value.prototype is UnitPrototype)		
			uiTextField.text = Math.ceil(Number(value.data.hp) / value.data.maxHp).toString();		
		else if (value.prototype is StructurePrototype)
			uiTextField.text = Util.roundNumber(value.data.hp,0) + "/" + value.data.maxHp;
			
		uiTextField.x = (getPreferredWidth() / 2) - (uiTextField.textWidth / 2);
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