package src.UI.Components 
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import org.aswing.AsWingConstants;
	import org.aswing.FlowLayout;
	import org.aswing.Icon;
	import org.aswing.JLabel;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.UI.GameLookAndFeel;	
	import src.UI.GameJBox;
	import src.UI.Tooltips.ResourcesTooltip;
	
	public class ResourcesContainer extends GameJBox
	{
		private var tooltip: ResourcesTooltip;
		
		public function ResourcesContainer() 
		{
			var me: GameJBox = this;		
			
			addEventListener(MouseEvent.MOUSE_MOVE, function(e: Event) : void {
				if (!tooltip) 
					tooltip = new ResourcesTooltip(Global.gameContainer.selectedCity);
					
				tooltip.show(me);
			});
			
			addEventListener(MouseEvent.MOUSE_OUT, function(e: Event) : void {
				if (tooltip) {
					tooltip.hide();				
					tooltip = null;
				}
			});									
		}
		
		public function displayResources():void
		{																
			var resourceLabelMaker: Function = function(value: int, max : int, icon: Icon = null) : JLabel {				
				var label: JLabel = new JLabel(value.toString(), icon);
				if (max != -1 && value >= max)
					GameLookAndFeel.changeClass(label, "Label.success Label.small");
				else
					GameLookAndFeel.changeClass(label, "Tooltip.text Label.small");
				
				label.mouseEnabled = false;
				label.mouseChildren = false;
				label.setIconTextGap(0);			
				label.setHorizontalTextPosition(AsWingConstants.RIGHT);				
				
				return label;
			};						
								
			var selectedCity: City = Global.gameContainer.selectedCity;
			
			setLayout(new FlowLayout(AsWingConstants.LEFT, 10, 5, true));
			removeAll();
			
			append(resourceLabelMaker(selectedCity.resources.labor.getValue(), -1, new AssetIcon(new ICON_LABOR())));
			append(resourceLabelMaker(selectedCity.resources.gold.getValue(), -1, new AssetIcon(new ICON_GOLD())));			
			append(resourceLabelMaker(selectedCity.resources.wood.getValue(), selectedCity.resources.wood.getLimit(), new AssetIcon(new ICON_WOOD())));			
			append(resourceLabelMaker(selectedCity.resources.crop.getValue(), selectedCity.resources.crop.getLimit(), new AssetIcon(new ICON_CROP())));			
			append(resourceLabelMaker(selectedCity.resources.iron.getValue(), selectedCity.resources.iron.getLimit(), new AssetIcon(new ICON_IRON())));			
			
			if (!getFrame())
			{
				show();										
			}
					
			getFrame().pack();			
			getFrame().setLocationXY(Constants.screenW - getFrame().getWidth() + 10, 3);								
		}		
		
	}

}