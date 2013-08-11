package src.UI.Dialog 
{
    import flash.events.Event;

    import mx.utils.*;

    import org.aswing.*;
    import org.aswing.event.InteractiveEvent;
    import org.aswing.ext.*;

    import src.*;
    import src.Map.*;
    import src.Objects.Effects.*;
    import src.Objects.Troop.*;
    import src.UI.*;
    import src.UI.Components.TroopPartialRetreatPanel;
    import src.Util.*;

    public class TroopTransferDialog extends GameJPanel
	{
		private var strongholds:Array;
		private var city: City;
		private var pickList: JComboBox;
		private var lblTime: JLabel;
        private var pnlPartialRetreat: TroopPartialRetreatPanel;
        private var originalStub: TroopStub;
		
		public function TroopTransferDialog(originalStub: TroopStub, strongholds : Array)
		{
            this.originalStub = originalStub;
			this.strongholds = strongholds;
			this.strongholds.sortOn("name");
			this.city = Global.map.cities.get(originalStub.cityId);
			this.pnlPartialRetreat = new TroopPartialRetreatPanel(originalStub, "TRANSFER");

			for (var i:int = 0; i < this.strongholds.length; ++i) {
				if (this.strongholds[i].id == originalStub.stationedLocation.strongholdId) {
					this.strongholds.splice(i, 1);
                    break;
				}
			}
			
			createUI();

            this.pnlPartialRetreat.addEventListener(InteractiveEvent.SELECTION_CHANGED, function(e: Event): void {
                if (getFrame()) {
                    getFrame().pack();
                    Util.centerFrame(getFrame());
                }

                recalculateTime();
            });

            this.pnlPartialRetreat.addChangeListener(function(e: Event): void {
                recalculateTime();
            });
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame
		{			
			var options : * = [];
			for (var i:int = 0; i < strongholds.length; ++i) {
				options.push(StringUtil.substitute("Lvl {0} - {1} ({2},{3})", strongholds[i].lvl, strongholds[i].name, strongholds[i].x, strongholds[i].y));
			}
			
			if (options.length == 0) {
				InfoDialog.showMessageDialog(StringHelper.localize("TRANSFER_TITLE"), StringHelper.localize("TRANSFER_NO_OTHER_STRONGHOLD"));
				return null;
			}
			
			pickList.setListData(options);
			pickList.setSelectedIndex(0);
			
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}

        private function recalculateTime(): void
        {
            var stub: TroopStub = pnlPartialRetreat.getTroop();
            if (stub.getIndividualUnitCount() == 0) {
                lblTime.setText("--");
                return;
            }

            var distance: int = MapUtil.distance(strongholds[pickList.getSelectedIndex()].x, strongholds[pickList.getSelectedIndex()].y, originalStub.x, originalStub.y);
            var timeAwayInSeconds: int = Formula.moveTimeTotal(city, stub.getSpeed(city), distance, false);
            lblTime.setText(StringHelper.localize("TRANSFER_ARRIVE_IN", DateUtil.niceTime(timeAwayInSeconds)));
        }

		private function createUI(): void
		{
			title = StringHelper.localize("TRANSFER_TITLE");
			setPreferredWidth(350);
			
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			
			append(new JLabel(StringHelper.localize("TRANSFER_CHOOSE_STRONGHOLD"),null,AsWingConstants.LEFT));
			
			pickList = new JComboBox();
			pickList.addActionListener(function (e: Event): void {
                recalculateTime();
            });
			append(pickList);

			var warning: MultilineLabel = new MultilineLabel(StringHelper.localize("TRANSFER_WARNING"), 3, 0);
			append(warning);

            append(pnlPartialRetreat);

			lblTime = new JLabel();
            lblTime.setIcon(new AssetIcon(new ICON_CLOCK()));
			lblTime.setHorizontalAlignment(AsWingConstants.RIGHT);
			append(lblTime);
			
			var pnlButtons: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 10, AsWingConstants.CENTER));
			var btnSend: JButton = new JButton(StringHelper.localize("TRANSFER_SEND"));
			btnSend.addActionListener(function():void {

                if (pnlPartialRetreat.getTroop().getIndividualUnitCount() == 0) {
                    InfoDialog.showMessageDialog(StringHelper.localize("STR_ERROR"), StringHelper.localize("TRANSFER_REQUIRED_ERROR"));
                    return;
                }

                Global.mapComm.Troop.troopTransfer(pnlPartialRetreat.shouldRetreatAll(), pnlPartialRetreat.getTroop(), originalStub.cityId, originalStub.id, strongholds[pickList.getSelectedIndex()].id);

				if (getFrame()) {
					getFrame().dispose();
				}
			});
			pnlButtons.append(btnSend);
			
			var btnCancel: JButton = new JButton(StringHelper.localize("TRANSFER_CANCEL"));
			btnCancel.addActionListener(function():void {
				getFrame().dispose();
			});
			pnlButtons.append(btnCancel);

			append(pnlButtons);
		}
	}
}