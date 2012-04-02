package src.UI.Dialog
{
	import flash.events.*;
	import flash.text.*;
	import flash.ui.*;
	import flash.utils.*;
	import mx.formatters.*;
	import mx.utils.StringUtil;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.*;
	import org.aswing.plaf.basic.BasicComboBoxUI;
	import org.aswing.skinbuilder.*;
	import src.*;
	import src.UI.*;
	import src.UI.Components.SimpleTooltip;
	import src.UI.LookAndFeel.*;
	import src.Util.*;

	public class CmdLineViewer extends GameJBox
	{
		private const MAX_CHAT_LENGTH: int = 15000;
		
		public const TYPE_GLOBAL: int = 0;
		public const TYPE_TRIBE: int = 1;
		
		public var CURRENT_CHAT_TYPE: int = 0;
		
		private var pnlContent: JPanel;
		private var txtConsole: JTextArea;
		private var txtCommand: JTextField;
		private var cmdHistory: Array = new Array();
		private var cmdIndex: int = -1;
		private var scrollConsole: JScrollPane;
		private var btnMinimize: JButton;
		private var btnDisableGlobalChat: JButton;
		private var btnClose: JButton;
		private var btnOpen: JButton;
		private var lstChatType: JComboBox;
		
		private var profanityFilter: ProfanityFilter = new ProfanityFilter();
		
		private var sizeMode: int;
		
		private var publicChatDisabled: Boolean;
		
		// We need to keep the chat separately from what's in the input.
		// This means a bit more memory used for chat than what is ideal but it's what we gotta do.
		private var chat: String = "";

		public function CmdLineViewer() {
			createUI();
			
			log('<a href="http://tribalhero.com/pages/donate" target="_blank">Donate to improve</a> Tribal Hero if you are enjoying the game.', false, false);
			log('Not sure what to do? Visit the <a href="http://tribalhero.wikia.com" target="_blank">wiki</a>.', false, false);
			log('Remember to keep it classy.');
			
			addEventListener(Event.ADDED_TO_STAGE, function(e: Event): void
			{
				if (Constants.screenH < 600)
				{
					onClose();
				}
				else
				{
					resizeAndReposition();
				}
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
			
			txtConsole.addEventListener(MouseEvent.MOUSE_WHEEL, function(e: Event): void {
				scrollConsole.getVerticalScrollBar().dispatchEvent(e);
			});

			txtConsole.getTextField().addEventListener(TextEvent.LINK, function(e: TextEvent) : void {
				var text: String = e.text;
				var parts: Array = text.split(':', 2);
				
				switch (parts[0])
				{
					case 'viewProfile':
						Global.mapComm.City.viewPlayerProfile(parts[1]);
						break;
				}
			});

			txtCommand.addEventListener(KeyboardEvent.KEY_DOWN, function(e: KeyboardEvent): void {
				if (e.keyCode == Keyboard.ENTER) {
					if (sendChat(lstChatType.getSelectedIndex() == 0 ? TYPE_GLOBAL : TYPE_TRIBE, txtCommand.getText())) {
						txtCommand.setText("");
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
					stage.focus = Global.map;
				}

				e.stopImmediatePropagation();
			});
			
			btnDisableGlobalChat.addActionListener(function(e: Event):void {
				if (publicChatDisabled) {
					btnDisableGlobalChat.setIcon(new SkinCustomIcon("Frame.chatEnabledIcon"));
				}
				else {
					btnDisableGlobalChat.setIcon(new SkinCustomIcon("Frame.chatDisabledIcon"));
				}
				
				publicChatDisabled = !publicChatDisabled;
			});
		}
		
		private function sendChat(type: int, message: String) : Boolean {			
			switch (message) {
				case "/clr":
				case "/clear":
				case "/cls":
					chat = "";
					txtConsole.setHtmlText("");
				break;
				default:
					if (message.length == 0) {																
						return false;
					}
					
					if (message.substr(0, 3) == "/t ") {
						message = message.substr(4);
						type = TYPE_TRIBE;
					}
					
					if (type == TYPE_GLOBAL && publicChatDisabled) {
						log("You can't use global chat when you have it disabled. Enable it then try again.");
						return false;
					}
					
					message = StringHelper.trim(message);
					
					if (profanityFilter.quickValidate(message) == false) {
						log('Looks like your chat message contains some offensive terms. Please keep it classy.', false);
						return false;
					}
					
					if (message.charAt(0) == '/')
					{								
						log(message, true);
						
						Global.mapComm.General.sendCommand(message.substr(1), function(resp: String) : void {
							log(resp, false);
						});
					}
					else
					{
						Global.mapComm.General.sendChat(type, message, function(resp: String) : void {
							log(resp, false);
						});
					}
					
					saveToHistory(message);						
				break;
			}
			
			return true;
		}

		private function getCurrentCmd(): String {
			if (cmdIndex == -1) return "";

			return cmdHistory[cmdIndex];
		}

		private function inCmd() : Boolean {
			return cmdIndex != -1 && cmdHistory[cmdIndex] == txtCommand.getText();
		}
		
		public function logChat(type: int, playerId: int, playerName: String, str: String): void {			
			var f: DateFormatter = new DateFormatter();
            f.formatString = "LL:NN";
			
			var cssClass: String = '';
			
			if (playerId == Constants.playerId)
			{
				cssClass = 'self';								
			}			
			else
			{
				switch (type)
				{
					case TYPE_TRIBE:
						cssClass = 'tribe';
						break;
					default:
						if (publicChatDisabled) {
							return;
						}
						
						cssClass = 'global';
						break;
				}
			}
			
			log(StringUtil.substitute('[{0}] {1}<a href="event:viewProfile:{3}"><span class="{2}">{4}</span></a>: {5}', f.format(new Date()), (type == TYPE_TRIBE ? "(Tribe) " : ""), cssClass, playerId, StringHelper.htmlEscape(playerName), StringHelper.linkify(str)), false, false);
		}

		public function log(str: String, isCommand: Boolean = false, escapeStr: Boolean = true) : void {
			if (str.length == 0)
				return;
				
			// Remove new lines
			str = str.replace("\n", "");
			
			if (escapeStr)			
				str = StringHelper.htmlEscape(str);
			
			// This should be moved to the guy calling it w/ command response
			if (isCommand)			
				str = "&gt;" + str;
			
			chat += "<p>" + str + "</p>";
				
			if (chat.length > MAX_CHAT_LENGTH)
			{
				var newlineIdx: int = chat.indexOf("</p>", chat.length - MAX_CHAT_LENGTH) + 4;
				chat = chat.substr(newlineIdx);
			}
			
			txtConsole.setHtmlText(chat);
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
			
			btnDisableGlobalChat = new JButton("", new SkinCustomIcon("Frame.chatEnabledIcon"));
			btnDisableGlobalChat.setBackgroundDecorator(null);
			new SimpleTooltip(btnDisableGlobalChat, "Mute global chat");

			txtConsole = new JTextArea("", 15, 0);
			txtConsole.setWordWrap(true);
			txtConsole.setBackgroundDecorator(null);
			txtConsole.setEditable(false);		
			
			var consoleCss: StyleSheet = new StyleSheet();
			consoleCss.setStyle("p", { marginBottom:'3px', leading:3, fontFamily:'Arial', fontSize:12, color:'#FFFFFF' });
			consoleCss.setStyle("a:link", { fontWeight:'bold', textDecoration:'none', color:'#8ecafe' });
			consoleCss.setStyle("a:hover", { textDecoration:'underline' } );
			
			consoleCss.setStyle(".global", { color:'#8ecafe' } );
			consoleCss.setStyle(".self", { color:'#aef64f' } );
			consoleCss.setStyle(".tribe", { color:'#ffff06' } );
			
			txtConsole.setCSS(consoleCss);

			txtCommand = new JTextField();
			txtCommand.setBackgroundDecorator(null);
			txtCommand.setConstraints("Center");
			txtCommand.setMaxChars(450);
			
			var lblCommandCursor: JLabel = new JLabel(">");
			lblCommandCursor.setConstraints("West");

			scrollConsole = new JScrollPane(txtConsole, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);

			GameLookAndFeel.changeClass(txtCommand, "Console.text");			
			GameLookAndFeel.changeClass(lblCommandCursor, "Tooltip.text");

			lstChatType = new JComboBox(new Array("Global", "Tribe"));			
			lstChatType.setSelectedIndex(0, true);			
			lstChatType.setConstraints("West");
			lstChatType.setPreferredWidth(65);
			lstChatType.setBackgroundDecorator(null);				
			lstChatType.setUI(new BasicComboBoxUI());
			GameLookAndFeel.changeClass(lstChatType, "Console.combobox");			
			
			var pnlCommandLineHolder: JPanel = new JPanel(new BorderLayout());
			pnlCommandLineHolder.setConstraints("Center");
			pnlCommandLineHolder.appendAll(lblCommandCursor, txtCommand);
			
			var pnlCommandHolder: JPanel = new JPanel(new BorderLayout());
			
			pnlCommandHolder.appendAll(lstChatType, pnlCommandLineHolder);
			
			pnlToolbar.appendAll(btnDisableGlobalChat, btnMinimize, btnClose);
			
			pnlContent.append(pnlToolbar);
			pnlContent.append(scrollConsole);
			pnlContent.append(pnlCommandHolder);
			
			append(pnlContent);
		}
	}

}

