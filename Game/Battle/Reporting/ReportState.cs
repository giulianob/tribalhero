namespace Game.Battle.Reporting
{
    /// <summary>
    ///     Different states available when a troop gets snapshotted in the battle report.
    /// </summary>
    public enum ReportState
    {
        Entering = 0,

        Staying = 1,

        Exiting = 2,

        Dying = 3,

        Retreating = 4,

        Reinforced = 5,

        OutOfStamina = 6
    }
}