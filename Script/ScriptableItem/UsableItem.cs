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
    
    public string tooltip;
    
    public override StringBuilder Tooltip(string text)
    {
       StringBuilder sb=  new StringBuilder(tooltip);

       sb.Replace("{ITEMLEVEL}", itemLevel.ToString());
       sb.Replace("COLDDOWN", coldDown.ToString());
       return sb;
    }
    
    /////////////////VIRTUAL///////////////
    public virtual bool CanUse(Players player,int inventoryIndex)
    {
        if (player.level >= itemLevel)
        {
           return player.GetItemCoolDown();
        }

        return false;
    }

    public virtual bool ItemsCooldown()
    {
        return true;
    }
    
    //[client]
    public virtual  void OnUsed(Players p){}
    
    public virtual  bool CanSplit(ScriptableItem items){}
    
    public virtual bool CanExchange(ScriptableItem items,Players p,NPC npc){}
    
}