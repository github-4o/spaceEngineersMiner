
public static class FsmStateFactory {
    public static FsmState makeState (
        string name,
        Grid g,
        FsmMemorySpace mem,
        List<string> ia,
        List<string> oa
    ) {
        switch (name) {
            // substates
            case "MoveBasic":
                return new MoveBasic (g, mem, ia, oa);
            case "DockBasic":
                return new DockBasic (g, mem, ia, oa);
            // implementations
            case "RequestValueOverRadio":
                return new RequestValueOverRadio (g, mem, ia, oa);
            case "Undock":
                return new Undock (g, mem, ia, oa);
            case "Dock":
                return new Dock (g, mem, ia, oa);
            case "goAboveMyPos":
                return new goAboveMyPos (g, mem, ia, oa);
            case "goAbovePos":
                return new goAbovePos (g, mem, ia, oa);
            case "DockPlanetary":
                return new DockPlanetary (g, mem, ia, oa);
            case "Recharge":
                return new Recharge (g, mem, ia, oa);
            default:
                return null;
        }
    }
}
