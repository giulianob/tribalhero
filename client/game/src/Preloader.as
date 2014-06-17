package src
{
    import flash.display.*;
    import flash.events.*;
    import flash.text.*;
    import flash.utils.*;

    [SWF]
	public dynamic class Preloader extends MovieClip
	{

		private var _clip:MovieClip;
		private var loadText: TextField;
		private var loadPercentage: TextField;
        private var uncaughtExceptionHandler: UncaughtExceptionHandler;

		public function Preloader()
		{
			// add a blank MovieClip to the preloader to hold the loader
			_clip = new MovieClip();
			addChild(_clip);

			loadText = new TextField();
			loadText.autoSize = TextFieldAutoSize.CENTER;
			loadText.text = "Downloading game. This may take a minute or two the first time...";
			loadText.setTextFormat(new TextFormat("Arial", 13, null, true));
			loadText.x = stage.stageWidth / 2 - loadText.textWidth / 2;
			loadText.y = stage.stageHeight / 2 - loadText.textHeight / 2;		
			
			loadPercentage = new TextField();
			loadPercentage.autoSize = TextFieldAutoSize.CENTER;
			loadPercentage.text = "0%";
			loadPercentage.setTextFormat(new TextFormat("Arial", 13, null, true));
			loadPercentage.x = stage.stageWidth / 2 - loadPercentage.textWidth / 2;
			loadPercentage.y = loadText.y + 25;			

			_clip.addChild(loadText);
			_clip.addChild(loadPercentage);

			addEventListener(Event.ENTER_FRAME, checkFrame);
		}

		private function checkFrame(e:Event):void
		{			
			var percentage: int = Math.floor((this.stage.loaderInfo.bytesLoaded / this.stage.loaderInfo.bytesTotal) * 100);
			
			loadPercentage.text = percentage + "%";
			
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
            
            uncaughtExceptionHandler = new UncaughtExceptionHandler(loaderInfo);
        
			var mainClass:Class = getDefinitionByName("src.Main") as Class;
			stage.addChild(new mainClass() as DisplayObject);
		}
	}
}

