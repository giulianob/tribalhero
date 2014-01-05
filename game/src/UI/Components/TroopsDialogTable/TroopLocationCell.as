package src.UI.Components.TroopsDialogTable 
{
    import org.aswing.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.UI.Components.*;
    import src.UI.Components.TableCells.*;
    import src.Util.*;

    public class TroopLocationCell extends AbstractPanelTableCell
	{		
		private var lbl: RichLabel = new RichLabel("", 1);
		
		override protected function getCellLayout():LayoutManager 
		{
			return new BorderLayout();
		}
		
		public function TroopLocationCell()
		{
			super();
			lbl.setConstraints("Center");
			getCellPanel().append(lbl);
		}
		
		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			
			if (value.isStationed()) {
				if (value.stationedLocation.type != Location.CITY || !Global.map.cities.get(value.stationedLocation.cityId)) {
					lbl.setHtmlText(RichLabel.getHtmlForLocation(value.stationedLocation));
				}
				else {
					Global.map.usernames.cities.getUsername(value.stationedLocation.cityId, function (u: Username, custom: * = null): void {
						lbl.setHtmlText(StringHelper.localize("RICH_LABEL_LOCATION_CITY_ONLY", value.stationedLocation.cityId, u.name));
					});
				}
			}
			else {
				Global.map.usernames.cities.getUsername(value.cityId, function (u: Username, custom: * = null): void {
					lbl.setHtmlText(StringHelper.localize("RICH_LABEL_LOCATION_CITY_ONLY", value.cityId, u.name));
				});
			}
		}
	}
}