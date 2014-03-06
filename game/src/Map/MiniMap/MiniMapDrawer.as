package src.Map.MiniMap
{
import flash.events.Event;

import src.Map.MiniMap.LegendGroups.MiniMapGroupCity;
import src.Map.MiniMap.LegendGroups.MiniMapGroupOther;
import src.Map.MiniMap.LegendGroups.MiniMapGroupStronghold;
import src.Map.MiniMapDrawers.*;
import src.Objects.Factories.ObjectFactory;

public class MiniMapDrawer
	{

 /*   private var treeDrawer: MiniMapTreeDrawer = new MiniMapTreeDrawer();
    private var strongholdDrawer: MiniMapStrongholdDrawer = new MiniMapStrongholdDrawer();
    private var troopDrawer: MiniMapTroopDrawer= new MiniMapTroopDrawer();
    private var barbarianDrawer: MiniMapBarbarianDrawer = new MiniMapBarbarianDrawer();
*/
        private var groupCity: MiniMapGroupCity = new MiniMapGroupCity();
        private var groupStronghold: MiniMapGroupStronghold = new MiniMapGroupStronghold();
        private var groupOther: MiniMapGroupOther = new MiniMapGroupOther();

        private var callback:Function = null;

		public function MiniMapDrawer() {
            groupCity.addOnChangeListener(onChange);
            groupStronghold.addOnChangeListener(onChange);
            groupOther.addOnChangeListener(onChange);
        }

        private function onChange(e:Event) : void {
            if(callback!=null) callback();
        }

        public function addOnChangeListener(callback: Function): void {
            this.callback=callback;
        }

		public function apply(obj: MiniMapRegionObject) : void {
			obj.removeSprite();
            obj.graphics
			switch(obj.type) {
                case ObjectFactory.TYPE_CITY:
                    groupCity.applyCity(obj);
                    break;
				case ObjectFactory.TYPE_TROOP_OBJ:
                    groupCity.applyTroop(obj);
					break;
				case ObjectFactory.TYPE_STRONGHOLD:
                    groupStronghold.applyStronghold(obj);
					break;
                case ObjectFactory.TYPE_FOREST:
                    groupOther.applyForest(obj);
                    break;
				case ObjectFactory.TYPE_BARBARIAN_TRIBE:
                    groupOther.applyBarbarian(obj);
					break;
			}
		}

		public function applyLegend(legend: MiniMapLegend) : void {
            legend.addPanel(groupCity.getLegendPanel());
            legend.addPanel(groupStronghold.getLegendPanel());
            legend.addPanel(groupOther.getLegendPanel());
		}

	}
}