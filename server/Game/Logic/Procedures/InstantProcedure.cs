using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Data;
using Game.Setup;

namespace Game.Logic.Procedures
{
    public class InstantProcedure
    {
        private readonly IObjectTypeFactory objectTypeFactory;

        public InstantProcedure(IObjectTypeFactory objectTypeFactory)
        {
            this.objectTypeFactory = objectTypeFactory;
        }

        public Boolean BuildNext(ICity city, IStructure structure)
        {
            if (objectTypeFactory.IsStructureType("NoInstantUpgrade", structure))
                return false;

            var cranny = city.FirstOrDefault(s => objectTypeFactory.IsStructureType("Cranny", s));

            if (cranny == null)
                return false;

            int count = (int)cranny["Structure Upgrades"];

            if (count <= 0)
                return false;

            cranny.BeginUpdate();
            cranny["Structure Upgrades"] = --count;
            cranny.EndUpdate();

            return true;
        }

        public Boolean ResearchNext(ICity city)
        {
            var cranny = city.FirstOrDefault(s => objectTypeFactory.IsStructureType("Cranny", s));

            if (cranny == null)
                return false;

            int count = (int)cranny["Technology Upgrades"];

            if (count <= 0)
                return false;

            cranny.BeginUpdate();
            cranny["Technology Upgrades"] = --count;
            cranny.EndUpdate();

            return true;
        }
    }
}
