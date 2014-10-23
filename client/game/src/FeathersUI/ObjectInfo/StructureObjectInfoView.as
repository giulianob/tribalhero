package src.FeathersUI.ObjectInfo {

    import feathers.controls.Button;
    import feathers.controls.Label;
    import feathers.controls.LayoutGroup;
    import feathers.controls.List;
    import feathers.controls.renderers.BaseDefaultItemRenderer;
    import feathers.controls.renderers.DefaultListItemRenderer;
    import feathers.controls.renderers.IListItemRenderer;
    import feathers.data.ListCollection;
    import feathers.layout.VerticalLayout;

    import mx.utils.StringUtil;

    import src.Constants;
    import src.FeathersUI.Controls.ActionButtonGroup;
    import src.FeathersUI.Controls.CityLabel;
    import src.FeathersUI.Controls.PlayerLabel;
    import src.FeathersUI.Controls.ResponsiveTooltip;
    import src.FeathersUI.Controls.StatsList;
    import src.FeathersUI.ObjectInfo.CustomInfo.CustomObjectProperty;
    import src.FeathersUI.ObjectInfo.CustomInfo.CustomObjectPropertyFactory;
    import src.FeathersUI.ObjectInfo.CustomInfo.ICustomObjectProperties;
    import src.Global;
    import src.Map.City;
    import src.Objects.Factories.ObjectFactory;
    import src.Objects.Factories.PropertyFactory;
    import src.Objects.Factories.SpriteFactory;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Location;
    import src.Objects.Prototypes.PropertyPrototype;
    import src.Objects.SimpleGameObject;
    import src.UI.Sidebars.ObjectInfo.Buttons.SendAttackButton;
    import src.UI.Sidebars.ObjectInfo.Buttons.SendReinforcementButton;
    import src.UI.Sidebars.ObjectInfo.Buttons.ViewBattleButton;
    import src.Util.StringHelper;
    import src.Util.Util;

    import starling.display.Image;

    public class StructureObjectInfoView extends LayoutGroup implements IObjectInfoView {
        private var vm: StructureObjectInfoVM;
        private var themeLink: Button;
        private var themeList: List;
        private var actionButtonGroup: ActionButtonGroup;
        private var isOurObject: Boolean;
        private var statsList: StatsList;
        private var customObjectPropertyFactory: CustomObjectPropertyFactory;
        private var city: City;

        public function StructureObjectInfoView(vm: StructureObjectInfoVM) {
            this.vm = vm;
            this.customObjectPropertyFactory = new CustomObjectPropertyFactory();
            this.layout = new VerticalLayout();

            city = Global.map.cities.get(vm.structure.cityId);
            isOurObject = city != null;
            if (isOurObject) {
                // Themes
                themeList = new List();
                themeList.dataProvider = new ListCollection();
                themeList.itemRendererFactory = function (): IListItemRenderer {
                    var renderer: DefaultListItemRenderer = new DefaultListItemRenderer();
                    renderer.itemHasAccessory = true;
                    renderer.accessoryGap = Number.POSITIVE_INFINITY;
                    renderer.accessoryPosition = BaseDefaultItemRenderer.ACCESSORY_POSITION_LEFT;
                    return renderer;
                };

                themeLink = new Button();

                for each (var theme: String in Constants.session.themesPurchased) {
                    var newSprite: String = StructureFactory.getSpriteName(theme, vm.structure.type, vm.structure.level);
                    if (!SpriteFactory.doesSpriteExist(newSprite)) {
                        continue;
                    }

                    var thumbnail: Image = SpriteFactory.getStarlingImage(newSprite);
                    Util.resizeStarlingSprite(thumbnail, 85, 85);

                    themeList.dataProvider.addItem({
                        label: StringHelper.localize(theme + "_THEME_NAME"),
                        accessory: thumbnail
                    });
                }

                // Events
                // city.addEventListener(City.RESOURCES_UPDATE, onResourcesUpdate);
                // city.currentActions.addEventListener(BinaryListEvent.CHANGED, onObjectUpdate);
                // city.references.addEventListener(BinaryListEvent.CHANGED, onObjectUpdate);
            }

            statsList = new StatsList();

            actionButtonGroup = new ActionButtonGroup(vm.structure);

            // currentActionGroup = new CurrentActionGroup();

            addChild(statsList);
            addChild(actionButtonGroup);

            // addChild(currentActionGroup);

            updateButtons();
            updateStats();
        }

        private function updateStats(): void {
            statsList.clear();

            statsList.addControl("OBJECT_INFO_SIDEBAR_PLAYER_LABEL", new PlayerLabel(vm.structure.playerId));
            statsList.addControl("OBJECT_INFO_SIDEBAR_CITY_LABEL", new CityLabel(vm.structure.cityId));
            statsList.addLabel("OBJECT_INFO_SIDEBAR_LEVEL_PLAYER", vm.structure.level.toString());

            var propertyPrototypes: Array;
            if (isOurObject) {
                if (!ObjectFactory.isType("Unattackable", vm.structurePrototype.type)) {
                    statsList.addLabel("OBJECT_INFO_SIDEBAR_HP_LABEL", vm.structure.hp.toString() + "/" + vm.structure.hp.toString());
                }

                themeLink.label = StringHelper.localize(vm.structure.theme + "_THEME_NAME");
                statsList.addControl("OBJECT_INFO_SIDEBAR_THEME_LABEL", themeLink);

                if (vm.structurePrototype.maxlabor > 0) {
                    statsList.addLabel("OBJECT_INFO_SIDEBAR_LABORERS_LABEL", vm.structure.labor + "/" + vm.structurePrototype.maxlabor, SpriteFactory.getStarlingImage("ICON_LABOR"));
                } else if (vm.structure.labor > 0) {
                    statsList.addLabel("OBJECT_INFO_SIDEBAR_LABORERS_LABEL", vm.structure.labor.toString(), SpriteFactory.getStarlingImage("ICON_LABOR"));
                }

                propertyPrototypes = PropertyFactory.getAllProperties(vm.structure.type);
            }
            else {
                propertyPrototypes = PropertyFactory.getProperties(vm.structure.type, PropertyPrototype.VISIBILITY_PUBLIC);
            }

            // Add object properties
            var icon: Image;
            var iconName: String;
            var propertyLabel: Label;
            for (var i: int = 0; i < propertyPrototypes.length; i++) {
                var propertyPrototype: PropertyPrototype = propertyPrototypes[i];
                icon = null;
                iconName = propertyPrototype.getIcon();
                if (iconName) {
                    icon = SpriteFactory.getStarlingImage(iconName);
                }
                propertyLabel = new Label();
                propertyLabel.text = propertyPrototype.toString(vm.structure.properties[i]);
                statsList.addControl(propertyPrototype.getName(), propertyLabel, icon);
                if (propertyPrototype.tooltip != "") {
                    new ResponsiveTooltip(propertyPrototype.tooltip, propertyLabel).bind();
                }
            }

            var customInfo:ICustomObjectProperties = customObjectPropertyFactory.getCustomInfo(city, vm.structure);
            if (customInfo != null) {
                for each (var customProperty: CustomObjectProperty in customInfo.getCustomProperties()) {
                    icon = null;
                    if (customProperty.icon) {
                        icon = SpriteFactory.getStarlingImage(customProperty.icon);
                    }
                    propertyLabel = new Label();
                    propertyLabel.text = customProperty.value;
                    statsList.addControl(customProperty.name, propertyLabel, icon);
                    if (customProperty.tooltip != "") {
                        new ResponsiveTooltip(customProperty.tooltip, propertyLabel).bind();
                    }
                }
            }

        }

        private function updateButtons(): void {
            var buttons: Array = [];

            if (vm.structure) {
                if (isOurObject) {
                    buttons = buttons.concat(StructureFactory.getButtons(vm.structure))
                            .concat(StructureFactory.getTechButtons(vm.structure));
                }
                else {
                    if (!ObjectFactory.isType("Unattackable", vm.structure.type)) {
                        buttons.push(new SendAttackButton(vm.structure, new Location(Location.CITY, vm.structure.cityId, vm.structure.objectId)));
                    }
                }
            }

            if (Global.selectedCity.id != vm.structure.cityId) {
                buttons.push(new SendReinforcementButton(vm.structure, new Location(Location.CITY, vm.structure.cityId, vm.structure.objectId)));
            }
            if (vm.structure.state.getStateType() == SimpleGameObject.STATE_BATTLE) {
                buttons.push(new ViewBattleButton(vm.structure));
            }

            actionButtonGroup.update(buttons);
        }

        public function get title(): String {
            return StringUtil.substitute("{0} {1}", vm.structurePrototype.getName(), vm.structure.primaryPosition.toPosition());
        }
    }
}
