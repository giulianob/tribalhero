package src.UI.Components.BattleReport
{

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Util.StringHelper;
	import src.Util.Util;

	public class OutcomeSnapshot extends JPanel
	{

		private var pnlEvents:JPanel;
		private var pnlTroops:JPanel;
		private var pnlUnits:JTabbedPane;
		private var border: SimpleTitledBorder;

		public function OutcomeSnapshot(snapshot: Object)
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

			//Units
			pnlUnits.appendTab(new TroopTable(snapshot.troop), snapshot.troop.name);
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
			var layout3:GridLayout = new GridLayout();
			layout3.setRows(1);
			layout3.setColumns(1);
			layout3.setHgap(10);
			pnlTroops.setLayout(layout3);			

			pnlUnits = new JTabbedPane();

			//component layoution
			append(pnlEvents);
			append(pnlTroops);

			pnlTroops.append(pnlUnits);
		}
	}

}

