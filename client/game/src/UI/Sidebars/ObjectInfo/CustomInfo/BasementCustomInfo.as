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
    import src.UI.Components.SimpleTooltip;
    import src.Util.StringHelper;

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

        key = new JLabel(StringHelper.localize("TMP_BASEMENT_SIDEBAR_HIDDEN_CROP"));
        key.setHorizontalAlignment(AsWingConstants.LEFT);
        value = new JLabel(resources.crop.toString());
        value.setHorizontalAlignment(AsWingConstants.LEFT);
        value.setHorizontalTextPosition(AsWingConstants.RIGHT);
        value.setIcon(new AssetIcon(new ICON_CROP()));
        new SimpleTooltip(value, StringHelper.localize("TMP_BASEMENT_SIDEBAR_TOOLTIP"));
        form.addRow(key,value);

        key = new JLabel(StringHelper.localize("TMP_BASEMENT_SIDEBAR_HIDDEN_WOOD"));
        key.setHorizontalAlignment(AsWingConstants.LEFT);
        value = new JLabel(resources.wood.toString());
        value.setHorizontalAlignment(AsWingConstants.LEFT);
        value.setHorizontalTextPosition(AsWingConstants.RIGHT);
        value.setIcon(new AssetIcon(new ICON_WOOD()));
        new SimpleTooltip(value, StringHelper.localize("TMP_BASEMENT_SIDEBAR_TOOLTIP"));
        form.addRow(key,value);


        key = new JLabel(StringHelper.localize("TMP_BASEMENT_SIDEBAR_HIDDEN_GOLD"));
        key.setHorizontalAlignment(AsWingConstants.LEFT);
        value = new JLabel(resources.gold.toString());
        value.setHorizontalAlignment(AsWingConstants.LEFT);
        value.setHorizontalTextPosition(AsWingConstants.RIGHT);
        value.setIcon(new AssetIcon(new ICON_GOLD()));
        new SimpleTooltip(value, StringHelper.localize("TMP_BASEMENT_SIDEBAR_TOOLTIP"));
        form.addRow(key,value);

        key = new JLabel(StringHelper.localize("TMP_BASEMENT_SIDEBAR_HIDDEN_IRON"));
        key.setHorizontalAlignment(AsWingConstants.LEFT);
        value = new JLabel(resources.iron.toString());
        value.setHorizontalAlignment(AsWingConstants.LEFT);
        value.setHorizontalTextPosition(AsWingConstants.RIGHT);
        value.setIcon(new AssetIcon(new ICON_IRON()))
        new SimpleTooltip(value, StringHelper.localize("TMP_BASEMENT_SIDEBAR_TOOLTIP"));
        form.addRow(key,value);

    }
}
}
