package src.UI.Components.CombatObjectGridList
{
    import flash.text.TextField;
    import flash.text.TextFieldAutoSize;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.Prototypes.UnitPrototype;

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
			uiTextField.text = Math.ceil(value.data.hp / value.data.maxHp).toString();		
		else if (value.prototype is StructurePrototype)
			uiTextField.text = Math.ceil(value.data.hp) + "/" + Math.ceil(value.data.maxHp);
			
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