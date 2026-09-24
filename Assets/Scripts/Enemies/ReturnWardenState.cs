namespace ReturnVector.Enemies
{
    /// <summary>
    /// High-level behaviour states and attack vocabulary for the Return Warden.
    /// </summary>
    public enum ReturnWardenState
    {
        Pursuit = 0,
        Windup = 1,
        Active = 2,
        Recovery = 3,
        Transforming = 4,
        Dead = 5
    }

    public enum ReturnWardenAttackKind
    {
        None = 0,
        Slam = 1,
        Charge = 2,
        Shockwave = 3
    }
}
