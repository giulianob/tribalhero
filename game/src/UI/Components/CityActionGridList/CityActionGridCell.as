package src.UI.Components.CityActionGridList
{
    import flash.display.DisplayObject;
    import flash.events.MouseEvent;

    import org.aswing.*;
    import org.aswing.ext.*;
    import org.aswing.geom.*;

    import src.Global;
    import src.Map.CityObject;
    import src.Objects.Actions.CurrentAction;
    import src.Objects.Actions.CurrentActiveAction;
    import src.Objects.Actions.CurrentPassiveAction;
    import src.Objects.Factories.WorkerFactory;
    import src.Objects.Prototypes.Worker;
    import src.UI.Components.SimpleTooltip;
    import src.Util.DateUtil;
    import src.Util.Util;

    public class CityActionGridCell extends JPanel implements GridListCell{
        private const iconSize: Number = 50;

		private var value: * ;

		private var icon:JPanel;
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
					Global.map.selectWhenViewable(cityObj.city.id, cityObj.objectId);
					Global.map.camera.ScrollToCenter(cityObj.primaryPosition.toScreenPosition());
					Util.getFrame(getParent()).dispose();
				}
			});
		}

		public function setCellValue(value:*):void {
			
			if (value.message) {
				remove(icon);
				lblAction.setText(value.message);
				return;
			}
			
			this.value = value;
		
			tooltip.setEnabled(value.cityObj != null);		

			var currentAction: CurrentAction = value.currentAction as CurrentAction;

			var actionDescription: String = "N/A";
			if (currentAction is CurrentActiveAction)
			{
				var actionWorkerPrototype: Worker = WorkerFactory.getPrototype(value.objPrototype.workerid);
				if (actionWorkerPrototype != null)
				actionDescription = actionWorkerPrototype.getAction((currentAction as CurrentActiveAction).index).toString();
			}
			else if (currentAction is CurrentPassiveAction)
			actionDescription = (currentAction as CurrentPassiveAction).toString();
			else
			actionDescription = "Unexpected action";

            var iconSprite: DisplayObject = value.source;
            Util.resizeSprite(iconSprite, iconSize, iconSize);
			icon.append(new AssetPane(iconSprite));

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

		private function createUI() : void {
            var layout1: FlowLayout = new FlowLayout(AsWingConstants.LEFT, 5, 0, false);
            setLayout(layout1);

            icon = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 0, 0, false));
            icon.setPreferredSize(new IntDimension(iconSize, iconSize));

            tooltip = new SimpleTooltip(this, "Click to go to event");
            tooltip.setEnabled(false);

            var panel6: JPanel = new JPanel();
            panel6.setPreferredSize(new IntDimension(175, 50));
            var layout2: SoftBoxLayout = new SoftBoxLayout();
            layout2.setAxis(AsWingConstants.VERTICAL);
            layout2.setAlign(AsWingConstants.LEFT);
            panel6.setLayout(layout2);

            lblAction = new JLabel();
            lblAction.setHorizontalAlignment(AsWingConstants.LEFT);

            lblTime = new JLabel();
            lblTime.setHorizontalAlignment(AsWingConstants.LEFT);

            panel6.append(lblAction);
            panel6.append(lblTime);

            append(icon);
            append(panel6);
        }
	}

}

