package src.UI.Components.NotificationGridList 
{
	import flash.display.DisplayObjectContainer;
	import flash.events.Event;
	import flash.events.MouseEvent;
	import flash.geom.Point;
	import flash.utils.Timer;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Global;
	import src.Map.MapUtil;
	import src.Objects.Actions.Notification;
	import src.Objects.Actions.PassiveAction;
	import src.Objects.Factories.WorkerFactory;
	import src.Objects.Prototypes.Worker;
	import src.UI.Components.SimpleTooltip;
	import src.Util.Util;

public class NotificationGridCell extends JPanel implements GridListCell{	
	
	private var value: * ;	
	
	private var panel2:JPanel;
	private var icon:JPanel;
	private var panel6:JPanel;
	private var lblAction:JLabel;
	private var lblDescription:JLabel;
	private var lblTime:JLabel;
	
	public function NotificationGridCell() 
	{		
		createUI();
		
		icon.buttonMode = true;
		icon.addEventListener(MouseEvent.CLICK, function(e: MouseEvent):void {
			if (value != null)
			{				
				var notification: Notification = value.notification as Notification;
				Global.map.mapComm.City.gotoNotificationLocation(value.cityId, notification.cityId, notification.actionId);
				Global.map.selectWhenViewable(notification.cityId, notification.objectId);
				Util.getFrame(getParent()).dispose();				
			}
		});
	}
	
	public function setCellValue(value:*):void{
		this.value = value;
		
		var notification: Notification = value.notification as Notification;		
		
		var actionDescription: String = PassiveAction.toString(notification.type);			
		
		var passiveIcon: DisplayObjectContainer = PassiveAction.getIcon(notification.type);
		Util.centerSprite(passiveIcon);
		icon.append(new AssetPane(passiveIcon));
		
		lblAction.setText(actionDescription);
		lblAction.setToolTipText(lblAction.getText());
		lblDescription.setText(value.local ? "(Local)" : "(Remote)");
		updateTime();
		
		pack();
	}
	
	public function updateTime() : void {
		var notification: Notification = value.notification as Notification;
		
		if (notification == null) return;
		
		var time: Number = Math.max(0, notification.endTime - Global.map.getServerTime());
		
		lblTime.setText(time > 0 ? Util.formatTime(time) : '--:--:--');
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
		icon.setPreferredWidth(40);
		new SimpleTooltip(icon, "Go to event");
		
		panel6 = new JPanel();
		panel6.setPreferredSize(new IntDimension(175, 70));
		var layout2:SoftBoxLayout = new SoftBoxLayout();
		layout2.setAxis(AsWingConstants.VERTICAL);
		layout2.setAlign(AsWingConstants.LEFT);
		panel6.setLayout(layout2);
		
		lblAction = new JLabel();
		lblAction.setHorizontalAlignment(AsWingConstants.LEFT);		
		
		lblDescription = new JLabel();
		lblDescription.setHorizontalAlignment(AsWingConstants.LEFT);		
		
		lblTime = new JLabel();				
		lblTime.setHorizontalAlignment(AsWingConstants.LEFT);		
				
		//component layout		
		panel2.append(icon);
		panel2.append(panel6);
		
		panel6.append(lblAction);
		panel6.append(lblDescription);
		panel6.append(lblTime);
		
		append(panel2);
	}
}
	
}
