package src.UI.Components.BattleReport
{
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
        private static const TYPE_LABELS: * = {
            'stronghold': '<a href="event:viewProfileByType:{0}:{1}">{2} ({3})</a>',
            'city': '<a href="event:viewProfileByType:{0}:{1}">{2} ({3})</a>',
            'barbariantribe': '{2}'
        };
        
		private static const EVENT_STATES:Array = [
			'{0} has joined with <a href="event:custom:viewTroop">{1}</a> units', 
			'{0} remains with <a href="event:custom:viewTroop">{1}</a> units',
			'{0} has left with <a href="event:custom:viewTroop">{1}</a> units',
			'{0} has died',
			'{0} has retreated with <a href="event:custom:viewTroop">{1}</a> units',
			'{0} has been reinforced with <a href="event:custom:viewTroop">{1}</a> units',
			'{0} ran out of stamina, left with <a href="event:custom:viewTroop">{1}</a> units'
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
                var locationHtml: String = StringUtil.substitute(TYPE_LABELS[event.owner.type.toLowerCase()], event.owner.type, event.owner.id, StringHelper.htmlEscape(event.owner.name), event.name == '[LOCAL]' ? StringHelper.localize("LOCAL_TROOP") : event.name);
				var header: String = StringUtil.substitute(EVENT_STATES[event.state], locationHtml, totalUnits);                
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