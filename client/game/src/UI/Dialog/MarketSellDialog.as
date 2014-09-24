package src.UI.Dialog
{
    import flash.events.*;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.event.*;
    import org.aswing.geom.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Effects.*;
    import src.Objects.Factories.SpriteFactory;
    import src.UI.*;
    import src.Util.StringHelper;

    public class MarketSellDialog extends GameJPanel
	{
		private var lblTitle1:JLabel;
		private var pnlResourcePrices:JPanel;
		private var pnlCrop:JPanel;
		private var lblCropAmount:JLabel;
		private var lblCropPrice:JLabel;
		private var pnlIron:JPanel;
		private var lblIronAmount:JLabel;
		private var lblIronPrice:JLabel;
		private var pnlWood:JPanel;
		private var lblWoodAmount:JLabel;
		private var lblWoodPrice:JLabel;
		private var separator:JSeparator;
		private var lblTitle2:JLabel;
		private var pnlTrade:JPanel;
		private var pnlTrade2:JPanel;
		private var lblTradeAmount:JLabel;
		private var lstResourceType:JComboBox;
		private var lblTradePrice:JLabel;
		private var sldAmount:JSlider;
		private var pnlBottom:JPanel;
		private var btnOk:JButton;
        private var lblMaxOut: JLabelButton;

		public var wood:int;
		public var crop:int;
		public var iron:int;
		
		public var structure:StructureObject;
		public var city:City;
				
		public function MarketSellDialog(structure:StructureObject, wood:int, crop:int, iron:int, onAccept:Function):void
		{
			createUI();
			setIcons();
			
			title = "Sell Resources";
			
			lstResourceType.setListData(Formula.resourcesSellable(structure.level));
			
			this.wood = wood;
			this.crop = crop;
			this.iron = iron;
			this.structure = structure;
			
			lblWoodPrice.setText(wood.toString());
			lblCropPrice.setText(crop.toString());
			lblIronPrice.setText(iron.toString());
			
			var self:MarketSellDialog = this;
			btnOk.addActionListener(function():void
				{
					if (onAccept != null)
						onAccept(self);
				});
			
			if (lstResourceType.getSelectedIndex() == -1)
				lstResourceType.setSelectedIndex(0);
			
			city = Global.map.cities.get(structure.cityId);
			
			onResourceChange(null);
			
			lstResourceType.addActionListener(onResourceChange);
			sldAmount.addEventListener(InteractiveEvent.STATE_CHANGED, onAmountChange);

            lblMaxOut.addActionListener(function(e: Event):void {
                sldAmount.setValue(sldAmount.getMaximum());
            });
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			
			city.addEventListener(City.RESOURCES_UPDATE, onResourceChange);
			structure.addEventListener(SimpleGameObject.OBJECT_UPDATE, onAmountChange);
			
			frame.addEventListener(PopupEvent.POPUP_CLOSED, function(e:PopupEvent):void
				{
					if (city != null)
						city.removeEventListener(City.RESOURCES_UPDATE, onResourceChange);
					if (structure != null)
						structure.removeEventListener(SimpleGameObject.OBJECT_UPDATE, onAmountChange);
				});
			
			Global.gameContainer.showFrame(frame);
			
			return frame;
		}
		
		public function onResourceChange(e:Event):void
		{
			var amount:int;
			
			switch (lstResourceType.getSelectedItem())
			{
				case "Crop": 
					amount = city.resources.crop.getValue();
					break;
				case "Iron": 
					amount = city.resources.iron.getValue();
					break;
				case "Wood": 
					amount = city.resources.wood.getValue();
					break;
				default: 
					return;
			}
			
			var value:int = sldAmount.getValue();
			
			var min:int = Math.min(Formula.RESOURCE_CHUNK, int(amount / Formula.RESOURCE_CHUNK) * Formula.RESOURCE_CHUNK);
			var max:int = Math.min(Formula.resourceMaxTrade(structure.level), int(amount / Formula.RESOURCE_CHUNK) * Formula.RESOURCE_CHUNK);
			
			if (value < min)
				value = min;
			if (value > max)
				value = max;
			
			sldAmount.setValues(value, 0, min, max);
			sldAmount.repaint();
			
			onAmountChange(e);
		}
		
		public function onAmountChange(e: *):void
		{
			var pricePerResource:int = 0;
			switch (lstResourceType.getSelectedItem())
			{
				case "Crop": 
					pricePerResource = crop;
					break;
				case "Iron": 
					pricePerResource = iron;
					break;
				case "Wood": 
					pricePerResource = wood;
					break;
				default: 
					return;
			}
			
			lblTradeAmount.setText(sldAmount.getValue().toString());
			lblTradePrice.setText(Formula.marketSellCost(pricePerResource, sldAmount.getValue()).toString());
		}
		
		public function amount():String
		{
			return lblTradeAmount.getText();
		}
		
		public function resourceType():int
		{
			switch (lstResourceType.getSelectedItem())
			{
				case "Crop": 
					return Resources.TYPE_CROP;
				case "Iron": 
					return Resources.TYPE_IRON;
				case "Wood": 
					return Resources.TYPE_WOOD;
				default: 
					return -1;
			}
		}
		
		public function setIcons():void
		{
			lblCropAmount.setIconTextGap(0);
			lblCropAmount.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_CROP")));
			lblCropPrice.setIconTextGap(0);
			lblCropPrice.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_GOLD")));
			
			lblWoodAmount.setIconTextGap(0);
			lblWoodAmount.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_WOOD")));
			lblWoodPrice.setIconTextGap(0);
			lblWoodPrice.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_GOLD")));
			
			lblIronAmount.setIconTextGap(0);
			lblIronAmount.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_IRON")));
			lblIronPrice.setIconTextGap(0);
			lblIronPrice.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_GOLD")));
			
			lblTradePrice.setIconTextGap(0);
			lblTradePrice.setIcon(new AssetIcon(SpriteFactory.getFlashSprite("ICON_GOLD")));
		}
		
		public function createUI():void
		{
			//component creation
			setBorder(new EmptyBorder(null, new Insets(5, 5, 5, 5)));
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
			
			lblTitle1 = new JLabel();
			lblTitle1.setText("Current Resource Trading Prices");
			lblTitle1.setHorizontalAlignment(AsWingConstants.LEFT);
			
			pnlResourcePrices = new JPanel(new FlowWrapLayout(AsWingConstants.LEFT, 15, 15));
			pnlResourcePrices.setPreferredSize(new IntDimension(250, 75));

			pnlCrop = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0));

			lblCropAmount = new JLabel();
			lblCropAmount.setText("100 = ");
			
			lblCropPrice = new JLabel();

			pnlIron = new JPanel(new FlowLayout(AsWingConstants.LEFT, 0, 0));
			
			lblIronAmount = new JLabel();
			lblIronAmount.setLocation(new IntPoint(5, 5));
			lblIronAmount.setSize(new IntDimension(26, 17));
			lblIronAmount.setText("100 = ");
			
			lblIronPrice = new JLabel();
			lblIronPrice.setLocation(new IntPoint(36, 5));
			lblIronPrice.setSize(new IntDimension(26, 17));
			
			pnlWood = new JPanel();
			pnlWood.setLocation(new IntPoint(152, 23));
			pnlWood.setSize(new IntDimension(30, 17));
			var layout5:FlowLayout = new FlowLayout();
			layout5.setAlignment(AsWingConstants.CENTER);
			layout5.setHgap(0);
			layout5.setVgap(0);
			pnlWood.setLayout(layout5);
			
			lblWoodAmount = new JLabel();
			lblWoodAmount.setSize(new IntDimension(26, 17));
			lblWoodAmount.setText("100 = ");
			
			lblWoodPrice = new JLabel();

			separator = new JSeparator();

			lblTitle2 = new JLabel();
			lblTitle2.setText("Choose resource and amount to sell");
			lblTitle2.setHorizontalAlignment(AsWingConstants.LEFT);
			
			pnlTrade = new JPanel();
			var layout6:BoxLayout = new BoxLayout();
			layout6.setAxis(AsWingConstants.VERTICAL);
			pnlTrade.setLayout(layout6);
			
			pnlTrade2 = new JPanel();
			pnlTrade2.setConstraints("North");
			var layout7:FlowLayout = new FlowLayout();
			layout7.setAlignment(AsWingConstants.CENTER);
			pnlTrade2.setLayout(layout7);
			
			lblTradeAmount = new JLabel();
			lblTradeAmount.setLocation(new IntPoint(89, 5));
			lblTradeAmount.setSize(new IntDimension(26, 17));
			lblTradeAmount.setPreferredSize(new IntDimension(30, 17));
			lblTradeAmount.setHorizontalAlignment(AsWingConstants.RIGHT);
			
			lstResourceType = new JComboBox();
			lstResourceType.setPreferredSize(new IntDimension(60, 21));
			lstResourceType.setLocation(new IntPoint(93, 5));
			lstResourceType.setSize(new IntDimension(40, 22));
			lstResourceType.setEditable(false);
			lstResourceType.setMaximumRowCount(7);
			
			lblTradePrice = new JLabel();
			lblTradePrice.setHorizontalAlignment(AsWingConstants.LEFT);
			lblTradePrice.setLocation(new IntPoint(122, 7));
			lblTradePrice.setSize(new IntDimension(28, 17));
			lblTradePrice.setPreferredSize(new IntDimension(65, 17));
			
			sldAmount = new JSlider();
			sldAmount.setLocation(new IntPoint(0, 27));
			sldAmount.setSize(new IntDimension(200, 18));
			sldAmount.setMajorTickSpacing(Formula.RESOURCE_CHUNK);
			sldAmount.setPaintTicks(true);
			sldAmount.setSnapToTicks(true);

            lblMaxOut = new JLabelButton(StringHelper.localize("TRADE_DIALOG_SET_MAX"), null, AsWingConstants.LEFT);

			pnlBottom = new JPanel();
			pnlBottom.setLocation(new IntPoint(0, 69));
			pnlBottom.setSize(new IntDimension(200, 10));
			var layout8:FlowLayout = new FlowLayout();
			layout8.setAlignment(AsWingConstants.CENTER);
			pnlBottom.setLayout(layout8);
			
			btnOk = new JButton();
			btnOk.setLocation(new IntPoint(86, 5));
			btnOk.setSize(new IntDimension(27, 22));
			btnOk.setText("Sell");
					
			//component layoution
			append(lblTitle1);
			append(pnlResourcePrices);
			append(separator);
			append(lblTitle2);
			append(pnlTrade);
			append(pnlBottom);
			
			pnlResourcePrices.append(pnlCrop);
			pnlResourcePrices.append(pnlIron);
			pnlResourcePrices.append(pnlWood);
			
			pnlCrop.append(lblCropAmount);
			pnlCrop.append(lblCropPrice);
			
			pnlIron.append(lblIronAmount);
			pnlIron.append(lblIronPrice);
			
			pnlWood.append(lblWoodAmount);
			pnlWood.append(lblWoodPrice);
			
			pnlTrade.append(pnlTrade2);
			pnlTrade.append(sldAmount);
			
			pnlTrade2.append(lblTradeAmount);
			pnlTrade2.append(lstResourceType);
			pnlTrade2.append(new JLabel("="));
			pnlTrade2.append(lblTradePrice);
            pnlTrade2.append(lblMaxOut);
			
			pnlBottom.append(btnOk);
		}
	}

}

