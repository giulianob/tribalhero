package src.UI.Components.BattleReport
{

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Util.StringHelper;
	import src.Util.Util;

	public class Snapshot extends JPanel
	{

		private var pnlEvents:JPanel;
		private var pnlTroops:JPanel;
		private var pnlDefense:JTabbedPane;
		private var pnlAttack:JTabbedPane;
		private var border: SimpleTitledBorder;

		public function Snapshot(snapshot: Object)
		{
			createUI();

			//Time information
			if (snapshot.time == 0)
			{
				border.setTitle("At the beginning of the battle");
			}
			else
			{
				border.setTitle(StringHelper.firstToUpper(Util.niceTime(snapshot.time)) + " into the battle");
			}

			pnlEvents.append(new JLabel("Round " + (int(snapshot.round) + 1) + " Turn " + (int(snapshot.turn) + 1), null, AsWingConstants.LEFT));
			
			//Battle events
			for each (var event: Object in snapshot.events) {
				pnlEvents.append(new JLabel(event.toString(), null, AsWingConstants.LEFT));
			}

			//Defenders
			for each (var defense: Object in snapshot.defenders)
			pnlDefense.appendTab(new TroopTable(defense), defense.name);

			//Attackers
			for each (var attack: Object in snapshot.attackers)
			pnlAttack.appendTab(new TroopTable(attack), attack.name);
		}

		private function createUI(): void {
			setPreferredWidth(530);
			border = new SimpleTitledBorder(null, "", AsWingConstants.TOP, AsWingConstants.LEFT, 0, new ASFont("Arial", 13, true));
			setBorder(border);

			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(10);
			setLayout(layout0);

			pnlEvents = new JPanel();
			var border1:EmptyBorder = new EmptyBorder(null, new Insets(5));
			pnlEvents.setBorder(border1);
			var layout2:SoftBoxLayout = new SoftBoxLayout();
			layout2.setAxis(AsWingConstants.VERTICAL);
			pnlEvents.setLayout(layout2);

			pnlTroops = new JPanel();
			pnlTroops.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0, AsWingConstants.CENTER));

			pnlDefense = new JTabbedPane();
			pnlDefense.setBorder(new SimpleTitledBorder(null, "Defense", AsWingConstants.TOP, AsWingConstants.LEFT));

			pnlAttack = new JTabbedPane();
			pnlAttack.setBorder(new SimpleTitledBorder(null, "Attack", AsWingConstants.TOP, AsWingConstants.LEFT));

			//component layoution
			append(pnlEvents);
			append(pnlTroops);

			pnlTroops.append(pnlDefense);
			pnlTroops.append(pnlAttack);
		}
	}

}
