package src.UI.Dialog
{
	import fl.lang.Locale;
	import flash.events.*;
	import flash.text.*;
	import flash.ui.*;
	import flash.utils.*;
	import mx.formatters.*;
	import mx.utils.StringUtil;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.event.*;
	import org.aswing.geom.IntDimension;
	import org.aswing.plaf.basic.BasicComboBoxUI;
	import org.aswing.skinbuilder.*;
	import src.*;
	import src.UI.*;
	import src.UI.Components.SimpleTooltip;
	import src.UI.LookAndFeel.*;
	import src.Util.*;
	
	public class CmdLineViewer extends GameJBox
	{
		private const MAX_CHAT_LENGTH:int = 45000;
		
		public const CHANNELS: Array = [{name: "CHAT_CHANNEL_GLOBAL"}, {name: "CHAT_CHANNEL_TRIBE"}, {name: "CHAT_CHANNEL_OFFTOPIC"}];
		
		public const TYPE_GLOBAL:int = 0;
		public const TYPE_TRIBE:int = 1;
		public const TYPE_OFFTOPIC:int = 2;
		
		private var pnlContent:JPanel;
		private var txtConsole:JTextArea;
		private var txtCommand:JTextField;
		private var cmdHistory:Array = new Array();
		private var cmdIndex:int = -1;
		private var scrollConsole:JScrollPane;
		private var channelTabs: JPanel;
		private var btnClose:JButton;
		private var btnOpen:JButton;
		private var currentChatType:int;
		private var tabbedPanel:int;
		
		private var profanityFilter:ProfanityFilter = new ProfanityFilter();		
		
		private var maximizedSize: IntDimension;
		
		// We need to keep the chat separately from what's in the input.
		// This means a bit more memory used for chat than what is ideal but it's what we gotta do.
		private var chats:Array;
		
		public function CmdLineViewer()
		{
			createUI();
			
			chats = new Array(CHANNELS.length);				
			for (var i: int = 0; i < chats.length; i++) {
				chats[i] = "";
			}
			
			log(TYPE_GLOBAL, '<a href="http://tribalhero.com/pages/donate" target="_blank">We need donations</a> to keep improving the game.', false, false);
			log(TYPE_GLOBAL, 'Remember to keep it classy.');
			log(TYPE_GLOBAL, 'Not sure what to do? <a href="http://tribalhero.wikia.com" target="_blank">Visit the wiki for help</a>.', false, false);
			
			var stickScroll:Boolean = true;
			var lastScrollValue:int = 0;
			scrollConsole.addAdjustmentListener(function(e:InteractiveEvent):void
				{
					if (e.isProgrammatic())
					{
						scrollConsole.getVerticalScrollBar().setValue(stickScroll ? scrollConsole.getVerticalScrollBar().getMaximum() : lastScrollValue, false);
					}
					else
					{
						stickScroll = scrollConsole.getVerticalScrollBar().getValue() == scrollConsole.getVerticalScrollBar().getModel().getMaximum() - scrollConsole.getVerticalScrollBar().getModel().getExtent();
						lastScrollValue = scrollConsole.getVerticalScrollBar().getValue();
					}
				});
			
			addEventListener(MouseEvent.MOUSE_DOWN, function(e:Event):void
				{
					e.stopImmediatePropagation();
				});
			
			btnClose.addActionListener(onClose);
			btnOpen.addActionListener(onOpen);
			
			var cursorEndTimer:Timer = new Timer(10, 0);
			cursorEndTimer.addEventListener(TimerEvent.TIMER, function(e:Event):void
				{
					cursorEndTimer.stop();
					var len:int = txtCommand.getText().length;
					txtCommand.setSelection(len, len);
				});
						
			txtConsole.addEventListener(MouseEvent.MOUSE_WHEEL, function(e:Event):void
				{
					scrollConsole.getVerticalScrollBar().dispatchEvent(e);
				});
			
			txtConsole.getTextField().addEventListener(TextEvent.LINK, function(e:TextEvent):void
				{
					var text:String = e.text;
					var parts:Array = text.split(':', 2);
					
					switch (parts[0])
					{
						case 'viewProfile': 
							Global.mapComm.City.viewPlayerProfile(parts[1]);
							break;
					}
				});
			
			txtCommand.addEventListener(KeyboardEvent.KEY_DOWN, function(e:KeyboardEvent):void
				{
					if (e.keyCode == Keyboard.ENTER)
					{
						if (sendChat(currentChatType, txtCommand.getText()))
						{
							txtCommand.setText("");
						}
					}
					else if (e.keyCode == Keyboard.UP)
					{
						if (cmdIndex == -1)
							cmdIndex = cmdHistory.length - 1;
						
						if (inCmd() && cmdIndex > 0)
							cmdIndex--;
						txtCommand.setText(getCurrentCmd());
						cursorEndTimer.start();
					}
					else if (e.keyCode == Keyboard.DOWN)
					{
						if (cmdIndex == -1)
							cmdIndex = cmdHistory.length - 1;
						if (inCmd() && cmdIndex < cmdHistory.length - 1)
							cmdIndex++;
						txtCommand.setText(getCurrentCmd());
					}
					else if (e.keyCode == Keyboard.ESCAPE)
					{
						stage.focus = Global.map;
					}
					else {
						channelHotkeys(e);
					}
					
					e.stopImmediatePropagation();
				});
		}
		
		private function sendChat(type:int, message:String):Boolean
		{
			if (message.length == 0)
			{
				return false;
			}
				
			message = StringHelper.trim(message);
			
			if (profanityFilter.quickValidate(message) == false)
			{
				log(currentChatType, 'Looks like your chat message contains some offensive terms. Please keep it classy.', false);
				return false;
			}
			
			if (message.charAt(0) == '/')
			{
				log(currentChatType, message, true);				
				
				Global.mapComm.General.sendCommand(message.substr(1), function(resp:String, type: int = 0):void
					{
						log(currentChatType, resp, false);
					});
			}
			else
			{
				Global.mapComm.General.sendChat(type, message, function(resp:String, type: int = 0):void
					{
						log(type, resp, false);
					});
			}
			
			saveToHistory(message);
			
			return true;
		}
		
		override public function show(owner:* = null, hasTitle:Boolean = false):JFrame 
		{
			var frame: JFrame = super.show(owner, hasTitle);		
			frame.stage.addEventListener(KeyboardEvent.KEY_DOWN, channelHotkeys);
			
			frame.addEventListener(FrameEvent.FRAME_CLOSING, function(e: Event): void {
				frame.removeEventListener(KeyboardEvent.KEY_DOWN, channelHotkeys);
			});

			frame.addEventListener(ResizedEvent.RESIZED, function(e: ResizedEvent): void {
				// Only set the maximized size if we are actually maximized
				if (pnlContent.getParent()) {
					maximizedSize = getFrame().getSize();
				}
				
				resizeAndReposition();				
			});
			
			frame.setSize(new IntDimension(Math.min(550, Constants.screenW * 0.5), Math.min(Constants.screenH - 300, Constants.screenH * 0.3)));
			maximizedSize = frame.getSize();
			
			if (Constants.screenH < 600)
			{
				onClose();
			}
			else
			{						
				resizeAndReposition();
			}			
			
			return frame;
		}
		
		private function channelHotkeys(e: KeyboardEvent): void {
			// Allow switching channels by pressing CTRL+#
			if (e.ctrlKey) {				
				for (var i: int = 0; i < CHANNELS.length; i++) {
					if (e.keyCode == Keyboard.NUMBER_1 + i) {
						var button:JToggleButton = CHANNELS[i].button;
						button.doClick();
						break;
					}
				}			
			}
		}
		
		private function getCurrentCmd():String
		{
			if (cmdIndex == -1)
				return "";
			
			return cmdHistory[cmdIndex];
		}
		
		private function inCmd():Boolean
		{
			return cmdIndex != -1 && cmdHistory[cmdIndex] == txtCommand.getText();
		}
		
		public function logChat(type:int, playerId:int, playerName:String, str:String):void
		{
			var f:DateFormatter = new DateFormatter();
			f.formatString = "LL:NN";
			
			var cssClass:String = '';
			
			if (playerId == Constants.playerId)
			{
				cssClass = 'self';
			}
			else
			{
				cssClass = 'global';
			}
			
			log(type, StringUtil.substitute('[{0}] {1}<a href="event:viewProfile:{3}"><span class="{2}">{4}</span></a>: {5}', f.format(new Date()), "", cssClass, playerId, StringHelper.htmlEscape(playerName), StringHelper.linkify(str)), false, false);
			
			if (type != currentChatType) {
				var button: JToggleButton = CHANNELS[type].button;
				button.setIcon(new SkinCustomIcon("Frame.chatEnabledIcon"));
			}
		}
		
		public function log(type: int, str:String, isCommand:Boolean = false, escapeStr:Boolean = true):void
		{
			if (str.length == 0)
				return;
			
			// Remove new lines
			str = str.replace("\n", "");
			
			if (escapeStr)
				str = StringHelper.htmlEscape(str);
			
			// This should be moved to the guy calling it w/ command response
			if (isCommand)
				str = "&gt;" + str;
			
			chats[type] += "<p>" + str + "</p>";
			
			if (chats[type].length > MAX_CHAT_LENGTH)
			{
				var newlineIdx:int = chats[type].indexOf("</p>", chats[type].length - MAX_CHAT_LENGTH) + 4;
				chats[type] = chats[type].substr(newlineIdx);
			}
			
			refreshText();
		}
		
		private function refreshText():void
		{
			txtConsole.setHtmlText(chats[currentChatType]);
		}
		
		private function saveToHistory(str:String):void
		{
			if (!inCmd())
			{
				if (cmdHistory.length > 50)
					cmdHistory.shift();
				
				cmdHistory.push(str);
				
				cmdIndex = cmdHistory.length - 1;
			}
		}
		
		private function resizeAndReposition():void
		{								
			if (pnlContent.getParent()) {
				frame.setResizable(true);
				frame.setMinimumSize(new IntDimension(360, 150));
			}
			else {
				frame.setResizable(false);
				frame.setMinimumSize(new IntDimension(0, 0));
			}
			
			getFrame().setWidth(Math.min(getFrame().getWidth(), Constants.screenW - 5));
			getFrame().setHeight(Math.min(getFrame().getHeight(), Constants.screenH - 75));
			
			getFrame().setLocationXY(5, Constants.screenH - getFrame().getHeight());
		}
		
		private function onClose(e:Event = null):void
		{
			remove(pnlContent);
			append(btnOpen);	
			resizeAndReposition();
			getFrame().pack();
			resizeAndReposition();
		}
		
		private function onOpen(e:Event = null):void
		{
			remove(btnOpen);
			append(pnlContent);
			getFrame().setSize(maximizedSize);
			resizeAndReposition();
		}
		
		private function createTab(type: int):JToggleButton
		{
			var button: JToggleButton = new JToggleButton(Locale.loadString(CHANNELS[type].name), new SkinCustomIcon("Frame.chatDisabledIcon"));
			button.setSelected(type == currentChatType);
			
			button.addActionListener(function(e: Event): void {				
				if (!button.isSelected()) {
					button.setSelected(true);
					return;
				}
				
				for each (var channel: * in CHANNELS) {
					if (channel.button != button) {						
						channel.button.setSelected(false);
					}
				}
					
				button.setIcon(new SkinCustomIcon("Frame.chatDisabledIcon"));				
				
				currentChatType = type;
				
				refreshText();
				
				txtCommand.requestFocus();
			});
			GameLookAndFeel.changeClass(button, "ChatTab");
			CHANNELS[type].button = button;
			
			return button;
		}
		
		private function createUI():void
		{						
			setLayout(new BorderLayout());
			
			pnlContent = new JPanel(new BorderLayout(SoftBoxLayout.Y_AXIS));
			pnlContent.setConstraints("Center");
			pnlContent.setBorder(new EmptyBorder(null, new Insets(5, 5, 5, 5)));
			
			var pnlToolbar:JPanel = new JPanel(new BorderLayout(0, 0));
			pnlToolbar.setConstraints("North");
			
			var pnlToolbarButtons:JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 0, 0, false));
			pnlToolbarButtons.setConstraints("East");
			
			btnClose = new JButton("", new SkinFrameCloseIcon());
			btnClose.setMargin(new Insets(3, 0, 0, 0));
			btnClose.setStyleTune(null);
			btnClose.setBackground(null);
			btnClose.setForeground(null);
			btnClose.setMideground(null);
			btnClose.setBackgroundDecorator(null);
			
			btnOpen = new JButton("", new SkinCustomIcon("Frame.chatIcon"));
			btnOpen.setBackgroundDecorator(null);			
			
			// Create channel tabs
			channelTabs = new JPanel(new FlowLayout(AsWingConstants.LEFT, 5, 0, false));			
			channelTabs.setConstraints("Center")			
			GameLookAndFeel.changeClass(channelTabs, "ChatTabbedPane");
			for (var i: int = 0; i < CHANNELS.length; i++) {
				channelTabs.append(createTab(i));
			}
			
			txtConsole = new JTextArea("", 15, 0);
			txtConsole.setWordWrap(true);
			txtConsole.setBackgroundDecorator(null);
			txtConsole.setEditable(false);
			txtConsole.setConstraints("Center");
			
			var consoleCss:StyleSheet = new StyleSheet();
			consoleCss.setStyle("p", {marginBottom: '3px', leading: 3, fontFamily: 'Arial', fontSize: 12, color: '#FFFFFF'});
			consoleCss.setStyle("a:link", {fontWeight: 'bold', textDecoration: 'none', color: '#8ecafe'});
			consoleCss.setStyle("a:hover", {textDecoration: 'underline'});
			
			consoleCss.setStyle(".global", {color: '#8ecafe'});
			consoleCss.setStyle(".self", {color: '#aef64f'});
			
			txtConsole.setCSS(consoleCss);
			
			txtCommand = new JTextField();
			txtCommand.setBackgroundDecorator(null);
			txtCommand.setConstraints("Center");
			txtCommand.setMaxChars(450);
			
			var lblCommandCursor:JLabel = new JLabel(">");
			lblCommandCursor.setConstraints("West");
			
			scrollConsole = new JScrollPane(txtConsole, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			
			GameLookAndFeel.changeClass(txtCommand, "Console.text");
			GameLookAndFeel.changeClass(lblCommandCursor, "Tooltip.text");
			
			var pnlCommandLineHolder:JPanel = new JPanel(new BorderLayout());
			pnlCommandLineHolder.setConstraints("Center");
			pnlCommandLineHolder.appendAll(lblCommandCursor, txtCommand);
			
			var pnlCommandHolder:JPanel = new JPanel(new BorderLayout());
			pnlCommandHolder.setConstraints("South");
			
			pnlCommandHolder.appendAll(pnlCommandLineHolder);
			
			pnlToolbar.appendAll(channelTabs, pnlToolbarButtons);
			pnlToolbarButtons.appendAll(btnClose);
			
			pnlContent.append(pnlToolbar);
			pnlContent.append(scrollConsole);
			pnlContent.append(pnlCommandHolder);
			
			append(pnlContent);
		}
	}

}

