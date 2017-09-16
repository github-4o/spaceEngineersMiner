
public class Radio {
    MyGridProgram pg;
    List<IMyRadioAntenna> l = new List<IMyRadioAntenna>();
    List<string> toSend = new List<string> ();

    int DEBUG_sentNum = 0;
    string DEBUG_last_msg = "";

    public Radio (MyGridProgram argPg) {
        pg = argPg;
    }

    public void send (string msg) {
        // msg = pg.Me.CubeGrid.CustomName + "%" + msg;
        // msg = pg.Me.EntityId + "%" + msg;
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
            pg.GridTerminalSystem.GetBlocksOfType (l, x=>x.CubeGrid == pg.Me.CubeGrid);
            return l.Count == 0;
        }
        return false;
    }
}
