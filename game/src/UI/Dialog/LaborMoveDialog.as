package src.UI.Dialog{

import org.aswing.*;
import org.aswing.border.*;
import org.aswing.event.InteractiveEvent;
import org.aswing.geom.*;
import org.aswing.colorchooser.*;
import org.aswing.ext.*;
import src.Constants;
import src.Global;
import src.Map.City;
import src.Map.Map;
import src.Objects.Factories.StructureFactory;
import src.Objects.Prototypes.StructurePrototype;
import src.Objects.StructureObject;
import src.UI.GameJPanel;

/**
 * LaborMoveDialog
 */
public class LaborMoveDialog extends GameJPanel{
	
	//members define
	private var label81:JLabel;
	private var panel82:JPanel;
	private var lblCount:JLabel;
	private var sldCount:JSlider;
	private var panel86:JPanel;
	private var btnOk:JButton;
	
	private var structure: StructureObject;
	
	/**
	 * LaborMoveDialog Constructor
	 */
	public function LaborMoveDialog(structure: StructureObject, onAccept: Function) {
		this.structure = structure;
		
		createUI();
		
		var city: City = Global.gameContainer.map.cities.get(structure.cityId);			
		var structPrototype: StructurePrototype = StructureFactory.getPrototype(structure.type, structure.level);
		
		sldCount.setMaximum(structPrototype.maxlabor);
		
		sldCount.setValue(structure.labor);
		
		if (city.resources.labor.getValue() + structure.labor < structPrototype.maxlabor)
			sldCount.setExtent(structPrototype.maxlabor - (city.resources.labor.getValue() + structure.labor));		
		
		sldCount.addEventListener(InteractiveEvent.STATE_CHANGED, onSlideChange);
		
		var self: LaborMoveDialog = this;
		btnOk.addActionListener(function():void { if (onAccept != null) onAccept(self) } );
		
		onSlideChange();
	}
	
	private function onSlideChange(e: InteractiveEvent = null):void {
		lblCount.setText(sldCount.getValue().toString() + " out of " + sldCount.getMaximum().toString());
	}
	
	public function getCount():int{
		return sldCount.getValue();
	}	
	
	public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
	{
		super.showSelf(owner, modal, onClose);
		Global.gameContainer.showFrame(frame);
		
		return frame;
	}
	
	private function createUI():void {
		//component creation	
		title = "Labor Assignment";
		setSize(new IntDimension(260, 111));
		var layout0:BorderLayout = new BorderLayout();
		setLayout(layout0);
		
		label81 = new JLabel();
		label81.setLocation(new IntPoint(5, 5));
		label81.setSize(new IntDimension(250, 30));
		label81.setPreferredSize(new IntDimension(260, 30));
		label81.setConstraints("North");
		label81.setText("How many laborers to place in this structure?");
		label81.setHorizontalAlignment(AsWingConstants.LEFT);
		
		panel82 = new JPanel();
		panel82.setLocation(new IntPoint(5, 37));
		panel82.setSize(new IntDimension(230, 164));
		panel82.setConstraints("Center");
		var layout1:FlowWrapLayout = new FlowWrapLayout();
		layout1.setPreferWidth(150);
		layout1.setAlignment(AsWingConstants.CENTER);
		panel82.setLayout(layout1);
		
		lblCount = new JLabel();
		lblCount.setLocation(new IntPoint(49, 5));
		lblCount.setSize(new IntDimension(150, 17));
		lblCount.setPreferredSize(new IntDimension(150, 17));
		lblCount.setConstraints("North");		
		
		sldCount = new JSlider();
		sldCount.setLocation(new IntPoint(0, 0));
		sldCount.setSize(new IntDimension(200, 30));
		sldCount.setConstraints("Center");
		sldCount.setMinimum(0);
		sldCount.setMajorTickSpacing(1);
		sldCount.setPaintTicks(true);
		sldCount.setSnapToTicks(true);
		
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
		append(panel82);
		append(panel86);
		
		panel82.append(lblCount);
		panel82.append(sldCount);
		
		panel86.append(btnOk);		
	}
}
}