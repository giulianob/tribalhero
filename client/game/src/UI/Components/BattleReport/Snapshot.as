package src.UI.Components.BattleReport
{

    import mx.utils.*;

    import org.aswing.*;
    import org.aswing.border.*;

    import src.*;
    import src.UI.*;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.Util.*;

    public class Snapshot extends GameJPanel
	{
		private var snapshot:Object;

		public function Snapshot(snapshot: Object)
		{
			this.snapshot = snapshot;
			
			createUI();
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			frame.setTitle("Battle Detail");
			
			frame.resizeToContents();			
			Util.centerFrame(frame);			
			
			return frame;
		}		
		
		private function troopName(troop: * ): String {
			return StringUtil.substitute("{0} ({1})", troop.owner.name, troop.name == '[LOCAL]' ? StringHelper.localize("LOCAL_TROOP") : troop.name);
		}

		private function createUI(): void {			
			setPreferredWidth(1100);
			
			setLayout(new BorderLayout());
            {
                var pnlHolder: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
                {
                    var lblTitle: JLabel = new JLabel("", null, AsWingConstants.LEFT);
                    if (snapshot.time == 0) {
                        lblTitle.setText(StringUtil.substitute("Round {0}, Turn {1}: At the beginning of the battle.", int(snapshot.round) + 1, int(snapshot.turn) + 1));
                    }
                    else {
                        lblTitle.setText(StringUtil.substitute("Round {1}, Turn {2}: {0} into the battle.", StringHelper.firstToUpper(DateUtil.niceTime(snapshot.time)), int(snapshot.round) + 1, int(snapshot.turn) + 1));
                    }

                    GameLookAndFeel.changeClass(lblTitle, "darkHeader");

                    var pnlTroops: JPanel = new JPanel(new GridLayout(1, 2, 0, 0));
                    {
                        var pnlDefense: JTabbedPane = new JTabbedPane();
                        pnlDefense.setBorder(new SimpleTitledBorder(null, "Defense", AsWingConstants.TOP, AsWingConstants.LEFT));
                        {
                            for each (var defense: Object in snapshot.defenders) {
                                pnlDefense.appendTab(new TroopTable(defense.units), troopName(defense));
                            }
                        }

                        var pnlAttack: JTabbedPane = new JTabbedPane();
                        pnlAttack.setBorder(new SimpleTitledBorder(null, "Attack", AsWingConstants.TOP, AsWingConstants.LEFT));
                        {
                            for each (var attack: Object in snapshot.attackers) {
                                pnlAttack.appendTab(new TroopTable(attack.units), troopName(attack));
                            }
                        }

                        pnlTroops.appendAll(pnlDefense, pnlAttack);
                    }

                    pnlHolder.appendAll(lblTitle, pnlTroops);
                }

                var viewport: JViewport = new JViewport(pnlHolder, true, false);
                viewport.setVerticalAlignment(AsWingConstants.TOP);
                var scrollPanel: JScrollPane = new JScrollPane(viewport);
                scrollPanel.setViewport(viewport);
                scrollPanel.setConstraints("Center");
                append(scrollPanel);
            }
		}
	}

}
