package src.Map.MiniMapFilters
{
    import flash.display.*;
    import flash.geom.*;

    import src.Constants;
    import src.Global;
    import src.Map.MiniMap.MiniMapLegend;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Util.StringHelper;

	public class MiniMapRegionFilterTribe extends MiniMapRegionFilter
	{
		override public function getName(): String {
			return "Tribe";
		}

		override public function applyCity(obj: MiniMapRegionObject) : void {
			obj.setIcon(new DOT_SPRITE);
			
			if (Global.map.cities.get(obj.groupId)) {
				obj.transform.colorTransform = new ColorTransform();
			} else if(Constants.tribe.isInTribe(obj.extraProps.tribeId)) {
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
			} else if (obj.extraProps.tribeId>0) {
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			} else {
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			}
		}
		
		override public function applyLegend(legend: MiniMapLegend) : void {
			var icon: DisplayObject = new DOT_SPRITE;
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_CITY"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_TRIBE_SELF"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_IN_TRIBE"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_TRIBE_OTHER_NO_TRIBE"));
			
			legend.setLegendTitle(StringHelper.localize("MINIMAP_LEGEND_TRIBE"));
		}
	}
}