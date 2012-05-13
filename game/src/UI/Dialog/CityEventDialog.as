package src.UI.Dialog
{
	
	import fl.lang.*;
	import flash.events.*;
	import mx.utils.StringUtil;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.event.*;
	import org.aswing.ext.*;
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
		private var lblAttackPoints:JLabel;
		private var lblDefensePoints:JLabel;
		private var lblUpkeepMsg:JLabel;
		private var lblValue:JLabel;
		
		private var city:City;
		
		private var lstCities: JComboBox;
		
		public function CityEventDialog(city:City)
		{
			this.city = city;			
			
			title = "City Overview";
			
			createUI();
			
			for each (var eachCity: City in Global.map.cities.each()) {
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
			
			frame.addEventListener(PopupEvent.POPUP_CLOSED, function(e:PopupEvent):void
				{
					city.removeEventListener(City.RESOURCES_UPDATE, onResourceChange);
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
			return (hourly ? (negative ? "-" : "+") : "") + value + (hourly ? " per hour" : "");
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
			
			return value + (includeLimit ? "/" + resource.getLimit() : "") + (includeRate ? " (+" + resource.getHourlyRate() + " per hour)" : "");
		}
		
		private function recreateResourcesPanel():void {
			pnlResources.removeAll();
			lblGold = resourceLabelMaker("Gold\n\n" + Locale.loadString("GOLD_DESC"), new AssetIcon(new ICON_GOLD()));
			lblWood = resourceLabelMaker("Wood\n\n" + Locale.loadString("WOOD_DESC"), new AssetIcon(new ICON_WOOD()));
			lblCrop = resourceLabelMaker("Crop\n\n" + Locale.loadString("CROP_DESC"), new AssetIcon(new ICON_CROP()));
			lblIron = resourceLabelMaker("Iron\n\n" + Locale.loadString("IRON_DESC"), new AssetIcon(new ICON_IRON()));
			
			var laborTime:String = Util.niceTime(Formula.laborRate(city), false);
			
			lblLabor = simpleLabelMaker("Laborer\n\n" + StringUtil.substitute(Locale.loadString("LABOR_DESC"), laborTime), new AssetIcon(new ICON_LABOR()));
			
			lblUpkeep = simpleLabelMaker("Troop Upkeep\n\n" + Locale.loadString("UPKEEP_DESC"), new AssetIcon(new ICON_CROP()));
			lblDefensePoints = simpleLabelMaker("Defense Points\n\n" + Locale.loadString("DEFENSE_POINTS_DESC"), new AssetIcon(new ICON_SHIELD()));
			lblAttackPoints = simpleLabelMaker("Attack Points\n\n" + Locale.loadString("ATTACK_POINTS_DESC"), new AssetIcon(new ICON_BATTLE()));
			lblValue = simpleLabelMaker("Influence Points\n\n" + Locale.loadString("INFLUENCE_POINTS_DESC"), new AssetIcon(new ICON_UPGRADE()));		
			
			pnlResources.appendAll(lblGold, lblWood, lblCrop, lblIron, lblLabor, lblUpkeep, lblDefensePoints, lblAttackPoints, lblValue);				
		}
		
		private function drawResources():void
		{
			{
				// Local Events Tab
				lblGold.setText(resourceLabelText(city.resources.gold, false, true));
				lblWood.setText(resourceLabelText(city.resources.wood));
				lblCrop.setText(resourceLabelText(city.resources.crop));
				lblIron.setText(resourceLabelText(city.resources.iron));
				lblLabor.setText(simpleLabelText(city.resources.labor.getValue().toString() + " " + StringHelper.makePlural(city.resources.labor.getValue(), "is", "are", "are") + " idle and " + city.getBusyLaborCount().toString() + " " + StringHelper.makePlural(city.getBusyLaborCount(), "is", "are", "are") + " working", false, false));
				lblUpkeep.setText(simpleLabelText(city.resources.crop.getUpkeep().toString(), true, true));
				lblUpkeepMsg.setVisible(city.resources.crop.getRate() < city.resources.crop.getUpkeep());
				lblAttackPoints.setText(city.attackPoint + " attack points");
				lblDefensePoints.setText(city.defensePoint + " defense points");
				lblValue.setText(city.value + " Influence points");							
			}
			
			{
				// Laborers tab
				// Go through each city object and add if it accepts laborers		
				laborersListModel.clear();
				for each (var cityObj: CityObject in city.objects.each()) {
					if (ObjectFactory.getClassType(cityObj.type) != ObjectFactory.TYPE_STRUCTURE) {
						continue;
					}
					
					var proto: StructurePrototype = cityObj.getStructurePrototype();
					if (proto.maxlabor == 0) {
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
				
				lblUpkeepMsg = new JLabel("Your troop upkeep currently exceeds your crop production rate. Your units will slowly starve to death.", new AssetIcon(new ICON_CROP()));
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
				pnlTabs.appendTab(new JScrollPane(gridLocalActions), "Local Events");				
			}
			
			// Laborers Tab
			{
				laborersListModel = new VectorListModel();
				laborersTable = new JTable(new PropertyTableModel(laborersListModel, ["Structure", "Working/Maximum Laborers"], [".", "."], [null, maxLaborerTranslator]));
				laborersTable.getColumnAt(0).setCellFactory(new GeneralTableCellFactory(StructureCell));
				laborersTable.getColumnAt(0).setPreferredWidth(160);
				laborersTable.getColumnAt(1).setPreferredWidth(370);
				laborersTable.setCellSelectionEnabled(false);		
				laborersTable.addEventListener(TableCellEditEvent.EDITING_STARTED, function(e: TableCellEditEvent) : void {
					laborersTable.getCellEditor().cancelCellEditing();
				});			
				
				laborersTable.setAutoResizeMode(JTable.AUTO_RESIZE_OFF);
				laborersTable.setRowHeight(40);
				pnlTabs.appendTab(new JScrollPane(laborersTable), "Laborers");
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

