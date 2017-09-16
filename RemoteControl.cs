
public class RemoteControl {
    Grid grid;
    List<IMyTerminalBlock> l = new List<IMyTerminalBlock> ();
    List<IMyRemoteControl> rms = new List<IMyRemoteControl> ();
    IMyRemoteControl rc;

    public RemoteControl (Grid g) {
        grid = g;
    }

    public bool check () {
        if (_checkRcExists()) {
            return _init ();
        }
        return false;
    }

    public bool getGravity (out Vector3D gravity) {
        gravity = Vector3D.Zero;
        if (_init ()) {
            return true;
        }
        gravity = rc.GetNaturalGravity();
        return false;
    }

    public bool getGravityVector (out Vector3D gravity) {
        gravity = Vector3D.Zero;
        if (_init ()) {
            return true;
        }
        rc.TryGetPlanetPosition(out gravity);
        gravity -= rc.GetPosition();
        gravity.Normalize();
        return false;
    }

    public bool getAltitude (out double alt) {
        alt = -1;
        if (_init ()) {
            return true;
        }
        rc.TryGetPlanetElevation(MyPlanetElevation.Surface, out alt);
        return false;
    }

    public bool getMass (out double mass) {
        mass = -1;
        if (_init ()) {
            return true;
        }
        mass = (double) rc.CalculateShipMass().BaseMass + (
            (
                rc.CalculateShipMass().TotalMass -
                rc.CalculateShipMass().BaseMass
            ) /10
        );
        return false;
    }

    public bool getPlanetCenter (out Vector3D pp) {
        pp = Vector3D.Zero;
        if (_init()) {
            return false;
        }
        return !rc.TryGetPlanetPosition (out pp);
    }

    bool _init () {
        if (_checkRcExists ()) {
            if (_initFromList ()) {
                grid.gts.GetBlocksOfType<IMyRemoteControl>(
                    l, x => x.CubeGrid == grid.Me.CubeGrid);
                rms = l.Cast<IMyRemoteControl>().ToList();
                return _initFromList();
            } else {
                return false;
            }
        }
        return false;
    }

    bool _initFromList() {
        for (int i=0;i<rms.Count;i++) {
            rc = rms [i];
            if (!_checkRcExists ()) {
                return false;
            }
        }
        return true;
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
}
