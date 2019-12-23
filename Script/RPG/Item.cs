using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Mirror;
using System.Linq;


public partial  struct Item
{
    public Item(ScriptableItem items)
    {
        hash = items.name.GetStableHashCode();
        summoned = null;
        summonedHealth = items is SummonableItem summonable ? summonable.summonPrefab.healthMax : 0;
        summonedLevel = items is SummonableItem ? 1 : 0;
        summonedExp = 0;
    }
    
    //
    public ScriptableItem data
    {
        get
        {
            if (!ScriptableItem.dict.ContainsKey(hash))
            {
                throw new KeyNotFoundException("No ScriptItem with has ="+hash+"no found");
            }

            return ScriptableItem.dict[hash];
        }
    }
    
    //Data combine
    public string name => data.name;
    public int stackSize => data.stackSize;
    public int buyPrice => data.itemBuy;
    public int sellPrice => data.itemSell;

    public bool canSell => data.canSellable;
    public bool canDestory => data.canDestorable;
    public bool canRepair => data.canRepairable;

    public int hash;

    public GameObject summoned;
    public int summonedHealth;
    public int summonedLevel;
    public int summonedExp;


    public string Tooltip()
    {
        StringBuilder sb =new StringBuilder(data.Tooltip());
        sb.Replace("{SUMMONEDHEALTH}", summonedHealth);
        sb.Replace("{SUMMONLEVEL", summonedLevel);
        sb.Replace("{SUMMONEXP}", summonedExp);
        

    }
}
