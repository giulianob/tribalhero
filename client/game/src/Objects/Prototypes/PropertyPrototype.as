/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.Objects.Prototypes {
    import flash.display.DisplayObject;
    import flash.utils.getDefinitionByName;

    import org.aswing.AssetIcon;

    public class PropertyPrototype {

		public static const VISIBILITY_PRIVATE: int = 0;
		public static const VISIBILITY_PUBLIC: int = 1;

		public var index: int;
		public var type: int;
		public var name: String;
		public var datatype: String;
		public var visibility: int;
		public var perHour: Boolean;
		public var icon: String;
		public var tooltip: String;

		public function PropertyPrototype(index: int, type: int, name: String, datatype: String, visibility: int, perHour: Boolean, icon: String, tooltip: String) {
			this.type = type;
			this.name = name;
			this.datatype = datatype;
			this.visibility = visibility;
			this.perHour = perHour;
			this.icon = icon;
			this.tooltip = tooltip;
		}

		public static function sortOnType(a:PropertyPrototype, b:PropertyPrototype):Number {
			var aType:Number = a.type;
			var bType:Number = b.type;

			if (aType > bType)
			return 1;
			else if (aType <bType)
			return -1;
			else
			return 0;
		}

		public static function sortOnTypeAndIndex(a:PropertyPrototype, b:PropertyPrototype):Number {
			var aType:Number = a.type;
			var bType:Number = b.type;

			var aIdx:Number = a.index;
			var bIdx:Number = b.index;

			if (aType > bType)
			return 1;
			else if (aType < bType)
			return -1;
			else if (aIdx > bIdx)
			return 1;
			else if (aIdx < bIdx)
			return -1;
			else
			return 0;
		}

		public static function compareType(a: PropertyPrototype, value: int):int
		{
			return a.type - value;
		}

		public function getIcon(): AssetIcon 
		{
			if (icon == "" || icon == null) return null;
			
			var objRef: Class = getDefinitionByName("ICON_" + icon) as Class;

			return new AssetIcon(new objRef() as DisplayObject);
		}
		
		public function toString(value: *): String
		{					
            if (value == undefined) {
                return "";
            }
            if (datatype=="STRING") {
                return value;
            }
            
			return (perHour && value > 0 ? "+" : "") + (int(value) != value ? value.toFixed(2) : value.toString()) + (perHour ? "/hour" : "");
		}
	}

}

