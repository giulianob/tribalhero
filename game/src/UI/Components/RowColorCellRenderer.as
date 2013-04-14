package src.UI.Components {
	    
	import fl.controls.listClasses.CellRenderer;

    import fl.controls.listClasses.ICellRenderer;

	public class RowColorCellRenderer extends CellRenderer implements ICellRenderer {
		
		public function RowColorCellRenderer() {
			super();
		}
		
		public static function getStyleDefinition():Object {
            return CellRenderer.getStyleDefinition();
        }
		
		override protected function drawBackground():void {
            switch (data.rowColor) {
                case "green":
                    setStyle("upSkin", CellRenderer_upSkinGreen);
                    break;
                case "red":
                    setStyle("upSkin", CellRenderer_upSkinRed);
                    break;
                default:
					setStyle("upSkin", CellRenderer_upSkin);
                    break;
            }

            super.drawBackground();
        }
		
	}
	
}

