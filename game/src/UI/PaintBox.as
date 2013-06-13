/**
* ...
* @author Default
* @version 0.1
*/

package src.UI {
	
	import flash.display.Bitmap;
	import flash.display.DisplayObject;
	import flash.display.Sprite;
	import flash.events.MouseEvent;
	import flash.text.TextField;
	import flash.text.TextFieldAutoSize;
	import flash.text.TextFieldType;
	import flash.text.TextFormat;
	import flash.text.TextFormatAlign;
	import flash.xml.*;
	import flash.utils.getDefinitionByName;
	import src.UI.Tooltips.TextTooltip;

	import src.Util.StringHelper;
	
	public class PaintBox extends Sprite
	{		
		private var defaultFormat: TextFormat;
		
		private var events: Array = [];
		private var tooltips: Array = [];
		
		public function PaintBox(xml: XML, staticObjects: Array = null, mouseEnabled: Boolean = false) {
			this.mouseEnabled = mouseEnabled;
			this.mouseChildren = mouseEnabled;
			
			defaultFormat = GetTextFormat(xml);		
			
			var width: int = -1;
			
			var spriteHolder: Sprite = new Sprite();
			
			var rowSpace: int = 0;
			var colSpace: int = 0;
			
			if (xml.@width.toString() != '')
				width = xml.@width;					
			
			if (xml.@rowspace.toString() != '')
				rowSpace = xml.@rowspace;
			
			if (xml.@colspace.toString() != '')
				colSpace = xml.@colspace;
			
			var y: int = -rowSpace;
			
			for each (var rowNode: XML in xml.Row)
			{				
				var widthUsed: int = 0;
				var align: String = "left";								
				var colHeight: int = 0;
				var colObjs: Array = [];
				var colVAligns: Array = [];
				
				if (rowNode.@align.toString() != '')
					align = String(rowNode.@align).toLowerCase();													
					
				y += rowSpace;
				
				for each (var colNode: XML in rowNode.Column)
				{
					var colWidth: int = width - widthUsed;
					
					if (width == -1)
						colWidth = -1;
					
					if (colNode.@width.toString() != '')
						colWidth = colNode.@width;
												
					var valign: String = "top";
					if (colNode.@valign.toString() != '')
						valign = String(colNode.@valign).toLowerCase();
						
					var obj: DisplayObject;
					
					var childNode: XML = colNode.children()[0];
					
					switch(childNode.name().toString())
					{
						case 'Text':
							obj = ParseText(colWidth, childNode);
							break;
						case 'Image':
							obj = ParseImage(colWidth, childNode);
							break;
						case 'Div':
							obj = ParseDiv(colWidth, childNode);
							break;
						case 'Object':
							obj = ParseObject(colWidth, childNode);
							break;
						case 'StaticObject':						
							obj = ParseStaticObject(colWidth, childNode, staticObjects);
							break;
						default:
							Util.log("Missing " + childNode.name());
							continue;
							break;
					}
					
					if (childNode.@tooltip.toString() != '')
					{
						var tooltip: String = childNode.@tooltip;
						var txtTooltip: TextTooltip = new TextTooltip(tooltip);
						obj.addEventListener(MouseEvent.MOUSE_OVER, onTooltipShow);
						obj.addEventListener(MouseEvent.MOUSE_OUT, onTooltipHide);
						events.push( { event: MouseEvent.MOUSE_OVER, sprite: obj, callback: onTooltipShow} );
						events.push( { event: MouseEvent.MOUSE_OUT, sprite: obj, callback: onTooltipHide} );
						tooltips.push( { sprite: obj, tooltip: txtTooltip } );
					}
					
					if (colWidth == -1)
						colWidth = obj.width;
											
					if (obj.height > colHeight)
						colHeight = obj.height;
					
					switch(align)
					{
						case "left":
							obj.x = int(widthUsed);
							break;
						case "right":
							obj.x = int(width - widthUsed - colWidth);
							break;
					}
					
					widthUsed += colWidth + colSpace;

					obj.y = y;
					
					spriteHolder.addChild(obj);	
					colObjs.push(obj);
					colVAligns.push(valign);
				}
				
				if (widthUsed > width)
					width = widthUsed;
				
				for (var i: int = 0; i < colObjs.length; i++)
				{
					var colObj: DisplayObject = colObjs[i];
					var colValign: String = colVAligns[i];

					switch(colValign)
					{
						case "top":
							colObj.y = y;
							break;
						case "bottom":
							colObj.y = y + (colHeight - colObj.height);
							break;
						case "middle":
							colObj.y = y + (colHeight/2 - colObj.height/2);
							break;
					}
				}
				
				y += colHeight;
			}
			
			var bgStyle: String = "none";
			if (xml.@bgstyle.toString() != '')
				bgStyle = xml.@bgstyle;
			
			var bgColor: String = "0x000000";
			if (xml.@bgcolor.toString() != '')
				bgColor = xml.@bgcolor;
				
			var bgAlpha: String = "1.0";
			if (xml.@bgalpha.toString() != '')
				bgAlpha = xml.@bgalpha;
				
			var borderColor:String = "0x000000";
			if (xml.@bordercolor.toString() != '')
				borderColor = xml.@bordercolor;
				
			var borderWidth:String = "1";
			if (xml.@borderwidth.toString() != '')
				borderWidth = xml.@borderwidth;				
				
			switch(bgStyle)
			{
				case 'rounded':
					graphics.beginFill(uint(bgColor), Number(bgAlpha));
					graphics.lineStyle(Number(borderWidth), uint(borderColor), 1.0);
					graphics.drawRoundRect(0, 0, (width == -1 ? spriteHolder.width : width) + 5, spriteHolder.height + 5, 18, 18);
					graphics.endFill();
					spriteHolder.x = 2;
					spriteHolder.y = 2;
					break;
				case 'rectangle':
					graphics.beginFill(uint(bgColor), Number(bgAlpha));
					graphics.lineStyle(Number(borderWidth), uint(borderColor), 1.0);
					graphics.drawRect(0, 0, width + 5, spriteHolder.height + 5);
					graphics.endFill();
					spriteHolder.x = 2;
					spriteHolder.y = 2;
					break;
				default:
					break;
			}
			
			addChild(spriteHolder);			
		}
		
		public function dispose():void
		{
			for each(var item: * in events)
				item.sprite.removeEventListener(item.event, item.callback);
			
			for each(item in tooltips)
				item.tooltip.hide();
		}
		
		public function onTooltipShow(event: MouseEvent):void
		{
			for each(var item: * in tooltips)
			{
				if (event.target == item.sprite)
				{
					item.tooltip.show(stage, item.sprite);
					break;
				}				
			}
		}
		
		public function onTooltipHide(event: MouseEvent):void
		{
			for each(var item: * in tooltips)
			{
				if (event.target == item.sprite)
				{
					item.tooltip.hide();
					break;
				}				
			}
		}
		
		private function ParseStaticObject(width: int, xml: XML, staticObjects: Array): DisplayObject
		{
			if (staticObjects == null)
				return new Sprite();
				
			var obj: * = null;
			
			try
			{
				obj = staticObjects[xml.@index];
			}
			catch (error: Error)
			{
				return new Sprite();
			}
			
			var p: Sprite = new Sprite();
			
			var align: String = "left";
			if (xml.@align.toString() != '')
				align = xml.@align;
				
			if (width == -1)
				width = obj.width;
						
			switch(align)
			{
				case 'left':
					obj.x = 0;					
					break;
				case 'right':
					obj.x = width - obj.width;
					break;
				case 'center':
					obj.x = int(width/2) - int(obj.width / 2);
					break;
				default:
					obj.x = 0;
					break;			
			}
			
			return obj as DisplayObject;
			
			p.addChild(obj);
			
			return p as DisplayObject;			
		}
		
		private function ParseText(width: int, xml: XML): TextField
		{			
			var textField: TextField = new TextField();
			
			textField.text = StringHelper.trim(xml.toString());
			textField.mouseEnabled = true;
			textField.setTextFormat(GetTextFormat(xml, defaultFormat));
			textField.wordWrap = true;
			textField.type = TextFieldType.DYNAMIC;
			textField.selectable = false;			
			textField.autoSize = TextFieldAutoSize.LEFT;
			
			if (width > -1)
				textField.width = width;
			else
				textField.wordWrap = false;									
			
			return textField;
		}
		
		private function ParseDiv(width: int, xml: XML): Sprite
		{
			var p: Sprite = new Sprite();
		
			p.visible = false;
			p.mouseEnabled = false;
			p.graphics.beginFill(0xFFFFFF);
            p.graphics.lineStyle(1);
			p.graphics.drawRect(0, 0, width, xml.@height);
			p.graphics.endFill();
			p.mouseEnabled = true;

			return p;
		}		
		
		private function ParseObject(width: int, xml: XML): DisplayObject
		{
			var obj: * = ParseImage(width, xml);
			
			if (xml.@param5.toString() != '')
				obj.init(xml.@param1, xml.@param2, xml.@param3, xml.@param4, xml.@param5);
			else if (xml.@param4.toString() != '')
				obj.init(xml.@param1, xml.@param2, xml.@param3, xml.@param4);
			else if (xml.@param3.toString() != '')
				obj.init(xml.@param1, xml.@param2, xml.@param3);
			else if (xml.@param2.toString() != '')
				obj.init(xml.@param1, xml.@param2);
			else if (xml.@param1.toString() != '')
				obj.init(xml.@param1);
		
			return obj as DisplayObject;
		}
			
		private function ParseImage(width: int, xml: XML): DisplayObject
		{	
			var objRef:Class;					
			
			try 
			{
				objRef = getDefinitionByName(xml.toString()) as Class;
			}
			catch (error: Error)
			{
				return new Sprite();
			}		
			
			var obj:DisplayObject = new objRef() as DisplayObject;			
			
			var p: Sprite = new Sprite();			
			p.mouseEnabled = false;
			p.mouseChildren = false;
			
			var align: String = "left";
			if (xml.@align.toString() != '')
				align = xml.@align;
				
			if (width == -1)
				width = obj.width;
				
			switch(align)
			{
				case 'left':
					obj.x = 0;
					break;
				case 'right':
					obj.x = width - obj.width;
					break;
				case 'center':
					obj.x = int(width/2) - int(obj.width / 2);
					break;
				default:
					obj.x = 0;
					break;			
			}
			
			p.addChild(obj);
			
			p.mouseEnabled = true;
			
			return p as DisplayObject;
		}
		
		private function GetTextFormat(xml: XML, copyFormat: TextFormat = null): TextFormat
		{
			var textFormat: TextFormat = new TextFormat();
			
			if (copyFormat)			
				textFormat = new TextFormat(copyFormat.font, copyFormat.size, copyFormat.color, copyFormat.bold, copyFormat.italic, copyFormat.underline);
				
			if (xml.@font.toString() != '')
				textFormat.font = xml.@size;
				
			if (xml.@size.toString() != '')
				textFormat.size = xml.@size;
				
			if (xml.@align.toString() != '')
				textFormat.align = xml.@align;			
				
			if (xml.@bold.toString() != '')
				textFormat.bold = Boolean(xml.@bold);
				
			if (xml.@color.toString() != '')
				textFormat.color = xml.@color;
				
			if (xml.@italic.toString() != '')
				textFormat.italic = Boolean(xml.@italic);
				
			return textFormat;
		}
	}
	
}
