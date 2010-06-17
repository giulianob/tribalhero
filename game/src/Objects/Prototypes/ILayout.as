/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
	import src.Map.City;
	import src.Map.CityObject;

	public interface ILayout {
		function toString(): String;
		function validate(builder: CityObject, city: City, x: int, y: int): Boolean;
	}

}

