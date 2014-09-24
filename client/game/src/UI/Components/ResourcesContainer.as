package src.UI.Components
{
    import flash.display.DisplayObject;
    import flash.events.Event;
    import flash.events.MouseEvent;

    import org.aswing.AsWingConstants;
    import org.aswing.AssetIcon;
    import org.aswing.FlowLayout;
    import org.aswing.Insets;
    import org.aswing.JFrame;
    import org.aswing.JLabel;
    import org.aswing.JPanel;
    import org.aswing.border.EmptyBorder;
    import org.aswing.geom.IntDimension;

    import src.Constants;
    import src.Global;
    import src.Map.City;
    import src.Objects.Factories.SpriteFactory;
    import src.UI.Dialog.CityEventDialog;
    import src.UI.LookAndFeel.GameLookAndFeel;
    import src.UI.Tooltips.ResourcesTooltip;

    public class ResourcesContainer extends JPanel
	{
		private var tooltip: ResourcesTooltip;
		private var frame: JFrame;
		private var labels: Object = new Object();
		
		public function ResourcesContainer()
		{
			var me: JPanel = this;
			
			addEventListener(MouseEvent.CLICK, function(e: Event): void {
				var cityDialog: CityEventDialog = new CityEventDialog(Global.gameContainer.selectedCity);
				cityDialog.show();
			});

			addEventListener(MouseEvent.MOUSE_MOVE, function(e: Event) : void {
				if (!tooltip)
					tooltip = new ResourcesTooltip(Global.gameContainer.selectedCity);

				tooltip.show(me);
			});

			addEventListener(MouseEvent.MOUSE_OUT, function(e: Event) : void {
				if (!tooltip)
					return;
				
				tooltip.hide();
				tooltip = null;
			});
		}

		public function getFrame(): JFrame {
			return frame;
		}

		public function displayResources():void
		{			
			var resourceLabelMaker: Function = function(key: String, value: int, max : int, iconClass: String = null, labelWidth: int = 30) : JLabel {
				
				var label: JLabel = labels[key];
				
				if (!label) {
					label = new JLabel(value.toString(), !iconClass ? null : new AssetIcon((SpriteFactory.getFlashSprite(iconClass)) as DisplayObject));
					label.mouseEnabled = false;
					label.setPreferredSize(new IntDimension(labelWidth, 16));				
					label.mouseChildren = false;
					label.setIconTextGap(0);
					label.setHorizontalTextPosition(AsWingConstants.RIGHT);
					label.setHorizontalAlignment(AsWingConstants.LEFT);	
					labels[key] = label;
				}
				else {
					label.setText(value.toString());
				}
				
				if (max != -1 && value >= max) {
					GameLookAndFeel.changeClass(label, "Label.success Label.small", true);
				}
				else {
					GameLookAndFeel.changeClass(label, "Tooltip.text Label.small", true);
				}			
				
				return label;
			};

			var selectedCity: City = Global.gameContainer.selectedCity;

			resourceLabelMaker("labor", selectedCity.resources.labor.getValue(), -1, "ICON_LABOR", 50);
			resourceLabelMaker("gold", selectedCity.resources.gold.getValue(), -1, "ICON_GOLD", 61);
			resourceLabelMaker("wood", selectedCity.resources.wood.getValue(), selectedCity.resources.wood.getLimit(), "ICON_WOOD", 61);
			resourceLabelMaker("crop", selectedCity.resources.crop.getValue(), selectedCity.resources.crop.getLimit(), "ICON_CROP", 61);
			resourceLabelMaker("iron", selectedCity.resources.iron.getValue(), selectedCity.resources.iron.getLimit(), "ICON_IRON", 56);

			if (!frame)
			{
				append(labels["labor"]);
				append(labels["gold"]);
				append(labels["wood"]);
				append(labels["crop"]);
				append(labels["iron"]);
				
				setLayout(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
							
				frame = new JFrame(null, "", false);
				frame.name = "Resources Container Frame";
				frame.setContentPane(this);
				frame.setBorder(new EmptyBorder(null, new Insets(0, 0, 0, 0)));
				frame.setBackgroundDecorator(null);
				frame.setTitleBar(null);
				frame.setDragable(false);
				frame.setClosable(false);
				frame.setResizable(false);
				frame.show();			
				
				frame.pack();
				
				frame.setLocationXY(Constants.screenW - frame.getWidth(), 14);
			}			
			
			frame.pack();
		}

	}

}

