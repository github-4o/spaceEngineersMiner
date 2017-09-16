
public class goAbovePos : FsmStateImplementation {

    Vector3D tpos;

    public goAbovePos (
        Grid g,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn
    ) : base (g, m, ivn, ovn) {
        if (ivn.Count < 2) {
            throw new Exception (
                "goAbovePos: false => ivn.Count = " + ivn.Count);
        }
    }

    public override void init () {
        base.init();
        Vector3D pc;
        if (grid.getPlanetCenter(out pc)) {
            throw new Exception ("failed to get planet center");
        }


        // double altitude;
        // if (grid.getAltitude(out altitude)) {
        //     throw new Exception (
        //         "goAbovePos: not designed to work in space");
        // }
        // Vector3D gravity;
        // if (grid.getGravity(out gravity)) {
        //     throw new Exception ("goAbovePos: failed to get gravity");
        // }
        Vector3D startPos = Vector3D.Zero;
        object o = mem.getVar (inVarNames[0]);
        if (o is Vector3D) {
            startPos = (Vector3D)o;
        } else {
            if (!(o is string)) {
                throw new Exception (
                    "goAbovePos: arg is not a string");
            }
            if (!Vector3D.TryParse ((string)o, out startPos)
            ) {
                throw new Exception (
                    "goAbovePos: failed to parse arg: '"
                    + (string)o + "'");
            }
        }
        double talt = Convert.ToDouble(mem.getVar (inVarNames[1]));

        startPos = (startPos - pc);
        double len = startPos.Length();
        startPos.Normalize();
        tpos = pc + startPos * (len + talt);
        // double altDiff = altitude - talt;
        // gravity.Normalize();
        // tpos = startPos + gravity * altDiff;
        // tpos = startPos + gravity * altDiff;
        // getPlanetCenter
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
