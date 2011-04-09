﻿package src.UI.Sidebars.NewCityPlaceholder {
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
	import src.UI.Sidebars.NewCityPlaceholder.Buttons.NewCityButton;
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

	public class NewCityPlaceholderSidebar extends GameJSidebar
	{
		private var pnlGroups: JPanel;

		private var newCityPlaceholderObj: NewCityPlaceholder;

		public function NewCityPlaceholderSidebar(newCityPlaceholderObj: NewCityPlaceholder)
		{
			this.newCityPlaceholderObj = newCityPlaceholderObj;

			createUI();

			pnlGroups.append(new NewCityButton(newCityPlaceholderObj));
		}
	
		public function dispose():void
		{

		}

		private function createUI() : void
		{
			//component creation
			setSize(new IntDimension(288, 180));
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));			

			pnlGroups = new JPanel();
			pnlGroups.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
			var border: TitledBorder = new TitledBorder();
			border.setColor(new ASColor(0x0, 1));
			border.setTitle("Actions");
			border.setBeveled(false);
			border.setRound(10);
			pnlGroups.setBorder(border);

			//component layoution
			append(pnlGroups);
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			var pt: Point = MapUtil.getMapCoord(newCityPlaceholderObj.getX(), newCityPlaceholderObj.getY());
			frame.getTitleBar().setText("New City Foundation");

			frame.show();
			return frame;
		}
	}

}

