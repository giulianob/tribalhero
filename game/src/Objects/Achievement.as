package src.Objects 
{
    import flash.display.DisplayObject;

    import src.Assets;

    public class Achievement
	{
		public static const TIER_GOLD: int = 0;
		public static const TIER_SILVER: int = 1;
		public static const TIER_BRONZE: int = 2;
		public static const TIER_HONORARY: int = 3;
		
		public var description:String;
		public var title:String;
		public var icon:String;
		public var tier:int;
		public var type:String;
		public var id:int;
		
		public function Achievement(id: int, type: String, tier: int, icon: String, title: String, description: String) 
		{
			this.description = description;
			this.title = title;
			this.icon = icon;
			this.tier = tier;
			this.type = type;
			this.id = id;			
		}
		
		public function getTierName(): String
		{
			switch (tier) {
				case TIER_GOLD:
					return "GOLD";
				case TIER_SILVER:
					return "SILVER";					
				case TIER_BRONZE:
					return "BRONZE";
				case TIER_HONORARY:
					return "HONORARY";
				default:
					throw new Error("Unknown tier type" + tier);
			}
		}
		
		public function getSprite(): DisplayObject
		{
			return Assets.getInstance("ICON_ACHIEVEMENT_" + icon + "_" + getTierName());
		}
	}

}