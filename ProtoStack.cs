
public class ProtoStack {
    List <Proto> protos = new List <Proto> ();

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
