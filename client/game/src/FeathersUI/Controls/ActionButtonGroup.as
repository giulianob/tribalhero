package src.FeathersUI.Controls {
    import System.Collection.Generic.IEnumerable;
    import System.Collection.Generic.IGrouping;
    import System.Linq.Enumerable;

    import feathers.controls.GroupedList;
    import feathers.controls.LayoutGroup;
    import feathers.controls.renderers.DefaultGroupedListItemRenderer;
    import feathers.controls.renderers.IGroupedListItemRenderer;
    import feathers.data.HierarchicalCollection;
    import feathers.layout.VerticalLayout;

    import src.Constants;
    import src.Global;
    import src.Map.City;
    import src.Objects.Actions.Action;
    import src.Objects.GameObject;

    public class ActionButtonGroup extends LayoutGroup {
        private var gameObject: GameObject;
        private var city: City;
        private var buttons: Array;
        private var list: GroupedList;

        public function ActionButtonGroup(gameObject: GameObject) {
            this.gameObject = gameObject;
            city = Global.map.cities.get(gameObject.cityId);
            layout = new VerticalLayout();

            list = new GroupedList();
            list.isSelectable = false;
            list.itemRendererFactory = function():IGroupedListItemRenderer
            {
                var renderer:DefaultGroupedListItemRenderer = new DefaultGroupedListItemRenderer();
                renderer.itemHasAccessory = true;
                renderer.itemHasLabel = false;
                return renderer;
            };

            list.dataProvider = new HierarchicalCollection();
            addChild(list);
        }

        public function update(buttons: Array): void {
            this.buttons = buttons;

            // Group the actions together
            var groups: IEnumerable = Enumerable.from(buttons).groupBy(function(button: ActionButton): Object {
                for each (var actionGroup: Object in Action.groups)
                {
                    for each (var actionType: Class in actionGroup.actions) {
                        if (button is actionType) {
                            return actionGroup;
                        }
                    }
                }
                throw new Error("Did not find a group for action type " + button.parentAction.actionType);
            }).orderBy(function(grouping: IGrouping): int {
                return grouping.key.order;
            });

            // Go through each group and convert it to feathers format
            var groupData: Array = [];
            for each (var group: IGrouping in groups) {
                groupData.push({
                    header: group.key.name,
                    children: group.select(function(button: ActionButton): Object {
                        button.width = 150;
                        button.defaultLabelProperties.wordWrap = true;

                        return { accessory: button };
                    }).toArray()
                });
            }

            list.dataProvider.data = groupData;

            validateButtons();
        }

        public function validateButtons():void
        {
            var city: City = Global.map.cities.get(gameObject.cityId);

            for each(var button: ActionButton in buttons)
            {
                button.enable();

                if (button.alwaysEnabled() || Constants.alwaysEnableButtons) {
                    continue;
                }

                if (!button.validateButton() || !city.validateAction(button.parentAction, gameObject))
                    button.disable();
            }
        }
    }
}
