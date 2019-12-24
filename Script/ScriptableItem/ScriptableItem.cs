using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Mirror;


//Item base Classes
//in THDN include these variable
//THDN in dungeon explore  can find item who needs , items include equipemtn
//potion und other special items,when player in dungeon can equip or us item 
public partial class ScriptableItem:ScriptableObjectNonAlloc{

    //Variable Common for all item
    public string itemName;
    public string itemRatity;
    public int itemSell;
    public int itemBuy;
    public int stackSize;
    public int itemDur;

    public bool canSellable;
    public bool canRepairable;
    public bool canDestorable;
    public Sprite image;
    
    //tooltip
    [SerializeField,TextArea(1,30)]
    protected   string tooltip  ;


    
    
    
    //Common tooltip
    public virtual string Tooltip(){
        
        StringBuilder sb= new StringBuilder(tooltip);
        sb.Replace("{ITEMNAME}",itemName);
        sb.Replace("{ITEMRATITY}", itemRatity);
        sb.Replace("{ITEMSELL}", itemSell.ToString());
        sb.Replace("{ITEMSTACKSIZE}", stackSize.ToString());
        sb.Replace("{ITEMDUR}", itemDur.ToString());

        sb.Replace("{CANSELLABLE}", canSellable ? "YES" : "NO");
        sb.Replace("{CANREPAIRABLE}", canRepairable?"YES":"NO");
        sb.Replace("{CANDESTORABLE}", canDestorable?"YES":"NO");

        
        return sb.ToString();
    }


    private static Dictionary<int, ScriptableItem> cache;
    public static Dictionary<int,ScriptableItem>dict{
        get{
            if (cache == null)
            {
                ScriptableItem[] items = Resources.LoadAll<ScriptableItem>("");
                //
                if (items != null)
                {
                    //create instance for item
                    List<string> dup = items.ToList().FindDup(item => item.name);
                    
                    //to dic
                    if (dup.Count == 0)
                    {
                        cache = items.ToDictionary(item => item.name.GetStableHashCode(), item=>item);
                    }
                    
                }
            }
            return cache;
        }
    }
    
    
    //default item in inventory
    [SerializeField]
    public struct ScriptObjectItemUndAmount
    {
        public ScriptableItem item;
        public int amount;
    }
        
}