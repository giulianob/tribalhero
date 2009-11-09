package src.UI.Components {
	import fl.controls.TileList;
	import fl.events.ListEvent;
	import flash.display.Sprite;
	import flash.events.MouseEvent;
	import flash.display.DisplayObject;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Formation;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.TemplateManager;
	import src.Objects.Troop;
	import src.Objects.Unit;
	import src.Objects.UnitTemplate;
	import src.UI.Components.TileListDragDrop.IDraggableTileList;
	import src.UI.Components.TileListDragDrop.TileListDragDropEvent;
	import src.UI.Dialog.Dialog;
	import src.UI.Dialog.NumberInputDialog;
	import src.UI.Tooltips.Tooltip;
	import src.UI.Tooltips.UnitTooltip;
	import src.UI.Renderers.TroopTileListCR;
	import fl.controls.ScrollPolicy;
	import fl.controls.ScrollBarDirection;
	
	public class TroopTileList extends TroopTileList_base implements IDraggableTileList
	{
		public static const UNIT_MOVE: String = "UNIT_MOVE";
		
		private var map: Map;
		private var tooltip: Tooltip;
		private var templates: TemplateManager;
		private var formationType: int;
		
		public function TroopTileList() 
		{
			addEventListener(ListEvent.ITEM_ROLL_OVER, onRollOver);
			addEventListener(ListEvent.ITEM_ROLL_OUT, onRollOut);
			
			rowHeight = 21;
			direction = ScrollBarDirection.HORIZONTAL;
			setStyle("cellRenderer", TroopTileListCR);			
			
			selectable = false;
			scrollPolicy = ScrollPolicy.AUTO;
			direction = ScrollBarDirection.HORIZONTAL;			
			
			sortItemsOn("name");
		}		
		
		public function init(map: Map, templates: TemplateManager, formationType: int, draggable: Boolean = false):void
		{
			this.map = map;
			this.formationType = formationType;
			this.templates = templates;
		}
		
		public override function addItem(item:Object):void 
		{						
			for (var i: int = 0; i < dataProvider.length; i++)
			{
				var currItem: Object = dataProvider.getItemAt(i);
				
				if (currItem.data.type == item.data.type)
				{
					currItem.data.count += item.data.count
					return;
				}
			}
			
			super.addItem(item);
		}
		
		public function getDragIcon(item: Object) : Sprite
		{					
			var template: UnitTemplate = templates.get(item.data.type);
			
			var level: int = 1;
			if (template != null)
				level = template.level;
			
			var icon: Sprite = UnitFactory.getSprite(item.data.type, level) as Sprite;					
			
			return icon;
		}
		
		public function onDragDropped(event: TileListDragDropEvent):void
		{			
			var item: Object = event.item;			

			var numberDialog: NumberInputDialog = new NumberInputDialog("How many troops to transfer?", 0, item.data.count, 
				function(sender: NumberInputDialog):void {										
					sender.getFrame().dispose();
					
					var count: int = sender.getAmount().getValue();										
					var oldCount: int = item.data.count - count;
					
					if (oldCount == 0)
					{
						event.dropSource.removeItem(item);
						addItem(item);
					}
					else
					{									
						//add to old list the left over
						item.data.count = oldCount;						
						
						//add to new list the new units
						var template: UnitTemplate = templates.get(item.data.type);
						var level: int = 1;
						if (template != null)
							level = template.level;
						
						var unit: Unit = new Unit(item.data.type, count);						
						var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, level);
						var icon: DisplayObject = UnitFactory.getSprite(unit.type, level) as DisplayObject;					
						addItem( { source: icon, name: (unitPrototype != null ? unitPrototype.getName() : unit.type), data: unit } );
					}
				},
				item.data.count
			);
			
			numberDialog.show(null, true, function(sender: NumberInputDialog):void { event.dropSource.addItem(item); } );
		}
		
		public function onRollOver(event: ListEvent):void
		{		
			var dp: Object = event.item;
			
			var unit: Unit = dp.data;
			
			var template: UnitTemplate = templates.get(unit.type);
			var level: int = 1;
			if (template != null)
				level = template.level;
			
			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, level);
			
			if (unitPrototype)
			{
				var unitTooltip: UnitTooltip = new UnitTooltip(unitPrototype, unit.count);			
				unitTooltip.show(this);
			}
			
			this.tooltip = unitTooltip;
		}
		
		public function onRollOut(event: ListEvent):void
		{
			if (tooltip)
				tooltip.hide();
					
			tooltip = null;			
		}
		
		public function getFormation(): Formation
		{
			var formation: Formation = new Formation(formationType);
			
			for (var i: int = 0; i < dataProvider.length; i++)
			{
				var currItem: Object = dataProvider.getItemAt(i);
				formation.add(new Unit(currItem.data.type, currItem.data.count));				
			}
			
			return formation;
		}
	}
	
}