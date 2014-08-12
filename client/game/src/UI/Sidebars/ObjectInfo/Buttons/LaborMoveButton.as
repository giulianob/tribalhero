
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.Event;
    import flash.events.MouseEvent;

    import src.Global;
    import src.Objects.*;
    import src.Objects.Actions.ActionButton;
    import src.Objects.Factories.StructureFactory;
    import src.Objects.Prototypes.StructurePrototype;
    import src.UI.Dialog.LaborMoveDialog;
    import src.UI.Tooltips.TextTooltip;
    import src.Util.StringHelper;

    public class LaborMoveButton extends ActionButton
	{
		private var textToolTip: TextTooltip;

		public function LaborMoveButton(parentObj: SimpleGameObject)
		{
			super(parentObj, "Assign Laborers");

			var strPrototype: StructurePrototype = StructureFactory.getPrototype(parentObj.type, (parentObj as StructureObject).level);

			var str: String = StringHelper.localize(strPrototype.name + "_STRUCTURE_LABOR_MOVE");
			if (str == "") {
				str = "Assign Laborers";
			}

			textToolTip = new TextTooltip(str);

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
		}

		public function onMouseOver(event: MouseEvent):void
		{
			textToolTip.show(this);
		}

		public function onMouseOut(event: MouseEvent):void
		{
			textToolTip.hide();
		}

		public function onMouseClick(MouseEvent: Event):void
		{
			if (isEnabled())
			{
				var inputDialog: LaborMoveDialog = new LaborMoveDialog(parentObj as StructureObject, onAcceptDialog);
				inputDialog.show();
			}
		}

		public function onAcceptDialog(sender: LaborMoveDialog):void
		{
			Global.mapComm.Objects.laborMove(parentObj.groupId, this.parentObj.objectId, sender.getCount());
			sender.getFrame().dispose();
		}
	}

}

