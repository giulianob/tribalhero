package src.Objects.Troop {

	import src.Objects.Factories.TroopFactory;
	import src.Objects.*;
	/**
	 * ...
	 * @author Default
	 */
	public class TroopObject extends GameObject {

		public var speed: int;
		public var attackRadius: int;
		public var stubId: int;
		public var targetX: int;
		public var targetY: int;

		public var troop: TroopStub;

		public var template: UnitTemplateManager = new UnitTemplateManager();

		public function TroopObject() {

		}

		public function getNiceStubId(includeParenthesis: Boolean = false) : String {
			if (stubId == 1) {
				if (includeParenthesis) return "(Local Troop)";
				else return "Local Troop";
			}
			else return (includeParenthesis?"(":"") + stubId.toString()  + (includeParenthesis?")":"");
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
