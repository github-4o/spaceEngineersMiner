
public abstract class FsmStateSub : FsmState {
    public abstract void init ();
    public abstract FsmState step ();
    public abstract FsmState getMe ();
    public abstract void setNext (FsmState n);
}
