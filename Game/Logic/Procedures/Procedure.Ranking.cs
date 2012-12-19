#region

using System.Threading;
using Game.Data;
using Game.Data.Tribe;
using Game.Logic.Formulas;
using Game.Util.Locking;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        /// <summary>
        ///     Gives the appropriate attack points to the specified city. Must call begin/endupdate on city.
        /// </summary>
        /// <param name="city"></param>
        /// <param name="attackPoints"></param>
        /// <param name="initialTroopValue"></param>
        /// <param name="endTroopValue"></param>
        public virtual void GiveAttackPoints(ICity city, int attackPoints, int initialTroopValue, int endTroopValue)
        {
            var point = Formula.Current.GetAttackPoint(attackPoints, initialTroopValue - endTroopValue);
            city.AttackPoint += point;
            if (city.Owner.Tribesman == null)
            {
                return;
            }
            var id = city.Owner.Tribesman.Tribe.Id;
            ThreadPool.QueueUserWorkItem(delegate
                {
                    ITribe tribe;
                    using (Concurrency.Current.Lock(id, out tribe))
                    {
                        if (tribe == null)
                        {
                            return;
                        }

                        tribe.AttackPoint += point;
                    }
                });
        }
    }
}