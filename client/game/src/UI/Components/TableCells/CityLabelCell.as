package src.UI.Components.TableCells
{
    import org.aswing.*;
    import org.aswing.table.*;

    import src.UI.Components.*;

    public class CityLabelCell extends AbstractTableCell
	{
		// We have to create a wrapper because the cells ui will be forced to the entire area of the container
		protected var wrapper: JPanel;

		private var cityLabel: CityLabel;
		
		public function CityLabelCell()
		{
			super();
			
			wrapper = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			wrapper.setOpaque(true);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			if (cityLabel)				
				wrapper.removeAll();			
			
			cityLabel = new CityLabel(value.cityId, value.cityName);						
			
			wrapper.append(cityLabel);
		}
		
		override public function getCellValue():*
		{
			return value;
		}

		override public function getCellComponent():Component
		{
			return wrapper;
		}		
	}

}

