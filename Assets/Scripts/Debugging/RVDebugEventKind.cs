namespace ReturnVector.Debugging
{
    /// <summary>
    /// Categories used by the combat event history.
    /// </summary>
    public enum RVDebugEventKind
    {
        StateTransition = 0,
        OutboundImpact = 1,
        RecallImpact = 2,
        SurfaceResponse = 3,
        OutboundParked = 4,
        RecallStarted = 5,
        RecallBlocked = 6,
        CatchStarted = 7,
        CatchCompleted = 8
    }
}
