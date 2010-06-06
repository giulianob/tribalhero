/**
* ...
* @author Default
* @version 0.1
*/

package src.Objects.Prototypes {
	import src.Map.City;
	import src.Map.Map;

	public interface ILayout {		
		function toString(): String;
		function validate(map: Map, city: City, x: int, y: int): Boolean;
	}
	
}
