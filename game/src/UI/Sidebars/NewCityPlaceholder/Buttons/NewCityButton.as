
package src.UI.Sidebars.NewCityPlaceholder.Buttons {
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import src.Comm.Commands;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.Map.TileLocator;
	import src.Objects.Actions.ForestCampBuildAction;
	import src.Objects.Factories.*;
	import src.Objects.Forest;
	import src.Objects.GameObject;
	import src.Objects.Actions.ActionButton;
	import src.Objects.NewCityPlaceholder;
	import src.Objects.Prototypes.StructurePrototype;
	import src.Objects.SimpleObject;
	import src.Objects.Troop.*;
	import src.Objects.Effects.Formula;
	import src.UI.Cursors.*;
	import src.UI.Dialog.CreateCityDialog;
	import src.UI.Dialog.ForestLaborDialog;
	import src.UI.GameJPanel;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Sidebars.NewCityPlaceholder.NewCityPlaceholderSidebar;
	import src.UI.Tooltips.NewCityTooltip;
	import src.UI.Tooltips.TextTooltip;
	import src.Util.Util;

	public class NewCityButton extends ActionButton
	{
		private var tooltip: NewCityTooltip;
		private var mainBuildingPrototype: StructurePrototype;
		
		private var newCityPlaceholder: NewCityPlaceholder;

		public function NewCityButton(newCityPlaceholder: NewCityPlaceholder)
		{
			super(null, "Build New City");
			
			this.newCityPlaceholder = newCityPlaceholder;

			mainBuildingPrototype = StructureFactory.getPrototype(ObjectFactory.getFirstType("MainBuilding"), 1);
			
			tooltip = new NewCityTooltip(mainBuildingPrototype);

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}
		
		override public function validateButton():Boolean 
		{
			var data:* = Formula.getResourceNewCity();
			if (Constants.alwaysEnableButtons) return true;
			if (data.influenceRequired > data.influenceCurrent || data.wagonRequired > data.wagonCurrent)
				return false;

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
					var mapPos: Point = TileLocator.getMapCoord(newCityPlaceholder.objX, newCityPlaceholder.objY);
					Global.mapComm.Region.createCity(Global.gameContainer.selectedCity.id, mapPos.x, mapPos.y, sender.getCityName());
					sender.getFrame().dispose();
				});						
				
				dlg.show();
			}

			event.stopImmediatePropagation();
		}
	}

}

