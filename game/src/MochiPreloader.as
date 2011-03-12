package src
{
	import flash.display.DisplayObject;
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.utils.getDefinitionByName;
	import mochi.as3.*;

	/**
	 * ...
	 * @author Giuliano Barberi
	 */
	public dynamic class MochiPreloader extends MovieClip
	{

		private static var GAME_ID: String = "7d9c5048045d1086";
		
		private var _clip:MovieClip;

		public function MochiPreloader()
		{
			// add a blank MovieClip to the preloader to hold the Ad
			_clip = new MovieClip();
			addChild(_clip);

			MochiAd.showPreGameAd( {
				id: GAME_ID,
				clip: _clip,
				ad_finished: startup,
				res:"976x640",
				no_bg:true
			} );
			
			MochiServices.connect(GAME_ID, _clip, function(status: String) : void { } );
			
			MochiEvents.startPlay();			
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

