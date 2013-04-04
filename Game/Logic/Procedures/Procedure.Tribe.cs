using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Data;
using Game.Data.Tribe;
using Game.Map;
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

            tribe.CreateRank("Chief", TribePermission.All);
            tribe.CreateRank("Elder", TribePermission.Invite | TribePermission.Kick | TribePermission.Repair | TribePermission.AssignmentCreate);
            tribe.CreateRank("Protector", TribePermission.Repair | TribePermission.AssignmentCreate);
            tribe.CreateRank("Aggressor", TribePermission.AssignmentCreate);
            tribe.CreateRank("Tribesmen", TribePermission.None);

            tribeManager.Add(tribe);

            var tribesman = new Tribesman(tribe, player, tribe.ChiefRank);
            tribe.AddTribesman(tribesman);
            return Error.Ok;
        }
    }
}
