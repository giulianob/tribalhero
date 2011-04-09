/*
 Copyright aswing.org, see the LICENCE.txt.
*/

package org.aswing.skinbuilder{

import org.aswing.plaf.basic.accordion.BasicAccordionHeader;
import org.aswing.*;
import org.aswing.plaf.basic.background.ButtonBackground;

public class SkinAccordionHeader extends BasicAccordionHeader{
	
	public function SkinAccordionHeader(){
		super();
	}

	override protected function createHeaderButton():AbstractButton{
		var tb:JButton = new JButton();
		tb.setBackgroundDecorator(new SkinButtonBackground(getPropertyPrefix()));
		return tb;
	}
	
	protected function getPropertyPrefix():String{
		return "Accordion.header.";
	}
	
	override public function setSelected(b:Boolean):void{
		button.setSelected(b);
	}
}
}