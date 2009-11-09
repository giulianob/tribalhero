package src.Objects 
{
	import flash.geom.Point;
	import src.Map.Map;
	import src.Map.MapUtil;
	import src.Objects.Factories.ObjectFactory;
	import src.Util.Util;
	import src.Constants;
	
	public class WallManager 
	{
		public var objects: Array = new Array();
		
		private var map: Map;
		private var parent: SimpleGameObject;
		public var radius: int = 0;
		
		public function WallManager(map: Map, parent: SimpleGameObject) 
		{
			this.map = map;
			this.parent = parent;
		}
		
		public function clear():void {
			for each(var obj: IScrollableObject in objects) {				
				map.objContainer.removeObject(obj);
			}
			
			objects = new Array();
		}
		
		public function draw(radius: int):void {
			this.radius = radius;
			clear();
			var pos: Point = MapUtil.getMapCoord(parent.getX(), parent.getY());					
			MapUtil.foreach_object(pos.x, pos.y, radius, addWallCallback, false, radius);
		}
		
		public function addWallCallback(x: int, y: int, custom: *):void
		{	
			var parentPos: Point = MapUtil.getMapCoord(parent.getX(), parent.getY());								
			
			var dist: int = MapUtil.distance(parentPos.x, parentPos.y, x, y);
			
			if (dist != custom)
				return;							

			var wall: SimpleObject;
			
			if (parentPos.y % 2 == 0) 
			{
				if (x < parentPos.x && y < parentPos.y)
					wall = ObjectFactory.getSimpleObject("WALL_SE");				
				else if (x < parentPos.x && y > parentPos.y)
					wall = ObjectFactory.getSimpleObject("WALL_NE");				
				else if ((x > parentPos.x && y > parentPos.y) || (x== parentPos.x && y%2==1 && y > parentPos.y))
					wall = ObjectFactory.getSimpleObject("WALL_NW");				
				else if ((x > parentPos.x && y < parentPos.y) || (x == parentPos.x && y%2==1 && y < parentPos.y))
					wall = ObjectFactory.getSimpleObject("WALL_SW");					
			}
			else
			{
				if ((x < parentPos.x && y < parentPos.y) || (x == parentPos.x && y%2==0 && y < parentPos.y))
					wall = ObjectFactory.getSimpleObject("WALL_SE");				
				else if (x < parentPos.x && y > parentPos.y || (x == parentPos.x && y%2==0 && y > parentPos.y))
					wall = ObjectFactory.getSimpleObject("WALL_NE");				
				else if (x > parentPos.x && y > parentPos.y)
					wall = ObjectFactory.getSimpleObject("WALL_NW");
				else if (x > parentPos.x && y < parentPos.y || (parentPos.y%2 == 0 && x == parentPos.x && y < parentPos.y))
					wall = ObjectFactory.getSimpleObject("WALL_SW");						
			}
			
			if (!wall)
				return;
			
			var pos: Point = MapUtil.getScreenCoord(x, y);
			x = pos.x;
			y = pos.y;							
						
			wall.setX(x);
			wall.setY(y);
			map.objContainer.addObject(wall);
			wall.moveWithCamera(map.gameContainer.camera);
			
			objects.push(wall);
		}
	}
	
}