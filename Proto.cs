
public abstract class Proto {

    protected Grid grid;
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
