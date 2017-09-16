
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
