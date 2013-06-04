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

				var icon: SmartMovieClip = ObjectFactory.getSprite(obj) as SmartMovieClip;
				icon.useHandCursor = true;
				icon.buttonMode = true;
				icon.tag = obj;
				icon.mouseChildren = false;
				icon.mouseEnabled = true;

				icon.addEventListener(MouseEvent.CLICK, function(e: MouseEvent):void {
					selectedObject = e.target.tag ? e.target.tag : e.target.parent.tag;
					onAccept(self);
				});

				var lblCity: JLabel = new JLabel(" ", null, AsWingConstants.CENTER);				
				var pnlHolder: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5, AsWingConstants.CENTER));
				pnlHolder.append(new AssetPane(icon));
				pnlHolder.append(lblCity);
				
				if (obj is GameObject) 
				{
					Global.map.usernames.cities.setLabelUsername((obj as GameObject).cityId, lblCity);
				}
				
				pnlObject.append(pnlHolder);
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
			var layout0:BorderLayout = new BorderLayout();
			setLayout(layout0);

			lblTitle = new JLabel();
			lblTitle.setConstraints("North");
			lblTitle.setText("Select an object...");
			lblTitle.setHorizontalAlignment(AsWingConstants.LEFT);

			pnlObject = new JPanel();
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.CENTER);
			layout1.setHgap(25);
			pnlObject.setLayout(layout1);

			var viewPort: JViewport = new JViewport(pnlObject, false, true);
			viewPort.setHorizontalAlignment(AsWingConstants.CENTER);
			viewPort.setVerticalAlignment(AsWingConstants.TOP);
			
			var scrollPnl: JScrollPane = new JScrollPane(viewPort, JScrollPane.SCROLLBAR_NEVER, JScrollPane.SCROLLBAR_AS_NEEDED);			
			scrollPnl.setConstraints("South");
			scrollPnl.setPreferredSize(new IntDimension(400, 125));
			
			//component layout
			append(lblTitle);
			append(scrollPnl);
		}

	}

}
