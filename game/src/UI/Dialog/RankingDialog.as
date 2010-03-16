package src.UI.Dialog{

	import src.Global;
	import src.UI.GameJPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class RankingDialog extends GameJPanel {
		
		private var tabs: JTabbedPane;
		private var cityRanking: JPanel;
		private var cityAttackRanking: JToggleButton;
		private var cityDefenseRanking: JToggleButton;
		private var cityLootRanking: JToggleButton;
		
		private var playerRanking: JPanel;		
		private var playerAttackRanking: JToggleButton;
		private var playerDefenseRanking: JToggleButton;
		private var playerLootRanking: JToggleButton;
		
		public function RankingDialog() {
			createUI();
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null) :JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI():void {
			title = "Ranking";
			setLayout(new BorderLayout());
			setSize(new IntDimension(350, 500));

			cityRanking = new JPanel(new BorderLayout(0, 10));
			cityAttackRanking = new JToggleButton("Attack");
			cityDefenseRanking = new JToggleButton("Defense");
			cityLootRanking = new JToggleButton("Loot");
			var cityButtonGroup: ButtonGroup = new ButtonGroup();						
			cityButtonGroup.appendAll(cityAttackRanking, cityDefenseRanking, cityLootRanking);
			var cityButtonGroupHolder: JPanel = new JPanel();
			cityButtonGroupHolder.setBorder(new SimpleTitledBorder(null, "Choose a ranking", AsWingConstants.TOP, AsWingConstants.LEFT));
			cityButtonGroupHolder.appendAll(cityAttackRanking, cityDefenseRanking, cityLootRanking);
			cityRanking.append(cityButtonGroupHolder);
			
			tabs = new JTabbedPane();
			tabs.setConstraints("Center");
//			tabs.appendTab(playerRanking, "Player");
			tabs.appendTab(cityRanking, "City");			
			
			append(tabs);
		}
	}
}

