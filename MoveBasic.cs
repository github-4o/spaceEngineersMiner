
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
