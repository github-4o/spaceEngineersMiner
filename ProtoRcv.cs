
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
