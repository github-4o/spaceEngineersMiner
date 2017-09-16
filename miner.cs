Grid s;
Profiler p;

string msg_old = "";
string msg_new;

public Program() {
    s = new Grid (
        this,
        "Timer Block miner",
        "LCD Panel [status]",
        100
    );
    Echo ("grid ok");
    Me.CubeGrid.CustomName = "Miner";
    p = new Profiler (this);
}

// double maxinstc = 0;

bool failure = false;
string fstr;

List<IMyBatteryBlock> batts = new List<IMyBatteryBlock> ();

void Main (string argument) {

    // GridTerminalSystem.GetBlocksOfType (batts, x => x.CubeGrid == Me.CubeGrid);
    // Echo ("check = " + batts.Count);

    if (!failure) {
        try {
            s.step(argument);
        } catch (Exception ex) {
            fstr = "Source = " + ex.Source + "\nStackTrace =" + ex.StackTrace
                + "\nTargetSite = " + ex.TargetSite + "\nMessage = " + ex.Message + "\nexception = " + ex;
            Me.CustomData = fstr;
            Echo (fstr);
            throw new Exception ("failure");
        }
    }

    // if (argument.Length > 0) {
    //     Echo ("last arg = " + argument);
    // }


    // double instc = ((double)Runtime.CurrentInstructionCount) / ((double)Runtime.MaxInstructionCount);
    // if (instc > maxinstc) {
    //     maxinstc = instc;
    // }

    // msg_new = s.printStatus() + "instc: " + maxinstc;
    if (failure) {
        Me.CustomData = fstr;
        Echo (fstr);
    } else {
        msg_new = s.printStatus();
        if (msg_old != msg_new) {
            // Me.Echo (msg_new);
            Me.CustomData = msg_new + "\n" +
                "Me.EntityId.ToString() = " + Me.EntityId.ToString("X") + "\n" +
                "Me.EntityId = " + Me.EntityId + "\n" +
                "Me = " + Me + "\n"
            ;
            msg_old = msg_new;
        }
    }

    // Me.CustomData = p.step();
}

public class Profiler {
    double[] perfAr = new double [60];
    int perfCnt;
    double avarage;
    double perfMaxSecond;
    double perfMaxEver;

    MyGridProgram prog;

    public Profiler (MyGridProgram p) {
        prog = p;
        perfCnt = 0;
        perfMaxSecond = 0;
        perfMaxEver = 0;
    }

    public string step () {
        perfAr [perfCnt] = prog.Runtime.LastRunTimeMs;
        perfCnt ++;
        if (perfCnt > 59) {
            perfCnt = 0;
        }
        avarage = (double) ((double)perfAr.Sum())/60.0;
        perfMaxSecond = perfAr.Max();
        if (perfMaxEver < perfMaxSecond) {
            perfMaxEver = perfMaxSecond;
        }
        return _msg ();
    }

    string _msg () {
        return "avarage = " + avarage + "\nmax during sec = " + perfMaxSecond
            + "\nmax ever" + perfMaxEver;
    }
}

// int count = 1;
// int maxSeconds = 100;
// StringBuilder profile = new StringBuilder();
// bool hasWritten = false;

// void ProfilerGraph() {
//     if (count <= maxSeconds * 60)
//     {
//         double timeToRunCode = Runtime.LastRunTimeMs;
//         profile.Append(timeToRunCode).Append("\n");
//         count++;
//     }
//     else if (!hasWritten)
//     {
//         hasWritten = true;
//         Me.CustomData = profile.ToString();
//     } else {
//         Echo ("done");
//     }
// }
