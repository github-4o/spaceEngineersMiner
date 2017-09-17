# spaceEngineersMiner

this repo holds code for my planetary miner. this miner requires a station to provide safe altitude and docking port info.

the station code resides in testReplier.cs. the rest of the code is related to the miner.
additional setup for a station:
1) add connectors facing up and add miner's PB.EntityId in hex to connector's CustomData. this is supposed to create dedicated connectors for different grids.

current state of repo:
1) this is a hobby. i rather doubt this code would look beautiful any time soon. i will put effort into making it server-friendly though;
2) this repo holds code for FSM interpreter, which could be viewed as custom programming language interpreter;
3) in some time this code will be expanded to support different roles. right now most of the code is generic, including a FSM interpreter. the code for FSM interpreter is hardcoded in Grid.cs. this class could be expanded to enable remote pregram loading over radio;
4) this repo holds code related to radio comms. this code is a subject to change for sure.
