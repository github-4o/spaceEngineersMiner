
public class Aligner : Gyroscope {
    bool IS_ALIGNED;
    public bool isAligned(){return IS_ALIGNED;}
    public enum Mode {Reference, MostThrusters};
    public Mode mode = Mode.MostThrusters;
    public Vector3D tVector = new Vector3D ();
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

    Grid grid;

    int reinitCnt = 101;
    const int reinitCntCap = 100;

    List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();

    public Aligner(
        Grid g, double threshold = 0.9999
    ){
        grid = g;
        rc = new RemoteControl (g);
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
        // grav = tVector;
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
        grid.gts.GetBlocksOfType <IMyGyro> (
            l, x=> x.CubeGrid == grid.Me.CubeGrid);
        IMyGyro gg;
        for (int i=0;i<l.Count;i++){
            gg = (IMyGyro)l[i];
            if (!_alreadyThere (gg)) {
                GYROS.Add(new gyro((IMyGyro)(l[i]), grid.Me));
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
