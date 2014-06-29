package src.UI.Dialog
{
import flash.events.Event;

import org.aswing.*;
import org.aswing.geom.*;

import src.Comm.Packet;
import src.Global;
import src.Map.City;
import src.Objects.GameError;
import src.Objects.Resources;
import src.Objects.StructureObject;
import src.UI.Components.TradeResourcesPanel;
import src.UI.GameJPanel;

public class ResourceWithdrawDialog extends GameJPanel
	{

		private var pnlResources:TradeResourcesPanel;
		private var btnOk:JButton;

		public var city: City;
		
		private var onAccept: Function;
		private var loadingDlg: InfoDialog;
		
		private var parentObj: StructureObject;

        private var stored: Resources;

		public function ResourceWithdrawDialog(parentObj: StructureObject, onAccept: Function):void
		{
			this.onAccept = onAccept;
			this.parentObj = parentObj;

            city = Global.map.cities.get(parentObj.cityId);
            stored = new Resources(
                    parentObj.properties[0],
                    parentObj.properties[2],
                    parentObj.properties[3],
                    parentObj.properties[1],
                    0
            );
			createUI();

            title = "Withdraw Resources";
			btnOk.addActionListener(onWithdraw);

		}

		public function onWithdraw(e: Event) : void {
			if (amount().total() == 0) {
				InfoDialog.showMessageDialog("Error", "No resources selected to send");
				return;
			}

			loadingDlg = InfoDialog.showMessageDialog("Withdraw Resources", "Withdrawing...", null, null, true, false, 0);

			Global.mapComm.City.withdrawResources(amount(), parentObj.cityId, parentObj.objectId, onWithdrawComplete);
		}

        public function onWithdrawComplete(packet: Packet, custom: * = null) : void {
			
			if (loadingDlg) loadingDlg.getFrame().dispose();
			
			if ((packet.option & Packet.OPTIONS_FAILED) == Packet.OPTIONS_FAILED)
			{
				var err: int = packet.readUInt();
				GameError.showMessage(err);
				return;
            }

			var infoPanel: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

		    infoPanel.append(new JLabel("Resources are transfered from Cranny to the City."));

			InfoDialog.showMessageDialog("Withdraw Resources", infoPanel, null, null, true, false, JOptionPane.OK);

            onAccept(this);
        }

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}

		public function amount(): Resources {
			return pnlResources.getResource();
		}

		public function createUI(): void {
            setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 0, AsWingConstants.TOP));

            var capacity:Resources = new Resources(
                    Math.min(stored.crop, city.resources.crop.getLimit() - city.resources.crop.getValue()),
                    Math.min(stored.gold, 99999),
                    Math.min(stored.iron, city.resources.iron.getLimit() - city.resources.iron.getValue()),
                    Math.min(stored.wood, city.resources.wood.getLimit() - city.resources.wood.getValue()),
                    0
            );

            pnlResources = new TradeResourcesPanel(parentObj, capacity, null, false);

            var pnlBottom:JPanel = new JPanel();
            pnlBottom.setSize(new IntDimension(200, 10));
            pnlBottom.setLayout(new FlowLayout(AsWingConstants.CENTER));

            btnOk = new JButton("Withdraw");

            //component layoution
            append(new JLabel(" ")); //separator
            append(pnlResources);
            append(pnlBottom);

            pnlBottom.append(btnOk);
        }
	}

}
