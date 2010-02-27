﻿package src.UI.Dialog
{
	import flash.events.Event;
	import src.Comm.GameURLLoader;
	import src.UI.Components.BattleReport.Snapshot;
	import src.UI.GameJPanel;
	import src.Global;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class BattleReportViewer extends GameJPanel
	{

		private var pnlSnapshots: JPanel;
		private var pnlFooter:JPanel;
		private var chkViewAll:JCheckBox;

		private var data: Object;

		private var loader: GameURLLoader = new GameURLLoader();
		private var id: int;
		private var isLocal: Boolean;

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
			Global.map.mapComm.BattleReport.viewLocal(loader, id);
			else
			Global.map.mapComm.BattleReport.viewRemote(loader, id);
		}

		private function onLoaded(e: Event) : void {
			try
			{
				data = loader.getDataAsObject();
				renderSnapshots();
				chkViewAll.setEnabled(true);
			}
			catch (e: Error) {
				InfoDialog.showMessageDialog("Error", "Unable to query report. Refresh the page if this problem persists");
				return;
			}
		}

		private function canSeeSnapshot(snapshot: Object) : Boolean {
			var found: Boolean = false;
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

			pnlSnapshots.removeAll();
			for each (var snapshot: Object in data.snapshots) {
				// Don't show this snapshot if we arent viewing them all
				// if it doesnt pertain to this player
				if (!chkViewAll.isSelected()) {
					if (!canSeeSnapshot(snapshot)) continue;
				}

				pnlSnapshots.append(new Snapshot(snapshot));
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
			setPreferredSize(new IntDimension(950, 500));
			var layout0:BorderLayout = new BorderLayout();
			setLayout(layout0);

			pnlSnapshots = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 25));

			var pnlSnapshotsScroll: JScrollPane = new JScrollPane(pnlSnapshots);
			pnlSnapshotsScroll.setConstraints("Center");

			pnlFooter = new JPanel();
			pnlFooter.setConstraints("South");

			chkViewAll = new JCheckBox("View complete report");
			chkViewAll.setEnabled(false);

			pnlFooter.append(chkViewAll);

			//component layoution
			append(pnlSnapshotsScroll);
			append(pnlFooter);

		}
	}

}

