/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 3/30/14
 * Time: 3:39 PM
 * To change this template use File | Settings | File Templates.
 */
package src.UI.Sidebars.ObjectInfo.CustomInfo {
import src.Map.City;
import src.Objects.Factories.ObjectFactory;
import src.Objects.StructureObject;

public class CustomInfoFactory {

    public function CustomInfoFactory() {
    }

    public function getCustomInfo(city:City, structure:StructureObject) : ICustomInfo {
        if(ObjectFactory.isType("Basement",structure.type) || ObjectFactory.isType("TempBasement",structure.type) ) {
            return new BasementCustomInfo(city,structure);
        }
        return null;
    }
}
}
