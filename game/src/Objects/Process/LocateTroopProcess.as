package src.Objects.Process 
{
    import src.Global;
    import src.Map.Position;
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

			Global.gameContainer.camera.ScrollToCenter(new Position(troop.x, troop.y).toScreenPosition());
			Global.gameContainer.closeAllFrames();
		}
		
	}

}