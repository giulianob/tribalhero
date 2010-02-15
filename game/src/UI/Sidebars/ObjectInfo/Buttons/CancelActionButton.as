/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.MovieClip;
	import flash.events.MouseEvent;
	import src.Global;
	import src.Objects.SimpleGameObject;
	import src.UI.Components.SimpleTooltip;

	public class CancelActionButton extends MovieClip  {

		private var ui: CancelActionButton_base = new CancelActionButton_base();
		private var id: int;
		private var parentObj: SimpleGameObject;

		public function CancelActionButton(parentObj: SimpleGameObject, id: int)
		{
			addChild(ui);			

			new SimpleTooltip(ui, "Cancel");

			ui.addEventListener(MouseEvent.CLICK, onClickEvent);

			this.parentObj = parentObj;
			this.id = id;
		}

		public function onClickEvent(e: MouseEvent):void
		{
			Global.map.mapComm.Object.cancelAction(parentObj.cityId, parentObj.objectId, id);
		}

	}

}

