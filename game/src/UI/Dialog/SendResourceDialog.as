package src.UI.Dialog
{
	import flash.events.Event;
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
	import src.UI.Components.AutoCompleteTextField;
	import src.UI.Components.SimpleTooltip;
	import src.UI.GameJPanel;
	import src.Util.Util;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class SendResourceDialog extends GameJPanel
	{
		private var lblTitle1:JLabel;
		private var lblCityTitle: JLabel;
		
		private var txtCityName: AutoCompleteTextField;

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

		public var city: City;
		
		private var onAccept: Function;
		private var loadingDlg: InfoDialog;
		
		private var parentObj: StructureObject;
		
		public function SendResourceDialog(parentObj: StructureObject, onAccept: Function):void
		{
			this.onAccept = onAccept;
			this.parentObj = parentObj;
			
			createUI();

			title = "Send Resources";			
			
			btnOk.addActionListener(requestUsername);

			city = Global.map.cities.get(parentObj.cityId);

			onResourceChange();
		}
		
		public function requestUsername(e: Event) : void {						
			if (amount().total() == 0) {
				InfoDialog.showMessageDialog("Error", "No resources selected to send");
				return;
			}
			
			if (txtCityName.getText().length == 0) {
				InfoDialog.showMessageDialog("Error", "You have to type in the city names you want to send some resources to");
				return;				
			}
			
			loadingDlg = InfoDialog.showMessageDialog("Send Resources", "Validating...", null, null, true, false, 0);		
			
			Global.mapComm.City.getSendResourcesConfirmation(amount(), parentObj.cityId, parentObj.objectId, txtCityName.getText(), onReceiveTradeInformation);			
		}
		
		public function onReceiveTradeInformation(packet: Packet, custom: * = null) : void {
			
			if (loadingDlg) loadingDlg.getFrame().dispose();
			
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
			{
				var err: int = packet.readUInt();
				GameError.showMessage(err);
				return;
			}
			
			var playerName: String = packet.readString();
			var tradeTime: int = packet.readInt();
			
			var infoPanel: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			infoPanel.append(new JLabel(Util.formatTime(tradeTime), new AssetIcon(new ICON_CLOCK()), AsWingConstants.RIGHT));
			infoPanel.append(new JLabel("You have chosen to send " + amount().toNiceString() + " to " + playerName + " " + txtCityName.getText() + "\n\nAre you sure?"));			
			
			InfoDialog.showMessageDialog("Confirm", infoPanel, onUserConfirms, null, true, false, JOptionPane.YES | JOptionPane.NO);
		}
		
		public function onUserConfirms(option: int): void {
			if (option == JOptionPane.YES) {			
				var self: SendResourceDialog = this;
				if (onAccept != null) onAccept(self);
			}
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);

			city.addEventListener(City.RESOURCES_UPDATE, onResourceChange);

			frame.addEventListener(PopupEvent.POPUP_CLOSED, function(e: PopupEvent):void {
				if (city != null) city.removeEventListener(City.RESOURCES_UPDATE, onResourceChange);
			});

			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function onResourceChange(e: Event = null) : void {
			
			var sendCapacity: int = Formula.sendCapacity(parentObj.level);
			
			lblWoodAmount.setMaximum(Math.min(sendCapacity, city.resources.wood.getValue()));
			lblCropAmount.setMaximum(Math.min(sendCapacity, city.resources.crop.getValue()));
			lblGoldAmount.setMaximum(Math.min(sendCapacity, city.resources.gold.getValue()));
			lblIronAmount.setMaximum(Math.min(sendCapacity, city.resources.iron.getValue()));
		}

		public function cityName(): String {
			return txtCityName.getText();
		}
		
		public function amount(): Resources {
			return new Resources(lblCropAmount.getValue(), lblGoldAmount.getValue(), lblIronAmount.getValue(), lblWoodAmount.getValue(), 0);
		}

		public function createUI(): void {
			setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 0, AsWingConstants.TOP));

			var pnlCity: JPanel = new JPanel(new SoftBoxLayout(AsWingConstants.HORIZONTAL, 5));
			
			lblCityTitle = new JLabel("Recipient City:");
			
			txtCityName = new AutoCompleteTextField(Global.mapComm.General.autoCompleteCity);
			txtCityName.setColumns(16);
			txtCityName.setMaxChars(32);			
			
			pnlCity.append(lblCityTitle);
			pnlCity.append(txtCityName);
			
			lblTitle1 = new JLabel();
			lblTitle1.setSize(new IntDimension(200, 17));
			lblTitle1.setText("Choose amount of resources to send");
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

			btnOk = new JButton("Send");

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
