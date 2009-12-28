
package src.UI.Sidebars.ObjectInfo.Buttons {
	import flash.display.MovieClip;
	import flash.display.SimpleButton;
	import flash.events.Event;
	import flash.events.MouseEvent;	
	import org.aswing.*;
	import src.Global;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.Actions.ActionButton;
	import src.Objects.Prototypes.UnitPrototype;
	import src.Objects.*;
	import src.UI.Cursors.*;
	import src.UI.Dialog.InfoDialog;
	import src.UI.Dialog.MarketBuyDialog;
	import src.UI.Dialog.MarketSellDialog;
	import src.UI.GameJPanel;
	import src.UI.Tooltips.TextTooltip;

	public class MarketButton extends ActionButton
	{		
		private var toolTip: TextTooltip;
		private var mode: String;
		private var pnlGetPrices: InfoDialog;
		
		public function MarketButton(button: SimpleButton, parentObj: GameObject, mode: String)
		{			
			super(button, parentObj);
			
			this.mode = mode;

			toolTip = new TextTooltip(mode == "sell" ? "Sell Resources" : "Buy Resources");
			
			ui.addEventListener(MouseEvent.CLICK, onMouseClick);
			ui.addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			ui.addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);						
		}
		
		public function onMouseOver(event: MouseEvent):void
		{			
			toolTip.show(this);
		}
		
		public function onMouseOut(event: MouseEvent):void
		{
			toolTip.hide();
		}
		
		public function onMouseClick(MouseEvent: Event):void
		{
			if (enabled)
			{
				pnlGetPrices = InfoDialog.showMessageDialog("Loading", "Getting current market prices...", null, null, true, false, 0);
				
				Global.map.mapComm.Market.getResourcePrices(onReceiveMarketPrices);
			}
		}
		
		public function onReceiveMarketPrices(wood: int, iron: int, crop: int):void
		{
			if (pnlGetPrices) pnlGetPrices.getFrame().dispose();
						
			if (mode == "sell")
			{
				var marketSellDialog: MarketSellDialog = new MarketSellDialog(parentObj as StructureObject, wood, crop, iron, onAcceptSellDialog);
				marketSellDialog.show();
			}
			else
			{
				var marketBuyDialog: MarketBuyDialog = new MarketBuyDialog(parentObj as StructureObject, wood, crop, iron, onAcceptBuyDialog);
				marketBuyDialog.show();
			}
		}
		
		public function onAcceptSellDialog(marketDialog: MarketSellDialog):void
		{
			var count: int;
			
			count = int(marketDialog.amount());					
			
			var originalPrice: int;
			switch (marketDialog.resourceType())
			{
				case Resources.TYPE_WOOD:
					originalPrice = marketDialog.wood;
					break;
				case Resources.TYPE_IRON:
					originalPrice = marketDialog.iron;
					break;
				case Resources.TYPE_CROP:
					originalPrice = marketDialog.crop;
					break;
				default:
					return;
			}
			
			Global.map.mapComm.Market.sellResources(onMarketResponse, marketDialog, parentObj.cityId, parentObj.objectId, marketDialog.resourceType(), count, originalPrice);
		}		
		
		public function onAcceptBuyDialog(marketDialog: MarketBuyDialog):void
		{			
			var count: int;			

			count = int(marketDialog.amount());											
			
			var originalPrice: int;
			switch (marketDialog.resourceType())
			{
				case Resources.TYPE_WOOD:
					originalPrice = marketDialog.wood;
					break;
				case Resources.TYPE_IRON:
					originalPrice = marketDialog.iron;
					break;
				case Resources.TYPE_CROP:
					originalPrice = marketDialog.crop;
					break;
				default:
					return;
			}
			
			Global.map.mapComm.Market.buyResources(onMarketResponse, marketDialog, parentObj.cityId, parentObj.objectId, marketDialog.resourceType(), count, originalPrice);			
		}
		
		public function onMarketResponse(status: int, custom: *):void
		{
			if (status == 0)
				(custom as GameJPanel).getFrame().dispose();
			else			
				GameError.showMessage(status);
		}		
		
		override public function validateButton(): Boolean
		{							
			return enabled;
		}
	}
	
}
