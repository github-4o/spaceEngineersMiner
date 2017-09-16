
public class FsmInterpreter {
    Grid grid = null;
    FsmState state = null;
    FsmState stateReg = null;
    List<FsmState> stateList = new List<FsmState> ();
    FsmMemorySpace mem = new FsmMemorySpace ();
    public string backup {get; protected set;} = "";
    Action triggerBackup;

    public string reportState {get {
        return state.GetType().Name;
    }}

    public FsmInterpreter (Grid g, Action trig) {
        grid = g;
        triggerBackup = trig;
    }

    public void load (string s) {
        backup = s;
        string[] lines = s.Split (new [] {'\r', '\n'});
        _loadMem (lines);
        _loadFsm (lines);

        for (int i=0;i<stateList.Count-1;i++) {
            stateList[i].setNext (stateList[i+1].getMe());
        }
        stateList[stateList.Count-1].setNext(stateList[0].getMe());
    }

    void _backup () {
        StringBuilder sb = new StringBuilder ();
        mem.backup(sb);
        _backupFsm(sb);
        backup = sb.ToString();
    }

    void _backupFsm (StringBuilder sb) {
        sb.Append ("fsm:\n");
        var st = state as FsmStateImplementation;
        _backupFsm (st, sb);
        st = st.next as FsmStateImplementation;
        while (st != state) {
            _backupFsm (st, sb);
            st = st.next as FsmStateImplementation;
        }
    }

    void _backupFsm (FsmStateImplementation st, StringBuilder sb) {
        if (st == null) {
            throw new Exception ("null state");
        }
        st.backup(sb);
        sb.Append ("\n");
    }

    void _loadMem (string[] ss) {
        mem.clear();
        for (int i=0;i<ss.Length;i++) {
            if (ss[i].StartsWith ("set ")) {
                _loadMemOneLine (ss[i].Substring (4), false);
            } else if (ss[i].StartsWith ("setConst ")) {
                _loadMemOneLine (ss[i].Substring (9), true);
            }
        }
    }

    void _loadFsm (string[] ss) {
        stateList.Clear();
        bool startFound = false;
        int index = 0;
        for (;index<ss.Length;index++) {
            if (ss[index].StartsWith("fsm:")) {
                startFound = true;
                break;
            }
        }
        if (!startFound) {
            throw new Exception ("failed to find 'fsm:' pattern");
        }
        List <string> usefulLines = new List<string> ();
        for (int i = index+1;i<ss.Length;i++) {
            if (ss[i].Length > 0) {
                usefulLines.Add (ss[i]);
            }
        }
        if (usefulLines.Count < 1) {
            throw new Exception ("failed to find any useful fsm lines");
        }
        for (int i=0;i<usefulLines.Count;i++) {
            _loadFsmOneLine (usefulLines[i]);
        }
    }

    void _loadFsmOneLine (string str) {
        string[] ar = str.Split (new [] {'(', ')'});
        if (ar.Length == 5) {
            List<string> inArgs = new List<string> ();
            if (ar[3].Length > 0) {
                inArgs = ar[3].Split(',').ToList();
            }
            List<string> outArgs = new List<string> ();
            if (ar[1].Length > 0) {
                outArgs = ar[1].Split(',').ToList();
            }
            string stateName = ar[2];
            FsmState nst = FsmStateFactory.makeState (
                stateName, grid, mem, inArgs, outArgs);
            if (state == null) {
                state = nst;
                nst.init();
            }
            if (nst == null) {
                throw new Exception ("(1)failed to create a state from line '"
                    + str + "'");
            }
            stateList.Add (nst);
        // } else if (ar.Length == 3) {
        } else if (ar.Length == 3 && str.EndsWith(")")) {
            List<string> inArgs = new List<string> ();
            if (ar[1].Length > 0) {
                inArgs = ar[1].Split(',').ToList();
            }
            string stateName = ar[0];
            FsmState nst = FsmStateFactory.makeState (
                stateName, grid, mem, inArgs, null);
            if (state == null) {
                state = nst;
                nst.init();
            }
            if (nst == null) {
                throw new Exception ("(2)failed to create a state from line '"
                    + str + "'");
            }
            stateList.Add (nst);
        } else if (ar.Length == 3 && str.StartsWith("(")) {
            List<string> outArgs = new List<string> ();
            if (ar[1].Length > 0) {
                outArgs = ar[1].Split(',').ToList();
            }
            string stateName = ar[2];
            FsmState nst = FsmStateFactory.makeState (
                stateName, grid, mem, null, outArgs);
            if (state == null) {
                state = nst;
                nst.init();
            }
            if (nst == null) {
                throw new Exception ("(3)failed to create a state from line '"
                    + str + "'");
            }
            stateList.Add (nst);
        } else if (ar.Length == 1) {
            string stateName = ar[0];
            FsmState nst = FsmStateFactory.makeState (
                stateName, grid, mem, null, null);
            if (state == null) {
                state = nst;
                nst.init();
            }
            if (nst == null) {
                throw new Exception ("(4)failed to create a state from line '"
                    + str + "'");
            }
            stateList.Add (nst);
        } else {
            throw new Exception ("words num = " + ar.Length + " str = " + str
                + " last char = " + str[str.Length-1]);
        }
    }

    void _loadMemOneLine (string s, bool c) {
        string[] words = s.Split ('=');
        if (words.Length != 2) {
            throw new Exception ("failed to load mem (" + words.Length + ")");
        }

        string name = words [0];
        string val = words [1];

        // grid.Echo ("setting var " + name + " = " + val);

        if (c) {
            mem.setVarConst (name, val);
        } else {
            mem.setVar (name, val);
        }

        words = null;
    }

    public bool step () {
        state = state?.step();
        if (stateReg != null && stateReg != state) {
            _backup();
            if (triggerBackup != null) {
                triggerBackup();
            }
        }
        stateReg = state;
        return state != null;
    }

    public string printMem () {
        return mem.print();
    }
}
