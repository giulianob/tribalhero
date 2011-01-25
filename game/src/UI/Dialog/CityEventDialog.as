package src.UI.Dialog{

	import fl.lang.Locale;
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
	import src.Objects.Effects.Formula;
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
		private var pnlLocalEvents:JTabbedPane;

		private var gridLocalActions: CityActionGridList;

		private var lblGold: JLabel;
		private var lblWood: JLabel;
		private var lblCrop: JLabel;
		private var lblIron: JLabel;
		private var lblLabor: JLabel;
		private var lblUpkeep: JLabel;
		private var lblAttackPoints: JLabel;
		private var lblDefensePoints: JLabel;
		private var lblUpkeepMsg: JLabel;

		private var city: City;

		public function CityEventDialog(city: City) {
			this.city = city;

			title = "Overview - " + city.name;
			
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
			lblUpkeepMsg.setVisible(city.resources.crop.getRate() < city.resources.crop.getUpkeep());
			lblAttackPoints.setText(city.attackPoint + " attack points");
			lblDefensePoints.setText(city.defensePoint + " defense points");

			if (getFrame() != null) {
				getFrame().pack();
			}
		}

		private function createUI(): void {
			//component creation
			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(10);
			setLayout(layout0);

			pnlResources = new JPanel(new GridLayout(4, 2, 20, 10));

			pnlLocalEvents = new JTabbedPane();
			pnlLocalEvents.setPreferredSize(new IntDimension(540, 235));
			pnlLocalEvents.appendTab(new JScrollPane(gridLocalActions), "Local Events");

			lblGold = resourceLabelMaker("Gold\n\n" + Locale.loadString("GOLD_DESC"), new AssetIcon(new ICON_GOLD()));
			lblWood = resourceLabelMaker("Wood\n\n" + Locale.loadString("WOOD_DESC"), new AssetIcon(new ICON_WOOD()));
			lblCrop = resourceLabelMaker("Crop\n\n" + Locale.loadString("CROP_DESC"), new AssetIcon(new ICON_CROP()));
			lblIron = resourceLabelMaker("Iron\n\n" + Locale.loadString("IRON_DESC"), new AssetIcon(new ICON_IRON()));

			var laborTime: String = "Currently receiving 1 labor every " + Util.niceTime(Formula.laborRate(city), false);
			lblLabor = simpleLabelMaker("Laborer\n\n" + Locale.loadString("LABOR_DESC") + "\n\n" + laborTime, new AssetIcon(new ICON_LABOR()));

			lblUpkeep = simpleLabelMaker("Troop Upkeep\n\n" + Locale.loadString("UPKEEP_DESC"), new AssetIcon(new ICON_CROP()));
			lblDefensePoints = simpleLabelMaker("Defense Points\n\n" + Locale.loadString("DEFENSE_POINTS_DESC"), new AssetIcon(new ICON_SHIELD()));
			lblAttackPoints = simpleLabelMaker("Attack Points\n\n" + Locale.loadString("ATTACK_POINTS_DESC"), new AssetIcon(new ICON_BATTLE()));

			lblUpkeepMsg = new JLabel("Your troop upkeep currently exceeds your crop production rate. Your units will slowly starve to death.", new AssetIcon(new ICON_CROP()));
			lblUpkeepMsg.setBorder(new LineBorder(null, new ASColor(0xff0000), 1, 10));

			pnlResources.append(lblGold);
			pnlResources.append(lblWood);
			pnlResources.append(lblCrop);
			pnlResources.append(lblIron);
			pnlResources.append(lblLabor);
			pnlResources.append(lblUpkeep);
			pnlResources.append(lblDefensePoints);
			pnlResources.append(lblAttackPoints);

			//component layoution
			append(lblUpkeepMsg);
			append(pnlResources);
			append(pnlLocalEvents);

			drawResources();
		}

	}
}

