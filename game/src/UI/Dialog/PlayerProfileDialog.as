package src.UI.Dialog 
{
	import com.greensock.loading.core.DisplayObjectLoader;
	import fl.lang.*;
	import flash.events.*;
	import mx.utils.StringUtil;
	import org.aswing.*;
	import org.aswing.border.*;
	import org.aswing.colorchooser.*;
	import org.aswing.ext.*;
	import org.aswing.geom.*;
	import src.*;
	import src.Map.City;
	import src.Objects.Achievement;
	import src.Objects.Effects.Formula;
	import src.Objects.Tribe;
	import src.UI.*;
	import src.UI.Components.*;
	import src.UI.LookAndFeel.*;
	import src.Util.StringHelper;
	import src.Util.Util;
	
	public class PlayerProfileDialog extends GameJPanel
	{
		private var profileData: * ;
		
		private var btnSendMessage: JLabelButton;
		private var btnInviteTribe: JLabelButton;
		private var btnSetDescription: JLabelButton;
		
		public function PlayerProfileDialog(profileData: *) 
		{
			this.profileData = profileData;
			
			createUI();
			
			btnSendMessage.addActionListener(function(e: Event = null): void {
				var messageDialog: MessageCreateDialog = new MessageCreateDialog(function(sender: MessageCreateDialog):void {
					sender.getFrame().dispose();
				}, profileData.username);
				
				messageDialog.show();
			});
			
			btnSetDescription.addActionListener(function(e: Event = null): void {
				
				var pnl: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
				var txtDescription: JTextArea = new JTextArea(profileData.description, 10, 10);
				GameLookAndFeel.changeClass(txtDescription, "Message");
				txtDescription.setMaxChars(3000);
				
				var scrollDescription: JScrollPane = new JScrollPane(txtDescription, JScrollPane.SCROLLBAR_AS_NEEDED, JScrollPane.SCROLLBAR_AS_NEEDED);			
			
				pnl.appendAll(new JLabel("Set a message to appears on your profile. This will be visible to everyone.", null, AsWingConstants.LEFT), scrollDescription);
				InfoDialog.showMessageDialog("Say something about yourself", pnl, function(result: *): void {			
					if (result == JOptionPane.CANCEL || result == JOptionPane.CLOSE)
						return;									
					
					Global.mapComm.City.setPlayerDescription(txtDescription.getText());
					
					getFrame().dispose();
					
					Global.mapComm.City.viewPlayerProfile(profileData.playerId, function(newProfileData: *): void {
						if (!newProfileData) 
							return;
			
						var dialog: PlayerProfileDialog = new PlayerProfileDialog(newProfileData);
						dialog.show();
					});
				});
			});
			
			btnInviteTribe.addActionListener(function(e: Event = null): void {
				Global.mapComm.Tribe.invitePlayer(profileData.username);
			});
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose: Function = null):JFrame 
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.closeAllFramesByType(PlayerProfileDialog);
			Global.gameContainer.showFrame(frame);
			return frame;
		}		
		
		private function createRanking(rank: * ): JLabel {
			var iconClass: Class = Constants.rankings[rank.type].icon;
			var icon: AssetIcon = new AssetIcon(new iconClass());
			
			var lblRanking: JLabel = new JLabel("#" + rank.rank, icon, AsWingConstants.LEFT);
			lblRanking.setPreferredWidth(60);
			new SimpleTooltip(lblRanking, Constants.rankings[rank.type].desc);
			return lblRanking;
		}
		
		private function createAchievement(achievement: Achievement):Component {
			var lbl: JLabel = new JLabel(achievement.title, new AssetIcon(achievement.getSprite()));
			lbl.setPreferredWidth(120);
			lbl.setVerticalTextPosition(AsWingConstants.BOTTOM);
			lbl.setHorizontalTextPosition(AsWingConstants.CENTER);
			new SimpleTooltip(lbl, achievement.description, achievement.title);
			
			return lbl;			
		}
		
		private function createAchievementsTab():Component {
			profileData.achievements.sortOn(["tier", "id"], [Array.NUMERIC, Array.NUMERIC | Array.DESCENDING]);
			
			var pnlAchievements: JPanel = new JPanel(new FlowWrapLayout(375 /* Achie. Width Above times 3 */, AsWingConstants.LEFT, 15, 10, false));
			pnlAchievements.setBorder(new EmptyBorder(null, new Insets(15, 0, 0, 15)));
			
			for each (var achievement: Achievement in profileData.achievements) {
				pnlAchievements.append(createAchievement(achievement));
			}
			
			return Util.createTopAlignedScrollPane(pnlAchievements);
		}
		
		private function createCitiesTab():Component {
			var pnlCities: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
			
			for each (var city: * in profileData.cities) {
				var pnlCity: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 5, 5, false));
				var lblCityName: CityLabel = new CityLabel(city.id, city.name);
				GameLookAndFeel.changeClass(lblCityName, "darkHeader");
				lblCityName.setPreferredWidth(125);
				
				// Build the list of "City1 - About 10 min away"
				var distanceMsg: String = "";
				
				for each (var myCity: City in Global.map.cities) {
					var distance: int = myCity.MainBuilding.distance(city.x, city.y);
					var timeAwayInSeconds: int = Formula.moveTimeTotal(myCity, 12, distance, true);
		
					distanceMsg += StringHelper.localize("PLAYER_PROFILE_DIALOG_CITY_DISTANCE", myCity.name, Util.niceTime(timeAwayInSeconds)) + "\n";
				}
				
				var cityDistanceTooltip: SimpleTooltip = new SimpleTooltip(lblCityName, distanceMsg);
				var footerDistanceTooltipLabel: MultilineLabel = new MultilineLabel(StringHelper.localize("PLAYER_PROFILE_DIALOG_CITY_DISTANCE_FOOTER"), 0, 20);
				GameLookAndFeel.changeClass(footerDistanceTooltipLabel, "Tooltip.italicsText");
				cityDistanceTooltip.append(footerDistanceTooltipLabel);				
				
				var pnlCityRanking: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 5, 0, false));
				for each (var rank: * in profileData.ranks) {
					if (rank.cityId != city.id) 
						continue;
					
					var lblRanking: JLabel = createRanking(rank);
					pnlCityRanking.append(lblRanking);
				}
				
				lblCityName.setConstraints("Center");
				pnlCityRanking.setConstraints("East");
				pnlCity.appendAll(lblCityName, pnlCityRanking);
				
				pnlCities.append(pnlCity);
			}
			
			return Util.createTopAlignedScrollPane(pnlCities);
		}
		
		private function createUI():void {
			setPreferredSize(new IntDimension(825, 375));
			title = "User Profile - " + profileData.username;
			setLayout(new BorderLayout(5));
			
			// Header panel
			var pnlHeader: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
			pnlHeader.setConstraints("North");
			
			var lblPlayerName: JLabel = new JLabel(profileData.username + (Constants.admin ? '['+profileData.playerId+']' : ''), null, AsWingConstants.LEFT);	
			lblPlayerName.setConstraints("Center");
			GameLookAndFeel.changeClass(lblPlayerName, "darkHeader");			
			
			var pnlRankings: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 5, 0, false));
			pnlRankings.setConstraints("East");
			
			for each (var rank: * in profileData.ranks) {
				// Only show ranking for player here
				if (rank.cityId != 0) 
					continue;
					
				var lblRanking: JLabel = createRanking(rank);
				pnlRankings.append(lblRanking);
			}
			
			var pnlActions: JPanel = new JPanel(new FlowLayout(AsWingConstants.LEFT, 10, 0, false));
			btnSendMessage = new JLabelButton("Send Message");
			btnSetDescription = new JLabelButton("Set Description");
			btnInviteTribe = new JLabelButton("Invite to Tribe");
			
			// Show correct buttons depending on who is viewing this profile
			if (Constants.playerId == profileData.playerId)
				pnlActions.append(btnSetDescription);
			else {
				pnlActions.append(btnSendMessage);
				if (profileData.tribeId == 0 && Constants.tribe.hasRight(Tribe.INVITE))
					pnlActions.append(btnInviteTribe);
			}
			
			pnlHeader.appendAll(lblPlayerName);
			if (Constants.playerId == profileData.playerId) {
				var invitationCode: JLabel = new JLabel("Invitation Code: " + Constants.playerHash);
				new SimpleTooltip(invitationCode, StringHelper.localize("PLAYER_PROFILE_INVITATION_TOOLTIP"));
				invitationCode.setHorizontalAlignment(AsWingConstants.LEFT);
				pnlHeader.append(invitationCode);
			}
			if (profileData.tribeId > 0) {
				var lblTribe: RichLabel = new RichLabel(StringUtil.substitute(
					'<a href="event:viewTribeProfile:{0}">{1}</a> ({2})', 
					profileData.tribeId, 
					profileData.tribeName, 
					profileData.tribeRank));
					
				pnlHeader.append(lblTribe);
			}
			
			pnlHeader.appendAll(new JLabel(" "), pnlRankings, new JLabel(" "), pnlActions);
			
			// description
			var description: String = profileData.description == "" ? "This player hasn't written anything about themselves yet" : profileData.description;
			var lblDescription: MultilineLabel = new MultilineLabel(description);
			GameLookAndFeel.changeClass(lblDescription, "Message");
			lblDescription.setPreferredWidth(325);
			lblDescription.setBackgroundDecorator(new GamePanelBackgroundDecorator("TabbedPane.top.contentRoundImage"));
			lblDescription.setBorder(new EmptyBorder(null, UIManager.get("TabbedPane.contentMargin") as Insets));
			
			var scrollDescription: JScrollPane = new JScrollPane(new JViewport(lblDescription));
			(scrollDescription.getViewport() as JViewport).setVerticalAlignment(AsWingConstants.TOP);
			(scrollDescription.getViewport() as JViewport).setHorizontalAlignment(AsWingConstants.LEFT);
			scrollDescription.setConstraints("Center");
			
			// Create west panel
			var pnlWest: JPanel = new JPanel(new BorderLayout());
			pnlWest.setConstraints("West");
			pnlWest.append(pnlHeader);
			pnlWest.append(scrollDescription);
			
			// Tab panel
			var pnlTabs: JTabbedPane = new JTabbedPane();
			pnlTabs.setPreferredSize(new IntDimension(400, 350));
			pnlTabs.setConstraints("Center");
			
			
			// Append tabs			
			pnlTabs.appendTab(createCitiesTab(), "Cities (" + profileData.cities.length + ")");			
			pnlTabs.appendTab(createAchievementsTab(), "Achievements (" + profileData.achievements.length + ")");			
			
			// Append main panels
			append(pnlWest);
			append(pnlTabs);
		}
	}
	
}