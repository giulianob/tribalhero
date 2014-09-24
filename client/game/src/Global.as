package src 
{
    import flash.display.Stage;

    import src.FeathersUI.Map.MapVM;

    import src.Map.MapComm;

    public class Global
	{
        public static var stage: Stage;
        public static var starlingStage: StarlingStage;
		public static var gameContainer: GameContainer;
		public static var map: MapVM;
		public static var main: Main;
		public static var mapComm: MapComm;
		public static var musicPlayer: MusicPlayer;
	}
}