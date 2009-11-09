﻿package src.UI.Components.TroopGridList
{
	import flash.display.DisplayObject;
	import org.aswing.AsWingConstants;
	import org.aswing.border.EmptyBorder;
	import org.aswing.border.TitledBorder;
	import org.aswing.dnd.DragListener;
	import org.aswing.event.DragAndDropEvent;
	import org.aswing.ext.DefaultGridCell;
	import org.aswing.ext.GeneralGridListCellFactory;
	import org.aswing.ext.GridList;
	import org.aswing.ext.GridListCellFactory;
	import org.aswing.ext.GridListItemEvent;
	import org.aswing.geom.IntDimension;
	import org.aswing.Insets;
	import org.aswing.JPanel;
	import org.aswing.JTabbedPane;
	import org.aswing.ListModel;
	import org.aswing.SoftBoxLayout;
	import org.aswing.VectorListModel;
	import src.Objects.Factories.UnitFactory;
	import src.Objects.Formation;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.TemplateManager;
	import src.Objects.Troop;
	import src.Objects.Unit;
	import src.Objects.UnitTemplate;
	import src.UI.Tooltips.Tooltip;
	import src.UI.Tooltips.UnitTooltip;
	
	/**
	 * ...
	 * @author Giuliano
	 */
	public class TroopGridList extends GridList
	{		
		private var templates: TemplateManager;
		private var formationType: int;
		private var tooltip: Tooltip;
		
		public function TroopGridList(templateManager: TemplateManager, formationType: int)
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(TroopGridCell), 0, 2);
			setBorder(new EmptyBorder(null, new Insets(8, 3, 8, 3)));
			setTracksWidth(true);
			this.templates = templateManager;
			this.formationType = formationType;				
						
			setTileWidth(32);
			setTileHeight(32);
			
			addEventListener(GridListItemEvent.ITEM_ROLL_OVER, onItemRollOver);
			addEventListener(GridListItemEvent.ITEM_ROLL_OUT, onItemRollOut);
		}				
		
		public function onItemRollOver(event: GridListItemEvent):void
		{		
			var dp: TroopGridCell = event.getCell() as TroopGridCell;
			
			var unit: Unit = dp.getCellValue().data;
			
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
		
		public function onItemRollOut(event: GridListItemEvent):void
		{
			if (tooltip)
				tooltip.hide();
					
			tooltip = null;			
		}		
		
		public function getFormation(): Formation
		{
			var formation: Formation = new Formation(formationType);
			
			for (var i: int = 0; i < getModel().getSize(); i++)
			{
				var currItem: * = getModel().getElementAt(i);
				formation.add(new Unit(currItem.data.type, currItem.data.count));				
			}
			
			return formation;
		}		
		
		public function addUnit(type: int, count: int) : TroopGridCell {
			for (var i: int = 0; i < getModel().getSize(); i++)
			{
				var troopCell: TroopGridCell = getCellByIndex(i) as TroopGridCell;
				var value: * = troopCell.getCellValue();
				if (value.data.type == type) {
					value.data.count += count;
					return null;
				}
			}
			
			var unit: Unit = new Unit(type, count);
			var template: UnitTemplate = templates.get(type);
			var level: int = 1;
			if (template != null)
				level = template.level;
			
			var unitPrototype: UnitPrototype = UnitFactory.getPrototype(type, level);
			var icon: DisplayObject = UnitFactory.getSprite(type, level) as DisplayObject;
						
			(getModel() as VectorListModel).append( { source: icon, name: (unitPrototype != null ? unitPrototype.getName() : unit.type), data: unit, level: level } );
			
			return getCellByIndex(getModel().getSize() - 1) as TroopGridCell;
		}
		
		public static function getGridList(srcTroop: Troop, templateManager: TemplateManager, formations: Array = null): Array
		{
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
			
			var tilelists: Array = new Array();
			
			for each(formation in troop.each())
			{											
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
				
				var ts: TroopGridList = new TroopGridList(templateManager, formation.type);								
				
				for (var z: int = 0; z < formation.size(); z++)
				{					
					unit = formation.getByIndex(z);
				
					var template: UnitTemplate = templateManager.get(unit.type);
					var level: int = 1;
					if (template != null)
						level = template.level;
					
					var unitPrototype: UnitPrototype = UnitFactory.getPrototype(unit.type, level);
					var icon: DisplayObject = UnitFactory.getSprite(unit.type, level) as DisplayObject;
					
					(ts.getModel() as VectorListModel).append( { source: icon, name: (unitPrototype != null ? unitPrototype.getName() : unit.type), data: unit, level: level } );
				}				
				
				tilelists.push(ts);
			}
			
			return tilelists;
		}
		
		public static function tabGridLists(tilelists: Array) : JPanel {
			var panel: JPanel = new JPanel();
			
			var tabbedPane: JTabbedPane = new JTabbedPane();
			
			for each(var ts: TroopGridList in tilelists)						
				tabbedPane.appendTab(ts, Formation.TypeStrings[ts.getFormation().type]);
		
			panel.append(tabbedPane);
			
			return panel;			
		}
		
		public static function stackGridLists(tilelists: Array) : JPanel {
			var panel: JPanel = new JPanel();
			
			var layout0: SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(10);
			
			panel.setLayout(layout0);								
						
			for each(var ts: TroopGridList in tilelists)
			{
				ts.setBorder(new TitledBorder(null, Formation.TypeStrings[ts.getFormation().type], AsWingConstants.TOP, AsWingConstants.LEFT, 0, 10));
				ts.setPreferredSize(new IntDimension(300, 100));
				panel.append(ts);
			}		
			
			return panel;
		}
	}
	
}