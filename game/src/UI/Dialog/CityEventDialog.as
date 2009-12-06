package src.UI.Dialog{

import org.aswing.*;
import org.aswing.border.*;
import org.aswing.geom.*;
import org.aswing.colorchooser.*;
import org.aswing.ext.*;
import src.Global;
import src.Map.City;
import src.Map.Username;
import src.Objects.LazyValue;
import src.UI.Components.CityActionGridList.CityActionGridList;
import src.UI.Components.NotificationGridList.NotificationGridList;
import src.UI.Components.SimpleTooltip;
import src.UI.GameJPanel;
import src.UI.GameLookAndFeel;

/**
 * CityEventDialog
 */
public class CityEventDialog extends GameJPanel{
		
	private var pnlResources:JPanel;
	private var pnlLocalEvents:JPanel;
	private var pnlNotifications:JPanel;
	
	private var gridLocalActions: CityActionGridList;	
	private var gridNotifications: NotificationGridList;	
	
	private var city: City;
	
	public function CityEventDialog(city: City) {
		title = "City Events";
		
		this.city = city;
		
		gridLocalActions = new CityActionGridList(city, 530);
		gridNotifications = new NotificationGridList(city, 530);
		createUI();					
	}		
	
	public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
	{
		super.showSelf(owner, modal, onClose, dispose);
		Global.gameContainer.showFrame(frame);
		return frame;
	}			
	
	private function dispose(): void {
		gridLocalActions.dispose();
		gridNotifications.dispose();
	}
	
	private function simpleLabelMaker(value: int, tooltip: String, icon: Icon = null) : JLabel {
		var label: JLabel = new JLabel(value.toString(), icon);
					
		label.setIconTextGap(0);
		label.setHorizontalTextPosition(AsWingConstants.RIGHT);
		label.setHorizontalAlignment(AsWingConstants.LEFT);
		
		new SimpleTooltip(label, tooltip);
		
		return label;
	}		
	
	private function resourceLabelMaker(resource: LazyValue, tooltip: String, icon: Icon = null, includeLimit: Boolean = true, includeRate: Boolean = true) : JLabel {
		var value: int = resource.getValue();
					
		var label: JLabel = new JLabel(value + (includeLimit ? "/" + resource.getLimit() : "") + (includeRate ? " (+" + resource.getHourlyRate() + " per hour)" : ""), icon);					
		
		label.setIconTextGap(0);
		label.setHorizontalTextPosition(AsWingConstants.RIGHT);
		label.setHorizontalAlignment(AsWingConstants.LEFT);
		
		new SimpleTooltip(label, tooltip);
		
		return label;
	}	
		
	private function createUI(): void {
		//component creation
		var layout0:SoftBoxLayout = new SoftBoxLayout();
		layout0.setAxis(AsWingConstants.VERTICAL);
		layout0.setGap(10);
		setLayout(layout0);
		
		var cityName: Username = Global.map.usernames.cities.get(city.id);
		title = cityName.name + " - Overview";
		
		pnlResources = new JPanel(new GridLayout(3, 2, 20, 10));
		pnlResources.append(resourceLabelMaker(city.resources.gold, "Gold", new AssetIcon(new ICON_GOLD()), false, false));
		pnlResources.append(resourceLabelMaker(city.resources.wood, "Wood", new AssetIcon(new ICON_WOOD())));
		pnlResources.append(resourceLabelMaker(city.resources.crop, "Crop", new AssetIcon(new ICON_CROP())));
		pnlResources.append(resourceLabelMaker(city.resources.iron, "Iron", new AssetIcon(new ICON_IRON())));
		pnlResources.append(resourceLabelMaker(city.resources.labor, "Labor", new AssetIcon(new ICON_LABOR()), false, false));
		pnlResources.append(simpleLabelMaker(0, "Upkeep", new AssetIcon(new ICON_CROP())));		
		
		pnlLocalEvents = new JPanel();
		var border1:TitledBorder = new TitledBorder();
		pnlLocalEvents.setPreferredSize(new IntDimension(500, 200));
		border1.setColor(new ASColor(0x0, 1));
		border1.setTitle("Local Events");
		border1.setPosition(1);
		border1.setAlign(AsWingConstants.LEFT);
		border1.setBeveled(true);
		border1.setRound(10);
		pnlLocalEvents.setBorder(border1);
		var layout2:BoxLayout = new BoxLayout();
		pnlLocalEvents.setLayout(layout2);
		
		pnlLocalEvents.append(new JScrollPane(gridLocalActions));
		
		pnlNotifications = new JPanel();
		pnlNotifications.setPreferredSize(new IntDimension(500, 200));
		var border3:TitledBorder = new TitledBorder();
		border3.setColor(new ASColor(0x0, 1));
		border3.setTitle("Unit Movements");
		border3.setPosition(1);
		border3.setAlign(AsWingConstants.LEFT);
		border3.setBeveled(true);
		border3.setRound(10);
		pnlNotifications.setBorder(border3);
		var layout4:BorderLayout = new BorderLayout();
		pnlNotifications.setLayout(layout4);
		
		pnlNotifications.append(new JScrollPane(gridNotifications));
				
		//component layoution
		append(pnlResources);
		append(pnlLocalEvents);
		append(pnlNotifications);		
	}
	
	
	
	
}
}
