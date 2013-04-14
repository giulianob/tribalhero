using Game.Data;
using Game.Data.Tribe;
using Game.Setup;

namespace Game.Logic.Procedures
{
    public partial class Procedure
    {
        public virtual Error CreateTribe(IPlayer player, string name, out ITribe tribe)
        {
            tribe = null;

            if (player.Tribesman != null)
            {
                return Error.TribesmanAlreadyInTribe;
            }

            if (tribeManager.TribeNameTaken(name))
            {
                return Error.TribeAlreadyExists;
            }

            if (!Tribe.IsNameValid(name))
            {
                return Error.TribeNameInvalid;
            }

            tribe = tribeFactory.CreateTribe(player, name);

            tribe.CreateRank(0, "Chief", TribePermission.All);
            tribe.CreateRank(1, "Elder", TribePermission.Invite | TribePermission.Kick | TribePermission.Repair | TribePermission.AssignmentCreate);
            tribe.CreateRank(2, "Protector", TribePermission.Repair | TribePermission.AssignmentCreate);
            tribe.CreateRank(3, "Aggressor", TribePermission.AssignmentCreate);
            tribe.CreateRank(4, "Tribesmen", TribePermission.None);

            tribeManager.Add(tribe);

            var tribesman = new Tribesman(tribe, player, tribe.ChiefRank);
            tribe.AddTribesman(tribesman);
            return Error.Ok;
        }
    }
}
