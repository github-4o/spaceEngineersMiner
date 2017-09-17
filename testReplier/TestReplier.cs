Grid g;
public Program() {
    g = new Grid (this);
}

void Main (string argument) {
    if (argument.Length > 0) {
        Echo ("arg = '" + argument + "'");
    }
    g.step(argument);
}

public class Grid {
    MyGridProgram pg;
    ProtoStack protoStack;
    Radio _radio;
    RadioHandler rh;
    Radio radio {
        get {
            if (_radio == null) {
                _radio = new Radio (this);
            }
            return _radio;
        }
    }
    // List<string> msgs = new List <string> ();
    public IMyGridTerminalSystem gts {get {return pg.GridTerminalSystem;}}
    public IMyProgrammableBlock Me {get {return pg.Me;}}

    public Grid (
        MyGridProgram argPg
    ) {
        pg = argPg;
        protoStack = new ProtoStack (this);
        rh = new RadioHandler (this);
    }
    public void Echo (string s) {
        pg.Echo (s);
    }
    public bool registerProto (Proto p) {
        return protoStack.registerProto (p);
    }
    public void unregisterProto (Proto p) {
        protoStack.unregisterProto (p);
    }
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
    public void step (string msg) {
        // if (msg.Length > 0 && !msgs.Contains (msg)) {
        //     msgs.Add (msg);
        // }
        if (msg.Length > 0) {
            protoStack.handleMsg (msg);
        }
        radio.step();
        rh.step();
    }
}

public class RadioHandler {
    Grid grid;
    ProtoUdp altProto;
    ProtoUdp dockProto;
    List<IMyShipConnector> cl = new List<IMyShipConnector> ();
    public RadioHandler (Grid g) {
        grid = g;
        altProto = new ProtoUdp (g);
        altProto.init ("", "altCh");
        altProto.logicAddress = "altSource";
        altProto.useLogicAddress = true;
        altProto.enable();

        dockProto = new ProtoUdp (g);
        dockProto.init ("", "dockCh");
        dockProto.logicAddress = "dockSource";
        dockProto.useLogicAddress = true;
        dockProto.enable();
    }

    public void step () {
        RadioMsg msg;
        if (!altProto.getMsg (out msg)) {
            grid.Echo ("sending reply: 200 to " + msg.sender);
            altProto.sendMsg (20.ToString(), msg.sender);
            msg = null;
        }

        if (!dockProto.getMsg (out msg)) {
            grid.Echo ("sending reply: dock to " + msg.sender);
            IMyShipConnector c = sendFreeConnector (msg.sender);
            if (c != null) {
                dockProto.sendMsg (c.GetPosition().ToString(), msg.sender);
            }
            msg = null;
        }
    }
    // bool _sendFreeConnector (string targetName, out Vector3D pos) {
    //     pos = Vector3D.Zero;
    //     grid.gts.GetBlocksOfType (cl, x=>x.CubeGrid == grid.Me.CubeGrid
    //         && x.CustomData == targetName
    //     );
    //     for (int i=0;i<cl.Count;i++) {
    //         if (cl[i].Status == MyShipConnectorStatus.Unconnected) {
    //             // radio.send (targetName + "%" + cl[i].GetPosition().ToString() + "%"
    //             //     + cl[i].WorldMatrix.Forward.ToString());
    //             pos = cl[i].GetPosition();
    //             return false;
    //         }
    //     }
    //     return true;
    // }

    // List<IMyShipConnector> cl = new List<IMyShipConnector> ();

    IMyShipConnector sendFreeConnector (string targetName) {
        grid.gts.GetBlocksOfType (cl, x=>x.CubeGrid == grid.Me.CubeGrid
            && x.CustomData == targetName
        );
        // cloc.Clear();
        for (int i=0;i<cl.Count;i++) {
            if (cl[i].Status == MyShipConnectorStatus.Unconnected) {
                // radio.send (targetName + "%" + cl[i].GetPosition().ToString() + "%"
                //     + cl[i].WorldMatrix.Forward.ToString());
                return cl[i];
            }
        }
        return null;
    }
}

public class Radio {
    Grid grid;
    List<IMyRadioAntenna> l = new List<IMyRadioAntenna>();
    List<string> toSend = new List<string> ();

    int DEBUG_sentNum = 0;
    string DEBUG_last_msg = "";

    public Radio (Grid g) {
        grid = g;
    }

    public void send (string msg) {
        // msg = pg.Me.CubeGrid.CustomName + "%" + msg;
        // msg = grid.Me.EntityId + "%" + msg;
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
            grid.gts.GetBlocksOfType (l, x=>x.CubeGrid == grid.Me.CubeGrid);
            return l.Count == 0;
        }
        return false;
    }
}

public abstract class Proto {

    protected Grid grid;
    public bool useLogicAddress = false;
    public string logicAddress = "";
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

public class ProtoStack {
    Grid grid;
    List <Proto> protos = new List <Proto> ();

    public ProtoStack (Grid g) {
        grid = g;
    }

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

public class ProtoUdp : Proto {

    const bool DEBUG = true;

    List<RadioMsg> msgList = new List<RadioMsg>();

    public ProtoUdp (Grid g) : base (g) {
    }
    public override bool handleMsg (string msg) {

        if (DEBUG) {
            grid.Echo ("handling msg '" + msg + "'");
        }

        if (host == "") {
            return handleMsgUnknownHost (msg);
        } else {
            return handleMsgKnownHost (msg);
        }

    }

    public bool handleMsgUnknownHost (string msg) {

        if (DEBUG) {
            grid.Echo ("handling msg from unknown host '" + msg + "'");
        }

        string v;
        int index;
        if (useLogicAddress) {
            index = msg.IndexOf ("%");
            v = "%" + logicAddress + "%" + channel + "%";

            if (msg.IndexOf (v) != index) {
                grid.Echo ("msg discarded: " + msg.IndexOf (v) + ":" + index);
                return false;
            }
        } else {
            string v0 = "%" + grid.Me.EntityId.ToString() + "%" + channel + "%";
            string v1 = "%" + grid.Me.CubeGrid.CustomName + "%" + channel + "%";

            index = msg.IndexOf ("%");

            if (msg.IndexOf (v0) == index) {
                v = v0;
            } else if (msg.IndexOf (v1) == index) {
                v = v1;
            } else {
                return false;
            }
        }

        msgList.Add (
            new RadioMsg (msg.Substring (0, index), msg.Substring (
                index + v.Length))
        );

        return false;
    }

    public bool handleMsgKnownHost (string msg) {

        if (DEBUG) {
            grid.Echo ("ProtoRcv: checking against host: " + host);
        }

        string v = null;
        if (useLogicAddress) {
            int index = msg.IndexOf ("%");
            v = host + "%" + logicAddress + "%" + channel + "%";

            if (msg.IndexOf (logicAddress) != index) {
                return false;
            }
        } else {

            string v0 = host + "%" + grid.Me.EntityId.ToString() + "%" + channel + "%";
            string v1 = host + "%" + grid.Me.CubeGrid.CustomName + "%" + channel + "%";


            if (DEBUG) {
                grid.Echo ("ProtoRcv: check 0: " + v0);
            }
            if (DEBUG) {
                grid.Echo ("ProtoRcv: check 1: " + v1);
            }

            if (msg.StartsWith (v0)) {
                v = v0;
            } else if (msg.StartsWith (v1)) {
                v = v1;
            }
            if (v == null) {
                if (DEBUG) {
                    grid.Echo ("ProtoRcv: check failed");
                }
                return false;
            }
        }

        msgList.Add (
            new RadioMsg (host, msg.Substring (v.Length))
        );
        return true;

    }

    public void sendMsg (string s) {
        if (useLogicAddress) {
            grid.send (logicAddress + "%" + host + "%" + channel
                + "%" + s);
        } else {
            grid.send (grid.Me.EntityId.ToString() + "%" + host + "%" + channel
                + "%" + s);
        }
    }

    public void sendMsg (string s, string hostOverride) {
        if (useLogicAddress) {
            grid.send (logicAddress + "%" + hostOverride + "%" + channel
                + "%" + s);
        } else {
            grid.send (grid.Me.EntityId.ToString() + "%" + hostOverride + "%" + channel
                + "%" + s);
        }
    }

    public bool getMsg (out RadioMsg msg) {
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
