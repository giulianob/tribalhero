package src
{
	import flash.display.DisplayObject;
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.text.TextField;
	import flash.text.TextFormat;
	import flash.utils.getDefinitionByName;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public dynamic class Preloader extends MovieClip
	{

		private var _clip:MovieClip;
		private var loadText: TextField;

		public function Preloader()
		{
			// add a blank MovieClip to the preloader to hold the loader
			_clip = new MovieClip();
			addChild(_clip);

			loadText = new TextField();
			loadText.text = "Downloading game. This may take a minute or two...";
			loadText.setTextFormat(new TextFormat("Arial", 13, null, true));
			loadText.x = stage.stageWidth / 2 - loadText.textWidth / 2;
			loadText.y = stage.stageHeight / 2 - loadText.textHeight / 2;

			_clip.addChild(loadText);

			addEventListener(Event.ENTER_FRAME, checkFrame);
		}

		private function checkFrame(e:Event):void
		{			
			if (currentFrame == totalFrames)
			{
				removeEventListener(Event.ENTER_FRAME, checkFrame);
				startup();
			}
		}

		private function startup():void
		{
			// hide loader
			stop();
			removeChild(_clip);
			var mainClass:Class = getDefinitionByName("src.Main") as Class;
			addChild(new mainClass() as DisplayObject);
		}

	}

}

