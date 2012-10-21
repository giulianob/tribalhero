package src.Objects
{
	import src.Util.StringHelper;
	import org.aswing.*;
	import src.Constants;
	import src.UI.Dialog.InfoDialog;

	public class GameError
	{
		public static function getMessage(errorCode: int): String
		{
			var str: String = StringHelper.localize("ERROR_" + errorCode.toString());
			if (str && str != "")
			return str + (Constants.debug > 0 ? " [" + errorCode + "]" : "");
			else
			return "An unexpected error occurred. [" + errorCode + "]";
		}

		public static function showMessage(errorCode: int, callback: Function = null, showDirectlyToStage: Boolean = false) : void
		{
			InfoDialog.showMessageDialog("Error", getMessage(errorCode), callback, null, true, true, 1, showDirectlyToStage);
		}

	}

}
