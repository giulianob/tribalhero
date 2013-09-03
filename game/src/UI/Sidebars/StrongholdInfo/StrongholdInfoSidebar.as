package src.UI.Sidebars.StrongholdInfo {
	import src.Util.StringHelper;
	import flash.display.*;
	import flash.events.*;
	import flash.geom.*;
	import flash.text.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.*;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Actions.*;
	import src.Objects.Factories.*;
	import src.Objects.Prototypes.*;
	import src.Objects.Stronghold.*;
	import src.Objects.Troop.*;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.Dialog.*;
	import src.UI.Sidebars.ObjectInfo.Buttons.*;
	import src.UI.Sidebars.StrongholdInfo.Buttons.*;
	import src.Util.*;


	public class StrongholdInfoSidebar extends GameJSidebar
	{
		//UI
		private var lblName:JLabel;
		private var pnlStats:Form;
		private var pnlGroups:JPanel;

		private var stronghold: Stronghold;

		public function StrongholdInfoSidebar(stronghold: Stronghold)
		{
			this.stronghold = stronghold;

			stronghold.addEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);

			createUI();
			update();
		}

		public function onObjectUpdate(e: Event):void
		{
			update();
		}

		public function update():void
		{

			clear();		
			
			addStatRow(StringHelper.localize("STR_NAME"), new StrongholdLabel(stronghold.id, Constants.tribe.isInTribe(stronghold.tribeId)));			
						
			if (stronghold.tribeId == 0) {
				addStatRow(StringHelper.localize("STR_TRIBE"), StringHelper.localize("STR_NOT_OCCUPIED"));
			} else {
				addStatRow(StringHelper.localize("STR_TRIBE"), new TribeLabel(stronghold.tribeId));
			}
			
			addStatRow(StringHelper.localize("STR_LEVEL"), stronghold.level.toString());
            addStatRow(StringHelper.localize("STR_GATEMAX"), stronghold.gateMax.toString());

			if (Constants.tribe.isInTribe(stronghold.tribeId)) {
				pnlGroups.append(new ViewStrongholdButton(stronghold));
				pnlGroups.append(new SendReinforcementButton(stronghold, new Location(Location.STRONGHOLD, stronghold.id)));
			} else {
				pnlGroups.append(new SendAttackButton(stronghold,new Location(Location.STRONGHOLD, stronghold.id)));
			}


			var buttons: Array = new Array();

			//Special Case Buttons
			switch(stronghold.state.getStateType())
			{
				case SimpleGameObject.STATE_BATTLE:
					pnlGroups.append(new ViewBattleButton(stronghold));
				break;
			}		
		}

		private function addStatRow(title: String, textOrComponent: *, icon: Icon = null) : * {
			var rowTitle: JLabel = new JLabel(title);
			rowTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			rowTitle.setName("title");

			var rowValue: Component;
			if (textOrComponent is String) {
				var label: JLabel = new JLabel(textOrComponent as String);
				label.setHorizontalAlignment(AsWingConstants.LEFT);
				label.setHorizontalTextPosition(AsWingConstants.LEFT);
				label.setName("value");
				label.setIcon(icon);
				rowValue = label;
			} 
			else			
				rowValue = textOrComponent as Component;			

			pnlStats.addRow(rowTitle, rowValue);

			return rowValue;
		}

		private function clear():void
		{
			pnlGroups.removeAll();
			pnlStats.removeAll();
		}

		public function dispose():void
		{
			stronghold.removeEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);
		}

		private function createUI() : void
		{
			//component creation
			setSize(new IntDimension(288, 180));
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));

			pnlStats = new Form();

			pnlGroups = new JPanel();
			pnlGroups.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
			pnlGroups.setBorder(new EmptyBorder(null, new Insets(0, 0, 20, 0)));


			//component layoution
			append(pnlStats);
			append(pnlGroups);			
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			var pt: Point = MapUtil.getMapCoord(stronghold.objX, stronghold.objY);
			frame.getTitleBar().setText("Stronghold (" + pt.x + "," + pt.y + ")");

			frame.show();
			return frame;
		}
	}

}

