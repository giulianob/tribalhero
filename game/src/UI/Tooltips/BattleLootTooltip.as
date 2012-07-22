package src.UI.Tooltips
{
	import flash.display.DisplayObject;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AsWingConstants;
	import org.aswing.JLabel;
	import org.aswing.JPanel;
	import org.aswing.SoftBoxLayout;
	import src.Objects.Resources;
	import src.UI.Components.SimpleResourcesPanel;
	import src.UI.GameJBox;
	import src.UI.LookAndFeel.GameLookAndFeel;

	/**
	 * ...
	 * @author
	 */
	public class BattleLootTooltip
	{
		private var obj: DisplayObject;
		private var tooltip: Tooltip;

		public function BattleLootTooltip(obj: DisplayObject, loot: Resources, bonus: Resources)
		{
			this.obj = obj;
			this.tooltip = new Tooltip();

			obj.addEventListener(MouseEvent.MOUSE_MOVE, onRollOver);
			obj.addEventListener(MouseEvent.ROLL_OUT, onRollOut);

			var lblLootTitle: JLabel = new JLabel("Stolen Loot");
			lblLootTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblLootTitle, "header");

			var lblBonusTitle: JLabel = new JLabel("Bonus Loot");
			lblBonusTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(lblBonusTitle, "header");

			var pnlLoot: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			pnlLoot.append(lblLootTitle);
			pnlLoot.append(new SimpleResourcesPanel(loot, false, true));

			var pnlBonus: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			pnlBonus.append(lblBonusTitle);
			pnlBonus.append(new SimpleResourcesPanel(bonus, false, true));

			var ui: GameJBox = tooltip.getUI();

			ui.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));

			ui.append(pnlLoot);
			ui.append(pnlBonus);

			ui.pack();
		}

		private function onRollOver(e: Event):void {
			tooltip.show(obj);
		}

		private function onRollOut(e: Event):void {
			tooltip.hide();
		}
	}

}
