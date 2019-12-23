using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Text;
using Unity.Mathematics;

[SerializeField]
public class InventorySlot : MonoBehaviour
{
   public Item item;
   public int amount;

   public InventorySlot(Item item, int amount)
   {
      this.item = item;
      this.amount = amount;
   }

   public int DecreaseAmount(int reduce)
   {
      int limit = math.clamp(reduce, 0, amount);
      amount -= limit;
      return limit;
   }

   public int IncreaseAmount(int increase)
   {
      int limit = math.clamp(increase, 0, item.maxStack - amount);
      amount += limit;
      return limit;
   }

   public string Tooltip()
   {
      if (amount == 0) return "";
      //
      StringBuilder sb = new StringBuilder(item.Tooltip());
      sb.Replace("{AMOUNT}", amount.ToString());

   }
}

public class SyncItemSlot:SyncList<InventorySlot>{}
