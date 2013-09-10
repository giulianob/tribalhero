package src.UI.Dialog 
{
    import flash.events.*;
    import flash.geom.*;

    import org.aswing.*;
    import org.aswing.border.*;
    import org.aswing.ext.*;

    import src.*;
    import src.Map.*;
    import src.Objects.*;
    import src.Objects.Battle.*;
    import src.Objects.Process.*;
    import src.Objects.Stronghold.*;
    import src.Objects.Troop.*;
    import src.UI.*;
    import src.UI.Components.*;
    import src.UI.Components.BattleReport.*;
    import src.UI.Components.TroopCompositionGridList.*;
    import src.UI.LookAndFeel.*;
    import src.Util.*;

    public class StrongholdProfileDialog extends GameJPanel
	{
		private var profileData: * ;
		
		private var pnlInfoContainer: Form;
		private var pnlButtonContainer: Container;
		private var pnlLeftContainer: Container;

		private var pnlTroopPanel: JPanel;
		private var pnlReportPanel: Container;
		private var pnlRightContainer: Container;
		
		private var lblStrongholdName: JLabel;
		
		private var reports: LocalReportList;
		private var nameLabel: JLabel;
		
		private var btnGoTo: JLabelButton;
		private var btnSendReinforcement: JLabelButton;
		private var btnViewBattle: JLabelButton;
		private var pnlHeaderButtons:JPanel;
				
		public function StrongholdProfileDialog(profileData: *) 
		{
			this.profileData = profileData;
			createUI();

            updateFromProfileData();

			var self: StrongholdProfileDialog = this;
			btnGoTo.addActionListener(function(e: Event):void {
				Global.gameContainer.closeAllFrames(true);
				var pt:Point = MapUtil.getScreenCoord(self.profileData.strongholdX, self.profileData.strongholdY);
				Global.map.camera.ScrollToCenter(pt.x, pt.y);
			});
			
			btnSendReinforcement.addActionListener(function(e:Event): void {
				var point: Point = MapUtil.getScreenCoord(self.profileData.strongholdX, self.profileData.strongholdY);
				Global.gameContainer.camera.ScrollToCenter(point.x, point.y);
				var process : ReinforcementSendProcess = new ReinforcementSendProcess(Global.gameContainer.selectedCity, new Location(Location.STRONGHOLD, self.profileData.strongholdId));
				process.execute();
			});			
			
			btnViewBattle.addActionListener(function(e: Event): void {
				var battleViewer: BattleViewer = new BattleViewer(self.profileData.strongholdBattleId);
				battleViewer.show(null, false);
			});			
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.closeAllFramesByType(StrongholdProfileDialog);
			Global.gameContainer.showFrame(frame);
			return frame;
		}
		
		private function createUI():void {
			setPreferredHeight(500);

            title = "Stronghold Profile - " + profileData.strongholdName;

			setLayout(new SoftBoxLayout(SoftBoxLayout.X_AXIS));
			
			nameLabel = new JLabel(profileData.strongholdName, null, AsWingConstants.LEFT);
			GameLookAndFeel.changeClass(nameLabel, "darkSectionHeader");
			
			// stronghold properties panel
			pnlInfoContainer = new Form();
			
			// button panel
			pnlButtonContainer = createButtonPanel();
			
			// Troop panel
			pnlTroopPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5, AsWingConstants.TOP));
			
			// Report panel
			pnlReportPanel = createReportPanel();
			
			// Left panel
			pnlLeftContainer = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			pnlLeftContainer.setPreferredWidth(255);			
			pnlLeftContainer.appendAll(nameLabel, pnlButtonContainer, pnlInfoContainer);
			
			// Right panel			
			var tabPanel: JTabbedPane = new JTabbedPane();
			tabPanel.setPreferredWidth(500);
			tabPanel.appendTab(Util.createTopAlignedScrollPane(pnlTroopPanel), StringHelper.localize("STR_TROOPS"));
			tabPanel.appendTab(pnlReportPanel, StringHelper.localize("STR_REPORTS"));			

			// Append main panels
			appendAll(pnlLeftContainer, tabPanel);
		}
		
		private function addInfo(form: Form, title: String, textOrComponent: *, icon: Icon = null) : void {
			var rowTitle: JLabel = new JLabel(title);
			rowTitle.setHorizontalAlignment(AsWingConstants.LEFT);
			rowTitle.setName("title");

			var rowValue: Component;
			if (textOrComponent is String) {
				var label: JLabel = new JLabel(textOrComponent as String);
				label.setHorizontalAlignment(AsWingConstants.LEFT);
				label.setHorizontalTextPosition(AsWingConstants.LEFT);
				label.setName("value");
				label.setIcon(icon);
				rowValue = label;
			} 
			else			
				rowValue = textOrComponent as Component;			

			form.addRow(rowTitle, rowValue);
		}
		
		private function createButtonPanel() : Container {
			if (!pnlHeaderButtons) {
				pnlHeaderButtons = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));
				pnlHeaderButtons.setBorder(new EmptyBorder(null, new Insets(0, 0, 10)));
				
				btnSendReinforcement = new JLabelButton("Send reinforcement");

				btnGoTo = new JLabelButton("Go there");
				
				btnViewBattle = new JLabelButton("View battle");
				
				pnlHeaderButtons.appendAll(btnSendReinforcement, btnGoTo, btnViewBattle);
			}

			return pnlHeaderButtons;
		}

        private function createTroopItem(troop: *) : JPanel {
			var pnl: JPanel = new JPanel(new BorderLayout(5));
			
			var label: PlayerCityLabel = new PlayerCityLabel(troop.playerId, troop.cityId, troop.playerName, troop.cityName);
			label.setPreferredWidth(175);
			label.setConstraints("West");
			pnl.append(label);
			
			var gridList: TroopCompositionGridList = new TroopCompositionGridList(troop.stub, 5, 0);
			gridList.setConstraints("Center");
			
			pnl.append(AsWingUtils.createPaneToHold(gridList, new SoftBoxLayout(), "Center"));
			
			if (troop.playerId == Constants.playerId) {
				var retreatButton: JLabelButton = new JLabelButton(StringHelper.localize("STR_RETREAT"), null, AsWingConstants.LEFT);
				retreatButton.setVerticalAlignment(AsWingConstants.TOP);
				
				var cityId: uint = troop.cityId;
				var troopId: int = troop.stub.id;
				retreatButton.addActionListener(function(e: Event):void {
					new RetreatTroopProcess(troop.stub, refresh).execute();
				},0, true);
				
				var pnlButtons: JPanel = new JPanel(new FlowLayout(AsWingConstants.RIGHT, 5, 0, false));
				retreatButton.setVerticalAlignment(AsWingConstants.TOP);
				pnlButtons.setConstraints("East");								
				pnlButtons.appendAll(retreatButton);
				pnl.append(pnlButtons);
			}
			
			return pnl;
		}

        private function refresh() : void {
            var self: StrongholdProfileDialog = this;
            Global.mapComm.Stronghold.viewStrongholdProfile(profileData.strongholdId, function (newProfileData: *) {
                if (newProfileData) {
                    self.profileData = newProfileData;
                    updateFromProfileData();
                }
            });
        }

		private function updateFromProfileData() : void {
            // Buttons
            btnViewBattle.setVisible(profileData.strongholdObjectState == SimpleGameObject.STATE_BATTLE);

            // Troops
            pnlTroopPanel.removeAll();

			for each (var troop: * in profileData.troops) {
                pnlTroopPanel.append(createTroopItem(troop));
			}

			if (profileData.troops.length == 0) {
                pnlTroopPanel.append(new JLabel(StringHelper.localize("STRONGHOLD_NO_TROOPS"), null, AsWingConstants.LEFT));
			}

            // Info
            pnlInfoContainer.removeAll();
            addInfo(pnlInfoContainer, StringHelper.localize("STR_LEVEL"), profileData.strongholdLevel.toString());
            addInfo(pnlInfoContainer, StringHelper.localize("STR_GATE"), Stronghold.gateToString(profileData.strongholdLevel, profileData.strongholdGate));
            var timediff: int = Global.map.getServerTime() - profileData.strongholdDateOccupied;
            addInfo(pnlInfoContainer, StringHelper.localize("STR_OCCUPIED"), DateUtil.niceDays(timediff));
            addInfo(pnlInfoContainer, StringHelper.localize("STR_VP_RATE"), StringHelper.localize("STR_PER_DAY_RATE", Util.roundNumber(profileData.strongholdVictoryPointRate).toString()));
            var s: TroopStub = new TroopStub();
            var f: Formation = new Formation(Formation.Defense);
            for each (troop in profileData.troops) {
                for each(var formation: Formation in troop.stub) {
                    if (formation.type != Formation.Defense || formation.size() == 0) continue;
                    for (var z: int = 0; z < formation.size(); z++) {
                        var u: Unit = formation.getByIndex(z);
                        var newUnit: Unit = new Unit(u.type, u.count);
                        f.add(newUnit);
                    }
                }
            }
            s.add(f);
            if (f.size() > 0) {
                addInfo(pnlInfoContainer, StringHelper.localize("STR_TOTAL_TROOPS"), new TroopCompositionGridList(s, 3, 0));
            }
            else {
                addInfo(pnlInfoContainer, StringHelper.localize("STR_TOTAL_TROOPS"), StringHelper.localize("STR_NONE_DEFENDING"));
            }
        }

		private function createReportPanel() : Container {
			reports = new LocalReportList(
				BattleReportViewer.REPORT_TRIBE_LOCAL, 
				[BattleReportListTable.COLUMN_DATE, BattleReportListTable.COLUMN_LOCATION, BattleReportListTable.COLUMN_ATTACK_TRIBES], 
				new BattleLocation(BattleLocation.STRONGHOLD, profileData.strongholdId));
							
			return reports;
		}
	}
	
}