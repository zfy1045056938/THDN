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
               p.invetory[inventoryIndex].item.summonHealth > 0 &&
               p.inventory[inventoryIndex].item.summonLevel <= p.level;
    }

    public void Use(Players p, int inventoryIndex)
    {
        p.nextRiskyActionTime = NetworkTime.time + 1;
    }

}