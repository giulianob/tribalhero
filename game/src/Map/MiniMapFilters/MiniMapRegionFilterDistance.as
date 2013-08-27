package src.Map.MiniMapFilters
{
    import flash.display.*;
    import flash.geom.*;

    import src.Global;
    import src.Map.MiniMap.MiniMapLegend;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Map.Position;
    import src.Map.TileLocator;
    import src.Util.StringHelper;

	public class MiniMapRegionFilterDistance extends MiniMapRegionFilter
	{		
		override public function getName(): String {
			return "Distance";
		}

		override public function applyCity(obj: MiniMapRegionObject) : void {
            obj.setIcon(new DOT_SPRITE);

			if (Global.map.cities.get(obj.groupId)) {

				obj.transform.colorTransform = new ColorTransform();
			} else {
				// Apply the difficulty transformation to the tile
				var point: Position = TileLocator.getScreenMinimapToMapCoord(obj.x, obj.y);
				var distance: int = TileLocator.distance(point.x, point.y, 1, Global.gameContainer.selectedCity.primaryPosition.x, Global.gameContainer.selectedCity.primaryPosition.y, 1);
				var distanceIdx: int;
				if (distance <= 100) distanceIdx = 4;
				else if (distance <= 200) distanceIdx = 3;
				else if (distance <= 300) distanceIdx = 2;
				else if (distance <= 400) distanceIdx = 1;
				else distanceIdx = 0;

				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[distanceIdx].r, DEFAULT_COLORS[distanceIdx].g, DEFAULT_COLORS[distanceIdx].b);
			}
		}
		
		override public function applyLegend(legend: MiniMapLegend) : void {
			var icon: DisplayObject = new DOT_SPRITE;
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_CITY"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN",100));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN",200));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[2].r, DEFAULT_COLORS[2].g, DEFAULT_COLORS[2].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN",300));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_LESS_THAN",400));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DISTANCE_500"));
			
			legend.setLegendTitle(StringHelper.localize("MINIMAP_LEGEND_DISTANCE"));
		}
	}
}