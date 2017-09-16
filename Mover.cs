
public class Mover {
    ThrusterManager tm;
    MyGridProgram pg;
    IMyRemoteControl rc;
    Vector3D targetVelocity;
    Vector3D zero = new Vector3D (0,0,0);
    bool working = false;
    List<IMyShipController> ctrls = new List<IMyShipController>();
    List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();
    public Vector3D refVelocity;

    public Mover (MyGridProgram argPg) {
        pg = argPg;
        tm = new ThrusterManager (argPg);
    }
    public bool getVelocity (out Vector3D v) {
        v = _velocity();
        return false;
    }
    public void setTargetVelocity (Vector3D v) {
        targetVelocity = v;
        working = true;
    }
    public void step () {
        if (working) {
            if ((targetVelocity - _velocity()).Length() > 0.1) {
                tm.apply(_getApplyVal());
                return;
            }
        }
        tm.stop();
    }
    public void stop () {
        working = false;
    }

    Vector3D _getApplyVal () {
        if (_checkOrInitRc()) {
            return zero;
        }
        Vector3D v = targetVelocity;
        Vector3D gravity = _gravity();
        if (gravity.Length() == 0) {
            gravity = zero;
        }
        Vector3D velocity = _velocity();
        double mass = (double) rc.CalculateShipMass().BaseMass + (
            (rc.CalculateShipMass().TotalMass -
            // rc.CalculateShipMass().BaseMass) /10);
            rc.CalculateShipMass().BaseMass));

        // Vector3D vn = Vector3D.Normalize (v);
        // Vector3D velocityProjection = velocity * (velocity.Dot(vn) / velocity.Length());

        // if (tm.unobtainable (velocityProjection)) {
        //     vn = velocityProjection - velocity;
        // }

        // Vector3D correction = velocity+gravity;
        // double corrLen = 100-correction.Length();
        // double vLen = v.Length();
        // if (vLen > corrLen) {
        //     v *= (corrLen/vLen);
        // }
        Vector3D tv = Vector3D.TransformNormal(
            v-velocity-gravity,
            MatrixD.Transpose(pg.Me.CubeGrid.WorldMatrix)
        );
        refVelocity = v-velocity-gravity;
        return tv*mass;
    }

    public void reinit () {
        tm.reinit();
    }

    public bool getStatus() {
        return _checkRcExists() || tm.getStatus();
    }

    public string printStatus () {
        string ret = "";
        if (_checkOrInitRc()) {
            ret += "rc: failure";
        }
            ret += "\n" + tm.printStatus ();
        return ret;
    }

    public Dictionary <Base6Directions.Direction, double> getOrDict () {
        return tm.getOrDict();
    }

    public Dictionary <Base6Directions.Direction, double> getOrDictAtmo () {
        return tm.getOrDictAtmo();
    }

    Vector3D _gravity () {
        if (_initRc()) {
            return new Vector3D();
        } else {
            return rc.GetNaturalGravity();
        }
    }

    Vector3D _velocity () {
        if (_checkOrInitRc ()) {
            return new Vector3D();
        }
        return rc.GetShipVelocities().LinearVelocity;
    }

    bool _initRc () {
        if (_checkRcExists()) {
            pg.GridTerminalSystem.GetBlocksOfType <IMyRemoteControl> (
                l, x=> x.CubeGrid == pg.Me.CubeGrid);

            if (l.Count == 1) {
                rc = (IMyRemoteControl) l[0];
                return false;
            }
            return true;
        }
        return false;
    }

    bool _checkRcExists () {
        if (rc == null) {
            return true;
        }
        if ((rc.CubeGrid.GetCubeBlock(rc.Position))?.FatBlock != rc) {
            rc = null;
            return true;
        }
        return false;
    }

    bool _checkOrInitRc () {
        if (_checkRcExists()) {
            return _initRc();
        }
        return false;
    }
}
