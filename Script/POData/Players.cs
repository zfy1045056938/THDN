using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;
using System.Linq;

    public class Players : Entity
    {
    internal static object agent;
    public static Dictionary<string,Players> onlinePlayers =new Dictionary<string, Players>();
    [SyncVar]public string account;
    [SyncVar] public string className;

    [SyncVar]
    public float remainingLogoutTime;

    [SyncVar]
    public Classes classType = Classes.Normal;

    public Players()
    {
    }


    #region  Client Module

    
    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    #endregion


    #region  Server Module

      public override void OnStartServer()
        {
            base.OnStartServer();
        }

      public  void OnServerShutDown()
      {
          
      }

    #endregion

    #region Common

    public bool GetItemCoolDown()
    {
        return true;

    }

    public void SetItemCoolDown()
    {
        
    }

    #endregion


    #region FSM

    

    #endregion

    #region COMMAND

    

    #endregion

  

  
}
