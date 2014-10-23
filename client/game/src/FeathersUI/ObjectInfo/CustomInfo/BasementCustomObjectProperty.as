package src.FeathersUI.ObjectInfo.CustomInfo {
    import org.aswing.AsWingConstants;
import org.aswing.AssetIcon;
import org.aswing.JLabel;
import org.aswing.ext.Form;

import src.Map.City;
import src.Objects.Effects.Formula;
    import src.Objects.Factories.SpriteFactory;
    import src.Objects.Prototypes.PropertyPrototype;
    import src.Objects.Resources;
import src.Objects.StructureObject;
    import src.UI.Components.SimpleTooltip;
    import src.Util.StringHelper;

    public class BasementCustomObjectProperty implements ICustomObjectProperties {
    private var city: City;
    private var structure: StructureObject;

    public function BasementCustomObjectProperty(city: City, structure: StructureObject) {
        this.city = city;
        this.structure = structure;
    }

    public function getCustomProperties(): Array {
        var resources: Resources = Formula.getHiddenResource(city);

        return [
            new CustomObjectProperty(t("TMP_BASEMENT_SIDEBAR_HIDDEN_CROP"), resources.crop.toString(), "ICON_CROP", t("TMP_BASEMENT_SIDEBAR_TOOLTIP")),
            new CustomObjectProperty(t("TMP_BASEMENT_SIDEBAR_HIDDEN_WOOD"), resources.wood.toString(), "ICON_WOOD", t("TMP_BASEMENT_SIDEBAR_TOOLTIP")),
            new CustomObjectProperty(t("TMP_BASEMENT_SIDEBAR_HIDDEN_GOLD"), resources.gold.toString(), "ICON_GOLD", t("TMP_BASEMENT_SIDEBAR_TOOLTIP")),
            new CustomObjectProperty(t("TMP_BASEMENT_SIDEBAR_HIDDEN_IRON"), resources.iron.toString(), "ICON_GOLD", t("TMP_BASEMENT_SIDEBAR_TOOLTIP"))
        ];
    }
}
}
