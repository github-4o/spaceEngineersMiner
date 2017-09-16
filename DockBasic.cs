
public class DockBasic : FsmStateSub {

    FsmState requestDock = null;
    FsmState undock = null;
    FsmState moveAboveConnector = null;
    FsmState dock = null;

    public DockBasic (
        Grid g,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn
    ) {
        List<string> dockPos = new List<string> () {"dockPos"};
        List<string> moveArgs = new List<string> () {"dockPos", "alt"};

        requestDock = new RequestValueOverRadio (g, m, ivn, dockPos);
        undock = new Undock (g, m, null, null);
        moveAboveConnector = new MoveBasic (g, m, moveArgs, null);
        dock = new DockPlanetary (g, m, dockPos, null);

        requestDock.setNext (undock.getMe());
        undock.setNext (moveAboveConnector.getMe());
        moveAboveConnector.setNext (dock.getMe());
    }

    public override FsmState getMe () {
        return requestDock.getMe();
    }

    public override void setNext (FsmState n) {
        dock.setNext (n);
    }

    public override void init () {
        throw new Exception ("DockBasic: this should never happen (0)");
    }

    public override FsmState step () {
        throw new Exception ("DockBasic: this should never happen (1)");
    }
}
