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
	import src.UI.Components.TradeResourcesPanel;
	import src.UI.GameJPanel;
	import src.Util.Util;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class SendResourceDialog extends GameJPanel
	{
		private var lblCityTitle: JLabel;		
		private var txtCityName: AutoCompleteTextField;
		private var pnlResources:TradeResourcesPanel;

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
			Global.gameContainer.showFrame(frame);
			return frame;
		}

		public function cityName(): String {
			return txtCityName.getText();
		}
		
		public function amount(): Resources {
			return pnlResources.getResource();
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
		
			pnlResources = new TradeResourcesPanel(parentObj, Formula.sendCapacity(parentObj.level));

			pnlBottom = new JPanel();
			pnlBottom.setSize(new IntDimension(200, 10));
			pnlBottom.setLayout(new FlowLayout(AsWingConstants.CENTER));

			btnOk = new JButton("Send");

			//component layoution
			append(pnlCity);
			append(new JLabel(" ")); //separator
			append(pnlResources);
			append(pnlBottom);

			pnlBottom.append(btnOk);
		}
	}

}
