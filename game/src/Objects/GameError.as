package src.Objects
{
	import src.Util.StringHelper;
	import org.aswing.*;
	import src.Constants;
	import src.UI.Dialog.InfoDialog;

	public class GameError
	{
		public static function getMessage(errorCode: int, params: Array = null): String
		{
            if (!params) {
                params = [];
            }
            
			params.unshift("ERROR_" + errorCode.toString());
            return StringHelper.localize.apply(null, params);
		}

		public static function showMessage(errorCode: int, callback: Function = null, showDirectlyToStage: Boolean = false) : void
		{
			InfoDialog.showMessageDialog("Error", getMessage(errorCode), callback, null, true, true, 1, showDirectlyToStage);
		}

	}

}
