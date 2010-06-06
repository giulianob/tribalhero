package src.Objects {
	import src.Map.Camera;
	
	public interface IScrollableObject {
		function moveWithCamera(camera: Camera): void;
		
		function getX(): int; //the X and Y of the object may not reflect the true position of the obj
							  //this property can be used to get the actual value
		function getY(): int;
		
		function setX(x: int): void;
		
		function setY(y: int): void;
	}
	
}
