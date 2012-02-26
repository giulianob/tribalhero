package src.UI.Components
{
	import flash.events.Event;
	import flash.events.MouseEvent;
	import org.aswing.AssetIcon;
	import org.aswing.AsWingConstants;
	import org.aswing.border.EmptyBorder;
	import org.aswing.border.LineBorder;
	import org.aswing.EmptyLayout;
	import org.aswing.FlowLayout;
	import org.aswing.FlowWrapLayout;
	import org.aswing.geom.IntDimension;
	import org.aswing.geom.IntPoint;
	import org.aswing.Icon;
	import org.aswing.Insets;
	import org.aswing.JFrame;
	import org.aswing.JLabel;
	import org.aswing.JPanel;
	import src.Constants;
	import src.Global;
	import src.Map.City;
	import src.UI.Dialog.CityEventDialog;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.UI.Tooltips.ResourcesTooltip;

	public class ResourcesContainer extends JPanel
	{
		private var tooltip: ResourcesTooltip;
		private var frame: JFrame;

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
			var resourceLabelMaker: Function = function(value: int, max : int, icon: Icon = null, labelWidth: int = 30) : JLabel {
				var label: JLabel = new JLabel(value.toString(), icon);
				if (max != -1 && value >= max)
				GameLookAndFeel.changeClass(label, "Label.success Label.small");
				else
				GameLookAndFeel.changeClass(label, "Tooltip.text Label.small");

				label.mouseEnabled = false;
				label.setPreferredSize(new IntDimension(labelWidth, 16));				
				label.mouseChildren = false;
				label.setIconTextGap(0);
				label.setHorizontalTextPosition(AsWingConstants.RIGHT);
				label.setHorizontalAlignment(AsWingConstants.LEFT);
				
				return label;
			};

			var selectedCity: City = Global.gameContainer.selectedCity;

			setLayout(new FlowLayout(AsWingConstants.LEFT, 0, 0, false));
			removeAll();

			append(resourceLabelMaker(selectedCity.resources.labor.getValue(), -1, new AssetIcon(new ICON_LABOR()), 50));
			append(resourceLabelMaker(selectedCity.resources.gold.getValue(), -1, new AssetIcon(new ICON_GOLD()), 61));
			append(resourceLabelMaker(selectedCity.resources.wood.getValue(), selectedCity.resources.wood.getLimit(), new AssetIcon(new ICON_WOOD()), 61));
			append(resourceLabelMaker(selectedCity.resources.crop.getValue(), selectedCity.resources.crop.getLimit(), new AssetIcon(new ICON_CROP()), 61));
			append(resourceLabelMaker(selectedCity.resources.iron.getValue(), selectedCity.resources.iron.getLimit(), new AssetIcon(new ICON_IRON()), 56));

			if (!frame)
			{
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

