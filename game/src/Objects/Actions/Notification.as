package src.Objects.Actions 
{
	public class Notification
	{
        public var targetCityId:int;		
		public var cityId: int;
		public var objectId: int;		
		public var startTime: int;
		public var endTime: int;
		public var actionId: int;
		public var type: int;
		
		public function Notification(targetCityId: int, cityId: int, objectId: int, actionId: int, type: int, startTime: int, endTime: int) 
		{			
			this.targetCityId = targetCityId;
            this.cityId = cityId;
			this.objectId = objectId;
			this.actionId = actionId;
			this.type = type;
			this.startTime = startTime;
			this.endTime = endTime;
		}
				
		public static function compareCityIdAndActionId(a: Notification, value: Array):int
		{
			var cityDelta: int = a.cityId - value[0];
			var idDelta: int = a.actionId - value[1];
			
			if (cityDelta != 0)
				return cityDelta;
				
			if (idDelta != 0)
				return idDelta;
			else
				return 0;
		}			
		
		public static function sortOnCityIdAndObjId(a:Notification, b:Notification):Number {
			var aCityId:Number = a.cityId;
			var bCityId:Number = b.cityId;

			var aId:Number = a.actionId;
			var bId:Number = b.actionId;
			
			if (aCityId > bCityId)
				return 1;
			else if (aCityId < bCityId)
				return -1;
			else if (aId > bId)
				return 1;
			else if (aId < bId)
				return -1;
			else
				return 0;
		}		
	}
	
}