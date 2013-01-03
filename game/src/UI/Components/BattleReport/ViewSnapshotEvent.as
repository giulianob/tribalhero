package src.UI.Components.BattleReport 
{
	import flash.events.Event;
	
	public class ViewSnapshotEvent extends Event 
	{
		private var _reportId:int;
		
		public function ViewSnapshotEvent(type: String, reportId: int) 
		{			
			super(type);
			
			this._reportId = reportId;
		}
		
		public function get reportId():int 
		{
			return _reportId;
		}
		
	}

}