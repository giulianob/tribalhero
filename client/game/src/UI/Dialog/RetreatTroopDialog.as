package src.UI.Dialog {

    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.event.InteractiveEvent;


    import src.*;
    import src.Map.*;
    import src.Objects.Troop.*;
    import src.UI.*;
    import src.UI.Components.TroopPartialRetreatPanel;
    import src.Util.StringHelper;
    import src.Util.Util;

    public class RetreatTroopDialog extends GameJPanel {

		//members define
		protected var pnlButton:JPanel;
		protected var btnOk:JButton;

		protected var city: City;
        protected var pnlPartialRetreat: TroopPartialRetreatPanel;

		public function RetreatTroopDialog(troop: TroopStub, onAccept: Function):void
		{
			title = StringHelper.localize("RETREAT_TROOP_DIALOG_TITLE");

			this.city = Global.map.cities.get(troop.cityId);
            this.pnlPartialRetreat = new TroopPartialRetreatPanel(troop, "RETREAT");

			createUI();			

			var self: RetreatTroopDialog = this;
			btnOk.addActionListener(function():void { 
				if (getTroop().getIndividualUnitCount() == 0) {
					InfoDialog.showMessageDialog(StringHelper.localize("STR_ERROR"), StringHelper.localize("RETREAT_TROOP_DIALOG_REQUIRED_ERROR"));
					return;
				}
				
				if (onAccept != null) onAccept(self); 
			} );

            this.pnlPartialRetreat.addEventListener(InteractiveEvent.SELECTION_CHANGED, function(e: Event): void {
                if (getFrame()) {
                    getFrame().pack();
                    Util.centerFrame(getFrame());
                }
            });
		}

        public function shouldRetreatAll(): Boolean
        {
            return pnlPartialRetreat.shouldRetreatAll();
        }

		public function getTroop(): TroopStub
		{
            return pnlPartialRetreat.getTroop();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}

		private function createUI(): void {
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
            setPreferredWidth(425);

			pnlButton = new JPanel(new FlowLayout(AsWingConstants.CENTER));

			btnOk = new JButton();
			btnOk.setText("Ok");

            append(pnlPartialRetreat);
			append(pnlButton);

			pnlButton.append(btnOk);
		}
	}

}

