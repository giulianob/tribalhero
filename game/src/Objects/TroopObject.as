package src.Objects {
	
	import fl.controls.Label;
	import flash.display.Sprite;
	import src.Constants;
	import src.Objects.Factories.TroopFactory;
	/**
	* ...
	* @author Default
	*/
	public class TroopObject extends GameObject {
		
		public var speed: int;
		public var attackRadius: int;
		
		public var troop: Troop;
		
		public var template: TemplateManager = new TemplateManager();
				
		public function TroopObject() {
			
		}
		
		override public function setSelected(bool:Boolean = false):void 
		{
			super.setSelected(bool);
			
			if (bool) showRadius(attackRadius);
			else hideRadius();
		}
		
		public override function copy(obj: SimpleGameObject):void
		{
			var copyObj: TroopObject = obj as TroopObject;
			if (copyObj == null)
				return;			
		}
		
		public function ToSprite(): Object
		{
			return TroopFactory.getSprite();
		}
	}
	
}