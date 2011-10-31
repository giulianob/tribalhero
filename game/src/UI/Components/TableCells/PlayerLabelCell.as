package src.UI.Components.TableCells
{
	import flash.events.*;
	import org.aswing.*;
	import org.aswing.event.*;
	import org.aswing.table.*;
	import src.*;
	import src.UI.Components.*;
	import src.UI.Components.Messaging.MessagingIcon;
	import src.UI.Components.Tribe.SetRankIcon;

	public class PlayerLabelCell extends AbstractTableCell
	{
		protected var label: PlayerLabel;
		protected var wrapper: JPanel;

		public function PlayerLabelCell()
		{
			super();
			
			wrapper = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			wrapper.setOpaque(true);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			wrapper.removeAll();
			
			if (value is int)
				label = new PlayerLabel(value);
			else
				label = new PlayerLabel(value.playerId, value.playerName);
				
			wrapper.append(label);
		}

		override public function getCellValue():*
		{
			return value;
		}

		override public function getCellComponent():Component
		{
			return wrapper;
		}
	}

}

