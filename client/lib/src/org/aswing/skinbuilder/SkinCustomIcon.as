package org.aswing.skinbuilder{

public class SkinCustomIcon extends SkinButtonIcon{
	
	private var propertyPrefix: String;
	
	public function SkinCustomIcon(propertyPrefix: String) {
		this.propertyPrefix = propertyPrefix;
		
		super();		
	}
	
	override protected function getPropertyPrefix():String{
        return propertyPrefix + ".";
    }
}
}