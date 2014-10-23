package src.FeathersUI.Controls {
    import src.Objects.Actions.*;
    import feathers.controls.Button;

    import org.aswing.Icon;

    import src.Objects.SimpleGameObject;

    public class ActionButton extends Button {
		public var parentObj: SimpleGameObject;

		public var parentAction: Action = new Action();

        function ActionButton(parentObj: SimpleGameObject, buttonText: String, icon: Icon = null)
		{
            super();

            this.label = buttonText;
			this.parentObj = parentObj;
		}

		public function enable():void {
            isEnabled = true;
		}

		public function disable():void {
            isEnabled = false;
		}

		public function alwaysEnabled(): Boolean 
		{
			return false;
		}
		
		public function validateButton(): Boolean
		{
			return true;
		}
	}
}

