Grid s;
Profiler p;

string msg_old = "";
string msg_new;

public Program() {
    s = new Grid (
        this,
        "Timer Block miner",
        "LCD Panel [status]",
        100
    );
    Echo ("grid ok");
    Me.CubeGrid.CustomName = "Miner";
    p = new Profiler (this);
}

// double maxinstc = 0;

void Main (string argument) {
    s.step(argument);

    // if (argument.Length > 0) {
    //     Echo ("last arg = " + argument);
    // }


    // double instc = ((double)Runtime.CurrentInstructionCount) / ((double)Runtime.MaxInstructionCount);
    // if (instc > maxinstc) {
    //     maxinstc = instc;
    // }

    // msg_new = s.printStatus() + "instc: " + maxinstc;
    msg_new = s.printStatus();
    if (msg_old != msg_new) {
        // Me.Echo (msg_new);
        Me.CustomData = msg_new + "\n" +
            "Me.EntityId.ToString() = " + Me.EntityId.ToString("X") + "\n" +
            "Me.EntityId = " + Me.EntityId + "\n" +
            "Me = " + Me + "\n"
        ;
        msg_old = msg_new;
    }

    // Me.CustomData = p.step();
}

public class Profiler {
    double[] perfAr = new double [60];
    int perfCnt;
    double avarage;
    double perfMaxSecond;
    double perfMaxEver;

    MyGridProgram prog;

    public Profiler (MyGridProgram p) {
        prog = p;
        perfCnt = 0;
        perfMaxSecond = 0;
        perfMaxEver = 0;
    }

    public string step () {
        perfAr [perfCnt] = prog.Runtime.LastRunTimeMs;
        perfCnt ++;
        if (perfCnt > 59) {
            perfCnt = 0;
        }
        avarage = (double) ((double)perfAr.Sum())/60.0;
        perfMaxSecond = perfAr.Max();
        if (perfMaxEver < perfMaxSecond) {
            perfMaxEver = perfMaxSecond;
        }
        return _msg ();
    }

    string _msg () {
        return "avarage = " + avarage + "\nmax during sec = " + perfMaxSecond
            + "\nmax ever" + perfMaxEver;
    }
}

// int count = 1;
// int maxSeconds = 100;
// StringBuilder profile = new StringBuilder();
// bool hasWritten = false;

// void ProfilerGraph() {
//     if (count <= maxSeconds * 60)
//     {
//         double timeToRunCode = Runtime.LastRunTimeMs;
//         profile.Append(timeToRunCode).Append("\n");
//         count++;
//     }
//     else if (!hasWritten)
//     {
//         hasWritten = true;
//         Me.CustomData = profile.ToString();
//     } else {
//         Echo ("done");
//     }
// }
public class Grid {
    Timer timer;
    Mover mover;
    Aligner aligner;
    MyGridProgram pg;
    Radio _radio;
    Goto gt;
    ProtoStack protoStack = new ProtoStack ();
    Radio radio {
        get {
            if (_radio == null) {
                _radio = new Radio (pg);
            }
            return _radio;
        }
    }
    // TelemetryGen tm;
    int reinitCount;
    int tickCounter = 0;
    string statusTpName;
    IMyTextPanel statusTp;
    List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();
    // IApp app = null;
    int index = 0;
    RemoteControl rc;

    // List<string> msgs = new List <string> ();

    public IMyGridTerminalSystem gts {get {return pg.GridTerminalSystem;}}
    public IMyProgrammableBlock Me {get {return pg.Me;}}

    FsmInterpreter prog = null;

    public bool align {
        get {return aligner.enabled;}
        set {aligner.enabled = value;}
    }

    public bool aligned {
        get {return aligner.isAligned();}
    }

    public double getDistanceToTposSquared {
        get {return gt.distanceToTposSquared;}
    }

    public Grid (
        MyGridProgram argPg,
        string timerName,
        string argStatusTpName,
        int argReinitCount
    ) {
        reinitCount = argReinitCount;
        statusTpName = argStatusTpName;
        pg = argPg;
        _initStatusTp();
        timer = new Timer (argPg, timerName);
        mover = new Mover (argPg);
        aligner = new Aligner (argPg);
        aligner.mode = Aligner.Mode.MostThrusters;
        // tm = new TelemetryGen (argPg);
        gt = new Goto (argPg, this, mover, aligner);
        gt.setRef (argPg.Me, Base6Directions.Direction.Down);
        rc = new RemoteControl (argPg);
        prog = new FsmInterpreter (this, null);

        string progString =
    // @"# preset variables
    // set altSource=altSource
    // set altCh=getAlt
    // fsm:
    // (alt)RequestValueOverRadio(altSource,altCh)
    // Undock
    // (alt)RequestValueOverRadio(altSource,altCh)
    // Dock
    // Recharge";

    // "# preset variables\n" +
    // "set altSource=altSource\n" +
    // "set altCh=getAlt\n" +
    // "fsm:\n" +
    // "(alt)RequestValueOverRadio(altSource,altCh)\n" +
    // "Undock\n" +
    // "(alt)RequestValueOverRadio(altSource,altCh)\n" +
    // "Dock\n" +
    // "Recharge";

    // "(dock)RequestValueOverRadio(dockSource,dockCh)\n" +
    "# preset variables\n" +
    "set altSource=altSource\n" +
    "set altCh=altCh\n" +
    "set dockSource=dockSource\n" +
    "set dockCh=dockCh\n" +
    "fsm:\n" +
    "(alt)RequestValueOverRadio(altSource,altCh)\n" +
    "DockBasic(dockSource,dockCh,alt)\n" +
    "Recharge";
    //  +
    // "Dock\n";

    //         prog.load (@"# preset variables
    // set altSource=altSource
    // set altCh=getAlt
    // set dockSource=dockSource
    // set dockCh=requestDock
    // # define fsm states
    // fsm:
    // (alt)RequestValueOverRadio(altSource,altCh)
    // Undock
    // DockBasic(sender,dockCh,alt)"
    // );
            // prog.load ("# preset variables\nset altSource=altSource\nset altCh=getAlt\nfsm:\n(alt)RequestValueOverRadio(altSource,altCh)\nUndock\n(alt)RequestValueOverRadio(altSource,altCh)\nDock\nRecharge");

        prog.load (progString);
    }

    // rc
    public bool getAltitude (out double alt) {
        return rc.getAltitude (out alt);
    }
    public bool getGravity (out Vector3D gravity) {
        return rc.getGravity (out gravity);
    }
    public bool getPlanetCenter (out Vector3D pc) {
        return rc.getPlanetCenter (out pc);
    }
    //
    public void setAlignMode (Aligner.Mode m) {
        aligner.mode = m;
    }
    //proto stack
    public bool registerProto (Proto p) {
        return protoStack.registerProto (p);
    }

    public void unregisterProto (Proto p) {
        protoStack.unregisterProto (p);
    }
    //

    public void Echo (string s) {
        pg.Echo (s);
    }

    public bool setRef (IMyTerminalBlock b, Base6Directions.Direction d) {
        return gt.setRef (b, d);
    }

    public void setTpos (Vector3D p) {
        gt.setTpos (p);
    }

    public void stop () {
        gt.stop ();
    }

    // public Mover requestMover () {
    //     return mover;
    // }

    // public Aligner requestAligner () {
    //     return aligner;
    // }

    // public void setApp (IApp argApp) {
    //     app = argApp;
    // }

    public void send (string s) {
        radio.send (s);
    }

    // public bool getMsg (out string msg) {
    //     if (msgs.Count > 0) {
    //         msg = msgs[0];
    //         msgs.RemoveAt (0);
    //         return false;
    //     }
    //     msg = "";
    //     return true;
    // }

    public bool step (string msg) {
        // if (msg.Length > 0 && !msgs.Contains (msg)) {
        //     msgs.Add (msg);
        // }
        if (msg.Length > 0) {
            // Echo ("handling msg: " + msg);
            protoStack.handleMsg (msg);
        }
        timer.startNextTick();
        tickCounter ++;
        if (tickCounter > reinitCount) {
            tickCounter = 0;
            _reinit();
        }
        index ++;
        if (index > 4) {
            index = 0;
        }
        switch (index) {
            case 0:
                // if (app != null) {
                //     app.step(msg);
                // }
                prog.step();
                prog.load (prog.backup);
                break;
            case 1:
                mover.step();
                break;
            case 2:
                aligner.step();
                break;
            case 3:
                gt.step();
                break;
            case 4:
                radio.step();
                // radio.send (pg.Me.GetPosition().ToString());
                // radio.send (MyConverter.toString(pg.Me.GetPosition()));
                // radio.send (tm.genTelementry());

                break;
        }
        return _getStatus();
    }

    public string printStatus () {
        string ret = "Grid status:\n"  +
            "stdout = " + _checkStatusTp() + "\n"
            + timer.printStatus()
            + mover.printStatus()
            + aligner.printStatus()
            + prog.backup + "\nstate = " + prog.reportState + "\n"
            // + app.printStatus()
            + gt.printStatus()
            + radio.printStatus();
        if (_checkStatusTp() == false) {
            statusTp.WritePublicText (ret);
        } else {
            _initStatusTp();
        }
        return ret;
    }

    void _initStatusTp () {
        List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();
        pg.GridTerminalSystem.GetBlocksOfType <IMyTextPanel> (
            l, x=> x.CubeGrid == pg.Me.CubeGrid &&
            x.CustomName == statusTpName);

        if (l.Count == 1) {
            statusTp = (IMyTextPanel) l[0];
        }
    }

    bool _checkStatusTp () {
        if (statusTp == null) {
            return true;
        }
        if ((pg.Me.CubeGrid.GetCubeBlock(statusTp.Position))?.FatBlock != statusTp) {
            statusTp = null;
            return true;
        }
        return false;
    }

    void _reinit () {
        mover.reinit();
    }

    bool _getStatus () {
        if (timer.getStatus()) {
            return true;
        }
        if (mover.getStatus()) {
            return true;
        }
        return false;
    }
}

public class Timer {
    string name;
    IMyTerminalBlock timer;
    MyGridProgram pg;

    public Timer (MyGridProgram argPg, string arg_name) {
        name = arg_name;
        pg = argPg;
        _findTimer ();
    }

    public bool startNextTick () {
        if (timer == null) {
            _findTimer();
            return false;
        } else {
            timer.ApplyAction ("TriggerNow");
            return true;
        }
    }

    public string printStatus () {
        if (_checkTimer()) {
            return "timer: failure\n";
        }
        return "";
    }

    public bool getStatus () {
        return _checkTimer();
    }

    void _findTimer () {
        List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();
        pg.GridTerminalSystem.GetBlocksOfType <IMyTimerBlock> (
            l, x=> x.CubeGrid == pg.Me.CubeGrid &&
            x.CustomName == name);

        if (l.Count == 1) {
            timer = (IMyTimerBlock) l[0];
        }
    }

    bool _checkTimer () {
        if (timer == null) {
            return true;
        }
        if ((timer.CubeGrid.GetCubeBlock(timer.Position))?.FatBlock != timer) {
            timer = null;
            return true;
        }
        return false;
    }
}

public class Radio {
    MyGridProgram pg;
    List<IMyRadioAntenna> l = new List<IMyRadioAntenna>();
    List<string> toSend = new List<string> ();

    int DEBUG_sentNum = 0;
    string DEBUG_last_msg = "";

    public Radio (MyGridProgram argPg) {
        pg = argPg;
    }

    public void send (string msg) {
        // msg = pg.Me.CubeGrid.CustomName + "%" + msg;
        // msg = pg.Me.EntityId + "%" + msg;
        if (!toSend.Contains (msg)) {
            toSend.Add (msg);
        }
    }

    public void step () {
        if (!_init() && toSend.Count > 0) {
            DEBUG_sentNum ++;
            DEBUG_last_msg = toSend[0];
            l[0].TransmitMessage (toSend[0]);
            toSend.RemoveAt (0);
        }
    }

    public string printStatus () {
        return "radio: sent num = " + DEBUG_sentNum + "\nlast msg = " + DEBUG_last_msg + "\n";
    }

    bool _init () {
        if (l.Count == 0) {
            pg.GridTerminalSystem.GetBlocksOfType (l, x=>x.CubeGrid == pg.Me.CubeGrid);
            return l.Count == 0;
        }
        return false;
    }
}

public class Mover {
    ThrusterManager tm;
    MyGridProgram pg;
    IMyRemoteControl rc;
    Vector3D targetVelocity;
    Vector3D zero = new Vector3D (0,0,0);
    bool working = false;
    List<IMyShipController> ctrls = new List<IMyShipController>();
    List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();

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
        Vector3D tv = Vector3D.TransformNormal(
            v-velocity-gravity,
            MatrixD.Transpose(pg.Me.CubeGrid.WorldMatrix)
        );
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
            mover.stop();
            return;
        }
        Vector3D togo = tpos - reference.GetPosition ();
        distValid = true;
        dist = togo;
        if (togo.LengthSquared() > 100) {
            togo = Vector3D.Normalize (togo) * 10;
        } else {
        //     togo = Vector3D.Normalize (togo) * 20;
            togo /= 5;
        }
        mover.setTargetVelocity (togo);
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
            "dist = " + dist + "\n";
    }
}

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
        Base6Directions.Direction key = th.getOrientation().Forward;
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

public class Thruster {

    IMyThrust th;
    Vector3D FORCE;
    public const int ATM = 1;
    public const int HY = 2;
    public const int ION = 3;
    int TYPE;
    int FORCE_DIM;
    int FORCE_MULT;
    MyGridProgram pg;

    public double getForce () {
        return th.MaxThrust;
    }

    public Thruster (IMyThrust t, MatrixD m, MyGridProgram argPg){
        pg = argPg;
        th = t;
        double force = 0;
        if (t.DetailedInfo.Contains("Atmospheric")){
            TYPE = ATM;
        } else if (t.DetailedInfo.Contains("Hydrogen")){
            TYPE = HY;
        } else if (t.DetailedInfo.Contains("Thruster")){
            TYPE = ION;
        } else {
        }
        force = t.MaxEffectiveThrust;

        FORCE = new Vector3D(0,0,0);
        if (t.WorldMatrix.Forward == m.Forward){
            FORCE_DIM = 2;
            FORCE_MULT = 1;
        }else if (t.WorldMatrix.Forward == m.Backward){
            FORCE_DIM = 2;
            FORCE_MULT = -1;
        }else if (t.WorldMatrix.Forward == m.Right){
            FORCE_DIM = 0;
            FORCE_MULT = -1;
        }else if (t.WorldMatrix.Forward == m.Left){
            FORCE_DIM = 0;
            FORCE_MULT = 1;
        }else if (t.WorldMatrix.Forward == m.Up){
            FORCE_DIM = 1;
            FORCE_MULT = -1;
        }else if (t.WorldMatrix.Forward == m.Down){
            FORCE_DIM = 1;
            FORCE_MULT = 1;
        }else{
            throw new Exception ("Thruster(): invalid matrix");
        }
        FORCE.SetDim(FORCE_DIM,FORCE_MULT);
    }

    public MyBlockOrientation getOrientation () {
        return th.Orientation;
    }

    public int get_type(){
        if (TYPE == 0){
            throw new Exception("Thruster.get_type(): invalid type");
        }
        return TYPE;
    }
    public void stop(){
        th.ApplyAction("OnOff_On");
        _set (0);
    }
    public Vector3D apply(Vector3D v){
        if (checkStatus()) {
            return v;
        }
        if (v.GetDim(FORCE_DIM) == 0){
            _set (0);
            return v;
        }
        th.ApplyAction("OnOff_On");
        Vector3D force = FORCE;
        force.Normalize();
        double effectiveForce = th.MaxEffectiveThrust;
        force = force * effectiveForce;
        double vforce = v.GetDim(FORCE_DIM);
        double tforce = force.GetDim(FORCE_DIM);
        double to_apply = 0;
        if ((vforce < 0) == (tforce < 0)){
            if (vforce < 0){
                if (vforce < tforce){
                    vforce -= tforce;
                    to_apply = tforce;
                }else{
                    to_apply = vforce;
                    vforce = 0;
                }
            }else{
                if (vforce > tforce){
                    vforce -= tforce;
                    to_apply = tforce;
                }else{
                    to_apply = vforce;
                    vforce = 0;
                }
            }
        }else{
            _set (0);
            return v;
        }
        to_apply = (to_apply / tforce)*100;
        v.SetDim(FORCE_DIM, vforce);
        _set (to_apply);
        return v;
    }

    void _set (double val) {
        th.SetValueFloat("Override", (float) val);
    }

    public bool checkStatus() {
        if (th == null) {
            return true;
        }
        if ((th.CubeGrid.GetCubeBlock(th.Position))?.FatBlock != th) {
            th = null;
            return true;
        }
        return false;
    }
}
public class Gyroscope {
    protected List<gyro> GYROS = new List<gyro>();
    public bool enabled = false;
    protected void gyros_set(string dir, double val){
        enabled=false;
        for (int i=0;i<GYROS.Count;i++){
            GYROS[i].set(dir, val);
        }
    }
    public void stopAligning(){
        if (enabled==false){
            _stopAll();
            enabled=true;
        }
    }
    public void startAligning(){
        // if (enabled==false){
        //     _stopAll();
            enabled=false;
        // }
    }
    public void _stopAll () {
        for (int i=0;i<GYROS.Count;i++){
            GYROS[i].stop();
            GYROS[i].SetOverride(false);
        }
    }
    public class gyro{
        public IMyGyro G {get; protected set;}
        string YAW_NAME;
        int YAW_MULT;
        string PITCH_NAME;
        int PITCH_MULT;
        string ROLL_NAME;
        int ROLL_MULT;

        public void SetOverride(bool val) {
            G.GyroOverride = val;
        }
        public bool getOverride () {
            return G.GyroOverride;
        }
        public bool checkMine (IMyGyro gg) {
            if (gg == G) {
                return true;
            }
            return false;
        }
        bool vectors_eq (Vector3D a, Vector3D b) {
            return (a - b).Length() < 0.01;
        }

        Base6Directions.Direction _inv (Base6Directions.Direction d) {
           return d ^ Base6Directions.Direction.Backward;
        }

        public void set_matrix (IMyTerminalBlock b) {
            MatrixD gm = G.WorldMatrix;
            MyBlockOrientation gor = G.Orientation;
            MyBlockOrientation ror = b.Orientation;
            MatrixD r = b.WorldMatrix;

            if (gor.Up == ror.Up) {
                YAW_NAME = "Yaw";
                YAW_MULT = -1;
            }else if (_inv(gor.Up) == ror.Up) {
                YAW_NAME = "Yaw";
                YAW_MULT = 1;
            }else if (gor.Forward == ror.Up) {
                YAW_NAME = "Roll";
                YAW_MULT = 1;
            }else if (_inv(gor.Forward) == ror.Up) {
                YAW_NAME = "Roll";
                YAW_MULT = -1;
            }else if (_inv(gor.Left) == ror.Up) {
                YAW_NAME = "Pitch";
                YAW_MULT = 1;
            }else if (gor.Left == ror.Up) {
                YAW_NAME = "Pitch";
                YAW_MULT = -1;
            } else {
                throw new Exception ("invalid 1" + gm.Up + "\n" + ror.Up);
            }

            if (gor.Up == _inv(ror.Left)) {
                PITCH_NAME = "Yaw";
                PITCH_MULT = -1;
            }else if (_inv(gor.Up) == _inv(ror.Left)) {
                PITCH_NAME = "Yaw";
                PITCH_MULT = 1;
            }else if (gor.Forward == _inv(ror.Left)) {
                PITCH_NAME = "Roll";
                PITCH_MULT = 1;
            }else if (_inv(gor.Forward) == _inv(ror.Left)) {
                PITCH_NAME = "Roll";
                PITCH_MULT = -1;
            }else if (_inv(gor.Left) == _inv(ror.Left)) {
                PITCH_NAME = "Pitch";
                PITCH_MULT = 1;
            }else if (gor.Left == _inv(ror.Left)) {
                PITCH_NAME = "Pitch";
                PITCH_MULT = -1;
            } else {
                throw new Exception  ("invalid 2");
            }

            if (gor.Up == ror.Forward) {
                ROLL_NAME = "Yaw";
                ROLL_MULT = -1;
            }else if (_inv(gor.Up) == ror.Forward) {
                ROLL_NAME = "Yaw";
                ROLL_MULT = 1;
            }else if (gor.Forward == ror.Forward) {
                ROLL_NAME = "Roll";
                ROLL_MULT = 1;
            }else if (_inv(gor.Forward) == ror.Forward) {
                ROLL_NAME = "Roll";
                ROLL_MULT = -1;
            }else if (_inv(gor.Left) == ror.Forward) {
                ROLL_NAME = "Pitch";
                ROLL_MULT = 1;
            }else if (gor.Left == ror.Forward) {
                ROLL_NAME = "Pitch";
                ROLL_MULT = -1;
            } else {
                throw new Exception  ("invalid 3");
            }
        }

        public gyro (
            IMyGyro g,
            IMyTerminalBlock b
        ){
            if (g == null || _checkExists (b)){
                throw new Exception("gyro(): null args");
            }
            G=g;
            set_matrix (b);
        }
        public void stop() {
            G.SetValueFloat("Pitch", (float)0);
            G.SetValueFloat("Yaw", (float)0);
            G.SetValueFloat("Roll", (float)0);
        }

        public void set (string name, double val){
            if (val == double.NaN) {
                val = 0;
            }
            G.GyroOverride = true;
            if (name == "Pitch"){
                G.SetValueFloat(PITCH_NAME, (float)val*PITCH_MULT);
            }else if (name == "Yaw"){
                G.SetValueFloat(YAW_NAME, (float)val*YAW_MULT);
            }else if (name == "Roll"){
                G.SetValueFloat(ROLL_NAME, (float)val*ROLL_MULT);
            }else{
                throw new Exception("gyro.set(): invalid name :'" + name + "'");
            }
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
    }
}

public class Aligner : Gyroscope {
    bool IS_ALIGNED;
    public bool isAligned(){return IS_ALIGNED;}
    public enum Mode {Reference, MostThrusters};
    public Mode mode = Mode.MostThrusters;
    // IMyRemoteControl rc;
    RemoteControl rc;

    Vector3D GRAVITY1_MASK;
    Vector3D GRAVITY2_MASK;
    Vector3D UNIFORM_MASK;
    Vector3D DIR_ALIGN;
    Vector3D DIR_SIGN1;
    Vector3D DIR_SIGN2;
    string NAME1;
    string NAME2;
    string NAME3;

    Vector3D zero = new Vector3D (0,0,0);

    bool ALIGN_UNIFORM;

    double THRESHOLD;

    IMyTerminalBlock REFERENCE = null;
    Base6Directions.Direction dir;

    MyGridProgram pg;

    int reinitCnt = 101;
    const int reinitCntCap = 100;

    List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();

    public Aligner(
        MyGridProgram argPg, double threshold = 0.9999
    ){
        pg = argPg;
        rc = new RemoteControl (argPg);
        THRESHOLD = threshold;
        ALIGN_UNIFORM = false;
    }
    public bool set_reference (
        IMyTerminalBlock b,
        Base6Directions.Direction d
    ) {
        if (_checkExists(b)) {
            return true;
        }
        REFERENCE = b;
        for (int i=0;i<GYROS.Count;i++){
            GYROS [i].set_matrix (REFERENCE);
        }
        dir = d;
        make_settings(dir);
        return false;
    }
    public void reset(){
        stopAligning();
        ALIGN_UNIFORM=false;
    }
    public void set_align_uniform(){
        ALIGN_UNIFORM = true;
    }
    public void set_align_uniform(bool val){
        ALIGN_UNIFORM = val;
    }
    public void set_threshold(double t){
        THRESHOLD = t;
    }
    bool make_settings(Base6Directions.Direction dir){
        switch (dir) {
            case Base6Directions.Direction.Down: // "Down"
                GRAVITY1_MASK = new Vector3D (1,1,0);
                GRAVITY2_MASK = new Vector3D (0,1,1);
                UNIFORM_MASK = new Vector3D (1,0,1);
                DIR_ALIGN = new Vector3D(0,-1,0);
                DIR_SIGN1 = new Vector3D(1,0,0);
                DIR_SIGN2 = new Vector3D(0,0,1);
                NAME1 = "Roll";
                NAME2 = "Pitch";
                NAME3 = "Yaw";
                break;
            case Base6Directions.Direction.Up: // "Up"
                GRAVITY1_MASK = new Vector3D (1,1,0);
                GRAVITY2_MASK = new Vector3D (0,1,1);
                UNIFORM_MASK = new Vector3D (1,0,1);
                DIR_ALIGN = new Vector3D(0,1,0);
                DIR_SIGN1 = new Vector3D(-1,0,0);
                DIR_SIGN2 = new Vector3D(0,0,-1);
                NAME1 = "Roll";
                NAME2 = "Pitch";
                NAME3 = "Yaw";
                break;
            case Base6Directions.Direction.Right: // "Right"
                GRAVITY1_MASK = new Vector3D (1,0,1);
                GRAVITY2_MASK = new Vector3D (1,1,0);
                UNIFORM_MASK = new Vector3D (0,1,1);
                DIR_ALIGN = new Vector3D(1,0,0);
                DIR_SIGN1 = new Vector3D(0,0,1);
                DIR_SIGN2 = new Vector3D(0,1,0);
                NAME1 = "Yaw";
                NAME2 = "Roll";
                NAME3 = "Pitch";
                break;
            case Base6Directions.Direction.Left: // "Left"
                GRAVITY1_MASK = new Vector3D (1,0,1);
                GRAVITY2_MASK = new Vector3D (1,1,0);
                UNIFORM_MASK = new Vector3D (0,1,1);
                DIR_ALIGN = new Vector3D(-1,0,0);
                DIR_SIGN1 = new Vector3D(0,0,-1);
                DIR_SIGN2 = new Vector3D(0,-1,0);
                NAME1 = "Yaw";
                NAME2 = "Roll";
                NAME3 = "Pitch";
                break;
            case Base6Directions.Direction.Forward: // "Forward"
                GRAVITY1_MASK = new Vector3D (0,1,1);
                GRAVITY2_MASK = new Vector3D (1,0,1);
                UNIFORM_MASK = new Vector3D (1,1,0);
                DIR_ALIGN = new Vector3D(0,0,-1);
                DIR_SIGN1 = new Vector3D(0,-1,0);
                DIR_SIGN2 = new Vector3D(1,0,0);
                NAME1 = "Pitch";
                NAME2 = "Yaw";
                NAME3 = "Roll";
                break;
            case Base6Directions.Direction.Backward: // "Backward"
                GRAVITY1_MASK = new Vector3D (0,1,1);
                GRAVITY2_MASK = new Vector3D (1,0,1);
                UNIFORM_MASK = new Vector3D (1,1,0);
                DIR_ALIGN = new Vector3D(0,0,1);
                DIR_SIGN1 = new Vector3D(0,1,0);
                DIR_SIGN2 = new Vector3D(-1,0,0);
                NAME1 = "Pitch";
                NAME2 = "Yaw";
                NAME3 = "Roll";
                break;
            default:
                throw new Exception("make_settings(): this should never happen");
        }
        return false;
    }

    Vector3D _getRefVector () {
        if (!_checkExists(REFERENCE)) {
            switch (dir) {
                case Base6Directions.Direction.Forward:
                    return REFERENCE.WorldMatrix.Forward;
                case Base6Directions.Direction.Backward:
                    return REFERENCE.WorldMatrix.Backward;
                case Base6Directions.Direction.Left:
                    return REFERENCE.WorldMatrix.Left;
                case Base6Directions.Direction.Right:
                    return REFERENCE.WorldMatrix.Right;
                case Base6Directions.Direction.Down:
                    return REFERENCE.WorldMatrix.Down;
                case Base6Directions.Direction.Up:
                    return REFERENCE.WorldMatrix.Up;
            }
        }
        return Vector3D.Zero;
    }

    public bool step(double mult = 1){
        reinitCnt ++;
        if (reinitCnt > reinitCntCap) {
            reinitCnt = 0;
            if (_init()) {
                return false;
            }
        }
        if (!enabled) {
            stopAligning();
            return false;
        }
        if (_checkExists(REFERENCE)) {
            stopAligning();
            return false;
        }
        if (rc.check()){
            stopAligning();
            return false;
        }

        Vector3D grav;
        rc.getGravityVector(out grav);

        Vector3D tst;
        rc.getGravity(out tst);
        if (tst.LengthSquared() == 0) {
            stopAligning();
            return false;
        }

        if ((_getRefVector() - grav).Length() < 0.0001) {
            stopAligning();
            return false;
        }

        grav = Vector3D.TransformNormal(
            grav, MatrixD.Transpose(REFERENCE.WorldMatrix)
        );
        Vector3D gravity1 = new Vector3D (
            grav.GetDim(0)*GRAVITY1_MASK.GetDim(0),
            grav.GetDim(1)*GRAVITY1_MASK.GetDim(1),
            grav.GetDim(2)*GRAVITY1_MASK.GetDim(2)
        );
        Vector3D gravity2 = new Vector3D (
            grav.GetDim(0)*GRAVITY2_MASK.GetDim(0),
            grav.GetDim(1)*GRAVITY2_MASK.GetDim(1),
            grav.GetDim(2)*GRAVITY2_MASK.GetDim(2)
        );

        gravity1.Normalize();
        gravity2.Normalize();

        double to_apply;
        bool is_aligned_tmp;
        bool ret;
        bool is_almost_aligned;
        to_apply = get_gyro_val (
            gravity1,
            DIR_ALIGN,
            DIR_SIGN1,
            out IS_ALIGNED,
            out ret
        ) * mult;

        gyros_set(NAME1, to_apply);
        to_apply = get_gyro_val (
            gravity2,
            DIR_ALIGN,
            DIR_SIGN2,
            out is_aligned_tmp,
            out is_almost_aligned
        ) * mult;
        IS_ALIGNED &= is_aligned_tmp;
        ret &= is_almost_aligned;
        gyros_set(NAME2, to_apply);
        // Vector3D pos_mod = rc.GetShipVelocities().LinearVelocity;
        // if (ALIGN_UNIFORM && pos_mod.Length() > 1) {
        // if (ALIGN_UNIFORM) {
        //     pos_mod = Vector3D.Normalize(pos_mod);
        //     pos_mod = Vector3D.TransformNormal(
        //         pos_mod, MatrixD.Transpose(REFERENCE.WorldMatrix)
        //     );
        //     pos_mod = Vector3D.Normalize(new Vector3D (
        //         pos_mod.GetDim(0)*UNIFORM_MASK.GetDim(0),
        //         pos_mod.GetDim(1)*UNIFORM_MASK.GetDim(1),
        //         pos_mod.GetDim(2)*UNIFORM_MASK.GetDim(2)
        //     ));
        //     to_apply = 10*get_gyro_val (
        //         pos_mod,
        //         DIR_SIGN2,
        //         DIR_SIGN1,
        //         out is_aligned_tmp,
        //         out is_almost_aligned
        //     ) * mult;
        //     ret &= is_almost_aligned;
        //     IS_ALIGNED &= is_aligned_tmp;
        // }else{
            to_apply = 0;
        // }
        gyros_set(NAME3, to_apply);
        return ret;
    }
    double get_gyro_val(
        Vector3D ref_v,
        Vector3D align_dir,
        Vector3D dir_sign,
        out bool aligned,
        out bool almost_aligned
    ){
        double c;
        double sign;
        double to_apply;
        double cap = 0.4;
        c = Vector3D.Dot(ref_v, align_dir);
        if (c < 0) {
            c = - c;
        }
        if (c > THRESHOLD) {
            aligned = true;
        }else{
            aligned = false;
        }
        if (c > THRESHOLD/2) {
            almost_aligned = true;
        }else{
            almost_aligned = false;
        }
        if (c > 0.9999) {
            return 0;
        }
        if (c > 0.85){
            cap = 0.12;
        }else if (c > 0.7){
            cap = 0.1;
        }else  if (c > 0.5){
            cap = 0.2;
        }
        sign = Vector3D.Dot(ref_v, dir_sign);
        if (sign < 0){
            sign = 1;
        }else {
            sign = -1;
        }
        to_apply = (1-c)*10;
        if (to_apply > cap){
            to_apply = cap;
        }
        return to_apply * sign;
    }
    // generic
    public string printStatus () {
        string ret = "";
        if (_checkGyros()) {
            ret += "gyros: failure\n";
        }
         else {
            ret += "gyros: " + GYROS.Count + "\n";
        }
        if (rc.check()) {
            ret += "rc: failure\n";
        }
        return ret;
    }
    public bool getStatus () {
        bool ret;
        ret = rc.check();
        ret |= _checkGyros();
        return ret;
    }
    bool _init() {
        bool ret;
        ret = rc.check();
        ret |= _initGyros();
        return ret;
    }
    // rc
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
    // gyros
    bool _checkGyros () {
        return GYROS.Count == 0;
    }
    bool _initGyros () {
        l.Clear();
        pg.GridTerminalSystem.GetBlocksOfType <IMyGyro> (
            l, x=> x.CubeGrid == pg.Me.CubeGrid);
        IMyGyro gg;
        for (int i=0;i<l.Count;i++){
            gg = (IMyGyro)l[i];
            if (!_alreadyThere (gg)) {
                GYROS.Add(new gyro((IMyGyro)(l[i]), pg.Me));
            }
        }
        return _checkGyros ();
    }

    bool _alreadyThere (IMyGyro gg) {
        for (int i=0;i<GYROS.Count;i++) {
            if (GYROS[i].checkMine (gg)) {
                return true;
            }
        }
        return false;
    }
}

public class RemoteControl {
    MyGridProgram pg;
    List<IMyTerminalBlock> l = new List<IMyTerminalBlock> ();
    List<IMyRemoteControl> rms = new List<IMyRemoteControl> ();
    IMyRemoteControl rc;

    public RemoteControl (MyGridProgram argPg) {
        pg = argPg;
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
                pg.GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(
                    l, x => x.CubeGrid == pg.Me.CubeGrid);
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

public interface FsmState {
    void init ();
    FsmState step ();
    FsmState getMe ();
    void setNext (FsmState n);
}

public abstract class FsmStateSub : FsmState {
    public abstract void init ();
    public abstract FsmState step ();
    public abstract FsmState getMe ();
    public abstract void setNext (FsmState n);
}

public abstract class FsmStateImplementation : FsmState {

    bool DEBUG = false;

    protected Grid grid = null;
    public FsmState next {get; private set;} = null;

    protected FsmMemorySpace mem;
    protected List<string> inVarNames = new List<string> ();
    protected List<string> outVarNames = new List<string> ();

    public bool active {get; private set;} = false;

    public FsmState getMe () {
        return this;
    }

    public void setNext (FsmState n) {
        next = n;
    }

    public abstract FsmState step ();

    public virtual void init () {
        active = true;
    }

    public void backup (StringBuilder sb) {
        _backupVars(outVarNames, sb);
        sb.Append (this.GetType().Name);
        _backupVars(inVarNames, sb);
    }

    void _backupVars (List<string> l, StringBuilder sb) {
        if (l == null) {
            // sb.Append("()");
            return;
        }
        sb.Append ("(");
        for (int i=0;i<l.Count;i++) {
            sb.Append (l[i]);
            if (i < l.Count-1) {
                sb.Append (",");
            }
        }
        sb.Append (")");
    }

    public FsmStateImplementation (
        Grid argG,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn
    ) {
        grid = argG;
        mem = m;
        inVarNames = ivn;
        outVarNames = ovn;

    }

    protected FsmState end () {
        if (DEBUG) {
            grid.Echo ("FsmStateImplementation: state " + this.ToString() + " done");
        }
        active = false;
        if (next != null) {
            next.init();
        }
        return next;
    }
}

public class FsmMemorySpace {
    Dictionary <string, Slot> mem = new Dictionary <string, Slot> ();

    public void setVar (string name, object o) {
        if (mem.ContainsKey (name)) {
            mem[name].setVal (o);
        } else {
            mem.Add (name, new Slot (o));
        }
    }

    public void clear () {
        mem.Clear();
    }

    public bool isConst (string name) {
        if (mem.ContainsKey (name)) {
            return mem[name].isConst;
        }
        return false;
    }

    public void setVarConst (string name, object o) {
        if (mem.ContainsKey (name)) {
            if (mem[name].isConst) {
                throw new Exception (
                    "Slot: prevented an attempt to set const var " + name);
            }
            mem[name].setVal (o);
            mem[name].isConst = true;
        } else {
            mem.Add (name, new Slot (o, true));
        }
    }

    public object getVar (string name) {
        if (mem.ContainsKey (name)) {
            return mem[name].val;
        }
        return null;
    }

    public string print () {
        StringBuilder ret = new StringBuilder ();
        foreach (var kvp in mem) {
            ret.Append (kvp.Key + " = " + kvp.Value.print() + "\n");
        }
        return ret.ToString();
    }

    public void backup (StringBuilder sb) {
        foreach (var kvp in mem) {
            if (kvp.Value.isConst) {
                sb.Append ("setConst ");
            } else {
                sb.Append ("set ");
            }
            sb.Append (kvp.Key + "=" + kvp.Value.val+"\n");
        }
    }

    protected class Slot {
        public object val {get; protected set;}
        public bool isConst;
        public Slot (object v, bool c = false) {
            val = v;
            isConst = c;
        }
        public object getVal () {
            return val;
        }
        public void setVal (object v) {
            val = v;
        }
        public string print () {
            return val.ToString();
        }
    }
}

public class FsmInterpreter {
    Grid grid = null;
    FsmState state = null;
    FsmState stateReg = null;
    List<FsmState> stateList = new List<FsmState> ();
    FsmMemorySpace mem = new FsmMemorySpace ();
    public string backup {get; protected set;} = "";
    Action triggerBackup;

    public string reportState {get {
        return state.GetType().Name;
    }}

    public FsmInterpreter (Grid g, Action trig) {
        grid = g;
        triggerBackup = trig;
    }

    public void load (string s) {
        backup = s;
        string[] lines = s.Split (new [] {'\r', '\n'});
        _loadMem (lines);
        _loadFsm (lines);

        for (int i=0;i<stateList.Count-1;i++) {
            stateList[i].setNext (stateList[i+1].getMe());
        }
        stateList[stateList.Count-1].setNext(stateList[0].getMe());
    }

    void _backup () {
        StringBuilder sb = new StringBuilder ();
        mem.backup(sb);
        _backupFsm(sb);
        backup = sb.ToString();
    }

    void _backupFsm (StringBuilder sb) {
        sb.Append ("fsm:\n");
        var st = state as FsmStateImplementation;
        _backupFsm (st, sb);
        st = st.next as FsmStateImplementation;
        while (st != state) {
            _backupFsm (st, sb);
            st = st.next as FsmStateImplementation;
        }
    }

    void _backupFsm (FsmStateImplementation st, StringBuilder sb) {
        if (st == null) {
            throw new Exception ("null state");
        }
        st.backup(sb);
        sb.Append ("\n");
    }

    void _loadMem (string[] ss) {
        mem.clear();
        for (int i=0;i<ss.Length;i++) {
            if (ss[i].StartsWith ("set ")) {
                _loadMemOneLine (ss[i].Substring (4), false);
            } else if (ss[i].StartsWith ("setConst ")) {
                _loadMemOneLine (ss[i].Substring (9), true);
            }
        }
    }

    void _loadFsm (string[] ss) {
        stateList.Clear();
        bool startFound = false;
        int index = 0;
        for (;index<ss.Length;index++) {
            if (ss[index].StartsWith("fsm:")) {
                startFound = true;
                break;
            }
        }
        if (!startFound) {
            throw new Exception ("failed to find 'fsm:' pattern");
        }
        List <string> usefulLines = new List<string> ();
        for (int i = index+1;i<ss.Length;i++) {
            if (ss[i].Length > 0) {
                usefulLines.Add (ss[i]);
            }
        }
        if (usefulLines.Count < 1) {
            throw new Exception ("failed to find any useful fsm lines");
        }
        for (int i=0;i<usefulLines.Count;i++) {
            _loadFsmOneLine (usefulLines[i]);
        }
    }

    void _loadFsmOneLine (string str) {
        string[] ar = str.Split (new [] {'(', ')'});
        if (ar.Length == 5) {
            List<string> inArgs = new List<string> ();
            if (ar[3].Length > 0) {
                inArgs = ar[3].Split(',').ToList();
            }
            List<string> outArgs = new List<string> ();
            if (ar[1].Length > 0) {
                outArgs = ar[1].Split(',').ToList();
            }
            string stateName = ar[2];
            FsmState nst = FsmStateFactory.makeState (
                stateName, grid, mem, inArgs, outArgs);
            if (state == null) {
                state = nst;
                nst.init();
            }
            if (nst == null) {
                throw new Exception ("(1)failed to create a state from line '"
                    + str + "'");
            }
            stateList.Add (nst);
        // } else if (ar.Length == 3) {
        } else if (ar.Length == 3 && str.EndsWith(")")) {
            List<string> inArgs = new List<string> ();
            if (ar[1].Length > 0) {
                inArgs = ar[1].Split(',').ToList();
            }
            string stateName = ar[0];
            FsmState nst = FsmStateFactory.makeState (
                stateName, grid, mem, inArgs, null);
            if (state == null) {
                state = nst;
                nst.init();
            }
            if (nst == null) {
                throw new Exception ("(2)failed to create a state from line '"
                    + str + "'");
            }
            stateList.Add (nst);
        } else if (ar.Length == 3 && str.StartsWith("(")) {
            List<string> outArgs = new List<string> ();
            if (ar[1].Length > 0) {
                outArgs = ar[1].Split(',').ToList();
            }
            string stateName = ar[2];
            FsmState nst = FsmStateFactory.makeState (
                stateName, grid, mem, null, outArgs);
            if (state == null) {
                state = nst;
                nst.init();
            }
            if (nst == null) {
                throw new Exception ("(3)failed to create a state from line '"
                    + str + "'");
            }
            stateList.Add (nst);
        } else if (ar.Length == 1) {
            string stateName = ar[0];
            FsmState nst = FsmStateFactory.makeState (
                stateName, grid, mem, null, null);
            if (state == null) {
                state = nst;
                nst.init();
            }
            if (nst == null) {
                throw new Exception ("(4)failed to create a state from line '"
                    + str + "'");
            }
            stateList.Add (nst);
        } else {
            throw new Exception ("words num = " + ar.Length + " str = " + str
                + " last char = " + str[str.Length-1]);
        }
    }

    void _loadMemOneLine (string s, bool c) {
        string[] words = s.Split ('=');
        if (words.Length != 2) {
            throw new Exception ("failed to load mem (" + words.Length + ")");
        }

        string name = words [0];
        string val = words [1];

        // grid.Echo ("setting var " + name + " = " + val);

        if (c) {
            mem.setVarConst (name, val);
        } else {
            mem.setVar (name, val);
        }

        words = null;
    }

    public bool step () {
        state = state?.step();
        if (stateReg != null && stateReg != state) {
            _backup();
            if (triggerBackup != null) {
                triggerBackup();
            }
        }
        stateReg = state;
        return state != null;
    }

    public string printMem () {
        return mem.print();
    }
}

public class Undock : FsmStateImplementation {

    List<IMyShipConnector> connectors = new List <IMyShipConnector> ();

    public Undock (
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
            connectors[i].ApplyAction("Unlock");
        }
    }

    public override FsmState step () {
        if (active) {
            return end();
        } else {
            throw new Exception ("Unlock: step() on inactive state");
        }
    }
}

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

public class RequestValueOverRadio : FsmStateImplementation {

    RadioMsg _val = null;

    public RadioMsg val {
        get {
            if (_val == null) {
                throw new Exception ("RequestValueOverRadio: someone requested an uninitialized .val");
            }
            return _val;
        }
    }

    int cntCap = 180;
    enum State {requestVal, catchVal}
    State state = State.requestVal;
    int cnt = 0;

    ProtoRcv proto = null;

    public RequestValueOverRadio (
        Grid g,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn,
        int retryCnt = 60
    ) : base (g, m, ivn, ovn) {
        if (ivn.Count < 2) {
            throw new Exception (
                "RequestValueOverRadio: invalid input var names list");
        }
        if (ovn.Count < 1) {
            throw new Exception (
                "RequestValueOverRadio: invalid output var names list");
        }

        proto = new ProtoRcv (grid);

        cntCap = retryCnt;
    }

    public override void init () {
        base.init ();
        cnt = 0;
        state = State.requestVal;
        object gridname = mem.getVar (inVarNames[0]);
        object chname = mem.getVar (inVarNames[1]);
        if (gridname == null) {
            throw new Exception ("gridname null: " + inVarNames[0]);
        }
        if (chname == null) {
            throw new Exception ("chname null");
        }
        if (
            !(gridname is string) ||
            !(chname is string)
        ) {
            throw new Exception ("RequestValueOverRadio: invalid var types"
                + gridname.GetType() + ":" + chname.GetType());
        }
        proto.init ((string) gridname, (string) chname);
        proto.enable();
    }

    // bool getReply = false;

    public override FsmState step () {
        if (active) {
            switch (state) {
                case State.requestVal:
                    cnt = 0;
                    proto.requestVal();
                    state = State.catchVal;
                    break;
                case State.catchVal:
                    // if (!getReply) { //
                    //     grid.Echo ("cnt = " + cnt);
                    // } else {
                    //     grid.Echo ("gotReply");
                    // }
                    if (proto.getVal(out _val)) {
                        cnt ++;
                        if (cnt > cntCap) {
                            state = State.requestVal;
                        }
                    } else {
                        mem.setVar (outVarNames[0], _val.val);
                        if (outVarNames.Count > 1) {
                            mem.setVar (outVarNames[1], _val.sender);
                        }
                        state = State.requestVal;
                        proto.disable ();
                        // getReply = true;
                        return end();
                    }
                    break;
            }
        } else {
            throw new Exception (
                "RequestValueOverRadio: step() on inactive state");
        }
        return this;
    }
}

public class MoveBasic : FsmStateSub {

    FsmState goUp = null;
    FsmState goAbovePos = null;

    public MoveBasic (
        Grid g,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn
    ) {
        goUp = new goAboveMyPos (g, m, ivn, null);
        goAbovePos = new goAbovePos (g, m, ivn, null);

        goUp.setNext (goAbovePos.getMe());
    }

    public override FsmState getMe () {
        return goUp.getMe();
    }

    public override void setNext (FsmState n) {
        goAbovePos.setNext (n);
    }

    public override void init () {
        throw new Exception ("MoveBasic: this should never happen (0)");
    }

    public override FsmState step () {
        throw new Exception ("MoveBasic: this should never happen (1)");
    }
}

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

public class ProtoStack {
    List <Proto> protos = new List <Proto> ();

    public bool registerProto (Proto p) {
        for (int i=0;i<protos.Count;i++) {
            if (protos[i].host == p.host &&
                protos[i].channel == p.channel
            ) {
                return true;
            }
        }
        protos.Add (p);
        return false;
    }

    public void unregisterProto (Proto p) {
        for (int i=0;i<protos.Count;i++) {
            if (protos[i].host == p.host &&
                protos[i].channel == p.channel
            ) {
                protos.RemoveAt (i);
            }
        }
    }

    public void handleMsg (string rawMsg) {
        if (rawMsg == "") {
            return;
        }
        for (int i=0;i<protos.Count;i++) {
            if (protos[i].handleMsg (rawMsg)) {
                return;
            }
        }
    }
}

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

public abstract class Proto {

    protected Grid grid;
    public string host {get; protected set;} = "";
    public string channel {get; protected set;} = "";

    public Proto (Grid g) {
        grid = g;
    }

    public abstract bool handleMsg (string msg);

    public void enable () {
        grid.registerProto (this);
    }
    public void disable () {
        grid.unregisterProto (this);
    }
    public void init (string h, string ch) {
        host = h;
        channel = ch;
    }
}

public class ProtoRcv : Proto {

    // const bool DEBUG = false;

    // Grid grid;
    List<RadioMsg> msgList = new List<RadioMsg>();

    public ProtoRcv (Grid g) : base (g) {
        // grid = g;
    }
    // public void enable () {
    //     grid.registerProto (this);
    // }
    // public void disable () {
    //     grid.unregisterProto (this);
    // }
    public override bool handleMsg (string msg) {

        // if (DEBUG) {
        //     grid.Echo ("ProtoRcv: handling msg '" + msg + "'");
        // }

        if (host == "") {
            return handleMsgUnknownHost (msg);
        } else {
            return handleMsgKnownHost (msg);
        }

    }

    public bool handleMsgUnknownHost (string msg) {

        string v0 = "%" + grid.Me.EntityId.ToString() + "%" + channel + "%";
        string v1 = "%" + grid.Me.CubeGrid.CustomName + "%" + channel + "%";
        string v;

        int index = msg.IndexOf ("%");

        if (msg.IndexOf (v0) == index) {
            v = v0;
        } else if (msg.IndexOf (v1) == index) {
            v = v1;
        } else {
            return false;
        }

        msgList.Add (
            new RadioMsg (msg.Substring (0, index), msg.Substring (
                index + v.Length))
        );

        return false;
    }

    public bool handleMsgKnownHost (string msg) {

        // if (DEBUG) {
        //     grid.Echo ("ProtoRcv: checking against host: " + host);
        // }

        string v0 = host + "%" + grid.Me.EntityId.ToString("X") + "%" + channel + "%";
        string v1 = host + "%" + grid.Me.CubeGrid.CustomName + "%" + channel + "%";

        string v = null;

        // if (DEBUG) {
        //     grid.Echo ("ProtoRcv: check 0: " + v0);
        // }
        // if (DEBUG) {
        //     grid.Echo ("ProtoRcv: check 1: " + v1);
        // }

        if (msg.StartsWith (v0)) {
            v = v0;
        } else if (msg.StartsWith (v1)) {
            v = v1;
        }
        if (v == null) {
            // if (DEBUG) {
            //     grid.Echo ("ProtoRcv: check failed");
            // }
            return false;
        }

        msgList.Add (
            new RadioMsg (host, msg.Substring (v.Length))
        );
        return true;

    }

    public void requestVal () {
        grid.send (grid.Me.EntityId.ToString("X") + "%" + host + "%" + channel
            + "%" + "give!");
    }

    public bool getVal (out RadioMsg msg) {
        msg = null;
        if (msgList.Count > 0) {
            msg = msgList[0];
            msgList.RemoveAt (0);
            return false;
        }
        return true;
    }
}

public class RadioMsg {
    public string sender {get; protected set;}
    public string val {get; protected set;}

    public RadioMsg (string s, string v) {
        sender = s;
        val = v;
    }
}

public class Recharge : FsmStateImplementation {
    public Recharge (
        Grid g,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn
    ) : base (g, m, ivn, ovn) {
    }

    List<IMyBatteryBlock> batts = new List<IMyBatteryBlock> ();

    public override void init () {
        base.init();

        grid.gts.GetBlocksOfType (batts, x => x.CubeGrid == grid.Me.CubeGrid);

        for (int i=0;i<batts.Count;i++) {
            batts[i].OnlyRecharge = true;
        }
    }

    public override FsmState step () {
        double minCharge = 100;
        double tmp = 0;
        for (int i=0;i<batts.Count;i++) {
            tmp = batts[i].CurrentStoredPower / batts[i].MaxStoredPower;
            if (tmp < minCharge) {
                minCharge = tmp;
            }
        }
        if (minCharge > 0.95) {
            for (int i=0;i<batts.Count;i++) {
                batts[i].OnlyRecharge = false;
            }
            return end();
        }
        return this;
    }
}
