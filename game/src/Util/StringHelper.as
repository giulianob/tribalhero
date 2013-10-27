package src.Util {
    import System.Linq.Enumerable;

    import fl.lang.Locale;

    import flash.text.*;
    import flash.xml.*;

    import mx.utils.StringUtil;

    import org.aswing.ASFont;

    import src.UI.LookAndFeel.GameLookAndFeel;

    public class StringHelper {
		
		private static var TEXT_FIELD:TextField = new TextField();
		private static var TEXT_FONT:ASFont = null;
		{
			TEXT_FIELD.autoSize = TextFieldAutoSize.LEFT;
			TEXT_FIELD.type = TextFieldType.DYNAMIC;
		}


        public static function htmlEscape(str:String):String
		{
			str = XML(new XMLNode(XMLNodeType.TEXT_NODE, str)).toXMLString();
            str = str.replace("\"", "&quot;");
            return str;
		}
		
		public static function linkify(str:String, escape: Boolean = true):String
		{
			// http://stackoverflow.com/questions/247479/jquery-text-to-link-script
			
			if (escape) {
				str = htmlEscape(str);
			}
			
			var url1: RegExp = new RegExp("(^|&lt;|\\s)(www\\..+?\\..+?)(\\s|&gt;|$)", "gi");
			var url2: RegExp = new RegExp("(^|&lt;|\\s)(((https?|ftp):\\\/\\\/|mailto:).+?)(\\s|&gt;|$)", "gi");		
			
			str = str.replace(url1, '$1<font color="'+GameLookAndFeel.LINK_COLOR+'"><a href="http://$2" target="_blank">$2</a></font>$3');
			str = str.replace(url2, '$1<font color="'+GameLookAndFeel.LINK_COLOR+'"><a href="$2" target="_blank">$2</a></font>$5');
			
			return str;
		}

		public static function replace(str:String, oldSubStr:String, newSubStr:String):String {
			return str.split(oldSubStr).join(newSubStr);
		}

		public static function trim(str:String, char:String = ' '):String {
			return trimBack(trimFront(str, char), char);
		}

		public static function trimFront(str:String, char:String):String {
			char = stringToCharacter(char);
			if (str.charAt(0) == char) {
				str = trimFront(str.substring(1), char);
			}
			return str;
		}
		
		public static function truncate(str:String, maxLength: int = 150, ending: String = "..."): String {
			if (str.length + ending.length <= maxLength) 
				return str;
				
			return str.substr(0, maxLength) + ending;
		}

		public static function trimBack(str:String, char:String):String {
			char = stringToCharacter(char);
			if (str.charAt(str.length - 1) == char) {
				str = trimBack(str.substring(0, str.length - 1), char);
			}
			return str;
		}

		public static function stringToCharacter(str:String):String {
			if (str.length == 1) {
				return str;
			}
			return str.slice(0, 1);
		}
		
		public static function numberInWords(value: int) : String {
			var numbers: Array = new Array("zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten");
			if (value >= numbers.length || value < 0) return value.toString();
			return numbers[value];
		}
		
		public static function makePlural(value: int, singular: String, plural: String, zero: String = "") : String {
			if (zero != "" && value == 0)
			return zero;

			return value > 1 ? plural : singular;
		}

		public static function firstToUpper(word: String) : String{
			var firstLetter: String = word.substring(1, 0);
			var restOfWord: String = word.substring(1);
			return firstLetter.toUpperCase() + restOfWord;
		}		
		
		public static function wordsToUpper(s:String):String {			
			var ret: * = s.replace(/ [a-z]/g, function (m:String, ... rest):String {
				return m.toUpperCase();
			});
			
			return ret;
		}
		
		public static function localize(msgId: String, ... params): String {
			if (msgId == "") {
				return "";
			}
			
			var localizedStr: String = Locale.loadString(msgId);
			if (localizedStr == null) {
				return "[" + msgId + "]";				
			}

			params.unshift(localizedStr);

            var newParams:* = Enumerable.from(params).aggregate(new Array(), function(a:Array, item:*): * {
                if(item is Array) {
                    return a.concat(item);
                } else {
                    a.push(item);
                    return a;
                }
            }).toArray();

			return StringUtil.substitute.apply(undefined, newParams);
		}
		
		public static function calculateTextWidth(font: ASFont, str: String): Number {
		    TEXT_FIELD.text = str;
			if (TEXT_FONT != font){
				font.apply(TEXT_FIELD);
				TEXT_FONT = font;
			}
			
			return TEXT_FIELD.width;
		}					
	}
	
}
