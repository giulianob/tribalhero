package src.Map.CityRegionFilters 
{
	import fl.lang.Locale;
	import mx.utils.StringUtil;
	import src.Map.CityRegionFilters.CityRegionFilter;
	import src.Map.CityRegionLegend;
	import src.Map.CityRegionObject;
	import src.Map.MapUtil;
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
	public class CityRegionFilterDistance extends CityRegionFilter 
	{		
		override public function getName(): String {
			return "Distance";
		}

		override public function applyCity(obj: CityRegionObject) : void {
			// If it's our city, we just show a special flag
			var img: DisplayObject;

			if (Global.map.cities.get(obj.groupId)) {
				img = new DOT_SPRITE;
				obj.sprite = img;
				obj.transform.colorTransform = new ColorTransform();
				obj.addChild(img);
			} else {
				img = new DOT_SPRITE;
				obj.sprite = img;
				
				// Apply the difficulty transformation to the tile
				var point: Point = MapUtil.getScreenMinimapToMapCoord(obj.x, obj.y);
				var distance: int = MapUtil.distance(point.x, point.y, Global.gameContainer.selectedCity.MainBuilding.x, Global.gameContainer.selectedCity.MainBuilding.y);
				var distanceIdx: int;
				if (distance <= 100) distanceIdx = 4;
				else if (distance <= 200) distanceIdx = 3;
				else if (distance <= 300) distanceIdx = 2;
				else if (distance <= 400) distanceIdx = 1;
				else distanceIdx = 0;

				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[distanceIdx].r, DEFAULT_COLORS[distanceIdx].g, DEFAULT_COLORS[distanceIdx].b);
				obj.addChild(img);
			}
		}
		
		override public function applyLegend(legend: CityRegionLegend) : void {
			var icon: DisplayObject = new DOT_SPRITE;
			legend.add(icon, Locale.loadString("MINIMAP_LEGEND_CITY"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			legend.add(icon, StringUtil.substitute(Locale.loadString("MINIMAP_LEGEND_DISTANCE_LESS_THAN"),100));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
			legend.add(icon, StringUtil.substitute(Locale.loadString("MINIMAP_LEGEND_DISTANCE_LESS_THAN"),200));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[2].r, DEFAULT_COLORS[2].g, DEFAULT_COLORS[2].b);
			legend.add(icon, StringUtil.substitute(Locale.loadString("MINIMAP_LEGEND_DISTANCE_LESS_THAN"),300));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
			legend.add(icon, StringUtil.substitute(Locale.loadString("MINIMAP_LEGEND_DISTANCE_LESS_THAN"),400));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			legend.add(icon, Locale.loadString("MINIMAP_LEGEND_DISTANCE_500"));
			
			legend.setLegendTitle(Locale.loadString("MINIMAP_LEGEND_DISTANCE"));
		}
	}
}