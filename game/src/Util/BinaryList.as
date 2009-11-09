package src.Util {
	import flash.events.Event;
	import flash.events.EventDispatcher;
	
	/**
	* ...
	* @author Default
	*/
	public class BinaryList extends EventDispatcher {
		
		public static var CHANGED: String = "CHANGED";
		
		private var sortFunc: Function;
		private var compareFunc: Function;				
		
		private var list: Array = new Array();
		
		public function BinaryList(sortFunc: Function, compareFunc: Function) {			
			this.sortFunc = sortFunc;
			this.compareFunc = compareFunc;
		}
		
		public function toArray(): Array
		{
			return list;
		}
		
		public function each(): Array
		{
			return list;
		}
		
		public function clear():void
		{
			list = new Array();
			super.dispatchEvent(new Event(CHANGED));
		}
		
		public function add(obj: *, resort: Boolean = true):void
		{
			list.push(obj);
			
			if (resort)
				sort();			
		}
		
		public function remove(val: *): *
		{
			var idx: int = Util.binarySearch(list, compareFunc, val);
			
			if (idx == -1)
				return null;
			
			var obj: * = removeByIndex(idx);
			
			super.dispatchEvent(new Event(CHANGED));
			
			return obj;
		}
		
		public function removeByIndex(index: int): *
		{
			var obj: * = list[index];
			
			list.splice(index, 1);
			
			super.dispatchEvent(new Event(CHANGED));
			
			return obj;
		}
		
		public function sort():void
		{
			list.sort(sortFunc);
			super.dispatchEvent(new Event(CHANGED));
		}
		
		public function getByIndex(index: int): *
		{
			return list[index];
		}
		
		public function size(): int
		{
			return list.length;
		}
		
		public function get(val: *): *
		{
			var idx: int = Util.binarySearch(list, compareFunc, val);
			
			if (idx == -1)
				return null;
			
			return list[idx];
		}			
		
		public function getRange(val: * ): *
		{
			var idxs: Array = Util.binarySearchRange(list, compareFunc, val);
			
			var objs: Array = new Array();
			
			for each(var idx: int in idxs)			
				objs.push(list[idx]);
			
			return objs;
		}
	}
	
}