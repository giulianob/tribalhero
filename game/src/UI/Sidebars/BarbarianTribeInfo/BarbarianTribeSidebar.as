package src.UI.Sidebars.BarbarianTribeInfo 
{
    import flash.events.*;
    import flash.geom.*;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Map.*;
    import src.Objects.*;
    import src.UI.*;
    import src.UI.Sidebars.ObjectInfo.Buttons.*;
    import src.Util.StringHelper;

    /**
	 * ...
	 * @author Anthony Lam
	 */
	public class BarbarianTribeSidebar extends GameJSidebar
	{
		private var lblName:JLabel;
		private var pnlStats:Form;
		private var pnlGroups:JPanel;

		private var tribe: BarbarianTribe;
		
		public function BarbarianTribeSidebar(tribe: BarbarianTribe) 
		{
			this.tribe = tribe;

			tribe.addEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate, false, 0, true);

			createUI();
			update();
		}
		
		public function createUI():void
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
		
		public function onObjectUpdate(e: Event):void
		{
			update();
		}

		public function update():void
		{
			pnlGroups.removeAll();
			pnlStats.removeAll();
			
			addStatRow(StringHelper.localize("STR_LEVEL"), tribe.level.toString());

			addStatRow(StringHelper.localize("BARBARIAN_TRIBE_CAMP_COUNT"), tribe.count.toString());

			addStatRow(StringHelper.localize("BARBARIAN_TRIBE_UPKEEP"), tribe.upkeep().toString());
			
			pnlGroups.append(new SendAttackButton(tribe,new Location(Location.BARBARIAN_TRIBE, tribe.objectId)));

			var buttons: Array = [];

			//Special Case Buttons
			switch(tribe.state.getStateType())
			{
				case SimpleGameObject.STATE_BATTLE:
					pnlGroups.append(new ViewBattleButton(tribe));
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
		
		public function dispose():void
		{
			tribe.removeEventListener(SimpleGameObject.OBJECT_UPDATE, onObjectUpdate);
		}
		
		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			var pt: Point = TileLocator.getMapCoord(tribe.objX, tribe.objY);
			frame.getTitleBar().setText("Barbarian Tribe (" + pt.x + "," + pt.y + ")");

			frame.show();
			return frame;
		}
	}

}