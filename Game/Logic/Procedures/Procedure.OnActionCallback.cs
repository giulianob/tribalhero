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
        public virtual void RecalculateCityResourceRates(ICity city)
        {
            city.Resource.Crop.Rate = Formula.Current.GetCropRate(city);
            city.Resource.Iron.Rate = Formula.Current.GetIronRate(city);
            city.Resource.Wood.Rate = Formula.Current.GetWoodRate(city);
        }

        public virtual void OnStructureUpgradeDowngrade(Structure structure)
        {
            SetResourceCap(structure.City);
            RecalculateCityResourceRates(structure.City);
        }

        public virtual void OnTechnologyChange(Structure structure)
        {
            structure.City.BeginUpdate();
            SetResourceCap(structure.City);
            structure.City.EndUpdate();
        }

        public virtual void OnSessionTribesmanQuit(Session session, uint tribeId, uint playerId, bool isKicked)
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