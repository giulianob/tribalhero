package src.Objects {

	import flash.filters.GlowFilter;
	import flash.geom.Point;

	public class GameObject extends SimpleGameObject implements IScrollableObject
	{
		private var onSelect: Function;
		private var selected: Boolean;

		private var ignoreClick: Boolean;
		private var originClick: Point;

		public function GameObject()
		{
			ignoreClick = false;
			originClick = new Point(0, 0);
		}

		public function setOnSelect(callback: Function):void
		{
			onSelect = callback;
		}

		public function setSelected(bool: Boolean = false):void
		{
			if (bool == false)
			{
				filters = [];
			}
			else
			{
				filters = [new GlowFilter(0xFFFFFF)];
			}

			selected = bool;
		}

		public function setHighlighted(bool: Boolean = false):void
		{
			if (selected)
			return;

			if (bool == false)
			{
				filters = [];
			}
			else
			{
				filters = [new GlowFilter(0xFF3300)];
			}
		}
	}
}

