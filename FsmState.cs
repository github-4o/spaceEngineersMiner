
public interface FsmState {
    void init ();
    FsmState step ();
    FsmState getMe ();
    void setNext (FsmState n);
}
