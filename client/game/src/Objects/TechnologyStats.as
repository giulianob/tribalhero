/**
* ...
* @author Default
* @version 0.1
*/
package src.Objects {

    import src.Objects.Prototypes.EffectPrototype;
    import src.Objects.Prototypes.TechnologyPrototype;

    public class TechnologyStats {
		
		public var techPrototype: TechnologyPrototype;
		
		public var ownerLocation: int;
		public var ownerId: int;		
		
		public function TechnologyStats(prototype: TechnologyPrototype, ownerLocation: int , ownerId: int) {
			this.techPrototype = prototype;
			this.ownerId = ownerId;
			this.ownerLocation = ownerLocation;
		}
		
		public function getAllEffects(inheritance: int, location: int): Array
		{
			var effects: Array = [];
			
			var isSelf: Boolean = (inheritance & EffectPrototype.INHERIT_SELF) == EffectPrototype.INHERIT_SELF;
			var isInvisible: Boolean = (inheritance & EffectPrototype.INHERIT_INVISIBLE) == EffectPrototype.INHERIT_INVISIBLE;
			
			for each(var effect: EffectPrototype in techPrototype.effects)
			{
				if (effect.location == location)
				{
					if (isSelf && !effect.isPrivate)
						effects.push(effect);
					else if (isInvisible && effect.isPrivate)
						effects.push(effect);
				}
			}
			
			return effects;
		}
		
		public function getEffects(effectCode: int, inheritance: int, location: int): Array
		{
			var effects: Array = [];
			
			var isSelf: Boolean = (inheritance & EffectPrototype.INHERIT_SELF) == EffectPrototype.INHERIT_SELF;
			var isInvisible: Boolean = (inheritance & EffectPrototype.INHERIT_INVISIBLE) == EffectPrototype.INHERIT_INVISIBLE;
						
			var techEffects: Array = techPrototype.effects.getRange(effectCode);
			
			for each (var effect: EffectPrototype in techEffects)
			{				
				if (effect.location == location)
				{
					if (isSelf && !effect.isPrivate)
						effects.push(effect);
					else if (isInvisible && effect.isPrivate)
						effects.push(effect);
				}
			}
			
			return effects;
		}

	}
	
}
