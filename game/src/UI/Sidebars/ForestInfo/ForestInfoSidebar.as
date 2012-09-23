package src.UI.Sidebars.ForestInfo {
	import flash.events.*;
	import flash.geom.Point;
	import flash.utils.Timer;
	import src.Map.*;
	import flash.text.*;
	import src.Objects.Effects.Formula;
	import src.Objects.Factories.*;
	import src.Objects.Prototypes.*;
	import src.Objects.*;
	import src.UI.Components.SimpleTooltip;
	import src.UI.Dialog.*;
	import src.UI.GameJSidebar;
	import src.UI.Sidebars.ForestInfo.Buttons.ForestCampBuildButton;
	import src.UI.Sidebars.ObjectInfo.Buttons.*;
	import src.Util.*;
	import flash.display.*;
	import src.Objects.Actions.*;
	import src.*;
	import src.Objects.Troop.*;

	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;

	public class ForestInfoSidebar extends GameJSidebar
	{
		//UI
		private var lblName:JLabel;
		private var pnlStats:Form;
		private var pnlGroups: JPanel;

		private var t: Timer = new Timer(1000);

		private var forestObj: Forest;

		public function ForestInfoSidebar(forestObj: Forest)
		{
			this.forestObj = forestObj;

			forestObj.addEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);

			createUI();

			pnlGroups.append(new ForestCampBuildButton(forestObj));

			update();

			t.addEventListener(TimerEvent.TIMER, update);
			t.start();
		}

		public function onObjectUpdate(e: Event):void
		{
			update();
		}

		public function update(e: Event = null):void
		{
			pnlStats.removeAll();

			var lbl: JLabel;

			addStatRow("Level", forestObj.level.toString());

			lbl = addStatRow("Rate", "+" + forestObj.rate.toFixed(2) + "/hr", new AssetIcon(new ICON_WOOD()));
			new SimpleTooltip(lbl, "Each labor will gather " + forestObj.rate.toFixed(2) + " wood per hour");

			addStatRow("Laborers", forestObj.labor + "/" + Formula.maxForestLabor(forestObj.level));
			addStatRow("Wood", forestObj.wood.getValue().toString(), new AssetIcon(new ICON_WOOD()));
			addStatRow("Depletion", "-" + Math.round(forestObj.wood.getUpkeep() / Constants.secondsPerUnit) + "/hr", new AssetIcon(new ICON_WOOD()));
			var timeLeft: int = forestObj.depleteTime > 0 && forestObj.wood.getUpkeep() > 0 ? forestObj.depleteTime - Global.map.getServerTime() : 0;
			addStatRow("Time left", Util.formatTime(timeLeft), new AssetIcon(new ICON_CLOCK()));
		}

		private function addStatRow(title: String, value: String, icon: AssetIcon = null) : JLabel {
			var rowTitle: JLabel = new JLabel(title);
			rowTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			rowTitle.setName("title");

			var rowValue: JLabel = new JLabel(value);
			rowValue.setHorizontalAlignment(AsWingConstants.LEFT);
			rowValue.setName("value");

			if (icon != null) {
				rowValue.setIcon(icon);
				rowValue.setHorizontalTextPosition(AsWingConstants.LEFT);
			}

			pnlStats.addRow(rowTitle, rowValue);

			return rowValue;
		}

		public function dispose():void
		{
			t.stop();
			forestObj.removeEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);
		}

		private function createUI() : void
		{
			//component creation
			setSize(new IntDimension(288, 180));
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

			pnlStats = new Form();

			pnlGroups = new JPanel();
			pnlGroups.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
			var border: TitledBorder = new TitledBorder();
			border.setColor(new ASColor(0x0, 1));
			border.setTitle("Actions");
			border.setBeveled(false);
			border.setRound(10);
			pnlGroups.setBorder(border);

			//component layoution
			append(pnlStats);
			append(pnlGroups);
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			var pt: Point = MapUtil.getMapCoord(forestObj.objX, forestObj.objY);
			frame.getTitleBar().setText("Forest (" + pt.x + "," + pt.y + ")");

			frame.show();
			return frame;
		}
	}

}

