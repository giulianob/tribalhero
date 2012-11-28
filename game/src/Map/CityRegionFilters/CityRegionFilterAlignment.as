package src.Map.CityRegionFilters 
{
	import src.Util.StringHelper;
	import src.Map.CityRegionFilters.CityRegionFilter;
	import src.Map.CityRegionLegend;
	import src.Map.CityRegionObject;
	import src.Objects.Factories.ObjectFactory;
	import flash.geom.*;
	import src.UI.Tooltips.MinimapInfoTooltip;
	import flash.events.*;
	import src.Global;
	import flash.display.*;
	/**
	 * ...
	 * @author Anthony Lam
	 */
	public class CityRegionFilterAlignment extends CityRegionFilter 
	{	
		override public function getName(): String {
			return "Alignment";
		}

		override public function applyCity(obj: CityRegionObject) : void {
			// If it's our city, we just show a special flag
			var img: DisplayObject;

			img = new DOT_SPRITE;
			obj.sprite = img;
			
			var alignment: Number = obj.extraProps.alignment;
			var alignmentIdx: int;
			if (alignment <= 20) alignmentIdx = 0;
			else if (alignment <= 40) alignmentIdx = 1;
			else if (alignment <= 60) alignmentIdx = 2;
			else if (alignment <= 80) alignmentIdx = 3;
			else alignmentIdx = 4;

			obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[alignmentIdx].r, DEFAULT_COLORS[alignmentIdx].g, DEFAULT_COLORS[alignmentIdx].b);
			obj.addChild(img);
		}
		
		override public function applyLegend(legend: CityRegionLegend) : void {
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