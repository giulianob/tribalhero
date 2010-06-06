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
		private var btnOk:JButton;
	
		public function GoToDialog() 
		{
			createUI();
			btnOk.addActionListener(onOk);
		}		
		
		private function onOk(e: *):void {
			if (txtX.getText() == "" || txtY.getText() == "") 
			{
				getFrame().dispose();
				return;
			}
			
			var pt: Point = MapUtil.getScreenCoord(getCoordX(), getCoordY());
			Global.gameContainer.map.camera.ScrollToCenter(pt.x, pt.y);
			
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
			//component creation
			setSize(new IntDimension(200, 93));
			var layout0:BorderLayout = new BorderLayout();
			setLayout(layout0);
			
			label81 = new JLabel();
			label81.setLocation(new IntPoint(5, 5));
			label81.setSize(new IntDimension(300, 30));
			label81.setPreferredSize(new IntDimension(200, 30));
			label81.setConstraints("North");
			label81.setText("Enter coordinates to visit");
			label81.setHorizontalAlignment(AsWingConstants.LEFT);
			
			panel8 = new JPanel();
			panel8.setLocation(new IntPoint(0, 30));
			panel8.setSize(new IntDimension(220, 31));
			panel8.setConstraints("Center");
			var layout1:FlowLayout = new FlowLayout();
			layout1.setAlignment(AsWingConstants.CENTER);
			layout1.setHgap(0);
			panel8.setLayout(layout1);
			
			txtX = new JTextField();
			txtX.setLocation(new IntPoint(61, 5));
			txtX.setSize(new IntDimension(40, 21));
			txtX.setColumns(4);
			txtX.setMaxChars(4);
			txtX.setRestrict("0-9");
			
			label10 = new JLabel();
			label10.setLocation(new IntPoint(20, 7));
			label10.setSize(new IntDimension(26, 17));
			label10.setText(",");
			
			txtY = new JTextField();
			txtY.setLocation(new IntPoint(104, 5));
			txtY.setSize(new IntDimension(40, 21));
			txtY.setColumns(4);
			txtY.setMaxChars(4);
			txtY.setRestrict("0-9");
			
			panel86 = new JPanel();
			panel86.setLocation(new IntPoint(0, 159));
			panel86.setSize(new IntDimension(230, 32));
			panel86.setConstraints("South");
			var layout2:FlowLayout = new FlowLayout();
			layout2.setAlignment(AsWingConstants.CENTER);
			panel86.setLayout(layout2);
			
			btnOk = new JButton();
			btnOk.setLocation(new IntPoint(113, 5));
			btnOk.setSize(new IntDimension(22, 22));
			btnOk.setText("Ok");
			
			//component layoution
			append(label81);
			append(panel8);
			append(panel86);
			
			panel8.append(txtX);
			panel8.append(label10);
			panel8.append(txtY);
			
			panel86.append(btnOk);			
		}
	}
	
}