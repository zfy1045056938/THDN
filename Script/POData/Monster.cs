using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;
using System.Linq;
public class Monster : Entity
{
    public override int healthMax => base.healthMax;

    public override int manaMax => base.manaMax;

    public override int damage => base.damage;

    public override void Awake()
    {
        base.Awake();
    }

    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
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

    public override void Start()
    {
        base.Start();
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override void Update()
    {
        base.Update();
    }

    public override string UpdateServer()
    {
        throw new NotImplementedException();
    }

    protected override void UpdateClient()
    {
        throw new NotImplementedException();
    }

    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();
    }
}
