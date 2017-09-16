
public class RadioMsg {
    public string sender {get; protected set;}
    public string val {get; protected set;}

    public RadioMsg (string s, string v) {
        sender = s;
        val = v;
    }
}
