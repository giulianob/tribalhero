package src.UI.Components
{
	import flash.events.MouseEvent;
	import flash.events.TextEvent;
	import flash.text.StyleSheet;
	import flash.text.TextFormat;
	import org.aswing.ext.MultilineLabel;
	import org.aswing.plaf.ASColorUIResource;
	import src.Constants;
	import src.Global;
	import src.UI.LookAndFeel.GameLookAndFeel;
	
	public class RichLabel extends MultilineLabel
	{		
		public function RichLabel(text:String = "", rows:int = 0, columns:int = 0)
		{
			super("", rows, columns);
			
			var css: StyleSheet = new StyleSheet();
			css.setStyle("a:link", { textDecoration:'underline', fontFamily:'Arial', color:'#0066cc' });
			setCSS(css);
			
			setHtmlText(text);

			getTextField().addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			
			getTextField().addEventListener(TextEvent.LINK, onClickLink);
		}
		
		private function onMouseMove(e:MouseEvent):void 
		{
			var idx: int = getTextField().getCharIndexAtPoint(e.localX, e.localY);
			
			if (idx == -1)
			{
				dispatchEvent(new RichLabelCustomEvent(RichLabelCustomEvent.CUSTOM_EVENT_MOUSE_OVER, ""));
				return;
			}
				
			var textFormat:TextFormat = getTextField().getTextFormat(idx, idx + 1);
			if (textFormat.url && textFormat.url.substr(0, 12) == "event:custom")
			{
				var parts: Array = textFormat.url.split(':', 3);
				dispatchEvent(new RichLabelCustomEvent(RichLabelCustomEvent.CUSTOM_EVENT_MOUSE_OVER, parts[2]));
			}
		}
		
		private function onClickLink(e:TextEvent):void 
		{
			var text:String = e.text;
			var parts:Array = text.split(':');
			
			switch (parts[0])
			{
				case 'viewProfile': 
					Global.mapComm.City.viewPlayerProfile(parts[1]);
					break;
				case 'viewProfileByType': 
					Global.mapComm.General.viewProfileByType(parts[1], parts[2]);
					break;					
				case 'goToCity': 
					Global.mapComm.City.gotoCityLocation(parts[1]);
					break;		
				case 'viewTribeProfile':
					var tribeId: int = int(parts[1]);
					if (Constants.tribeId == tribeId)
					{
						Global.mapComm.Tribe.viewTribeProfile();
					}
					else
					{
						Global.mapComm.Tribe.viewTribePublicProfile(tribeId);
					}
					break;
				case 'custom':
					dispatchEvent(new RichLabelCustomEvent(RichLabelCustomEvent.CUSTOM_EVENT_CLICK, parts[1]));
					break;
			}			
		}
	}

}