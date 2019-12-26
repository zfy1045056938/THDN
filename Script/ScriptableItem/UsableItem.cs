using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Mirror;

//items who can use or equip
//LEVELREQUIRED: p.level> i.level
//OTHERREQUIRED: p.sdi>i.level

public class UsableItem : ScriptableItem
{
    
    
    
    //declare usable Item variable
    public int itemLevel;
    public int coldDown;


    [SerializeField] private string _cooldownCategory;

    public string CooldownCategory => string.IsNullOrWhiteSpace(_cooldownCategory) ? name : _cooldownCategory;

    public override string Tooltip()
    {
       StringBuilder sb=  new StringBuilder(tooltip);

       sb.Replace("{ITEMLEVEL}", itemLevel.ToString());
       sb.Replace("COLDDOWN", coldDown.ToString());
       return sb.ToString();
    }


    public bool CanUse(Players p, int inventoryIndex)
    {
        return FindEquipSlotFor(p, inventoryIndex) != -1;
    }

    public bool CanEquip(Players p, int inventoryIndexs, int equipmentIndex)
    {
        return true;
    }

    int FindEquipSlotFor(Players p, int inventoryIndex)
    {
        return 0;
    }

    public  void Use(Players p, int inventoryIndex)
    {
        
    }
}