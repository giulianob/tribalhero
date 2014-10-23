
package src.UI.Sidebars.ObjectInfo.Buttons {
    import flash.events.Event;
    import flash.events.MouseEvent;

    import src.Global;
    import src.Objects.*;
    import src.FeathersUI.Controls.ActionButton;
    import src.Objects.Effects.Formula;
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

		public function MarketButton(parentObj: SimpleGameObject, mode: String)
		{
			super(parentObj, mode == "sell" ? "Sell Resources" : "Buy Resources");

			this.mode = mode;

			toolTip = new TextTooltip(mode == "sell" ? "Sell Resources" : "Buy Resources");

			addEventListener(MouseEvent.CLICK, onMouseClick);
			addEventListener(MouseEvent.MOUSE_OVER, onMouseOver);
			addEventListener(MouseEvent.MOUSE_MOVE, onMouseOver);
			addEventListener(MouseEvent.MOUSE_OUT, onMouseOut);
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
			if (isEnabled)
			{
				pnlGetPrices = InfoDialog.showMessageDialog("Loading", "Getting current market prices...", null, null, true, false, 0);

				Global.mapComm.Market.getResourcePrices(onReceiveMarketPrices);
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

			Global.mapComm.Market.sellResources(onMarketResponse, marketDialog, parentObj.groupId, parentObj.objectId, marketDialog.resourceType(), count, originalPrice);
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

			Global.mapComm.Market.buyResources(onMarketResponse, marketDialog, parentObj.groupId, parentObj.objectId, marketDialog.resourceType(), count, originalPrice);
		}

		public function onMarketResponse(status: int, custom: *):void
		{
			if (status == 0)
				(custom as GameJPanel).getFrame().dispose();
			else
				GameError.showMessage(status);
		}
		override public function validateButton():Boolean 
		{
			if (mode == "sell") {
				return Formula.resourcesSellable((parentObj as StructureObject).level).length > 0;
			}
			return Formula.resourcesBuyable((parentObj as StructureObject).level).length > 0;
		}
	}

}

