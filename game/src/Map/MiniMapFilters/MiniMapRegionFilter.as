package src.Map.MiniMapFilters
{
    import flash.display.*;
    import flash.geom.*;

    import src.Constants;
    import src.Global;
    import src.Map.MiniMap.MiniMapLegend;
    import src.Map.MiniMap.MiniMapRegionObject;
    import src.Objects.Factories.ObjectFactory;
    import src.Util.StringHelper;

	public class MiniMapRegionFilter
	{
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

		public function apply(obj: MiniMapRegionObject) : void {
			obj.removeSprite();

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
				case ObjectFactory.TYPE_BARBARIAN_TRIBE:
					applyBarbarianTribe(obj);
					break;
			}
		}
        
        protected function setupMinimapButton(obj: MiniMapRegionObject, button: *, lowAlpha: Boolean = true) : void {
            var hitArea: Sprite = new MINIMAP_HIT_AREA();                                    
            obj.addChild(hitArea);
            hitArea.visible = false;
            button.hitArea = hitArea;            
            button.mouseEnabled = false;            
            button.mouseChildren = false;
            if (lowAlpha) {
                button.alpha = 0.5;
            }
        }
        
		public function applyForest(obj: MiniMapRegionObject) : void {
			var icon: MINIMAP_FOREST_ICON = ObjectFactory.getIcon("MINIMAP_FOREST_ICON") as MINIMAP_FOREST_ICON;
			obj.setIcon(icon);
            if(obj.extraProps.camps>0) {
                icon.lvlText.text = obj.extraProps.camps.toString();
                icon.lvlText.visible = true;
            } else {
			    icon.lvlText.visible = false;
            }
            setupMinimapButton(obj, icon);
		}
		
		public function applyTroop(obj: MiniMapRegionObject) : void {
			var icon: MINIMAP_TROOP_ICON = ObjectFactory.getIcon("MINIMAP_TROOP_ICON") as MINIMAP_TROOP_ICON;
            obj.setIcon(icon);
            // Highlight friendly troops
			if (Constants.tribe.isInTribe(obj.extraProps.tribeId)) {
				obj.transform.colorTransform = new ColorTransform(0, 0, 0, 1, 255, 255, 0);
			}
            setupMinimapButton(obj, icon, false);
		}
		
		public function applyCity(obj: MiniMapRegionObject) : void {
			var icon: DisplayObject;
			
			if (Global.map.cities.get(obj.groupId)) {
				icon = new DOT_SPRITE;
				obj.transform.colorTransform = new ColorTransform();
                obj.setIcon(icon);
			} else {
				icon = new DOT_SPRITE;
                obj.setIcon(icon);
				
				// Apply the difficulty transformation to the tile				
				var percDiff: Number = Number(obj.extraProps.value) / Math.max(1.0, Number(Global.gameContainer.selectedCity.value));
				var difficultyIdx: int;
				if (percDiff <= 0.2) difficultyIdx = 0;
				else if (percDiff <= 0.75) difficultyIdx = 1;
				else if (percDiff <= 1.5) difficultyIdx = 2;
				else if (percDiff <= 1.9) difficultyIdx = 3;
				else difficultyIdx = 4;

				obj.transform.colorTransform = new ColorTransform(.5, .5, .5, 1, DEFAULT_COLORS[difficultyIdx].r, DEFAULT_COLORS[difficultyIdx].g, DEFAULT_COLORS[difficultyIdx].b);
			}
		}
		public function applyStronghold(obj: MiniMapRegionObject) : void {
			var icon: MINIMAP_STRONGHOLD_ICON = ObjectFactory.getIcon("MINIMAP_STRONGHOLD_ICON") as MINIMAP_STRONGHOLD_ICON;
            obj.setIcon(icon);
			icon.lvlText.text = obj.extraProps.level.toString();			
            setupMinimapButton(obj, icon);
		}
		
		public function applyBarbarianTribe(obj: MiniMapRegionObject) : void {
			var icon: MINIMAP_BARBARIAN_TRIBE_ICON = ObjectFactory.getIcon("MINIMAP_BARBARIAN_TRIBE_ICON") as MINIMAP_BARBARIAN_TRIBE_ICON;
            obj.setIcon(icon);
			icon.lvlText.text = obj.extraProps.level.toString();			
            setupMinimapButton(obj, icon);
		}		
		
		public function applyLegend(legend: MiniMapLegend) : void {
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