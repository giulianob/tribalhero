package src.Util.BinaryList {
    import flash.events.Event;
    import flash.events.EventDispatcher;
    import flash.events.IEventDispatcher;
    import flash.utils.Proxy;
    import flash.utils.flash_proxy;

    import src.Util.Util;

    public class BinaryList extends Proxy implements IEventDispatcher {

		private var dispatcher: EventDispatcher;
		private var dirty: Boolean;
		private var sortFunc: Function;
		private var compareFunc: Function;

		private var list: Array = [];

		public function BinaryList(sortFunc: Function, compareFunc: Function) {
			this.dispatcher = new EventDispatcher(this);
			this.sortFunc = sortFunc;
			this.compareFunc = compareFunc;
		}

        public function toArray(): Array
		{
			return list;
		}

		public function clear():void
		{
			list = [];
			dispatcher.dispatchEvent(new BinaryListEvent(BinaryListEvent.CHANGED));
		}

		public function add(obj: *, sorted: Boolean = true):void
		{
			if (!sorted) {
				list.push(obj);
				dirty = true;
			}
			else {
				var idx: int = Util.binarySearch(list, sortFunc, obj);
				if (idx > -1) list.splice(idx, 0, obj);
				else {
					list.splice(~idx, 0, obj);
				}
			}

			dispatcher.dispatchEvent(new BinaryListEvent(BinaryListEvent.CHANGED, obj));
			dispatcher.dispatchEvent(new BinaryListEvent(BinaryListEvent.ADDED, obj));
		}

		public function update(obj: *, val: Array) : void
		{
			//find the index and change it
			var idx: int = Util.binarySearch(list, compareFunc, val);

			if (idx <= -1) return;

			list[idx] = obj;

			dispatcher.dispatchEvent(new BinaryListEvent(BinaryListEvent.CHANGED));
			dispatcher.dispatchEvent(new BinaryListEvent(BinaryListEvent.UPDATED, obj));
		}

		public function remove(val: *): *
		{
			var idx: int = Util.binarySearch(list, compareFunc, val);

			if (idx <= -1) return null;

			return removeByIndex(idx);
		}

		public function removeByIndex(index: int): *
		{
			var obj: * = list[index];

			list.splice(index, 1);

			dispatcher.dispatchEvent(new BinaryListEvent(BinaryListEvent.CHANGED));

			dispatcher.dispatchEvent(new BinaryListEvent(BinaryListEvent.REMOVED, obj));

			return obj;
		}

		public function sort():void
		{
			if (!dirty) 
				return;
				
			list.sort(sortFunc);
			dirty = false;
			dispatcher.dispatchEvent(new BinaryListEvent(BinaryListEvent.CHANGED));
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

			if (idx <= -1) return null;

			return list[idx];
		}

        public function getRange(val: * ): Array
		{
			var idxs: Array = Util.binarySearchRange(list, compareFunc, val);

			var objs: Array = [];

			for each(var idx: int in idxs)
				objs.push(list[idx]);

			return objs;
		}
		
		public function removeRange(val: * ): void
		{
			var idxs: Array = Util.binarySearchRange(list, compareFunc, val);
			
			if (!idxs || idxs.length == 0) {
				return;
			}
			
			// Sort the array so the smallest number is at the index 0 then splice the array so it removes
			// the items in place
			idxs.sort(Array.NUMERIC);
			
			list.splice(idxs[0], idxs.length);
		}
		
		override flash_proxy function deleteProperty(name:*):Boolean {
			return delete list[name];
		}

		override flash_proxy function getProperty(name:*):* {
			return list[name];
		}

		override flash_proxy function hasProperty(name:*):Boolean {
			return name in list;
		}

		override flash_proxy function nextNameIndex(index:int):int
		{
			if (index < list.length) return index + 1;
			return 0;
		}

		override flash_proxy function nextName(index:int):String {
			return String(index - 1);
		}

		override flash_proxy function nextValue(index:int):* {
			return list[index - 1];
		}		
        
		override flash_proxy function callProperty(methodName:*, ... args):*
		{
			var res:*;

			switch (methodName.toString())
			{
				default:
				res = list[methodName].apply(list, args);
				break;
			}
			return res;
		}        
		
		public function addEventListener(type:String, listener:Function, useCapture:Boolean = false, priority:int = 0, useWeakReference:Boolean = false):void{
			dispatcher.addEventListener(type, listener, useCapture, priority);
		}

		public function dispatchEvent(evt: Event):Boolean{
			return dispatcher.dispatchEvent(evt);
		}

		public function hasEventListener(type:String):Boolean{
			return dispatcher.hasEventListener(type);
		}

		public function removeEventListener(type:String, listener:Function, useCapture:Boolean = false):void{
			dispatcher.removeEventListener(type, listener, useCapture);
		}

		public function willTrigger(type:String):Boolean {
			return dispatcher.willTrigger(type);
		}		
	}

}

