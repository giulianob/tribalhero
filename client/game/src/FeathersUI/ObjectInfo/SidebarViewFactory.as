package src.FeathersUI.ObjectInfo {
    import src.Objects.SimpleGameObject;
    import src.Objects.StructureObject;

    public class SidebarViewFactory {
        public function getSidebar(obj: SimpleGameObject): IObjectInfoView {
            var sidebar: IObjectInfoView;

            if (obj is StructureObject) {
                sidebar = new StructureObjectInfoView(new StructureObjectInfoVM(StructureObject(obj)));
            }
//            else if (obj is TroopObject)
//                sidebar = new TroopInfoSidebar(obj as TroopObject);
//            else if (obj is Forest)
//                sidebar = new ForestInfoSidebar(obj as Forest);
//            else if (obj is Stronghold)
//                sidebar = new StrongholdInfoSidebar(obj as Stronghold);
//            else if (obj is NewCityPlaceholder)
//                sidebar = new NewCityPlaceholderSidebar(obj as NewCityPlaceholder);
//            else if (obj is BarbarianTribe)
//                sidebar = new BarbarianTribeSidebar(obj as BarbarianTribe);
            else {
                throw new ArgumentError("Unknown object type: " + obj.type);
            }

            return sidebar;
        }
    }
}
