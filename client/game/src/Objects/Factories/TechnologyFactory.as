package src.Objects.Factories {
    import src.Map.Map;
    import src.Objects.Prototypes.EffectPrototype;
    import src.Objects.Prototypes.TechnologyPrototype;
    import src.Objects.Resources;
    import src.Util.BinaryList.*;

    public class TechnologyFactory {

		private static var map: Map;
		private static var technologies: BinaryList;

		public static function init(_map: Map, data: XML):void
		{
			map = _map;

			technologies = new BinaryList(TechnologyPrototype.sortOnTypeAndLevel, TechnologyPrototype.compareTypeAndLevel);

			for each (var techNode: XML in data.Technologies.*)
			{
				var resources: Resources = new Resources(techNode.@crop, techNode.@gold, techNode.@iron, techNode.@wood, techNode.@labor);
				var tech: TechnologyPrototype = new TechnologyPrototype(techNode.@techtype, techNode.@level, resources, techNode.@time, techNode.@spriteclass);

				for each (var effectNode: XML in techNode.*)
				{
					var effect: EffectPrototype = new EffectPrototype();
					effect.effectCode = effectNode.@effect;
					effect.location = effectNode.@location;
					effect.isPrivate = effectNode.@private.toLowerCase() == 'true';
					effect.param1 = effectNode.@param1;
					effect.param2 = effectNode.@param2;
					effect.param3 = effectNode.@param3;
					effect.param4 = effectNode.@param4;
					effect.param5 = effectNode.@param5;

					tech.effects.add(effect, false);
				}

				tech.effects.sort();
				technologies.add(tech, false);
			}

			technologies.sort();
		}

		public static function getPrototype(type: int, level: int): TechnologyPrototype
		{
			return technologies.get([type, level]);
		}
	}

}

