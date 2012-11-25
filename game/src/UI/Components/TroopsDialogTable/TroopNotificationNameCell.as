package src.UI.Components.TroopsDialogTable 
{
	import org.aswing.*;
	import src.*;
	import src.Map.*;
	import src.UI.Components.*;
	import src.UI.Components.TableCells.*;
	import src.Util.*;
	
	public class TroopNotificationNameCell extends AbstractPanelTableCell 
	{				
		private var lbl: RichLabel = new RichLabel();
		
		public function TroopNotificationNameCell()
		{
			super();
			lbl.setConstraints("Center");
			getCellPanel().append(lbl);
		}
		
		override public function setCellValue(value:*):void 
		{
			super.setCellValue(value);
			
			Global.map.usernames.cities.getUsername(value.cityId, function (u: Username, custom: * = null): void {
				lbl.setHtmlText(StringHelper.localize("RICH_LABEL_LOCATION_CITY_ONLY", value.cityId, u.name));
			});			
		}
		
		override protected function getCellLayout():LayoutManager 
		{
			return new BorderLayout();
		}
	}
}