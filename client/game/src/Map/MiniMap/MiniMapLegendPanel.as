/**
 * Created with IntelliJ IDEA.
 * User: OscarMike
 * Date: 2/23/14
 * Time: 4:49 PM
 * To change this template use File | Settings | File Templates.
 */
package src.Map.MiniMap {
import flash.display.DisplayObject;
import flash.events.Event;

import org.aswing.AsWingConstants;
import org.aswing.AssetIcon;
import org.aswing.AssetPane;
import org.aswing.Component;
import org.aswing.JLabel;
import org.aswing.JPanel;
import org.aswing.JToggleButton;
import org.aswing.geom.IntDimension;

import src.UI.LookAndFeel.GameLookAndFeel;

public class MiniMapLegendPanel extends JPanel {
    public static const TOGGLE_BUTTON_WIDTH: int = 140;
    public static const TOGGLE_BUTTON_HEIGHT: int = 20;
    public static const ICON_WIDTH: int = 20;
    public static const ICON_HEIGHT: int = 20;

    public function addRaw(icon: Component) : void {
        append(icon);
    }

     public function add(icon: DisplayObject, desc: String) : void
     {
         var legendLabel: JLabel = new JLabel(desc, new AssetIcon(icon), AsWingConstants.LEFT);
         legendLabel.mouseEnabled = false;
         legendLabel.mouseEnabled = false;
         GameLookAndFeel.changeClass(legendLabel, "Tooltip.text Label.small");
         appendAll(legendLabel);
     }

     public function addToggleButton(button: JToggleButton, text:String, icon: DisplayObject) : void
     {
         if(icon!=null) {
             var assetPane: AssetPane = new AssetPane(icon, AssetPane.PREFER_SIZE_LAYOUT);
             assetPane.setPreferredSize(new IntDimension(ICON_WIDTH, ICON_HEIGHT));
             assetPane.setHorizontalAlignment(AsWingConstants.CENTER);
             assetPane.setVerticalAlignment(AsWingConstants.CENTER);
             assetPane.pack();
             button.setIcon(new AssetIcon(assetPane));
         }

         if(text!=null) {
             button.setText(text);
         }
         button.setPreferredSize(new IntDimension(TOGGLE_BUTTON_WIDTH,TOGGLE_BUTTON_HEIGHT));
         button.setHorizontalAlignment(AsWingConstants.LEFT);
         button.setVerticalAlignment(AsWingConstants.CENTER);
         button.addStateListener(function (e: Event) :void {
            button.setAlpha(button.isSelected() ? 0.5 : 1);
         });
         GameLookAndFeel.changeClass(button, "MapFilter");
         append(button);
     }
}
}
