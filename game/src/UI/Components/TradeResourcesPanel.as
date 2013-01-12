package src.UI.Components
{
	import flash.events.Event;
	import org.aswing.event.AWEvent;
	import src.Map.City;
	import src.Objects.*;
	import src.Objects.Effects.Formula;
	import src.UI.LookAndFeel.GameLookAndFeel;
	import src.Global;
    import src.Util.StringHelper;
	
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	
	public class TradeResourcesPanel extends JPanel
	{
		private var structure:StructureObject = null;
		private var city:City = null;
		private var prompt:String;
		
		private var lblTitle1:JLabel;
		
		private var pnlResources:JPanel;
		
		private var pnlCrop:JPanel;
		private var lblCropAmount:JAdjuster;
		
		private var pnlIron:JPanel;
		private var lblIronAmount:JAdjuster;
		
		private var pnlWood:JPanel;
		private var lblWoodAmount:JAdjuster;
		
		private var pnlGold:JPanel;
		private var lblGoldAmount:JAdjuster;
        private var lblMaxOut:JLabelButton;
		
		public function TradeResourcesPanel(parentObj:StructureObject, prompt:String = null)
		{
			this.structure = parentObj;
			this.city = Global.map.cities.get(parentObj.cityId);
			this.prompt = prompt;
			
			setBorder(null);
			setLayout(new SoftBoxLayout(AsWingConstants.VERTICAL, 0, AsWingConstants.TOP));
			parentObj.getCorrespondingCityObj().city.addEventListener(City.RESOURCES_UPDATE, onResourceChange);
			draw();
			onResourceChange();
            lblMaxOut.addActionListener(function (e: Event): void {
                lblWoodAmount.setValue(lblWoodAmount.getMaximum());
                lblIronAmount.setValue(lblIronAmount.getMaximum())
                lblCropAmount.setValue(lblCropAmount.getMaximum())
                lblGoldAmount.setValue(lblGoldAmount.getMaximum())
            });
		}
		
		public function dispose():void
		{
			city.removeEventListener(City.RESOURCES_UPDATE, onResourceChange);
		}
		
		private function onResourceChange(e:Event = null):void
		{			
			var sendCapacity:int = Formula.sendCapacity(structure.level);
			
			lblWoodAmount.setMaximum(Math.min(sendCapacity, city.resources.wood.getValue()));
			lblCropAmount.setMaximum(Math.min(sendCapacity, city.resources.crop.getValue()));
			lblGoldAmount.setMaximum(Math.min(sendCapacity, city.resources.gold.getValue()));
			lblIronAmount.setMaximum(Math.min(sendCapacity, city.resources.iron.getValue()));
		}
		
		public function getResource():Resources
		{
			return new Resources(lblCropAmount.getValue(), lblGoldAmount.getValue(), lblIronAmount.getValue(), lblWoodAmount.getValue(), 0);
		}
		
		private function draw(e:Event = null):void
		{
			removeAll();
			
            var pnlTitle: JPanel = new JPanel(new BorderLayout(5));
			lblTitle1 = new JLabel();
			lblTitle1.setSize(new IntDimension(200, 17));
			lblTitle1.setText(StringHelper.localize("TRADE_DIALOG_CHOOSE_RESOURCE_AMOUNT"));
			lblTitle1.setHorizontalAlignment(AsWingConstants.LEFT);
            lblTitle1.setConstraints("Center");
            
            lblMaxOut = new JLabelButton(StringHelper.localize("TRADE_DIALOG_SET_MAX"), null, AsWingConstants.RIGHT);
            lblMaxOut.setConstraints("East");
			
			pnlResources = new JPanel();
			pnlResources.setLayout(new FlowLayout(AsWingConstants.LEFT, 10, 10));
			
			pnlCrop = new JPanel();
			pnlCrop.setLayout(new FlowLayout(AsWingConstants.LEFT, 5, 0));
			
			lblCropAmount = new JAdjuster(3);
			lblCropAmount.setPreferredWidth(65);
			lblCropAmount.setMinimum(0);
			lblCropAmount.setValue(0);
			
			pnlIron = new JPanel();
			pnlIron.setLayout(new FlowLayout(AsWingConstants.LEFT, 5, 0));
			
			lblIronAmount = new JAdjuster(3);
			lblIronAmount.setPreferredWidth(65);
			lblIronAmount.setMinimum(0);
			lblIronAmount.setValue(0);
			
			pnlWood = new JPanel();
			pnlWood.setLayout(new FlowLayout(AsWingConstants.CENTER, 5, 0));
			
			lblWoodAmount = new JAdjuster(3);
			lblWoodAmount.setPreferredWidth(65);
			lblWoodAmount.setMinimum(0);
			lblWoodAmount.setValue(0);
			
			pnlGold = new JPanel();
			pnlGold.setLayout(new FlowLayout(AsWingConstants.LEFT, 5, 0));
			
			lblGoldAmount = new JAdjuster(3);
			lblGoldAmount.setPreferredWidth(65);
			lblGoldAmount.setMinimum(0);
			lblGoldAmount.setValue(0);
			
			//component layoution
            pnlTitle.appendAll(lblTitle1, lblMaxOut);            			
			pnlResources.appendAll(pnlGold, pnlWood, pnlCrop, pnlIron);
            appendAll(pnlTitle, pnlResources);		
			
			var icon:AssetPane = new AssetPane(new ICON_GOLD());
			new SimpleTooltip(icon, "Gold");
			pnlGold.append(icon);
			pnlGold.append(lblGoldAmount);
			
			icon = new AssetPane(new ICON_CROP());
			new SimpleTooltip(icon, "Crop");
			pnlCrop.append(icon);
			pnlCrop.append(lblCropAmount);
			
			icon = new AssetPane(new ICON_IRON());
			new SimpleTooltip(icon, "Iron");
			pnlIron.append(icon);
			pnlIron.append(lblIronAmount);
			
			icon = new AssetPane(new ICON_WOOD());
			new SimpleTooltip(icon, "Wood");
			pnlWood.append(icon);
			pnlWood.append(lblWoodAmount);
		}
	
	}

}