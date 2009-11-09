/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects {
	import flash.display.Sprite;
	import nbilyk.gui.layoutManagers.GridLayout_3_0;
	import src.Objects.Prototypes.UnitPrototype;
	import src.UI.Components.TroopGridList.*;
	import src.UI.Components.TroopTileList;
	import src.Util.BinaryList;
	import src.Util.Util;
	import src.Map.Map;
	import src.Map.City;
	import src.Objects.Factories.UnitFactory;
	import src.UI.Components.TroopGridList.*;
	import fl.data.DataProvider;
	import flash.text.TextField;
	import fl.controls.ScrollPolicy;
	import fl.controls.ScrollBarDirection;	
	import flash.display.DisplayObject;
	import flash.text.TextFieldAutoSize;
	import flash.text.TextFieldType;	
	
	public class Troop extends BinaryList {		
		
		public static const IDLE: int = 0;
		public static const BATTLE: int = 1;
		public static const STATIONED: int = 2;
        public static const BATTLE_STATIONED: int = 3;
        public static const MOVING: int = 4;
		
		public var id: int;
		public var state: int = 0;
		
		public var x: int;
		public var y: int;
		
		public var cityId: int;
		public var objectId: int;
		public var playerId: int;
		
		public function Troop(id: int = 0) 
		{
			super(Formation.sortOnType, Formation.compareType);
			this.id = id;
		}
		
		public static function getGridContainer(map: Map, templateManager: TemplateManager, srcTroop: Troop, container: Sprite, unitsInnerContainer: Sprite, formations: Array = null): Array
		{			
			TroopGridList;
			//make a copy of troop
			var troop: Troop = new Troop(srcTroop.id);
			for each(var formation: Formation in srcTroop.each())
			{
				var newFormation: Formation = new Formation(formation.type);
				for each(var unit: Unit in formation.each())
				{
					newFormation.add(new Unit(unit.type, unit.count));
				}
				troop.add(newFormation);
			}
			
			unitsInnerContainer.graphics.beginFill(0xFFFFFF, 0.0);
			unitsInnerContainer.graphics.lineStyle(0, 0xFFFFFF);
			unitsInnerContainer.graphics.drawRect(0, 0, container.width - 4, container.height - 4);
			unitsInnerContainer.graphics.endFill();
			
			unitsInnerContainer.x = 2;
			unitsInnerContainer.y = 2;
			
			var tilelists: Array = new Array();
			
			var lmUnits: GridLayout_3_0 = new GridLayout_3_0(unitsInnerContainer, troop.size() * 2, 1);
			lmUnits.topMargin = 5;
			lmUnits.leftMargin = 5;
			lmUnits.rightMargin = 5;
			
			for each(formation in troop.each())
			{											
				var ts: TroopTileList = new TroopTileList();				
				ts.init(map, templateManager, formation.type);
				
				ts.width = unitsInnerContainer.width - 10;
				ts.height = 90;										
				
				var dp: DataProvider = new DataProvider();
				
				if (formations != null)
				{
					var found: Boolean = false;
					for each (var formationType: int in formations)
					{
						if (formationType == formation.type)
						{
							found = true;
							break;
						}
					}
					if (!found) continue;
				}
				
				for (var z: int = 0; z < formation.size(); z++)
				{					
					unit = formation.getByIndex(z);
				
					var template: UnitTemplate = templateManager.get(unit.type);
					var level: int = 1;
					if (template != null)
						level = template.level;
					
					var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, level);
					var icon: DisplayObject = UnitFactory.getSprite(unit.type, level) as DisplayObject;
					
					dp.addItem( { source: icon, name: (unitPrototype != null ? unitPrototype.getName() : unit.type), data: unit } );
				}
				
				ts.dataProvider = dp;
				
				var lbl: TextField = new TextField();
				lbl.text = Formation.TypeStrings[formation.type];
				lbl.autoSize = TextFieldAutoSize.LEFT;
				lbl.selectable = false;
				lbl.type = TextFieldType.DYNAMIC;
				
				unitsInnerContainer.addChild(lbl);		
				unitsInnerContainer.addChild(ts);		
				
				lmUnits.addObj(lbl);
				lmUnits.addObj(ts);				
				
				tilelists.push(ts);
			}
			
			lmUnits.drawNow();	
			
			return tilelists;
		}
		
		public function getIndividualUnitCount(): int
		{
			var total: int = 0;
			for each (var formation: Formation in each())
			{
				total += formation.getIndividualUnitCount();
			}
			
			return total;
		}
		
		public function ToString():void
		{
			trace("=========");
			trace("Troop " + id );
			trace("Formation count: " + size());
			trace("Total unit count: " + getIndividualUnitCount());
			for each (var formation: Formation in each())
			{				
				trace("\tFormation: " + formation.type);
				trace("\tSize: " + formation.size());
				for each (var unit: Unit in formation.each()) 
				{
					trace("\t\tUnit: " + unit.type + " (" + unit.count + ")");
				}
			}
			trace("=========");
		}
		
		public static function sortOnId(a:Troop, b:Troop):Number 
		{
			var aId:Number = a.id;
			var bId:Number = b.id;

			if(aId > bId) {
				return 1;
			} else if(aId < bId) {
				return -1;
			} else  {
				return 0;
			}
		}
		
		public static function compareId(a: Troop, value: int):int
		{
			return a.id - value;
		}							
	}
	
}
