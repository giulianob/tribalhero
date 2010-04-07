package src.UI.Components.TableCells
{
	import flash.events.Event;
	import org.aswing.CenterLayout;
	import org.aswing.Component;
	import org.aswing.JCheckBox;
	import org.aswing.JPanel;
	import org.aswing.JToggleButton;
	import org.aswing.table.AbstractTableCell;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class CheckboxTextCell extends AbstractTableCell
	{
		protected var checkbox: JToggleButton;

		// We have to create a wrapper because the checkbox will be forced to the entire area of the container
		protected var wrapper: JPanel;

		public function CheckboxTextCell()
		{
			super();

			checkbox = new JCheckBox();
			checkbox.addActionListener(function(e: Event) : void {
				value.checked = checkbox.isSelected();
			});
			wrapper = new JPanel(new CenterLayout());
			wrapper.append(checkbox);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);
			checkbox.setSelected(value.checked is Boolean ? value.checked : false);
		}

		override public function getCellValue():*
		{
			return checkbox.isSelected();
		}

		override public function getCellComponent():Component
		{
			return wrapper;
		}
	}

}

