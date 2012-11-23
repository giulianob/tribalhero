/**
* ...
* @author Default
* @version 0.1
*/
package src.Objects {

	import src.Objects.Prototypes.EffectPrototype;
	import src.Objects.Prototypes.TechnologyPrototype;
	import src.Util.Util;

	public class TechnologyStats {
		
		public var prototype: TechnologyPrototype;
		
		public var ownerLocation: int;
		public var ownerId: int;		
		
		public function TechnologyStats(prototype: TechnologyPrototype, ownerLocation: int , ownerId: int) {
			this.prototype = prototype;
			this.ownerId = ownerId;
			this.ownerLocation = ownerLocation;
		}
		
		public function getAllEffects(inheritance: int, location: int): Array
		{
			var effects: Array = new Array();
			
			var isSelf: Boolean = (inheritance & EffectPrototype.INHERIT_SELF) == EffectPrototype.INHERIT_SELF;
			var isInvisible: Boolean = (inheritance & EffectPrototype.INHERIT_INVISIBLE) == EffectPrototype.INHERIT_INVISIBLE;
			
			for each(var effect: EffectPrototype in prototype.effects)
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
			var effects: Array = new Array();
			
			var isSelf: Boolean = (inheritance & EffectPrototype.INHERIT_SELF) == EffectPrototype.INHERIT_SELF;
			var isInvisible: Boolean = (inheritance & EffectPrototype.INHERIT_INVISIBLE) == EffectPrototype.INHERIT_INVISIBLE;
						
			var techEffects: Array = prototype.effects.getRange(effectCode);
			
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
