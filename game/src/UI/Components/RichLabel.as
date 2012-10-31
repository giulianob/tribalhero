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
	import src.UI.Tooltips.TextTooltip;
	import src.Util.StringHelper;
	
	public class RichLabel extends MultilineLabel
	{		
		
		private var tooltip: TextTooltip = new TextTooltip("");
		
		public function RichLabel(text:String = "", rows:int = 0, columns:int = 0, showTooltips: Boolean = true)
		{
			/**
			if (rows == 0 && columns == 0) {
				var removeHtml:RegExp = new RegExp(/<\/?\w+((\s+\w+(\s*=\s*(?:".*?"|'.*?'|[^'">\s]+))?)+\s*|\s*)\/?>/ig);
				columns = text.replace(removeHtml, "").length;
			}*/
			
			super("", rows, columns);
			
			var css: StyleSheet = new StyleSheet();
			css.setStyle("a:link", { textDecoration:'underline', fontFamily:'Arial', color:'#0066cc' });
			setCSS(css);
			
			setHtmlText(text);			

			getTextField().addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			getTextField().addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);			
			getTextField().addEventListener(TextEvent.LINK, onClickLink);
		}
		
		private function onMouseOut(e:MouseEvent):void 
		{
			tooltip.hide();
		}
		
		private function onMouseMove(e:MouseEvent):void 
		{
			var idx: int = getTextField().getCharIndexAtPoint(e.localX, e.localY);
			
			if (idx == -1)
			{
				dispatchEvent(new RichLabelCustomEvent(RichLabelCustomEvent.CUSTOM_EVENT_MOUSE_OVER, ""));
				tooltip.hide();
				return;
			}
				
			if (getTextField().htmlText.length == 0) {
				tooltip.hide();
				return;
			}
			
			var textFormat:TextFormat = getTextField().getTextFormat(idx, idx + 1);			
			if (!textFormat.url) {
				tooltip.hide();
				return;
			}
			
			var parts: Array = textFormat.url.split(":");
			
			if (parts[0] == "event") {
				parts.shift();
			}
			
			switch (parts[0]) {
				case 'viewProfile': 
				case 'viewProfileByType': 
				case 'viewTribeProfile':
				case 'viewTribeProfileByName':					
					tooltip.setText(StringHelper.localize("STR_VIEW_PROFILE"));
					tooltip.show(this);
					break;
				case 'goToCity': 
				case 'goToStronghold': 
					tooltip.setText(StringHelper.localize("STR_GOTO"));
					tooltip.show(this);					
					break;
				case 'viewBattle':					
					tooltip.setText(StringHelper.localize("STR_VIEW_BATTLE"));
					tooltip.show(this);					
					break;
				case "custom":
					tooltip.hide();
					dispatchEvent(new RichLabelCustomEvent(RichLabelCustomEvent.CUSTOM_EVENT_MOUSE_OVER, parts[1]));
					break;
				default:
					tooltip.hide();
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
				case 'goToStronghold': 
					Global.mapComm.Stronghold.gotoStrongholdLocation(parts[1]);
					break;											
				case 'viewTribeProfile':
					Global.mapComm.Tribe.viewTribeProfile(int(parts[1]));
					break;
				case 'viewTribeProfileByName':
					Global.mapComm.Tribe.viewTribeProfileByName(parts[1]);
					break;
				case 'viewBattle':
					Global.mapComm.Battle.viewBattle(int(parts[1]));
					break;
				case 'custom':
					dispatchEvent(new RichLabelCustomEvent(RichLabelCustomEvent.CUSTOM_EVENT_CLICK, parts[1]));
					break;
			}			
		}
	}

}