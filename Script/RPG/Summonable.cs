using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Basic;
using UnityEngine;

public class Summonable : Entity
{
   [SyncVar] private GameObject _owner;

   public Players owner
   {
      get { return _owner != null ? _owner.GetComponent<Players>() : null; }
      set { _owner = value != null ? value.gameObject : null; }
   }
   protected override void UpdateClient()
   {
      throw new System.NotImplementedException();
   }

   public override string UpdateServer()
   {
      throw new System.NotImplementedException();
   }
   
   /// <summary>
   /// 
   /// </summary>
   [Server]
   public void SyncToOwnItem()
   {
      
   }
   
   /// <summary>
   /// 
   /// </summary>
   /// <param name="slot"></param>
   /// <returns></returns>
   protected virtual InventorySlot SyncStateToItemSlot(InventorySlot slot)
   {

      return slot;
   }
}
