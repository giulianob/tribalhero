package src.Objects {

	import src.Objects.Factories.ObjectFactory;
	import src.Objects.*;

	public class NewCityPlaceholder extends GameObject {

		public function NewCityPlaceholder() {
			type = 201;
		}
		
		public function ToSprite(): Object
		{
			return ObjectFactory.getNewCityPlaceholderSprite();
		}
	}

}

