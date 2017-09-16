
public class goAboveMyPos : FsmStateImplementation {

    Vector3D tpos;

    public goAboveMyPos (
        Grid g,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn
    ) : base (g, m, ivn, ovn) {
        if (ivn.Count < 1) {
            throw new Exception (
                "goAbovePos: true => ivn.Count = " + ivn.Count);
        }
    }

    public override void init () {
        base.init();
        double altitude;
        if (grid.getAltitude(out altitude)) {
            throw new Exception (
                "goAbovePos: not designed to work in space");
        }
        Vector3D gravity;
        if (grid.getGravity(out gravity)) {
            throw new Exception ("goAbovePos: failed to get gravity");
        }
        Vector3D startPos = Vector3D.Zero;
        startPos = grid.Me.GetPosition ();
        double talt = Convert.ToDouble(mem.getVar (inVarNames[1]));
        double altDiff = altitude - talt;
        gravity.Normalize();
        tpos = startPos + gravity * altDiff;
        grid.setTpos (tpos);
        grid.setAlignMode (Aligner.Mode.MostThrusters);
    }

    public override FsmState step () {
        if (active) {
            if (grid.getDistanceToTposSquared < 2) {
                return end();
            }
        } else {
            throw new Exception ("goAbovePos: step() on inactive state");
        }
        return this;
    }
}
