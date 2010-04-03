package src.UI 
{
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.plaf.ASColorUIResource;
	import org.aswing.plaf.ASFontUIResource;
	import org.aswing.plaf.basic.BasicLookAndFeel;
	import org.aswing.plaf.ComponentUI;
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
				"Frame.mideground", new ASColorUIResource(0xB5D350),
				"Frame.colorAdjust", new UIStyleTune(0.1, 0.0, 0.0, 0.3, 10, new UIStyleTune(0.1, 0.0, 0.0, 0.6, 10)), 
				
				"FrameTitleBar.foreground", new ASColorUIResource(0x3A261B),
				"FrameTitleBar.mideground", new ASColorUIResource(0xBEDB5D),
				"FrameTitleBar.background", new ASColorUIResource(0xC4E066),
				"FrameTitleBar.colorAdjust", new UIStyleTune(0.0, 0.0, 0.0, 0.0, 0, new UIStyleTune(0.2, -0.3, 0.08, 0.1, 1)), 
				
				"Label.font", new ASFontUIResource("Arial", 11),
				
				"TextArea.font", new ASFontUIResource("Arial", 11),			
				
				"LabelButton.font", new ASFontUIResource("Arial", 12, false, false, true),
				
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
			];
			
			table.putDefaults(comDefaults);
		}		
		
	}

}