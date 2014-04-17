package src.UI.Components.TroopsDialogTable 
{
    import flash.events.MouseEvent;

    import org.aswing.*;
    import org.aswing.event.*;

    import src.UI.Components.TableCells.*;
    import src.UI.Components.TroopCompositionGridList.*;
    import src.UI.Tooltips.*;
    import src.Util.*;

    public class TroopUnitsCell extends AbstractPanelTableCell
	{
		private var unitGrid: TroopCompositionGridList;
		
		private var lblMoreUnits: JLabel;
		
		private var moreTextSize: int;	
		
		private var tooltip: TroopStubTooltip;
		
		public function TroopUnitsCell() 
		{
			super();		
			
			lblMoreUnits = new JLabel("", null, AsWingConstants.LEFT);
			moreTextSize = Math.ceil(StringHelper.calculateTextWidth(lblMoreUnits.getFont(), StringHelper.localize("TROOP_MORE_UNITS", 99999)));
			
			getCellPanel().addEventListener(ResizedEvent.RESIZED, function(t: ResizedEvent): void {
				recalcSize();
			});								
									
			unitGrid = new TroopCompositionGridList(null, 1, 1);
			
			getCellPanel().appendAll(unitGrid, lblMoreUnits);
			
			
			unitGrid.addEventListener(MouseEvent.ROLL_OVER, showTooltip);
			unitGrid.addEventListener(MouseEvent.ROLL_OUT, hideTooltip);
			lblMoreUnits.addEventListener(MouseEvent.ROLL_OVER, showTooltip);
			lblMoreUnits.addEventListener(MouseEvent.ROLL_OUT, hideTooltip);
		}
		
		private function hideTooltip(e:MouseEvent = null):void 
		{
			if (!tooltip) {
				return;
			}
			
			tooltip.hide();
		}
		
		private function showTooltip(e:MouseEvent = null):void 
		{
			if (!tooltip) {
				return;
			}
			
			tooltip.show(getCellPanel());
		}
		
		override public function setCellValue(value:*):void 
		{
			// Ignore so we don't redraw the tooltip accidentally
			if (value == getCellValue()) {
				return;
			}
			
			super.setCellValue(value);		
			
			unitGrid.setTroop(value);
			
			hideTooltip();
			
			tooltip = new TroopStubTooltip(value);
			
			recalcSize();
		}
		
		private function getMaximumCols(): int {
			return getCellPanel().getWidth() / unitGrid.getTileWidth();
		}
		
		private function recalcSize(): void {			
			var maxVisibleItems: int = Math.floor(getCellPanel().getWidth() / unitGrid.getTileWidth());
			
			unitGrid.setVisible(false);
			lblMoreUnits.setVisible(false);
			
			if (unitGrid.getModel().getSize() > maxVisibleItems) {
				maxVisibleItems = Math.max(0, Math.floor((getCellPanel().getWidth() - moreTextSize) / unitGrid.getTileWidth()));
				
				lblMoreUnits.setVisible(true);
				if (maxVisibleItems == 0) {					
					lblMoreUnits.setText(StringHelper.localize("TROOP_UNITS", getCellValue().getIndividualUnitCount()));
				}
				else {					
					unitGrid.setVisible(true);
					
					var unitsHidden: int = 0;
					for (var i: int = maxVisibleItems; i < unitGrid.getModel().getSize(); i++) {
						var item: * = unitGrid.getModel().getElementAt(i);
						unitsHidden += item.data.count;
					}
					
					lblMoreUnits.setText(StringHelper.localize("TROOP_MORE_UNITS", unitsHidden));					
				}
			}
			else {
				unitGrid.setVisible(true);
			}
			
			unitGrid.setPreferredWidth(Math.min(maxVisibleItems * unitGrid.getTileWidth(), unitGrid.getModel().getSize() * unitGrid.getTileWidth()));
			unitGrid.setColumns(maxVisibleItems);
		}
	}

}