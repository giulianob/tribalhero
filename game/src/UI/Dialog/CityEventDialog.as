package src.UI.Dialog
{

    import flash.events.*;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.event.*;
    import org.aswing.geom.*;
    import org.aswing.table.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Effects.*;
    import src.Objects.Factories.*;
    import src.Objects.Prototypes.*;
    import src.UI.*;
    import src.UI.Components.*;
    import src.UI.Components.CityActionGridList.*;
    import src.UI.Components.TableCells.*;
    import src.Util.*;

    public class CityEventDialog extends GameJPanel
	{
		private var pnlResources:JPanel;
		private var pnlTabs:JTabbedPane;
		
		private var gridLocalActions:CityActionGridList;
		private var laborersTable:JTable;
		private var laborersListModel: VectorListModel;
		
		private var lblGold:JLabel;
		private var lblWood:JLabel;
		private var lblCrop:JLabel;
		private var lblIron:JLabel;
		private var lblLabor:JLabel;
		private var lblUpkeep:JLabel;
		private var lblApPoints:JLabel;
		private var lblAttackPoints:JLabel;
		private var lblDefensePoints:JLabel;
		private var lblUpkeepMsg:JLabel;
		private var lblValue:JLabel;
		private var lblUnits:JLabel;
		
		private var city:City;
		
		private var lstCities: JComboBox;
		
		public function CityEventDialog(city:City)
		{
			this.city = city;			
			
			title = StringHelper.localize("CITY_OVERVIEW_TITLE");
			
			createUI();
			
			for each (var eachCity: City in Global.map.cities) {
				(lstCities.getModel() as VectorListModel).append( { id: eachCity.id, city: eachCity, toString: function() : String { return this.city.name; } } );
				if (eachCity == city) {
					lstCities.setSelectedIndex(lstCities.getItemCount() - 1);
				}
			}
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose, dispose);
			
			city.addEventListener(City.RESOURCES_UPDATE, onResourceChange);
			
			var innerCity: City = city;
			frame.addEventListener(PopupEvent.POPUP_CLOSED, function(e:PopupEvent):void
				{
					innerCity.removeEventListener(City.RESOURCES_UPDATE, onResourceChange);
				});
						
			frame.setResizable(true);
			frame.setMinimumSize(new IntDimension(640, 345));													
			frame.pack();
			
			Global.gameContainer.showFrame(frame);
			
			return frame;
		}
		
		private function onResourceChange(e:Event):void
		{
			drawResources();
		}
		
		private function dispose():void
		{
			gridLocalActions.dispose();
		}
		
		public function onChangeCitySelection(e: InteractiveEvent):void {		
			if (city) {
				city.removeEventListener(City.RESOURCES_UPDATE, onResourceChange);
			}
			
			city = null;
			
			if (lstCities.getSelectedIndex() == -1) return;
			
			city = lstCities.getSelectedItem().city;
			gridLocalActions.setCity(city);
			city.addEventListener(City.RESOURCES_UPDATE, onResourceChange);
			
			recreateResourcesPanel();
			drawResources();
		}
		
		private function simpleLabelMaker(tooltip:String, icon:Icon = null):JLabel
		{
			var label:JLabel = new JLabel("", icon);
			
			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.RIGHT);
			label.setHorizontalAlignment(AsWingConstants.LEFT);
			
			new SimpleTooltip(label, tooltip);
			
			return label;
		}
		
		private function simpleLabelText(value:String, hourly:Boolean = false, negative:Boolean = false):String
		{
			var valueStr: String = (hourly ? (negative ? "-" : "+") : "") + value;
			
			if (!hourly) {
				return valueStr;
			}
			
			return StringHelper.localize("STR_PER_HOUR", valueStr);
		}
		
		private function resourceLabelMaker(tooltip:String, icon:Icon = null):JLabel
		{
			var label:JLabel = new JLabel("", icon);
			
			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.RIGHT);
			label.setHorizontalAlignment(AsWingConstants.LEFT);
			
			new SimpleTooltip(label, tooltip);
			
			return label;
		}
		
		private function resourceLabelText(resource:LazyValue, includeLimit:Boolean = true, includeRate:Boolean = true):String
		{
			var value:int = resource.getValue();
			
			return value.toString() + (includeLimit ? "/" + resource.getLimit() : "") + (includeRate ? " (" + StringHelper.localize("STR_PER_HOUR", "+" + resource.getHourlyRate()) + ")" : "");
		}
		
		private function recreateResourcesPanel():void {
			pnlResources.removeAll();
			lblGold = resourceLabelMaker(StringHelper.localize("GOLD_DESC"), new AssetIcon(new ICON_GOLD()));
			lblWood = resourceLabelMaker(StringHelper.localize("WOOD_DESC"), new AssetIcon(new ICON_WOOD()));
			lblCrop = resourceLabelMaker(StringHelper.localize("CROP_DESC"), new AssetIcon(new ICON_CROP()));
			lblIron = resourceLabelMaker(StringHelper.localize("IRON_DESC"), new AssetIcon(new ICON_IRON()));
			lblUnits = simpleLabelMaker(StringHelper.localize("UNITS_DESC"), new AssetIcon(new ICON_SINGLE_SWORD()));			
			
			var laborTime:String = DateUtil.niceTime(Formula.laborRate(city));
			
			lblLabor = simpleLabelMaker(StringHelper.localize("LABOR_DESC", laborTime), new AssetIcon(new ICON_LABOR()));
			
			lblUpkeep = simpleLabelMaker(StringHelper.localize("UPKEEP_DESC"), new AssetIcon(new ICON_CROP()));
			lblDefensePoints = simpleLabelMaker(StringHelper.localize("DEFENSE_POINTS_DESC"), new AssetIcon(new ICON_SHIELD()));
			lblAttackPoints = simpleLabelMaker(StringHelper.localize("ATTACK_POINTS_DESC"), new AssetIcon(new ICON_BATTLE()));
			lblValue = simpleLabelMaker(StringHelper.localize("INFLUENCE_POINTS_DESC"), new AssetIcon(new ICON_UPGRADE()));		
			lblApPoints = simpleLabelMaker(StringHelper.localize("AP_POINTS_DESC"), new AssetIcon(new ICON_HAMMER()));
			
			pnlResources.appendAll(lblGold, lblWood, lblCrop, lblIron, lblLabor, lblUpkeep, lblDefensePoints, lblAttackPoints, lblValue, lblUnits, lblApPoints);			
		}
		
		private function drawResources():void
		{
			{
				// Local Events Tab
				lblGold.setText(resourceLabelText(city.resources.gold, false, true));
				lblWood.setText(resourceLabelText(city.resources.wood));
				lblCrop.setText(resourceLabelText(city.resources.crop));
				lblIron.setText(resourceLabelText(city.resources.iron));
				// Divide and multiply by 10 to truncate to 1 decimal place w/o rounding
				lblApPoints.setText(StringHelper.localize("CITY_OVERVIEW_AP_POINTS_LABEL", Util.truncateNumber(city.ap)));
				lblLabor.setText(simpleLabelText(StringHelper.localize("CITY_OVERVIEW_LABORERS_LABEL", city.resources.labor.getValue(), city.getBusyLaborCount()), false, false));
				lblUpkeep.setText(simpleLabelText((city.resources.crop.getUpkeep() / Constants.secondsPerUnit).toString(), true, true));
				lblUpkeepMsg.setVisible(city.resources.crop.getRate() < city.resources.crop.getUpkeep());
				lblAttackPoints.setText(StringHelper.localize("CITY_OVERVIEW_ATTACK_POINTS_LABEL", city.attackPoint));
				lblDefensePoints.setText(StringHelper.localize("CITY_OVERVIEW_DEFENSE_POINTS_LABEL", city.defensePoint));
				lblValue.setText(StringHelper.localize("CITY_OVERVIEW_INFLUENCE_POINTS_LABEL", city.value));			
				
				var unitCounts: * = city.troops.getUnitTotalsByStatus();
				lblUnits.setText(StringHelper.localize("CITY_OVERVIEW_UNITS_STATUS_LABEL", unitCounts.idle, unitCounts.onTheMove));
			}
			
			{
				// Laborers tab
				// Go through each city object and add if it accepts laborers		
				laborersListModel.clear();
				for each (var cityObj: CityObject in city.objects) {
					if (ObjectFactory.getClassType(cityObj.type) != ObjectFactory.TYPE_STRUCTURE) {
						continue;
					}
					
					var proto: StructurePrototype = cityObj.getStructurePrototype();
					if (cityObj.labor == 0 && proto.maxlabor == 0) {
						continue;
					}
					
					laborersListModel.append(cityObj);
				}
			}			
			
			repaintAndRevalidate();
		}
		
		public function maxLaborerTranslator(info:CityObject, key:String):String
		{
			var proto:StructurePrototype = info.getStructurePrototype();
			if (!proto)
			{
				return "-";
			}
			
            if (proto.maxlabor == 0) {
                return info.labor.toString();
            }
            
			return info.labor + "/" + proto.maxlabor.toString();
		}
		
		private function createUI():void
		{
			setLayout(new BorderLayout(0, 10));
			
			var pnlNorth: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			pnlNorth.setConstraints("North");
			{
				lstCities = new JComboBox();			
				lstCities.setModel(new VectorListModel());
				lstCities.addEventListener(InteractiveEvent.SELECTION_CHANGED, onChangeCitySelection);
				lstCities.setPreferredSize(new IntDimension(150, 22));
				
				lblUpkeepMsg = new JLabel(StringHelper.localize("CITY_OVERVIEW_TROOPS_STARVING"), new AssetIcon(new ICON_CROP()));
				lblUpkeepMsg.setBorder(new LineBorder(null, new ASColor(0xff0000), 2, 10));				
				
				pnlResources = new JPanel(new GridLayout(0, 3, 20, 10));			
				
				pnlNorth.appendAll(AsWingUtils.createPaneToHold(lstCities, new FlowLayout()), pnlResources);
			}
			
			pnlTabs = new JTabbedPane();
			pnlTabs.setConstraints("Center");
			pnlTabs.setPreferredSize(new IntDimension(540, 235));
			
			// Local Events Tab
			{
				gridLocalActions = new CityActionGridList(city, 530);
				pnlTabs.appendTab(new JScrollPane(gridLocalActions), StringHelper.localize("CITY_OVERVIEW_LOCAL_EVENTS_TAB"));				
			}
			
			// Laborers Tab
			{
				laborersListModel = new VectorListModel();
				laborersTable = new JTable(new PropertyTableModel(laborersListModel, [StringHelper.localize("CITY_OVERVIEW_LABORERS_STRUCTURE_COLUMN"), StringHelper.localize("CITY_OVERVIEW_LABORERS_WORKING_COLUMN")], [".", "."], [null, maxLaborerTranslator]));
				laborersTable.getColumnAt(0).setCellFactory(new GeneralTableCellFactory(StructureCell));
				laborersTable.getColumnAt(0).setPreferredWidth(160);
				laborersTable.getColumnAt(1).setPreferredWidth(370);
				laborersTable.setCellSelectionEnabled(false);		
				laborersTable.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
					laborersTable.getCellEditor().cancelCellEditing();
				});			
				
				laborersTable.setAutoResizeMode(JTable.AUTO_RESIZE_OFF);
				laborersTable.setRowHeight(40);
				pnlTabs.appendTab(new JScrollPane(laborersTable), StringHelper.localize("CITY_OVERVIEW_LABORERS_TAB"));
			}
			
			//component layoution			
			append(pnlNorth);
			append(lblUpkeepMsg);
			append(pnlTabs);
			
			recreateResourcesPanel();
			drawResources();
		}
	
	}
}

