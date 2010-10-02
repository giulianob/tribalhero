package src.UI.Dialog
{
	import flash.events.Event;
	import flash.events.KeyboardEvent;
	import flash.events.MouseEvent;
	import flash.events.TimerEvent;
	import flash.ui.Keyboard;
	import flash.utils.Timer;
	import org.aswing.border.EmptyBorder;
	import org.aswing.event.InteractiveEvent;
	import org.aswing.Insets;
	import org.aswing.JScrollPane;
	import org.aswing.JTextArea;
	import org.aswing.JTextField;
	import org.aswing.SoftBoxLayout;
	import src.Constants;
	import src.Global;
	import src.UI.GameJBox;
	import src.UI.LookAndFeel.GameLookAndFeel;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public class CmdLineViewer extends GameJBox
	{

		private var txtConsole: JTextArea;
		private var txtCommand: JTextField;
		private var cmdHistory: Array = new Array();
		private var cmdIndex: int = -1;
		private var scrollConsole: JScrollPane;

		public function CmdLineViewer() {
			createUI();

			scrollConsole.addAdjustmentListener(function(e: InteractiveEvent) : void {
				if (e.isProgrammatic()) {
					scrollConsole.getVerticalScrollBar().setValue(scrollConsole.getVerticalScrollBar().getMaximum(), false);
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

			var cursorEndTimer: Timer = new Timer(10, 0);
			cursorEndTimer.addEventListener(TimerEvent.TIMER, function(e: Event): void {
				cursorEndTimer.stop();
				var len: int = txtCommand.getText().length;
				txtCommand.setSelection(len, len);
			});

			txtCommand.addEventListener(KeyboardEvent.KEY_DOWN, function(e: KeyboardEvent): void {
				if (e.keyCode == Keyboard.ENTER) {
					switch (txtCommand.getText()) {
						case "clr":
						case "clear":
						case "cls":
							txtCommand.setText("");
						break;
						default:
							if (txtCommand.getText().length > 0) {
								log(txtCommand.getText(), true);
								Global.mapComm.Login.sendCommand(txtCommand.getText(), function(resp: String) : void {
									log(resp, false);
							});
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
				else if (e.keyCode == Keyboard.ESCAPE) {
					getFrame().hide();
				}

				e.stopImmediatePropagation();
			});
		}

		private function stageKeyDown(e: KeyboardEvent) : void {
			if (e.keyCode == 192) {
				getFrame().show();
				txtCommand.makeFocus();
			}

			if (e.keyCode == Keyboard.ESCAPE) {
				getFrame().setVisible(false);
			}
		}

		private function getCurrentCmd(): String {
			if (cmdIndex == -1) return "";

			return cmdHistory[cmdIndex];
		}

		private function inCmd() : Boolean {
			return cmdIndex != -1 && cmdHistory[cmdIndex] == txtCommand.getText();
		}

		private function log(str: String, isCommand: Boolean = false) : void {
			if (isCommand) {
				txtConsole.appendText("\n>" + str);
				if (!inCmd()) cmdHistory.push(str);
			} else {
				txtConsole.appendText("\n" + str);
			}
		}

		private function createUI() : void {
			setPreferredWidth(500);
			setBorder(new EmptyBorder(null, new Insets(5, 5, 5, 5)));

			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));

			txtConsole = new JTextArea(">Tribal Hero v" + Constants.version + "." + Constants.revision, 15, 0);
			txtConsole.setWordWrap(true);
			txtConsole.setBackgroundDecorator(null);
			txtConsole.setEditable(false);

			txtCommand = new JTextField();
			txtCommand.setBackgroundDecorator(null);
			txtCommand.setRestrict("^`");

			scrollConsole = new JScrollPane(txtConsole);

			GameLookAndFeel.changeClass(txtCommand, "Tooltip.text");
			GameLookAndFeel.changeClass(txtConsole, "Tooltip.text");

			append(scrollConsole);
			append(txtCommand);
		}
	}

}

