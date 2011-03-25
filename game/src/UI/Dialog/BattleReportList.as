package src.UI.Dialog
{
	import src.Global;
	import src.UI.Components.BattleReport.LocalReportList;
	import src.UI.Components.BattleReport.RemoteReportList;
	import src.UI.GameJPanel;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import flash.net.*;

	public class BattleReportList extends GameJPanel
	{
		private var pnlLocal: JPanel;
		private var pnlRemote: JPanel;

		private var localReports: LocalReportList;
		private var remoteReports: RemoteReportList;		

		public function BattleReportList() {
			createUI();
		}
		
		public function getRefreshOnClose() : Boolean {
			return localReports.refreshOnClose || remoteReports.refreshOnClose;
		}

		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			frame.setTitle("Battle Reports");
			return frame;
		}

		public function createUI() : void {
			setPreferredSize(new IntDimension(600, 520));

			var layout0:SoftBoxLayout = new SoftBoxLayout();
			layout0.setAxis(AsWingConstants.VERTICAL);
			layout0.setGap(10);
			setLayout(layout0);

			pnlLocal = new JPanel();
			pnlLocal.setPreferredSize(new IntDimension(600, 230));
			var border1:TitledBorder = new TitledBorder(null, "Invasion Reports", 1, AsWingConstants.LEFT, 0, 10);
			border1.setColor(new ASColor(0x0, 1));			
			border1.setBeveled(true);			
			pnlLocal.setBorder(border1);
			pnlLocal.setLayout(new BorderLayout());

			localReports = new LocalReportList();
			localReports.setConstraints("Center");
			pnlLocal.append(localReports);

			pnlRemote = new JPanel();
			pnlRemote.setPreferredSize(new IntDimension(600, 230));
			var border2:TitledBorder = new TitledBorder(null, "Foreign Reports", 1, AsWingConstants.LEFT, 0, 10);
			border2.setColor(new ASColor(0x0, 1));			
			border2.setBeveled(true);
			pnlRemote.setBorder(border2);
			pnlRemote.setLayout(new BorderLayout());

			remoteReports = new RemoteReportList();
			remoteReports.setConstraints("Center");
			pnlRemote.append(remoteReports);

			append(pnlLocal);
			append(pnlRemote);
		}
	}

}

