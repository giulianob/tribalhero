package src.UI.Dialog
{
    import flash.events.*;

    import org.aswing.*;
    import org.aswing.border.*;

    import src.*;
    import src.Comm.*;
    import src.Objects.*;
    import src.UI.*;
    import src.UI.Components.*;
    import src.UI.Components.BattleReport.*;
    import src.UI.LookAndFeel.*;
    import src.UI.Tooltips.*;
    import src.Util.*;

    public class BattleReportViewer extends GameJPanel
	{
		public static const REPORT_CITY_LOCAL: int = 1;
		public static const REPORT_CITY_FOREIGN: int = 2;
		public static const REPORT_TRIBE_LOCAL: int = 3;
		public static const REPORT_TRIBE_FOREIGN: int = 4;
		
		private var pnlResources:JPanel;
		private var pnlGroupOutcomeTableHolder:JPanel;
		
		private var pnlBattleStatsLooted:JPanel;
		private var lblBattleStatsTime:JLabel;
		private var lblBattleStatsStructuresDestroyed:JLabel;
		private var lblBattleStatsAttackUnits:JLabel;
		private var lblBattleStatsDefenseUnits:JLabel;
		private var lblBattleStatsTribes:JLabel;
		private var tooltipBattleStatsTribes:Tooltip;
		private var pnlBattleStatsTribesDefenders:JPanel;
		private var pnlBattleStatsTribesAttackers:JPanel;
		private var pnlImportantEvents:JPanel;
		private var lblLoadMoreEvents:JLabelButton;		
		
		private var loader:GameURLLoader = new GameURLLoader();
		private var eventLoader:GameURLLoader = new GameURLLoader();
		private var id:int;
		private var viewType:int;
		private var playerNameFilter:String;
		private var currentPage: int = 0;
		private var pnlGroupOutcome:JPanel;
		private var pnlBattleStats:JPanel;
		private var pnlHolder:JPanel;
		private var scrollPanel:JScrollPane;
		private var lblOutcomeOnly:JLabel;
		private var snapshotLoader: GameURLLoader;
		
		public var refreshOnClose:Boolean;
		
		public function BattleReportViewer(id:int, playerNameFilter:String, viewType:int)
		{
			this.id = id;
			this.viewType = viewType;
			this.playerNameFilter = playerNameFilter;	
			
			snapshotLoader = new GameURLLoader();
			snapshotLoader.addEventListener(Event.COMPLETE, onLoadSnapshot);
			
			loader.addEventListener(Event.COMPLETE, onLoadedReport);
			eventLoader.addEventListener(Event.COMPLETE, onLoadedEvents);
		}
		
		private function load():void
		{
			Global.mapComm.BattleReport.viewReport(loader, id, playerNameFilter, viewType);
		}
		
		private function onLoadSnapshot(e:Event):void 
		{
			var data:*;
			try
			{
				data = snapshotLoader.getDataAsObject();
			}
			catch (e:Error)
			{
				InfoDialog.showMessageDialog("Error", "Unable to query snapshot. Log out then back in if this problem persists.");
				return;
			}
			
			var snapshotDialog: Snapshot = new Snapshot(data);
			snapshotDialog.show();
		}		
		
		private function loadMoreEvents():void
		{
			Global.mapComm.BattleReport.viewMoreEvents(eventLoader, id, playerNameFilter, viewType, currentPage);
		}		
		
		private function onLoadedEvents(e:Event):void
		{
			var data:*;
			try
			{
				data = eventLoader.getDataAsObject();
			}
			catch (e:Error)
			{
				InfoDialog.showMessageDialog("Error", "Unable to query report. Refresh the page if this problem persists");
				return;
			}
			
			parseEvents(data);
		}
		
		private function resizeToContents(): void {
			getFrame().pack();
			getFrame().resizeToContents();
			Util.centerFrame(getFrame());
		}
		
		private function onLoadedReport(e:Event):void
		{
			// We delay UI creation until there is data
			createUI();
			
			var data:*;
			try
			{
				data = loader.getDataAsObject();
			}
			catch (e:Error)
			{
				InfoDialog.showMessageDialog("Error", "Unable to query report. Refresh the page if this problem persists");
				return;
			}
			
			refreshOnClose = data.refreshOnClose;
			
			lblOutcomeOnly.setVisible(data.outcomeOnly);
			
			// Show resources gained if applicable
			if (data.loot)
			{
				var loot:Resources = new Resources(data.loot.crop, data.loot.gold, data.loot.iron, data.loot.wood, 0);
				var bonus:Resources = new Resources(data.loot.bonus.crop, data.loot.bonus.gold, data.loot.bonus.iron, data.loot.bonus.wood, 0);
				var total:Resources = Resources.sum(loot, bonus);
				pnlResources.appendAll(new ResourcesPanel(total, null, false, false));
				new BattleLootTooltip(pnlResources, loot, bonus);
			}
			
			// Show overall battle stats
			if (!data.outcomeOnly) {
				var totalBattleLoot:Resources = new Resources(data.battleOutcome.totalCropLooted, data.battleOutcome.totalGoldLooted, data.battleOutcome.totalIronLooted, data.battleOutcome.totalWoodLooted, 0);
				pnlBattleStatsLooted.appendAll(new JLabel("Total loot stolen", null, AsWingConstants.LEFT), new ResourcesPanel(totalBattleLoot, null, false, false));
				lblBattleStatsAttackUnits.setText("Attack lost " + (data.battleOutcome.attackerJoinCount - data.battleOutcome.attackerLeaveCount) + " out of " + (data.battleOutcome.attackerJoinCount) + " units");
				lblBattleStatsDefenseUnits.setText("Defender lost " + (data.battleOutcome.defenderJoinCount - data.battleOutcome.defenderLeaveCount) + " out of " + (data.battleOutcome.defenderJoinCount) + " units");
				lblBattleStatsStructuresDestroyed.setText(StringHelper.makePlural(data.battleOutcome.destroyedStructures, "1 structure was", data.battleOutcome.destroyedStructures + " structures were", "No structures were") + " knocked down");
				lblBattleStatsTime.setText("Battle lasted " + DateUtil.niceTime(data.battleOutcome.timeLasted));
				
				var tribesCount: int = data.battleOutcome.attackerTribes.length + data.battleOutcome.defenderTribes.length;
				lblBattleStatsTribes.setText(tribesCount + " tribe(s) participated");
							
				if (tribesCount > 0) {
					for each (var defenderTribe:Object in data.battleOutcome.defenderTribes)
					{
						var lblTribeDefender:JLabel = new JLabel(defenderTribe.name, null, AsWingConstants.LEFT);
						GameLookAndFeel.changeClass(lblTribeDefender, "Tooltip.text");
						pnlBattleStatsTribesDefenders.append(lblTribeDefender);
					}
					
					for each (var attackerTribe:Object in data.battleOutcome.attackerTribes)
					{
						var lblTribeAttacker:JLabel = new JLabel(attackerTribe.name, null, AsWingConstants.LEFT);
						GameLookAndFeel.changeClass(lblTribeAttacker, "Tooltip.text");
						pnlBattleStatsTribesAttackers.append(lblTribeAttacker);
					}					
					tooltipBattleStatsTribes.bind(lblBattleStatsTribes);			
					tooltipBattleStatsTribes.getUI().pack();
				}
			}
			else {
				pnlHolder.remove(pnlBattleStats);
			}
			
			// Show group outcome
			if (data.hasOwnProperty("playerOutcome")) {
				var groupOutcome:TroopTable = new TroopTable(data.playerOutcome);
				pnlGroupOutcomeTableHolder.append(groupOutcome);
			}
			else {
				pnlGroupOutcome.setVisible(false);
			}
			
			// Show events
			if (!data.outcomeOnly) {
				parseEvents(data.battleEvents);
			}
			else {
				pnlHolder.remove(pnlImportantEvents);
			}			
			
			// Resize
			resizeToContents();
		}
		
		private function parseEvents(battleEvents: *):void {
			// Show important battle events
			for each (var snapshot:* in battleEvents.reports) {				
				var importantRound: BattleImportantRound = new BattleImportantRound(snapshot);
				importantRound.addEventListener(BattleImportantRound.EVENT_VIEW_SNAPSHOT, function(e: ViewSnapshotEvent): void {
					Global.mapComm.BattleReport.viewSnapshot(snapshotLoader, id, playerNameFilter, viewType, e.reportId);
				});
				pnlImportantEvents.append(importantRound);
			}
			
			// Decide whether there are more pages to load
			currentPage++;
			lblLoadMoreEvents.setVisible(currentPage < battleEvents.pages);
		}
		
		public function show(owner:* = null, modal:Boolean = true, onClose:Function = null):JFrame
		{
			super.showSelf(owner, modal, onClose);
			Global.gameContainer.showFrame(frame);
			frame.setTitle("Battle Report");
			
			load();
			
			return frame;
		}
		
		public function createUI():void
		{
			setLayout(new BorderLayout());
			{
				scrollPanel = new JScrollPane();
				{
					pnlHolder = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 10));
					{
						lblOutcomeOnly = new JLabel("Your troop wasn't in the battle long enough to scout the enemies. You can only see a partial report.");
						
						// Battle stats
						pnlBattleStats = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
						{
							var lblBattleStatsTitle:JLabel = new JLabel("Battle Statistics", null, AsWingConstants.LEFT);
							GameLookAndFeel.changeClass(lblBattleStatsTitle, "darkSectionHeader");
							
							var pnlBattleStatsGrid:JPanel = new JPanel(new GridLayout(3, 2, 5, 0));
							{
								pnlBattleStatsLooted = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0, AsWingConstants.LEFT));
								lblBattleStatsTime = new JLabel("", null, AsWingConstants.LEFT);
								lblBattleStatsStructuresDestroyed = new JLabel("", null, AsWingConstants.LEFT);
								lblBattleStatsAttackUnits = new JLabel("", null, AsWingConstants.LEFT);
								lblBattleStatsDefenseUnits = new JLabel("", null, AsWingConstants.LEFT);
								lblBattleStatsTribes = new JLabel("", null, AsWingConstants.LEFT);
								
								pnlBattleStatsGrid.appendAll(pnlBattleStatsLooted, lblBattleStatsStructuresDestroyed);
								pnlBattleStatsGrid.appendAll(lblBattleStatsTime, lblBattleStatsAttackUnits);
								pnlBattleStatsGrid.appendAll(lblBattleStatsTribes, lblBattleStatsDefenseUnits);
							}
							
							pnlBattleStats.appendAll(lblBattleStatsTitle, pnlBattleStatsGrid);
						}
						
						// Group Outcome
						pnlGroupOutcome = new JPanel(new BorderLayout());
						{
							var pnlGroupOutcomeHeader:JPanel = new JPanel(new BorderLayout(5));
							pnlGroupOutcomeHeader.setConstraints("North");
							{
								var lblGroupOutcomeTitle:JLabel = new JLabel("Your Troops Outcome", null, AsWingConstants.LEFT);
								lblGroupOutcomeTitle.setConstraints("Center");
								GameLookAndFeel.changeClass(lblGroupOutcomeTitle, "darkSectionHeader");
								
								pnlResources = new JPanel();
								pnlResources.setConstraints("East");
								
								pnlGroupOutcomeHeader.appendAll(lblGroupOutcomeTitle, pnlResources);
							}
							
							pnlGroupOutcomeTableHolder = new JPanel();
							pnlGroupOutcomeTableHolder.setConstraints("Center");
							
							pnlGroupOutcome.appendAll(pnlGroupOutcomeHeader, pnlGroupOutcomeTableHolder);
						}
						
						// Battle Events
						pnlImportantEvents = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
						{
							var lblImportantEventsTitle:JLabel = new JLabel("Important Events", null, AsWingConstants.LEFT);
							GameLookAndFeel.changeClass(lblImportantEventsTitle, "darkSectionHeader");
							
							pnlImportantEvents.appendAll(lblImportantEventsTitle, lblLoadMoreEvents);							
						}
						
						// Load more events label
						lblLoadMoreEvents = new JLabelButton("View more events", null, AsWingConstants.CENTER);
						lblLoadMoreEvents.setVisible(false);
						lblLoadMoreEvents.addActionListener(function(e: Event):void {
							loadMoreEvents();
						});						
						
						pnlHolder.appendAll(lblOutcomeOnly, pnlBattleStats, pnlGroupOutcome, pnlImportantEvents, lblLoadMoreEvents);
					}
				}
				
				// Tooltip that shows the battles that participated
				tooltipBattleStatsTribes = new Tooltip();				
				var pnlTooltipBattleStatsTribes:JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.X_AXIS, 15));
				{
					pnlBattleStatsTribesDefenders = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
					{
						var lblBattleStatsTribesDefenders:JLabel = new JLabel("Defenders", null, AsWingConstants.LEFT);
						GameLookAndFeel.changeClass(lblBattleStatsTribesDefenders, "Tooltip.text header");
						pnlBattleStatsTribesDefenders.append(lblBattleStatsTribesDefenders);
					}
					pnlBattleStatsTribesAttackers = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 5));
					{
						var lblBattleStatsTribesAttackers:JLabel = new JLabel("Attackers", null, AsWingConstants.LEFT);
						GameLookAndFeel.changeClass(lblBattleStatsTribesAttackers, "Tooltip.text header");
						pnlBattleStatsTribesAttackers.append(lblBattleStatsTribesAttackers);
					}
					pnlTooltipBattleStatsTribes.appendAll(pnlBattleStatsTribesDefenders, pnlBattleStatsTribesAttackers);
					
					tooltipBattleStatsTribes.getUI().append(pnlTooltipBattleStatsTribes);
				}				
				
				var viewport:JViewport = new JViewport(pnlHolder, true, false);
				viewport.setBorder(new EmptyBorder(null, new Insets(0, 0, 0, 5)));
				viewport.setVerticalAlignment(AsWingConstants.TOP);				
				scrollPanel.setViewport(viewport);
				scrollPanel.setConstraints("Center");
				append(scrollPanel);
			}
		}
	}

}

