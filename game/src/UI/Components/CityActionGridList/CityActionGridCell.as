package src.UI.Components.CityActionGridList
{
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.CityObject;
	import src.Map.TileLocator;
	import src.Objects.Actions.CurrentAction;
	import src.Objects.Actions.CurrentActiveAction;
	import src.Objects.Actions.CurrentPassiveAction;
	import src.Objects.Actions.PassiveAction;
	import src.Objects.Factories.WorkerFactory;
	import src.Objects.Prototypes.Worker;
	import src.UI.Components.SimpleTooltip;
    import src.Util.DateUtil;
    import src.Util.Util;

	public class CityActionGridCell extends JPanel implements GridListCell{

		private var value: * ;

		private var panel2:JPanel;
		private var icon:JPanel;
		private var panel6:JPanel;
		private var lblAction:JLabel;
		private var lblTime:JLabel;
		private var tooltip: SimpleTooltip;

		public function CityActionGridCell()
		{
			createUI();

			icon.buttonMode = true;
			addEventListener(MouseEvent.CLICK, function(e: MouseEvent):void {
				if (value != null)
				{
					var cityObj: CityObject = value.cityObj;
					var pt: Point = TileLocator.getScreenCoord(cityObj.x, cityObj.y);
					Global.map.selectWhenViewable(cityObj.city.id, cityObj.objectId);
					Global.map.camera.ScrollToCenter(pt.x, pt.y);					
					Util.getFrame(getParent()).dispose();
				}
			});
		}

		public function setCellValue(value:*):void {
			
			if (value.message) {
				panel2.remove(icon);
				lblAction.setText(value.message);
				return;
			}
			
			this.value = value;
		
			tooltip.setEnabled(value.cityObj != null);		

			var currentAction: CurrentAction = value.currentAction as CurrentAction;

			var actionDescription: String = "N/A";
			if (currentAction is CurrentActiveAction)
			{
				var actionWorkerPrototype: Worker = WorkerFactory.getPrototype(value.prototype.workerid);
				if (actionWorkerPrototype != null)
				actionDescription = actionWorkerPrototype.getAction((currentAction as CurrentActiveAction).index).toString();
			}
			else if (currentAction is CurrentPassiveAction)
			actionDescription = (currentAction as CurrentPassiveAction).toString();
			else
			actionDescription = "Unexpected action";

			icon.append(new AssetPane(value.source));

			lblAction.setText(actionDescription);

			updateTime();

			pack();
		}

		public function updateTime() : void {
			if (value == null) return;
			
			var currentAction: CurrentAction = value.currentAction as CurrentAction;

			if (currentAction == null) return;

			var time: Number = Math.max(0, currentAction.endTime - Global.map.getServerTime());

			lblTime.setText(time > 0 ? DateUtil.formatTime(time) : '');
		}

		public function getCellValue():*{
			return value;
		}

		public function getCellComponent():Component{
			return this;
		}

		public function setGridListCellStatus(gridList:GridList, selected:Boolean, index:int):void {
		}

		private function createUI() : void
		{
			var layout0:GridLayout= new GridLayout();
			layout0.setRows(1);
			layout0.setColumns(2);
			setLayout(layout0);

			panel2 = new JPanel();
			var layout1:FlowLayout = new FlowLayout();
			panel2.setLayout(layout1);

			icon = new JPanel();
			icon.setPreferredWidth(50);
			
			tooltip = new SimpleTooltip(this, "Click to go to event");
			tooltip.setEnabled(false);
			
			panel6 = new JPanel();
			panel6.setPreferredSize(new IntDimension(175, 50));
			var layout2:SoftBoxLayout = new SoftBoxLayout();
			layout2.setAxis(AsWingConstants.VERTICAL);
			layout2.setAlign(AsWingConstants.LEFT);
			panel6.setLayout(layout2);

			lblAction = new JLabel();
			lblAction.setHorizontalAlignment(AsWingConstants.LEFT);

			lblTime = new JLabel();
			lblTime.setHorizontalAlignment(AsWingConstants.LEFT);

			//component layout
			panel2.append(icon);
			panel2.append(panel6);

			panel6.append(lblAction);
			panel6.append(lblTime);

			append(panel2);
		}
	}

}

