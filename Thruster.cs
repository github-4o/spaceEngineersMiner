
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
