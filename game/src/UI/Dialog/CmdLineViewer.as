package src.UI.Dialog
{
	import src.Util.StringHelper;
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
	import src.UI.Components.StickyScroll;
	import src.UI.LookAndFeel.*;
	import src.Util.*;
	
	public class CmdLineViewer extends GameJBox
	{
		private const MAX_CHAT_LENGTH:int = 45000;
		
		public const CHANNELS: Array = [{name: "CHAT_CHANNEL_GLOBAL"}, {name: "CHAT_CHANNEL_TRIBE"}, {name: "CHAT_CHANNEL_HELPDESK"}, {name: "CHAT_CHANNEL_OFFTOPIC"}];
		
		public static const TYPE_GLOBAL:int = 0;
		public static const TYPE_TRIBE:int = 1;
        public static const TYPE_HELPDESK:int = 2;
		public static const TYPE_OFFTOPIC:int = 3;        
		
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
           
            log(TYPE_GLOBAL, Constants.motd, false, false);
            log(TYPE_HELPDESK, Constants.motd_helpdesk, false, false);
			
			new StickyScroll(scrollConsole);
			
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
									
			txtConsole.getTextField().addEventListener(TextEvent.LINK, function(e:TextEvent):void
				{
					var text:String = e.text;
					var parts:Array = text.split(':', 2);
					
					switch (parts[0])
					{
						case 'viewPlayerProfile': 
							Global.mapComm.City.viewPlayerProfile(parts[1]);
							break;
						case 'viewPlayerProfileByName': 
							Global.mapComm.City.viewPlayerProfileByName(parts[1]);
							break;
						case 'viewTribeProfileByName': 
							Global.mapComm.Tribe.viewTribeProfileByName(parts[1]);
							break;
						case 'viewCityProfileByName': 
							Global.mapComm.City.gotoCityLocationByName(parts[1]);
							break;
						case 'gotoStrongholdByName': 
							Global.mapComm.Stronghold.gotoStrongholdLocationByName(parts[1]);
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
					
			if (message.charAt(0) == '/')
			{
				log(currentChatType, message, true);				
                
                CONFIG::debug {
                    if (message == "/togglestacktracer") {
                        Constants.debugStacktracer = true;
                        return true;
                    }
                }
				
				Global.mapComm.General.sendCommand(message.substr(1), function(resp:String, type: int = 0):void
					{
						log(currentChatType, resp, false);
					});
			}
			else
			{
                if (profanityFilter.quickValidate(message) == false)
                {
                    log(currentChatType, 'Looks like your chat message contains some offensive terms. Please keep it classy.', false);
                    return false;
                }
            
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
			
			frame.setSize(new IntDimension(Math.min(550, Constants.screenW * 0.5), Math.min(Math.max(300, Constants.screenH - 300), Constants.screenH * 0.3)));
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
		
        // TODO: Move this into rich label so it can be used in more places
		public function replaceProfileLabel(str: String): String
		{
			var index: int;
			var beginOfs: int;
			var endOfs: int;
			var result: String = "";
			while ((beginOfs = str.indexOf("#", index)) != -1 && (endOfs = str.indexOf("#", beginOfs + 1)) != -1) {
				result += str.substring(index, beginOfs);
				var values: Array =  str.substring(beginOfs + 1, endOfs).split(":", 2);
				if (values.length == 2 && StringUtil.trim(values[1]) !== "") {
					switch(values[0]) {
						case "p":
							result += StringUtil.substitute('<a href="event:viewPlayerProfileByName:{0}"><span class="global">{0}</span></a>', values[1]);
							break;
						case "t":
							result += StringUtil.substitute('<a href="event:viewTribeProfileByName:{0}"><span class="global">{0}</span></a>', values[1]);
							break;
						case "s":
							result += StringUtil.substitute('<a href="event:gotoStrongholdByName:{0}"><span class="global">{0}</span></a>', values[1]);
							break;
						case "c":
							result += StringUtil.substitute('<a href="event:viewCityProfileByName:{0}"><span class="global">{0}</span></a>', values[1]);
							break;
						default:
							result += str.substring(beginOfs, endOfs + 1);
							break;
					}
				} else {
					result += str.substring(beginOfs, endOfs + 1);
				}
				index = endOfs + 1;
			}
			return result + str.substring(index);
		}
		
		public function logChat(type:int, playerId:int, playerName:String, achievements: *, distinguish: Boolean, str:String):void
		{
			var f:DateFormatter = new DateFormatter();
			f.formatString = "LL:NN";
			
			var cssClass:String = '';
			
            if (distinguish) {
                cssClass = 'distinguished';
            }
			else if (playerId == Constants.playerId)
			{
				cssClass = 'self';
			}
			else
			{
				cssClass = 'global';
			}
			
			var i: int;
			var achievementStars: Array = [];
			for (i = 0; i < Math.min(3, Math.ceil(achievements.gold/3.0)); i++) {
				achievementStars.push('<span class="rank-gold">♦</span>');
			}
			for (i = 0; i < Math.min(3 - achievementStars.length, Math.ceil(achievements.silver/3.0)); i++) {
				achievementStars.push('<span class="rank-silver">♦</span>');
			}
			for (i = 0; i < Math.min(3 - achievementStars.length, Math.ceil(achievements.bronze/3.0)); i++) {
				achievementStars.push('<span class="rank-bronze">♦</span>');
			}			
			
			log(type, StringUtil.substitute('[{0}] {5} <a href="event:viewPlayerProfile:{2}"><span class="{1}">{3}</span></a>: {4}', 
									  f.format(new Date()),
									  cssClass,
									  playerId,
									  StringHelper.htmlEscape(playerName),
									  StringHelper.linkify(str),
									  achievementStars.join("")
				), false,  false);			
			
			if (type != currentChatType) {
				var button: JToggleButton = CHANNELS[type].button;
				button.setIcon(new SkinCustomIcon("Frame.chatEnabledIcon"));
			}
		}
		
		public function log(type: int, str:String, isCommand:Boolean = false, escapeStr:Boolean = true):void
		{
			if (str.length == 0)
				return;
						
			if (escapeStr)
				str = StringHelper.htmlEscape(str);
			
			str = replaceProfileLabel(str);
			
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
		
		public function logSystem(messageId:String, params:Array):void 
		{
			if (Global.gameContainer.cmdLine == null) return;
			var substituteArgs: Array = new Array();
			substituteArgs.push('<span class="system">' + StringHelper.localize(messageId) + '</span>');
			
			for each (var str: String in params) {
				substituteArgs.push(StringHelper.htmlEscape(str));
			}
			
			var message: String = StringUtil.substitute.apply(StringUtil, substituteArgs);
			
			Global.gameContainer.cmdLine.log(CmdLineViewer.TYPE_GLOBAL, message, false, false);
			Global.gameContainer.cmdLine.log(CmdLineViewer.TYPE_TRIBE, message, false, false);
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
			var button: JToggleButton = new JToggleButton(StringHelper.localize(CHANNELS[type].name), new SkinCustomIcon("Frame.chatDisabledIcon"));
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
			
			var consoleCss:StyleSheet = new StyleSheet();
			consoleCss.setStyle("p", {marginBottom: '3px', leading: 3, fontFamily: 'Arial', fontSize: 12, color: '#FFFFFF'});
			consoleCss.setStyle("a:link", {fontWeight: 'bold', textDecoration: 'none', color: '#8ecafe'});
			consoleCss.setStyle("a:hover", {textDecoration: 'underline'});
			
			consoleCss.setStyle(".global", { color: '#8ecafe' } );
            consoleCss.setStyle(".distinguished", {color: '#fbd100'});
			consoleCss.setStyle(".self", { color: '#aef64f' } );
			consoleCss.setStyle(".system", { color: '#ec7600', fontWeight: 'bold' } );
			
			consoleCss.setStyle(".rank-bronze", { color: '#B87C37' } );
			consoleCss.setStyle(".rank-silver", { color: '#C9C9B6' } );
			consoleCss.setStyle(".rank-gold", { color: '#fbd100' } );
			
			txtConsole.setCSS(consoleCss);
			
			txtCommand = new JTextField();
			txtCommand.setBackgroundDecorator(null);
			txtCommand.setConstraints("Center");
			txtCommand.setMaxChars(Constants.admin ? 5000 : 450);
			
			var lblCommandCursor:JLabel = new JLabel(">");
			lblCommandCursor.setConstraints("West");
			           
			scrollConsole = new JScrollPane(txtConsole, JScrollPane.SCROLLBAR_ALWAYS, JScrollPane.SCROLLBAR_NEVER);
			scrollConsole.setConstraints("Center");
            
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

