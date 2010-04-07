package src.UI.Components.Messaging
{
	import org.aswing.AsWingConstants;
	import org.aswing.Component;
	import org.aswing.FlowLayout;
	import org.aswing.JPanel;
	import org.aswing.JTextField;
	import org.aswing.table.AbstractTableCell;
	import src.UI.GameLookAndFeel;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class PreviewTextCell extends AbstractTableCell
	{
		// We have to use textfields instead of labels because the default
		// implementation of the cell uses textfield instead of label
		protected var subject: JTextField;
		protected var preview: JTextField;

		protected var wrapper: JPanel;

		public function PreviewTextCell()
		{
			super();

			subject = new JTextField();
			subject.getTextField().mouseEnabled = false;
			subject.setEditable(false);
			subject.setOpaque(false);
			subject.getTextField().selectable = false;

			preview = new JTextField();
			preview.setForeground(GameLookAndFeel.getClassAttribute("Message.preview", "Textfield.foreground"));
			preview.getTextField().mouseEnabled = false;
			preview.getTextField().selectable = false;
			preview.setEditable(false);
			preview.setOpaque(false);

			wrapper = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			wrapper.buttonMode = true;
			wrapper.append(subject);
			wrapper.append(preview);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);

			if (value.unread) {
				subject.setFont(GameLookAndFeel.getClassAttribute("Message.unread", "Textfield.font"));
				subject.setForeground(GameLookAndFeel.getClassAttribute("Message.unread", "Textfield.foreground"));
			} else {
				subject.setFont(GameLookAndFeel.getClassAttribute("Message.read", "Textfield.font"));
				subject.setForeground(GameLookAndFeel.getClassAttribute("Message.read", "Textfield.foreground"));
			}

			subject.setText(value.subject);
			preview.setText(value.preview);
		}

		override public function getCellComponent():Component
		{
			return wrapper;
		}
	}

}

