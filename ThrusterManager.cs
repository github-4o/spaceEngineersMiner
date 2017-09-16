
public class ThrusterManager {
    List<Thruster> atmThs = new List<Thruster>();
    List<Thruster> hyThs = new List<Thruster>();
    List<Thruster> ionThs = new List<Thruster>();
    List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();
    MyGridProgram pg;
    Dictionary <Base6Directions.Direction, double> forceMapDict
        = new Dictionary <Base6Directions.Direction, double> ();

    public ThrusterManager(MyGridProgram argPg){
        pg = argPg;
        _initThusters();
    }

    void _initThusters () {
        atmThs.Clear();
        hyThs.Clear();
        ionThs.Clear();
        pg.GridTerminalSystem.GetBlocksOfType <IMyThrust> (
            l, x=> x.CubeGrid == pg.Me.CubeGrid);
        MatrixD m = pg.Me.CubeGrid.WorldMatrix;
        Thruster t;
        for(int i=0;i<l.Count;i++){
            t = new Thruster((IMyThrust)l[i], m, pg);
            if (t.get_type() == Thruster.ATM){
                atmThs.Add(t);
            }else if (t.get_type() == Thruster.HY){
                hyThs.Add(t);
            }else if (t.get_type() == Thruster.ION){
                ionThs.Add(t);
            }else{
            }
        }
        forceMapDict.Clear();
        _addOrDictList (atmThs);
        _addOrDictList (ionThs);
        _addOrDictList (hyThs);
    }

    public Vector3D GetThrustAlongVector (Vector3D v) {
        Vector3D maxThrust = Vector3D.Zero;
        foreach (var kvp in forceMapDict) {
            Vector3D dir = _getDir(kvp.Key);
            if (v.Dot(dir) > 0) {
                maxThrust += dir*kvp.Value;
            }
        }
        return v * (maxThrust.Dot(v) / maxThrust.Length());
    }

    Vector3D _getDir (Base6Directions.Direction dir) {
        switch (dir) {
            case Base6Directions.Direction.Forward:
                return pg.Me.CubeGrid.WorldMatrix.Forward;
            case Base6Directions.Direction.Backward:
                return pg.Me.CubeGrid.WorldMatrix.Backward;
            case Base6Directions.Direction.Left:
                return pg.Me.CubeGrid.WorldMatrix.Left;
            case Base6Directions.Direction.Right:
                return pg.Me.CubeGrid.WorldMatrix.Right;
            case Base6Directions.Direction.Up:
                return pg.Me.CubeGrid.WorldMatrix.Up;
            case Base6Directions.Direction.Down:
                return pg.Me.CubeGrid.WorldMatrix.Down;
        }
        throw new Exception ("this should never happen");
    }

    public Dictionary <Base6Directions.Direction, double> getOrDict () {
        forceMapDict.Clear();
        _addOrDictList (atmThs);
        _addOrDictList (ionThs);
        _addOrDictList (hyThs);
        return forceMapDict;
    }

    public Dictionary <Base6Directions.Direction, double> getOrDictAtmo () {
        forceMapDict.Clear();
        _addOrDictList (atmThs);
        return forceMapDict;
    }

    void _addOrDictList (List<Thruster> l) {
        for (int i=0;i<l.Count;i++) {
            _addOrDictThruster (l[i]);
        }
    }

    void _addOrDictThruster (Thruster th) {
        Base6Directions.Direction key = th.getOrientation();
        double val = th.getForce();
        if (forceMapDict.ContainsKey(key)) {
            forceMapDict[key] += val;
        } else {
            forceMapDict.Add(key, val);
        }
    }

    public string printStatus () {
        string ret;
        ret =
            "atmo thrusters: " + atmThs.Count + "\n" +
            "hy thrusters: " + hyThs.Count + "\n" +
            "ion thrusters: " + ionThs.Count + "\n"
            ;

        return ret;
    }

    public void reinit () {
        _initThusters();
    }

    public void stop(){
        for(int i=0;i<atmThs.Count;i++){
            atmThs[i].stop();
        }
        for(int i=0;i<hyThs.Count;i++){
            hyThs[i].stop();
        }
        for(int i=0;i<ionThs.Count;i++){
            ionThs[i].stop();
        }
    }
    public void stop (List<Thruster> tl) {
        for(int i=0;i<tl.Count;i++){
            tl[i].stop();
        }
    }
    public void apply(Vector3D v){
        v = apply (v, atmThs);
        apply (v, hyThs);
    }

    public Vector3D apply (Vector3D v, List<Thruster> l){
        for(int j=0;j<l.Count;j++){
            if (l[j].checkStatus()){
                l.RemoveAt (j);
            } else {
                v = l[j].apply(v);
            }
        }
        return v;
    }

    public bool getStatus () {
        return false;
    }
}
