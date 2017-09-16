
public class FsmMemorySpace {
    Dictionary <string, Slot> mem = new Dictionary <string, Slot> ();

    public void setVar (string name, object o) {
        if (mem.ContainsKey (name)) {
            mem[name].setVal (o);
        } else {
            mem.Add (name, new Slot (o));
        }
    }

    public void clear () {
        mem.Clear();
    }

    public bool isConst (string name) {
        if (mem.ContainsKey (name)) {
            return mem[name].isConst;
        }
        return false;
    }

    public void setVarConst (string name, object o) {
        if (mem.ContainsKey (name)) {
            if (mem[name].isConst) {
                throw new Exception (
                    "Slot: prevented an attempt to set const var " + name);
            }
            mem[name].setVal (o);
            mem[name].isConst = true;
        } else {
            mem.Add (name, new Slot (o, true));
        }
    }

    public object getVar (string name) {
        if (mem.ContainsKey (name)) {
            return mem[name].val;
        }
        return null;
    }

    public string print () {
        StringBuilder ret = new StringBuilder ();
        foreach (var kvp in mem) {
            ret.Append (kvp.Key + " = " + kvp.Value.print() + "\n");
        }
        return ret.ToString();
    }

    public void backup (StringBuilder sb) {
        foreach (var kvp in mem) {
            if (kvp.Value.isConst) {
                sb.Append ("setConst ");
            } else {
                sb.Append ("set ");
            }
            sb.Append (kvp.Key + "=" + kvp.Value.val+"\n");
        }
    }

    protected class Slot {
        public object val {get; protected set;}
        public bool isConst;
        public Slot (object v, bool c = false) {
            val = v;
            isConst = c;
        }
        public object getVal () {
            return val;
        }
        public void setVal (object v) {
            val = v;
        }
        public string print () {
            return val.ToString();
        }
    }
}
