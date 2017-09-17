
public class Recharge : FsmStateImplementation {
    public Recharge (
        Grid g,
        FsmMemorySpace m,
        List<string> ivn,
        List<string> ovn
    ) : base (g, m, ivn, ovn) {
    }

    List<IMyBatteryBlock> batts = new List<IMyBatteryBlock> ();

    public override void init () {
        base.init();

        grid.gts.GetBlocksOfType (batts, x => x.CubeGrid == grid.Me.CubeGrid);

        // grid.Echo ("hit " + batts.Count);

        for (int i=0;i<batts.Count;i++) {
            batts[i].OnlyRecharge = true;
        }
    }

    List<int> toRemove = new List<int>();
    int num = 0;

    public override FsmState step () {
        // if (batts.Count == 0) {
            grid.gts.GetBlocksOfType (batts, x => x.CubeGrid == grid.Me.CubeGrid);
            for (int i=0;i<batts.Count;i++) {
                batts[i].OnlyRecharge = true;
            }
            // grid.Echo ("hit " + batts.Count);
        //     return this;
        // }
        grid.Echo ("num = " + num++);
        double minCharge = 100;
        double tmp = 0;
        // grid.Echo ("count = " + batts.Count);
        for (int i=batts.Count-1;i>=0;i--) {
            // grid.Echo ("tmp = " + tmp);
            if (_checkExists(batts[i])) {
                batts.RemoveAt (i);
            } else {
                tmp = batts[i].CurrentStoredPower / batts[i].MaxStoredPower;
                grid.Echo (i.ToString() + ": c = " + batts[i].CurrentStoredPower);
                grid.Echo (i.ToString() + ": input = " + batts[i].CurrentInput);
                grid.Echo (i.ToString() + ": tmp = " + tmp);
                if (tmp < minCharge) {
                    minCharge = tmp;
                }
            }
        }
        grid.Echo (batts.Count.ToString() + ":min charge = " + minCharge);
        if (minCharge > 0.90) {
            for (int i=0;i<batts.Count;i++) {
                batts[i].OnlyRecharge = false;
            }
            return end();
        }
        return this;
    }

    bool _checkExists (IMyTerminalBlock b) {
        if (b == null) {
            return true;
        }
        if ((b.CubeGrid.GetCubeBlock(b.Position))?.FatBlock != b) {
            b = null;
            return true;
        }
        return false;
    }
}
