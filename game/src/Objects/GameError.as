package src.Objects
{
	import fl.lang.Locale;
	import org.aswing.*;
	import src.UI.Dialog.InfoDialog;

	public class GameError
	{
		public static function getMessage(errorCode: int): String
		{
			var str: String = Locale.loadString("ERROR_" + errorCode.toString());
			if (str && str != "")
			return str + " [" + errorCode + "]";
			else
			return "An unexpected error occurred [" + errorCode + "]";
		}

		public static function showMessage(errorCode: int, callback: Function = null) : void
		{
			InfoDialog.showMessageDialog("Error", getMessage(errorCode), callback);
		}

	}

}
