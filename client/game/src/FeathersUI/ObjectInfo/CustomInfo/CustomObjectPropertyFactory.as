package src.FeathersUI.ObjectInfo.CustomInfo {
import src.Map.City;
import src.Objects.Factories.ObjectFactory;
import src.Objects.StructureObject;

public class CustomObjectPropertyFactory {

    public function CustomObjectPropertyFactory() {
    }

    public function getCustomInfo(city:City, structure:StructureObject) : ICustomObjectProperties {
        if(ObjectFactory.isType("Basement",structure.type) || ObjectFactory.isType("TempBasement",structure.type) ) {
            return new BasementCustomObjectProperty(city,structure);
        }
        return null;
    }
}
}
