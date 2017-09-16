
public class Grid {
    Timer timer;
    Mover mover;
    Aligner aligner;
    MyGridProgram pg;
    Radio _radio;
    Goto gt;
    ProtoStack protoStack = new ProtoStack ();
    Radio radio {
        get {
            if (_radio == null) {
                _radio = new Radio (pg);
            }
            return _radio;
        }
    }
    // TelemetryGen tm;
    int reinitCount;
    int tickCounter = 0;
    string statusTpName;
    IMyTextPanel statusTp;
    List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();
    // IApp app = null;
    int index = 0;
    RemoteControl rc;

    // List<string> msgs = new List <string> ();

    public IMyGridTerminalSystem gts {get {return pg.GridTerminalSystem;}}
    public IMyProgrammableBlock Me {get {return pg.Me;}}

    FsmInterpreter prog = null;

    public bool align {
        get {return aligner.enabled;}
        set {aligner.enabled = value;}
    }

    public bool aligned {
        get {return aligner.isAligned();}
    }

    public double getDistanceToTposSquared {
        get {return gt.distanceToTposSquared;}
    }

    public Grid (
        MyGridProgram argPg,
        string timerName,
        string argStatusTpName,
        int argReinitCount
    ) {
        reinitCount = argReinitCount;
        statusTpName = argStatusTpName;
        pg = argPg;
        _initStatusTp();
        timer = new Timer (argPg, timerName);
        mover = new Mover (argPg);
        aligner = new Aligner (this);
        aligner.mode = Aligner.Mode.MostThrusters;
        // tm = new TelemetryGen (argPg);
        gt = new Goto (argPg, this, mover, aligner);
        gt.setRef (argPg.Me, Base6Directions.Direction.Down);
        rc = new RemoteControl (this);
        prog = new FsmInterpreter (this, null);

        string progString =
    // @"# preset variables
    // set altSource=altSource
    // set altCh=getAlt
    // fsm:
    // (alt)RequestValueOverRadio(altSource,altCh)
    // Undock
    // (alt)RequestValueOverRadio(altSource,altCh)
    // Dock
    // Recharge";

    // "# preset variables\n" +
    // "set altSource=altSource\n" +
    // "set altCh=getAlt\n" +
    // "fsm:\n" +
    // "(alt)RequestValueOverRadio(altSource,altCh)\n" +
    // "Undock\n" +
    // "(alt)RequestValueOverRadio(altSource,altCh)\n" +
    // "Dock\n" +
    // "Recharge";

    // "(dock)RequestValueOverRadio(dockSource,dockCh)\n" +
    "# preset variables\n" +
    "set altSource=altSource\n" +
    "set altCh=altCh\n" +
    "set dockSource=dockSource\n" +
    "set dockCh=dockCh\n" +
    "fsm:\n" +
    "(alt)RequestValueOverRadio(altSource,altCh)\n" +
    "DockBasic(dockSource,dockCh,alt)\n" +
    "Recharge";
    //  +
    // "Dock\n";

    //         prog.load (@"# preset variables
    // set altSource=altSource
    // set altCh=getAlt
    // set dockSource=dockSource
    // set dockCh=requestDock
    // # define fsm states
    // fsm:
    // (alt)RequestValueOverRadio(altSource,altCh)
    // Undock
    // DockBasic(sender,dockCh,alt)"
    // );
            // prog.load ("# preset variables\nset altSource=altSource\nset altCh=getAlt\nfsm:\n(alt)RequestValueOverRadio(altSource,altCh)\nUndock\n(alt)RequestValueOverRadio(altSource,altCh)\nDock\nRecharge");

        prog.load (progString);
    }

    // rc
    public bool getAltitude (out double alt) {
        return rc.getAltitude (out alt);
    }
    public bool getGravity (out Vector3D gravity) {
        return rc.getGravity (out gravity);
    }
    public bool getPlanetCenter (out Vector3D pc) {
        return rc.getPlanetCenter (out pc);
    }
    //
    public void setAlignMode (Aligner.Mode m) {
        aligner.mode = m;
    }
    //proto stack
    public bool registerProto (Proto p) {
        return protoStack.registerProto (p);
    }

    public void unregisterProto (Proto p) {
        protoStack.unregisterProto (p);
    }
    //

    public void Echo (string s) {
        pg.Echo (s);
    }

    public bool setRef (IMyTerminalBlock b, Base6Directions.Direction d) {
        return gt.setRef (b, d);
    }

    public void setTpos (Vector3D p) {
        gt.setTpos (p);
    }

    public void stop () {
        gt.stop ();
    }

    // public Mover requestMover () {
    //     return mover;
    // }

    // public Aligner requestAligner () {
    //     return aligner;
    // }

    // public void setApp (IApp argApp) {
    //     app = argApp;
    // }

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

    public bool step (string msg) {
        // if (msg.Length > 0 && !msgs.Contains (msg)) {
        //     msgs.Add (msg);
        // }
        if (msg.Length > 0) {
            // Echo ("handling msg: " + msg);
            protoStack.handleMsg (msg);
        }
        timer.startNextTick();
        tickCounter ++;
        if (tickCounter > reinitCount) {
            tickCounter = 0;
            _reinit();
        }
        index ++;
        if (index > 4) {
            index = 0;
        }
        switch (index) {
            case 0:
                // if (app != null) {
                //     app.step(msg);
                // }
                prog.step();
                prog.load (prog.backup);
                break;
            case 1:
                mover.step();
                break;
            case 2:
                aligner.step();
                break;
            case 3:
                gt.step();
                break;
            case 4:
                radio.step();
                // radio.send (pg.Me.GetPosition().ToString());
                // radio.send (MyConverter.toString(pg.Me.GetPosition()));
                // radio.send (tm.genTelementry());

                break;
        }
        return _getStatus();
    }

    public string printStatus () {
        string ret = "Grid status:\n"  +
            "stdout = " + _checkStatusTp() + "\n"
            + timer.printStatus()
            + mover.printStatus()
            + aligner.printStatus()
            + prog.backup + "\nstate = " + prog.reportState + "\n"
            // + app.printStatus()
            + gt.printStatus()
            + radio.printStatus();
        if (_checkStatusTp() == false) {
            statusTp.WritePublicText (ret);
        } else {
            _initStatusTp();
        }
        return ret;
    }

    void _initStatusTp () {
        List<IMyTerminalBlock> l = new List<IMyTerminalBlock>();
        pg.GridTerminalSystem.GetBlocksOfType <IMyTextPanel> (
            l, x=> x.CubeGrid == pg.Me.CubeGrid &&
            x.CustomName == statusTpName);

        if (l.Count == 1) {
            statusTp = (IMyTextPanel) l[0];
        }
    }

    bool _checkStatusTp () {
        if (statusTp == null) {
            return true;
        }
        if ((pg.Me.CubeGrid.GetCubeBlock(statusTp.Position))?.FatBlock != statusTp) {
            statusTp = null;
            return true;
        }
        return false;
    }

    void _reinit () {
        mover.reinit();
    }

    bool _getStatus () {
        if (timer.getStatus()) {
            return true;
        }
        if (mover.getStatus()) {
            return true;
        }
        return false;
    }
}
