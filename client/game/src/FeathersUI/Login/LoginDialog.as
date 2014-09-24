package src.FeathersUI.Login{

    import feathers.controls.Button;
    import feathers.controls.Label;
    import feathers.controls.Panel;
    import feathers.controls.TextInput;
    import feathers.layout.VerticalLayout;
	import src.Constants;
    import src.FeathersUI.Controls.Form;

    import src.FeathersUI.Login.LoginVM;

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

            this.layout = new VerticalLayout();

            var form: Form = new Form();
            form.addControl("Address", txtAddress);
            form.addControl("Username", txtUsername);
            form.addControl("Password", txtPassword);

            form.addButton(btnLogin);

            this.addChild(form);
		}
	}
}

