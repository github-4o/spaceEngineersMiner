
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
                        mem.setVar (outVarNames[0], _val.val);
                        if (outVarNames.Count > 1) {
                            mem.setVar (outVarNames[1], _val.sender);
                        }
                        state = State.requestVal;
                        proto.disable ();
                        // getReply = true;
                        return end();
                    } else {
                        cnt ++;
                        if (cnt > cntCap) {
                            state = State.requestVal;
                        }
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
