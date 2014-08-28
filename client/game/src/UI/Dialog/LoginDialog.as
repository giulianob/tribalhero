package src.UI.Dialog{

    import feathers.controls.Button;
    import feathers.controls.Label;
    import feathers.controls.Panel;
    import feathers.controls.TextInput;
    import feathers.layout.VerticalLayout;
	import src.Constants;

    import src.UI.ViewModels.LoginVM;

    import starling.events.Event;

    public class LoginDialog extends Panel {

        private var vm: LoginVM;
        private var btnLogin: Button;
        private var txtAddress: TextInput;
        private var txtUsername: TextInput;
        private var txtPassword: TextInput;

		public function LoginDialog(vm: LoginVM) {
            this.vm = vm;
            createUI();

			btnLogin.addEventListener(Event.TRIGGERED, function(e: Event): void {
				vm.login(txtUsername.text, txtPassword.text, txtAddress.text);
			});
		}

		private function createUI():void {
			this.headerProperties.title = "Login";

            txtAddress = new TextInput();
			txtAddress.text = Constants.session.hostname;
            
			txtUsername = new TextInput();
			txtUsername.text = Constants.session.username;
            
			txtPassword = new TextInput();
            txtPassword.displayAsPassword = true;

            btnLogin = new Button();
            btnLogin.label = "Login";

            var lblAddress: Label = new Label();
            lblAddress.text = "Address";

            var lblUsername: Label = new Label();
            lblUsername.text = "Username";

            var lblPassword: Label = new Label();
            lblPassword.text = "Password";

            this.layout = new VerticalLayout();

            addChild(lblAddress);
            addChild(txtAddress);

            addChild(lblUsername);
            addChild(txtUsername);

            addChild(lblPassword);
            addChild(txtPassword);

            addChild(btnLogin);
		}
	}
}

