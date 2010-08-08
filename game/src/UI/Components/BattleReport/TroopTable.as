﻿package src.UI.Components.BattleReport 
{
	import flash.display.DisplayObject;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.TableCellEditEvent;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.table.GeneralTableCellFactory;
	import org.aswing.table.PropertyTableModel;
	import src.Objects.Factories.ObjectFactory;
	import src.UI.Components.SimpleTooltip;
		
	public class TroopTable extends JPanel
	{
		private var unitList: VectorListModel;
		private var tableModel: PropertyTableModel;
		private var tblUnits: JTable;
		private var pnlResources: JPanel;
		
		private var troop: Object;
		
		public function TroopTable(troop: Object) 
		{
			this.troop = troop;
			
			createUI();
			
			tblUnits.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
				tblUnits.getCellEditor().stopCellEditing();
			});
			
			for each (var unit: Object in troop.units) {
				//We need to preload the icon and tooltip because we don't have access to all of this info inside of the unit table cell
				var prototype: * = ObjectFactory.getPrototype(unit.type, unit.level);
				
				var icon: DisplayObject = ObjectFactory.getSpriteEx(prototype.type, prototype.level, true);
			
				var scale: Number = 42 / icon.height;
				if (scale < 1) {
					scale = Math.min(0.5, Number(scale.toFixed(1)));
					icon.scaleX = scale;
					icon.scaleY = scale;
				}				
				
				new SimpleTooltip(icon, prototype.getName());
				
				unitList.append({icon: icon, level: unit.level, count: unit.count, hp: unit.hp, hitsTaken: unit.hitsTaken, dmgTaken: unit.dmgTaken, hitsDealt: unit.hitsDealt, dmgDealt: unit.dmgDealt});			
			}
			
			if (troop.resources != null) {
				pnlResources.append(resourceLabelMaker(troop.resources.gold, "Gold", new AssetIcon(new ICON_GOLD())));
				pnlResources.append(resourceLabelMaker(troop.resources.wood, "Wood", new AssetIcon(new ICON_WOOD())));
				pnlResources.append(resourceLabelMaker(troop.resources.crop, "Crop", new AssetIcon(new ICON_CROP())));
				pnlResources.append(resourceLabelMaker(troop.resources.iron, "Iron", new AssetIcon(new ICON_IRON())));								
				
				append(pnlResources);
			}
			
			append(tblUnits);
		}
		
		private function createUI() : void {
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			
			unitList = new VectorListModel();
			tableModel = new PropertyTableModel(unitList,
				["Type", "Level", "Count", "HP", "Hits\nTaken", "Damage\nTaken", "Hits\nDealt", "Damage\nDealt"],
				["icon", "level", "count", "hp", "hitsTaken", 	"dmgTaken", 	 "hitsDealt", 	"dmgDealt"],
				[null, null, null, null, null, null]
			);			
			
			tblUnits = new JTable(tableModel);
			tblUnits.setRowSelectionAllowed(false);
			tblUnits.setRowHeight(42);			
			tblUnits.getColumn("Type").setCellFactory(new GeneralTableCellFactory(UnitIconCell));
			
			pnlResources = new JPanel(new FlowLayout(AsWingConstants.LEFT, 12, 5, false));			
		}	
		
		private function resourceLabelMaker(value: int, tooltip: String, icon: Icon = null) : JLabel {
			var label: JLabel = new JLabel(value.toString(), icon);
			
			new SimpleTooltip(label, tooltip);
			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.RIGHT);
			return label;
		}		
	}

}