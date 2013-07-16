package src.UI.Sidebars.NewCityPlaceholder {
    import flash.events.*;
    import flash.geom.Point;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.geom.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.UI.GameJSidebar;
    import src.UI.Sidebars.NewCityPlaceholder.Buttons.NewCityButton;

    public class NewCityPlaceholderSidebar extends GameJSidebar
	{
		private var pnlGroups: JPanel;		
		private var newCityPlaceholderObj: NewCityPlaceholder;
		private var newCityButton: NewCityButton;

		public function NewCityPlaceholderSidebar(newCityPlaceholderObj: NewCityPlaceholder)
		{
			this.newCityPlaceholderObj = newCityPlaceholderObj;			
			
			createUI();

			this.newCityButton = new NewCityButton(newCityPlaceholderObj);
			pnlGroups.append(newCityButton);
			
			update();
			Global.gameContainer.selectedCity.addEventListener(City.RESOURCES_UPDATE, onResourcesUpdate);
		}
	
		public function dispose():void
		{
			Global.gameContainer.selectedCity.removeEventListener(City.RESOURCES_UPDATE, onResourcesUpdate);
		}
		
		private function onResourcesUpdate(e: Event): void {
			update();
		}		
		
		private function update() : void {
			if (!newCityButton.validateButton())
				newCityButton.disable();
			else
				newCityButton.enable();
		}

		private function createUI() : void
		{
			//component creation
			setSize(new IntDimension(288, 180));
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));			

			pnlGroups = new JPanel();
			pnlGroups.setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
			var border: TitledBorder = new TitledBorder();
			border.setColor(new ASColor(0x0, 1));
			border.setTitle("Actions");
			border.setBeveled(false);
			border.setRound(10);
			pnlGroups.setBorder(border);

			//component layoution
			append(pnlGroups);
		}

		override public function show(owner:* = null, onClose:Function = null):JFrame
		{
			super.showSelf(owner, onClose, dispose);

			var pt: Point = TileLocator.getMapCoord(newCityPlaceholderObj.objX, newCityPlaceholderObj.objY);
			frame.getTitleBar().setText("New City Foundation");

			frame.show();
			return frame;
		}
	}

}

