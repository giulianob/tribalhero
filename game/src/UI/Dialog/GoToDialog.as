package src.UI.Dialog 
{
    import flash.events.Event;
    import flash.geom.Point;

    import org.aswing.*;
    import org.aswing.geom.*;

    import src.Global;
    import src.Map.TileLocator;
    import src.UI.Components.AutoCompleteTextField;
    import src.UI.GameJPanel;

    public class GoToDialog extends GameJPanel
	{
		//members define
		private var txtX:JTextField;
		private var txtY:JTextField;		
		private var btnOkCoord:JButton;
		
		private var txtCityName:JTextField;
		private var btnOkCity:JButton;
		
		private var txtPlayerName:JTextField;
		private var btnOkPlayer:JButton;
		
		private var txtStrongholdName:JTextField;
		private var btnOkStronghold:JButton;		
		
		private var pnlTabs: JTabbedPane;
	
		public function GoToDialog() 
		{
			createUI();		
			
			txtX.addActionListener(onOkCoord);
			txtY.addActionListener(onOkCoord);
			btnOkCoord.addActionListener(onOkCoord);
			
			txtCityName.addActionListener(onOkCity);
			btnOkCity.addActionListener(onOkCity);
			
			txtStrongholdName.addActionListener(onOkStronghold);
			btnOkStronghold.addActionListener(onOkStronghold);			
			
			txtPlayerName.addActionListener(onOkPlayer);
			btnOkPlayer.addActionListener(onOkPlayer);
			
			AsWingManager.callLater(txtX.requestFocus);
			
			pnlTabs.addStateListener(function(e: Event = null): void {
				switch (pnlTabs.getSelectedIndex())
				{
					case 0:
						AsWingManager.callLater(txtX.requestFocus);
						break;
					case 1:
						AsWingManager.callLater(txtCityName.requestFocus);										
						break;
					case 2:
						AsWingManager.callLater(txtPlayerName.requestFocus);
						break;
					case 3:
						AsWingManager.callLater(txtStrongholdName.requestFocus);
				}
			});
		}		
		
		private function onOkCoord(e: *):void {
			if (txtX.getText() == "" || txtY.getText() == "") 
			{
				getFrame().dispose();
				return;
			}
			
			var pt: Point = TileLocator.getScreenCoord(getCoordX(), getCoordY());
			Global.gameContainer.map.camera.ScrollToCenter(pt.x, pt.y);
			
			getFrame().dispose();
		}
		
		private function onOkCity(e: * ):void {		
			if (txtCityName.getText() == "") {
				getFrame().dispose();
				return;
			}
			
			Global.mapComm.City.gotoCityLocationByName(txtCityName.getText());	
			getFrame().dispose();
		}		
		
		private function onOkStronghold(e: * ):void {		
			if (txtStrongholdName.getText() == "") {
				getFrame().dispose();
				return;
			}
			
			Global.mapComm.Stronghold.gotoStrongholdLocationByName(txtStrongholdName.getText());	
			getFrame().dispose();
		}		
				
		private function onOkPlayer(e: * ):void {		
			if (txtPlayerName.getText() == "") {
				getFrame().dispose();
				return;
			}
			
			Global.mapComm.City.viewPlayerProfileByName(txtPlayerName.getText(), function(profileData: * ):void {
				if (!profileData)
					return;
					
				getFrame().dispose();
				var playerProfile: PlayerProfileDialog = new PlayerProfileDialog(profileData);
				playerProfile.show();
			});			
		}				
		
		private function getCoordX(): int {
			return int(txtX.getText());
		}
		
		private function getCoordY(): int {
			return int(txtY.getText());
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			return frame;
		}		
		
		private function createUI():void {
			title = "Go To";			
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
			
			//coord panel			
			var pnlCoords: JPanel = new JPanel(new BorderLayout());			
			
			var lblCoords: JLabel = new JLabel("Enter coordinates to visit", null, AsWingConstants.LEFT);
			lblCoords.setConstraints("North");
			
			var pnlCoordCenter: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0));
			pnlCoordCenter.setConstraints("Center");
		
			txtX = new JTextField();
			txtX.setSize(new IntDimension(40, 21));
			txtX.setColumns(4);
			txtX.setMaxChars(4);
			txtX.setRestrict("0-9");
			
			var lblCoordComma: JLabel = new JLabel(",");
			
			txtY = new JTextField();
			txtY.setSize(new IntDimension(40, 21));
			txtY.setColumns(4);
			txtY.setMaxChars(4);
			txtY.setRestrict("0-9");
			
			var pnlCoordSouth: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER));
			pnlCoordSouth.setConstraints("South");
			
			btnOkCoord = new JButton();
			btnOkCoord.setText("Ok");
			
			pnlCoordSouth.appendAll(btnOkCoord);
			pnlCoordCenter.appendAll(txtX, lblCoordComma, txtY);			
			pnlCoords.appendAll(lblCoords, pnlCoordCenter, pnlCoordSouth);
			
			//city name panel
			var pnlCity: JPanel = new JPanel(new BorderLayout());			
			
			var lblCityTitle: JLabel = new JLabel("Enter city name to visit", null, AsWingConstants.LEFT);
			lblCityTitle.setConstraints("North");
			
			var pnlCityCenter: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0));
			pnlCityCenter.setConstraints("Center");
			
			txtCityName = new AutoCompleteTextField(Global.mapComm.General.autoCompleteCity);
			txtCityName.setColumns(16);
			txtCityName.setMaxChars(32);
			
			var pnlCitySouth: JPanel = new JPanel();
			pnlCitySouth.setConstraints("South");
			pnlCitySouth.setLayout(new FlowLayout(AsWingConstants.CENTER));
			
			btnOkCity = new JButton();
			btnOkCity.setText("Ok");
			
			pnlCitySouth.appendAll(btnOkCity);
			pnlCityCenter.appendAll(txtCityName);
			pnlCity.appendAll(lblCityTitle, pnlCityCenter, pnlCitySouth);
			
			//player name panel
			var pnlPlayer: JPanel = new JPanel(new BorderLayout());			
			
			var lblPlayerTitle: JLabel = new JLabel("Enter a player name to view their profile", null, AsWingConstants.LEFT);
			lblPlayerTitle.setConstraints("North");
			
			var pnlPlayerCenter: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0));
			pnlPlayerCenter.setConstraints("Center");
			
			txtPlayerName = new AutoCompleteTextField(Global.mapComm.General.autoCompletePlayer);
			txtPlayerName.setColumns(16);
			txtPlayerName.setMaxChars(32);
			
			var pnlPlayerSouth: JPanel = new JPanel();
			pnlPlayerSouth.setConstraints("South");
			pnlPlayerSouth.setLayout(new FlowLayout(AsWingConstants.CENTER));
			
			btnOkPlayer = new JButton();
			btnOkPlayer.setText("Ok");
			
			pnlPlayerSouth.appendAll(btnOkPlayer);
			pnlPlayerCenter.appendAll(txtPlayerName);
			pnlPlayer.appendAll(lblPlayerTitle, pnlPlayerCenter, pnlPlayerSouth);					
			
			//stronghold name panel
			var pnlStronghold: JPanel = new JPanel(new BorderLayout());			
			
			var lblStrongholdTitle: JLabel = new JLabel("Enter a stronghold name to find it", null, AsWingConstants.LEFT);
			lblStrongholdTitle.setConstraints("North");
			
			var pnlStrongholdCenter: JPanel = new JPanel(new FlowLayout(AsWingConstants.CENTER, 0));
			pnlStrongholdCenter.setConstraints("Center");
			
			txtStrongholdName = new AutoCompleteTextField(Global.mapComm.General.autoCompleteStronghold);
			txtStrongholdName.setColumns(16);
			txtStrongholdName.setMaxChars(32);
			
			var pnlStrongholdSouth: JPanel = new JPanel();
			pnlStrongholdSouth.setConstraints("South");
			pnlStrongholdSouth.setLayout(new FlowLayout(AsWingConstants.CENTER));
			
			btnOkStronghold = new JButton();
			btnOkStronghold.setText("Ok");
			
			pnlStrongholdSouth.appendAll(btnOkStronghold);
			pnlStrongholdCenter.appendAll(txtStrongholdName);
			pnlStronghold.appendAll(lblStrongholdTitle, pnlStrongholdCenter, pnlStrongholdSouth);				
			
			// Tabs
			pnlTabs = new JTabbedPane();
			
			pnlTabs.appendTab(pnlCity, "Find city");
			pnlTabs.appendTab(pnlPlayer, "Find player");
			pnlTabs.appendTab(pnlStronghold, "Find stronghold");
			pnlTabs.appendTab(pnlCoords, "Go to coordinates");

			append(pnlTabs);			
		}
	}
	
}