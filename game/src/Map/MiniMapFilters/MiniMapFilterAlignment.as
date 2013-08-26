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
	public class MiniMapFilterAlignment extends MiniMapRegionFilter
	{	
		override public function getName(): String {
			return "Alignment";
		}

		override public function applyCity(obj: MiniMapRegionObject) : void {
			// If it's our city, we just show a special flag
			var img: DisplayObject;

			img = new DOT_SPRITE;
			obj.sprite = img;
			
			var alignment: Number = obj.extraProps.alignment;
			var alignmentIdx: int;
			if (Global.map.cities.get(obj.groupId)) {
				obj.transform.colorTransform = new ColorTransform();
				obj.addChild(img);				
			}
			else {
				if (alignment <= 20) alignmentIdx = 0;			
				else if (alignment <= 40) alignmentIdx = 1;
				else if (alignment <= 60) alignmentIdx = 2;
				else if (alignment <= 80) alignmentIdx = 3;
				else alignmentIdx = 4;
				
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[alignmentIdx].r, DEFAULT_COLORS[alignmentIdx].g, DEFAULT_COLORS[alignmentIdx].b);
				obj.addChild(img);				
			}
		}
		
		override public function applyLegend(legend: MiniMapLegend) : void {
			var icon: DisplayObject = new DOT_SPRITE;
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_CITY"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_90"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_75"));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[2].r, DEFAULT_COLORS[2].g, DEFAULT_COLORS[2].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_50"));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_25"));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT_10"));
			
			legend.setLegendTitle(StringHelper.localize("MINIMAP_LEGEND_ALIGNMENT"));
		}
	}
}