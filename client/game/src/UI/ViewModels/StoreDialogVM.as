package src.UI.ViewModels {
    import src.Objects.Theme;
    import src.UI.Dialog.StoreViewThemeDetailsDialog;

    public class StoreDialogVM {
        private var _themes: Array;

        public function StoreDialogVM(themes: Array) {
            this._themes = themes;
        }

        public function viewThemeDetails(theme: Theme): void {
            new StoreViewThemeDetailsDialog(new StoreViewThemeDetailsVM(theme)).show();
        }

        public function get themes(): Array {
            return _themes;
        }
    }
}
