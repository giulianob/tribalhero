#region

using Game.Data;
using Game.Logic.Formulas;

#endregion

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        /// <summary>
        ///   Gives the appropriate attack points to the specified city. Must call begin/endupdate on city.
        /// </summary>
        /// <param name = "city"></param>
        /// <param name = "attackPoints"></param>
        /// <param name = "initialTroopValue"></param>
        /// <param name = "endTroopValue"></param>
        public static void GiveAttackPoints(City city, int attackPoints, int initialTroopValue, int endTroopValue)
        {
            city.AttackPoint += Formula.GetAttackPoint(attackPoints, initialTroopValue - endTroopValue);
        }
    }
}