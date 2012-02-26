package src.UI.Dialog
{
	import flash.events.Event;
	import org.aswing.event.InteractiveEvent;
	import org.aswing.event.PopupEvent;
	import src.Comm.Packet;
	import src.Global;
	import src.Map.City;
	import src.Map.Username;
	import src.Objects.Effects.Formula;
	import src.Objects.GameError;
	import src.Objects.GameObject;
	import src.Objects.Resources;
	import src.Objects.StructureObject;
	import src.UI.Components.SimpleTooltip;
	import src.UI.GameJPanel;
	import src.Util.Util;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class TribeContributeDialog extends GameJPanel
	{
		private var lblCityTitle:JLabel;
		private var lstCities:JComboBox;

		private var pnlResources:JPanel;

		private var pnlCrop:JPanel;
		private var lblCropAmount:JAdjuster;

		private var pnlIron:JPanel;
		private var lblIronAmount:JAdjuster;

		private var pnlWood:JPanel;
		private var lblWoodAmount:JAdjuster;

		private var pnlGold:JPanel;
		private var lblGoldAmount:JAdjuster;

		private var pnlBottom:JPanel;
		private var btnOk:JButton;

		private var city: City;
		
		private var onAccept: Function;
		
		public function TribeContributeDialog(onAccept: Function):void
		{
			this.onAccept = onAccept;
			
			createUI();	
			
			title = "Contribute Resources";							
			
			btnOk.addActionListener(sendResources);
			
			for each (var city: City in Global.map.cities.each())
				(lstCities.getModel() as VectorListModel).append( { id: city.id, city: city, toString: function() : String { return this.city.name; } } );		
				
			lstCities.addEventListener(InteractiveEvent.SELECTION_CHANGED, onChangeCitySelection);
			lstCities.setSelectedIndex(Global.gameContainer.getSelectedCityIndex());
			
			onChangeCitySelection();
		}
		
		public function sendResources(e: Event) : void {						
			if (amount().total() == 0) {
				InfoDialog.showMessageDialog("Error", "No resources selected to contribute");
				return;
			}		

			var self: TribeContributeDialog = this;
			Global.mapComm.Tribe.contribute(city.id, amount(), function(): void {
				if (onAccept != null)
					onAccept(self);
			});
		}	

		public function amount(): Resources {
			return new Resources(lblCropAmount.getValue(), lblGoldAmount.getValue(), lblIronAmount.getValue(), lblWoodAmount.getValue(), 0);
		}		
		
		private function onChangeCitySelection(e: Event = null): void {
			if (city != null) 
				city.removeEventListener(City.RESOURCES_UPDATE, onResourceChange);
			
			city = lstCities.getSelectedItem().city;
			
			city.addEventListener(City.RESOURCES_UPDATE, onResourceChange);
			
			onResourceChange();
		}
		
		public function dispose(): void {
			if (city != null) 
				city.removeEventListener(City.RESOURCES_UPDATE, onResourceChange);
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose, dispose);			

			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function onResourceChange(e: Event = null) : void {
			lblWoodAmount.setMaximum(city.resources.wood.getValue());
			lblCropAmount.setMaximum(city.resources.crop.getValue());
			lblGoldAmount.setMaximum(city.resources.gold.getValue());
			lblIronAmount.setMaximum(city.resources.iron.getValue());
		}
		
		public function createUI(): void {
			setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 0, AsWingConstants.TOP));

			var pnlCity: JPanel = new JPanel(new SoftBoxLayout(AsWingConstants.HORIZONTAL, 5));
			
			lblCityTitle = new JLabel("City:");
			
			lstCities = new JComboBox();
			lstCities.setModel(new VectorListModel());			
			lstCities.setPreferredSize(new IntDimension(128, 22));
			
			pnlCity.append(lblCityTitle);
			pnlCity.append(lstCities);
			
			var lblTitle1: JLabel = new JLabel();
			lblTitle1.setSize(new IntDimension(200, 17));
			lblTitle1.setText("Choose amount of resources to contribute");
			lblTitle1.setHorizontalAlignment(AsWingConstants.LEFT);

			pnlResources = new JPanel();
			pnlResources.setLayout(new FlowLayout(AsWingConstants.LEFT, 10, 10));

			pnlCrop = new JPanel();
			pnlCrop.setLayout(new FlowLayout(AsWingConstants.LEFT, 5, 0));

			lblCropAmount = new JAdjuster(3);
			lblCropAmount.setPreferredWidth(65);
			lblCropAmount.setMinimum(0);
			lblCropAmount.setValue(0);

			pnlIron = new JPanel();
			pnlIron.setLayout(new FlowLayout(AsWingConstants.LEFT, 5, 0));

			lblIronAmount = new JAdjuster(3);
			lblIronAmount.setPreferredWidth(65);
			lblIronAmount.setMinimum(0);
			lblIronAmount.setValue(0);

			pnlWood = new JPanel();
			pnlWood.setLayout(new FlowLayout(AsWingConstants.CENTER, 5, 0));

			lblWoodAmount = new JAdjuster(3);
			lblWoodAmount.setPreferredWidth(65);
			lblWoodAmount.setMinimum(0);
			lblWoodAmount.setValue(0);

			pnlGold = new JPanel();
			pnlGold.setLayout(new FlowLayout(AsWingConstants.LEFT, 5, 0));

			lblGoldAmount = new JAdjuster(3);
			lblGoldAmount.setPreferredWidth(65);
			lblGoldAmount.setMinimum(0);
			lblGoldAmount.setValue(0);

			pnlBottom = new JPanel();
			pnlBottom.setSize(new IntDimension(200, 10));
			pnlBottom.setLayout(new FlowLayout(AsWingConstants.CENTER));

			btnOk = new JButton("Contribute");

			//component layoution
			append(pnlCity);
			append(new JLabel(" ")); //separator
			append(lblTitle1);
			append(pnlResources);
			append(pnlBottom);

			pnlResources.append(pnlGold);
			pnlResources.append(pnlWood);
			pnlResources.append(pnlCrop);
			pnlResources.append(pnlIron);

			var icon: AssetPane = new AssetPane(new ICON_GOLD());
			new SimpleTooltip(icon, "Gold");
			pnlGold.append(icon);
			pnlGold.append(lblGoldAmount);

			icon = new AssetPane(new ICON_CROP());
			new SimpleTooltip(icon, "Crop");
			pnlCrop.append(icon);
			pnlCrop.append(lblCropAmount);

			icon = new AssetPane(new ICON_IRON());
			new SimpleTooltip(icon, "Iron");
			pnlIron.append(icon);
			pnlIron.append(lblIronAmount);

			icon = new AssetPane(new ICON_WOOD());
			new SimpleTooltip(icon, "Wood");
			pnlWood.append(icon);
			pnlWood.append(lblWoodAmount);

			pnlBottom.append(btnOk);
		}
	}

}
