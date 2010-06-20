package src.UI 
{
	import flash.filters.DropShadowFilter;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.plaf.ArrayUIResource;
	import org.aswing.plaf.ASColorUIResource;
	import org.aswing.plaf.ASFontUIResource;
	import org.aswing.plaf.basic.background.ButtonBackground;
	import org.aswing.plaf.basic.background.ToggleButtonBackground;
	import org.aswing.plaf.basic.BasicLookAndFeel;
	import org.aswing.plaf.ComponentUI;
	import org.aswing.plaf.InsetsUIResource;
	import org.aswing.plaf.UIStyleTune;

	public class GameLookAndFeel extends BasicLookAndFeel
	{		
		public function GameLookAndFeel() 
		{
			super();
		}
		
		public static function changeClass(obj: Component, classes: String) : void {
			var ui: ComponentUI = obj.getUI();
			
			var keys: Array = classes.split(" ");
			
			for each(var key: String in keys) {
				if (key == "") continue;
				
				var keyValueList: Array = UIManager.getDefaults().get("Class." + key);
				for(var i:Number = 0; i < keyValueList.length; i += 2)
					ui.putDefault(keyValueList[i], keyValueList[i + 1]);							
			}
			
			obj.setUI(ui);				
		}
		
		public static function getClassAttribute(className: String, attribute: String) : * {
			var keyValueList: Array = UIManager.getDefaults().get("Class." + className);
			
			for(var i:Number = 0; i < keyValueList.length; i += 2) {
				if (keyValueList[i] == attribute)
					return keyValueList[i + 1];
			}
				
			return null;
		}
		
		override protected function initComponentDefaults(table:UIDefaults):void{
			super.initComponentDefaults(table);
			var comDefaults:Array = [	
			
				/* DEFAULT COMPONENTS */				
				"Frame.foreground", new ASColorUIResource(0x666666),
				"Frame.background", new ASColorUIResource(0xE6E6E6),
				"Frame.mideground", new ASColorUIResource(0x3e9cd5),
				"Frame.colorAdjust", new UIStyleTune(0.1, 0.0, 0.0, 0.3, 10, new UIStyleTune(0.04, -0.06, 1, 0.22, 5)),
				
				"FrameTitleBar.foreground", new ASColorUIResource(0xffffff),
				"FrameTitleBar.mideground", new ASColorUIResource(0xe0e0e0),
				"FrameTitleBar.background", new ASColorUIResource(0xC4E066),
				"FrameTitleBar.colorAdjust", new UIStyleTune(0.0, 0.0, 0.0, 0.0, 0, new UIStyleTune(0.04, -0.06, 1, 0.22, 5)),
				
				"Label.font", new ASFontUIResource("Arial", 11),
				
				"TextArea.font", new ASFontUIResource("Arial", 11),			
				
				"LabelButton.font", new ASFontUIResource("Arial", 12, false, false, true),
				
				"Button.background", new ASColorUIResource(0xe0e0e0),
				"Button.foreground", new ASColorUIResource(0x333333),
				"Button.mideground", table.get("controlMide"),
				"Button.colorAdjust", new UIStyleTune(0.04, -0.06, 1, 0.22, 5), 
				"Button.opaque", true,  
				"Button.focusable", true,  
				"Button.font", new ASFontUIResource("Arial", 13, false),
				"Button.bg", ButtonBackground,
				"Button.margin", new InsetsUIResource(2, 3, 5, 3), 
				"Button.textShiftOffset", 0, 
				"Button.textFilters", new ArrayUIResource([]),				
				
				"ToggleButton.background", new ASColorUIResource(0xe0e0e0),
				"ToggleButton.foreground", new ASColorUIResource(0x333333), 
				"ToggleButton.mideground", table.get("controlMide"), 
				"ToggleButton.colorAdjust", new UIStyleTune(0.04, -0.06, 1, 0.22, 5), 
				"ToggleButton.opaque", true, 
				"ToggleButton.focusable", true, 
				"ToggleButton.font", new ASFontUIResource("Arial", 13, false),
				"ToggleButton.bg", ToggleButtonBackground,
				"ToggleButton.margin", new InsetsUIResource(2, 3, 5, 3), 
				"ToggleButton.textShiftOffset", 1, 
				"ToggleButton.textFilters", new ArrayUIResource([]),
				
				/* CLASSES */				
				"Class.header", [
					"Label.font", new ASFontUIResource("Arial", 13, true),
					"Label.foreground", new ASColorUIResource(0xFFFFFF)
				],
				
				"Class.darkHeader", [
					"Label.font", new ASFontUIResource("Arial", 13, true),
					"Label.foreground", new ASColorUIResource(0x000000)
				],				
				
				"Class.Tooltip.text", [
					"Label.font", new ASFontUIResource("Arial", 13, false),
					"Label.foreground", new ASColorUIResource(0xFFFFFF),
					"TextArea.font", new ASFontUIResource("Arial", 13, false),
					"TextArea.foreground", new ASColorUIResource(0xFFFFFF)
				],
				
				"Class.Message.preview", [
					"Textfield.foreground", new ASColorUIResource(0x707070)
				],				
				"Class.Message.unread", [
					"Textfield.font", new ASFontUIResource("Arial", 12, true),
					"Textfield.foreground", new ASColorUIResource(0x000000)
				],			
				"Class.Message.read", [
					"Textfield.font", new ASFontUIResource("Arial", 12, false),
					"Textfield.foreground", new ASColorUIResource(0x000000)
				],					
				
				"Class.Label.small", [
					"Label.font", new ASFontUIResource("Arial", 11, false),					
					"TextArea.font", new ASFontUIResource("Arial", 11, false),					
				],								
				"Class.Label.error", [
					"Label.font", new ASFontUIResource("Arial", 13, false),
					"Label.foreground", new ASColorUIResource(0xFF0000)
				],			
				"Class.Label.success", [
					"Label.font", new ASFontUIResource("Arial", 13, false),
					"Label.foreground", new ASColorUIResource(0x00FF00)
				],			
				
				"Class.Form.label", [
					"Label.font", new ASFontUIResource("Arial", 11, true)
				],
				
				"Class.Button.action", [
					"Button.background", new ASColorUIResource(0xe0e0e0),
					"Button.foreground", new ASColorUIResource(0x333333),
					"Button.mideground", table.get("controlMide"),
					"Button.colorAdjust", new UIStyleTune(0, -0.06, 1, 0.22, 5), 
					"Button.opaque", true,  
					"Button.focusable", true,  
					"Button.font", new ASFontUIResource("Arial", 11, false),
					"Button.bg", ButtonBackground,
					"Button.margin", new InsetsUIResource(2, 3, 2, 3), 
					"Button.textShiftOffset", 0, 
					"Button.textFilters", new ArrayUIResource([])
				]
			];
			
			table.putDefaults(comDefaults);
		}		
		
	}

}