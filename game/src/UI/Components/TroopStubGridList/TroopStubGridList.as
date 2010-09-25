package src.UI.Components.TroopStubGridList
{
	import flash.display.DisplayObject;
	import org.aswing.border.EmptyBorder;
	import org.aswing.ext.GeneralGridListCellFactory;
	import org.aswing.ext.GridList;
	import org.aswing.ext.GridListItemEvent;
	import org.aswing.Insets;
	import org.aswing.VectorListModel;
	import src.Map.City;
	import src.Objects.Effects.Formula;
	import src.Objects.Factories.TroopFactory;
	import src.Objects.Troop.*;
	import src.UI.Dialog.TroopStubDialog;
	import src.UI.Tooltips.*;

	public class TroopStubGridList extends GridList
	{
		private var templates: UnitTemplateManager;
		private var formationType: int;
		private var tooltip: Tooltip;
		private var city: City;

		public function TroopStubGridList(city: City)
		{
			super(new VectorListModel(), new GeneralGridListCellFactory(TroopStubGridCell), 4, 0);

			this.city = city;
			setBorder(new EmptyBorder(null, new Insets(8, 3, 8, 3)));

			setTileWidth(160);
			setTileHeight(72);
			setHGap(0);
			setVGap(0);

			addEventListener(GridListItemEvent.ITEM_ROLL_OVER, onItemRollOver);
			addEventListener(GridListItemEvent.ITEM_ROLL_OUT, onItemRollOut);
			addEventListener(GridListItemEvent.ITEM_CLICK, onItemClick);
		}

		public function onItemClick(event: GridListItemEvent):void
		{
			var dp: TroopStubGridCell = event.getCell() as TroopStubGridCell;
			var troop: TroopStub = dp.getCellValue().troop;

			var troopStubDialog: TroopStubDialog = new TroopStubDialog(city, troop);
			troopStubDialog.show();
		}

		public function onItemRollOver(event: GridListItemEvent):void
		{
			var dp: TroopStubGridCell = event.getCell() as TroopStubGridCell;

			var troop: TroopStub = dp.getCellValue().troop;

			var troopTooltip: TroopStubTooltip = new TroopStubTooltip(city, troop);
			troopTooltip.show(dp);

			this.tooltip = troopTooltip;
		}

		public function onItemRollOut(event: GridListItemEvent):void
		{
			if (tooltip)
			tooltip.hide();

			tooltip = null;
		}

		public function removeStub(troop: TroopStub) : void {
			var model: VectorListModel = getModel() as VectorListModel;

			for (var i: int = 0; i < model.getSize(); i++) {
				var iter_troop: TroopStub = model.get(i).troop;
				if (TroopStub.compareCityIdAndTroopId(iter_troop, [troop.cityId, troop.id]) == 0) {
					model.removeAt(i);
					break;
				}
			}

			if (this.tooltip) {
				this.tooltip.hide();
				this.tooltip = null;
			}
		}

		public function addStub(troop: TroopStub) : TroopStubGridCell {
			var model: VectorListModel = getModel() as VectorListModel;

			var icon: DisplayObject = TroopFactory.getStateSprite(troop.state, Formula.movementIconTroopSize(troop));

			model.append( { source: icon, troop: troop, id: troop.id, cityId: troop.cityId } );

			model.sortOn(["cityId", "id"], Array.NUMERIC);

			if (this.tooltip) {
				this.tooltip.hide();
				this.tooltip = null;
			}

			return getCellByIndex(getModel().getSize() - 1) as TroopStubGridCell;
		}
	}

}

