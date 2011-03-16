package src.UI.Dialog 
{
	import flash.events.Event;
	import flash.geom.Point;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.geom.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import src.Constants;
	import src.Global;
	import src.Map.MapUtil;
	import src.UI.GameJPanel;
	
	public class GoToDialog extends GameJPanel
	{
		//members define
		private var label81:JLabel;
		private var panel8:JPanel;
		private var txtX:JTextField;
		private var label10:JLabel;
		private var txtY:JTextField;
		private var panel86:JPanel;
		private var btnOkCoord:JButton;
		
		private var txtCityName:JTextField;
		private var btnOkName:JButton;
	
		public function GoToDialog() 
		{
			createUI();
			btnOkCoord.addActionListener(onOkCoord);
			btnOkName.addActionListener(onOkName);
		}		
		
		private function onOkCoord(e: *):void {
			if (txtX.getText() == "" || txtY.getText() == "") 
			{
				getFrame().dispose();
				return;
			}
			
			var pt: Point = MapUtil.getScreenCoord(getCoordX(), getCoordY());
			Global.gameContainer.map.camera.ScrollToCenter(pt.x, pt.y);
			
			getFrame().dispose();
		}
		
		private function onOkName(e: *):void {
			if (txtCityName.getText() == "") {
				getFrame().dispose();
				return;
			}
			
			Global.mapComm.City.gotoCityLocationByName(txtCityName.getText());	
			getFrame().dispose();
		}		
		
		private function getCoordX(): int {
			return int(txtX.getText());
		}
		
		private function getCoordY(): int {
			return int(txtY.getText());
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}		
		
		private function createUI():void {
			title = "Go To";			
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			
			//coord panel			
			var pnlCoords: JPanel = new JPanel(new BorderLayout());			
			
			label81 = new JLabel();
			label81.setConstraints("North");
			label81.setText("Enter coordinates to visit");
			label81.setPreferredSize(new IntDimension(200, 21));
			label81.setHorizontalAlignment(AsWingConstants.LEFT);
			
			panel8 = new JPanel();
			panel8.setConstraints("Center");
			panel8.setLayout(new FlowLayout(AsWingConstants.CENTER, 0));
			
			txtX = new JTextField();
			txtX.setSize(new IntDimension(40, 21));
			txtX.setColumns(4);
			txtX.setMaxChars(4);
			txtX.setRestrict("0-9");
			
			label10 = new JLabel();
			label10.setSize(new IntDimension(26, 17));
			label10.setText(",");
			
			txtY = new JTextField();
			txtY.setSize(new IntDimension(40, 21));
			txtY.setColumns(4);
			txtY.setMaxChars(4);
			txtY.setRestrict("0-9");
			
			panel86 = new JPanel();
			panel86.setConstraints("South");
			panel86.setLayout(new FlowLayout(AsWingConstants.CENTER));
			
			btnOkCoord = new JButton();
			btnOkCoord.setText("Ok");
			
			//city name panel
			var pnlName: JPanel = new JPanel(new BorderLayout());			
			
			var pnlNameTitle: JLabel = new JLabel();
			pnlNameTitle.setConstraints("North");
			pnlNameTitle.setText("Enter city name to visit");
			pnlNameTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			
			var pnlNameCenter: JPanel = new JPanel();
			pnlNameCenter.setConstraints("Center");
			pnlNameCenter.setLayout(new FlowLayout(AsWingConstants.CENTER, 0));			
			
			txtCityName = new JTextField();
			txtCityName.setColumns(16);
			txtCityName.setMaxChars(32);
			
			var pnlNameSouth: JPanel = new JPanel();
			pnlNameSouth.setConstraints("South");
			pnlNameSouth.setLayout(new FlowLayout(AsWingConstants.CENTER));
			
			btnOkName = new JButton();
			btnOkName.setText("Ok");			
			
			//component layoution
			pnlCoords.append(label81);
			pnlCoords.append(panel8);
			pnlCoords.append(panel86);			
			panel8.append(txtX);
			panel8.append(label10);
			panel8.append(txtY);			
			panel86.append(btnOkCoord);		
			
			pnlNameCenter.append(txtCityName);
			pnlNameSouth.append(btnOkName);
			pnlName.append(pnlNameTitle);
			pnlName.append(pnlNameCenter);
			pnlName.append(pnlNameSouth);
			
			append(pnlCoords);
			append(new JSeparator());
			append(pnlName);
			
		}
	}
	
}