using UnityEngine;
using Mirror;
using System.IO;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using Mirror;
using UnityEngine.AI;


//Game Entity Base Classes Includes(For THDN has 3 Entities(Player,NPC,Monster))
//1.Stats 
//2.C/S Callback Handler Msg
//3.
public partial class Entity : NetworkBehaviour
{


    //Declare Player 
    public SyncListInventory Inventory =new SyncInventory();
    public SyncListEquipment equipment =new SyncListEquipment();

    [SyncVar]public int gold=-1;
    [SyncVar]public int money=-1;

    [SyncVar]public int level=1;
    
    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool InvokeCommand(int cmdHash, NetworkReader reader)
    {
        return base.InvokeCommand(cmdHash, reader);
    }

    public override bool InvokeRPC(int rpcHash, NetworkReader reader)
    {
        return base.InvokeRPC(rpcHash, reader);
    }

    public override bool InvokeSyncEvent(int eventHash, NetworkReader reader)
    {
        return base.InvokeSyncEvent(eventHash, reader);
    }

    public override bool OnCheckObserver(NetworkConnection conn)
    {
        return base.OnCheckObserver(conn);
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        base.OnDeserialize(reader, initialState);
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
    }

    public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
    {
        return base.OnRebuildObservers(observers, initialize);
    }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        return base.OnSerialize(writer, initialState);
    }

    public override void OnSetHostVisibility(bool visible)
    {
        base.OnSetHostVisibility(visible);
    }

    public override void OnSetLocalVisibility(bool visible)
    {
        base.OnSetLocalVisibility(visible);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
