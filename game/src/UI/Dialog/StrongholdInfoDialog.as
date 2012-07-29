/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.UI.Dialog {
	import flash.display.*
	import org.aswing.AsWingConstants;
	import org.aswing.Component;
	import org.aswing.ext.Form;
	import org.aswing.Icon;
	import org.aswing.JFrame;
	import org.aswing.JLabel;
	import src.Global;
	import src.Map.*;
	import src.Objects.*;
	import src.Objects.Stronghold.Stronghold;
	import src.Objects.Troop.*;
	import src.UI.Components.SimpleTroopGridList.SimpleTroopGridList;
	import src.UI.GameJPanel;

	public class StrongholdInfoDialog extends GameJPanel {

		private var stronghold: Stronghold;
		private var pnlStats: Form;

		public function StrongholdInfoDialog(stronghold: Stronghold):void
		{
			title = "Stronghold Information";

			this.stronghold = stronghold;
			pnlStats = new Form();

			addStatRow("Name", Global.map.usernames.strongholds.getUsername(stronghold.id).name);
			addStatRow("Level", stronghold.level.toString());
			if(stronghold.tribeId!=0) {
				addStatRow("TribeId", Global.map.usernames.tribes.getUsername(stronghold.tribeId).name);
			} else {
				addStatRow("TribeId", "Not occupied");
			}
			addStatRow("Id", stronghold.id.toString());
			append(pnlStats);
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


		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
	}

}

