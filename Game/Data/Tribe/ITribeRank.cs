using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Game.Data.Tribe
{

    [Flags]
    public enum TribePermission
    {
        None = 0x00,
        All = 0x01,
        Invite = 0x2,
        Kick = 0x4,
        SetRank = 0x8,
        Repair = 0x10,
        Upgrade = 0x20,
        AssignmentCreate = 0x40,
        DeletePost = 0x80,
        SetAnnouncement = 0x100,
    }

    public interface ITribeRank 
    {
        byte Id { get; set; }
        string Name { get; set; }
        TribePermission Permission { get; set; }
    }
}
