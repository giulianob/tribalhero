package src.UI.Dialog 
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
		private var pnlPaging:JPanel;
		private var btnPrevious:JButton;
		private var lblPages:JLabel;
		private var btnNext:JButton;
		
		private var loader: GameURLLoader = new GameURLLoader();
		private var id: int;
		private var page: int;
		
		public function BattleReportViewer(id: int) 
		{
			this.id = id;
			
			createUI();
			
			btnNext.addActionListener(function() : void {
				loadPage(page + 1);
			});
			
			btnPrevious.addActionListener(function() : void{
				loadPage(page - 1);
			});			
			
			loader.addEventListener(Event.COMPLETE, onLoaded);
			loadPage(0);
		}
		
		private function loadPage(page: int) : void {	
			btnPrevious.setVisible(false);
			btnNext.setVisible(false);
			lblPages.setText("Loading...");
			
			Global.map.mapComm.BattleReport.view(loader, id, page);
		}
		
		private function onLoaded(e: Event) : void {
			var data: Object;
			try
			{
				data = loader.getDataAsObject();
			}
			catch (e: Error) {				
				InfoDialog.showMessageDialog("Error", "Unable to query report. Refresh the page if this problem persists");
				return;
			}
			
			//Paging info
			this.page = data.page;
			btnPrevious.setVisible(page > 1);
			btnNext.setVisible(page < data.pages);
			lblPages.setText(data.page + " of " + data.pages);
			
			pnlSnapshots.removeAll();
			for each (var snapshot: Object in data.snapshots) {
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
			
			pnlPaging = new JPanel();
			pnlPaging.setConstraints("South");			
			
			btnPrevious = new JButton();
			btnPrevious.setText("« Previous");
			
			lblPages = new JLabel();
			
			btnNext = new JButton();
			btnNext.setText("Next »");		
			
			//component layoution
			append(pnlSnapshotsScroll);
			append(pnlPaging);
			
			pnlPaging.append(btnPrevious);
			pnlPaging.append(lblPages);
			pnlPaging.append(btnNext);			
			
		}
	}

}