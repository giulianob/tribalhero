package src.UI.Components
{
	import flash.events.MouseEvent;
	import flash.events.TextEvent;
	import flash.text.StyleSheet;
	import flash.text.TextFormat;
	import mx.utils.StringUtil;
	import org.aswing.AsWingManager;
	import org.aswing.ext.MultilineLabel;
	import org.aswing.plaf.ASColorUIResource;
	import src.Constants;
	import src.Global;
	import src.Objects.Location;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.UI.Tooltips.TextTooltip;
	import src.Util.StringHelper;
	
	public class RichLabel extends MultilineLabel
	{		
		
		private var usingCustomSize: Boolean = false;
		
		private var tooltip: TextTooltip = new TextTooltip("");
		
		public function RichLabel(text:String = "", rows:int = 0, columns:int = 0, showTooltips: Boolean = true)
		{									
			usingCustomSize = rows == 1 && columns == 0;
			
			super("", rows, columns);				
			
			var css: StyleSheet = new StyleSheet();
			css.setStyle("a:link", { textDecoration:'underline', fontFamily:'Arial', color:'#0066cc' });
			setCSS(css);
			
			setHtmlText(text);			

			getTextField().addEventListener(MouseEvent.MOUSE_MOVE, onMouseMove);
			getTextField().addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);			
			// We cannot use the Link event here because when a tooltip shows for some reason
			// the link event isnt fired when the user clicks it
			getTextField().addEventListener(MouseEvent.CLICK, onClickLink);
		}	
		
		override public function setHtmlText(ht:String):void 
		{
			if (usingCustomSize) {
				var removeHtml:RegExp = new RegExp("<\\\/?\\w+((\\s+\\w+(\\s*=\\s*(?:\".*?\"|'.*?'|[^'\">\\s]+))?)+\\s*|\\s*)\\\/?>", "ig");
				setColumns(ht.replace(removeHtml, "").length + 1);
			}
			
			super.setHtmlText(ht);
		}
		
		private function onMouseOut(e:MouseEvent):void 
		{
			tooltip.hide();
		}
		
		private function onMouseMove(e:MouseEvent):void 
		{				
			var parts: Array = getLinkParts(e);
			
			if (parts == null) {
				dispatchEvent(new RichLabelCustomEvent(RichLabelCustomEvent.CUSTOM_EVENT_MOUSE_OVER, ""));
				tooltip.hide();
				return;
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
		
		private function getLinkParts(e:MouseEvent):Array
		{
			var idx: int = getTextField().getCharIndexAtPoint(e.localX, e.localY);
			
			if (idx == -1)
			{				
				return null;
			}
			
			if (getTextField().htmlText.length == 0) {
				return null;
			}
			
			var textFormat:TextFormat = getTextField().getTextFormat(idx, idx + 1);			
			if (!textFormat.url) {
				return null;
			}
			
			var parts: Array = textFormat.url.split(":");
			
			if (parts[0] == "event") {
				parts.shift();
			}
			
			return parts;
		}
		
		private function onClickLink(e:MouseEvent):void 
		{
			var parts:Array = getLinkParts(e);
			
			if (parts == null) {
				return;
			}
			
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
		
		public static function getHtmlForLocation(location: *) : String {
			switch(location.type)
			{
				case Location.CITY:
					return StringHelper.localize("RICH_LABEL_LOCATION_CITY", location.cityId, location.cityName, location.playerId, location.playerName);
                case Location.BARBARIAN_TRIBE:
                    return StringHelper.localize("RICH_LABEL_BARBARIAN_TRIBE");
				case Location.STRONGHOLD:
					return StringHelper.localize(location.tribeId == 0 ? "RICH_LABEL_LOCATION_STRONGHOLD_NEUTRAL" : "RICH_LABEL_LOCATION_STRONGHOLD", location.strongholdId, location.strongholdName, location.tribeId, location.tribeName);
				default:
					return "Bad Location";
			}
		}
	}

}