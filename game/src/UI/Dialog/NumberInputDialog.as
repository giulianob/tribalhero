/**
* ...
* @author Default
* @version 0.1
*/

package src.UI.Dialog {
	
import flash.display.MovieClip;
import flash.events.Event;
import flash.events.KeyboardEvent;
import flash.events.MouseEvent;
import flash.display.SimpleButton;
import flash.text.TextField;
import flash.ui.Keyboard;
import src.Global;
import src.UI.GameJPanel;

import org.aswing.*;
import org.aswing.border.*;
import org.aswing.geom.*;
import org.aswing.colorchooser.*;
import org.aswing.ext.*;

public class NumberInputDialog extends GameJPanel {

	private var txtTitle:JLabel;
	private var panel4:JPanel;
	private var sldAmount:JAdjuster;
	private var panel8:JPanel;
	private var btnOk:JButton;	
		
	public function NumberInputDialog(prompt: String, minValue: int, maxValue: int, onAccept: Function, initialValue: int = 1):void {
		createUI();
		
		title = "Enter the amount";
		
		txtTitle.setText(prompt);

		if (initialValue < minValue || initialValue > maxValue) initialValue = minValue;
		
		sldAmount.setMinimum(minValue);
		sldAmount.setValues(initialValue, 0, minValue, maxValue);

		var self: NumberInputDialog = this;
		btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); });
	}
	
	public function getAmount(): JAdjuster
	{
		return sldAmount;
	}
	
	public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
	{
		super.showSelf(owner, modal, onClose);
		Global.gameContainer.showFrame(frame);
		
		return frame;
	}	
	
	private function createUI(): void 
	{
		//component creation
		setSize(new IntDimension(220, 93));
		var layout0:BorderLayout = new BorderLayout();
		setLayout(layout0);
		
		txtTitle = new JLabel();
		txtTitle.setLocation(new IntPoint(5, 5));
		txtTitle.setSize(new IntDimension(230, 30));
		txtTitle.setPreferredSize(new IntDimension(220, 30));
		txtTitle.setConstraints("North");		
		txtTitle.setHorizontalAlignment(AsWingConstants.LEFT);
		
		panel4 = new JPanel();
		panel4.setSize(new IntDimension(230, 40));
		panel4.setConstraints("Center");
		var layout1:FlowLayout = new FlowLayout();
		layout1.setAlignment(AsWingConstants.CENTER);
		panel4.setLayout(layout1);
		
		sldAmount = new JAdjuster();
		sldAmount.setLocation(new IntPoint(80, 5));
		sldAmount.setSize(new IntDimension(59, 21));
		sldAmount.setColumns(5);
		
		panel8 = new JPanel();
		panel8.setLocation(new IntPoint(0, 29));
		panel8.setSize(new IntDimension(220, 32));
		panel8.setConstraints("South");
		var layout2:FlowLayout = new FlowLayout();
		layout2.setAlignment(AsWingConstants.CENTER);
		panel8.setLayout(layout2);
		
		btnOk = new JButton();
		btnOk.setLocation(new IntPoint(99, 5));
		btnOk.setSize(new IntDimension(22, 22));
		btnOk.setText("Ok");
		
		//component layoution
		append(txtTitle);
		append(panel4);
		append(panel8);
		
		panel4.append(sldAmount);
		
		panel8.append(btnOk);
	}
}
	
}
