package src.UI.Dialog{

	import flash.events.Event;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.PopupEvent;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.City;
	import src.Map.Username;
	import src.Objects.LazyValue;
	import src.UI.Components.CityActionGridList.CityActionGridList;
	import src.UI.Components.SimpleTooltip;
	import src.UI.GameJPanel;
	import src.Util.StringHelper;
	import src.Util.Util;

	/**
	 * CityEventDialog
	 */
	public class CityEventDialog extends GameJPanel{

		private var pnlResources:JPanel;
		private var pnlLocalEvents:JPanel;

		private var gridLocalActions: CityActionGridList;

		private var lblGold: JLabel;
		private var lblWood: JLabel;
		private var lblCrop: JLabel;
		private var lblIron: JLabel;
		private var lblLabor: JLabel;
		private var lblUpkeep: JLabel;

		private var city: City;

		public function CityEventDialog(city: City) {
			title = "City Events";

			this.city = city;

			gridLocalActions = new CityActionGridList(city, 530);
			createUI();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose, dispose);

			city.addEventListener(City.RESOURCES_UPDATE, onResourceChange);

			frame.addEventListener(PopupEvent.POPUP_CLOSED, function(e: PopupEvent):void {
				city.removeEventListener(City.RESOURCES_UPDATE, onResourceChange);
			});

			Global.gameContainer.showFrame(frame);
			return frame;
		}

		private function onResourceChange(e: Event) : void {
			drawResources();
		}

		private function dispose(): void {
			gridLocalActions.dispose();
		}

		private function simpleLabelMaker(tooltip: String, icon: Icon = null) : JLabel {
			var label: JLabel = new JLabel("", icon);

			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.RIGHT);
			label.setHorizontalAlignment(AsWingConstants.LEFT);

			new SimpleTooltip(label, tooltip);

			return label;
		}

		private function simpleLabelText(value: String, hourly: Boolean = false, negative: Boolean = false) : String {
			return (hourly?(negative?"-":"+"):"") + value + (hourly?" per hour":"");
		}

		private function resourceLabelMaker(tooltip: String, icon: Icon = null) : JLabel {
			var label: JLabel = new JLabel("", icon);

			label.setIconTextGap(0);
			label.setHorizontalTextPosition(AsWingConstants.RIGHT);
			label.setHorizontalAlignment(AsWingConstants.LEFT);

			new SimpleTooltip(label, tooltip);

			return label;
		}

		private function resourceLabelText(resource: LazyValue, includeLimit: Boolean = true, includeRate: Boolean = true) : String {
			var value: int = resource.getValue();

			return value + (includeLimit ? "/" + resource.getLimit() : "") + (includeRate ? " (+" + resource.getHourlyRate() + " per hour)" : "");
		}

		private function drawResources() : void {
			lblGold.setText(resourceLabelText(city.resources.gold, false, false));
			lblWood.setText(resourceLabelText(city.resources.wood));
			lblCrop.setText(resourceLabelText(city.resources.crop));
			lblIron.setText(resourceLabelText(city.resources.iron));
			lblLabor.setText(simpleLabelText(city.resources.labor.getValue().toString() + " " + StringHelper.makePlural(city.resources.labor.getValue(), "is", "are", "are") + " idle and " + city.getBusyLaborCount().toString() + " " + StringHelper.makePlural(city.getBusyLaborCount(), "is", "are", "are") + " working", false, false));
			lblUpkeep.setText(simpleLabelText(city.resources.crop.getUpkeep().toString(), true, true));
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

			lblGold = resourceLabelMaker("Gold", new AssetIcon(new ICON_GOLD()));
			lblWood = resourceLabelMaker("Wood", new AssetIcon(new ICON_WOOD()));
			lblCrop = resourceLabelMaker("Crop", new AssetIcon(new ICON_CROP()));
			lblIron = resourceLabelMaker("Iron", new AssetIcon(new ICON_IRON()));
			lblLabor = simpleLabelMaker("Laborer", new AssetIcon(new ICON_LABOR()));
			lblUpkeep = simpleLabelMaker("Troop Upkeep", new AssetIcon(new ICON_CROP()));

			pnlResources.append(lblGold);
			pnlResources.append(lblWood);
			pnlResources.append(lblCrop);
			pnlResources.append(lblIron);
			pnlResources.append(lblLabor);
			pnlResources.append(lblUpkeep);

			//component layoution
			append(pnlResources);
			append(pnlLocalEvents);

			drawResources();
		}

	}
}

