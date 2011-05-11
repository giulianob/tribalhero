package src.UI.Dialog 
{
	import flash.events.Event;
	import org.aswing.JTable;
	import org.aswing.VectorListModel;
	import src.Comm.GameURLLoader;
	import src.UI.GameJPanel;	

	public class MessageBoard extends GameJPanel
	{
		var modelThreads: VectorListModel;		
		var tableThreads: JTable;
		
		var threadLoader: GameURLLoader;
		var actionsLoader: GameURLLoader;
		
		public function MessageBoard() 
		{
			threadLoader.addEventListener(Event.COMPLETE, onReceiveThreads);
			actionsLoader.addEventListener(Event.COMPLETE, onReceiveActionComplete);
			
			createUI();
		}
		
		private function onReceiveActionComplete(e: Event): void {
			var data: Object;
			try
			{
				data = actionLoader.getDataAsObject();
			}
			catch (e: Error) {
				InfoDialog.showMessageDialog("Error", "Unable to perform this action. Try again later.");
				return;
			}

			if (data.error != null) {
				InfoDialog.showMessageDialog("Info", data.error);
				return;
			}	
			
			refresh();
		}
		
		private function onReceiveThreads(e: Event): void {
			var data: Object;
			try
			{
				data = actionLoader.getDataAsObject();
			}
			catch (e: Error) {
				InfoDialog.showMessageDialog("Error", "Unable to perform this action. Try again later.");
				return;
			}

			if (data.error != null) {
				InfoDialog.showMessageDialog("Info", data.error);
				return;
			}			
			
			
		}
		
		private function refresh() {
			
		}
		
		private function createUI() {
			
		}
	}

}