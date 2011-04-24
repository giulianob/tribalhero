namespace Game.Comm
{
    public enum CmdLineCommand
    {
        SendResources = 100,
        Ban = 110,
        Unban = 111,
        Delete = 112,

        TribeInfo = 201,
        TribeCreate = 202,
        TribeDelete = 203,
        TribeUpdate = 204,
        TribeUpgrade = 205,
        TribesmanAdd = 211,
        TribesmanRemove = 212,
        TribesmanUpdate = 213,
        TribeIncomingList = 214,

    }
}