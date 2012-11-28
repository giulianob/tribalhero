package src.UI.Components 
{
	import mx.utils.StringUtil;
	import src.Objects.Battle.BattleLocation;
	public class BattleLocationLabel extends RichLabel 
	{
		private var battleLocation:BattleLocation;
		
		public function BattleLocationLabel(battleLocation: BattleLocation) 
		{
			this.battleLocation = battleLocation;
			super();
			
			switch (battleLocation.type) {
				case BattleLocation.CITY:
					setHtmlText(StringUtil.substitute("<a href='event:goToCity:{0}'>{1}</a>", battleLocation.id, battleLocation.name));
					break;
				case BattleLocation.STRONGHOLD:
				case BattleLocation.STRONGHOLDGATE:
					setHtmlText(StringUtil.substitute("<a href='event:goToStronghold:{0}'>{1}</a>", battleLocation.id, battleLocation.name));
					break;					
				default:					
					break;
			}
			
			setColumns(battleLocation.name.length);			
		}
		
	}

}