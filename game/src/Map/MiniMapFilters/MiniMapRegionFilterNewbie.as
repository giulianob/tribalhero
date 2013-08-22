package src.Map.MiniMapFilters
{
    import flash.display.*;
    import flash.geom.*;

    import src.Global;
    import src.Map.MiniMap.MiniMapLegend;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Util.StringHelper;

    /**
	 * ...
	 * @author Anthony Lam
	 */
	public class MiniMapRegionFilterNewbie extends MiniMapRegionFilter
	{
		override public function getName(): String {
			return "Newbie";
		}

		override public function applyCity(obj: MiniMapRegionObject) : void {
			// If it's our city, we just show a special flag
			var img: DisplayObject;

			img = new DOT_SPRITE;
			obj.sprite = img;
			
			if (Global.map.cities.get(obj.groupId)) {
				obj.transform.colorTransform = new ColorTransform();
			} else if(obj.extraProps.isNewbie) {
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
			} else {
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			}
			obj.addChild(img);
		}
		
		override public function applyLegend(legend: MiniMapLegend) : void {
			var icon: DisplayObject = new DOT_SPRITE;
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_CITY"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_NEWBIE_YES"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_NEWBIE_NO"));
			
			legend.setLegendTitle(StringHelper.localize("MINIMAP_LEGEND_NEWBIE"));
		}
	}
}