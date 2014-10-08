package src.FeathersUI.Login {
    import src.FeathersUI.ViewModel;

    public class LoginVM extends ViewModel {
        public static const LOGIN: String = "LOGIN";

        public function login(username: String, password: String, address: String): void {
            dispatchWith(LOGIN, username, password, address);
        }
    }
}
