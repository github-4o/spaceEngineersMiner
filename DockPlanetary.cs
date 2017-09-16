
public class DockPlanetary : FsmStateImplementation {
    Vector3D tpos;
    List<IMyShipConnector> connectors = new List<IMyShipConnector> ();
    public DockPlanetary (
        Grid g,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn
    ) : base (g, m, ivn, ovn) {
        if (ivn.Count < 1) {
            throw new Exception ("DockPlanetary: ivn.Count = " + ivn.Count);
        }
        grid.gts.GetBlocksOfType (
                connectors, x => x.CubeGrid == grid.Me.CubeGrid);
    }

    public override void init () {
        base.init();

        Vector3D gravity;
        if (grid.getGravity(out gravity)) {
            throw new Exception ("Descend: failed to get gravity");
        }
        Vector3D startPos = Vector3D.Zero;

        object o = mem.getVar (inVarNames[0]);
        if (o is Vector3D) {
            startPos = (Vector3D)o;
        } else {
            if (!(o is string)) {
                throw new Exception (
                    "Descend: arg is not a string");
            }
            if (!Vector3D.TryParse ((string)o, out startPos)
            ) {
                throw new Exception (
                    "Descend: failed to parse arg: '"
                    + (string)o + "'");
            }
        }

        gravity.Normalize();
        tpos = startPos - gravity * (2.5/2 + (double)grid.Me.CubeGrid.GridSize/2);
        grid.setRef (connectors[0], Base6Directions.Direction.Forward);
        grid.setTpos (tpos);
        grid.setAlignMode (Aligner.Mode.Reference);
    }

    public override FsmState step () {
        if (active) {
            grid.gts.GetBlocksOfType (
                connectors, x => x.CubeGrid == grid.Me.CubeGrid);
            if (connectors.Count == 1) {
                connectors[0].ApplyAction ("Lock");
                if (connectors[0].Status == MyShipConnectorStatus.Connected) {
                    grid.stop();
                    return end();
                }
            }
            // if (grid.getDistanceToTposSquared < 2) {
            //     return end();
            // }
        } else {
            throw new Exception ("Descend: step() on inactive state");
        }
        return this;
    }
}
