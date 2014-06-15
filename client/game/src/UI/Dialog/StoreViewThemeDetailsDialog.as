package src.UI.Dialog {
    import com.codecatalyst.promise.Deferred;

    import flash.events.Event;

    import org.aswing.*;
    import org.aswing.ext.GeneralGridListCellFactory;
    import org.aswing.ext.GridList;
    import org.aswing.ext.GridListItemEvent;
    import org.aswing.ext.MultilineLabel;

    import src.Assets;
    import src.Global;
    import src.Map.City;
    import src.Objects.Store.IStoreAsset;
    import src.UI.Components.Store.StoreItemAssetGridCell;
    import src.UI.GameJPanel;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.Tooltips.StoreItemTooltip;
    import src.UI.ViewModels.StoreViewThemeDetailsVM;
    import src.Util.Util;

    public class StoreViewThemeDetailsDialog extends GameJPanel {
        private var lblDescription: MultilineLabel;

        private var btnBuy: JButton;

        private var itemTooltip: StoreItemTooltip;

        private var viewModel: StoreViewThemeDetailsVM;

        public function StoreViewThemeDetailsDialog(viewModel: StoreViewThemeDetailsVM) {
            this.viewModel = viewModel;
            this.title = viewModel.theme.localizedName;

            createUI();
        }

        private function createUI(): void {
            setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
            setPreferredWidth(540);

            var lblTitle: JLabel = new JLabel(viewModel.theme.localizedName, null, AsWingConstants.LEFT);
            GameLookAndFeel.changeClass(lblTitle, "darkHeader");

            lblDescription = new MultilineLabel(viewModel.theme.localizedDescription, 3, 100);

            var pnlPreviewImage: JPanel = new JPanel(new CenterLayout());
            pnlPreviewImage.appendAll(new AssetPane(Assets.getInstance(viewModel.theme.id + "_THEME_BANNER")));

            var gridStoreItems: GridList = new GridList(new VectorListModel(viewModel.getThemeAssets()), new GeneralGridListCellFactory(StoreItemAssetGridCell), 5, 0);
            gridStoreItems.setTracksHeight(true);
            gridStoreItems.setTileWidth(85);
            gridStoreItems.setTileHeight(80);

            var previewTabs: JTabbedPane = new JTabbedPane();
            previewTabs.setPreferredHeight(220);
            previewTabs.appendTab(pnlPreviewImage, t("STORE_VIEW_THEME_DIALOG_PREVIEW_TAB"));
            previewTabs.appendTab(Util.createTopAlignedScrollPane(gridStoreItems), t("STORE_VIEW_THEME_DIALOG_DETAIL_TAB"));

            appendAll(lblTitle, lblDescription);

            if (viewModel.isWallIncluded() && viewModel.isStrongholdIncluded()) {
                var lblWallAndStrongholdIncluded: JLabel = new JLabel(t("STORE_VIEW_THEME_DIALOG_STRONGHOLD_AND_WALL_INCLUDED"), null, AsWingConstants.LEFT);
                append(lblWallAndStrongholdIncluded);
            }
            else if (viewModel.isWallIncluded()) {
                var lblWallIncluded: JLabel = new JLabel(t("STORE_VIEW_THEME_DIALOG_WALL_INCLUDED"), null, AsWingConstants.LEFT);
                append(lblWallIncluded);
            }
            else if (viewModel.isStrongholdIncluded()) {
                var lblStrongholdIncluded: JLabel = new JLabel(t("STORE_VIEW_THEME_DIALOG_STRONGHOLD_INCLUDED"), null, AsWingConstants.LEFT);
                append(lblStrongholdIncluded);
            }

            appendAll(previewTabs);

            if (!viewModel.theme.hasPurchased()) {
                btnBuy = new JButton(t("STORE_VIEW_THEME_DIALOG_BUY", viewModel.theme.cost), new AssetIcon(Assets.getInstance("ICON_COIN")));
                btnBuy.setHorizontalTextPosition(AsWingConstants.LEFT);
                btnBuy.setIconTextGap(0);

                var pnlBuyRow: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 0, 0, false));
                pnlBuyRow.appendAll(btnBuy);

                append(pnlBuyRow);

                btnBuy.addActionListener(function(): void {
                    viewModel.buy();
                });
            }
            else {
                var applyAllDropDown: JPopupMenu = new JPopupMenu();
                applyAllDropDown.append(createChooseCityHeaderLabel());

                for each (var city: City in Global.map.cities) {
                    applyAllDropDown.append(createApplyAllMenuItem(city));
                }

                var btnApplyAll: JButton = new JButton(t("STORE_VIEW_THEME_APPLY_ALL"));
                btnApplyAll.addActionListener(function(e: Event): void {
                    if (Global.map.cities.size() == 1) {
                        viewModel.applyAllTheme(Global.map.cities.getByIndex(0));
                    }
                    else {
                        if (applyAllDropDown.isVisible()) {
                            applyAllDropDown.setVisible(false);
                        }
                        else {
                            applyAllDropDown.show(btnApplyAll, 0, btnApplyAll.getHeight());
                        }
                    }
                });

                var applyWallDropDown: JPopupMenu = new JPopupMenu();
                applyWallDropDown.append(createChooseCityHeaderLabel());

                for each (city in Global.map.cities) {
                    applyWallDropDown.append(createSetWallThemeMenuItem(city));
                }

                var btnSetWallTheme: JButton = new JButton(t("STORE_VIEW_THEME_APPLY_WALL"));
                btnSetWallTheme.addActionListener(function(e: Event): void {
                    if (Global.map.cities.size() == 1) {
                        viewModel.applyWallTheme(Global.map.cities.getByIndex(0));
                    }
                    else {
                        if (applyWallDropDown.isVisible()) {
                            applyWallDropDown.setVisible(false);
                        }
                        else {
                            applyWallDropDown.show(btnSetWallTheme, 0, btnSetWallTheme.getHeight());
                        }
                    }
                });

                var setDefaultThemeDropDown: JPopupMenu = new JPopupMenu();
                setDefaultThemeDropDown.append(createChooseCityHeaderLabel());
                for each (city in Global.map.cities) {
                    setDefaultThemeDropDown.append(createSetDefaultThemeMenuItem(city));
                }

                var btnSetDefault: JButton = new JButton(t("STORE_VIEW_THEME_SET_DEFAULT"));
                btnSetDefault.addActionListener(function(e: Event): void {
                    if (Global.map.cities.size() == 1) {
                        viewModel.setDefaultTheme(Global.map.cities.getByIndex(0));
                    }
                    else {
                        if (setDefaultThemeDropDown.isVisible()) {
                            setDefaultThemeDropDown.setVisible(false);
                        }
                        else {
                            setDefaultThemeDropDown.show(btnSetDefault, 0, btnSetDefault.getHeight());
                        }
                    }
                });

                var pnlSetThemeRow: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5, AsWingConstants.CENTER));
                pnlSetThemeRow.setPreferredWidth(300);
                pnlSetThemeRow.appendAll(btnApplyAll, btnSetWallTheme, btnSetDefault);

                var helpLabel: MultilineLabel = new MultilineLabel(t("STORE_VIEW_THEME_SET_INSTRUCTIONS"), 3, 100);
                GameLookAndFeel.changeClass(helpLabel, "darkText");

                append(pnlSetThemeRow);
                append(helpLabel);

                helpLabel.pack();
            }

            lblDescription.pack();

            gridStoreItems.addEventListener(GridListItemEvent.ITEM_ROLL_OUT, onItemRollOut);
            gridStoreItems.addEventListener(GridListItemEvent.ITEM_ROLL_OVER, onItemRollOver);
        }

        private function createSetDefaultThemeMenuItem(city: City): Component {
            var menuItem: JMenuItem = new JMenuItem(city.name);
            menuItem.addActionListener(function(): void {
                viewModel.setDefaultTheme(city);
            });

            return menuItem;
        }

        private function createChooseCityHeaderLabel(): Component {
            var label: JLabel = new JLabel(t("STORE_VIEW_THEME_CHOOSE_CITY_HEADER_LABEL"));
            GameLookAndFeel.changeClass(label, "darkText");
            return label;
        }

        private function createApplyAllMenuItem(city: City): Component {
            var menuItem: JMenuItem = new JMenuItem(city.name);
            menuItem.addActionListener(function(): void {
                viewModel.applyAllTheme(city);
            });

            return menuItem;
        }

        private function createSetWallThemeMenuItem(city: City): Component {
            var menuItem: JMenuItem = new JMenuItem(city.name);
            menuItem.addActionListener(function(): void {
                viewModel.applyWallTheme(city);
            });

            return menuItem;
        }

        public function onItemRollOver(event: GridListItemEvent):void
        {
            var storeItem: IStoreAsset = IStoreAsset(event.getValue());

            onItemRollOut(event);
            this.itemTooltip = new StoreItemTooltip(storeItem);
            this.itemTooltip.show(this);
        }

        public function onItemRollOut(event: GridListItemEvent):void
        {
            if (this.itemTooltip) {
                this.itemTooltip.hide();
            }

            this.itemTooltip = null;
        }

        public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
        {
            super.showSelf(owner, modal, onClose, null);

            frame.setResizable(false);
            frame.pack();

            Global.gameContainer.showFrame(frame);

            return frame;
        }
    }
}
