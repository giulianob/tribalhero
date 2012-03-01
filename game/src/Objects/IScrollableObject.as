package src.Objects {
	import src.Map.Camera;
	
	public interface IScrollableObject {
		function moveWithCamera(camera: Camera): void;
		
		function getX(): int; 
		function getY(): int;		
		function setX(x: int): void;		
		function setY(y: int): void;		
		function setXY(x: int, y: int):void;
	}
	
}
