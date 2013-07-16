package src.Objects {

	/**
	 * ...
	 * @author Default
	 */

    import src.Objects.Prototypes.EffectPrototype;

    public class TechnologyManager {

		public var technologies: Array = [];
		public var location: int;
		public var parent: TechnologyManager;

		public function TechnologyManager(location: int, parent: TechnologyManager = null) {
			this.location = location;
			this.parent = parent;
		}

		public function add(tech: TechnologyStats, notify: Boolean = true):void
		{
			if (tech.ownerLocation != location)
			{
                if (tech.getAllEffects(EffectPrototype.INHERIT_SELF, location).length == 0) {
				    return; //do not add effects that do not affect this location
                }
			}

			//check for duplicates
			var dupe: Boolean = false;
			for each(var currTech: TechnologyStats in technologies)
			{
				if (currTech.ownerId == tech.ownerId && currTech.ownerLocation == tech.ownerLocation && currTech.techPrototype.techtype == tech.techPrototype.techtype)
				{
					dupe = true;
					break;
				}
			}

			if (!dupe)
			technologies.push(tech);

			if (parent != null && notify)
			parent.add(tech);
		}

		public function find(techtype: int): TechnologyStats
		{
			for each(var currTech: TechnologyStats in technologies)
			{
				if (currTech.ownerLocation != EffectPrototype.LOCATION_OBJECT)
				continue;

				if (currTech.techPrototype.techtype == techtype)
				return currTech;
			}

			return null;
		}

		public function remove(tech: TechnologyStats, notify: Boolean = true):void
		{
			for (var i: int = 0; i < technologies.length; i++)
			{
				var currentTechStats: TechnologyStats = technologies[i];
				if (currentTechStats.techPrototype.techtype == tech.techPrototype.techtype && currentTechStats.ownerId == tech.ownerId && currentTechStats.ownerLocation == tech.ownerLocation)
				{
					technologies.splice(i, 1);
					break;
				}
			}

			if (parent != null && notify)
			parent.remove(tech);
		}

		public function clear():void
		{
			for each (var tech: TechnologyStats in technologies)
				parent.remove(tech);
			
			technologies = [];
		}

		public function update(technologyStats: TechnologyStats):void
		{
			remove(technologyStats, false);
			add(technologyStats, false);

			if (parent != null)
			parent.update(technologyStats);
		}

		public function getAllEffects(inheritance: int): Array
		{
			var ret: Array = [];

			var i: int;
			var f: Array;
			for each(var tech: TechnologyStats in technologies)
			{
				f = tech.getAllEffects(inheritance, location);

				for (i = 0; i < f.length; i++)
				ret.push(f[i]);
			}

			if ( (inheritance & EffectPrototype.INHERIT_UPWARD) == EffectPrototype.INHERIT_UPWARD )
			{
				if (parent != null)
				{
					f = parent.getAllEffects(EffectPrototype.INHERIT_SELF | EffectPrototype.INHERIT_UPWARD);

					for (i = 0; i < f.length; i++)
					ret.push(f[i]);
				}

			}

			return ret;
		}

		public function getEffects(effectCode: int, inheritance: int): Array
		{
			var ret: Array = [];

			var i: int;
			var f: Array;
			for each(var tech: TechnologyStats in technologies)
			{
				f = tech.getEffects(effectCode, inheritance, location);

				for (i = 0; i < f.length; i++)
				ret.push(f[i]);
			}

			if ( (inheritance & EffectPrototype.INHERIT_UPWARD) == EffectPrototype.INHERIT_UPWARD )
			{
				if (parent != null)
				{
					f = parent.getEffects(effectCode, EffectPrototype.INHERIT_SELF | EffectPrototype.INHERIT_UPWARD);

					for (i = 0; i < f.length; i++)
					ret.push(f[i]);
				}
			}

			return ret;
		}

    }

}
