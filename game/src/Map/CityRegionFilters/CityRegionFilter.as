package src.Map.CityRegionFilters 
{
	import src.Util.StringHelper;
	import org.aswing.AssetIcon;
	import src.Constants;
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
	public class CityRegionFilter
	{
		private var legend : CityRegionFilter;
		
		protected static const DEFAULT_COLORS: Array = [
		{ r: 200, g: 200, b: 200 },
		{ r: 0, g: 156, b: 20 },
		{ r: 255, g: 255, b: 0},
		{ r: 255, g: 100, b: 0 },
		{ r: 255, g: 0, b: 0 }
		];
		
		public function getName(): String {
			return "Default";
		}

		public function apply(obj: CityRegionObject) : void {
			while (obj.numChildren > 0) obj.removeChildAt(0);
			switch(obj.type) {
				case ObjectFactory.TYPE_FOREST:
					applyForest(obj);
					break;
				case ObjectFactory.TYPE_TROOP_OBJ:
					applyTroop(obj);
					break;
				case ObjectFactory.TYPE_CITY:
					applyCity(obj);
					break;
				case ObjectFactory.TYPE_STRONGHOLD:
					applyStronghold(obj);
					break;
				case ObjectFactory.TYPE_SETTLEMENT:
					applySettlement(obj);
					break;
			}
		}
		public function applyForest(obj: CityRegionObject) : void {
			var img: DisplayObject = ObjectFactory.getIcon("MINIMAP_FOREST_ICON");
			obj.sprite = img;
			obj.addChild(img);
			
			var icon: MINIMAP_FOREST_ICON = obj.sprite as MINIMAP_FOREST_ICON;
			icon.lvlText.mouseEnabled = false;
			icon.useHandCursor = true;
			icon.lvlText.text = obj.extraProps.level.toString();
			obj.alpha = 0.5;
		}
		
		public function applyTroop(obj: CityRegionObject) : void {
			var img: DisplayObject = ObjectFactory.getIcon("MINIMAP_TROOP_ICON");
			obj.sprite = img;
			if (Constants.tribeId > 0 && obj.extraProps.tribeId == Constants.tribeId) {
				obj.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 255, 255, 0);
			}
			obj.addChild(img);
		}
		
		public function applyCity(obj: CityRegionObject) : void {
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
				var percDiff: Number = Number(obj.extraProps.value) / Math.max(1.0, Number(Global.gameContainer.selectedCity.value));
				var difficultyIdx: int;
				if (percDiff <= 0.2) difficultyIdx = 0;
				else if (percDiff <= 0.75) difficultyIdx = 1;
				else if (percDiff <= 1.5) difficultyIdx = 2;
				else if (percDiff <= 1.9) difficultyIdx = 3;
				else difficultyIdx = 4;

				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[difficultyIdx].r, DEFAULT_COLORS[difficultyIdx].g, DEFAULT_COLORS[difficultyIdx].b);
				obj.addChild(img);
			}
		}
		public function applyStronghold(obj: CityRegionObject) : void {
			var img: DisplayObject = ObjectFactory.getIcon("MINIMAP_FOREST_ICON");
			obj.sprite = img;
			obj.sprite.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 255, 255, 0);
			obj.addChild(img);
			
			var icon: MINIMAP_FOREST_ICON = obj.sprite as MINIMAP_FOREST_ICON;
			icon.lvlText.mouseEnabled = false;
			icon.useHandCursor = true;
			icon.lvlText.text = obj.extraProps.level.toString();
			obj.alpha = 0.5;
		}
		
		public function applySettlement(obj: CityRegionObject) : void {
			var img: DisplayObject = ObjectFactory.getIcon("MINIMAP_FOREST_ICON");
			obj.sprite = img;
			obj.sprite.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 0, 255, 255);
			obj.addChild(img);
			
			var icon: MINIMAP_FOREST_ICON = obj.sprite as MINIMAP_FOREST_ICON;
			icon.lvlText.mouseEnabled = false;
			icon.useHandCursor = true;
			icon.lvlText.text = obj.extraProps.level.toString();
			obj.alpha = 0.5;
		}		
		
		public function applyLegend(legend: CityRegionLegend) : void {
			var icon: DisplayObject = new DOT_SPRITE;
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_CITY"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[4].r, DEFAULT_COLORS[4].g, DEFAULT_COLORS[4].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_STRONGEST"));
			
			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[3].r, DEFAULT_COLORS[3].g, DEFAULT_COLORS[3].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_STRONG"));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[2].r, DEFAULT_COLORS[2].g, DEFAULT_COLORS[2].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_NORMAL"));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[1].r, DEFAULT_COLORS[1].g, DEFAULT_COLORS[1].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_WEAK"));

			icon = new DOT_SPRITE;
			icon.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[0].r, DEFAULT_COLORS[0].g, DEFAULT_COLORS[0].b);
			legend.add(icon, StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY_WEAKEST"));
			
			legend.setLegendTitle(StringHelper.localize("MINIMAP_LEGEND_DIFFICULTY"));
		}
		
	}
}