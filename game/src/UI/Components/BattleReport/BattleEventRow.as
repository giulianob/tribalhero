package src.UI.Components.BattleReport
{
	import src.Util.StringHelper;
	import flash.events.*;
	import mx.utils.*;
	import org.aswing.*;
	import src.UI.Components.*;
	import src.UI.Tooltips.*;
	import src.Util.StringHelper;
	
	/**
	 * Represents a single item in the battle overview event. E.g.
	 * "Blabla has left the battle"
	 */
	public class BattleEventRow extends JPanel
	{
		private static const EVENT_STATES:Array = [
			'<a href="event:viewProfileByType:{0}:{1}">{2} ({3})</a> has joined with <a href="event:custom:viewTroop">{4}</a> units', 
			'<a href="event:viewProfileByType:{0}:{1}">{2} ({3})</a> remains with <a href="event:custom:viewTroop">{4}</a> units',
			'<a href="event:viewProfileByType:{0}:{1}">{2} ({3})</a> has left with <a href="event:custom:viewTroop">{4}</a> units',
			'<a href="event:viewProfileByType:{0}:{1}">{2} ({3})</a> has died',
			'<a href="event:viewProfileByType:{0}:{1}">{2} ({3})</a> has retreated with <a href="event:custom:viewTroop">{4}</a> units',
			'<a href="event:viewProfileByType:{0}:{1}">{2} ({3})</a> has been reinforced with <a href="event:custom:viewTroop">{4}</a> units',
			'<a href="event:viewProfileByType:{0}:{1}">{2} ({3})</a> ran out of stamina, left with <a href="event:custom:viewTroop">{4}</a> units'
		];
		
		private var totalUnits:int = 0;
		private var event:*;
		private var lblHeader:RichLabel;
		
		private var troopTooltip: Tooltip;
		
		public function BattleEventRow(event:*)
		{
			this.event = event;
			
			for each (var unit:*in event.units)
			{
				totalUnits += int(unit.count);
			}
			
			createUI();
		}
		
		private function createUI():void
		{
			setLayout(new SoftBoxLayout(SoftBoxLayout.Y_AXIS));
			{
				var header: String = StringUtil.substitute(EVENT_STATES[event.state], event.owner.type, event.owner.id, StringHelper.htmlEscape(event.owner.name), event.name == '[LOCAL]' ? StringHelper.localize("LOCAL_TROOP") : event.name, totalUnits);
				lblHeader = new RichLabel(header, 0, 30);
				lblHeader.addEventListener(RichLabelCustomEvent.CUSTOM_EVENT_MOUSE_OVER, customEventMouseOver);
				lblHeader.addEventListener(MouseEvent.MOUSE_OUT, eventMouseOut);
				
				appendAll(lblHeader);				
			}
		}
		
		private function eventMouseOut(e:MouseEvent):void 
		{
			if (troopTooltip) {
				troopTooltip.hide();
			}
		}
		
		public function customEventMouseOver(e: RichLabelCustomEvent):void 
		{
			switch(e.eventName)
			{
				case 'viewTroop':
					if (!troopTooltip) {
						troopTooltip = new Tooltip();
						troopTooltip.getUI().append(new BattleEventTroopGridList(event.units));
					}
					
					troopTooltip.show(lblHeader);
					break;
				default:
					if (troopTooltip) {
						troopTooltip.hide();
					}
			}
		}
	}

}