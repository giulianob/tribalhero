#region

using Game.Comm;
using Game.Data;
using Game.Logic.Formulas;
using Game.Setup;
using Ninject;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public static void RecalculateCityResourceRates(City city)
        {
            city.Resource.Crop.Rate = Formula.GetCropRate(city);
            city.Resource.Iron.Rate = Formula.GetIronRate(city);
            city.Resource.Wood.Rate = Formula.GetWoodRate(city);
        }

        public static void OnStructureUpgradeDowngrade(Structure structure)
        {
            SetResourceCap(structure.City);
            RecalculateCityResourceRates(structure.City);
        }

        public static void OnTechnologyChange(Structure structure)
        {
            structure.City.BeginUpdate();
            SetResourceCap(structure.City);
            structure.City.EndUpdate();
        }

        public static void OnSessionTribesmanQuit(Session session, uint tribeId, uint playerId, bool isKicked)
        {
            if (session != null)
            {
                Global.Channel.Unsubscribe(session, "/TRIBE/" + tribeId);
                if(isKicked)
                {
                    session.Write(new Packet(Command.TribesmanKicked));
                }
            }
        }
    }
}