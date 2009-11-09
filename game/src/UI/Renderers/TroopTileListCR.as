package src.UI.Renderers {
	import fl.controls.listClasses.ICellRenderer;
	import fl.controls.listClasses.ImageCell;
	import flash.text.TextFormatAlign;
	import flash.text.TextFormat;

	public class TroopTileListCR extends ImageCell implements ICellRenderer{
		
		public function TroopTileListCR() {
			setStyle("upSkin", TroopTileListCR_upSkin);
			
			setStyle("textOverlayAlpha", 0.0);			
		}
		
	}
	
}