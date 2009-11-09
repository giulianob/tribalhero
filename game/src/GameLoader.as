/**
* ...
* @author Default
* @version 0.1
*/

package src {

	public class GameLoader {
		
		var doneLoading: Function;
		
		public function GameLoader() {
			
		}
		
		public function setDoneLoading(event: Function)
		{
			doneLoading = event;
		}
	}
	
}
