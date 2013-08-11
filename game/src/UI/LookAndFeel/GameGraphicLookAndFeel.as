package src.UI.LookAndFeel
{

    import flash.filters.DropShadowFilter;

    import org.aswing.*;
    import org.aswing.plaf.*;
    import org.aswing.plaf.basic.BasicLookAndFeel;
    import org.aswing.skinbuilder.*;

    /**
	 * Holds only definition for graphics. Colors and classes are held in the other file.
	 */
	public class GameGraphicLookAndFeel extends BasicLookAndFeel{

		public function GameGraphicLookAndFeel(){
			super();
		}

		override protected function initClassDefaults(table:UIDefaults):void{
			super.initClassDefaults(table);
			var uiDefaults:Array = [
			"ScrollBarUI", SkinScrollBarUI,
			"ProgressBarUI", SkinProgressBarUI,
			"ComboBoxUI", SkinComboBoxUI,
			"SliderUI", SkinSliderUI,
			"AdjusterUI", SkinAdjusterUI,
			"TabbedPaneUI", SkinTabbedPaneUI,
			"ClosableTabbedPaneUI", SkinClosableTabbedPaneUI,
			"SplitPaneUI", GameSplitPaneUI
			];
			table.putDefaults(uiDefaults);
		}

		override protected function initSystemColorDefaults(table:UIDefaults):void{
			super.initSystemColorDefaults(table);
			var defaultSystemColors:Array = [
			"activeCaption", 0xF2F2F2, /* Color for captions (title bars) when they are active. */
			"activeCaptionText", 0x212121, /* Text color for text in captions (title bars). */
			"activeCaptionBorder", 0xC0C0C0, /* Border color for caption (title bar) window borders. */
			"inactiveCaption", 0xF2F2F2, /* Color for captions (title bars) when not active. */
			"inactiveCaptionText", 0x515151, /* Text color for text in inactive captions (title bars). */
			"inactiveCaptionBorder", 0x888888, /* Border color for inactive caption (title bar) window borders. */
			"window", 0xEFEFEF, /* Default color for the interior of windows */
			"windowBorder", 0x000000, /* ??? */
			"windowText", 0xffffff, /* ??? */
			"menu", 0xCCCCCC, /* Background color for menus */
			"menuText", 0x000000, /* Text color for menus  */
			"text", 0xC0C0C0, /* Text background color */
			"textText", 0x000000, /* Text foreground color */
			"textHighlight", 0x000080, /* Text background color when selected */
			"textHighlightText", 0xFFFFFF, /* Text color when selected */
			"textInactiveText", 0x808080, /* Text color when disabled */
			"selectionBackground", 0xbcbcbc, //0x316AC5,
			"selectionForeground", 0x000000,
			"control", 0xF4F4F4,//0xEFEFEF, /* Default color for controls (buttons, sliders, etc) */
			"controlText", 0x002a37, /* Default color for text in controls */
			"controlHighlight", 0xEEEEEE, /* Specular highlight (opposite of the shadow) */
			"controlLtHighlight", 0x666666, /* Highlight color for controls */
			"controlShadow", 0xC7C7C5, /* Shadow color for controls */
			"controlDkShadow", 0x666666, /* Dark shadow color for controls */
			"scrollbar", 0xE0E0E0 /* Scrollbar background (usually the "track") */
			];

			for(var i:Number=0; i<defaultSystemColors.length; i+=2){
				table.put(defaultSystemColors[i], new ASColorUIResource(defaultSystemColors[i+1]));
			}
			table.put("focusInner", new ASColorUIResource(0x40FF40, 0.2));
			table.put("focusOutter", new ASColorUIResource(0x40FF40, 0.4));
		}

		override protected function initSystemFontDefaults(table:UIDefaults):void{
			super.initSystemFontDefaults(table);
			var defaultSystemFonts:Array = [
			"systemFont", new ASFontUIResource("Arial", 11),
			"menuFont", new ASFontUIResource("Arial", 11),
			"controlFont", new ASFontUIResource("Arial", 11),
			"windowFont", new ASFontUIResource("Arial", 11, true)
			];
			table.putDefaults(defaultSystemFonts);
		}

		override protected function initCommonUtils(table:UIDefaults):void{
			super.initCommonUtils(table);

			var arrowColors:Array = [
			"resizeArrow", new ASColorUIResource(0xF2F2F2, 0),
			"resizeArrowLight", new ASColorUIResource(0xCCCCCC, 0),
			"resizeArrowDark", new ASColorUIResource(0x000000, 1)
			];
			table.putDefaults(arrowColors);

			var cursors:Array = [
			"System.hResizeCursor", System_hResizeCursor,
			"System.vResizeCursor", System_vResizeCursor,
			"System.hMoveCursor", System_hMoveCursor,
			"System.vMoveCursor", System_vMoveCursor
			];
			table.putDefaults(cursors);
		}

		//----------------------------------------------------------------------
		//___________________________ Gamebar ___________________________________
		//======================================================================
		[Embed(source="../../../../graphics/ui/Game-bar.png", scaleGridTop="5", scaleGridBottom="40",
		scaleGridLeft="190", scaleGridRight="675")]
		private var GameContainer_gameBar:Class;

		//----------------------------------------------------------------------
		//___________________________ System ___________________________________
		//======================================================================
		[Embed(source="../../../../graphics/ui/System_hResizeCursor.png")]
		private var System_hResizeCursor:Class;

		[Embed(source="../../../../graphics/ui/System_vResizeCursor.png")]
		private var System_vResizeCursor:Class;

		[Embed(source="../../../../graphics/ui/System_hMoveCursor.png")]
		private var System_hMoveCursor:Class;

		[Embed(source="../../../../graphics/ui/System_vMoveCursor.png")]
		private var System_vMoveCursor:Class;

		//----------------------------------------------------------------------
		//___________________________ Button scale-9 ___________________________
		//======================================================================
		[Embed(source="../../../../graphics/ui/Button_defaultImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var Button_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Button_pressedImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var Button_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Button_rolloverImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var Button_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/Button_disabledImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var Button_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Button_DefaultButton_defaultImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var Button_DefaultButton_defaultImage:Class;

		//----------------------------------------------------------------------------
		//___________________________ ToggleButton scale-9 ___________________________
		//============================================================================
		[Embed(source="../../../../graphics/ui/ToggleButton_defaultImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var ToggleButton_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ToggleButton_pressedImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var ToggleButton_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/ToggleButton_disabledImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var ToggleButton_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/ToggleButton_selectedImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var ToggleButton_selectedImage:Class;

		[Embed(source="../../../../graphics/ui/ToggleButton_disabledSelectedImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var ToggleButton_disabledSelectedImage:Class;

		[Embed(source="../../../../graphics/ui/ToggleButton_rolloverImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var ToggleButton_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/ToggleButton_rolloverSelectedImage.png", scaleGridTop="7", scaleGridBottom="19",
		scaleGridLeft="7", scaleGridRight="124")]
		private var ToggleButton_rolloverSelectedImage:Class;

		//-------------------------------------------------------------------
		//___________________________ RadioButton ___________________________
		//===================================================================
		[Embed(source="../../../../graphics/ui/RadioButton_defaultImage.png")]
		private var RadioButton_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/RadioButton_pressedImage.png")]
		private var RadioButton_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/RadioButton_pressedSelectedImage.png")]
		private var RadioButton_pressedSelectedImage:Class;

		[Embed(source="../../../../graphics/ui/RadioButton_disabledImage.png")]
		private var RadioButton_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/RadioButton_selectedImage.png")]
		private var RadioButton_selectedImage:Class;

		[Embed(source="../../../../graphics/ui/RadioButton_disabledSelectedImage.png")]
		private var RadioButton_disabledSelectedImage:Class;

		[Embed(source="../../../../graphics/ui/RadioButton_rolloverImage.png")]
		private var RadioButton_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/RadioButton_rolloverSelectedImage.png")]
		private var RadioButton_rolloverSelectedImage:Class;

		//----------------------------------------------------------------
		//___________________________ CheckBox ___________________________
		//================================================================
		[Embed(source="../../../../graphics/ui/CheckBox_defaultImage.png")]
		private var CheckBox_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/CheckBox_pressedImage.png")]
		private var CheckBox_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/CheckBox_pressedSelectedImage.png")]
		private var CheckBox_pressedSelectedImage:Class;

		[Embed(source="../../../../graphics/ui/CheckBox_disabledImage.png")]
		private var CheckBox_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/CheckBox_selectedImage.png")]
		private var CheckBox_selectedImage:Class;

		[Embed(source="../../../../graphics/ui/CheckBox_disabledSelectedImage.png")]
		private var CheckBox_disabledSelectedImage:Class;

		[Embed(source="../../../../graphics/ui/CheckBox_rolloverImage.png")]
		private var CheckBox_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/CheckBox_rolloverSelectedImage.png")]
		private var CheckBox_rolloverSelectedImage:Class;

		//------------------------------------------------------------------
		//___________________________ ScrollBar ____________________________
		//==================================================================
		//========= Left Arrow Images =======
		[Embed(source="../../../../graphics/ui/ScrollBar_arrowLeft_defaultImage.png")]
		private var ScrollBar_arrowLeft_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowLeft_pressedImage.png")]
		private var ScrollBar_arrowLeft_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowLeft_disabledImage.png")]
		private var ScrollBar_arrowLeft_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowLeft_rolloverImage.png")]
		private var ScrollBar_arrowLeft_rolloverImage:Class;

		//========= Right Arrow Images =======
		[Embed(source="../../../../graphics/ui/ScrollBar_arrowRight_defaultImage.png")]
		private var ScrollBar_arrowRight_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowRight_pressedImage.png")]
		private var ScrollBar_arrowRight_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowRight_disabledImage.png")]
		private var ScrollBar_arrowRight_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowRight_rolloverImage.png")]
		private var ScrollBar_arrowRight_rolloverImage:Class;

		//========= Up Arrow Images =======
		[Embed(source="../../../../graphics/ui/ScrollBar_arrowUp_defaultImage.png")]
		private var ScrollBar_arrowUp_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowUp_pressedImage.png")]
		private var ScrollBar_arrowUp_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowUp_disabledImage.png")]
		private var ScrollBar_arrowUp_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowUp_rolloverImage.png")]
		private var ScrollBar_arrowUp_rolloverImage:Class;

		//========= Down Arrow Images =======
		[Embed(source="../../../../graphics/ui/ScrollBar_arrowDown_defaultImage.png")]
		private var ScrollBar_arrowDown_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowDown_pressedImage.png")]
		private var ScrollBar_arrowDown_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowDown_disabledImage.png")]
		private var ScrollBar_arrowDown_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_arrowDown_rolloverImage.png")]
		private var ScrollBar_arrowDown_rolloverImage:Class;

		//========= Background Images scale-9 =======
		[Embed(source="../../../../graphics/ui/ScrollBar_verticalBGImage.png", scaleGridTop="3", scaleGridBottom="158",
		scaleGridLeft="7", scaleGridRight="9")]
		private var ScrollBar_verticalBGImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_horizotalBGImage.png", scaleGridTop="7", scaleGridBottom="9",
		scaleGridLeft="3", scaleGridRight="158")]
		private var ScrollBar_horizotalBGImage:Class;
		//by default the disabled bg state is null(means same to normal state), but you can add it by remove the comments
		//[Embed(source="../../../../graphics/ui/ScrollBar_verticalBGDisabledImage.png", scaleGridTop="10", scaleGridBottom="223",
		//	scaleGridLeft="7", scaleGridRight="9")]
		private var ScrollBar_verticalBGDisabledImage:Class;

		//[Embed(source="../../../../graphics/ui/ScrollBar_horizotalBGDisabledImage.png", scaleGridTop="7", scaleGridBottom="9",
		//	scaleGridLeft="10", scaleGridRight="223")]
		private var ScrollBar_horizotalBGDisabledImage:Class;

		//========= Thumb Images scale-9 =======
		//vertical
		[Embed(source="../../../../graphics/ui/ScrollBar_thumbVertical_defaultImage.png", scaleGridTop="3", scaleGridBottom="122",
		scaleGridLeft="3", scaleGridRight="16")]
		private var ScrollBar_thumbVertical_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_thumbVertical_pressedImage.png", scaleGridTop="3", scaleGridBottom="122",
		scaleGridLeft="3", scaleGridRight="16")]
		private var ScrollBar_thumbVertical_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_thumbVertical_rolloverImage.png", scaleGridTop="3", scaleGridBottom="122",
		scaleGridLeft="3", scaleGridRight="16")]
		private var ScrollBar_thumbVertical_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_thumbVertical_disabledImage.png", scaleGridTop="3", scaleGridBottom="122",
		scaleGridLeft="3", scaleGridRight="16")]
		private var ScrollBar_thumbVertical_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_thumbVertical_iconImage.png")]
		private var ScrollBar_thumbVertical_iconImage:Class;
		[Embed(source="../../../../graphics/ui/ScrollBar_thumbVertical_iconPressedImage.png")]
		private var ScrollBar_thumbVertical_iconPressedImage:Class;
		[Embed(source="../../../../graphics/ui/ScrollBar_thumbVertical_iconDisabledImage.png")]
		private var ScrollBar_thumbVertical_iconDisabledImage:Class;
		[Embed(source="../../../../graphics/ui/ScrollBar_thumbVertical_iconRolloverImage.png")]
		private var ScrollBar_thumbVertical_iconRolloverImage:Class;

		//horizontal
		[Embed(source="../../../../graphics/ui/ScrollBar_thumbHorizontal_defaultImage.png", scaleGridTop="3", scaleGridBottom="16",
		scaleGridLeft="3", scaleGridRight="122")]
		private var ScrollBar_thumbHorizontal_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_thumbHorizontal_pressedImage.png", scaleGridTop="3", scaleGridBottom="16",
		scaleGridLeft="3", scaleGridRight="122")]
		private var ScrollBar_thumbHorizontal_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_thumbHorizontal_rolloverImage.png", scaleGridTop="3", scaleGridBottom="16",
		scaleGridLeft="3", scaleGridRight="122")]
		private var ScrollBar_thumbHorizontal_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_thumbHorizontal_disabledImage.png", scaleGridTop="3", scaleGridBottom="16",
		scaleGridLeft="3", scaleGridRight="122")]
		private var ScrollBar_thumbHorizontal_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/ScrollBar_thumbHorizontal_iconImage.png")]
		private var ScrollBar_thumbHorizontal_iconImage:Class;
		[Embed(source="../../../../graphics/ui/ScrollBar_thumbHorizontal_iconPressedImage.png")]
		private var ScrollBar_thumbHorizontal_iconPressedImage:Class;
		[Embed(source="../../../../graphics/ui/ScrollBar_thumbHorizontal_iconDisabledImage.png")]
		private var ScrollBar_thumbHorizontal_iconDisabledImage:Class;
		[Embed(source="../../../../graphics/ui/ScrollBar_thumbHorizontal_iconRolloverImage.png")]
		private var ScrollBar_thumbHorizontal_iconRolloverImage:Class;

		//---------------------------------------------------------------------
		//___________________________ TextField scale-9 _______________________
		//=====================================================================
		[Embed(source="../../../../graphics/ui/TextField_defaultImage.png", scaleGridTop="4", scaleGridBottom="20",
		scaleGridLeft="4", scaleGridRight="87")]
		private var TextField_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TextField_uneditableImage.png", scaleGridTop="4", scaleGridBottom="20",
		scaleGridLeft="4", scaleGridRight="87")]
		private var TextField_uneditableImage:Class;

		[Embed(source="../../../../graphics/ui/TextField_disabledImage.png", scaleGridTop="4", scaleGridBottom="20",
		scaleGridLeft="4", scaleGridRight="87")]
		private var TextField_disabledImage:Class;

		//------------------------------------------------------------------------
		//___________________________ TextArea scale-9 ___________________________
		//========================================================================
		[Embed(source="../../../../graphics/ui/TextArea_defaultImage.png", scaleGridTop="4", scaleGridBottom="21",
		scaleGridLeft="4", scaleGridRight="126")]
		private var TextArea_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TextArea_uneditableImage.png", scaleGridTop="4", scaleGridBottom="21",
		scaleGridLeft="4", scaleGridRight="126")]
		private var TextArea_uneditableImage:Class;

		[Embed(source="../../../../graphics/ui/TextArea_disabledImage.png", scaleGridTop="4", scaleGridBottom="21",
		scaleGridLeft="4", scaleGridRight="126")]
		private var TextArea_disabledImage:Class;

		//--------------------------------------------------------------
		//___________________________ Sidebar BG ____________________________
		//==============================================================
		//Backgorund scale-9 (Include title bar background all in one picture)
		[Embed(source="../../../../graphics/ui/Sidebar_frame_activeBG.png", scaleGridTop="45", scaleGridBottom="374",
		scaleGridLeft="29", scaleGridRight="146")]
		private var Sidebar_frame_activeBG:Class;

		//--------------------------------------------------------------
		//___________________________ Frame ____________________________
		//==============================================================
		//Backgorund scale-9 (Include title bar background all in one picture)
		[Embed(source="../../../../graphics/ui/Frame_activeBG.png", scaleGridTop="66", scaleGridBottom="350",
		scaleGridLeft="128", scaleGridRight="668")]
		private var Frame_activeBG:Class;

		//========= Frame_iconifiedIcon Images =======
		[Embed(source="../../../../graphics/ui/Frame_iconifiedIcon_defaultImage.png")]
		private var Frame_iconifiedIcon_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_iconifiedIcon_pressedImage.png")]
		private var Frame_iconifiedIcon_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_iconifiedIcon_disabledImage.png")]
		private var Frame_iconifiedIcon_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_iconifiedIcon_rolloverImage.png")]
		private var Frame_iconifiedIcon_rolloverImage:Class;

		//========= Frame_normalIcon Images =======
		[Embed(source="../../../../graphics/ui/Frame_normalIcon_defaultImage.png")]
		private var Frame_normalIcon_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_normalIcon_pressedImage.png")]
		private var Frame_normalIcon_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_normalIcon_disabledImage.png")]
		private var Frame_normalIcon_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_normalIcon_rolloverImage.png")]
		private var Frame_normalIcon_rolloverImage:Class;

		//========= Frame_maximizeIcon Images =======
		[Embed(source="../../../../graphics/ui/Frame_maximizeIcon_defaultImage.png")]
		private var Frame_maximizeIcon_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_maximizeIcon_pressedImage.png")]
		private var Frame_maximizeIcon_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_maximizeIcon_disabledImage.png")]
		private var Frame_maximizeIcon_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_maximizeIcon_rolloverImage.png")]
		private var Frame_maximizeIcon_rolloverImage:Class;

		//========= Close Icon Images =======
		[Embed(source="../../../../graphics/ui/Frame_closeIcon_defaultImage.png")]
		private var Frame_closeIcon_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_closeIcon_pressedImage.png")]
		private var Frame_closeIcon_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_closeIcon_disabledImage.png")]
		private var Frame_closeIcon_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_closeIcon_rolloverImage.png")]
		private var Frame_closeIcon_rolloverImage:Class;
		
		[Embed(source="../../../../graphics/ui/Frame_largeCloseIcon_defaultImage.png")]
		private var Frame_largeCloseIcon_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_largeCloseIcon_pressedImage.png")]
		private var Frame_largeCloseIcon_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/clear.png")]
		private var Frame_largeCloseIcon_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Frame_largeCloseIcon_rolloverImage.png")]
		private var Frame_largeCloseIcon_rolloverImage:Class;		
		
		//========= Chat Image =======
		[Embed(source="../../../../graphics/ui/chat_defaultImage.png")]
		private var Frame_chatIcon_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/chat_defaultImage.png")]
		private var Frame_chatIcon_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/chat_defaultImage.png")]
		private var Frame_chatIcon_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/chat_rolloverImage.png")]
		private var Frame_chatIcon_rolloverImage:Class;	
		
		//========= Chat Enabled Image =======
		[Embed(source="../../../../graphics/ui/chat_enabled_icon.png")]
		private var Frame_chatEnabledIcon_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/chat_enabled_icon.png")]
		private var Frame_chatEnabledIcon_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/chat_enabled_icon.png")]
		private var Frame_chatEnabledIcon_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/chat_enabled_icon.png")]
		private var Frame_chatEnabledIcon_rolloverImage:Class;				
		
		//========= Chat Enabled Image =======
		[Embed(source="../../../../graphics/ui/chat_disabled_icon.png")]
		private var Frame_chatDisabledIcon_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/chat_disabled_icon.png")]
		private var Frame_chatDisabledIcon_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/chat_disabled_icon.png")]
		private var Frame_chatDisabledIcon_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/chat_disabled_icon.png")]
		private var Frame_chatDisabledIcon_rolloverImage:Class;	
		//----------------------------------------------------------------------
		//___________________________ ToolTip scale-9 __________________________
		//======================================================================
		[Embed(source="../../../../graphics/ui/ToolTip_bgImage.png", scaleGridTop="3", scaleGridBottom="16",
		scaleGridLeft="3", scaleGridRight="39")]
		private var ToolTip_bgImage:Class;

		//------------------------------------------------------------------------
		//___________________________ ComboBox scale-9 ___________________________
		//========================================================================

		//========= Background Images =======
		[Embed(source="../../../../graphics/ui/ComboBox_defaultImage.png", scaleGridTop="4", scaleGridBottom="17",
		scaleGridLeft="4", scaleGridRight="73")]
		private var ComboBox_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ComboBox_uneditableImage.png", scaleGridTop="4", scaleGridBottom="17",
		scaleGridLeft="4", scaleGridRight="73")]
		private var ComboBox_uneditableImage:Class;
		//by default the rollover state is null(means same to normal state), but you can add it by remove the comments
		//[Embed(source="../../../../graphics/ui/ComboBox_defaultRolloverImage.png", scaleGridTop="3", scaleGridBottom="20",
		//	scaleGridLeft="3", scaleGridRight="131")]
		private var ComboBox_defaultRolloverImage:Class;
		//[Embed(source="../../../../graphics/ui/ComboBox_uneditableRolloverImage.png", scaleGridTop="3", scaleGridBottom="20",
		//	scaleGridLeft="3", scaleGridRight="131")]
		private var ComboBox_uneditableRolloverImage:Class;
		//[Embed(source="../../../../graphics/ui/ComboBox_defaultPressedImage.png", scaleGridTop="3", scaleGridBottom="20",
		//	scaleGridLeft="3", scaleGridRight="131")]
		private var ComboBox_defaultPressedImage:Class;
		//[Embed(source="../../../../graphics/ui/ComboBox_uneditablePressedImage.png", scaleGridTop="3", scaleGridBottom="20",
		//	scaleGridLeft="3", scaleGridRight="131")]
		private var ComboBox_uneditablePressedImage:Class;

		[Embed(source="../../../../graphics/ui/ComboBox_disabledImage.png", scaleGridTop="4", scaleGridBottom="17",
		scaleGridLeft="4", scaleGridRight="73")]
		private var ComboBox_disabledImage:Class;

		//========= Arrow Button Images =======
		[Embed(source="../../../../graphics/ui/ComboBox_arrowButton_defaultImage.png")]
		private var ComboBox_arrowButton_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ComboBox_arrowButton_pressedImage.png")]
		private var ComboBox_arrowButton_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/ComboBox_arrowButton_disabledImage.png")]
		private var ComboBox_arrowButton_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/ComboBox_arrowButton_rolloverImage.png")]
		private var ComboBox_arrowButton_rolloverImage:Class;

		//----------------------------------------------------------------------
		//___________________________ 51 header scale-9 ___________________________
		//======================================================================
		[Embed(source="../../../../graphics/ui/Accordion_header_defaultImage.png", scaleGridTop="11", scaleGridBottom="12",
		scaleGridLeft="5", scaleGridRight="75")]
		private var Accordion_header_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Accordion_header_pressedImage.png", scaleGridTop="11", scaleGridBottom="12",
		scaleGridLeft="5", scaleGridRight="75")]
		private var Accordion_header_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Accordion_header_rolloverImage.png", scaleGridTop="11", scaleGridBottom="12",
		scaleGridLeft="5", scaleGridRight="75")]
		private var Accordion_header_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/Accordion_header_disabledImage.png", scaleGridTop="11", scaleGridBottom="12",
		scaleGridLeft="5", scaleGridRight="75")]
		private var Accordion_header_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Accordion_header_selectedImage.png", scaleGridTop="11", scaleGridBottom="12",
		scaleGridLeft="5", scaleGridRight="75")]
		private var Accordion_header_selectedImage:Class;

		//----------------------------------------------------------------------
		//___________________________ TabbedPane _______________________________
		//======================================================================
		//========= header top scale-9 =======
		[Embed(source="../../../../graphics/ui/TabbedPane_top_tab_defaultImage.png", scaleGridTop="5", scaleGridBottom="18",
		scaleGridLeft="5", scaleGridRight="61")]
		private var TabbedPane_top_tab_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_top_tab_pressedImage.png", scaleGridTop="5", scaleGridBottom="18",
		scaleGridLeft="5", scaleGridRight="61")]
		private var TabbedPane_top_tab_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_top_tab_rolloverImage.png", scaleGridTop="5", scaleGridBottom="18",
		scaleGridLeft="5", scaleGridRight="61")]
		private var TabbedPane_top_tab_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_top_tab_disabledImage.png", scaleGridTop="5", scaleGridBottom="18",
		scaleGridLeft="5", scaleGridRight="61")]
		private var TabbedPane_top_tab_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_top_tab_selectedImage.png", scaleGridTop="5", scaleGridBottom="18",
		scaleGridLeft="5", scaleGridRight="61")]
		private var TabbedPane_top_tab_selectedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_top_tab_rolloverSelectedImage.png", scaleGridTop="5", scaleGridBottom="18",
		scaleGridLeft="5", scaleGridRight="61")]
		private var TabbedPane_top_tab_rolloverSelectedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_top_tab_pressedSelectedImage.png", scaleGridTop="5", scaleGridBottom="18",
		scaleGridLeft="5", scaleGridRight="61")]
		private var TabbedPane_top_tab_pressedSelectedImage:Class;

		//========= header bottom scale-9 =======
		[Embed(source="../../../../graphics/ui/TabbedPane_bottom_tab_defaultImage.png", scaleGridTop="12", scaleGridBottom="14",
		scaleGridLeft="8", scaleGridRight="58")]
		private var TabbedPane_bottom_tab_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_bottom_tab_pressedImage.png", scaleGridTop="12", scaleGridBottom="14",
		scaleGridLeft="8", scaleGridRight="58")]
		private var TabbedPane_bottom_tab_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_bottom_tab_rolloverImage.png", scaleGridTop="12", scaleGridBottom="14",
		scaleGridLeft="8", scaleGridRight="58")]
		private var TabbedPane_bottom_tab_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_bottom_tab_disabledImage.png", scaleGridTop="12", scaleGridBottom="14",
		scaleGridLeft="8", scaleGridRight="58")]
		private var TabbedPane_bottom_tab_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_bottom_tab_selectedImage.png", scaleGridTop="12", scaleGridBottom="14",
		scaleGridLeft="8", scaleGridRight="58")]
		private var TabbedPane_bottom_tab_selectedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_bottom_tab_rolloverSelectedImage.png", scaleGridTop="12", scaleGridBottom="14",
		scaleGridLeft="8", scaleGridRight="58")]
		private var TabbedPane_bottom_tab_rolloverSelectedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_bottom_tab_pressedSelectedImage.png", scaleGridTop="12", scaleGridBottom="14",
		scaleGridLeft="8", scaleGridRight="58")]
		private var TabbedPane_bottom_tab_pressedSelectedImage:Class;

		//========= header left scale-9 =======
		[Embed(source="../../../../graphics/ui/TabbedPane_left_tab_defaultImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_left_tab_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_left_tab_pressedImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_left_tab_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_left_tab_rolloverImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_left_tab_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_left_tab_disabledImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_left_tab_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_left_tab_selectedImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_left_tab_selectedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_left_tab_rolloverSelectedImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_left_tab_rolloverSelectedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_left_tab_pressedSelectedImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_left_tab_pressedSelectedImage:Class;

		//========= header right scale-9 =======
		[Embed(source="../../../../graphics/ui/TabbedPane_right_tab_defaultImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_right_tab_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_right_tab_pressedImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_right_tab_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_right_tab_rolloverImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_right_tab_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_right_tab_disabledImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_right_tab_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_right_tab_selectedImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_right_tab_selectedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_right_tab_rolloverSelectedImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_right_tab_pressedSelectedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_right_tab_pressedSelectedImage.png", scaleGridTop="8", scaleGridBottom="58",
		scaleGridLeft="12", scaleGridRight="14")]
		private var TabbedPane_right_tab_rolloverSelectedImage:Class;

		//========= Background Image scale-9 =======
		[Embed(source="../../../../graphics/ui/TabbedPane_top_contentRoundImage.png", scaleGridTop="20", scaleGridBottom="325",
		scaleGridLeft="25", scaleGridRight="655")]
		private var TabbedPane_top_contentRoundImage:Class;
		[Embed(source="../../../../graphics/ui/TabbedPane_bottom_contentRoundImage.png", scaleGridTop="20", scaleGridBottom="80",
		scaleGridLeft="20", scaleGridRight="80")]
		private var TabbedPane_bottom_contentRoundImage:Class;
		[Embed(source="../../../../graphics/ui/TabbedPane_left_contentRoundImage.png", scaleGridTop="20", scaleGridBottom="80",
		scaleGridLeft="20", scaleGridRight="80")]
		private var TabbedPane_left_contentRoundImage:Class;
		[Embed(source="../../../../graphics/ui/TabbedPane_right_contentRoundImage.png", scaleGridTop="20", scaleGridBottom="80",
		scaleGridLeft="20", scaleGridRight="80")]
		private var TabbedPane_right_contentRoundImage:Class;

		//========= Left Arrow Images =======
		[Embed(source="../../../../graphics/ui/TabbedPane_arrowLeft_defaultImage.png")]
		private var TabbedPane_arrowLeft_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_arrowLeft_pressedImage.png")]
		private var TabbedPane_arrowLeft_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_arrowLeft_disabledImage.png")]
		private var TabbedPane_arrowLeft_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_arrowLeft_rolloverImage.png")]
		private var TabbedPane_arrowLeft_rolloverImage:Class;

		//========= Right Arrow Images =======
		[Embed(source="../../../../graphics/ui/TabbedPane_arrowRight_defaultImage.png")]
		private var TabbedPane_arrowRight_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_arrowRight_pressedImage.png")]
		private var TabbedPane_arrowRight_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_arrowRight_disabledImage.png")]
		private var TabbedPane_arrowRight_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_arrowRight_rolloverImage.png")]
		private var TabbedPane_arrowRight_rolloverImage:Class;

		//========= Close Button Images =======
		[Embed(source="../../../../graphics/ui/TabbedPane_closeButton_defaultImage.png")]
		private var TabbedPane_closeButton_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_closeButton_pressedImage.png")]
		private var TabbedPane_closeButton_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_closeButton_disabledImage.png")]
		private var TabbedPane_closeButton_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/TabbedPane_closeButton_rolloverImage.png")]
		private var TabbedPane_closeButton_rolloverImage:Class;
		
		//============ Chat Tab Images ===========
		[Embed(source="../../../../graphics/ui/ChatTabbedPane_top_tab_defaultImage.png", scaleGridTop="5", scaleGridBottom="15",
		scaleGridLeft="8", scaleGridRight="58")]
		private var Chat_tab_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/ChatTabbedPane_top_tab_light.png", scaleGridTop="5", scaleGridBottom="15",
		scaleGridLeft="8", scaleGridRight="58")]
		private var Chat_tab_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/ChatTabbedPane_top_tab_dark.png", scaleGridTop="5", scaleGridBottom="15",
		scaleGridLeft="8", scaleGridRight="58")]
		private var Chat_tab_selectedImage:Class;
		
		//----------------------------------------------------------------------
		//_______________________________ Slider _______________________________
		//======================================================================
		//========= track scale-9 or not=======
		[Embed(source="../../../../graphics/ui/Slider_vertical_trackImage.png", scaleGridTop="7", scaleGridBottom="93",
		scaleGridLeft="3", scaleGridRight="13")]
		private var Slider_vertical_trackImage:Class;
		[Embed(source="../../../../graphics/ui/Slider_vertical_trackDisabledImage.png", scaleGridTop="7", scaleGridBottom="93",
		scaleGridLeft="3", scaleGridRight="13")]
		private var Slider_vertical_trackDisabledImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_vertical_trackProgressImage.png", scaleGridTop="3", scaleGridBottom="90",
		scaleGridLeft="3", scaleGridRight="13")]
		private var Slider_vertical_trackProgressImage:Class;
		[Embed(source="../../../../graphics/ui/Slider_vertical_trackProgressDisabledImage.png", scaleGridTop="3", scaleGridBottom="90",
		scaleGridLeft="3", scaleGridRight="13")]
		private var Slider_vertical_trackProgressDisabledImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_horizontal_trackImage.png", scaleGridTop="3", scaleGridBottom="12",
		scaleGridLeft="7", scaleGridRight="93")]
		private var Slider_horizontal_trackImage:Class;
		[Embed(source="../../../../graphics/ui/Slider_horizontal_trackDisabledImage.png", scaleGridTop="3", scaleGridBottom="12",
		scaleGridLeft="7", scaleGridRight="93")]
		private var Slider_horizontal_trackDisabledImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_horizontal_trackProgressImage.png", scaleGridTop="3", scaleGridBottom="12",
		scaleGridLeft="3", scaleGridRight="90")]
		private var Slider_horizontal_trackProgressImage:Class;
		[Embed(source="../../../../graphics/ui/Slider_horizontal_trackProgressDisabledImage.png", scaleGridTop="3", scaleGridBottom="12",
		scaleGridLeft="3", scaleGridRight="90")]
		private var Slider_horizontal_trackProgressDisabledImage:Class;

		//========= Thumb Images =======
		[Embed(source="../../../../graphics/ui/Slider_thumbHorizontal_defaultImage.png")]
		private var Slider_thumbHorizontal_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_thumbHorizontal_pressedImage.png")]
		private var Slider_thumbHorizontal_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_thumbHorizontal_disabledImage.png")]
		private var Slider_thumbHorizontal_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_thumbHorizontal_rolloverImage.png")]
		private var Slider_thumbHorizontal_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_thumbVertical_defaultImage.png")]
		private var Slider_thumbVertical_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_thumbVertical_pressedImage.png")]
		private var Slider_thumbVertical_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_thumbVertical_disabledImage.png")]
		private var Slider_thumbVertical_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Slider_thumbVertical_rolloverImage.png")]
		private var Slider_thumbVertical_rolloverImage:Class;

		//----------------------------------------------------------------------
		//___________________________ TabbedPane _______________________________
		//======================================================================
		//========= Icon Images =======
		[Embed(source="../../../../graphics/ui/Tree_leafImage.png")]
		private var Tree_leafImage:Class;

		[Embed(source="../../../../graphics/ui/Tree_folderExpandedImage.png")]
		private var Tree_folderExpandedImage:Class;

		[Embed(source="../../../../graphics/ui/Tree_folderCollapsedImage.png")]
		private var Tree_folderCollapsedImage:Class;

		//========= Control Images =======
		[Embed(source="../../../../graphics/ui/Tree_leafControlImage.png")]
		private var Tree_leafControlImage:Class;

		[Embed(source="../../../../graphics/ui/Tree_folderExpandedControlImage.png")]
		private var Tree_folderExpandedControlImage:Class;

		[Embed(source="../../../../graphics/ui/Tree_folderCollapsedControlImage.png")]
		private var Tree_folderCollapsedControlImage:Class;

		//------------------------------------------------------------------
		//___________________________ SplitPane ____________________________
		//==================================================================
		//========= Left Arrow Images =======
		[Embed(source="../../../../graphics/ui/SplitPane_arrowLeft_defaultImage.png")]
		private var SplitPane_arrowLeft_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowLeft_pressedImage.png")]
		private var SplitPane_arrowLeft_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowLeft_disabledImage.png")]
		private var SplitPane_arrowLeft_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowLeft_rolloverImage.png")]
		private var SplitPane_arrowLeft_rolloverImage:Class;

		//========= Right Arrow Images =======
		[Embed(source="../../../../graphics/ui/SplitPane_arrowRight_defaultImage.png")]
		private var SplitPane_arrowRight_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowRight_pressedImage.png")]
		private var SplitPane_arrowRight_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowRight_disabledImage.png")]
		private var SplitPane_arrowRight_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowRight_rolloverImage.png")]
		private var SplitPane_arrowRight_rolloverImage:Class;

		//========= Up Arrow Images =======
		[Embed(source="../../../../graphics/ui/SplitPane_arrowUp_defaultImage.png")]
		private var SplitPane_arrowUp_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowUp_pressedImage.png")]
		private var SplitPane_arrowUp_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowUp_disabledImage.png")]
		private var SplitPane_arrowUp_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowUp_rolloverImage.png")]
		private var SplitPane_arrowUp_rolloverImage:Class;

		//========= Down Arrow Images =======
		[Embed(source="../../../../graphics/ui/SplitPane_arrowDown_defaultImage.png")]
		private var SplitPane_arrowDown_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowDown_pressedImage.png")]
		private var SplitPane_arrowDown_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowDown_disabledImage.png")]
		private var SplitPane_arrowDown_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_arrowDown_rolloverImage.png")]
		private var SplitPane_arrowDown_rolloverImage:Class;

		//========= Background Images scale-9 =======
		[Embed(source="../../../../graphics/ui/SplitPane_divider_verticalBGImage.png")]
		private var SplitPane_divider_verticalBGImage:Class;

		[Embed(source="../../../../graphics/ui/SplitPane_divider_horizotalBGImage.png")]
		private var SplitPane_divider_horizotalBGImage:Class;

		//by default the disabled bg state is null(means same to normal state), but you can add it by remove the comments
		//[Embed(source="../../../../graphics/ui/SplitPane_divider_verticalBGDisabledImage.png", scaleGridTop="6", scaleGridBottom="18",
		//scaleGridLeft="6", scaleGridRight="67")]
		private var SplitPane_divider_verticalBGDisabledImage:Class;

		//[Embed(source="../../../../graphics/ui/SplitPane_divider_horizotalBGDisabledImage.png", scaleGridTop="6", scaleGridBottom="18",
		//scaleGridLeft="6", scaleGridRight="67")]
		private var SplitPane_divider_horizotalBGDisabledImage:Class;

		//------------------------------------------------------------------
		//___________________________ ProgressBar __________________________
		//==================================================================
		//========= Background Images scale-9 or not =======
		[Embed(source="../../../../graphics/ui/ProgressBar_verticalBGImage.png", scaleGridTop="4", scaleGridBottom="157",
		scaleGridLeft="4", scaleGridRight="11")]
		private var ProgressBar_verticalBGImage:Class;

		[Embed(source="../../../../graphics/ui/ProgressBar_horizotalBGImage.png", scaleGridTop="4", scaleGridBottom="11",
		scaleGridLeft="4", scaleGridRight="157")]
		private var ProgressBar_horizotalBGImage:Class;

		//========= Foreground Images scale-9 or not =======
		[Embed(source="../../../../graphics/ui/ProgressBar_verticalFGImage.png")]
		private var ProgressBar_verticalFGImage:Class;

		[Embed(source="../../../../graphics/ui/ProgressBar_horizotalFGImage.png")]
		private var ProgressBar_horizotalFGImage:Class;

		//----------------------------------------------------------------------
		//_______________________________ Adjuster _____________________________
		//======================================================================
		//========= Arrow Images =======
		[Embed(source="../../../../graphics/ui/Adjuster_arrowButton_defaultImage.png")]
		private var Adjuster_arrowButton_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_arrowButton_pressedImage.png")]
		private var Adjuster_arrowButton_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_arrowButton_disabledImage.png")]
		private var Adjuster_arrowButton_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_arrowButton_rolloverImage.png")]
		private var Adjuster_arrowButton_rolloverImage:Class;

		//========= Track scale-9 or not =======
		[Embed(source="../../../../graphics/ui/Adjuster_Slider_vertical_trackImage.png", scaleGridTop="7", scaleGridBottom="93",
		scaleGridLeft="3", scaleGridRight="13")]
		private var Adjuster_Slider_vertical_trackImage:Class;
		[Embed(source="../../../../graphics/ui/Adjuster_Slider_vertical_trackDisabledImage.png", scaleGridTop="7", scaleGridBottom="93",
		scaleGridLeft="3", scaleGridRight="13")]
		private var Adjuster_Slider_vertical_trackDisabledImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_vertical_trackProgressImage.png", scaleGridTop="3", scaleGridBottom="90",
		scaleGridLeft="3", scaleGridRight="13")]
		private var Adjuster_Slider_vertical_trackProgressImage:Class;
		[Embed(source="../../../../graphics/ui/Adjuster_Slider_vertical_trackProgressDisabledImage.png", scaleGridTop="3", scaleGridBottom="90",
		scaleGridLeft="3", scaleGridRight="13")]
		private var Adjuster_Slider_vertical_trackProgressDisabledImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_horizontal_trackImage.png", scaleGridTop="3", scaleGridBottom="13",
		scaleGridLeft="7", scaleGridRight="93")]
		private var Adjuster_Slider_horizontal_trackImage:Class;
		[Embed(source="../../../../graphics/ui/Adjuster_Slider_horizontal_trackDisabledImage.png", scaleGridTop="3", scaleGridBottom="13",
		scaleGridLeft="7", scaleGridRight="93")]
		private var Adjuster_Slider_horizontal_trackDisabledImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_horizontal_trackProgressImage.png", scaleGridTop="3", scaleGridBottom="13",
		scaleGridLeft="3", scaleGridRight="90")]
		private var Adjuster_Slider_horizontal_trackProgressImage:Class;
		[Embed(source="../../../../graphics/ui/Adjuster_Slider_horizontal_trackProgressDisabledImage.png", scaleGridTop="3", scaleGridBottom="13",
		scaleGridLeft="3", scaleGridRight="90")]
		private var Adjuster_Slider_horizontal_trackProgressDisabledImage:Class;

		//========= Thumb Images =======
		[Embed(source="../../../../graphics/ui/Adjuster_Slider_thumbHorizontal_defaultImage.png")]
		private var Adjuster_Slider_thumbHorizontal_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_thumbHorizontal_pressedImage.png")]
		private var Adjuster_Slider_thumbHorizontal_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_thumbHorizontal_disabledImage.png")]
		private var Adjuster_Slider_thumbHorizontal_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_thumbHorizontal_rolloverImage.png")]
		private var Adjuster_Slider_thumbHorizontal_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_thumbVertical_defaultImage.png")]
		private var Adjuster_Slider_thumbVertical_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_thumbVertical_pressedImage.png")]
		private var Adjuster_Slider_thumbVertical_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_thumbVertical_disabledImage.png")]
		private var Adjuster_Slider_thumbVertical_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/Adjuster_Slider_thumbVertical_rolloverImage.png")]
		private var Adjuster_Slider_thumbVertical_rolloverImage:Class;

		//----------------------------------------------------------------------
		//_______________________________ Table Header _________________________
		//======================================================================
		//========= Cell Background Images =======
		[Embed(source="../../../../graphics/ui/TableHeader_cell_defaultImage.png", scaleGridTop="2", scaleGridBottom="17",
		scaleGridLeft="2", scaleGridRight="122")]
		private var TableHeader_cell_defaultImage:Class;

		[Embed(source="../../../../graphics/ui/TableHeader_cell_pressedImage.png", scaleGridTop="2", scaleGridBottom="17",
		scaleGridLeft="2", scaleGridRight="122")]
		private var TableHeader_cell_pressedImage:Class;

		[Embed(source="../../../../graphics/ui/TableHeader_cell_disabledImage.png", scaleGridTop="2", scaleGridBottom="17",
		scaleGridLeft="2", scaleGridRight="122")]
		private var TableHeader_cell_disabledImage:Class;

		[Embed(source="../../../../graphics/ui/TableHeader_cell_rolloverImage.png", scaleGridTop="2", scaleGridBottom="17",
		scaleGridLeft="2", scaleGridRight="122")]
		private var TableHeader_cell_rolloverImage:Class;

		//----------------------------------------------------------------------
		//___________________ Menu containers scale-9 or not ___________________
		//======================================================================
		//not specified a image, so it will be solid bg, however, you can add the image
		//[Embed(source="../../../../graphics/ui/MenuBar_bgImage.png")]
		private var MenuBar_bgImage:Class;
		[Embed(source="../../../../graphics/ui/PopupMenu_bgImage.png", scaleGridTop="6", scaleGridBottom="274",
		scaleGridLeft="6", scaleGridRight="169")]
		private var PopupMenu_bgImage:Class;

		//----------------------------------------------------------------------
		//______________________ MenuItemss scale-9 or not _____________________
		//======================================================================
		//Just defined roll over image, however, you can add other state images.
		[Embed(source="../../../../graphics/ui/Menu_rolloverImage.png", scaleGridTop="2", scaleGridBottom="16",
		scaleGridLeft="2", scaleGridRight="165")]
		private var Menu_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/MenuItem_rolloverImage.png", scaleGridTop="2", scaleGridBottom="16",
		scaleGridLeft="2", scaleGridRight="165")]
		private var MenuItem_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/CheckBoxMenuItem_rolloverImage.png", scaleGridTop="2", scaleGridBottom="16",
		scaleGridLeft="2", scaleGridRight="165")]
		private var CheckBoxMenuItem_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/RadioButtonMenuItem_rolloverImage.png", scaleGridTop="2", scaleGridBottom="16",
		scaleGridLeft="2", scaleGridRight="165")]
		private var RadioButtonMenuItem_rolloverImage:Class;

		[Embed(source="../../../../graphics/ui/Menu_arrowIconImage.png")]
		private var Menu_arrowIconImage:Class;

		[Embed(source="../../../../graphics/ui/RadioButtonMenuItem_checkIconImage.png")]
		private var RadioButtonMenuItem_checkIconImage:Class;

		[Embed(source="../../../../graphics/ui/CheckBoxMenuItem_checkIconImage.png")]
		private var CheckBoxMenuItem_checkIconImage:Class;

		//-----------------------------------------------------------------------------
		//___________________________ initComponentDefaults ___________________________
		//=============================================================================

		override protected function initComponentDefaults(table:UIDefaults):void{
			super.initComponentDefaults(table);
			
			// *** Game Menu
			var comDefaults:Array = [
				"GameMenu.bar", GameContainer_gameBar
			];
			table.putDefaults(comDefaults);
			
			// *** Button
			comDefaults = [
			"Button.background", new ASColorUIResource(0x839DAD), //this is just for calculate disabled color
			"Button.foreground", new ASColorUIResource(0xffffff),
			"Button.textFilters", new ArrayUIResource([]),
			"Button.opaque", false,
			"Button.defaultImage", Button_defaultImage,
			"Button.pressedImage", Button_pressedImage,
			"Button.disabledImage", Button_disabledImage,
			"Button.rolloverImage", Button_rolloverImage,
			"Button.DefaultButton.defaultImage", Button_DefaultButton_defaultImage,
			"Button.bg", SkinButtonBackground,
			"Button.margin", new InsetsUIResource(5, 8, 5, 8), //modify this to fit the image border margin
			"Button.textShiftOffset", 0
			];
			table.putDefaults(comDefaults);

			// *** Button in GameJBox
			comDefaults = [
			"Class.GameJBoxButton", [
				"Button.background", new ASColorUIResource(0x839DAD), //this is just for calculate disabled color
				"Button.foreground", new ASColorUIResource(0xffffff),
				"Button.textFilters", new ArrayUIResource([]),
				"Button.opaque", false,
				"Button.defaultImage", Chat_tab_selectedImage,
				"Button.pressedImage", Chat_tab_selectedImage,
				"Button.disabledImage", Chat_tab_rolloverImage,
				"Button.rolloverImage", Chat_tab_rolloverImage,
				//"Button.DefaultButton.defaultImage", Chat_tab_defaultImage,
				"Button.bg", SkinButtonBackground,
				"Button.margin", new InsetsUIResource(5, 8, 5, 8), //modify this to fit the image border margin
				"Button.textShiftOffset", 0
			]];
			table.putDefaults(comDefaults);

			// *** ToggleButton
			comDefaults = [
			"ToggleButton.background", new ASColorUIResource(0x839DAD), //this is just for calculate disabled color
			"ToggleButton.foreground", new ASColorUIResource(0xffffff),
			"ToggleButton.textFilters", new ArrayUIResource([]),
			"ToggleButton.opaque", false,
			"ToggleButton.defaultImage", ToggleButton_defaultImage,
			"ToggleButton.pressedImage", ToggleButton_pressedImage,
			"ToggleButton.disabledImage", ToggleButton_disabledImage,
			"ToggleButton.selectedImage", ToggleButton_selectedImage,
			"ToggleButton.disabledSelectedImage", ToggleButton_disabledSelectedImage,
			"ToggleButton.rolloverImage", ToggleButton_rolloverImage,
			"ToggleButton.rolloverSelectedImage", ToggleButton_rolloverSelectedImage,
			"ToggleButton.bg", SkinToggleButtonBackground,
			"ToggleButton.margin", new InsetsUIResource(5, 8, 5, 8) //modify this to fit the image border margin
			];
			table.putDefaults(comDefaults);
			
			// *** Chat Tabs Toggle Button
			comDefaults = [
			"Class.ChatTab", [
				"ToggleButton.background", new ASColorUIResource(0x839DAD),
				"ToggleButton.foreground", new ASColorUIResource(0xffffff),
				"ToggleButton.textFilters", new ArrayUIResource([]),
				"ToggleButton.opaque", false,
				"ToggleButton.defaultImage", Chat_tab_defaultImage,
				"ToggleButton.pressedImage", Chat_tab_rolloverImage,
				"ToggleButton.disabledImage", Chat_tab_selectedImage,
				"ToggleButton.selectedImage", Chat_tab_selectedImage,
				"ToggleButton.disabledSelectedImage", Chat_tab_selectedImage,
				"ToggleButton.rolloverImage", Chat_tab_selectedImage,
				"ToggleButton.rolloverSelectedImage", Chat_tab_selectedImage,
				"ToggleButton.bg", SkinToggleButtonBackground,
				"ToggleButton.margin", new InsetsUIResource(5, 5, 5, 5)
			]];
			table.putDefaults(comDefaults);			

			// *** RadioButton
			comDefaults = [
			"RadioButton.icon", SkinRadioButtonIcon,
			"RadioButton.defaultImage", RadioButton_defaultImage,
			"RadioButton.pressedImage", RadioButton_pressedImage,
			"RadioButton.pressedSelectedImage", RadioButton_pressedSelectedImage,
			"RadioButton.disabledImage", RadioButton_disabledImage,
			"RadioButton.selectedImage", RadioButton_selectedImage,
			"RadioButton.disabledSelectedImage", RadioButton_disabledSelectedImage,
			"RadioButton.rolloverImage", RadioButton_rolloverImage,
			"RadioButton.rolloverSelectedImage", RadioButton_rolloverSelectedImage
			];
			table.putDefaults(comDefaults);

			// *** CheckBox
			comDefaults = [
			"CheckBox.icon", SkinCheckBoxIcon,
			"CheckBox.defaultImage", CheckBox_defaultImage,
			"CheckBox.pressedImage", CheckBox_pressedImage,
			"CheckBox.pressedSelectedImage", CheckBox_pressedSelectedImage,
			"CheckBox.disabledImage", CheckBox_disabledImage,
			"CheckBox.selectedImage", CheckBox_selectedImage,
			"CheckBox.disabledSelectedImage", CheckBox_disabledSelectedImage,
			"CheckBox.rolloverImage", CheckBox_rolloverImage,
			"CheckBox.rolloverSelectedImage", CheckBox_rolloverSelectedImage
			];
			table.putDefaults(comDefaults);

			// *** ScrollBar
			comDefaults = [
			"ScrollBar.opaque", false,
			"ScrollBar.thumbDecorator", SkinScrollBarThumb,

			"ScrollBar.bg", SkinScrollBarBackground,
			"ScrollBar.verticalBGImage", ScrollBar_verticalBGImage,
			"ScrollBar.horizotalBGImage", ScrollBar_horizotalBGImage,
			"ScrollBar.verticalBGDisabledImage", ScrollBar_verticalBGDisabledImage,
			"ScrollBar.horizotalBGDisabledImage", ScrollBar_horizotalBGDisabledImage,

			"ScrollBar.arrowLeft.defaultImage", ScrollBar_arrowLeft_defaultImage,
			"ScrollBar.arrowLeft.pressedImage", ScrollBar_arrowLeft_pressedImage,
			"ScrollBar.arrowLeft.disabledImage", ScrollBar_arrowLeft_disabledImage,
			"ScrollBar.arrowLeft.rolloverImage", ScrollBar_arrowLeft_rolloverImage,

			"ScrollBar.arrowRight.defaultImage", ScrollBar_arrowRight_defaultImage,
			"ScrollBar.arrowRight.pressedImage", ScrollBar_arrowRight_pressedImage,
			"ScrollBar.arrowRight.disabledImage", ScrollBar_arrowRight_disabledImage,
			"ScrollBar.arrowRight.rolloverImage", ScrollBar_arrowRight_rolloverImage,

			"ScrollBar.arrowUp.defaultImage", ScrollBar_arrowUp_defaultImage,
			"ScrollBar.arrowUp.pressedImage", ScrollBar_arrowUp_pressedImage,
			"ScrollBar.arrowUp.disabledImage", ScrollBar_arrowUp_disabledImage,
			"ScrollBar.arrowUp.rolloverImage", ScrollBar_arrowUp_rolloverImage,

			"ScrollBar.arrowDown.defaultImage", ScrollBar_arrowDown_defaultImage,
			"ScrollBar.arrowDown.pressedImage", ScrollBar_arrowDown_pressedImage,
			"ScrollBar.arrowDown.disabledImage", ScrollBar_arrowDown_disabledImage,
			"ScrollBar.arrowDown.rolloverImage", ScrollBar_arrowDown_rolloverImage,

			"ScrollBar.thumbVertical.defaultImage", ScrollBar_thumbVertical_defaultImage,
			"ScrollBar.thumbVertical.pressedImage", ScrollBar_thumbVertical_pressedImage,
			"ScrollBar.thumbVertical.disabledImage", ScrollBar_thumbVertical_disabledImage,
			"ScrollBar.thumbVertical.rolloverImage", ScrollBar_thumbVertical_rolloverImage,

			"ScrollBar.thumbHorizontal.defaultImage", ScrollBar_thumbHorizontal_defaultImage,
			"ScrollBar.thumbHorizontal.pressedImage", ScrollBar_thumbHorizontal_pressedImage,
			"ScrollBar.thumbHorizontal.disabledImage", ScrollBar_thumbHorizontal_disabledImage,
			"ScrollBar.thumbHorizontal.rolloverImage", ScrollBar_thumbHorizontal_rolloverImage,

			];
			table.putDefaults(comDefaults);

			// *** TextField
			comDefaults = [
			"TextField.opaque", false,
			"TextField.bg", SkinTextFieldBackground,
			"TextField.border", new SkinEmptyBorder(4, 4, 4, 4), //modify this to fit the bg image
			"TextField.defaultImage", TextField_defaultImage,
			"TextField.uneditableImage", TextField_uneditableImage,
			"TextField.disabledImage", TextField_disabledImage
			];
			table.putDefaults(comDefaults);

			// *** TextArea
			comDefaults = [
			"TextArea.opaque", false,
			"TextArea.bg", SkinTextAreaBackground,
			"TextArea.border", new SkinEmptyBorder(4, 4, 4, 4), //modify this to fit the bg image
			"TextArea.defaultImage", TextArea_defaultImage,
			"TextArea.uneditableImage", TextArea_uneditableImage,
			"TextArea.disabledImage", TextArea_disabledImage
			];
			table.putDefaults(comDefaults);

			// *** Sidebar Frame
			comDefaults = [
			"Class.Sidebar.frame", [
			"Frame.activeBG", Sidebar_frame_activeBG,
			"Frame.inactiveBG", Sidebar_frame_activeBG, // use same img as active for now
			"Frame.border", new SkinEmptyBorder(-3, 5, 10, 5)
			]
			];
			table.putDefaults(comDefaults);

			// *** Frame
			comDefaults = [
			"Frame.background", table.get("window"),
			"Frame.opaque", true,
			"Frame.icon", GameFrameIcon,
			"Frame.bg", SkinFrameBackground, //this will use Frame.activeBG and Frame.inactiveBG
			"Frame.border", new SkinEmptyBorder( -3, 4, 5, 4), //modify this to fit the frame bg image
			"Frame.borderWithoutPaper", new SkinEmptyBorder(20, 10, 10, 10),
			"Frame.activeBG", Frame_activeBG,
			"Frame.inactiveBG", Frame_activeBG, // use same img as active for now

			"FrameTitleBar.colorAdjust", new UIStyleTune(0.0, 0.0, 0.0, 0.0, 0, new UIStyleTune(0.04, -0.06, 1, 0.22, 5)),
			"FrameTitleBar.bg", SkinFrameTitleBarBG,
			"FrameTitleBar.foreground", table.get("windowText"),
			"FrameTitleBar.opaque", false,
			"FrameTitleBar.buttonGap", 2,
			"FrameTitleBar.titleBarHeight", 24, //modify this to fit title bar height of bg image
			"FrameTitleBar.iconifiedIcon", SkinFrameIconifiedIcon,
			"FrameTitleBar.normalIcon", SkinFrameNormalIcon,
			"FrameTitleBar.maximizeIcon", SkinFrameMaximizeIcon,
			"FrameTitleBar.closeIcon", SkinFrameCloseIcon,

			"Frame.iconifiedIcon.defaultImage", Frame_iconifiedIcon_defaultImage,
			"Frame.iconifiedIcon.pressedImage", Frame_iconifiedIcon_pressedImage,
			"Frame.iconifiedIcon.disabledImage", Frame_iconifiedIcon_disabledImage,
			"Frame.iconifiedIcon.rolloverImage", Frame_iconifiedIcon_rolloverImage,

			"Frame.normalIcon.defaultImage", Frame_normalIcon_defaultImage,
			"Frame.normalIcon.pressedImage", Frame_normalIcon_pressedImage,
			"Frame.normalIcon.disabledImage", Frame_normalIcon_disabledImage,
			"Frame.normalIcon.rolloverImage", Frame_normalIcon_rolloverImage,

			"Frame.maximizeIcon.defaultImage", Frame_maximizeIcon_defaultImage,
			"Frame.maximizeIcon.pressedImage", Frame_maximizeIcon_pressedImage,
			"Frame.maximizeIcon.disabledImage", Frame_maximizeIcon_disabledImage,
			"Frame.maximizeIcon.rolloverImage", Frame_maximizeIcon_rolloverImage,

			"Frame.closeIcon.defaultImage", Frame_closeIcon_defaultImage,
			"Frame.closeIcon.pressedImage", Frame_closeIcon_pressedImage,
			"Frame.closeIcon.disabledImage", Frame_closeIcon_disabledImage,
			"Frame.closeIcon.rolloverImage", Frame_closeIcon_rolloverImage,
			
			"Frame.chatIcon.defaultImage", Frame_chatIcon_defaultImage,
			"Frame.chatIcon.pressedImage", Frame_chatIcon_pressedImage,
			"Frame.chatIcon.disabledImage", Frame_chatIcon_disabledImage,
			"Frame.chatIcon.rolloverImage", Frame_chatIcon_rolloverImage,

			"Frame.chatEnabledIcon.defaultImage", Frame_chatEnabledIcon_defaultImage,
			"Frame.chatEnabledIcon.pressedImage", Frame_chatEnabledIcon_pressedImage,
			"Frame.chatEnabledIcon.disabledImage", Frame_chatEnabledIcon_disabledImage,
			"Frame.chatEnabledIcon.rolloverImage", Frame_chatEnabledIcon_rolloverImage,				
			
			"Frame.chatDisabledIcon.defaultImage", Frame_chatDisabledIcon_defaultImage,
			"Frame.chatDisabledIcon.pressedImage", Frame_chatDisabledIcon_pressedImage,
			"Frame.chatDisabledIcon.disabledImage", Frame_chatDisabledIcon_disabledImage,
			"Frame.chatDisabledIcon.rolloverImage", Frame_chatDisabledIcon_rolloverImage				
			];
			table.putDefaults(comDefaults);
			
			comDefaults = [			
			"Frame.largeCloseIcon.defaultImage", Frame_largeCloseIcon_defaultImage,
			"Frame.largeCloseIcon.pressedImage", Frame_largeCloseIcon_pressedImage,
			"Frame.largeCloseIcon.disabledImage", Frame_largeCloseIcon_disabledImage,
			"Frame.largeCloseIcon.rolloverImage", Frame_largeCloseIcon_rolloverImage
			];
			table.putDefaults(comDefaults);

			// *** ToolTip
			comDefaults = [
			"ToolTip.opaque", false,
			"ToolTip.bg", SkinToolTipBackground,
			"ToolTip.bgImage", ToolTip_bgImage,
			"ToolTip.filters", [new DropShadowFilter(1.0, 45, 0, 1.0, 2.0, 2.0, 0.5)],
			"ToolTip.border", new SkinEmptyBorder(1, 3, 1, 3) //modify this to fit the bg image
			];
			table.putDefaults(comDefaults);

			// *** List
			comDefaults = [
			"List.opaque", true, // List is used by the combobox for the popup
			"List.background", table.get("window"),
			];
			table.putDefaults(comDefaults);

			// *** ComboBox
			comDefaults = [
			"ComboBox.opaque", false,
			"ComboBox.popupBorder", GameComboBoxPopupBorder,
			"ComboBox.bg", SkinComboBoxBackground,
			"ComboBox.border", new SkinEmptyBorder(3, 4, 2, 5), //modify this to fit the bg image
			"ComboBox.defaultImage", ComboBox_defaultImage,
			"ComboBox.uneditableImage", ComboBox_uneditableImage,
			"ComboBox.disabledImage", ComboBox_disabledImage,
			"ComboBox.defaultRolloverImage", ComboBox_defaultRolloverImage,
			"ComboBox.uneditableRolloverImage", ComboBox_uneditableRolloverImage,
			"ComboBox.defaultPressedImage", ComboBox_defaultPressedImage,
			"ComboBox.uneditablePressedImage", ComboBox_uneditablePressedImage,
			"ComboBox.arrowButton.defaultImage", ComboBox_arrowButton_defaultImage,
			"ComboBox.arrowButton.pressedImage", ComboBox_arrowButton_pressedImage,
			"ComboBox.arrowButton.disabledImage", ComboBox_arrowButton_disabledImage,
			"ComboBox.arrowButton.rolloverImage", ComboBox_arrowButton_rolloverImage
			];
			table.putDefaults(comDefaults);

			// *** Accordion
			comDefaults = [
			"Accordion.header", SkinAccordionHeader,
			"Accordion.foreground", table.get("windowText"),
			"Accordion.tabMargin", new InsetsUIResource(2, 3, 3, 2),  //modify this to fit header image
			"Accordion.header.defaultImage", Accordion_header_defaultImage,
			"Accordion.header.pressedImage", Accordion_header_pressedImage,
			"Accordion.header.disabledImage", Accordion_header_disabledImage,
			"Accordion.header.rolloverImage", Accordion_header_rolloverImage,
			"Accordion.header.selectedImage", Accordion_header_selectedImage
			];
			table.putDefaults(comDefaults);

			// *** TabbedPane
			comDefaults = [
			"TabbedPane.font", new ASFontUIResource("Arial", 12, true),
			"TabbedPane.tab", SkinTabbedPaneTab,
			"TabbedPane.tabMargin", new InsetsUIResource(5, 5, 0, 5),  //modify this to fit header image
			"TabbedPane.tabBorderInsets", new InsetsUIResource(0, 10, 0, 10),
			"TabbedPane.selectedTabExpandInsets", new InsetsUIResource(0, 2, 0, 2),
			"TabbedPane.top.tab.defaultImage", TabbedPane_top_tab_defaultImage,
			"TabbedPane.top.tab.pressedImage", TabbedPane_top_tab_pressedImage,
			"TabbedPane.top.tab.disabledImage", TabbedPane_top_tab_disabledImage,
			"TabbedPane.top.tab.rolloverImage", TabbedPane_top_tab_rolloverImage,
			"TabbedPane.top.tab.selectedImage", TabbedPane_top_tab_selectedImage,
			"TabbedPane.top.tab.rolloverSelectedImage", TabbedPane_top_tab_rolloverSelectedImage,
			"TabbedPane.top.tab.pressedSelectedImage", TabbedPane_top_tab_pressedSelectedImage,

			"TabbedPane.bottom.tab.defaultImage", TabbedPane_bottom_tab_defaultImage,
			"TabbedPane.bottom.tab.pressedImage", TabbedPane_bottom_tab_pressedImage,
			"TabbedPane.bottom.tab.disabledImage", TabbedPane_bottom_tab_disabledImage,
			"TabbedPane.bottom.tab.rolloverImage", TabbedPane_bottom_tab_rolloverImage,
			"TabbedPane.bottom.tab.selectedImage", TabbedPane_bottom_tab_selectedImage,
			"TabbedPane.bottom.tab.rolloverSelectedImage", TabbedPane_bottom_tab_rolloverSelectedImage,
			"TabbedPane.bottom.tab.pressedSelectedImage", TabbedPane_bottom_tab_pressedSelectedImage,

			"TabbedPane.left.tab.defaultImage", TabbedPane_left_tab_defaultImage,
			"TabbedPane.left.tab.pressedImage", TabbedPane_left_tab_pressedImage,
			"TabbedPane.left.tab.disabledImage", TabbedPane_left_tab_disabledImage,
			"TabbedPane.left.tab.rolloverImage", TabbedPane_left_tab_rolloverImage,
			"TabbedPane.left.tab.selectedImage", TabbedPane_left_tab_selectedImage,
			"TabbedPane.left.tab.rolloverSelectedImage", TabbedPane_left_tab_rolloverSelectedImage,
			"TabbedPane.left.tab.pressedSelectedImage", TabbedPane_left_tab_pressedSelectedImage,

			"TabbedPane.right.tab.defaultImage", TabbedPane_right_tab_defaultImage,
			"TabbedPane.right.tab.pressedImage", TabbedPane_right_tab_pressedImage,
			"TabbedPane.right.tab.disabledImage", TabbedPane_right_tab_disabledImage,
			"TabbedPane.right.tab.rolloverImage", TabbedPane_right_tab_rolloverImage,
			"TabbedPane.right.tab.selectedImage", TabbedPane_right_tab_selectedImage,
			"TabbedPane.right.tab.rolloverSelectedImage", TabbedPane_right_tab_rolloverSelectedImage,
			"TabbedPane.right.tab.pressedSelectedImage", TabbedPane_right_tab_pressedSelectedImage,

			"TabbedPane.arrowLeft.defaultImage", TabbedPane_arrowLeft_defaultImage,
			"TabbedPane.arrowLeft.pressedImage", TabbedPane_arrowLeft_pressedImage,
			"TabbedPane.arrowLeft.disabledImage", TabbedPane_arrowLeft_disabledImage,
			"TabbedPane.arrowLeft.rolloverImage", TabbedPane_arrowLeft_rolloverImage,

			"TabbedPane.arrowRight.defaultImage", TabbedPane_arrowRight_defaultImage,
			"TabbedPane.arrowRight.pressedImage", TabbedPane_arrowRight_pressedImage,
			"TabbedPane.arrowRight.disabledImage", TabbedPane_arrowRight_disabledImage,
			"TabbedPane.arrowRight.rolloverImage", TabbedPane_arrowRight_rolloverImage,

			"TabbedPane.contentMargin", new InsetsUIResource(5, 15, 20, 15), //modify this to fit TabbedPane_contentRoundImage
			"TabbedPane.top.contentRoundImage", TabbedPane_top_contentRoundImage,
			"TabbedPane.bottom.contentRoundImage", TabbedPane_bottom_contentRoundImage,
			"TabbedPane.left.contentRoundImage", TabbedPane_left_contentRoundImage,
			"TabbedPane.right.contentRoundImage", TabbedPane_right_contentRoundImage,
			"TabbedPane.contentRoundLineThickness", 3, //modify this to fit contentRoundImage
			"TabbedPane.bg", null //bg is managed by SkinTabbedPaneUI
			];
			table.putDefaults(comDefaults);

			// *** ClosableTabbedPane
			comDefaults = [
			"ClosableTabbedPane.tab", SkinClosableTabbedPaneTab,
			"ClosableTabbedPane.tabMargin", new InsetsUIResource(2, 3, 0, 3),  //modify this to fit header image
			"ClosableTabbedPane.tabBorderInsets", new InsetsUIResource(0, 10, 0, 10),
			"ClosableTabbedPane.selectedTabExpandInsets", new InsetsUIResource(0, 2, 0, 2),
			"ClosableTabbedPane.top.tab.defaultImage", TabbedPane_top_tab_defaultImage,
			"ClosableTabbedPane.top.tab.pressedImage", TabbedPane_top_tab_pressedImage,
			"ClosableTabbedPane.top.tab.disabledImage", TabbedPane_top_tab_disabledImage,
			"ClosableTabbedPane.top.tab.rolloverImage", TabbedPane_top_tab_rolloverImage,
			"ClosableTabbedPane.top.tab.selectedImage", TabbedPane_top_tab_selectedImage,
			"ClosableTabbedPane.top.tab.rolloverSelectedImage", TabbedPane_top_tab_rolloverSelectedImage,

			"ClosableTabbedPane.bottom.tab.defaultImage", TabbedPane_bottom_tab_defaultImage,
			"ClosableTabbedPane.bottom.tab.pressedImage", TabbedPane_bottom_tab_pressedImage,
			"ClosableTabbedPane.bottom.tab.disabledImage", TabbedPane_bottom_tab_disabledImage,
			"ClosableTabbedPane.bottom.tab.rolloverImage", TabbedPane_bottom_tab_rolloverImage,
			"ClosableTabbedPane.bottom.tab.selectedImage", TabbedPane_bottom_tab_selectedImage,
			"ClosableTabbedPane.bottom.tab.rolloverSelectedImage", TabbedPane_bottom_tab_rolloverSelectedImage,

			"ClosableTabbedPane.left.tab.defaultImage", TabbedPane_left_tab_defaultImage,
			"ClosableTabbedPane.left.tab.pressedImage", TabbedPane_left_tab_pressedImage,
			"ClosableTabbedPane.left.tab.disabledImage", TabbedPane_left_tab_disabledImage,
			"ClosableTabbedPane.left.tab.rolloverImage", TabbedPane_left_tab_rolloverImage,
			"ClosableTabbedPane.left.tab.selectedImage", TabbedPane_left_tab_selectedImage,
			"ClosableTabbedPane.left.tab.rolloverSelectedImage", TabbedPane_left_tab_rolloverSelectedImage,

			"ClosableTabbedPane.right.tab.defaultImage", TabbedPane_right_tab_defaultImage,
			"ClosableTabbedPane.right.tab.pressedImage", TabbedPane_right_tab_pressedImage,
			"ClosableTabbedPane.right.tab.disabledImage", TabbedPane_right_tab_disabledImage,
			"ClosableTabbedPane.right.tab.rolloverImage", TabbedPane_right_tab_rolloverImage,
			"ClosableTabbedPane.right.tab.selectedImage", TabbedPane_right_tab_selectedImage,
			"ClosableTabbedPane.right.tab.rolloverSelectedImage", TabbedPane_right_tab_rolloverSelectedImage,

			"ClosableTabbedPane.arrowLeft.defaultImage", TabbedPane_arrowLeft_defaultImage,
			"ClosableTabbedPane.arrowLeft.pressedImage", TabbedPane_arrowLeft_pressedImage,
			"ClosableTabbedPane.arrowLeft.disabledImage", TabbedPane_arrowLeft_disabledImage,
			"ClosableTabbedPane.arrowLeft.rolloverImage", TabbedPane_arrowLeft_rolloverImage,

			"ClosableTabbedPane.arrowRight.defaultImage", TabbedPane_arrowRight_defaultImage,
			"ClosableTabbedPane.arrowRight.pressedImage", TabbedPane_arrowRight_pressedImage,
			"ClosableTabbedPane.arrowRight.disabledImage", TabbedPane_arrowRight_disabledImage,
			"ClosableTabbedPane.arrowRight.rolloverImage", TabbedPane_arrowRight_rolloverImage,

			"ClosableTabbedPane.closeButton.defaultImage", TabbedPane_closeButton_defaultImage,
			"ClosableTabbedPane.closeButton.pressedImage", TabbedPane_closeButton_pressedImage,
			"ClosableTabbedPane.closeButton.disabledImage", TabbedPane_closeButton_disabledImage,
			"ClosableTabbedPane.closeButton.rolloverImage", TabbedPane_closeButton_rolloverImage,

			"ClosableTabbedPane.contentMargin", new InsetsUIResource(3, 3, 3, 3), //modify this to fit TabbedPane_contentRoundImage
			"ClosableTabbedPane.top.contentRoundImage", TabbedPane_top_contentRoundImage,
			"ClosableTabbedPane.bottom.contentRoundImage", TabbedPane_bottom_contentRoundImage,
			"ClosableTabbedPane.left.contentRoundImage", TabbedPane_left_contentRoundImage,
			"ClosableTabbedPane.right.contentRoundImage", TabbedPane_right_contentRoundImage,
			"ClosableTabbedPane.contentRoundLineThickness", 3, //modify this to fit contentRoundImage
			"ClosableTabbedPane.bg", null //bg is managed by SkinTabbedPaneUI
			];
			table.putDefaults(comDefaults);

			// *** Slider
			comDefaults = [
			"Slider.vertical.trackImage", Slider_vertical_trackImage,
			"Slider.vertical.trackDisabledImage", Slider_vertical_trackDisabledImage,
			"Slider.vertical.trackProgressImage", Slider_vertical_trackProgressImage,
			"Slider.vertical.trackProgressDisabledImage", Slider_vertical_trackProgressDisabledImage,

			"Slider.horizontal.trackImage", Slider_horizontal_trackImage,
			"Slider.horizontal.trackDisabledImage", Slider_horizontal_trackDisabledImage,
			"Slider.horizontal.trackProgressImage", Slider_horizontal_trackProgressImage,
			"Slider.horizontal.trackProgressDisabledImage", Slider_horizontal_trackProgressDisabledImage,

			"Slider.thumbVertical.defaultImage", Slider_thumbVertical_defaultImage,
			"Slider.thumbVertical.pressedImage", Slider_thumbVertical_pressedImage,
			"Slider.thumbVertical.disabledImage", Slider_thumbVertical_disabledImage,
			"Slider.thumbVertical.rolloverImage", Slider_thumbVertical_rolloverImage,

			"Slider.thumbHorizontal.defaultImage", Slider_thumbHorizontal_defaultImage,
			"Slider.thumbHorizontal.pressedImage", Slider_thumbHorizontal_pressedImage,
			"Slider.thumbHorizontal.disabledImage", Slider_thumbHorizontal_disabledImage,
			"Slider.thumbHorizontal.rolloverImage", Slider_thumbHorizontal_rolloverImage,

			"Slider.thumbIcon", SkinSliderThumb
			];
			table.putDefaults(comDefaults);

			// *** Tree
			comDefaults = [
			"Tree.leafIcon", SkinTreeLeafIcon,
			"Tree.folderExpandedIcon", SkinTreeFolderExpandedIcon,
			"Tree.folderCollapsedIcon", SkinTreeFolderCollapsedIcon,
			"Tree.leafImage", Tree_leafImage,
			"Tree.folderExpandedImage", Tree_folderExpandedImage,
			"Tree.folderCollapsedImage", Tree_folderCollapsedImage,

			"Tree.leftChildIndent", 15, //modify this to fit control images width
			"Tree.rightChildIndent", 0,
			"Tree.expandControl", SkinTreeExpandControl,
			"Tree.leafControlImage", Tree_leafControlImage,
			"Tree.folderExpandedControlImage", Tree_folderExpandedControlImage,
			"Tree.folderCollapsedControlImage", Tree_folderCollapsedControlImage
			];
			table.putDefaults(comDefaults);

			// *** SplitPane
			comDefaults = [
			"SplitPane.presentDragColor", new ASColorUIResource(0x000000, 40),

			"SplitPane.defaultDividerSize", 7, //modify this to fit the divier images
			"SplitPane.divider.verticalBGImage", SplitPane_divider_verticalBGImage,
			"SplitPane.divider.horizotalBGImage", SplitPane_divider_horizotalBGImage,
			"SplitPane.divider.verticalBGDisabledImage", SplitPane_divider_verticalBGDisabledImage,
			"SplitPane.divider.horizotalBGDisabledImage", SplitPane_divider_horizotalBGDisabledImage,

			"SplitPane.arrowLeft.defaultImage", SplitPane_arrowLeft_defaultImage,
			"SplitPane.arrowLeft.pressedImage", SplitPane_arrowLeft_pressedImage,
			"SplitPane.arrowLeft.disabledImage", SplitPane_arrowLeft_disabledImage,
			"SplitPane.arrowLeft.rolloverImage", SplitPane_arrowLeft_rolloverImage,

			"SplitPane.arrowRight.defaultImage", SplitPane_arrowRight_defaultImage,
			"SplitPane.arrowRight.pressedImage", SplitPane_arrowRight_pressedImage,
			"SplitPane.arrowRight.disabledImage", SplitPane_arrowRight_disabledImage,
			"SplitPane.arrowRight.rolloverImage", SplitPane_arrowRight_rolloverImage,

			"SplitPane.arrowUp.defaultImage", SplitPane_arrowUp_defaultImage,
			"SplitPane.arrowUp.pressedImage", SplitPane_arrowUp_pressedImage,
			"SplitPane.arrowUp.disabledImage", SplitPane_arrowUp_disabledImage,
			"SplitPane.arrowUp.rolloverImage", SplitPane_arrowUp_rolloverImage,

			"SplitPane.arrowDown.defaultImage", SplitPane_arrowDown_defaultImage,
			"SplitPane.arrowDown.pressedImage", SplitPane_arrowDown_pressedImage,
			"SplitPane.arrowDown.disabledImage", SplitPane_arrowDown_disabledImage,
			"SplitPane.arrowDown.rolloverImage", SplitPane_arrowDown_rolloverImage
			];
			table.putDefaults(comDefaults);

			// *** ProgressBar
			comDefaults = [
			"ProgressBar.border", null,
			"ProgressBar.foreground", table.get("controlText"),
			"ProgressBar.bg", SkinProgressBarBackground,
			"ProgressBar.fg", GameProgressBarFG,
			"ProgressBar.indeterminateDelay", 200,
			"ProgressBar.fgMargin", new InsetsUIResource(2, 2, 2, 2), //modify this to margin fg
			"ProgressBar.verticalBGImage", ProgressBar_verticalBGImage,
			"ProgressBar.horizotalBGImage", ProgressBar_horizotalBGImage,
			"ProgressBar.verticalFGImage", ProgressBar_verticalFGImage,
			"ProgressBar.horizotalFGImage", ProgressBar_horizotalFGImage
			];
			table.putDefaults(comDefaults);

			// *** Adjuster
			comDefaults = [
			"Adjuster.bg", SkinComboBoxBackground,
			"Adjuster.border", new SkinEmptyBorder(2, 4, 0, 0),

			"Adjuster.arrowButton.defaultImage", Adjuster_arrowButton_defaultImage,
			"Adjuster.arrowButton.pressedImage", Adjuster_arrowButton_pressedImage,
			"Adjuster.arrowButton.disabledImage", Adjuster_arrowButton_disabledImage,
			"Adjuster.arrowButton.rolloverImage", Adjuster_arrowButton_rolloverImage,

			"Adjuster.Slider.vertical.trackImage", Adjuster_Slider_vertical_trackImage,
			"Adjuster.Slider.vertical.trackDisabledImage", Adjuster_Slider_vertical_trackDisabledImage,
			"Adjuster.Slider.vertical.trackProgressImage", Adjuster_Slider_vertical_trackProgressImage,
			"Adjuster.Slider.vertical.trackProgressDisabledImage", Adjuster_Slider_vertical_trackProgressDisabledImage,

			"Adjuster.Slider.horizontal.trackImage", Adjuster_Slider_horizontal_trackImage,
			"Adjuster.Slider.horizontal.trackDisabledImage", Adjuster_Slider_horizontal_trackDisabledImage,
			"Adjuster.Slider.horizontal.trackProgressImage", Adjuster_Slider_horizontal_trackProgressImage,
			"Adjuster.Slider.horizontal.trackProgressDisabledImage", Adjuster_Slider_horizontal_trackProgressDisabledImage,

			"Adjuster.Slider.thumbVertical.defaultImage", Adjuster_Slider_thumbVertical_defaultImage,
			"Adjuster.Slider.thumbVertical.pressedImage", Adjuster_Slider_thumbVertical_pressedImage,
			"Adjuster.Slider.thumbVertical.disabledImage", Adjuster_Slider_thumbVertical_disabledImage,
			"Adjuster.Slider.thumbVertical.rolloverImage", Adjuster_Slider_thumbVertical_rolloverImage,

			"Adjuster.Slider.thumbHorizontal.defaultImage", Adjuster_Slider_thumbHorizontal_defaultImage,
			"Adjuster.Slider.thumbHorizontal.pressedImage", Adjuster_Slider_thumbHorizontal_pressedImage,
			"Adjuster.Slider.thumbHorizontal.disabledImage", Adjuster_Slider_thumbHorizontal_disabledImage,
			"Adjuster.Slider.thumbHorizontal.rolloverImage", Adjuster_Slider_thumbHorizontal_rolloverImage,

			"Adjuster.Slider.thumbIcon", SkinAdjusterSliderThumb,

			"Adjuster.Slider.background", table.getFont("controlFont"),
			"Adjuster.Slider.foreground", table.get("controlText"),
			"Adjuster.Slider.opaque", false,
			"Adjuster.Slider.focusable", true
			];
			table.putDefaults(comDefaults);

			// *** Table
			comDefaults = [
			"Table.background", table.get("control"),
			"Table.foreground", table.get("controlText"),
			"Table.opaque", true,
			"Table.focusable", true,
			"Table.font", table.getFont("controlFont"),
			"Table.selectionBackground", table.get("selectionBackground"),
			"Table.selectionForeground", table.get("selectionForeground"),
			"Table.gridColor", new ASColorUIResource(0xd5d5d5),
			"Table.border", undefined
			];
			table.putDefaults(comDefaults);

			// *** TableHeader
			comDefaults = [
			"TableHeader.foreground", new ASColorUIResource(0xffffff),
			"TableHeader.opaque", false,
			"TableHeader.focusable", true,
			"TableHeader.gridColor", new ASColorUIResource(0xa9a9a9),
			"TableHeader.border", undefined,
			"TableHeader.cellBorder", undefined,
			"TableHeader.sortableCellBorder", undefined,

			"TableHeader.cellBackground", SkinTableHeaderCellBackground,
			"TableHeader.sortableCellBackground", SkinTableHeaderSortableCellBackground,

			"TableHeader.cell.defaultImage", TableHeader_cell_defaultImage,
			"TableHeader.cell.pressedImage", TableHeader_cell_pressedImage,
			"TableHeader.cell.disabledImage", TableHeader_cell_disabledImage,
			"TableHeader.cell.rolloverImage", TableHeader_cell_rolloverImage
			];
			table.putDefaults(comDefaults);

			// *** ToolBar
			comDefaults = [
			"ToolBar.background", table.get("window"),
			"ToolBar.foreground", table.get("windowText"),
			"ToolBar.opaque", false,
			"ToolBar.focusable", false
			];
			table.putDefaults(comDefaults);

			// *** MenuItem
			comDefaults = [
			"MenuItem.opaque", false,
			"MenuItem.selectionForeground", table.get("menuText"),
			"MenuItem.acceleratorSelectionForeground", table.get("menuText"),
			"MenuItem.defaultImage", null,
			"MenuItem.pressedImage", null,
			"MenuItem.disabledImage", null,
			"MenuItem.rolloverImage", MenuItem_rolloverImage,
			"MenuItem.bg", SkinMenuItemBackground,
			"MenuItem.margin", new InsetsUIResource(0, 0, 0, 0)
			];
			table.putDefaults(comDefaults);

			// *** CheckBoxMenuItem
			comDefaults = [
			"CheckBoxMenuItem.opaque", false,
			"CheckBoxMenuItem.selectionForeground", table.get("menuText"),
			"CheckBoxMenuItem.acceleratorSelectionForeground", table.get("menuText"),
			"CheckBoxMenuItem.defaultImage", null,
			"CheckBoxMenuItem.pressedImage", null,
			"CheckBoxMenuItem.disabledImage", null,
			"CheckBoxMenuItem.rolloverImage", CheckBoxMenuItem_rolloverImage,
			"CheckBoxMenuItem.bg", SkinCheckBoxMenuItemBackground,
			"CheckBoxMenuItem.checkIconImage", CheckBoxMenuItem_checkIconImage,
			"CheckBoxMenuItem.checkIcon", SkinCheckBoxMenuItemCheckIcon,
			"CheckBoxMenuItem.margin", new InsetsUIResource(0, 0, 0, 0)
			];
			table.putDefaults(comDefaults);

			// *** RadioButtonMenuItem
			comDefaults = [
			"RadioButtonMenuItem.opaque", false,
			"RadioButtonMenuItem.selectionForeground", table.get("menuText"),
			"RadioButtonMenuItem.acceleratorSelectionForeground", table.get("menuText"),
			"RadioButtonMenuItem.defaultImage", null,
			"RadioButtonMenuItem.pressedImage", null,
			"RadioButtonMenuItem.disabledImage", null,
			"RadioButtonMenuItem.rolloverImage", RadioButtonMenuItem_rolloverImage,
			"RadioButtonMenuItem.bg", SkinRadioButtonMenuItemBackground,
			"RadioButtonMenuItem.checkIconImage", RadioButtonMenuItem_checkIconImage,
			"RadioButtonMenuItem.checkIcon", SkinRadioButtonMenuItemCheckIcon,
			"RadioButtonMenuItem.margin", new InsetsUIResource(0, 0, 0, 0)
			];
			table.putDefaults(comDefaults);

			// *** Menu -- by default the menu will not use decorators, if you need, uncomment the lines below
			comDefaults = [
			"Menu.opaque", false,
			"Menu.selectionForeground", table.get("menuText"),
			"Menu.acceleratorSelectionForeground", table.get("menuText"),
			"Menu.bg", SkinMenuBackground,
			"Menu.selectedImage", MenuItem_rolloverImage,
			"Menu.rolloverImage", MenuItem_rolloverImage,
			"Menu.arrowIconImage", Menu_arrowIconImage,
			"Menu.arrowIcon", SkinMenuArrowIcon
			];
			table.putDefaults(comDefaults);

			// *** PopupMenu
			comDefaults = [
			"PopupMenu.bgImage", PopupMenu_bgImage,
			"PopupMenu.border", new SkinEmptyBorder(2, 2, 2, 2),
			"PopupMenu.bg", GamePopupMenuBackground
			];
			table.putDefaults(comDefaults);

			// *** MenuBar
			comDefaults = [
			"MenuBar.bgImage", MenuBar_bgImage,
			"MenuBar.bg", SkinMenuBarBackground,
			"MenuBar.border", undefined
			];
			table.putDefaults(comDefaults);
		}
	}
}

