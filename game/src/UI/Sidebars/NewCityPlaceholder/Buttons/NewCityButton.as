
package src.UI.Sidebars.NewCityPlaceholder.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import src.Comm.Commands;
	import src.Global;
	import src.Objects.Actions.ForestCampBuildAction;
	import src.Objects.Factories.*;
	import src.Objects.Forest;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Troop.*;
	import src.UI.Cursors.*;
	import src.UI.Dialog.CreateCityDialog;
	import src.UI.Dialog.ForestLaborDialog;
	import src.UI.GameJPanel;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Tooltips.TextTooltip;

	public class NewCityButton extends ActionButton
	{
		private var tooltip: TextTooltip;

		public function NewCityButton(parentObj: GameObject)
		{
			super(parentObj, "Build New City");

			tooltip = new TextTooltip("Build New City");

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
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
					Global.mapComm.Region.createCity(Global.gameContainer.selectedCity.id, parentObj.getX(), parentObj.getY(), sender.getCityName());
					sender.getFrame().dispose();
				});						
				
				dlg.show();
			}

			event.stopImmediatePropagation();
		}
	}

}

