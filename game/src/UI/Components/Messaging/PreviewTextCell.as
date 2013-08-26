package src.UI.Components.Messaging
{
    import org.aswing.AsWingConstants;
    import org.aswing.Component;
    import org.aswing.FlowLayout;
    import org.aswing.JPanel;
    import org.aswing.JTable;
    import org.aswing.JTextField;
    import org.aswing.plaf.basic.background.TextComponentBackBround;
    import org.aswing.table.AbstractTableCell;

    import src.UI.LookAndFeel.GameLookAndFeel;

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
			subject.setBackgroundDecorator(new TextComponentBackBround());
			subject.getTextField().mouseEnabled = false;
			subject.setEditable(false);
			subject.setOpaque(false);
			subject.getTextField().selectable = false;

			preview = new JTextField();
			preview.setBackgroundDecorator(new TextComponentBackBround());
			preview.setForeground(GameLookAndFeel.getClassAttribute("Message.preview", "TextField.foreground"));
			preview.getTextField().mouseEnabled = false;
			preview.getTextField().selectable = false;
			preview.setEditable(false);
			preview.setOpaque(false);

			wrapper = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			wrapper.buttonMode = true;					
			wrapper.append(subject);
			wrapper.append(preview);
			wrapper.setOpaque(true);
		}

		override public function setCellValue(value:*):void
		{
			super.setCellValue(value);

			if (value.unread) {
				subject.setFont(GameLookAndFeel.getClassAttribute("Message.unread", "TextField.font"));
				subject.setForeground(GameLookAndFeel.getClassAttribute("Message.unread", "TextField.foreground"));
			} else {
				subject.setFont(GameLookAndFeel.getClassAttribute("Message.read", "TextField.font"));
				subject.setForeground(GameLookAndFeel.getClassAttribute("Message.read", "TextField.foreground"));
			}

			subject.setText(value.subject);
			preview.setText(value.preview);
		}

		override public function getCellComponent():Component
		{
			return wrapper;
		}

		override public function setTableCellStatus(table:JTable, isSelected:Boolean, row:int, column:int):void{
			if(isSelected){
				wrapper.setBackground(table.getSelectionBackground());
				wrapper.setForeground(table.getSelectionForeground());
			}else{				
				wrapper.setBackground(table.getBackground());
				wrapper.setForeground(table.getForeground());
			}			
		}
	}

}

