
package src.UI.Sidebars.NewCityPlaceholder.Buttons {

import flash.events.Event;
import flash.events.MouseEvent;

import src.Constants;
import src.Global;
import src.Map.Position;
import src.Objects.Actions.ActionButton;
import src.Objects.Factories.*;
import src.Objects.NewCityPlaceholder;
import src.Objects.Prototypes.StructurePrototype;
import src.UI.Components.SimpleTooltip;
import src.UI.Dialog.CreateCityDialog;
import src.UI.Tooltips.NewCityTooltip;
import src.Util.DateUtil;
import src.Util.StringHelper;

public class MoveCityButton extends ActionButton
{
    private var tooltip: NewCityTooltip;
    private var mainBuildingPrototype: StructurePrototype;

    private var newCityPlaceholder: NewCityPlaceholder;

    public function MoveCityButton(newCityPlaceholder: NewCityPlaceholder)
    {
        super(null, "Move Current City");

        this.newCityPlaceholder = newCityPlaceholder;

        mainBuildingPrototype = StructureFactory.getPrototype(ObjectFactory.getFirstType("MainBuilding"), 1);

        var delta: int = Global.map.getServerTime() - Constants.session.lastMoved.time/1000;

        if(delta<0) {
            new SimpleTooltip(this,StringHelper.localize("MOVE_CITY_DESC","Never moved"),"Move city");
        } else {
            new SimpleTooltip(this,StringHelper.localize("MOVE_CITY_DESC","Last moved "+DateUtil.niceTime(delta)+" ago"),"Move city");
        }
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
                Global.mapComm.Region.moveCity(Global.gameContainer.selectedCity.id, Global.gameContainer.selectedCity.name, mapPos.x, mapPos.y, sender.getCityName());
                sender.getFrame().dispose();
            });

            dlg.show();
        }

        event.stopImmediatePropagation();
    }
}

}

