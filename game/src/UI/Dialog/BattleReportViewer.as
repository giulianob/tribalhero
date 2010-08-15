package src.UI.Dialog
{
	import flash.events.Event;
	import src.Comm.GameURLLoader;
	import src.Objects.Resources;
	import src.UI.Components.BattleReport.OutcomeSnapshot;
	import src.UI.Components.BattleReport.Snapshot;
	import src.UI.Components.ResourcesPanel;
	import src.UI.GameJPanel;
	import src.Global;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.UI.Tooltips.BattleLootTooltip;
	import src.Util.Util;

	public class BattleReportViewer extends GameJPanel
	{

		private var pnlSnapshotsScroll: JScrollPane;
		private var pnlSnapshots: JPanel;
		private var pnlFooter:JPanel;
		private var chkViewAll:JCheckBox;
		private var pnlResources:JPanel;

		private var data: Object;

		private var loader: GameURLLoader = new GameURLLoader();
		private var id: int;
		private var isLocal: Boolean;

		public var refreshOnClose: Boolean;

		public function BattleReportViewer(id: int, isLocal: Boolean)
		{
			this.id = id;
			this.isLocal = isLocal;

			createUI();

			chkViewAll.addActionListener(function(): void {
				renderSnapshots();
			});

			loader.addEventListener(Event.COMPLETE, onLoaded);
			load();
		}

		private function load() : void {

			if (isLocal)
			Global.mapComm.BattleReport.viewLocal(loader, id);
			else
			Global.mapComm.BattleReport.viewRemote(loader, id);
		}

		private function onLoaded(e: Event) : void {
			try
			{
				data = loader.getDataAsObject();
				refreshOnClose = data.refreshOnClose;
				renderSnapshots();
				chkViewAll.setVisible(!data.outcomeOnly);
				chkViewAll.setEnabled(!data.outcomeOnly);

				// Show resources gained if applicable
				if (data.loot) {
					var loot: Resources = new Resources(data.loot.crop, data.loot.gold, data.loot.iron, data.loot.wood, 0);
					var bonus: Resources = new Resources(data.bonus.crop, data.bonus.gold, data.bonus.iron, data.bonus.wood, 0);
					var total: Resources = Resources.sum(loot, bonus);
					pnlResources.append(new ResourcesPanel(total, null, false, false));
					pnlFooter.append(pnlResources);
					new BattleLootTooltip(pnlResources, loot, bonus);
				}
				pack();
			}
			catch (e: Error) {
				InfoDialog.showMessageDialog("Error", "Unable to query report. Refresh the page if this problem persists");
				return;
			}
		}

		private function canSeeSnapshot(snapshot: Object) : Boolean {
			// Look for our player in the defense
			for each (var defense: Object in snapshot.defenders) {
				if (Global.map.cities.get(defense.cityId) != null) return true;
			}

			// Look for our player in the attack
			for each (var attack: Object in snapshot.attackers) {
				if (Global.map.cities.get(attack.cityId) != null) return true;
			}

			return false;
		}

		private function renderSnapshots() : void {

			chkViewAll.setText("View complete report");

			pnlSnapshots.removeAll();
			for each (var snapshot: Object in data.snapshots) {
				// Don't show this snapshot if we arent viewing them all
				// if it doesnt pertain to this player OR show them all regardless if this is an outcome only reports
				if (data.outcomeOnly == false && !chkViewAll.isSelected()) {
					if (!canSeeSnapshot(snapshot)) continue;
				}

				if (data.outcomeOnly)  {
					pnlSnapshotsScroll.setBorder(new SimpleTitledBorder(null, "Your troop was unable to report the enemies. You can only see a partial report"));
					pnlSnapshots.append(new OutcomeSnapshot(snapshot));

					//Resize accordingly
					setPreferredSize(new IntDimension(600, 520));
					getFrame().pack();
					Util.centerFrame(getFrame());
				}
				else {
					pnlSnapshotsScroll.setBorder(new SimpleTitledBorder(null, ""));
					pnlSnapshots.append(new Snapshot(snapshot));

					//Resize accordingly
					setPreferredSize(new IntDimension(600, 520));
					getFrame().pack();
					Util.centerFrame(getFrame());
				}
			}
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			frame.setTitle("Battle Report Viewer");
			return frame;
		}

		public function createUI() : void {
			var layout0:BorderLayout = new BorderLayout();
			setLayout(layout0);

			pnlSnapshots = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 25));

			pnlSnapshotsScroll = new JScrollPane(pnlSnapshots);
			pnlSnapshotsScroll.setConstraints("Center");

			pnlFooter = new JPanel();
			pnlFooter.setLayout(new BorderLayout(15));
			pnlFooter.setConstraints("South");

			pnlResources = new JPanel();
			pnlResources.setLayout(new FlowLayout(AsWingConstants.LEFT, 8));
			pnlResources.setConstraints("East");

			chkViewAll = new JCheckBox("Loading...");
			chkViewAll.setConstraints("West");
			chkViewAll.setEnabled(false);

			pnlFooter.append(chkViewAll);

			//component layoution
			append(pnlSnapshotsScroll);
			append(pnlFooter);

		}
	}

}

