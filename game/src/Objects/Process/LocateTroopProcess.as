package src.Objects.Process 
{
	import src.Constants;
	import src.Global;
	import src.Objects.Troop.TroopStub;
	public class LocateTroopProcess implements IProcess 
	{
		private var troop:TroopStub;
		
		public function LocateTroopProcess(troop: TroopStub) 
		{
			this.troop = troop;			
		}
				
		public function execute():void 
		{
			if (troop.isMoving()) {
				Global.map.selectWhenViewable(troop.cityId, troop.objectId);
			}

			Global.gameContainer.camera.ScrollTo(troop.x * Constants.tileW - Constants.screenW / 2, troop.y * Constants.tileH / 2 - Constants.screenH / 2);
			Global.gameContainer.closeAllFrames();
		}
		
	}

}