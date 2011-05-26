package src.UI.Dialog{

	import src.Constants;
	import src.Global;
	import src.UI.GameJPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.UI.LookAndFeel.GameLookAndFeel;

	public class TribeSetRankDialog extends GameJPanel {

		private var lblName:JLabel;
		private var lblNameTitle:JLabel;		
		private var btnAccept:JButton;
		private var btnDecline:JButton;
		private var lblMessage: MultilineLabel;
		private var lstRank: JComboBox;
		
		private var playerId: int;
		private var currentRank: int;

		public function TribeSetRankDialog(playerId: int, currentRank: int, onAccept: Function) {		
			this.playerId = playerId;
			this.currentRank = currentRank;
			
			createUI();			

			Global.map.usernames.players.setLabelUsername(playerId, lblName);
			
			var self: TribeSetRankDialog = this;
			btnAccept.addActionListener(function():void {				
				if (onAccept != null) onAccept(self);
			});
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			return frame;
		}
		
		public function getNewRank(): int {
			return lstRank.getSelectedIndex() + 1;
		}

		private function createUI():void {
			title = "Set Member Rank";
			//component creation
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			setPreferredWidth(350);			
			var border0:EmptyBorder = new EmptyBorder();
			border0.setTop(10);
			border0.setLeft(10);
			border0.setBottom(10);
			border0.setRight(10);
			setBorder(border0);

			var pnlName: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));
			pnlName.setPreferredWidth(150);
			
			lblNameTitle = new JLabel("Player");
			lblNameTitle.setHorizontalAlignment(AsWingConstants.RIGHT);

			lblName = new JLabel();
			lblName.setText("Loading...");
			GameLookAndFeel.changeClass(lblName, "Form.label");
			lblName.setHorizontalAlignment(AsWingConstants.LEFT);
			
			var pnlNewRank: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));
			pnlNewRank.setPreferredWidth(150);
			
			var lblNewRankTitle: JLabel = new JLabel("Rank");
			lblNewRankTitle.setHorizontalAlignment(AsWingConstants.RIGHT);
			
			lstRank = new JComboBox(["Elder", "Tribesman"]);
			lstRank.setPreferredWidth(100);
			lstRank.setSelectedIndex(currentRank - 1);

			var pnlButtons: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));
			pnlButtons.setPreferredWidth(150);

			btnAccept = new JButton();
			btnAccept.setText("Set New Rank");

			lblMessage = new MultilineLabel("Since you are the chief of your tribe, you are the only one that can change the rank of your members. Elders have the ability to recruit/kick tribesmen and can moderate the message board. The Tribesman rank has no administrative privilege.", 5);

			//component layout
			append(lblMessage);
			append(pnlName);
			append(pnlNewRank);
			append(pnlButtons);

			pnlNewRank.append(lblNewRankTitle);
			pnlNewRank.append(lstRank);			
			
			pnlName.append(lblNameTitle);
			pnlName.append(lblName);
			
			pnlButtons.append(btnAccept);					
		}
	}
}

