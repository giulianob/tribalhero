package src.UI.Dialog 
{

import src.Constants;
import src.Global;
import src.UI.GameJFrame;
import src.UI.GameJPanel;
import org.aswing.ext.MultilineLabel;
import org.aswing.*;
import org.aswing.geom.IntDimension;

import org.aswing.border.EmptyBorder;
import org.aswing.event.*;
import org.aswing.geom.IntPoint;

public class InfoDialog extends GameJPanel {
	private var okButton:JButton;
	private var cancelButton:JButton;
	private var yesButton:JButton;
	private var noButton:JButton;
	private var closeButton:JButton;
	
	private var centerPane:JPanel;
	private var msgLabel:MultilineLabel;
	private var inputText:JTextField;
	private var buttonPane:JPanel;	
	
	public function InfoDialog() {
		setLayout(new BorderLayout());
		centerPane = SoftBox.createVerticalBox(6);
		msgLabel = new MultilineLabel("", 0);
		centerPane.append(AsWingUtils.createPaneToHold(msgLabel, new FlowLayout(FlowLayout.CENTER, 5, 5)));
		inputText = new JTextField();
		var inputContainer:JPanel = new JPanel(new BorderLayout());		
		inputContainer.append(inputText, BorderLayout.CENTER);
		centerPane.append(inputContainer);
		buttonPane = new JPanel(new FlowLayout(FlowLayout.CENTER));
		append(centerPane, BorderLayout.CENTER);
		append(buttonPane, BorderLayout.SOUTH);
	}	
	
	public function getInputText():JTextField{
		return inputText;
	}
	
	public function getMsgLabel():MultilineLabel{
		return msgLabel;
	}
	
	public function getOkButton():JButton{
		if(okButton == null){
			okButton = new JButton(JOptionPane.OK_STR);
		}
		return okButton;
	}
	
	public function getCancelButton():JButton{
		if(cancelButton == null){
			cancelButton = new JButton(JOptionPane.CANCEL_STR);
		}
		return cancelButton;
	}	
	public function getYesButton():JButton{
		if(yesButton == null){
			yesButton = new JButton(JOptionPane.YES_STR);
		}
		return yesButton;
	}
	
	public function getNoButton():JButton{
		if(noButton == null){
			noButton = new JButton(JOptionPane.NO_STR);
		}
		return noButton;
	}	
	public function getCloseButton():JButton{
		if(closeButton == null){
			closeButton = new JButton(JOptionPane.CLOSE_STR);
		}
		return closeButton;
	}	
	
	public function addButton(button:JButton):void{
		buttonPane.append(button);
	}
	
	private function setMessage(msg:String):void{
		msgLabel.setText(msg);				
		msgLabel.setColumns(Math.max(20, Math.min(msg.length - 20, 40)));
	}
	private function addCloseListener(button:JButton):void{
		var f:JFrame = getFrame();		
		button.addActionListener(function():void{ f.tryToClose(); });
	}
	
	public static function showMessageDialog(title:String, msg:String, finishHandler:Function=null, parentComponent:Component=null, modal:Boolean=true, closable:Boolean=true, buttons:int=1):InfoDialog{		
		var pane:InfoDialog = new InfoDialog();
		pane.getInputText().setVisible(false);
		pane.setMessage(msg);				
		
		pane.title = title;		
		
		pane.showSelf(AsWingUtils.getOwnerAncestor(parentComponent), modal, finishHandler);
		
		var handler:Function = finishHandler;
		
		if((buttons & JOptionPane.OK) == JOptionPane.OK){
			pane.addButton(pane.getOkButton());
			pane.addCloseListener(pane.getOkButton());
			pane.getOkButton().addActionListener(function():void{
				if (handler != null) handler(JOptionPane.OK);				
			});
		}
		if((buttons & JOptionPane.YES) == JOptionPane.YES){
			pane.addButton(pane.getYesButton());
			pane.addCloseListener(pane.getYesButton());
			pane.getYesButton().addActionListener(function():void{
				if (handler != null) handler(JOptionPane.YES);
			});
		}
		if((buttons & JOptionPane.NO) == JOptionPane.NO){
			pane.addButton(pane.getNoButton());
			pane.addCloseListener(pane.getNoButton());
			pane.getNoButton().addActionListener(function():void{
				if (handler != null) handler(JOptionPane.NO);
			});
		}
		if((buttons & JOptionPane.CANCEL) == JOptionPane.CANCEL){
			pane.addButton(pane.getCancelButton());
			pane.addCloseListener(pane.getCancelButton());
			pane.getCancelButton().addActionListener(function():void{
				if (handler != null) handler(JOptionPane.CANCEL);
			});
		}
		if((buttons & JOptionPane.CLOSE) == JOptionPane.CLOSE){
			pane.addButton(pane.getCloseButton());
			pane.addCloseListener(pane.getCloseButton());
			pane.getCloseButton().addActionListener(function():void{
				if (handler != null) handler(JOptionPane.CLOSE);
			});
		}		
		
		pane.frame.addEventListener(FrameEvent.FRAME_CLOSING, 
			function():void{
				if (handler != null) handler(JOptionPane.CLOSE);
			});
		pane.frame.setDefaultCloseOperation(JFrame.DISPOSE_ON_CLOSE);						
		pane.frame.setResizable(false);
		pane.frame.setClosable(closable);		
		pane.frame.pack();
		
		Global.gameContainer.showFrame(pane.frame);
		return pane;
	}
	
	public static function showInputDialog(title:String, msg:String, finishHandler:Function=null, defaultValue:String="", parentComponent:Component=null, modal:Boolean=true):InfoDialog{
		var frame:GameJFrame = new GameJFrame(AsWingUtils.getOwnerAncestor(parentComponent), title, modal);
		var pane:InfoDialog = new InfoDialog();
		if(defaultValue != ""){
			pane.inputText.setText(defaultValue);
		}
		pane.setMessage(msg);
		pane.frame = frame;
		
		pane.addButton(pane.getOkButton());
		pane.addCloseListener(pane.getOkButton());
		pane.addButton(pane.getCancelButton());
		pane.addCloseListener(pane.getCancelButton());
		
		var handler:Function = finishHandler;
		
		pane.getOkButton().addActionListener(
			function():void{
				if (handler != null) handler(pane.getInputText().getText());
			}
		);
		
		var cancelHandler:Function = function():void{
			if (handler != null) handler(null);
		};
		
		pane.getCancelButton().addActionListener(cancelHandler);
		frame.addEventListener(FrameEvent.FRAME_CLOSING, cancelHandler);
			
		frame.setDefaultCloseOperation(JFrame.DISPOSE_ON_CLOSE);
		frame.setResizable(false);
		frame.getContentPane().append(pane, BorderLayout.CENTER);
		frame.pack();
		var location:IntPoint = AsWingUtils.getScreenCenterPosition();
		location.x = Math.round(location.x - frame.getWidth()/2);
		location.y = Math.round(location.y - frame.getHeight()/2);
		frame.setLocation(location);
		Global.gameContainer.showFrame(frame);
		return pane;
	}
}
	
}