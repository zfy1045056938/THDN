using UnityEngine;
using System.Collections.Generic;
using Mirror;


[CreateAssetMenu(menuName="ItemManager/SummonableItem")]
public abstract class SummonableItem : UsableItem
{
    public Summonable summonPrefab;
    public int price = 10;
    public int removeItemIfDied;


    public  bool CanUse(Players p, int inventoryIndex)
    {
        return base.CanUse(p, inventoryIndex) &&
               NetworkTime.time >= p.nextRiskyActionTime &&
               summonPrefab != null &&
               p.Inventory[inventoryIndex].item.summonedHealth > 0 &&
               p.Inventory[inventoryIndex].item.summonedLevel <= p.level;
    }

    public void Use(Players p, int inventoryIndex)
    {
        base.Use(p,inventoryIndex);
        p.nextRiskyActionTime = NetworkTime.time + 1;
    }

}