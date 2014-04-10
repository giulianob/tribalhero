/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 3/30/14
 * Time: 4:36 PM
 * To change this template use File | Settings | File Templates.
 */
package src.UI.Sidebars.ObjectInfo.CustomInfo {
import org.aswing.AsWingConstants;
import org.aswing.AssetIcon;
import org.aswing.JLabel;
import org.aswing.ext.Form;

import src.Map.City;
import src.Objects.Effects.Formula;
import src.Objects.Resources;
import src.Objects.StructureObject;

public class BasementCustomInfo implements ICustomInfo {
    private var city: City;
    private var structure: StructureObject;

    public function BasementCustomInfo(city: City, structure: StructureObject) {
        this.city=city;
        this.structure=structure;
    }

    public function loadForm(form:Form):void {
        var resources: Resources = Formula.getHiddenResource(city);

        var key: JLabel;
        var value:JLabel;

        key = new JLabel("Hidden Crop");
        key.setHorizontalAlignment(AsWingConstants.LEFT);
        value = new JLabel(resources.crop.toString());
        value.setHorizontalAlignment(AsWingConstants.LEFT);
        value.setHorizontalTextPosition(AsWingConstants.RIGHT);
        value.setIcon(new AssetIcon(new ICON_CROP()));
        form.addRow(key,value);

        key = new JLabel("Hidden Wood");
        key.setHorizontalAlignment(AsWingConstants.LEFT);
        value = new JLabel(resources.wood.toString());
        value.setHorizontalAlignment(AsWingConstants.LEFT);
        value.setHorizontalTextPosition(AsWingConstants.RIGHT);
        value.setIcon(new AssetIcon(new ICON_WOOD()));
        form.addRow(key,value);


        key = new JLabel("Hidden Gold");
        key.setHorizontalAlignment(AsWingConstants.LEFT);
        value = new JLabel(resources.gold.toString());
        value.setHorizontalAlignment(AsWingConstants.LEFT);
        value.setHorizontalTextPosition(AsWingConstants.RIGHT);
        value.setIcon(new AssetIcon(new ICON_GOLD()));
        form.addRow(key,value);

        key = new JLabel("Hidden Iron");
        key.setHorizontalAlignment(AsWingConstants.LEFT);
        value = new JLabel(resources.iron.toString());
        value.setHorizontalAlignment(AsWingConstants.LEFT);
        value.setHorizontalTextPosition(AsWingConstants.RIGHT);
        value.setIcon(new AssetIcon(new ICON_IRON()));
        form.addRow(key,value);

    }
}
}
