/**
* ...
* @author Default
* @version 0.1
*/

package src.UI.Dialog {
	import fl.controls.ComboBox;
	import fl.controls.List;
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.events.KeyboardEvent;
	import flash.events.MouseEvent;
	import flash.display.SimpleButton;
	import flash.text.TextField;	
	import flash.ui.Keyboard;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.Factories.StructureFactory;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.StructureObject;
	import fl.controls.NumericStepper;

	public class DefaultActionDialog extends Dialog {		
		private var ui: DefaultActionDialog_base = new DefaultActionDialog_base();
		
		private var values : Array = new Array();
		
		public function DefaultActionDialog()
		{
			addChild(ui);
			resize(330, 260);
		}
		
		public function init(map: Map, structure: StructureObject, onAccept: Function, onClose: Function):void {
			setOnAccept(onAccept, ui.btnAccept);
			setOnClose(onClose);			
			
			var city: City = map.cities.get(structure.cityId);
			
			var structPrototype: StructurePrototype = StructureFactory.getPrototype(structure.type, structure.level);
			
			if (!city || !structPrototype)
				return;
			
			ui.btnAdd.addEventListener(MouseEvent.CLICK, onParamAdd);
		}
		
		public function onParamAdd(event: MouseEvent):void
		{
			values.push( { type:ui.cbxType.value, value:ui.textValue.text } );
			ui.textValues.text += ui.cbxType.value + "(" + ui.textValue.text + ")\n";
		}
		
		public function onKeyPress(event: KeyboardEvent):void
		{			
			if (event.keyCode == Keyboard.ENTER)
				onAcceptDialog(null);
		}
		
		public function Command(): int
		{
			return parseInt(ui.textCommand.text);
		}
	
		public function Value(): Array
		{
			return values;
		}		
	}
	
}