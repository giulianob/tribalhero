package src.UI.Components.BattleReport 
{
	import flash.events.Event;
	import mx.utils.*;
	import org.aswing.*;
	import org.aswing.ext.*;
	import src.Comm.GameURLLoader;
	import src.Constants;
	import src.Global;
	import src.UI.Components.*;
	import src.UI.LookAndFeel.*;
	
	public class BattleImportantRound extends JPanel 
	{
		public static const EVENT_VIEW_SNAPSHOT: String = "EVENT_VIEW_SNAPSHOT";
		
		private var snapshot:*;
		private var lblRound:JLabelButton;
	
		public function BattleImportantRound(snapshot: *) 
		{
			this.snapshot = snapshot;
			
			createUI();			
			
			lblRound.addActionListener(function(e: Event): void {
				dispatchEvent(new ViewSnapshotEvent(EVENT_VIEW_SNAPSHOT, snapshot.id));				
			});
		}
		
		private function createUI():void 
		{
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 0));
			{
				lblRound = new JLabelButton(StringUtil.substitute("Round {0}, Turn {1}", int(snapshot.round) + 1, int(snapshot.turn) + 1), null, AsWingConstants.LEFT);
				GameLookAndFeel.changeClass(lblRound, "darkHeader");
				new SimpleTooltip(lblRound, "View Details");
				
				var pnlGrid: JPanel = new JPanel(new GridLayout(0, 2, 10));
				
				var pnlAttackerEvents: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
				var pnlDefenderEvents: JPanel = new JPanel(new SoftBoxLayout(SoftBoxLayout.Y_AXIS, 3));
				
				for each (var event:* in snapshot.events) {
					if (!event.isAttacker) {
						pnlDefenderEvents.append(new BattleEventRow(event));
					} else {
						pnlAttackerEvents.append(new BattleEventRow(event));						
					}
				}
				
				// Adds the "no events to display" message if no events are found
				addNoEventsMessageIfNeeded(pnlAttackerEvents);
				addNoEventsMessageIfNeeded(pnlDefenderEvents);
				
				// Add the Attackers (10 troops, etc..) message
				pnlAttackerEvents.insert(0, createEventHeader("Attackers", snapshot.attackerUnits, snapshot.attackerStructures, snapshot.attackerTroops)); 
				pnlDefenderEvents.insert(0, createEventHeader("Defenders", snapshot.defenderUnits, snapshot.defenderStructures, snapshot.defenderTroops));
				
				pnlGrid.appendAll(					
					pnlDefenderEvents,
					pnlAttackerEvents
				);
				
				appendAll(AsWingUtils.createPaneToHold(lblRound, new FlowLayout(AsWingConstants.LEFT, 0, 0, false)), pnlGrid);
			}			
		}		
		
		private function addNoEventsMessageIfNeeded(panel:JPanel):void 
		{
			if (panel.getComponentCount()) {
				return;
			}
			
			panel.append(new JLabel("No important events", null, AsWingConstants.LEFT));
		}
		
		private function createEventHeader(side:String, unitCount:int, structureCount:int, troopCount:int):Component
		{
			var header: MultilineLabel = new MultilineLabel("", 0, 30);

			var headerFormat: String;
			if (structureCount)
			{
				headerFormat = "<b>{0}</b> ({1} troops, {2} units, {3} structures)";
			}
			else
			{
				headerFormat = "<b>{0}</b> ({1} troops, {2} units)";
			}
			
			var text:String = StringUtil.substitute(headerFormat, side, troopCount, unitCount, structureCount);
			
			header.setHtmlText(text);
			
			return header;
		}
	}

}