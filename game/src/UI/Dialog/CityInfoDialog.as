/**
 * ...
 * @author Default
 * @version 0.1
 */

package src.UI.Dialog {
	import fl.controls.dataGridClasses.DataGridColumn;
	import flash.display.*;
	import flash.events.MouseEvent;
	import flash.events.Event;
	import fl.data.DataProvider;
	import flash.text.*;
	import src.Global;
	import src.Map.City;
	import src.Map.Map;
	import src.Objects.Factories.*;
	import src.Objects.Prototypes.*;
	import src.Objects.*;
	import src.UI.Components.TroopTileList;
	import src.UI.Cursors.GroundAttackCursor;
	import src.UI.Cursors.GroundReinforceCursor;
	import src.UI.PaintBox;
	import src.UI.Sidebars.CursorCancel.CursorCancelSidebar;
	import src.UI.Components.TileListDragDrop.*;
	import src.Constants;
	import src.Objects.Troop.*;
	import src.Util.BinaryList.*;

	public class CityInfoDialog extends Dialog implements IDisposable {
		private var ui: CityInfoDialog_base = new CityInfoDialog_base();

		private var city: City;
		private var map: Map;
		private var resourcesPb: PaintBox;
		private var unitsInnerContainer: Sprite;
		private var tilelists: Array = new Array();

		public function CityInfoDialog() {
			addChild(ui);
			resize(480, 500);

			ui.btnLocate.addEventListener(MouseEvent.CLICK, onClickLocate);
			ui.btnLocateTroop.addEventListener(MouseEvent.CLICK, onClickLocateTroop);
			ui.btnRetreat.addEventListener(MouseEvent.CLICK, onClickRetreatTroop);
			ui.btnAttack.addEventListener(MouseEvent.CLICK, onClickAttack);
			ui.btnReinforce.addEventListener(MouseEvent.CLICK, onClickReinforce);
			ui.btnManageTroop.addEventListener(MouseEvent.CLICK, manageTroop);
			ui.dgTroops.addEventListener(Event.CHANGE, troopSelected);

			ui.btnManageTroop.visible = false;
			ui.btnRetreat.visible = false;
		}

		public function init(map: Map, city: City, onClose: Function):void
		{
			setOnClose(onClose);

			this.map = map;
			this.city = city;

			city.addEventListener(City.RESOURCES_UPDATE, onResourcesUpdate);
			city.troops.addEventListener(BinaryListEvent.CHANGED, onTroopUpdate);

			draw();
		}

		private function draw():void
		{
			ui.btnManageTroop.visible = false;
			ui.btnLocateTroop.visible = false;
			ui.txtState.mouseEnabled = false;

			if (unitsInnerContainer)
			{
				ui.unitsContainer.removeChild(unitsInnerContainer);
				unitsInnerContainer = null;
			}

			ui.lblCityName.text = city.name;

			var selectedId: int = 1;
			var selectedIndex: int = -1;
			if (ui.dgTroops.selectedIndex > -1)
			{
				selectedId = ui.dgTroops.dataProvider.getItemAt(ui.dgTroops.selectedIndex).id;
			}

			ui.dgTroops.dataProvider = new DataProvider();
			ui.dgTroops.columns = [ new DataGridColumn("Troops") ];

			for (var i: int = 0; i < city.troops.size(); i++)
			{
				var troop: Troop = city.troops.getByIndex(i);

				var item: Object;				
				item = { Troops: troop.getNiceId(), id: troop.id, data: troop };				

				ui.dgTroops.dataProvider.addItem(item);

				if (troop.id == selectedId)
				{
					selectedIndex = i;
				}
			}

			if (selectedIndex != -1)
			{
				ui.dgTroops.selectedIndex = selectedIndex;
				ui.dgTroops.dispatchEvent(new Event(Event.CHANGE));
			}

			displayResources();
		}

		public function onClickAttack(event: MouseEvent):void
		{
			var attackTroopDialog: AttackTroopDialog = new AttackTroopDialog(city, city.troops.getDefaultTroop(), [Formation.Attack], onSendTroopAttack);
			map.gameContainer.closeDialog(this);
			attackTroopDialog.show();
		}

		public function onClickLocate(event: MouseEvent):void
		{
			map.gameContainer.map.gameContainer.camera.ScrollTo(city.MainBuilding.x * Constants.tileW - Constants.screenW / 2, city.MainBuilding.y * Constants.tileH / 2 - Constants.screenH / 2);
			Global.map.selectWhenViewable(city.id, city.MainBuilding.objectId);
			map.gameContainer.closeDialog(this);
		}

		public function onClickLocateTroop(event: MouseEvent):void
		{
			if (ui.dgTroops.selectedIndex == -1)
			return;

			var troop: Troop = (ui.dgTroops.dataProvider.getItemAt(ui.dgTroops.selectedIndex).data as Troop);
			Global.map.gameContainer.map.gameContainer.camera.ScrollTo(troop.x * Constants.tileW - Constants.screenW / 2, troop.y * Constants.tileH / 2 - Constants.screenH / 2);
			map.gameContainer.closeDialog(this);
		}

		public function onClickRetreatTroop(event: MouseEvent):void
		{
			if (ui.dgTroops.selectedIndex == -1)
			return;

			var troop: Troop = (ui.dgTroops.dataProvider.getItemAt(ui.dgTroops.selectedIndex).data as Troop);
			map.mapComm.Troop.retreat(city.id, troop.id);
		}

		public function onClickReinforce(event: MouseEvent):void
		{
			var reinforceTroopDialog: ReinforceTroopDialog = new ReinforceTroopDialog(city, city.troops.getDefaultTroop(), [Formation.Defense], onSendTroopReinforce);
			map.gameContainer.closeDialog(this);
			reinforceTroopDialog.show();
		}

		public function onSendTroopReinforce(dialog: ReinforceTroopDialog):void
		{
			dialog.getFrame().dispose();

			var troop: Troop = dialog.getTroop();
			if (troop.getIndividualUnitCount() == 0)
			return;

			var cursor: GroundReinforceCursor = new GroundReinforceCursor();

			cursor.init(troop, city.id);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
			map.gameContainer.setSidebar(sidebar);
		}

		public function onSendTroopAttack(dialog: AttackTroopDialog):void
		{
			dialog.getFrame().dispose();

			var troop: Troop = dialog.getTroop();
			if (troop.getIndividualUnitCount() == 0)
			{
				return;
			}

			var cursor: GroundAttackCursor = new GroundAttackCursor();

			cursor.init(troop, dialog.getMode(), city.id);

			var sidebar: CursorCancelSidebar = new CursorCancelSidebar();
			map.gameContainer.setSidebar(sidebar);
		}

		public function troopSelected(e: Event):void
		{
			ui.btnManageTroop.visible = false;
			ui.btnLocateTroop.visible = false;
			ui.txtState.mouseEnabled = false;
			ui.btnRetreat.visible = false;

			if (unitsInnerContainer)
			{
				ui.unitsContainer.removeChild(unitsInnerContainer);
				unitsInnerContainer = null;

				for each(var ts: TroopTileList in tilelists)
				ts.removeEventListener(TileListDragDropEvent.DRAG_DROPPED, ts.onDragDropped);

				tilelists = new Array();
			}

			unitsInnerContainer = new Sprite();
			ui.unitsContainer.addChild(unitsInnerContainer);

			var selectedItem: * = e.target.selectedItem;
			var troop: Troop = selectedItem.data;

			switch (troop.state)
			{
				case Troop.BATTLE:
					ui.txtState.text = "Attacking";
					if (troop.id > 1) ui.btnLocateTroop.visible = true;
				break;
				case Troop.BATTLE_STATIONED:
					ui.txtState.text = "Defending";
					if (troop.id > 1) ui.btnLocateTroop.visible = true;
				break;
				case Troop.MOVING:
					ui.txtState.text = "Moving";
					ui.btnLocateTroop.visible = true;
				break;
				case Troop.STATIONED:
					ui.txtState.text = "Stationed";
					ui.btnLocateTroop.visible = true;
					ui.btnRetreat.visible = (Constants.playerId == troop.playerId);
				break;
				default:
					ui.txtState.text = "Idle";
				break;
			}

			//tilelists = Troop.getGridContainer(map, city.template, troop, ui.unitsContainer, unitsInnerContainer);

			unitsInnerContainer.y = 30;

			ui.btnManageTroop.visible = (troop.id == 1 && troop.state == Troop.IDLE);
		}

		public function manageTroop(e: MouseEvent) :void
		{
			var unitMove: UnitMoveDialog = new UnitMoveDialog(city, onManageTroop);
			unitMove.show();
		}

		public function onManageTroop(dialog: UnitMoveDialog):void
		{
			dialog.getFrame().dispose();

			var troop: Troop = dialog.getTroop();
			if (troop.getIndividualUnitCount() == 0)
			return;

			map.mapComm.Troop.moveUnit(city.id, troop);
		}

		public function displayResources():void
		{
			return;
			if (resourcesPb != null)
			{
				ui.resourcesContainer.removeChild(resourcesPb);
				resourcesPb.dispose();
				resourcesPb = null;
			}

			var layout: XML =
			<PaintBox width="-1" color="0x000000" bordercolor="0xCCCCCC" bgstyle="rounded" bgcolor="0xFFFFFF">
			<Row valign="middle">
			<Column>
			<Text tooltip="Gold">{city.resources.gold}</Text>
			</Column>
			<Column width="20">
			<Image tooltip="Gold">ICON_GOLD</Image>
			</Column>
			<Column>
			<Text tooltip="Iron">{city.resources.iron}</Text>
			</Column>
			<Column width="25">
			<Image tooltip="Iron">ICON_IRON</Image>
			</Column>
			<Column>
			<Text tooltip="Wood">{city.resources.wood}</Text>
			</Column>
			<Column width="25">
			<Image tooltip="Wood">ICON_WOOD</Image>
			</Column>
			<Column>
			<Text tooltip="Crop">{city.resources.crop}</Text>
			</Column>
			<Column>
			<Image tooltip="Crop">ICON_CROP</Image>
			</Column>
			</Row>
			</PaintBox>;

			resourcesPb = new PaintBox(layout);
			ui.resourcesContainer.addChild(resourcesPb);
		}

		public function dispose():void
		{
			if (city != null)
			{
				city.troops.removeEventListener(BinaryListEvent.CHANGED, onTroopUpdate);
				city.removeEventListener(City.RESOURCES_UPDATE, onResourcesUpdate);
				ui.btnLocate.removeEventListener(MouseEvent.CLICK, onClickLocate);
				ui.btnAttack.removeEventListener(MouseEvent.CLICK, onClickAttack);
				ui.btnReinforce.removeEventListener(MouseEvent.CLICK, onClickReinforce);
				ui.btnManageTroop.removeEventListener(MouseEvent.CLICK, manageTroop);
			}

			if (resourcesPb != null)
			{
				resourcesPb.dispose();
				resourcesPb = null;
			}
		}

		public function onTroopUpdate(event: Event):void
		{
			draw();
		}

		public function onResourcesUpdate(event: Event):void
		{
			displayResources();
		}
	}

}

