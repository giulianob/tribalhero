
package src.UI.Sidebars.NewCityPlaceholder.Buttons {
import flash.events.Event;
import flash.events.MouseEvent;

import src.Constants;
import src.Global;
import src.Map.Position;
import src.Objects.Actions.ActionButton;
import src.Objects.Effects.Formula;
import src.Objects.Factories.*;
import src.Objects.NewCityPlaceholder;
import src.Objects.Prototypes.StructurePrototype;
import src.UI.Components.SimpleTooltip;
import src.UI.Dialog.CreateCityDialog;
import src.UI.Tooltips.NewCityTooltip;

public class MoveCityButton extends ActionButton
{
    private var tooltip: NewCityTooltip;
    private var mainBuildingPrototype: StructurePrototype;

    private var newCityPlaceholder: NewCityPlaceholder;

    public function MoveCityButton(newCityPlaceholder: NewCityPlaceholder)
    {
        super(null, "Rebuild Current City");

        this.newCityPlaceholder = newCityPlaceholder;

        mainBuildingPrototype = StructureFactory.getPrototype(ObjectFactory.getFirstType("MainBuilding"), 1);

        new SimpleTooltip(this,"Tear down current city and move to this spot.","Move city");
        ///		tooltip = new NewCityTooltip(mainBuildingPrototype);

        addEventListener(MouseEvent.CLICK, onMouseClick);
//			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
//			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
//			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
    }

    override public function validateButton():Boolean
    {
        if (Constants.alwaysEnableButtons) return true;

        return true;
    }

    public function onMouseOver(event: MouseEvent):void
    {
        tooltip.show(this);
    }

    public function onMouseOut(event: MouseEvent):void
    {
        tooltip.hide();
    }

    public function onMouseClick(event: Event):void
    {
        if (isEnabled())
        {
            var dlg: CreateCityDialog = new CreateCityDialog(function(sender: CreateCityDialog) : void {
                var mapPos: Position = newCityPlaceholder.primaryPosition.toPosition();
                Global.mapComm.Region.moveCity(Global.gameContainer.selectedCity.id, mapPos.x, mapPos.y, sender.getCityName());
                sender.getFrame().dispose();
            });

            dlg.show();
        }

        event.stopImmediatePropagation();
    }
}

}

