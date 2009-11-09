package src.UI.Dialog{

import org.aswing.*;
import org.aswing.border.*;
import org.aswing.geom.*;
import org.aswing.colorchooser.*;
import org.aswing.ext.*;
import src.Global;
import src.Map.City;
import src.UI.Components.CityActionGridList.CityActionGridList;
import src.UI.Components.NotificationGridList.NotificationGridList;
import src.UI.GameJPanel;

/**
 * CityEventDialog
 */
public class CityEventDialog extends GameJPanel{
	
	//members define
	private var panel2:JPanel;
	private var panel3:JPanel;
	
	private var gridLocalActions: CityActionGridList;	
	private var gridNotifications: NotificationGridList;	
	
	/**
	 * CityEventDialog Constructor
	 */
	public function CityEventDialog(city: City) {
		title = "City Events";
		gridLocalActions = new CityActionGridList(city, 530);
		gridNotifications = new NotificationGridList(city, 530);
		createUI();					
	}		
	
	public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
	{
		super.showSelf(owner, modal, onClose, dispose);
		Global.gameContainer.showFrame(frame);
		return frame;
	}			
	
	private function dispose(): void {
		gridLocalActions.dispose();
		gridNotifications.dispose();
	}
	
	private function createUI(): void {
		//component creation
		setSize(new IntDimension(500, 410));
		var layout0:SoftBoxLayout = new SoftBoxLayout();
		layout0.setAxis(AsWingConstants.VERTICAL);
		layout0.setGap(10);
		setLayout(layout0);
		
		panel2 = new JPanel();
		panel2.setSize(new IntDimension(500, 200));
		panel2.setPreferredSize(new IntDimension(500, 200));
		var border1:TitledBorder = new TitledBorder();
		border1.setColor(new ASColor(0x0, 1));
		border1.setTitle("Local Events");
		border1.setPosition(1);
		border1.setAlign(AsWingConstants.LEFT);
		border1.setBeveled(true);
		border1.setRound(10);
		panel2.setBorder(border1);
		var layout2:BoxLayout = new BoxLayout();
		panel2.setLayout(layout2);
		
		panel2.append(new JScrollPane(gridLocalActions));
		
		panel3 = new JPanel();
		panel3.setLocation(new IntPoint(0, 210));
		panel3.setSize(new IntDimension(500, 200));
		panel3.setPreferredSize(new IntDimension(0, 200));
		var border3:TitledBorder = new TitledBorder();
		border3.setColor(new ASColor(0x0, 1));
		border3.setTitle("Unit Movements");
		border3.setPosition(1);
		border3.setAlign(AsWingConstants.LEFT);
		border3.setBeveled(true);
		border3.setRound(10);
		panel3.setBorder(border3);
		var layout4:BorderLayout = new BorderLayout();
		panel3.setLayout(layout4);
		
		panel3.append(new JScrollPane(gridNotifications));
				
		//component layoution
		append(panel2);
		append(panel3);		
	}
	
	
	
	
}
}
