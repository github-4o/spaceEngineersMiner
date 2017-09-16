
public class Timer {
    string name;
    IMyTerminalBlock timer;
    MyGridProgram pg;

    public Timer (MyGridProgram argPg, string arg_name) {
        name = arg_name;
        pg = argPg;
        _findTimer ();
    }

    public bool startNextTick () {
        if (timer == null) {
            _findTimer();
            return false;
        } else {
            timer.ApplyAction ("TriggerNow");
            return true;
        }
    }

    public string printStatus () {
        if (_checkTimer()) {
            return "timer: failure\n";
        }
        return "";
    }

    public bool getStatus () {
        return _checkTimer();
    }

    void _findTimer () {
        List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();
        pg.GridTerminalSystem.GetBlocksOfType <IMyTimerBlock> (
            l, x=> x.CubeGrid == pg.Me.CubeGrid &&
            x.CustomName == name);

        if (l.Count == 1) {
            timer = (IMyTimerBlock) l[0];
        }
    }

    bool _checkTimer () {
        if (timer == null) {
            return true;
        }
        if ((timer.CubeGrid.GetCubeBlock(timer.Position))?.FatBlock != timer) {
            timer = null;
            return true;
        }
        return false;
    }
}
