
public class Dock : FsmStateImplementation {

    List<IMyShipConnector> connectors = new List <IMyShipConnector> ();

    public Dock (
        Grid g,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn
    ) : base (g, m, ivn, ovn) {

    }

    public override void init () {
        base.init ();
        grid.gts.GetBlocksOfType (
            connectors, x => x.CubeGrid == grid.Me.CubeGrid);
        for (int i=0;i<connectors.Count;i++) {
            connectors[i].ApplyAction("Lock");
        }
    }

    public override FsmState step () {
        if (active) {
            return end();
        } else {
            throw new Exception ("step() on inactive state");
        }
    }
}
