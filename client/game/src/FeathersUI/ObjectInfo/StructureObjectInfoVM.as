package src.FeathersUI.ObjectInfo {
    import src.FeathersUI.ViewModel;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.Objects.SimpleGameObject;
    import src.Objects.StructureObject;

    public class StructureObjectInfoVM extends ViewModel {
        private var _structure: StructureObject;
        private var _structurePrototype: StructurePrototype;

        public function StructureObjectInfoVM(structure: StructureObject) {
            _structure = structure;
            _structurePrototype = StructureFactory.getPrototype(structure.type, structure.level);
        }

        public function get structure(): StructureObject {
            return _structure;
        }

        public function get structurePrototype(): StructurePrototype {
            return _structurePrototype;
        }
    }
}
