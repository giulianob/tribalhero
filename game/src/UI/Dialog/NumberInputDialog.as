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
import src.Objects.Resources;
import src.UI.Components.SimpleResourcesPanel;
import src.UI.GameJPanel;
    import src.Util.DateUtil;
    import src.Util.Util;

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
	private var pnlResources: JPanel;
	private var lblTime: JLabel;
	private var pnlTime: JPanel;
	private var btnOk:JButton;		
	private var costPerUnit: Resources;	
	private var timePerUnit: int;	
	
	public function NumberInputDialog(prompt: String, minValue: int, maxValue: int, onAccept: Function, initialValue: int = 1, costPerUnit: Resources = null, timePerUnit: int = 0):void {
		this.costPerUnit = costPerUnit;
		this.timePerUnit = timePerUnit;
		
		createUI();			
		
		title = "Enter the amount";
		
		txtTitle.setText(prompt);
		
		if (initialValue < minValue || initialValue > maxValue) initialValue = minValue;
		
		sldAmount.setMinimum(minValue);
		sldAmount.setValues(initialValue, 0, minValue, maxValue);
		
		sldAmount.addStateListener(updateResources);
		sldAmount.addStateListener(updateTime);	

		var self: NumberInputDialog = this;		
		sldAmount.addEventListener(KeyboardEvent.KEY_UP, function(e: KeyboardEvent) : void { 
			if (e.keyCode == Keyboard.ENTER) {
				btnOk.doClick();
			}
		});
		btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self); } );
		
		updateResources();
		updateTime();
	}
	
	private function updateTime(e: Event = null) : void {	
		if (timePerUnit <= 0) return;
		
		lblTime.setText(DateUtil.formatTime(timePerUnit * sldAmount.getValue()));
	}
	
	private function updateResources(e: Event = null) : void {
		if (costPerUnit == null) return;
		
		pnlResources.removeAll();
		pnlResources.append(new SimpleResourcesPanel(costPerUnit.multiplyByUnit(sldAmount.getValue())));
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
		var layout0:BoxLayout = new BoxLayout(BoxLayout.Y_AXIS, 5);
		setLayout(layout0);
		
		txtTitle = new JLabel();
		txtTitle.setMinimumWidth(220);
		txtTitle.setHorizontalAlignment(AsWingConstants.LEFT);
		
		panel4 = new JPanel();
		var layout1:FlowLayout = new FlowLayout();
		layout1.setAlignment(AsWingConstants.CENTER);
		panel4.setLayout(layout1);
		
		sldAmount = new JAdjuster();
		sldAmount.setColumns(5);
		
		panel8 = new JPanel();
		var layout2:FlowLayout = new FlowLayout();
		layout2.setAlignment(AsWingConstants.CENTER);
		panel8.setLayout(layout2);	
		
		btnOk = new JButton();		
		btnOk.setSize(new IntDimension(22, 22));
		btnOk.setText("Ok");
		
		if (costPerUnit != null)
		{
			pnlResources = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0, 0, false));		
			pnlResources.setBorder(new SimpleTitledBorder(null, "Total Cost"));
		}
		
		if (timePerUnit > 0)
		{				
			pnlTime = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0, 0, false));		
			pnlTime.setBorder(new SimpleTitledBorder(null, "Total Time"));
			
			lblTime = new JLabel("", new AssetIcon(new ICON_CLOCK()), AsWingConstants.CENTER);
			pnlTime.append(lblTime);
		}		
			
		//component layoution
		append(txtTitle);
		append(panel4);
		
		if (costPerUnit != null)
			append(pnlResources);
			
		if (timePerUnit > 0)
			append(pnlTime);
			
		append(panel8);
		
		panel4.append(sldAmount);
		
		panel8.append(btnOk);
	}
}
	
}
