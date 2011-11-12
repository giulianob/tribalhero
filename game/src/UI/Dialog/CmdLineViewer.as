package src.UI.Dialog
{
	import flash.events.*;
	import flash.sampler.NewObjectSample;
	import flash.ui.*;
	import flash.utils.*;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.*;
	import org.aswing.skinbuilder.SkinCustomIcon;
	import org.aswing.skinbuilder.SkinFrameCloseIcon;
	import org.aswing.skinbuilder.SkinFrameMaximizeIcon;
	import src.*;
	import src.UI.*;
	import src.UI.LookAndFeel.*;
	import src.Util.StringHelper;

	public class CmdLineViewer extends GameJBox
	{
		private var pnlContent: JPanel;
		private var txtConsole: JTextArea;
		private var txtCommand: JTextField;
		private var cmdHistory: Array = new Array();
		private var cmdIndex: int = -1;
		private var scrollConsole: JScrollPane;
		private var btnMinimize: JButton;
		private var btnClose: JButton;
		private var btnOpen: JButton;
		
		private var sizeMode: int;

		public function CmdLineViewer() {
			createUI();
			
			addEventListener(Event.ADDED_TO_STAGE, function(e: Event): void
			{
				resizeAndReposition();
			});
		
			var stickScroll: Boolean = true;
			var lastScrollValue: int = 0;
			scrollConsole.addAdjustmentListener(function(e: InteractiveEvent) : void {				
				if (e.isProgrammatic()) {															
					scrollConsole.getVerticalScrollBar().setValue(stickScroll ? scrollConsole.getVerticalScrollBar().getMaximum() : lastScrollValue, false);
				}
				else
				{
					stickScroll = scrollConsole.getVerticalScrollBar().getValue() == scrollConsole.getVerticalScrollBar().getModel().getMaximum() - scrollConsole.getVerticalScrollBar().getModel().getExtent();
					lastScrollValue = scrollConsole.getVerticalScrollBar().getValue();
				}
			});

			addEventListener(MouseEvent.MOUSE_DOWN, function(e: Event) : void {
				e.stopImmediatePropagation();
			});

			addEventListener(Event.ADDED_TO_STAGE, function(e: Event) : void {
				stage.addEventListener(KeyboardEvent.KEY_DOWN, stageKeyDown);
			});

			addEventListener(Event.REMOVED_FROM_STAGE, function(e: Event) : void {
				stage.removeEventListener(KeyboardEvent.KEY_DOWN, stageKeyDown);
			});
			
			btnClose.addActionListener(onClose);
			btnOpen.addActionListener(onOpen);

			var cursorEndTimer: Timer = new Timer(10, 0);
			cursorEndTimer.addEventListener(TimerEvent.TIMER, function(e: Event): void {
				cursorEndTimer.stop();
				var len: int = txtCommand.getText().length;
				txtCommand.setSelection(len, len);
			});
			
			btnMinimize.addActionListener(function(e: Event): void {				
				sizeMode = 1 - sizeMode;
				resizeAndReposition();
			});

			txtCommand.addEventListener(KeyboardEvent.KEY_DOWN, function(e: KeyboardEvent): void {
				if (e.keyCode == Keyboard.ENTER) {
					switch (txtCommand.getText()) {
						case "clr":
						case "clear":
						case "cls":
							txtConsole.setText("");
						break;
						default:
							if (txtCommand.getText().length > 0) {																
								var message: String = StringHelper.trim(txtCommand.getText());
								
								if (message.charAt(0) == '/')
								{								
									log(txtCommand.getText(), true);
									
									Global.mapComm.General.sendCommand(message.substr(1), function(resp: String) : void {
										log(resp, false);
									});
								}
								else
								{
									Global.mapComm.General.sendChat(message, function(resp: String) : void {
										log(resp, false);
									});
								}
								
								saveToHistory(message);
								txtCommand.setText("");
							}
						break;
					}
				}
				else if (e.keyCode == Keyboard.UP) {
					if (cmdIndex == -1) cmdIndex = cmdHistory.length - 1;

					if (inCmd() && cmdIndex > 0) cmdIndex--;
					txtCommand.setText(getCurrentCmd());
					cursorEndTimer.start();
				}
				else if (e.keyCode == Keyboard.DOWN) {
					if (cmdIndex == -1) cmdIndex = cmdHistory.length - 1;
					if (inCmd() && cmdIndex < cmdHistory.length - 1) cmdIndex ++;
					txtCommand.setText(getCurrentCmd());
				}

				e.stopImmediatePropagation();
			});
		}

		private function stageKeyDown(e: KeyboardEvent) : void {			
			if (e.keyCode == 192) {
				getFrame().show();
				txtCommand.makeFocus();
			}
		}

		private function getCurrentCmd(): String {
			if (cmdIndex == -1) return "";

			return cmdHistory[cmdIndex];
		}

		private function inCmd() : Boolean {
			return cmdIndex != -1 && cmdHistory[cmdIndex] == txtCommand.getText();
		}

		public function log(str: String, isCommand: Boolean = false) : void {
			if (str.length == 0)
				return;
			
			if (txtConsole.getLength() > 8000)
				txtConsole.replaceText(0, txtConsole.getLength() - 8000, "");
			
			if (isCommand)
			{
				txtConsole.appendText("\n>" + str);
			} else {
				txtConsole.appendText("\n" + str);
			}
		}
		
		private function saveToHistory(str: String): void {
			if (!inCmd()) {
				if (cmdHistory.length > 50)
					cmdHistory.shift();
				
				cmdHistory.push(str);
				
				cmdIndex = cmdHistory.length - 1;
			}
		}
		
		private function resizeAndReposition(): void {
			var rowsInScreen: int = Constants.screenH / txtConsole.getFont().computeTextSize(" ", false).height;
			
			switch (sizeMode)
			{
				case 0:
					txtConsole.setRows(rowsInScreen * 0.20);					
					break;
				case 1:
					txtConsole.setRows(rowsInScreen * 0.75);
					break;
			}			
			
			getFrame().pack();
			getFrame().setLocationXY(300, Constants.screenH - getFrame().getHeight());
		}
		
		private function onClose(e: Event = null) : void {
			remove(pnlContent);
			append(btnOpen);
			resizeAndReposition();
		}
		
		private function onOpen(e: Event = null) : void {
			remove(btnOpen);
			append(pnlContent);
			resizeAndReposition();
		}

		private function createUI() : void {				
			setLayout(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			
			pnlContent = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));		
			pnlContent.setBorder(new EmptyBorder(null, new Insets(5, 5, 5, 5)));
			
			var pnlToolbar: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 0, 0, false));
			pnlToolbar.setPreferredWidth(650);
			
			btnMinimize = new JButton("", new SkinFrameMaximizeIcon());
			btnMinimize.setBackgroundDecorator(null);			
			
			btnClose = new JButton("", new SkinFrameCloseIcon());
			btnClose.setBackgroundDecorator(null);
			
			btnOpen = new JButton("", new SkinCustomIcon("Frame.chatIcon"));
			btnOpen.setBackgroundDecorator(null);

			txtConsole = new JTextArea("Welcome to Tribal Hero v" + Constants.version + "." + Constants.revision, 15, 0);
			txtConsole.setWordWrap(true);
			txtConsole.setBackgroundDecorator(null);
			txtConsole.setEditable(false);			

			txtCommand = new JTextField();
			txtCommand.setBackgroundDecorator(null);
			txtCommand.setRestrict("^`");
			txtCommand.setConstraints("Center");
			txtCommand.setMaxChars(450);
			
			var lblCommandCursor: JLabel = new JLabel(">");
			lblCommandCursor.setConstraints("West");

			scrollConsole = new JScrollPane(txtConsole, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);

			GameLookAndFeel.changeClass(txtCommand, "Console.text");
			GameLookAndFeel.changeClass(txtConsole, "Console.text");
			GameLookAndFeel.changeClass(lblCommandCursor, "Tooltip.text");

			var pnlCommandHolder: JPanel = new JPanel(new BorderLayout());
			
			pnlCommandHolder.appendAll(lblCommandCursor, txtCommand);
			
			pnlToolbar.appendAll(btnMinimize, btnClose);
			
			pnlContent.append(pnlToolbar);
			pnlContent.append(scrollConsole);
			pnlContent.append(pnlCommandHolder);
			
			append(pnlContent);
		}
	}

}

