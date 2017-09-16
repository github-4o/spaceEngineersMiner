
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
