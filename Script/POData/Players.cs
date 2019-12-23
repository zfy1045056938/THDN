using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Mathematics;
using Unity.Entities;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
    public class Players : Entity
    {
    public  agent;
    public static Dictionary<string,Players> onlinePlayers =new Dictionary<string, Players>();
    [SyncVar]public string account;
    [SyncVar] public string className;

    [SyncVar]
    public float remainingLogoutTime;

    [SyncVar]
    public Classes classType = Classes.Normal;

    public override int healthMax => base.healthMax;

    public override int manaMax => base.manaMax;

    public override int damage => base.damage;


    public float nextRiskyActionTime=1.0f;
    public Players()
    {
    }

    protected override void UpdateClient()
    {
        throw new NotImplementedException();
    }

    public override string UpdateServer()
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();
    }
}
