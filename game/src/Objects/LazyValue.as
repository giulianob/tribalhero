package src.Objects 
{
	import src.Constants;
	import src.Global;

	public class LazyValue
	{
		private var value: int;
		private var rate: int;
		private var limit: int;
		private var lastRealizeTime: int;		
		
		public function LazyValue(value: int, rate: int, limit: int, lastRealizeTime: int) 
		{
			this.value = value;
			this.limit = limit;
			this.rate = rate;
			this.lastRealizeTime = lastRealizeTime;
		}
		
		public function getLimit(): int
		{
			return limit;
		}
		
		public function getRate() :int 
		{
			return rate;
		}
		
		public function getRawValue() :int
		{
			return value;
		}
		
		public function getValue(): int 
		{			
            var delta: int = 0;
            if (rate != 0) {
				var elapsed: int = (Global.map.getServerTime() - lastRealizeTime) * 1000;
				delta = int(elapsed / (rate * Constants.secondsPerUnit));
            }
            if (limit > 0 && (value + delta) > limit) {
				return limit;
            }
            return value + delta;
		}
		
		public function getHourlyRate(): int
		{
			return int(3600000 / (rate * Constants.secondsPerUnit));
		}
	}

}