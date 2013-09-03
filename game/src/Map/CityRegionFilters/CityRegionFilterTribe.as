package src.Map.CityRegionFilters 
{
	import src.Util.StringHelper;
	import src.Constants;
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
	public class CityRegionFilterTribe extends CityRegionFilter 
	{
		override public function getName(): String {
			return "Tribe";
		}

		override public function applyCity(obj: CityRegionObject) : void {
			// If it's our city, we just show a special flag
			var img: DisplayObject;

			img = new DOT_SPRITE;
			obj.sprite = img;
			
			if (Global.map.cities.get(obj.groupId)) {
				obj.transform.colorTransform = new ColorTransform();
			} else if(Constants.tribe.isInTribe(obj.extraProps.tribeId)) {
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
			} else if (obj.extraProps.tribeId>0) {
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			} else {
				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			}
			obj.addChild(img);
		}
		
		override public function applyLegend(legend: CityRegionLegend) : void {
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