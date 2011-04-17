package src.UI.Dialog {
	import flash.events.MouseEvent;
	import src.Global;
	import src.Objects.Factories.ObjectFactory;
	import src.Objects.GameObject;
	import src.Objects.SimpleGameObject;
	import src.Objects.SimpleObject;
	import src.UI.GameJPanel;
	import src.UI.SmartMovieClip;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class ObjectSelectDialog extends GameJPanel {

		public var selectedObject: SimpleObject;

		private var lblTitle:JLabel;
		private var pnlObject:JPanel;

		public function ObjectSelectDialog(objects: Array, onAccept: Function):void
		{
			createUI();

			title = "Select an object";

			var self: ObjectSelectDialog = this;

			for each(var obj: SimpleObject in objects)
			{
				if (!obj.isSelectable()) continue;

				var icon: SmartMovieClip = ObjectFactory.getSprite(obj, true, true) as SmartMovieClip;
				icon.useHandCursor = true;
				icon.buttonMode = true;
				icon.tag = obj;
				icon.mouseChildren = false;
				icon.mouseEnabled = true;

				icon.addEventListener(MouseEvent.CLICK, function(e: MouseEvent):void {
					selectedObject = e.target.tag ? e.target.tag : e.target.parent.tag;
					onAccept(self);
				});

				pnlObject.append(new AssetPane(icon));
			}

		}

		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);

			Global.gameContainer.showFrame(frame);

			return frame;
		}

		private function createUI():void
		{
			//component creation
			setSize(new IntDimension(256, 72));
			var layout0:BorderLayout = new BorderLayout();
			setLayout(layout0);

			lblTitle = new JLabel();
			lblTitle.setSize(new IntDimension(200, 30));
			lblTitle.setPreferredSize(new IntDimension(200, 30));
			lblTitle.setConstraints("North");
			lblTitle.setText("Select an object...");
			lblTitle.setHorizontalAlignment(AsWingConstants.LEFT);

			pnlObject = new JPanel();
			pnlObject.setLocation(new IntPoint(0, 52));
			pnlObject.setSize(new IntDimension(256, 20));
			pnlObject.setConstraints("South");
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.CENTER);
			layout1.setHgap(25);
			layout1.setVgap(20);
			pnlObject.setLayout(layout1);

			//component layout
			append(lblTitle);
			append(pnlObject);
		}

	}

}
