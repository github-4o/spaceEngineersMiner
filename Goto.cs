
public class Goto {
    MyGridProgram pg;
    Grid g;
    Mover mover;
    Aligner aligner;

    bool refReady = false;
    IMyTerminalBlock reference = null;
    Base6Directions.Direction dir;

    bool tposReady = false;
    Vector3D tpos;

    bool distValid = false;
    Vector3D dist;

    RemoteControl rc;

    public double distanceToTposSquared {
        get {
            if (distValid) {
                Vector3D velocity;
                if (mover.getVelocity (out velocity)) {
                    return dist.LengthSquared();
                } else {
                    return (dist+velocity*100).LengthSquared();
                }

            } else {
                return -1;
            }
        }
    }

    public Goto (MyGridProgram argPg, Grid argG, Mover m, Aligner a) {
        pg = argPg;
        g = argG;
        mover = m;
        aligner = a;
        rc = new RemoteControl (g);
    }

    public bool setRef (IMyTerminalBlock b, Base6Directions.Direction d) {
        if (_checkExists (b)) {
            return true;
        }
        reference = b;
        dir = d;
        refReady = true;
        return false;
    }

    public void setTpos (Vector3D p) {
        tpos = p;
        tposReady = true;
    }

    public void stop () {
        tposReady = false;
        mover.stop();
    }

    public void step () {
        if (!refReady) {
            mover.stop ();
            return;
        }
        aligner.set_reference (reference, dir);
        aligner.enabled = true;
        if (!tposReady) {
            rc.getGravityVector (out aligner.tVector);
            mover.stop();
            return;
        }
        distValid = true;
        Vector3D togo = tpos - reference.GetPosition ();
        dist = togo;
        // if (old) {
            double len = togo.Length();
            // // if (len < breakingDistance*breakingDistance) {
            // //     togo.Normalize();
            // //     togo *=
            // // }
            if (len > 100) {
                togo /= (len/100);
            }
            //     togo = Vector3D.Normalize (togo) * 10;
            // } else if (len < 7) {
            //     // togo *= 2;
            // } else {
            // //     togo = Vector3D.Normalize (togo) * 20;
            //     // togo.Normalize();
            //     togo *= len / breakingDistance;
            // }
        // } else {
        //     Vector3D thrust = mover.getPotentialThrust;
        // }
        mover.setTargetVelocity (togo);
        aligner.tVector = -mover.refVelocity;
    }

    bool _checkExists (IMyTerminalBlock b) {
        if (b == null) {
            return true;
        }
        if ((b.CubeGrid.GetCubeBlock(b.Position))?.FatBlock != b) {
            b = null;
            return true;
        }
        return false;
    }
    public string printStatus () {
        return
            "refReady = " + refReady + "\n" +
            "reference = " + reference + "\n" +
            "dir = " + dir + "\n" +
            "tposReady = " + tposReady + "\n" +
            "tpos = " + tpos + "\n" +
            "distValid = " + distValid + "\n" +
            "dist = " + dist.Length() + "\n";
    }
}
